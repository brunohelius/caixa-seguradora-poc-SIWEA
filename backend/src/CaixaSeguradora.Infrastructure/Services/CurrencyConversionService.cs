using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Infrastructure.Services;

/// <summary>
/// Currency conversion service with caching and fallback rates
/// </summary>
public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrencyConversionService> _logger;

    // Fallback rates (updated periodically or from configuration)
    private static readonly Dictionary<string, Dictionary<string, decimal>> FallbackRates = new()
    {
        ["BRL"] = new() { ["USD"] = 0.20m, ["EUR"] = 0.18m, ["GBP"] = 0.16m, ["JPY"] = 29.50m },
        ["USD"] = new() { ["BRL"] = 5.00m, ["EUR"] = 0.92m, ["GBP"] = 0.79m, ["JPY"] = 148.50m },
        ["EUR"] = new() { ["BRL"] = 5.45m, ["USD"] = 1.09m, ["GBP"] = 0.86m, ["JPY"] = 161.30m },
        ["GBP"] = new() { ["BRL"] = 6.30m, ["USD"] = 1.26m, ["EUR"] = 1.16m, ["JPY"] = 187.20m },
        ["JPY"] = new() { ["BRL"] = 0.034m, ["USD"] = 0.0067m, ["EUR"] = 0.0062m, ["GBP"] = 0.0053m }
    };

    private static readonly string[] SupportedCurrencies = { "BRL", "USD", "EUR", "GBP", "JPY", "CHF", "CAD", "AUD" };

    public CurrencyConversionService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<CurrencyConversionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<CurrencyConversionResult> ConvertAsync(
        decimal amount,
        string sourceCurrency,
        string targetCurrency,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Same currency - no conversion needed
            if (sourceCurrency.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase))
            {
                return new CurrencyConversionResult
                {
                    OriginalAmount = amount,
                    SourceCurrency = sourceCurrency.ToUpperInvariant(),
                    ConvertedAmount = amount,
                    TargetCurrency = targetCurrency.ToUpperInvariant(),
                    ExchangeRate = 1.0m,
                    ConvertedAt = DateTime.UtcNow,
                    RateSource = "DIRECT",
                    IsSuccess = true
                };
            }

            // Get exchange rate
            var exchangeRate = await GetExchangeRateAsync(sourceCurrency, targetCurrency, cancellationToken);

            // Calculate converted amount
            var convertedAmount = Math.Round(amount * exchangeRate, 2, MidpointRounding.AwayFromZero);

            return new CurrencyConversionResult
            {
                OriginalAmount = amount,
                SourceCurrency = sourceCurrency.ToUpperInvariant(),
                ConvertedAmount = convertedAmount,
                TargetCurrency = targetCurrency.ToUpperInvariant(),
                ExchangeRate = exchangeRate,
                ConvertedAt = DateTime.UtcNow,
                RateSource = "BACEN_CACHE",
                RateValidUntil = DateTime.UtcNow.AddHours(1),
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Currency conversion failed: {Source} -> {Target}", sourceCurrency, targetCurrency);

            return new CurrencyConversionResult
            {
                OriginalAmount = amount,
                SourceCurrency = sourceCurrency.ToUpperInvariant(),
                ConvertedAmount = 0,
                TargetCurrency = targetCurrency.ToUpperInvariant(),
                ExchangeRate = 0,
                IsSuccess = false,
                ErrorMessage = $"Conversion failed: {ex.Message}"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<decimal> GetExchangeRateAsync(
        string sourceCurrency,
        string targetCurrency,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"exchange_rate_{sourceCurrency}_{targetCurrency}";

        // Try cache first
        if (_cache.TryGetValue<decimal>(cacheKey, out var cachedRate))
        {
            _logger.LogDebug("Exchange rate retrieved from cache: {Source} -> {Target} = {Rate}",
                sourceCurrency, targetCurrency, cachedRate);
            return cachedRate;
        }

        try
        {
            // Try external API (e.g., BACEN - Banco Central do Brasil)
            var rate = await FetchExchangeRateFromApiAsync(sourceCurrency, targetCurrency, cancellationToken);

            // Cache for 1 hour
            _cache.Set(cacheKey, rate, TimeSpan.FromHours(1));

            _logger.LogInformation("Exchange rate fetched from API: {Source} -> {Target} = {Rate}",
                sourceCurrency, targetCurrency, rate);

            return rate;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API fetch failed, using fallback rate: {Source} -> {Target}",
                sourceCurrency, targetCurrency);

            // Use fallback rates
            var fallbackRate = GetFallbackRate(sourceCurrency, targetCurrency);

            // Cache fallback for shorter period (15 minutes)
            _cache.Set(cacheKey, fallbackRate, TimeSpan.FromMinutes(15));

            return fallbackRate;
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<string>>(SupportedCurrencies);
    }

    /// <inheritdoc/>
    public Task<bool> IsCurrencySupportedAsync(string currencyCode)
    {
        var isSupported = SupportedCurrencies.Contains(currencyCode.ToUpperInvariant());
        return Task.FromResult(isSupported);
    }

    /// <summary>
    /// Fetches exchange rate from external API
    /// </summary>
    private async Task<decimal> FetchExchangeRateFromApiAsync(
        string sourceCurrency,
        string targetCurrency,
        CancellationToken cancellationToken)
    {
        // Mock implementation - in production, integrate with BACEN or other financial API
        // Example: https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata

        await Task.Delay(100, cancellationToken); // Simulate API call

        // For demonstration, return fallback rate
        return GetFallbackRate(sourceCurrency, targetCurrency);
    }

    /// <summary>
    /// Gets fallback exchange rate from static configuration
    /// </summary>
    private decimal GetFallbackRate(string sourceCurrency, string targetCurrency)
    {
        var source = sourceCurrency.ToUpperInvariant();
        var target = targetCurrency.ToUpperInvariant();

        if (FallbackRates.TryGetValue(source, out var targetRates))
        {
            if (targetRates.TryGetValue(target, out var rate))
            {
                return rate;
            }
        }

        // If direct rate not found, try inverse rate
        if (FallbackRates.TryGetValue(target, out var inverseRates))
        {
            if (inverseRates.TryGetValue(source, out var inverseRate))
            {
                return Math.Round(1 / inverseRate, 4, MidpointRounding.AwayFromZero);
            }
        }

        // Default to 1:1 if no rate found
        _logger.LogWarning("No exchange rate found for {Source} -> {Target}, using 1:1", source, target);
        return 1.0m;
    }
}
