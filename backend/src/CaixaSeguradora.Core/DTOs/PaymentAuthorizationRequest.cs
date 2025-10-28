using System.ComponentModel.DataAnnotations;

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

    /// <summary>
    /// External validation required before authorization
    /// </summary>
    public bool ExternalValidationRequired { get; set; } = false;

    /// <summary>
    /// Target validation service (CNOUA, SIPUA, SIMDA) - null for auto-routing
    /// </summary>
    public string? RoutingService { get; set; }

    /// <summary>
    /// Payment type (1-5 per BR-004)
    /// 1 = Direct Payment, 2 = Reimbursement, 3 = Salvage, 4 = Advance, 5 = Other
    /// </summary>
    [Range(1, 5, ErrorMessage = "Payment type must be between 1 and 5")]
    public int PaymentType { get; set; } = 1;

    /// <summary>
    /// Claim composite key: Insurance Type
    /// </summary>
    public int? Tipseg { get; set; }

    /// <summary>
    /// Claim composite key: Origin
    /// </summary>
    public int? Orgsin { get; set; }

    /// <summary>
    /// Claim composite key: Branch
    /// </summary>
    public int? Rmosin { get; set; }

    /// <summary>
    /// Claim composite key: Claim Number
    /// </summary>
    public int? Numsin { get; set; }

    // Portuguese property names for backward compatibility with tests

    /// <summary>
    /// Tipo de pagamento (1-5) - Portuguese alias for PaymentType
    /// </summary>
    public int TipoPagamento { get => PaymentType; set => PaymentType = value; }

    /// <summary>
    /// Valor principal - Portuguese alias for Amount
    /// </summary>
    public decimal ValorPrincipal { get => Amount; set => Amount = value; }

    /// <summary>
    /// Valor de correção - Portuguese alias for correction/interest
    /// </summary>
    public decimal ValorCorrecao { get; set; }

    /// <summary>
    /// Favorecido/Beneficiário - Portuguese alias for BeneficiaryName
    /// </summary>
    public string? Favorecido { get => BeneficiaryName; set => BeneficiaryName = value ?? string.Empty; }

    /// <summary>
    /// Tipo de apólice - Portuguese alias for policy type
    /// </summary>
    public string? TipoApolice { get; set; }

    /// <summary>
    /// Observações - Portuguese alias for Notes
    /// </summary>
    public string? Observacoes { get => Notes; set => Notes = value; }

    /// <summary>
    /// Número do contrato (for consortium contract validation routing)
    /// </summary>
    public int? NumContrato { get; set; }
}
