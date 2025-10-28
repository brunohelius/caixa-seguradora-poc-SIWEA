using FluentValidation;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Core.Validators.BusinessRules;

/// <summary>
/// Payment Validation Rules (BR-010 to BR-026)
/// Enforces business rules for payment authorization
/// </summary>
public class PaymentValidationRules : AbstractValidator<PaymentAuthorizationRequest>
{
    /// <summary>
    /// Constructor with optional claim context for balance validation
    /// </summary>
    public PaymentValidationRules(ClaimMaster? claim = null)
    {
        // BR-010: Payment type must be 1, 2, 3, 4, or 5
        RuleFor(x => x.PaymentType)
            .InclusiveBetween(1, 5)
            .WithMessage("Tipo de pagamento deve ser 1, 2, 3, 4 ou 5")
            .WithErrorCode("BR-010");

        // BR-012: Principal amount must be numeric, non-negative
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor principal deve ser não negativo")
            .WithErrorCode("BR-012");

        // BR-013: Principal amount <= pending value (SDOPAG - TOTPAG)
        // This requires claim context - validated at service layer with proper error handling
        When(x => claim != null, () =>
        {
            RuleFor(x => x.Amount)
                .Must((request, amount) =>
                {
                    if (claim == null) return true;
                    var pending = claim.Sdopag - claim.Totpag;
                    var totalAmount = amount + request.ValorCorrecao;
                    return totalAmount <= pending;
                })
                .WithMessage((request) =>
                {
                    if (claim == null) return "Valor excede saldo pendente";
                    var pending = claim.Sdopag - claim.Totpag;
                    return $"Valor total (R$ {request.Amount + request.ValorCorrecao:N2}) excede o saldo pendente (R$ {pending:N2})";
                })
                .WithErrorCode("BR-013");
        });

        // BR-015: Policy type must be 1 or 2
        When(x => !string.IsNullOrEmpty(x.TipoApolice), () =>
        {
            RuleFor(x => x.TipoApolice)
                .Must(tipo => tipo == "1" || tipo == "2")
                .WithMessage("Tipo de apólice deve ser 1 ou 2")
                .WithErrorCode("BR-015");
        });

        // BR-017: Correction amount optional, defaults to 0.00
        RuleFor(x => x.ValorCorrecao)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor de correção deve ser não negativo (padrão: 0.00)")
            .WithErrorCode("BR-017");

        // BR-018: Correction amount numeric, non-negative (already covered by BR-017)

        // BR-019: Beneficiary required if TPSEGU != 0
        // Note: TPSEGU comes from claim, requires service-layer validation with claim context
        When(x => claim != null && claim.Tipseg != 0, () =>
        {
            RuleFor(x => x.BeneficiaryName)
                .NotEmpty()
                .WithMessage("Favorecido é obrigatório para este tipo de seguro (TIPSEG != 0)")
                .WithErrorCode("BR-019");
        });

        // BR-021: Beneficiary field max 255 characters
        When(x => !string.IsNullOrEmpty(x.BeneficiaryName), () =>
        {
            RuleFor(x => x.BeneficiaryName)
                .MaximumLength(255)
                .WithMessage("Favorecido não pode exceder 255 caracteres")
                .WithErrorCode("BR-021");
        });

        // BR-022: Special characters in beneficiary preserved
        // This is an implementation rule, not a validation rule - characters are preserved in data layer

        // BR-023: Currency conversion formula: AMOUNT_BTNF = AMOUNT × VLCRUZAD
        // This is a calculation rule enforced in the currency conversion service

        // BR-024: Conversion rate from TGEUNIMO table
        // This is a data retrieval rule enforced in the currency conversion service

        // BR-025: Rate selection: DTINIVIG <= DATE <= DTTERVIG
        // This is a data retrieval rule with validation in the currency conversion service

        // BR-026: No rate for date error message (handled in ErrorMessages.pt-BR.resx)
        // VAL-008: "Taxa de conversão não disponível para a data"
    }
}

/// <summary>
/// Payment Validation Rules with ClaimMaster context
/// Use this when you have the claim loaded for balance validation
/// </summary>
public class PaymentValidationRulesWithClaim : AbstractValidator<(PaymentAuthorizationRequest Request, ClaimMaster Claim)>
{
    public PaymentValidationRulesWithClaim()
    {
        // Include base payment rules
        RuleFor(x => x.Request)
            .SetValidator(ctx => new PaymentValidationRules(ctx.Claim));

        // BR-013: Principal amount <= pending value with full context
        RuleFor(x => x)
            .Must(ctx =>
            {
                var pending = ctx.Claim.Sdopag - ctx.Claim.Totpag;
                var totalAmount = ctx.Request.Amount + ctx.Request.ValorCorrecao;
                return totalAmount <= pending;
            })
            .WithMessage(ctx =>
            {
                var pending = ctx.Claim.Sdopag - ctx.Claim.Totpag;
                var totalAmount = ctx.Request.Amount + ctx.Request.ValorCorrecao;
                return $"Valor total (R$ {totalAmount:N2}) excede o saldo pendente (R$ {pending:N2})";
            })
            .WithErrorCode("BR-013");

        // BR-019: Beneficiary required if TPSEGU != 0 with full context
        RuleFor(x => x)
            .Must(ctx => ctx.Claim.Tipseg == 0 || !string.IsNullOrWhiteSpace(ctx.Request.BeneficiaryName))
            .WithMessage("Favorecido é obrigatório para este tipo de seguro (TIPSEG != 0)")
            .WithErrorCode("BR-019");
    }
}

/// <summary>
/// Error codes for payment validation
/// </summary>
public static class PaymentErrorCodes
{
    // BR-011
    public const string InvalidPaymentType = "VAL-004";

    // BR-014
    public const string AmountExceedsPending = "VAL-005";

    // BR-016
    public const string InvalidPolicyType = "VAL-006";

    // BR-020
    public const string MissingBeneficiary = "VAL-007";

    // BR-026
    public const string CurrencyRateNotFound = "VAL-008";
}
