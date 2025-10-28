using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CaixaSeguradora.Core.Tests.Services;

/// <summary>
/// T111-T125 [BR-023, BR-027-BR-037] - Currency Conversion Service Unit Tests
/// Tests currency conversion calculations, caching, and fallback behavior
/// </summary>
public class CurrencyConversionServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<CurrencyConversionService>> _mockLogger;
    private readonly CurrencyConversionService _service;

    public CurrencyConversionServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<CurrencyConversionService>>();

        _service = new CurrencyConversionService(
            _mockHttpClientFactory.Object,
            _memoryCache,
            _mockLogger.Object);
    }

    #region ConvertAsync Tests

    [Fact]
    public async Task ConvertAsync_WithSameCurrency_ReturnsOriginalAmount()
    {
        // Arrange
        var amount = 1000.00m;
        var currency = "BRL";

        // Act
        var result = await _service.ConvertAsync(amount, currency, currency);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(amount, result.OriginalAmount);
        Assert.Equal(amount, result.ConvertedAmount);
        Assert.Equal(1.0m, result.ExchangeRate);
        Assert.Equal("DIRECT", result.RateSource);
    }

    [Fact]
    public async Task ConvertAsync_BRLToUSD_ReturnsConvertedAmount()
    {
        // Arrange
        var amount = 5000.00m;
        var sourceCurrency = "BRL";
        var targetCurrency = "USD";

        // Act
        var result = await _service.ConvertAsync(amount, sourceCurrency, targetCurrency);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(amount, result.OriginalAmount);
        Assert.Equal("BRL", result.SourceCurrency);
        Assert.Equal("USD", result.TargetCurrency);
        Assert.True(result.ExchangeRate > 0);
        Assert.True(result.ConvertedAmount > 0);
        Assert.Equal(Math.Round(amount * result.ExchangeRate, 2), result.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertAsync_USDToBRL_ReturnsConvertedAmount()
    {
        // Arrange
        var amount = 1000.00m;
        var sourceCurrency = "USD";
        var targetCurrency = "BRL";

        // Act
        var result = await _service.ConvertAsync(amount, sourceCurrency, targetCurrency);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(amount, result.OriginalAmount);
        Assert.Equal("USD", result.SourceCurrency);
        Assert.Equal("BRL", result.TargetCurrency);
        Assert.True(result.ExchangeRate > 0);
        Assert.True(result.ConvertedAmount > 0);
    }

    [Fact]
    public async Task ConvertAsync_WithZeroAmount_ReturnsZero()
    {
        // Arrange
        var amount = 0.00m;
        var sourceCurrency = "BRL";
        var targetCurrency = "USD";

        // Act
        var result = await _service.ConvertAsync(amount, sourceCurrency, targetCurrency);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0.00m, result.ConvertedAmount);
    }

    [Theory]
    [InlineData("BRL", "EUR")]
    [InlineData("EUR", "BRL")]
    [InlineData("GBP", "JPY")]
    [InlineData("JPY", "USD")]
    public async Task ConvertAsync_WithDifferentCurrencyPairs_ReturnsSuccess(string source, string target)
    {
        // Arrange
        var amount = 1000.00m;

        // Act
        var result = await _service.ConvertAsync(amount, source, target);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(source.ToUpperInvariant(), result.SourceCurrency);
        Assert.Equal(target.ToUpperInvariant(), result.TargetCurrency);
        Assert.True(result.ExchangeRate > 0);
    }

    [Fact]
    public async Task ConvertAsync_WithNegativeAmount_ConvertsNormally()
    {
        // Arrange - negative amounts should still convert (e.g., for reversals)
        var amount = -500.00m;
        var sourceCurrency = "BRL";
        var targetCurrency = "USD";

        // Act
        var result = await _service.ConvertAsync(amount, sourceCurrency, targetCurrency);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.ConvertedAmount < 0);
        Assert.Equal(Math.Round(amount * result.ExchangeRate, 2), result.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertAsync_WithLargeAmount_RoundsTo2DecimalPlaces()
    {
        // Arrange - BR-028: Final currency calculations must have 2 decimal places
        var amount = 123456.789m;
        var sourceCurrency = "BRL";
        var targetCurrency = "USD";

        // Act
        var result = await _service.ConvertAsync(amount, sourceCurrency, targetCurrency);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, CountDecimalPlaces(result.ConvertedAmount));
    }

    #endregion

    #region GetExchangeRateAsync Tests

    [Fact]
    public async Task GetExchangeRateAsync_FirstCall_FetchesAndCachesRate()
    {
        // Arrange
        var sourceCurrency = "BRL";
        var targetCurrency = "USD";

        // Act
        var rate = await _service.GetExchangeRateAsync(sourceCurrency, targetCurrency);

        // Assert
        Assert.True(rate > 0);

        // Verify caching by calling again
        var cachedRate = await _service.GetExchangeRateAsync(sourceCurrency, targetCurrency);
        Assert.Equal(rate, cachedRate);
    }

    [Fact]
    public async Task GetExchangeRateAsync_WithCachedRate_ReturnsFromCache()
    {
        // Arrange
        var sourceCurrency = "BRL";
        var targetCurrency = "EUR";

        // First call to populate cache
        var firstRate = await _service.GetExchangeRateAsync(sourceCurrency, targetCurrency);

        // Act - Second call should use cache
        var secondRate = await _service.GetExchangeRateAsync(sourceCurrency, targetCurrency);

        // Assert
        Assert.Equal(firstRate, secondRate);
    }

    [Fact]
    public async Task GetExchangeRateAsync_WithUnsupportedCurrency_UsesFallbackOrDefault()
    {
        // Arrange
        var sourceCurrency = "XXX"; // Non-existent currency
        var targetCurrency = "BRL";

        // Act
        var rate = await _service.GetExchangeRateAsync(sourceCurrency, targetCurrency);

        // Assert - Should return 1.0 as fallback
        Assert.Equal(1.0m, rate);
    }

    #endregion

    #region GetSupportedCurrenciesAsync Tests

    [Fact]
    public async Task GetSupportedCurrenciesAsync_ReturnsListOfCurrencies()
    {
        // Act
        var currencies = await _service.GetSupportedCurrenciesAsync();

        // Assert
        Assert.NotNull(currencies);
        Assert.NotEmpty(currencies);
        Assert.Contains("BRL", currencies);
        Assert.Contains("USD", currencies);
        Assert.Contains("EUR", currencies);
    }

    [Fact]
    public async Task GetSupportedCurrenciesAsync_ContainsAtLeast5Currencies()
    {
        // Act
        var currencies = await _service.GetSupportedCurrenciesAsync();

        // Assert
        Assert.True(currencies.Count() >= 5);
    }

    #endregion

    #region IsCurrencySupportedAsync Tests

    [Theory]
    [InlineData("BRL", true)]
    [InlineData("USD", true)]
    [InlineData("EUR", true)]
    [InlineData("GBP", true)]
    [InlineData("JPY", true)]
    [InlineData("XXX", false)]
    [InlineData("ZZZ", false)]
    public async Task IsCurrencySupportedAsync_WithVariousCurrencies_ReturnsCorrectResult(string currency, bool expected)
    {
        // Act
        var result = await _service.IsCurrencySupportedAsync(currency);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task IsCurrencySupportedAsync_IsCaseInsensitive()
    {
        // Act
        var upperCase = await _service.IsCurrencySupportedAsync("BRL");
        var lowerCase = await _service.IsCurrencySupportedAsync("brl");
        var mixedCase = await _service.IsCurrencySupportedAsync("BrL");

        // Assert
        Assert.True(upperCase);
        Assert.True(lowerCase);
        Assert.True(mixedCase);
    }

    #endregion

    #region Business Rules Tests

    [Fact]
    public async Task ConvertAsync_BR023_CalculatesVALPRIBT_Correctly()
    {
        // Arrange - BR-023: VALPRIBT = VALPRI × VLCRUZAD
        var valpri = 1000.00m;
        var sourceCurrency = "BRL";
        var targetCurrency = "BTNF"; // Target is always BTNF

        // Act
        var result = await _service.ConvertAsync(valpri, sourceCurrency, targetCurrency);

        // Assert - Verify formula: ConvertedAmount = Amount × ExchangeRate
        var expectedAmount = Math.Round(valpri * result.ExchangeRate, 2);
        Assert.Equal(expectedAmount, result.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertAsync_BR028_RoundsTo2DecimalPlaces()
    {
        // Arrange - BR-028: Final currency calculations must have 2 decimal places
        var amount = 1234.56789m;
        var sourceCurrency = "BRL";
        var targetCurrency = "USD";

        // Act
        var result = await _service.ConvertAsync(amount, sourceCurrency, targetCurrency);

        // Assert
        Assert.Equal(2, CountDecimalPlaces(result.ConvertedAmount));
        Assert.DoesNotContain(".", result.ConvertedAmount.ToString().Substring(
            result.ConvertedAmount.ToString().IndexOf('.') + 1 + 2));
    }

    #endregion

    #region Helper Methods

    private int CountDecimalPlaces(decimal value)
    {
        var valueString = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var decimalIndex = valueString.IndexOf('.');

        if (decimalIndex == -1)
            return 0;

        return valueString.Length - decimalIndex - 1;
    }

    #endregion
}
