namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// Request DTO for payment authorization
/// </summary>
public class PaymentAuthorizationRequest
{
    /// <summary>
    /// Claim identifier
    /// </summary>
    public int ClaimId { get; set; }

    /// <summary>
    /// Payment amount in the original currency
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (ISO 4217: BRL, USD, EUR, etc.)
    /// </summary>
    public string CurrencyCode { get; set; } = "BRL";

    /// <summary>
    /// Target currency for conversion (if different from CurrencyCode)
    /// </summary>
    public string? TargetCurrencyCode { get; set; }

    /// <summary>
    /// Product code for validation routing
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>
    /// Payment method (TRANSFER, CHECK, CREDIT, etc.)
    /// </summary>
    public string PaymentMethod { get; set; } = "TRANSFER";

    /// <summary>
    /// Beneficiary name
    /// </summary>
    public string BeneficiaryName { get; set; } = string.Empty;

    /// <summary>
    /// Beneficiary tax ID (CPF/CNPJ)
    /// </summary>
    public string BeneficiaryTaxId { get; set; } = string.Empty;

    /// <summary>
    /// Bank code for transfer
    /// </summary>
    public string? BankCode { get; set; }

    /// <summary>
    /// Bank branch
    /// </summary>
    public string? BranchCode { get; set; }

    /// <summary>
    /// Account number
    /// </summary>
    public string? AccountNumber { get; set; }

    /// <summary>
    /// Account type (CHECKING, SAVINGS)
    /// </summary>
    public string? AccountType { get; set; }

    /// <summary>
    /// Payment justification/notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User requesting authorization
    /// </summary>
    public string RequestedBy { get; set; } = string.Empty;

    /// <summary>
    /// Request timestamp
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Force validation bypass (admin only)
    /// </summary>
    public bool BypassValidation { get; set; } = false;
}
