using FluentValidation;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.DTOs;

namespace CaixaSeguradora.Core.Validators.BusinessRules;

/// <summary>
/// Data Validation Rules (BR-075 to BR-087)
/// Enforces business rules for data integrity and field validation
/// </summary>
public class DataValidationRules : AbstractValidator<ClaimMaster>
{
    public DataValidationRules()
    {
        // BR-075: TIPSEG numeric and consistent across records
        RuleFor(x => x.Tipseg)
            .GreaterThan(0)
            .WithMessage("TIPSEG deve ser numérico e maior que 0")
            .WithErrorCode("BR-075");

        // BR-076: ORGSIN 2-digit code (01-99)
        RuleFor(x => x.Orgsin)
            .InclusiveBetween(1, 99)
            .WithMessage("ORGSIN deve ser código de 2 dígitos (01-99)")
            .WithErrorCode("BR-076");

        // BR-077: RMOSIN 2-digit code (00-99)
        RuleFor(x => x.Rmosin)
            .InclusiveBetween(0, 99)
            .WithMessage("RMOSIN deve ser código de 2 dígitos (00-99)")
            .WithErrorCode("BR-077");

        // BR-078: NUMSIN 1-6 digit claim number
        RuleFor(x => x.Numsin)
            .InclusiveBetween(1, 999999)
            .WithMessage("NUMSIN deve ser número de sinistro de 1-6 dígitos")
            .WithErrorCode("BR-078");

        // BR-079: FONTE numeric
        RuleFor(x => x.Fonte)
            .GreaterThanOrEqualTo(0)
            .WithMessage("FONTE deve ser numérico")
            .WithErrorCode("BR-079");

        // BR-080: PROTSINI numeric
        RuleFor(x => x.Protsini)
            .GreaterThanOrEqualTo(0)
            .WithMessage("PROTSINI deve ser numérico")
            .WithErrorCode("BR-080");

        // BR-081: DAC single digit (0-9)
        RuleFor(x => x.Dac)
            .InclusiveBetween(0, 9)
            .WithMessage("DAC deve ser dígito único (0-9)")
            .WithErrorCode("BR-081");

        // BR-082: CODPRODU numeric and valid product code
        RuleFor(x => x.Codprodu)
            .GreaterThan(0)
            .WithMessage("CODPRODU deve ser numérico e código de produto válido")
            .WithErrorCode("BR-082");

        // BR-083: SDOPAG (reserve) >= 0
        RuleFor(x => x.Sdopag)
            .GreaterThanOrEqualTo(0)
            .WithMessage("SDOPAG (reserva) deve ser >= 0")
            .WithErrorCode("BR-083");

        // BR-084: TOTPAG (payments) >= 0 and <= SDOPAG
        RuleFor(x => x.Totpag)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TOTPAG (pagamentos) deve ser >= 0")
            .WithErrorCode("BR-084");

        RuleFor(x => x.Totpag)
            .LessThanOrEqualTo(x => x.Sdopag)
            .WithMessage("TOTPAG (pagamentos) deve ser <= SDOPAG (reserva)")
            .WithErrorCode("BR-084");

        // BR-085: OCORHIST non-negative integer
        RuleFor(x => x.Ocorhist)
            .GreaterThanOrEqualTo(0)
            .WithMessage("OCORHIST deve ser inteiro não negativo")
            .WithErrorCode("BR-085");

        // BR-086: VALPRI >= 0 and <= SDOPAG - TOTPAG (validated in payment context)
        // This is validated in PaymentValidationRules with full context

        // BR-087: CRRMON >= 0 (validated in payment context)
        // This is validated in PaymentValidationRules
    }
}

/// <summary>
/// Data validation rules for payment amounts
/// </summary>
public class PaymentAmountValidationRules : AbstractValidator<PaymentAmountContext>
{
    public PaymentAmountValidationRules()
    {
        // BR-086: VALPRI >= 0 and <= SDOPAG - TOTPAG
        RuleFor(x => x.PrincipalAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("VALPRI (valor principal) deve ser >= 0")
            .WithErrorCode("BR-086");

        When(x => x.Reserve.HasValue && x.TotalPaid.HasValue, () =>
        {
            RuleFor(x => x.PrincipalAmount)
                .Must((ctx, amount) =>
                {
                    var pending = ctx.Reserve!.Value - ctx.TotalPaid!.Value;
                    return amount <= pending;
                })
                .WithMessage(ctx =>
                {
                    var pending = ctx.Reserve!.Value - ctx.TotalPaid!.Value;
                    return $"VALPRI deve ser <= saldo pendente (SDOPAG - TOTPAG = {pending:N2})";
                })
                .WithErrorCode("BR-086");
        });

        // BR-087: CRRMON >= 0
        RuleFor(x => x.CorrectionAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("CRRMON (correção) deve ser >= 0")
            .WithErrorCode("BR-087");
    }
}

/// <summary>
/// Context for payment amount validation
/// </summary>
public class PaymentAmountContext
{
    /// <summary>
    /// Principal amount (VALPRI)
    /// </summary>
    public decimal PrincipalAmount { get; set; }

    /// <summary>
    /// Correction amount (CRRMON)
    /// </summary>
    public decimal CorrectionAmount { get; set; }

    /// <summary>
    /// Reserve amount (SDOPAG)
    /// </summary>
    public decimal? Reserve { get; set; }

    /// <summary>
    /// Total paid (TOTPAG)
    /// </summary>
    public decimal? TotalPaid { get; set; }
}

/// <summary>
/// Data validation rules for protocol components
/// </summary>
public class ProtocolValidationRules : AbstractValidator<ProtocolContext>
{
    public ProtocolValidationRules()
    {
        // BR-079: FONTE numeric
        RuleFor(x => x.Fonte)
            .GreaterThanOrEqualTo(0)
            .WithMessage("FONTE deve ser numérico e >= 0")
            .WithErrorCode("BR-079");

        // BR-080: PROTSINI numeric
        RuleFor(x => x.Protsini)
            .GreaterThanOrEqualTo(0)
            .WithMessage("PROTSINI deve ser numérico e >= 0")
            .WithErrorCode("BR-080");

        // BR-081: DAC single digit (0-9)
        RuleFor(x => x.Dac)
            .InclusiveBetween(0, 9)
            .WithMessage("DAC deve ser dígito único (0-9)")
            .WithErrorCode("BR-081");
    }
}

/// <summary>
/// Context for protocol validation
/// </summary>
public class ProtocolContext
{
    /// <summary>
    /// Protocol source (FONTE)
    /// </summary>
    public int Fonte { get; set; }

    /// <summary>
    /// Protocol number (PROTSINI)
    /// </summary>
    public int Protsini { get; set; }

    /// <summary>
    /// Check digit (DAC)
    /// </summary>
    public int Dac { get; set; }

    /// <summary>
    /// Formats protocol as FONTE/PROTSINI-DAC
    /// </summary>
    public string Format()
    {
        return $"{Fonte}/{Protsini}-{Dac}";
    }
}

/// <summary>
/// Data validation rules for claim number components
/// </summary>
public class ClaimNumberValidationRules : AbstractValidator<ClaimNumberContext>
{
    public ClaimNumberValidationRules()
    {
        // BR-075: TIPSEG numeric and consistent
        RuleFor(x => x.Tipseg)
            .GreaterThan(0)
            .WithMessage("TIPSEG deve ser numérico e maior que 0")
            .WithErrorCode("BR-075");

        // BR-076: ORGSIN 2-digit code (01-99)
        RuleFor(x => x.Orgsin)
            .InclusiveBetween(1, 99)
            .WithMessage("ORGSIN deve ser código de 2 dígitos (01-99)")
            .WithErrorCode("BR-076");

        // BR-077: RMOSIN 2-digit code (00-99)
        RuleFor(x => x.Rmosin)
            .InclusiveBetween(0, 99)
            .WithMessage("RMOSIN deve ser código de 2 dígitos (00-99)")
            .WithErrorCode("BR-077");

        // BR-078: NUMSIN 1-6 digit claim number
        RuleFor(x => x.Numsin)
            .InclusiveBetween(1, 999999)
            .WithMessage("NUMSIN deve ser número de sinistro de 1-6 dígitos")
            .WithErrorCode("BR-078");
    }
}

/// <summary>
/// Context for claim number validation
/// </summary>
public class ClaimNumberContext
{
    /// <summary>
    /// Insurance type (TIPSEG)
    /// </summary>
    public int Tipseg { get; set; }

    /// <summary>
    /// Claim origin (ORGSIN)
    /// </summary>
    public int Orgsin { get; set; }

    /// <summary>
    /// Claim branch (RMOSIN)
    /// </summary>
    public int Rmosin { get; set; }

    /// <summary>
    /// Claim number (NUMSIN)
    /// </summary>
    public int Numsin { get; set; }

    /// <summary>
    /// Formats claim number as TIPSEG/ORGSIN/RMOSIN/NUMSIN
    /// </summary>
    public string Format()
    {
        return $"{Tipseg}/{Orgsin:D2}/{Rmosin:D2}/{Numsin}";
    }
}
