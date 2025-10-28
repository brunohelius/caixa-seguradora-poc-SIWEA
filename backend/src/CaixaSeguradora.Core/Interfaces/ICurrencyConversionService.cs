namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// Service for currency conversion operations
/// </summary>
public interface ICurrencyConversionService
{
    /// <summary>
    /// Converts amount from source currency to target currency
    /// </summary>
    /// <param name="amount">Amount to convert</param>
    /// <param name="sourceCurrency">Source currency code (ISO 4217)</param>
    /// <param name="targetCurrency">Target currency code (ISO 4217)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversion result with converted amount and exchange rate</returns>
    Task<CurrencyConversionResult> ConvertAsync(
        decimal amount,
        string sourceCurrency,
        string targetCurrency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current exchange rate between two currencies
    /// </summary>
    /// <param name="sourceCurrency">Source currency code</param>
    /// <param name="targetCurrency">Target currency code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current exchange rate</returns>
    Task<decimal> GetExchangeRateAsync(
        string sourceCurrency,
        string targetCurrency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all supported currencies
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of supported currency codes</returns>
    Task<IEnumerable<string>> GetSupportedCurrenciesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if currency code is supported
    /// </summary>
    /// <param name="currencyCode">Currency code to validate</param>
    /// <returns>True if currency is supported</returns>
    Task<bool> IsCurrencySupportedAsync(string currencyCode);
}

/// <summary>
/// Result of currency conversion operation
/// </summary>
public class CurrencyConversionResult
{
    /// <summary>
    /// Original amount
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Source currency code
    /// </summary>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Converted amount
    /// </summary>
    public decimal ConvertedAmount { get; set; }

    /// <summary>
    /// Target currency code
    /// </summary>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Applied exchange rate
    /// </summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// Conversion timestamp
    /// </summary>
    public DateTime ConvertedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Exchange rate source (BACEN, ECB, MANUAL, etc.)
    /// </summary>
    public string RateSource { get; set; } = string.Empty;

    /// <summary>
    /// Rate validity period
    /// </summary>
    public DateTime? RateValidUntil { get; set; }

    /// <summary>
    /// Indicates if conversion was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if conversion failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
