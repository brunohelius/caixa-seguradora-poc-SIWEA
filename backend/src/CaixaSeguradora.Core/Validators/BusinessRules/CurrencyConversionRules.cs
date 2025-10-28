using FluentValidation;
using CaixaSeguradora.Core.ValueObjects;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Core.Validators.BusinessRules;

/// <summary>
/// Currency Conversion Rules (BR-027 to BR-037)
/// Enforces business rules for currency conversion calculations
/// </summary>
public class CurrencyConversionRules : AbstractValidator<CurrencyConversionContext>
{
    public CurrencyConversionRules()
    {
        // BR-027: Conversion rate precision: 8 decimal places
        RuleFor(x => x.ConversionRate)
            .Must(rate => rate == null || CountDecimalPlaces(rate.Value) <= 8)
            .WithMessage("Taxa de conversão deve ter no máximo 8 casas decimais")
            .WithErrorCode("BR-027");

        // BR-028: Final currency calculations: 2 decimal places
        RuleFor(x => x.PrincipalAmountBtnf)
            .Must(amount => amount == null || CountDecimalPlaces(amount.Value) <= 2)
            .WithMessage("Valor principal convertido (VALPRIBT) deve ter no máximo 2 casas decimais")
            .WithErrorCode("BR-028");

        RuleFor(x => x.CorrectionAmountBtnf)
            .Must(amount => amount == null || CountDecimalPlaces(amount.Value) <= 2)
            .WithMessage("Valor de correção convertido (CRRMONBT) deve ter no máximo 2 casas decimais")
            .WithErrorCode("BR-028");

        RuleFor(x => x.TotalAmountBtnf)
            .Must(amount => amount == null || CountDecimalPlaces(amount.Value) <= 2)
            .WithMessage("Valor total convertido (VALTOTBT) deve ter no máximo 2 casas decimais")
            .WithErrorCode("BR-028");

        // BR-029: Principal conversion: VALPRIBT = VALPRI × VLCRUZAD
        When(x => x.PrincipalAmount.HasValue && x.ConversionRate.HasValue && x.PrincipalAmountBtnf.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(ctx =>
                {
                    var expected = Math.Round(ctx.PrincipalAmount!.Value * ctx.ConversionRate!.Value, 2);
                    var actual = Math.Round(ctx.PrincipalAmountBtnf!.Value, 2);
                    return Math.Abs(expected - actual) < 0.01m; // Allow for rounding tolerance
                })
                .WithMessage("Valor principal convertido (VALPRIBT) deve ser igual a VALPRI × VLCRUZAD")
                .WithErrorCode("BR-029");
        });

        // BR-030: Correction conversion: CRRMONBT = CRRMON × VLCRUZAD
        When(x => x.CorrectionAmount.HasValue && x.ConversionRate.HasValue && x.CorrectionAmountBtnf.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(ctx =>
                {
                    var expected = Math.Round(ctx.CorrectionAmount!.Value * ctx.ConversionRate!.Value, 2);
                    var actual = Math.Round(ctx.CorrectionAmountBtnf!.Value, 2);
                    return Math.Abs(expected - actual) < 0.01m; // Allow for rounding tolerance
                })
                .WithMessage("Valor de correção convertido (CRRMONBT) deve ser igual a CRRMON × VLCRUZAD")
                .WithErrorCode("BR-030");
        });

        // BR-031: Total calculation: VALTOTBT = VALPRIBT + CRRMONBT
        When(x => x.PrincipalAmountBtnf.HasValue && x.CorrectionAmountBtnf.HasValue && x.TotalAmountBtnf.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(ctx =>
                {
                    var expected = Math.Round(ctx.PrincipalAmountBtnf!.Value + ctx.CorrectionAmountBtnf!.Value, 2);
                    var actual = Math.Round(ctx.TotalAmountBtnf!.Value, 2);
                    return Math.Abs(expected - actual) < 0.01m; // Allow for rounding tolerance
                })
                .WithMessage("Valor total convertido (VALTOTBT) deve ser igual a VALPRIBT + CRRMONBT")
                .WithErrorCode("BR-031");
        });

        // BR-032: Daily amounts calculated (PRIDIABT, CRRDIABT, TOTDIABT)
        // This is an implementation rule - daily amounts are calculated if needed

        // BR-033: Currency code always BTNF
        RuleFor(x => x.TargetCurrency)
            .Equal("BTNF")
            .When(x => !string.IsNullOrEmpty(x.TargetCurrency))
            .WithMessage("Código da moeda deve ser sempre BTNF")
            .WithErrorCode("BR-033");

        // BR-034: Operation code always 1098
        RuleFor(x => x.OperationCode)
            .Equal(1098)
            .When(x => x.OperationCode.HasValue)
            .WithMessage("Código de operação deve ser sempre 1098")
            .WithErrorCode("BR-034");

        // BR-035: Transaction date = TSISTEMA.DTMOVABE (business date)
        // This is validated in transaction recording - business date comes from SystemControl

        // BR-036: Transaction time = current system time
        // This is an implementation rule - time is recorded automatically

        // BR-037: Accounting status initialized to '0'
        RuleFor(x => x.AccountingStatus)
            .Equal('0')
            .When(x => x.AccountingStatus.HasValue)
            .WithMessage("Status contábil deve ser inicializado como '0'")
            .WithErrorCode("BR-037");
    }

    /// <summary>
    /// Counts decimal places in a decimal number
    /// </summary>
    private int CountDecimalPlaces(decimal value)
    {
        var valueString = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var decimalIndex = valueString.IndexOf('.');

        if (decimalIndex == -1)
            return 0;

        return valueString.Length - decimalIndex - 1;
    }
}

/// <summary>
/// Context for currency conversion validation
/// Contains all data needed to validate conversion rules
/// </summary>
public class CurrencyConversionContext
{
    /// <summary>
    /// Original principal amount
    /// </summary>
    public decimal? PrincipalAmount { get; set; }

    /// <summary>
    /// Original correction amount
    /// </summary>
    public decimal? CorrectionAmount { get; set; }

    /// <summary>
    /// Conversion rate (VLCRUZAD) - 8 decimal places
    /// </summary>
    public decimal? ConversionRate { get; set; }

    /// <summary>
    /// Converted principal amount (VALPRIBT) - 2 decimal places
    /// </summary>
    public decimal? PrincipalAmountBtnf { get; set; }

    /// <summary>
    /// Converted correction amount (CRRMONBT) - 2 decimal places
    /// </summary>
    public decimal? CorrectionAmountBtnf { get; set; }

    /// <summary>
    /// Converted total amount (VALTOTBT) - 2 decimal places
    /// </summary>
    public decimal? TotalAmountBtnf { get; set; }

    /// <summary>
    /// Target currency code (should be "BTNF")
    /// </summary>
    public string? TargetCurrency { get; set; }

    /// <summary>
    /// Operation code (should be 1098)
    /// </summary>
    public int? OperationCode { get; set; }

    /// <summary>
    /// Accounting status (should be '0' initially)
    /// </summary>
    public char? AccountingStatus { get; set; }

    /// <summary>
    /// Business date from TSISTEMA.DTMOVABE
    /// </summary>
    public DateTime? BusinessDate { get; set; }

    /// <summary>
    /// Rate validity start date
    /// </summary>
    public DateTime? RateStartDate { get; set; }

    /// <summary>
    /// Rate validity end date
    /// </summary>
    public DateTime? RateEndDate { get; set; }
}

/// <summary>
/// Validator for CurrencyUnit entity to ensure rate precision
/// </summary>
public class CurrencyUnitRules : AbstractValidator<CurrencyUnit>
{
    public CurrencyUnitRules()
    {
        // BR-027: Conversion rate precision: 8 decimal places
        RuleFor(x => x.Vlcruzad)
            .Must(rate => CountDecimalPlaces(rate) <= 8)
            .WithMessage("Taxa de conversão (VLCRUZAD) deve ter no máximo 8 casas decimais")
            .WithErrorCode("BR-027");

        // BR-025: Rate selection: DTINIVIG <= DATE <= DTTERVIG
        RuleFor(x => x.Dtinivig)
            .LessThanOrEqualTo(x => x.Dttervig)
            .WithMessage("Data inicial de vigência (DTINIVIG) deve ser anterior ou igual à data final (DTTERVIG)")
            .WithErrorCode("BR-025");
    }

    private int CountDecimalPlaces(decimal value)
    {
        var valueString = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var decimalIndex = valueString.IndexOf('.');

        if (decimalIndex == -1)
            return 0;

        return valueString.Length - decimalIndex - 1;
    }
}
