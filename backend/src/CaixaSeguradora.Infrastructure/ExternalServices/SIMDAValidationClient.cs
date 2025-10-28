using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Infrastructure.ExternalServices;

/// <summary>
/// SIMDA (HB Contract Validation) SOAP 1.2 service client with Polly resilience policies
/// Handles validation for claims with NUM_CONTRATO = 0 or not found (HB contracts)
/// Implements retry (3 attempts, exponential backoff), circuit breaker (5 failures, 30s break), and timeout (10s)
/// </summary>
public class SimdaValidationClient : ISimdaValidationClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SimdaValidationClient> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
    private readonly AsyncTimeoutPolicy _timeoutPolicy;

    // T023: EZERT8 error code mapping for SIMDA
    private static readonly Dictionary<string, (string ErrorCode, string Message)> EzertErrorMap = new()
    {
        ["00000000"] = ("SUCCESS", "Validação aprovada"),
        ["EZERT8001"] = ("CONS-001", "Contrato de consórcio inválido"),
        ["EZERT8002"] = ("CONS-002", "Contrato cancelado"),
        ["EZERT8003"] = ("CONS-003", "Grupo encerrado"),
        ["EZERT8004"] = ("CONS-004", "Cota não contemplada"),
        ["EZERT8005"] = ("CONS-005", "Beneficiário não autorizado")
    };

    private const string SoapNamespace = "http://caixaseguradora.com.br/simda";
    private const string SoapAction = "http://caixaseguradora.com.br/simda/ValidateHBContract";

    public string SystemName => "SIMDA";

    public SimdaValidationClient(
        IHttpClientFactory httpClientFactory,
        ILogger<SimdaValidationClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        // T020: Polly retry policy - 3 retries with exponential backoff (2s, 4s, 8s)
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r =>
                r.StatusCode == HttpStatusCode.RequestTimeout ||
                r.StatusCode == HttpStatusCode.TooManyRequests ||
                r.StatusCode >= HttpStatusCode.InternalServerError)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "SIMDA retry {RetryCount}/3 after {DelayMs}ms. Reason: {Reason}",
                        retryCount,
                        timespan.TotalMilliseconds,
                        outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
                });

        // T020: Polly circuit breaker - open after 5 consecutive failures for 30 seconds
        _circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, breakDelay) =>
                {
                    _logger.LogError(
                        "SIMDA circuit breaker opened for {BreakSeconds}s after 5 failures. Reason: {Reason}",
                        breakDelay.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown");
                },
                onReset: () =>
                {
                    _logger.LogInformation("SIMDA circuit breaker reset - service recovered");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("SIMDA circuit breaker half-open - testing service");
                });

        // T020: Polly timeout policy - 10 seconds per request
        _timeoutPolicy = Policy.TimeoutAsync(
            seconds: 10,
            onTimeoutAsync: (context, timespan, task) =>
            {
                _logger.LogWarning("SIMDA request timeout after {TimeoutSeconds}s", timespan.TotalSeconds);
                return Task.CompletedTask;
            });
    }

    /// <inheritdoc/>
    public async Task<ExternalValidationResponse> ValidateAsync(
        ExternalValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestTimestamp = DateTime.UtcNow;

        // T024: Structured logging - request payload
        _logger.LogInformation(
            "SIMDA validation started: Contract={Contract}, Claim={Claim}, Amount={Amount}",
            request.NumContrato, $"{request.Orgsin:D2}/{request.Rmosin:D2}/{request.Numsin:D6}", request.ValorPrincipal);

        try
        {
            // Validate contract routing
            if (!SupportsContract(request.NumContrato))
            {
                var errorResponse = CreateErrorResponse(
                    "INVALID_CONTRACT",
                    $"Contract {request.NumContrato} not suitable for SIMDA (expected NUM_CONTRATO = 0 or null)",
                    requestTimestamp,
                    stopwatch.ElapsedMilliseconds);

                _logger.LogWarning("SIMDA validation rejected: invalid contract {Contract}", request.NumContrato);
                return errorResponse;
            }

            // Execute with Polly policies: Timeout → Retry → Circuit Breaker
            var httpResponse = await _timeoutPolicy.ExecuteAsync(async (ct) =>
                await _retryPolicy.ExecuteAsync(async () =>
                    await _circuitBreakerPolicy.ExecuteAsync(async () =>
                        await CallSimdaSoapServiceAsync(request, ct))), cancellationToken);

            stopwatch.Stop();

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "SIMDA SOAP error: {StatusCode} - {ReasonPhrase}",
                    httpResponse.StatusCode, httpResponse.ReasonPhrase);

                return CreateErrorResponse(
                    "SOAP_ERROR",
                    $"SIMDA service returned {httpResponse.StatusCode}: {httpResponse.ReasonPhrase}",
                    requestTimestamp,
                    stopwatch.ElapsedMilliseconds);
            }

            // Parse SOAP response
            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var ezert8Code = ParseSoapResponse(responseContent);

            // T024: Structured logging - response with EZERT8 code and elapsed time
            _logger.LogInformation(
                "SIMDA validation completed: EZERT8={Ezert8}, ElapsedMs={ElapsedMs}, Success={Success}",
                ezert8Code, stopwatch.ElapsedMilliseconds, ezert8Code == "00000000");

            // Map EZERT8 to response
            return MapSimdaResponse(ezert8Code, requestTimestamp, stopwatch.ElapsedMilliseconds);
        }
        catch (BrokenCircuitException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "SIMDA circuit breaker open - service unavailable");

            return CreateErrorResponse(
                "SYS-005",
                "Serviço de validação indisponível (circuit breaker open)",
                requestTimestamp,
                stopwatch.ElapsedMilliseconds);
        }
        catch (TimeoutRejectedException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "SIMDA request timeout after 10 seconds");

            return CreateErrorResponse(
                "SYS-005",
                "Serviço de validação indisponível (timeout)",
                requestTimestamp,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "SIMDA validation exception: {Message}", ex.Message);

            return CreateErrorResponse(
                "SYS-005",
                "Serviço de validação indisponível (exception)",
                requestTimestamp,
                stopwatch.ElapsedMilliseconds);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("SIMDA");

            // SOAP services typically use ?wsdl for health check
            var response = await httpClient.GetAsync("?wsdl", cancellationToken);
            var isHealthy = response.IsSuccessStatusCode;

            _logger.LogInformation("SIMDA health check: {Status}", isHealthy ? "HEALTHY" : "UNHEALTHY");
            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SIMDA health check failed: {Message}", ex.Message);
            return false;
        }
    }

    /// <inheritdoc/>
    public bool SupportsContract(int? numContrato)
    {
        // T022: SIMDA handles HB contracts where NUM_CONTRATO = 0 or null
        return !numContrato.HasValue || numContrato.Value == 0;
    }

    /// <summary>
    /// Calls SIMDA SOAP 1.2 service with HTTP POST
    /// </summary>
    private async Task<HttpResponseMessage> CallSimdaSoapServiceAsync(
        ExternalValidationRequest request,
        CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("SIMDA");

        // Build SOAP envelope
        var soapEnvelope = BuildSoapEnvelope(request);
        var content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");
        content.Headers.Add("SOAPAction", SoapAction);

        // T024: Log SOAP request envelope
        _logger.LogDebug("SIMDA SOAP request: {Envelope}", soapEnvelope);

        return await httpClient.PostAsync("", content, cancellationToken);
    }

    /// <summary>
    /// Builds SOAP 1.2 envelope for SIMDA ValidateHBContract operation
    /// </summary>
    private string BuildSoapEnvelope(ExternalValidationRequest request)
    {
        var envelope = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/") + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", "http://schemas.xmlsoap.org/soap/envelope/"),
                new XAttribute(XNamespace.Xmlns + "sim", SoapNamespace),
                new XElement(XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/") + "Header"),
                new XElement(XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/") + "Body",
                    new XElement(XNamespace.Get(SoapNamespace) + "ValidateHBContractRequest",
                        new XElement(XNamespace.Get(SoapNamespace) + "ContractNumber", request.NumContrato ?? 0),
                        new XElement(XNamespace.Get(SoapNamespace) + "ClaimNumber",
                            $"{request.Orgsin:D2}/{request.Rmosin:D2}/{request.Numsin:D6}"),
                        new XElement(XNamespace.Get(SoapNamespace) + "PolicyType", request.TipoPagamento),
                        new XElement(XNamespace.Get(SoapNamespace) + "PrincipalAmount", request.ValorPrincipal)
                    )
                )
            )
        );

        return envelope.ToString();
    }

    /// <summary>
    /// Parses SOAP response envelope to extract EZERT8 code
    /// </summary>
    private string ParseSoapResponse(string soapResponse)
    {
        try
        {
            var doc = XDocument.Parse(soapResponse);
            var ns = XNamespace.Get(SoapNamespace);

            var ezert8Element = doc.Descendants(ns + "Ezert8").FirstOrDefault();
            if (ezert8Element != null)
            {
                return ezert8Element.Value;
            }

            _logger.LogWarning("EZERT8 element not found in SIMDA SOAP response");
            return "PARSE_ERROR";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse SIMDA SOAP response: {Message}", ex.Message);
            return "PARSE_ERROR";
        }
    }

    /// <summary>
    /// Maps SIMDA EZERT8 code to ExternalValidationResponse DTO
    /// </summary>
    private ExternalValidationResponse MapSimdaResponse(
        string ezert8Code,
        DateTime requestTimestamp,
        long elapsedMs)
    {
        var (errorCode, message) = EzertErrorMap.TryGetValue(ezert8Code, out var mapping)
            ? mapping
            : ("SYS-005", "Erro desconhecido de validação SIMDA");

        return new ExternalValidationResponse
        {
            Ezert8 = ezert8Code,
            ErrorMessage = ezert8Code == "00000000" ? null : message,
            ValidationService = SystemName,
            RequestTimestamp = requestTimestamp,
            ResponseTimestamp = DateTime.UtcNow,
            ElapsedMilliseconds = elapsedMs
        };
    }

    /// <summary>
    /// Creates error response for validation failures
    /// </summary>
    private ExternalValidationResponse CreateErrorResponse(
        string ezert8,
        string errorMessage,
        DateTime requestTimestamp,
        long elapsedMs)
    {
        return new ExternalValidationResponse
        {
            Ezert8 = ezert8,
            ErrorMessage = errorMessage,
            ValidationService = SystemName,
            RequestTimestamp = requestTimestamp,
            ResponseTimestamp = DateTime.UtcNow,
            ElapsedMilliseconds = elapsedMs
        };
    }
}
