using FluentValidation;
using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Validators.BusinessRules;

/// <summary>
/// Product Validation Rules (BR-047 to BR-060)
/// Enforces business rules for external product validation and phase management
/// </summary>
public class ProductValidationRules : AbstractValidator<ProductValidationContext>
{
    private static readonly int[] ConsortiumProducts = { 6814, 7701, 7709 };

    public ProductValidationRules()
    {
        // BR-043: Consortium products: 6814, 7701, 7709 → CNOUA
        RuleFor(x => x.ProductCode)
            .Must((ctx, code) =>
            {
                if (!code.HasValue || !ConsortiumProducts.Contains(code.Value))
                    return true; // Not a consortium product, no routing needed

                return ctx.RoutingService == "CNOUA" || string.IsNullOrEmpty(ctx.RoutingService);
            })
            .WithMessage("Produtos de consórcio (6814, 7701, 7709) devem ser validados via CNOUA")
            .WithErrorCode("BR-043");

        // BR-044: Query EF_CONTR_SEG_HABIT for contract number
        // This is a data retrieval rule enforced in the external service router

        // BR-045: EFP contract (NUM_CONTRATO > 0) → SIPUA
        When(x => x.ContractNumber.HasValue && x.ContractNumber > 0, () =>
        {
            RuleFor(x => x.RoutingService)
                .Must(service => service == "SIPUA" || string.IsNullOrEmpty(service))
                .WithMessage("Contratos EFP (NUM_CONTRATO > 0) devem ser validados via SIPUA")
                .WithErrorCode("BR-045");
        });

        // BR-046: HB contract (NUM_CONTRATO = 0 or not found) → SIMDA
        When(x => !x.ContractNumber.HasValue || x.ContractNumber == 0, () =>
        {
            RuleFor(x => x.RoutingService)
                .Must(service => service == "SIMDA" || string.IsNullOrEmpty(service))
                .When(x => x.ProductCode.HasValue && !ConsortiumProducts.Contains(x.ProductCode.Value))
                .WithMessage("Contratos HB (NUM_CONTRATO = 0 ou não encontrado) devem ser validados via SIMDA")
                .WithErrorCode("BR-046");
        });

        // BR-047: External service response code EZERT8 checked for success
        When(x => x.ExternalValidationResponse != null, () =>
        {
            RuleFor(x => x.ExternalValidationResponse!.Ezert8)
                .NotEmpty()
                .WithMessage("Código de resposta (EZERT8) é obrigatório na validação externa")
                .WithErrorCode("BR-047");
        });

        // BR-048: EZERT8 != '00000000' indicates validation failure
        When(x => x.ExternalValidationResponse != null, () =>
        {
            RuleFor(x => x.ExternalValidationResponse!.Ezert8)
                .Must(code => code == "00000000")
                .When(x => x.ExternalValidationResponse?.IsSuccess == true)
                .WithMessage("Código de resposta EZERT8 deve ser '00000000' para validação bem-sucedida")
                .WithErrorCode("BR-048");
        });

        // BR-049: Validation error response contains descriptive message
        When(x => x.ExternalValidationResponse != null && !x.ExternalValidationResponse.IsSuccess, () =>
        {
            RuleFor(x => x.ExternalValidationResponse!.ErrorMessage)
                .NotEmpty()
                .WithMessage("Mensagem de erro descritiva é obrigatória quando validação falha")
                .WithErrorCode("BR-049");
        });

        // BR-050: Payment authorization halted if validation fails
        // This is a workflow rule enforced in the payment authorization service

        // BR-051: Transaction rolled back if validation fails
        // This is a transaction management rule enforced in the transaction coordinator

        // BR-052-056: Error codes (handled in ErrorMessages.pt-BR.resx)
        // CONS-001: Consortium validation error
        // CONS-002: Contract cancelled
        // CONS-003: Group closed
        // CONS-004: Quota not contemplated
        // CONS-005: Beneficiary not authorized

        // BR-057: Claim accompaniment record created with COD_EVENTO = 1098
        // This is validated in PhaseManagementRules

        // BR-058: Phase changes determined by SI_REL_FASE_EVENTO config
        // This is validated in PhaseManagementRules

        // BR-059: Phase opening (IND='1'): Create with DATA_FECHA='9999-12-31'
        // This is validated in PhaseManagementRules

        // BR-060: Phase closing (IND='2'): Update existing open phase
        // This is validated in PhaseManagementRules
    }

    /// <summary>
    /// Determines if a product code is a consortium product
    /// </summary>
    public static bool IsConsortiumProduct(int productCode)
    {
        return ConsortiumProducts.Contains(productCode);
    }

    /// <summary>
    /// Determines the correct routing service based on product and contract
    /// </summary>
    public static string DetermineRoutingService(int? productCode, int? contractNumber)
    {
        // BR-043: Consortium products → CNOUA
        if (productCode.HasValue && IsConsortiumProduct(productCode.Value))
            return "CNOUA";

        // BR-045: EFP contract (NUM_CONTRATO > 0) → SIPUA
        if (contractNumber.HasValue && contractNumber > 0)
            return "SIPUA";

        // BR-046: HB contract (NUM_CONTRATO = 0 or not found) → SIMDA
        return "SIMDA";
    }
}

/// <summary>
/// Context for product validation
/// Contains all data needed to validate product validation rules
/// </summary>
public class ProductValidationContext
{
    /// <summary>
    /// Product code (CODPRODU)
    /// </summary>
    public int? ProductCode { get; set; }

    /// <summary>
    /// Contract number from EF_CONTR_SEG_HABIT
    /// </summary>
    public int? ContractNumber { get; set; }

    /// <summary>
    /// Target routing service (CNOUA, SIPUA, SIMDA)
    /// </summary>
    public string? RoutingService { get; set; }

    /// <summary>
    /// External validation response
    /// </summary>
    public ExternalValidationResponse? ExternalValidationResponse { get; set; }
}

/// <summary>
/// Error codes for product validation (BR-052 to BR-056)
/// </summary>
public static class ProductValidationErrorCodes
{
    // BR-052
    public const string ConsortiumValidationError = "CONS-001";

    // BR-053
    public const string ContractCancelled = "CONS-002";

    // BR-054
    public const string GroupClosed = "CONS-003";

    // BR-055
    public const string QuotaNotContemplated = "CONS-004";

    // BR-056
    public const string BeneficiaryNotAuthorized = "CONS-005";
}
