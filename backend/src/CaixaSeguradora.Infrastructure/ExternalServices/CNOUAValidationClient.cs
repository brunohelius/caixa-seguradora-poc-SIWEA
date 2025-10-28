using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Infrastructure.ExternalServices;

/// <summary>
/// CNOUA (Consortium Product Validation) REST API client with Polly resilience policies
/// Handles validation for consortium product codes: 6814, 7701, 7709
/// Implements retry (3 attempts, exponential backoff), circuit breaker (5 failures, 30s break), and timeout (10s)
/// </summary>
public class CnouaValidationClient : ICnouaValidationClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CnouaValidationClient> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
    private readonly AsyncTimeoutPolicy _timeoutPolicy;

    // T023: EZERT8 error code mapping to Portuguese error messages (CONS-001 through CONS-005)
    private static readonly Dictionary<string, (string ErrorCode, string Message)> EzertErrorMap = new()
    {
        ["00000000"] = ("SUCCESS", "Validação aprovada"),
        ["EZERT8001"] = ("CONS-001", "Contrato de consórcio inválido"),
        ["EZERT8002"] = ("CONS-002", "Contrato cancelado"),
        ["EZERT8003"] = ("CONS-003", "Grupo encerrado"),
        ["EZERT8004"] = ("CONS-004", "Cota não contemplada"),
        ["EZERT8005"] = ("CONS-005", "Beneficiário não autorizado")
    };

    public string SystemName => "CNOUA";

    public CnouaValidationClient(
        IHttpClientFactory httpClientFactory,
        ILogger<CnouaValidationClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        // T018: Polly retry policy - 3 retries with exponential backoff (2s, 4s, 8s)
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
                        "CNOUA retry {RetryCount}/3 after {DelayMs}ms. Reason: {Reason}",
                        retryCount,
                        timespan.TotalMilliseconds,
                        outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
                });

        // T018: Polly circuit breaker - open after 5 consecutive failures for 30 seconds
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
                        "CNOUA circuit breaker opened for {BreakSeconds}s after 5 failures. Reason: {Reason}",
                        breakDelay.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown");
                },
                onReset: () =>
                {
                    _logger.LogInformation("CNOUA circuit breaker reset - service recovered");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("CNOUA circuit breaker half-open - testing service");
                });

        // T018: Polly timeout policy - 10 seconds per request
        _timeoutPolicy = Policy.TimeoutAsync(
            seconds: 10,
            onTimeoutAsync: (context, timespan, task) =>
            {
                _logger.LogWarning("CNOUA request timeout after {TimeoutSeconds}s", timespan.TotalSeconds);
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
            "CNOUA validation started: Fonte={Fonte}, Protocol={Protocol}, DAC={Dac}, Product={Product}, Amount={Amount}",
            request.Fonte, request.Protsini, request.Dac, request.CodProdu, request.ValorPrincipal);

        try
        {
            // Validate product routing
            if (!SupportsProduct(request.CodProdu))
            {
                var errorResponse = CreateErrorResponse(
                    "UNSUPPORTED_PRODUCT",
                    $"Product code {request.CodProdu} not supported by CNOUA (expected 6814, 7701, or 7709)",
                    requestTimestamp,
                    stopwatch.ElapsedMilliseconds);

                _logger.LogWarning("CNOUA validation rejected: unsupported product {Product}", request.CodProdu);
                return errorResponse;
            }

            // Execute with Polly policies: Timeout → Retry → Circuit Breaker
            var httpResponse = await _timeoutPolicy.ExecuteAsync(async (ct) =>
                await _retryPolicy.ExecuteAsync(async () =>
                    await _circuitBreakerPolicy.ExecuteAsync(async () =>
                        await CallCnouaServiceAsync(request, ct))), cancellationToken);

            stopwatch.Stop();

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CNOUA HTTP error: {StatusCode} - {ReasonPhrase}",
                    httpResponse.StatusCode, httpResponse.ReasonPhrase);

                return CreateErrorResponse(
                    "HTTP_ERROR",
                    $"CNOUA service returned {httpResponse.StatusCode}: {httpResponse.ReasonPhrase}",
                    requestTimestamp,
                    stopwatch.ElapsedMilliseconds);
            }

            // Parse response
            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var cnouaResponse = JsonSerializer.Deserialize<CnouaApiResponse>(responseContent, jsonOptions);

            if (cnouaResponse == null)
            {
                _logger.LogError("CNOUA response deserialization failed: {Content}", responseContent);
                return CreateErrorResponse(
                    "PARSE_ERROR",
                    "Failed to parse CNOUA response",
                    requestTimestamp,
                    stopwatch.ElapsedMilliseconds);
            }

            // T024: Structured logging - response with EZERT8 code and elapsed time
            _logger.LogInformation(
                "CNOUA validation completed: EZERT8={Ezert8}, ElapsedMs={ElapsedMs}, Success={Success}",
                cnouaResponse.Ezert8, stopwatch.ElapsedMilliseconds, cnouaResponse.Ezert8 == "00000000");

            // Map EZERT8 to response
            return MapCnouaResponse(cnouaResponse, requestTimestamp, stopwatch.ElapsedMilliseconds);
        }
        catch (BrokenCircuitException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "CNOUA circuit breaker open - service unavailable");

            return CreateErrorResponse(
                "SYS-005",
                "Serviço de validação indisponível (circuit breaker open)",
                requestTimestamp,
                stopwatch.ElapsedMilliseconds);
        }
        catch (TimeoutRejectedException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "CNOUA request timeout after 10 seconds");

            return CreateErrorResponse(
                "SYS-005",
                "Serviço de validação indisponível (timeout)",
                requestTimestamp,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "CNOUA validation exception: {Message}", ex.Message);

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
            var httpClient = _httpClientFactory.CreateClient("CNOUA");
            var healthEndpoint = "health"; // Relative to base address

            var response = await httpClient.GetAsync(healthEndpoint, cancellationToken);
            var isHealthy = response.IsSuccessStatusCode;

            _logger.LogInformation("CNOUA health check: {Status}", isHealthy ? "HEALTHY" : "UNHEALTHY");
            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CNOUA health check failed: {Message}", ex.Message);
            return false;
        }
    }

    /// <inheritdoc/>
    public bool SupportsProduct(int productCode)
    {
        // T022: Consortium product routing (6814, 7701, 7709)
        var supportedProducts = new[] { 6814, 7701, 7709 };
        return supportedProducts.Contains(productCode);
    }

    /// <summary>
    /// Calls CNOUA REST API service with HTTP POST
    /// </summary>
    private async Task<HttpResponseMessage> CallCnouaServiceAsync(
        ExternalValidationRequest request,
        CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("CNOUA");

        // Build request payload for CNOUA API
        var payload = new
        {
            fonte = request.Fonte,
            protocolNumber = request.Protsini,
            dac = request.Dac,
            productCode = request.CodProdu,
            claimNumber = $"{request.Orgsin:D2}/{request.Rmosin:D2}/{request.Numsin:D6}",
            contractNumber = request.NumContrato,
            principalAmount = request.ValorPrincipal,
            beneficiary = request.Beneficiario
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // T024: Log request payload
        _logger.LogDebug("CNOUA request payload: {Payload}", json);

        return await httpClient.PostAsync("validate", content, cancellationToken);
    }

    /// <summary>
    /// Maps CNOUA API response to ExternalValidationResponse DTO
    /// </summary>
    private ExternalValidationResponse MapCnouaResponse(
        CnouaApiResponse cnouaResponse,
        DateTime requestTimestamp,
        long elapsedMs)
    {
        var (errorCode, message) = EzertErrorMap.TryGetValue(cnouaResponse.Ezert8, out var mapping)
            ? mapping
            : ("SYS-005", "Erro desconhecido de validação CNOUA");

        return new ExternalValidationResponse
        {
            Ezert8 = cnouaResponse.Ezert8,
            ErrorMessage = cnouaResponse.Ezert8 == "00000000" ? null : message,
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

    /// <summary>
    /// CNOUA API response structure
    /// </summary>
    private class CnouaApiResponse
    {
        public string Ezert8 { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
