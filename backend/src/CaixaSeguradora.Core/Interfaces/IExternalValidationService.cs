namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// Interface for external validation services (CNOUA, SIPUA, SIMDA)
/// </summary>
public interface IExternalValidationService
{
    /// <summary>
    /// System name identifier (CNOUA, SIPUA, SIMDA)
    /// </summary>
    string SystemName { get; }

    /// <summary>
    /// Validates payment authorization request (legacy MVP method)
    /// </summary>
    /// <param name="request">Validation request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation response</returns>
    Task<LegacyExternalValidationResponse> ValidatePaymentAsync(
        LegacyExternalValidationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks system health and availability
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result</returns>
    Task<SystemHealthStatus> CheckHealthAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets system configuration and capabilities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System configuration</returns>
    Task<SystemConfiguration> GetConfigurationAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for external validation (legacy MVP version - use DTOs.ExternalValidationRequest for complete migration)
/// </summary>
public class LegacyExternalValidationRequest
{
    /// <summary>
    /// Claim identifier
    /// </summary>
    public int ClaimId { get; set; }

    /// <summary>
    /// Product code
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string CurrencyCode { get; set; } = "BRL";

    /// <summary>
    /// Beneficiary tax ID
    /// </summary>
    public string BeneficiaryTaxId { get; set; } = string.Empty;

    /// <summary>
    /// Payment method
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Request timestamp
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional validation parameters
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}

/// <summary>
/// Response from external validation (legacy MVP version - use DTOs.ExternalValidationResponse for complete migration)
/// </summary>
public class LegacyExternalValidationResponse
{
    /// <summary>
    /// Validation status (APPROVED, REJECTED, PENDING, ERROR, TIMEOUT)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Validation timestamp
    /// </summary>
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Validation code (system-specific)
    /// </summary>
    public string? ValidationCode { get; set; }

    /// <summary>
    /// Error details (if applicable)
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Additional response data
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }

    /// <summary>
    /// Indicates if validation was successful
    /// </summary>
    public bool IsSuccess => Status == "APPROVED";

    /// <summary>
    /// Indicates if request should be retried
    /// </summary>
    public bool ShouldRetry { get; set; }
}

/// <summary>
/// System health status
/// </summary>
public class SystemHealthStatus
{
    /// <summary>
    /// System name
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// Health status (HEALTHY, DEGRADED, UNHEALTHY, UNKNOWN)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Last check timestamp
    /// </summary>
    public DateTime LastCheckedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional health metrics
    /// </summary>
    public Dictionary<string, object>? Metrics { get; set; }

    /// <summary>
    /// Error message (if unhealthy)
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// System configuration
/// </summary>
public class SystemConfiguration
{
    /// <summary>
    /// System name
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// System version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Supported product codes
    /// </summary>
    public List<string> SupportedProducts { get; set; } = new();

    /// <summary>
    /// Timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Retry policy configuration
    /// </summary>
    public RetryConfiguration? RetryConfig { get; set; }

    /// <summary>
    /// Additional configuration properties
    /// </summary>
    public Dictionary<string, object>? Properties { get; set; }
}

/// <summary>
/// Retry configuration
/// </summary>
public class RetryConfiguration
{
    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Initial retry delay in milliseconds
    /// </summary>
    public int InitialDelayMs { get; set; } = 1000;

    /// <summary>
    /// Retry delay multiplier
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Maximum retry delay in milliseconds
    /// </summary>
    public int MaxDelayMs { get; set; } = 10000;
}
