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

    /// <summary>
    /// Transaction unique identifier (Guid for tracking atomicity)
    /// </summary>
    public Guid? TransactionId { get; set; }

    /// <summary>
    /// External validation results (from CNOUA/SIPUA/SIMDA)
    /// </summary>
    public List<ExternalValidationSummary> ExternalValidationResults { get; set; } = new();

    /// <summary>
    /// Indicates if transaction rollback occurred
    /// </summary>
    public bool RollbackOccurred { get; set; }

    /// <summary>
    /// Transaction step that failed (if rollback occurred)
    /// </summary>
    public string? FailedStep { get; set; }

    /// <summary>
    /// THISTSIN occurrence number (history record sequence)
    /// </summary>
    public int? HistoryOccurrence { get; set; }

    /// <summary>
    /// Transaction business date (DTMOVABE from TSISTEMA)
    /// </summary>
    public DateTime? TransactionDate { get; set; }

    /// <summary>
    /// Transaction system time (HHmmss format)
    /// </summary>
    public string? TransactionTime { get; set; }

    // Portuguese property names for backward compatibility with tests

    /// <summary>
    /// Sucesso - Success flag (Portuguese)
    /// </summary>
    public bool Sucesso => Status == "APPROVED";

    /// <summary>
    /// Ocorhist - History occurrence number (Portuguese alias for HistoryOccurrence)
    /// </summary>
    public int Ocorhist { get => HistoryOccurrence ?? 0; set => HistoryOccurrence = value; }

    /// <summary>
    /// Operacao - Operation code (always 1098 for payment authorization)
    /// </summary>
    public int Operacao { get; set; } = 1098;

    /// <summary>
    /// Valpri - Principal value (Portuguese)
    /// </summary>
    public decimal Valpri { get; set; }

    /// <summary>
    /// Crrmon - Correction/interest value (Portuguese)
    /// </summary>
    public decimal Crrmon { get; set; }

    /// <summary>
    /// Dtmovto - Transaction date (Portuguese alias for TransactionDate)
    /// </summary>
    public DateTime? Dtmovto { get => TransactionDate; set => TransactionDate = value; }

    /// <summary>
    /// Horaoper - Operation time (Portuguese alias for TransactionTime)
    /// </summary>
    public string? Horaoper { get => TransactionTime; set => TransactionTime = value; }

    /// <summary>
    /// Erros - Error list (Portuguese alias for Errors)
    /// </summary>
    public List<string> Erros { get => Errors; set => Errors = value; }

    /// <summary>
    /// Mensagem - Message (Portuguese) - first error or success message
    /// </summary>
    public string? Mensagem => Errors.Any() ? Errors.First() : (Sucesso ? "Autorização realizada com sucesso" : null);
}

/// <summary>
/// Simplified external validation summary for response
/// </summary>
public class ExternalValidationSummary
{
    /// <summary>
    /// Validation service name
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Validation status (APPROVED, REJECTED, ERROR, TIMEOUT)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Status message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }
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
