using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Infrastructure.ExternalServices;

/// <summary>
/// CNOUA (Consórcio Nacional de Ouvidorias) validation client
/// </summary>
public class CNOUAValidationClient : IExternalValidationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CNOUAValidationClient> _logger;

    // T089: EZERT8 error code mapping dictionary
    private static readonly Dictionary<string, string> EzertErrorMessages = new()
    {
        ["EZERT8001"] = "Contrato de consórcio inválido",
        ["EZERT8002"] = "Contrato cancelado",
        ["EZERT8003"] = "Grupo encerrado",
        ["EZERT8004"] = "Cota suspensa por inadimplência",
        ["EZERT8005"] = "Participante não contemplado",
        ["EZERT8006"] = "Documentação pendente",
        ["EZERT8007"] = "Valor excede limite permitido",
        ["EZERT8008"] = "Prazo de carência não cumprido",
        ["EZERT8009"] = "Beneficiário não autorizado",
        ["EZERT8010"] = "Duplicidade de solicitação",
        ["00000000"] = "Validação aprovada" // Success code
    };

    public string SystemName => "CNOUA";

    public CNOUAValidationClient(
        IHttpClientFactory httpClientFactory,
        ILogger<CNOUAValidationClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ExternalValidationResponse> ValidatePaymentAsync(
        ExternalValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("CNOUA validation started for ClaimId: {ClaimId}, Product: {Product}",
                request.ClaimId, request.ProductCode);

            // Validate product code routing
            if (!IsProductSupported(request.ProductCode))
            {
                stopwatch.Stop();
                return new ExternalValidationResponse
                {
                    Status = "REJECTED",
                    Message = $"Product code {request.ProductCode} not supported by CNOUA",
                    ValidatedAt = DateTime.UtcNow,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    ValidationCode = "CNOUA_UNSUPPORTED_PRODUCT"
                };
            }

            // Simulate CNOUA validation logic
            var validationResult = await PerformCNOUAValidationAsync(request, cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation("CNOUA validation completed: {Status} in {Ms}ms",
                validationResult.Status, stopwatch.ElapsedMilliseconds);

            validationResult.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            return validationResult;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "CNOUA validation failed for ClaimId: {ClaimId}", request.ClaimId);

            return new ExternalValidationResponse
            {
                Status = "ERROR",
                Message = "CNOUA system error",
                ValidatedAt = DateTime.UtcNow,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorDetails = ex.Message,
                ShouldRetry = true
            };
        }
    }

    /// <inheritdoc/>
    public async Task<SystemHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Simulate health check
            await Task.Delay(50, cancellationToken);

            stopwatch.Stop();

            return new SystemHealthStatus
            {
                SystemName = SystemName,
                Status = "HEALTHY",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                LastCheckedAt = DateTime.UtcNow,
                Metrics = new Dictionary<string, object>
                {
                    ["uptime_seconds"] = 86400,
                    ["request_count_24h"] = 1523,
                    ["avg_response_time_ms"] = 125
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "CNOUA health check failed");

            return new SystemHealthStatus
            {
                SystemName = SystemName,
                Status = "UNHEALTHY",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                LastCheckedAt = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <inheritdoc/>
    public Task<SystemConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var config = new SystemConfiguration
        {
            SystemName = SystemName,
            Version = "2.1.0",
            SupportedProducts = new List<string> { "CNOUA_CONSORTIUM", "CNOUA_PROPERTY", "CNOUA_VEHICLE" },
            TimeoutSeconds = 30,
            RetryConfig = new RetryConfiguration
            {
                MaxRetries = 3,
                InitialDelayMs = 1000,
                BackoffMultiplier = 2.0,
                MaxDelayMs = 10000
            },
            Properties = new Dictionary<string, object>
            {
                ["max_payment_amount"] = 500000,
                ["requires_authorization"] = true,
                ["validation_rules"] = "CNOUA_STANDARD_V2"
            }
        };

        return Task.FromResult(config);
    }

    /// <summary>
    /// Checks if product code is supported by CNOUA
    /// </summary>
    private bool IsProductSupported(string productCode)
    {
        var supportedPrefixes = new[] { "CNOUA", "CNS", "CONSORTIUM" };
        return supportedPrefixes.Any(prefix =>
            productCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Performs CNOUA-specific validation logic
    /// </summary>
    private async Task<ExternalValidationResponse> PerformCNOUAValidationAsync(
        ExternalValidationRequest request,
        CancellationToken cancellationToken)
    {
        // Simulate external API call
        await Task.Delay(150, cancellationToken);

        // Business rules validation
        var validationErrors = new List<string>();

        // Rule 1: Maximum payment amount
        if (request.Amount > 500000)
        {
            validationErrors.Add("Payment amount exceeds CNOUA maximum limit (500,000)");
        }

        // Rule 2: Tax ID validation
        if (string.IsNullOrWhiteSpace(request.BeneficiaryTaxId))
        {
            validationErrors.Add("Beneficiary tax ID is required for CNOUA validation");
        }

        // Rule 3: Currency validation
        if (request.CurrencyCode != "BRL")
        {
            validationErrors.Add("CNOUA only supports BRL currency");
        }

        // Return validation result
        if (validationErrors.Any())
        {
            return new ExternalValidationResponse
            {
                Status = "REJECTED",
                Message = string.Join("; ", validationErrors),
                ValidatedAt = DateTime.UtcNow,
                ValidationCode = "CNOUA_VALIDATION_FAILED",
                AdditionalData = new Dictionary<string, object>
                {
                    ["validation_errors"] = validationErrors
                }
            };
        }

        // T089: Simulate EZERT8 response code (in production this would come from actual service)
        var ezertCode = SimulateEzertResponse(request);

        // Log raw EZERT8 code for debugging
        _logger.LogInformation("CNOUA EZERT8 code received: {EzertCode} for ClaimId: {ClaimId}",
            ezertCode, request.ClaimId);

        // Map EZERT8 code to Portuguese error message
        var errorMessage = MapEzertCodeToMessage(ezertCode);

        if (ezertCode != "00000000")
        {
            _logger.LogWarning("CNOUA validation failed with EZERT8: {EzertCode} - {Message}",
                ezertCode, errorMessage);

            return new ExternalValidationResponse
            {
                Status = "REJECTED",
                Message = errorMessage,
                ValidatedAt = DateTime.UtcNow,
                ValidationCode = ezertCode,
                AdditionalData = new Dictionary<string, object>
                {
                    ["ezert8_code"] = ezertCode,
                    ["raw_error_code"] = ezertCode,
                    ["error_source"] = "CNOUA"
                }
            };
        }

        return new ExternalValidationResponse
        {
            Status = "APPROVED",
            Message = "CNOUA validation passed successfully",
            ValidatedAt = DateTime.UtcNow,
            ValidationCode = ezertCode,
            AdditionalData = new Dictionary<string, object>
            {
                ["approval_reference"] = $"CNOUA-{DateTime.UtcNow:yyyyMMddHHmmss}-{request.ClaimId}",
                ["validation_level"] = "STANDARD",
                ["ezert8_code"] = ezertCode
            }
        };
    }

    /// <summary>
    /// Maps EZERT8 code to Portuguese error message
    /// </summary>
    private string MapEzertCodeToMessage(string ezertCode)
    {
        if (EzertErrorMessages.TryGetValue(ezertCode, out var message))
        {
            return message;
        }

        _logger.LogWarning("Unknown EZERT8 code: {EzertCode}. Using generic error message.", ezertCode);
        return $"Erro de validação CNOUA (código: {ezertCode})";
    }

    /// <summary>
    /// Simulates EZERT8 response code (for demonstration purposes)
    /// In production, this would be replaced with actual service call parsing
    /// </summary>
    private string SimulateEzertResponse(ExternalValidationRequest request)
    {
        // Simulate business rules that would return different EZERT8 codes
        if (request.Amount > 500000)
        {
            return "EZERT8007"; // Valor excede limite permitido
        }

        if (string.IsNullOrWhiteSpace(request.BeneficiaryTaxId))
        {
            return "EZERT8009"; // Beneficiário não autorizado
        }

        // Default success
        return "00000000";
    }
}
