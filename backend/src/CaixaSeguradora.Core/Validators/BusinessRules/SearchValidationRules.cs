using FluentValidation;
using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Validators.BusinessRules;

/// <summary>
/// Search Validation Rules (BR-001 to BR-009)
/// Enforces business rules for claim search functionality
/// </summary>
public class SearchValidationRules : AbstractValidator<ClaimSearchCriteria>
{
    public SearchValidationRules()
    {
        // BR-001: Three mutually exclusive search modes (Protocol/Claim/Leader)
        RuleFor(x => x)
            .Must(HasOnlyOneSearchMode)
            .WithMessage("Apenas um modo de busca deve ser fornecido (Protocolo, Sinistro ou Líder)")
            .WithErrorCode("BR-001");

        // BR-002: At least one complete search criterion required
        RuleFor(x => x)
            .Must(x => x.IsValid())
            .WithMessage("Critérios de busca inválidos. Forneça protocolo completo (FONTE/PROTSINI-DAC), número do sinistro completo (ORGSIN/RMOSIN/NUMSIN) ou código do líder completo (CODLIDER/SINLID)")
            .WithErrorCode("BR-002");

        // BR-004: Protocol displayed as FONTE/PROTSINI-DAC (validation for completeness)
        When(x => x.Fonte.HasValue || x.Protsini.HasValue || x.Dac.HasValue, () =>
        {
            RuleFor(x => x.Fonte)
                .NotNull()
                .WithMessage("FONTE é obrigatório quando buscando por protocolo")
                .GreaterThan(0)
                .WithMessage("FONTE deve ser maior que 0")
                .WithErrorCode("BR-004");

            RuleFor(x => x.Protsini)
                .NotNull()
                .WithMessage("PROTSINI é obrigatório quando buscando por protocolo")
                .GreaterThan(0)
                .WithMessage("PROTSINI deve ser maior que 0")
                .WithErrorCode("BR-004");

            RuleFor(x => x.Dac)
                .NotNull()
                .WithMessage("DAC é obrigatório quando buscando por protocolo")
                .InclusiveBetween(0, 9)
                .WithMessage("DAC deve estar entre 0 e 9")
                .WithErrorCode("BR-004");
        });

        // BR-005: Claim displayed as ORGSIN/RMOSIN/NUMSIN (validation for completeness)
        When(x => x.Orgsin.HasValue || x.Rmosin.HasValue || x.Numsin.HasValue, () =>
        {
            RuleFor(x => x.Orgsin)
                .NotNull()
                .WithMessage("ORGSIN é obrigatório quando buscando por número de sinistro")
                .InclusiveBetween(1, 99)
                .WithMessage("ORGSIN deve estar entre 01 e 99")
                .WithErrorCode("BR-005");

            RuleFor(x => x.Rmosin)
                .NotNull()
                .WithMessage("RMOSIN é obrigatório quando buscando por número de sinistro")
                .InclusiveBetween(0, 99)
                .WithMessage("RMOSIN deve estar entre 00 e 99")
                .WithErrorCode("BR-005");

            RuleFor(x => x.Numsin)
                .NotNull()
                .WithMessage("NUMSIN é obrigatório quando buscando por número de sinistro")
                .InclusiveBetween(1, 999999)
                .WithMessage("NUMSIN deve estar entre 1 e 999999 (1-6 dígitos)")
                .WithErrorCode("BR-005");
        });

        // BR-005 (Leader Code): Leader code validation for completeness
        When(x => x.Codlider.HasValue || x.Sinlid.HasValue, () =>
        {
            RuleFor(x => x.Codlider)
                .NotNull()
                .WithMessage("CODLIDER é obrigatório quando buscando por código do líder")
                .GreaterThan(0)
                .WithMessage("CODLIDER deve ser maior que 0")
                .WithErrorCode("BR-005");

            RuleFor(x => x.Sinlid)
                .NotNull()
                .WithMessage("SINLID é obrigatório quando buscando por código do líder")
                .GreaterThan(0)
                .WithMessage("SINLID deve ser maior que 0")
                .WithErrorCode("BR-005");
        });

        // BR-009: Currency amounts formatted with commas, 2 decimals (validation not enforcement - enforced in UI)
        // This rule is primarily UI-focused, but we validate that numeric fields are properly bounded
    }

    /// <summary>
    /// BR-001: Validates that only one search mode is active at a time
    /// </summary>
    private bool HasOnlyOneSearchMode(ClaimSearchCriteria criteria)
    {
        bool hasProtocol = criteria.Fonte.HasValue || criteria.Protsini.HasValue || criteria.Dac.HasValue;
        bool hasClaimNumber = criteria.Orgsin.HasValue || criteria.Rmosin.HasValue || criteria.Numsin.HasValue;
        bool hasLeaderCode = criteria.Codlider.HasValue || criteria.Sinlid.HasValue;

        int activeModesCount = 0;
        if (hasProtocol) activeModesCount++;
        if (hasClaimNumber) activeModesCount++;
        if (hasLeaderCode) activeModesCount++;

        return activeModesCount <= 1;
    }
}

/// <summary>
/// Error messages for Search Validation (BR-006)
/// These are handled in ErrorMessages.pt-BR.resx
/// VAL-001: "Protocolo {0}-{1} NAO ENCONTRADO"
/// VAL-002: "SINISTRO {0}/{1}/{2} NAO ENCONTRADO"
/// VAL-003: "LIDER {0}-{1} NAO ENCONTRADO"
/// </summary>
public static class SearchErrorCodes
{
    public const string ProtocolNotFound = "VAL-001";
    public const string ClaimNotFound = "VAL-002";
    public const string LeaderNotFound = "VAL-003";
}
