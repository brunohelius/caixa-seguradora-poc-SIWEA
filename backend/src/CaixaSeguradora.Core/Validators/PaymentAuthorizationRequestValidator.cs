using FluentValidation;
using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Validators;

/// <summary>
/// FluentValidation validator for PaymentAuthorizationRequest
/// Implements business rules: BR-004, BR-006, BR-019
/// Note: BR-005 (amount vs pending balance) is validated in service layer after claim retrieval
/// </summary>
public class PaymentAuthorizationRequestValidator : AbstractValidator<PaymentAuthorizationRequest>
{
    public PaymentAuthorizationRequestValidator()
    {
        // BR-006: Composite key validation
        RuleFor(x => x.Tipseg)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TIPSEG deve ser maior ou igual a 0")
            .WithErrorCode("BR-006");

        RuleFor(x => x.Orgsin)
            .InclusiveBetween(1, 99)
            .WithMessage("ORGSIN deve estar entre 1 e 99")
            .WithErrorCode("BR-006");

        RuleFor(x => x.Rmosin)
            .InclusiveBetween(0, 99)
            .WithMessage("RMOSIN deve estar entre 0 e 99")
            .WithErrorCode("BR-006");

        RuleFor(x => x.Numsin)
            .InclusiveBetween(1, 999999)
            .WithMessage("NUMSIN deve estar entre 1 e 999999")
            .WithErrorCode("BR-006");

        // BR-004: Payment type validation (1-5)
        RuleFor(x => x.TipoPagamento)
            .InclusiveBetween(1, 5)
            .WithMessage("Tipo de Pagamento deve ser 1, 2, 3, 4, ou 5")
            .WithErrorCode("BR-004");

        // Amount validation
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Valor do pagamento deve ser maior que zero")
            .WithErrorCode("VALIDATION-001");

        RuleFor(x => x.Amount)
            .Must(amount => amount == Math.Round(amount, 2))
            .WithMessage("Valor do pagamento deve ter no máximo 2 casas decimais")
            .WithErrorCode("VALIDATION-002");

        // Currency validation
        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .WithMessage("Código da moeda é obrigatório")
            .Length(3)
            .WithMessage("Código da moeda deve ter 3 caracteres (ex: BRL, USD)")
            .WithErrorCode("VALIDATION-003");

        // Product code validation
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .WithMessage("Código do produto é obrigatório")
            .WithErrorCode("VALIDATION-004");

        // Payment method validation
        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Método de pagamento é obrigatório")
            .WithErrorCode("VALIDATION-005");

        // BR-019: Beneficiary name validation (conditional, detailed validation in service layer)
        // Service layer will validate based on TPSEGU value from claim record
        RuleFor(x => x.BeneficiaryName)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.BeneficiaryName))
            .WithMessage("Nome do beneficiário não pode ser vazio se fornecido")
            .WithErrorCode("BR-019");

        RuleFor(x => x.BeneficiaryName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.BeneficiaryName))
            .WithMessage("Nome do beneficiário deve ter no máximo 100 caracteres")
            .WithErrorCode("VALIDATION-006");

        // Operator ID validation (audit trail)
        RuleFor(x => x.RequestedBy)
            .NotEmpty()
            .WithMessage("ID do operador é obrigatório (EZEUSRID)")
            .WithErrorCode("VALIDATION-007");

        RuleFor(x => x.RequestedBy)
            .MaximumLength(20)
            .WithMessage("ID do operador deve ter no máximo 20 caracteres")
            .WithErrorCode("VALIDATION-007");

        // External validation routing validation
        RuleFor(x => x.ExternalValidationRequired)
            .Equal(true)
            .When(x => !string.IsNullOrWhiteSpace(x.RoutingService))
            .WithMessage("ExternalValidationRequired deve ser true quando RoutingService é fornecido")
            .WithErrorCode("VALIDATION-008");

        RuleFor(x => x.RoutingService)
            .Must(service => service == null || service == "CNOUA" || service == "SIPUA" || service == "SIMDA")
            .WithMessage("RoutingService deve ser CNOUA, SIPUA, SIMDA, ou null (roteamento automático)")
            .WithErrorCode("VALIDATION-009");

        // Contract number validation (required for SIPUA/SIMDA routing)
        RuleFor(x => x.NumContrato)
            .GreaterThan(0)
            .When(x => x.ExternalValidationRequired && x.RoutingService is "SIPUA" or "SIMDA")
            .WithMessage("Número do contrato é obrigatório para validação SIPUA/SIMDA")
            .WithErrorCode("VALIDATION-010");
    }
}
