using CaixaSeguradora.Core.DTOs;
using FluentValidation;

namespace CaixaSeguradora.Core.Validators;

/// <summary>
/// Validator for ClaimSearchCriteria
/// Ensures at least one complete search criteria set is provided
/// </summary>
public class ClaimSearchValidator : AbstractValidator<ClaimSearchCriteria>
{
    public ClaimSearchValidator()
    {
        // At least one complete criteria set must be provided
        RuleFor(x => x)
            .Must(HaveValidCriteria)
            .WithMessage("Você deve fornecer ao menos um critério de busca completo: " +
                        "(Fonte, Protsini e DAC) OU " +
                        "(Orgsin, Rmosin e Numsin) OU " +
                        "(Codlider e Sinlid)");

        // Protocol criteria validation
        When(x => x.Fonte.HasValue || x.Protsini.HasValue || x.Dac.HasValue, () =>
        {
            RuleFor(x => x.Fonte)
                .GreaterThan(0)
                .When(x => x.Fonte.HasValue)
                .WithMessage("Fonte deve ser maior que zero");

            RuleFor(x => x.Protsini)
                .GreaterThan(0)
                .When(x => x.Protsini.HasValue)
                .WithMessage("Número do protocolo deve ser maior que zero");

            RuleFor(x => x.Dac)
                .InclusiveBetween(0, 9)
                .When(x => x.Dac.HasValue)
                .WithMessage("DAC deve estar entre 0 e 9");

            // All protocol fields must be provided together
            RuleFor(x => x)
                .Must(x => x.Fonte.HasValue && x.Protsini.HasValue && x.Dac.HasValue)
                .When(x => x.Fonte.HasValue || x.Protsini.HasValue || x.Dac.HasValue)
                .WithMessage("Para busca por protocolo, todos os campos (Fonte, Protsini e DAC) devem ser fornecidos");
        });

        // Claim number criteria validation
        When(x => x.Orgsin.HasValue || x.Rmosin.HasValue || x.Numsin.HasValue, () =>
        {
            RuleFor(x => x.Orgsin)
                .GreaterThan(0)
                .When(x => x.Orgsin.HasValue)
                .WithMessage("Órgão do sinistro deve ser maior que zero");

            RuleFor(x => x.Rmosin)
                .GreaterThan(0)
                .When(x => x.Rmosin.HasValue)
                .WithMessage("Ramo do sinistro deve ser maior que zero");

            RuleFor(x => x.Numsin)
                .GreaterThan(0)
                .When(x => x.Numsin.HasValue)
                .WithMessage("Número do sinistro deve ser maior que zero");

            // All claim number fields must be provided together
            RuleFor(x => x)
                .Must(x => x.Orgsin.HasValue && x.Rmosin.HasValue && x.Numsin.HasValue)
                .When(x => x.Orgsin.HasValue || x.Rmosin.HasValue || x.Numsin.HasValue)
                .WithMessage("Para busca por número de sinistro, todos os campos (Orgsin, Rmosin e Numsin) devem ser fornecidos");
        });

        // Leader code criteria validation
        When(x => x.Codlider.HasValue || x.Sinlid.HasValue, () =>
        {
            RuleFor(x => x.Codlider)
                .GreaterThan(0)
                .When(x => x.Codlider.HasValue)
                .WithMessage("Código do líder deve ser maior que zero");

            RuleFor(x => x.Sinlid)
                .GreaterThan(0)
                .When(x => x.Sinlid.HasValue)
                .WithMessage("Sinistro do líder deve ser maior que zero");

            // Both leader fields must be provided together
            RuleFor(x => x)
                .Must(x => x.Codlider.HasValue && x.Sinlid.HasValue)
                .When(x => x.Codlider.HasValue || x.Sinlid.HasValue)
                .WithMessage("Para busca por código de líder, ambos os campos (Codlider e Sinlid) devem ser fornecidos");
        });
    }

    private bool HaveValidCriteria(ClaimSearchCriteria criteria)
    {
        return criteria.IsValid();
    }
}
