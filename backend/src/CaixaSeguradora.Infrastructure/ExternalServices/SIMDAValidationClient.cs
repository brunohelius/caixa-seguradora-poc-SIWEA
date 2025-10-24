using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Infrastructure.ExternalServices;

/// <summary>
/// SIMDA (Sistema Integrado de Monitoramento e Dados de Apólices) validation client
/// </summary>
public class SIMDAValidationClient : IExternalValidationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SIMDAValidationClient> _logger;

    public string SystemName => "SIMDA";

    public SIMDAValidationClient(
        IHttpClientFactory httpClientFactory,
        ILogger<SIMDAValidationClient> logger)
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
            _logger.LogInformation("SIMDA validation started for ClaimId: {ClaimId}, Product: {Product}",
                request.ClaimId, request.ProductCode);

            if (!IsProductSupported(request.ProductCode))
            {
                stopwatch.Stop();
                return new ExternalValidationResponse
                {
                    Status = "REJECTED",
                    Message = $"Product code {request.ProductCode} not supported by SIMDA",
                    ValidatedAt = DateTime.UtcNow,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    ValidationCode = "SIMDA_UNSUPPORTED_PRODUCT"
                };
            }

            var validationResult = await PerformSIMDAValidationAsync(request, cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation("SIMDA validation completed: {Status} in {Ms}ms",
                validationResult.Status, stopwatch.ElapsedMilliseconds);

            validationResult.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            return validationResult;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "SIMDA validation failed for ClaimId: {ClaimId}", request.ClaimId);

            return new ExternalValidationResponse
            {
                Status = "ERROR",
                Message = "SIMDA system error",
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
            await Task.Delay(60, cancellationToken);

            stopwatch.Stop();

            return new SystemHealthStatus
            {
                SystemName = SystemName,
                Status = "HEALTHY",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                LastCheckedAt = DateTime.UtcNow,
                Metrics = new Dictionary<string, object>
                {
                    ["uptime_seconds"] = 259200,
                    ["request_count_24h"] = 2876,
                    ["avg_response_time_ms"] = 85,
                    ["success_rate"] = 0.995,
                    ["database_connections"] = 45
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "SIMDA health check failed");

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
            Version = "4.2.1",
            SupportedProducts = new List<string> { "SIMDA_POLICY", "SIMDA_CLAIM", "SIMDA_ENDORSEMENT", "SIMDA_RENEWAL" },
            TimeoutSeconds = 20,
            RetryConfig = new RetryConfiguration
            {
                MaxRetries = 3,
                InitialDelayMs = 750,
                BackoffMultiplier = 2.5,
                MaxDelayMs = 8000
            },
            Properties = new Dictionary<string, object>
            {
                ["max_payment_amount"] = 2000000,
                ["requires_authorization"] = true,
                ["validation_rules"] = "SIMDA_COMPREHENSIVE_V4",
                ["supports_bulk_validation"] = true,
                ["policy_verification_enabled"] = true
            }
        };

        return Task.FromResult(config);
    }

    private bool IsProductSupported(string productCode)
    {
        var supportedPrefixes = new[] { "SIMDA", "SIM", "POLICY", "CLAIM", "ENDORSEMENT", "RENEWAL" };
        return supportedPrefixes.Any(prefix =>
            productCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<ExternalValidationResponse> PerformSIMDAValidationAsync(
        ExternalValidationRequest request,
        CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);

        var validationErrors = new List<string>();
        var warnings = new List<string>();

        // SIMDA-specific comprehensive validation
        if (request.Amount > 2000000)
        {
            validationErrors.Add("Payment amount exceeds SIMDA maximum limit (2,000,000)");
        }

        if (request.Amount > 1000000)
        {
            warnings.Add("High-value payment requires additional approval levels");
        }

        // Policy validation simulation
        var policyExists = await ValidatePolicyExistsAsync(request.ClaimId, cancellationToken);
        if (!policyExists)
        {
            validationErrors.Add("No active policy found for claim");
        }

        // Coverage validation
        var coverageValid = await ValidateCoverageAsync(request.ClaimId, request.Amount, cancellationToken);
        if (!coverageValid)
        {
            validationErrors.Add("Payment amount exceeds policy coverage limits");
        }

        // Multi-currency support check
        if (request.CurrencyCode != "BRL" && request.CurrencyCode != "USD")
        {
            warnings.Add($"Currency {request.CurrencyCode} requires additional verification");
        }

        if (validationErrors.Any())
        {
            return new ExternalValidationResponse
            {
                Status = "REJECTED",
                Message = string.Join("; ", validationErrors),
                ValidatedAt = DateTime.UtcNow,
                ValidationCode = "SIMDA_VALIDATION_FAILED",
                AdditionalData = new Dictionary<string, object>
                {
                    ["validation_errors"] = validationErrors,
                    ["warnings"] = warnings,
                    ["requires_manual_review"] = true,
                    ["escalation_level"] = validationErrors.Count > 1 ? "HIGH" : "MEDIUM"
                }
            };
        }

        return new ExternalValidationResponse
        {
            Status = "APPROVED",
            Message = "SIMDA validation passed successfully",
            ValidatedAt = DateTime.UtcNow,
            ValidationCode = "SIMDA_APPROVED",
            AdditionalData = new Dictionary<string, object>
            {
                ["approval_reference"] = $"SIMDA-{DateTime.UtcNow:yyyyMMddHHmmss}-{request.ClaimId}",
                ["validation_level"] = "COMPREHENSIVE",
                ["policy_verified"] = true,
                ["coverage_verified"] = true,
                ["risk_score"] = 0.08,
                ["fraud_check"] = "PASSED",
                ["warnings"] = warnings
            }
        };
    }

    private async Task<bool> ValidatePolicyExistsAsync(int claimId, CancellationToken cancellationToken)
    {
        await Task.Delay(20, cancellationToken);
        // Mock: assume policy exists for all claims
        return true;
    }

    private async Task<bool> ValidateCoverageAsync(int claimId, decimal amount, CancellationToken cancellationToken)
    {
        await Task.Delay(30, cancellationToken);
        // Mock: assume coverage is valid if amount < 1.5M
        return amount <= 1500000;
    }
}
