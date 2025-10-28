namespace CaixaSeguradora.Core.ValueObjects;

/// <summary>
/// Immutable value object representing monetary amount with currency and precision
/// Validation Rules:
/// - Amount >= 0
/// - Currency must equal "BTNF" for all converted amounts
/// - Precision ∈ {2, 8} (2 for final amounts, 8 for conversion rates)
///
/// Usage Example:
/// var vlcruzad = new CurrencyAmount(1.23456789m, "BTNF", 8);  // Conversion rate
/// var valpri = new CurrencyAmount(1000.00m, "BRL", 2);         // Original amount
/// var valpribt = valpri.Multiply(vlcruzad.Amount)
///                      .WithCurrency("BTNF")
///                      .WithPrecision(2);  // Result: 1234.57 BTNF
/// </summary>
public class CurrencyAmount : IEquatable<CurrencyAmount>
{
    /// <summary>
    /// Monetary value
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Currency code (always "BTNF" for standardized amounts)
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Decimal places (2 for display amounts, 8 for conversion rates)
    /// </summary>
    public int Precision { get; }

    /// <summary>
    /// Constructor with validation
    /// </summary>
    public CurrencyAmount(decimal amount, string currency, int precision)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be >= 0", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        if (precision != 2 && precision != 8)
            throw new ArgumentException("Precision must be 2 or 8", nameof(precision));

        Amount = Math.Round(amount, precision);
        Currency = currency.ToUpperInvariant();
        Precision = precision;
    }

    /// <summary>
    /// Returns new CurrencyAmount with amount × rate
    /// </summary>
    public CurrencyAmount Multiply(decimal rate)
    {
        return new CurrencyAmount(Amount * rate, Currency, Precision);
    }

    /// <summary>
    /// Returns new CurrencyAmount with sum (validates same currency)
    /// </summary>
    public CurrencyAmount Add(CurrencyAmount other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add amounts with different currencies: {Currency} and {other.Currency}");

        var precision = Math.Max(Precision, other.Precision);
        return new CurrencyAmount(Amount + other.Amount, Currency, precision);
    }

    /// <summary>
    /// Returns new CurrencyAmount with different currency
    /// </summary>
    public CurrencyAmount WithCurrency(string newCurrency)
    {
        return new CurrencyAmount(Amount, newCurrency, Precision);
    }

    /// <summary>
    /// Returns new CurrencyAmount with different precision
    /// </summary>
    public CurrencyAmount WithPrecision(int newPrecision)
    {
        return new CurrencyAmount(Amount, Currency, newPrecision);
    }

    /// <summary>
    /// Returns formatted "###,###,###.##" with thousands separator
    /// </summary>
    public override string ToString()
    {
        return Amount.ToString($"N{Precision}");
    }

    /// <summary>
    /// Returns formatted with currency code "1,234.57 BTNF"
    /// </summary>
    public string ToStringWithCurrency()
    {
        return $"{ToString()} {Currency}";
    }

    public bool Equals(CurrencyAmount? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Amount == other.Amount
            && Currency == other.Currency
            && Precision == other.Precision;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as CurrencyAmount);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency, Precision);
    }

    public static bool operator ==(CurrencyAmount? left, CurrencyAmount? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(CurrencyAmount? left, CurrencyAmount? right)
    {
        return !Equals(left, right);
    }
}
