namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// Response DTO for payment authorization
/// </summary>
public class PaymentAuthorizationResponse
{
    /// <summary>
    /// Authorization unique identifier
    /// </summary>
    public string AuthorizationId { get; set; } = string.Empty;

    /// <summary>
    /// Authorization status (APPROVED, REJECTED, PENDING, ERROR)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Original claim ID
    /// </summary>
    public int ClaimId { get; set; }

    /// <summary>
    /// Authorized amount in target currency
    /// </summary>
    public decimal AuthorizedAmount { get; set; }

    /// <summary>
    /// Final currency code
    /// </summary>
    public string CurrencyCode { get; set; } = "BRL";

    /// <summary>
    /// Applied exchange rate (if currency conversion occurred)
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Original amount (before conversion)
    /// </summary>
    public decimal? OriginalAmount { get; set; }

    /// <summary>
    /// Original currency code
    /// </summary>
    public string? OriginalCurrencyCode { get; set; }

    /// <summary>
    /// Authorization timestamp
    /// </summary>
    public DateTime AuthorizedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Authorized by user/system
    /// </summary>
    public string AuthorizedBy { get; set; } = string.Empty;

    /// <summary>
    /// Validation results from external systems
    /// </summary>
    public List<ValidationResult> ValidationResults { get; set; } = new();

    /// <summary>
    /// Error messages (if any)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Warning messages
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Transaction reference number
    /// </summary>
    public string? TransactionReference { get; set; }

    /// <summary>
    /// Estimated processing time (in seconds)
    /// </summary>
    public int? EstimatedProcessingTime { get; set; }

    /// <summary>
    /// Next required action (if status is PENDING)
    /// </summary>
    public string? NextAction { get; set; }

    /// <summary>
    /// Expiration timestamp for pending authorizations
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Validation result from external system
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Validation system name (CNOUA, SIPUA, SIMDA)
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// Validation status (APPROVED, REJECTED, ERROR, TIMEOUT)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Status message
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
    /// Additional validation data
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}
