using FluentValidation;

namespace CaixaSeguradora.Core.Validators.BusinessRules;

/// <summary>
/// UI Display Rules (BR-088 to BR-095)
/// Enforces business rules for UI presentation and formatting
/// Note: Most UI rules are enforced in the frontend, these validators ensure backend compliance
/// </summary>
public class UiDisplayRules : AbstractValidator<UiDisplayContext>
{
    public UiDisplayRules()
    {
        // BR-088: All UI text in Portuguese
        // This is primarily a frontend concern, validated through localization resources

        // BR-089: Numeric amounts formatted ###,###,###.##
        // This is a display rule enforced in the frontend, but we validate the data precision
        RuleFor(x => x.Amount)
            .Must(amount => !amount.HasValue || CountDecimalPlaces(amount.Value) <= 2)
            .WithMessage("Valores numéricos devem ter no máximo 2 casas decimais para formatação (###,###,###.##)")
            .WithErrorCode("BR-089");

        // BR-090: Date format: DD/MM/YYYY display, YYYY-MM-DD storage
        // This is a display rule - dates stored as DateTime in database, formatted in frontend
        When(x => x.Date.HasValue, () =>
        {
            RuleFor(x => x.Date)
                .Must(date => date!.Value.Year >= 1900 && date.Value.Year <= 9999)
                .WithMessage("Data deve estar em formato válido (ano entre 1900 e 9999)")
                .WithErrorCode("BR-090");
        });

        // BR-091: Time format: HH:MM:SS
        // This is a display rule - time stored as DateTime or TimeSpan, formatted in frontend
        When(x => x.Time.HasValue, () =>
        {
            RuleFor(x => x.Time)
                .Must(time => time!.Value.Hours >= 0 && time.Value.Hours <= 23 &&
                             time.Value.Minutes >= 0 && time.Value.Minutes <= 59 &&
                             time.Value.Seconds >= 0 && time.Value.Seconds <= 59)
                .WithMessage("Hora deve estar em formato válido (HH:MM:SS)")
                .WithErrorCode("BR-091");
        });

        // BR-092: Error messages displayed in red (#e80c4d)
        // This is a frontend CSS concern - validated through E2E tests

        // BR-093: Caixa Seguradora logo in header
        // This is a frontend component concern - validated through E2E tests

        // BR-094: Site.css stylesheet applied without modification
        // This is a frontend asset concern - validated through E2E tests

        // BR-095: Responsive design supporting mobile (max-width: 850px)
        // This is a frontend CSS/responsive design concern - validated through E2E tests
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
/// Context for UI display validation
/// Contains data that affects UI presentation
/// </summary>
public class UiDisplayContext
{
    /// <summary>
    /// Numeric amount to display
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Date to display
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Time to display
    /// </summary>
    public TimeSpan? Time { get; set; }

    /// <summary>
    /// Error message text (should be in Portuguese)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether this is for mobile display (max-width: 850px)
    /// </summary>
    public bool IsMobile { get; set; }
}

/// <summary>
/// Formatting utilities for UI display rules
/// These enforce the formatting standards defined in BR-089 to BR-091
/// </summary>
public static class UiFormatters
{
    /// <summary>
    /// BR-089: Formats currency amount as ###,###,###.##
    /// </summary>
    public static string FormatCurrency(decimal amount)
    {
        return amount.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));
    }

    /// <summary>
    /// BR-090: Formats date as DD/MM/YYYY for display
    /// </summary>
    public static string FormatDate(DateTime date)
    {
        return date.ToString("dd/MM/yyyy", new System.Globalization.CultureInfo("pt-BR"));
    }

    /// <summary>
    /// BR-090: Formats date as YYYY-MM-DD for storage/API
    /// </summary>
    public static string FormatDateStorage(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// BR-091: Formats time as HH:MM:SS
    /// </summary>
    public static string FormatTime(TimeSpan time)
    {
        return time.ToString(@"hh\:mm\:ss");
    }

    /// <summary>
    /// BR-091: Formats time as HH:MM:SS from DateTime
    /// </summary>
    public static string FormatTime(DateTime dateTime)
    {
        return dateTime.ToString("HH:mm:ss");
    }

    /// <summary>
    /// Validates Portuguese language text
    /// </summary>
    public static bool IsPortugueseText(string text)
    {
        // Basic validation - text should not contain obvious English-only words
        // More sophisticated validation would use NLP or dictionary lookup
        if (string.IsNullOrWhiteSpace(text))
            return true;

        // Check for common English error message patterns that should be in Portuguese
        var englishPatterns = new[]
        {
            "required", "invalid", "error", "failed", "success",
            "not found", "unauthorized", "forbidden", "timeout"
        };

        var lowerText = text.ToLowerInvariant();
        return !englishPatterns.Any(pattern => lowerText.Contains(pattern));
    }

    /// <summary>
    /// BR-004: Formats protocol as FONTE/PROTSINI-DAC
    /// </summary>
    public static string FormatProtocol(int fonte, int protsini, int dac)
    {
        return $"{fonte}/{protsini}-{dac}";
    }

    /// <summary>
    /// BR-005: Formats claim number as ORGSIN/RMOSIN/NUMSIN
    /// </summary>
    public static string FormatClaimNumber(int orgsin, int rmosin, int numsin)
    {
        return $"{orgsin:D2}/{rmosin:D2}/{numsin}";
    }
}

/// <summary>
/// Constants for UI display
/// </summary>
public static class UiDisplayConstants
{
    /// <summary>
    /// BR-092: Error message color
    /// </summary>
    public const string ErrorColor = "#e80c4d";

    /// <summary>
    /// BR-095: Mobile breakpoint
    /// </summary>
    public const int MobileMaxWidth = 850;

    /// <summary>
    /// BR-094: Required CSS file
    /// </summary>
    public const string RequiredStylesheet = "Site.css";
}
