using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Infrastructure.Services;
using Moq;
using Xunit;

namespace CaixaSeguradora.Infrastructure.Tests.Services;

/// <summary>
/// T072: Unit tests for CurrencyConversionService
/// Tests currency conversion to BTNF with various scenarios including
/// valid rates, missing rates, decimal precision, and rounding logic.
/// </summary>
public class CurrencyConversionServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<CurrencyUnit>> _mockCurrencyRepo;
    private readonly CurrencyConversionService _service;

    public CurrencyConversionServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCurrencyRepo = new Mock<IRepository<CurrencyUnit>>();
        _service = new CurrencyConversionService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task ConvertToBTNF_WithValidRate_ReturnsConvertedValue()
    {
        // Arrange
        var transactionDate = new DateTime(2025, 10, 23);
        var amount = 1000.00m;
        var expectedRate = 1.5m;
        var expectedResult = 1500.00m;

        var currencyUnit = new CurrencyUnit
        {
            Dtinivig = new DateTime(2025, 10, 1),
            Dttervig = new DateTime(2025, 10, 31),
            Vlcruzad = expectedRate
        };

        // Mock repository to return the currency unit
        _mockCurrencyRepo
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CurrencyUnit, bool>>>()))
            .ReturnsAsync(new List<CurrencyUnit> { currencyUnit });

        // Mock UnitOfWork to return the mocked repository
        _mockUnitOfWork
            .Setup(u => u.GetRepository<CurrencyUnit>())
            .Returns(_mockCurrencyRepo.Object);

        // Act
        var result = await _service.ConvertToBTNF(amount, transactionDate);

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(2, GetDecimalPlaces(result)); // Verify 2 decimal precision
    }

    [Fact]
    public async Task ConvertToBTNF_NoRateForDate_ThrowsException()
    {
        // Arrange
        var transactionDate = new DateTime(2025, 12, 25);
        var amount = 1000.00m;

        // Mock repository to return empty list (no rate found)
        _mockCurrencyRepo
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CurrencyUnit, bool>>>()))
            .ReturnsAsync(new List<CurrencyUnit>());

        _mockUnitOfWork
            .Setup(u => u.GetRepository<CurrencyUnit>())
            .Returns(_mockCurrencyRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ConvertToBTNF(amount, transactionDate)
        );
    }

    [Fact]
    public async Task ConvertToBTNF_RoundsTo2Decimals()
    {
        // Arrange
        var transactionDate = new DateTime(2025, 10, 23);
        var amount = 1000.00m;
        var rate = 1.33333333m; // Rate with many decimals
        var expectedResult = 1333.33m; // Should round to 2 decimals

        var currencyUnit = new CurrencyUnit
        {
            Dtinivig = new DateTime(2025, 10, 1),
            Dttervig = new DateTime(2025, 10, 31),
            Vlcruzad = rate
        };

        _mockCurrencyRepo
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CurrencyUnit, bool>>>()))
            .ReturnsAsync(new List<CurrencyUnit> { currencyUnit });

        _mockUnitOfWork
            .Setup(u => u.GetRepository<CurrencyUnit>())
            .Returns(_mockCurrencyRepo.Object);

        // Act
        var result = await _service.ConvertToBTNF(amount, transactionDate);

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(2, GetDecimalPlaces(result));
    }

    [Fact]
    public async Task ConvertToBTNF_RoundsAwayFromZero()
    {
        // Arrange - Test MidpointRounding.AwayFromZero behavior
        var transactionDate = new DateTime(2025, 10, 23);
        var amount = 100.00m;
        var rate = 1.555m; // This will result in 155.50 exactly
        var expectedResult = 155.50m;

        var currencyUnit = new CurrencyUnit
        {
            Dtinivig = new DateTime(2025, 10, 1),
            Dttervig = new DateTime(2025, 10, 31),
            Vlcruzad = rate
        };

        _mockCurrencyRepo
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CurrencyUnit, bool>>>()))
            .ReturnsAsync(new List<CurrencyUnit> { currencyUnit });

        _mockUnitOfWork
            .Setup(u => u.GetRepository<CurrencyUnit>())
            .Returns(_mockCurrencyRepo.Object);

        // Act
        var result = await _service.ConvertToBTNF(amount, transactionDate);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task ConvertToBTNF_WithZeroAmount_ReturnsZero()
    {
        // Arrange
        var transactionDate = new DateTime(2025, 10, 23);
        var amount = 0.00m;
        var rate = 1.5m;

        var currencyUnit = new CurrencyUnit
        {
            Dtinivig = new DateTime(2025, 10, 1),
            Dttervig = new DateTime(2025, 10, 31),
            Vlcruzad = rate
        };

        _mockCurrencyRepo
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CurrencyUnit, bool>>>()))
            .ReturnsAsync(new List<CurrencyUnit> { currencyUnit });

        _mockUnitOfWork
            .Setup(u => u.GetRepository<CurrencyUnit>())
            .Returns(_mockCurrencyRepo.Object);

        // Act
        var result = await _service.ConvertToBTNF(amount, transactionDate);

        // Assert
        Assert.Equal(0.00m, result);
    }

    [Fact]
    public async Task ConvertToBTNF_LargeAmount_HandlesCorrectly()
    {
        // Arrange - Test with large amounts to ensure no overflow
        var transactionDate = new DateTime(2025, 10, 23);
        var amount = 999999.99m;
        var rate = 2.0m;
        var expectedResult = 1999999.98m;

        var currencyUnit = new CurrencyUnit
        {
            Dtinivig = new DateTime(2025, 10, 1),
            Dttervig = new DateTime(2025, 10, 31),
            Vlcruzad = rate
        };

        _mockCurrencyRepo
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CurrencyUnit, bool>>>()))
            .ReturnsAsync(new List<CurrencyUnit> { currencyUnit });

        _mockUnitOfWork
            .Setup(u => u.GetRepository<CurrencyUnit>())
            .Returns(_mockCurrencyRepo.Object);

        // Act
        var result = await _service.ConvertToBTNF(amount, transactionDate);

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(2, GetDecimalPlaces(result));
    }

    [Fact]
    public async Task ConvertToBTNF_DateAtStartOfRange_ReturnsCorrectRate()
    {
        // Arrange - Date exactly at start of validity range
        var transactionDate = new DateTime(2025, 10, 1);
        var amount = 1000.00m;
        var rate = 1.25m;

        var currencyUnit = new CurrencyUnit
        {
            Dtinivig = new DateTime(2025, 10, 1), // Same as transaction date
            Dttervig = new DateTime(2025, 10, 31),
            Vlcruzad = rate
        };

        _mockCurrencyRepo
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CurrencyUnit, bool>>>()))
            .ReturnsAsync(new List<CurrencyUnit> { currencyUnit });

        _mockUnitOfWork
            .Setup(u => u.GetRepository<CurrencyUnit>())
            .Returns(_mockCurrencyRepo.Object);

        // Act
        var result = await _service.ConvertToBTNF(amount, transactionDate);

        // Assert
        Assert.Equal(1250.00m, result);
    }

    [Fact]
    public async Task ConvertToBTNF_DateAtEndOfRange_ReturnsCorrectRate()
    {
        // Arrange - Date exactly at end of validity range
        var transactionDate = new DateTime(2025, 10, 31);
        var amount = 1000.00m;
        var rate = 1.25m;

        var currencyUnit = new CurrencyUnit
        {
            Dtinivig = new DateTime(2025, 10, 1),
            Dttervig = new DateTime(2025, 10, 31), // Same as transaction date
            Vlcruzad = rate
        };

        _mockCurrencyRepo
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CurrencyUnit, bool>>>()))
            .ReturnsAsync(new List<CurrencyUnit> { currencyUnit });

        _mockUnitOfWork
            .Setup(u => u.GetRepository<CurrencyUnit>())
            .Returns(_mockCurrencyRepo.Object);

        // Act
        var result = await _service.ConvertToBTNF(amount, transactionDate);

        // Assert
        Assert.Equal(1250.00m, result);
    }

    [Fact]
    public async Task GetCurrentRate_ReturnsValidRate()
    {
        // Arrange
        var date = new DateTime(2025, 10, 23);
        var expectedRate = 1.5m;

        var currencyUnit = new CurrencyUnit
        {
            Dtinivig = new DateTime(2025, 10, 1),
            Dttervig = new DateTime(2025, 10, 31),
            Vlcruzad = expectedRate
        };

        _mockCurrencyRepo
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CurrencyUnit, bool>>>()))
            .ReturnsAsync(new List<CurrencyUnit> { currencyUnit });

        _mockUnitOfWork
            .Setup(u => u.GetRepository<CurrencyUnit>())
            .Returns(_mockCurrencyRepo.Object);

        // Act
        var result = await _service.GetCurrentRate(date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRate, result.Vlcruzad);
        Assert.Equal(currencyUnit.Dtinivig, result.Dtinivig);
        Assert.Equal(currencyUnit.Dttervig, result.Dttervig);
    }

    /// <summary>
    /// Helper method to count decimal places
    /// </summary>
    private int GetDecimalPlaces(decimal value)
    {
        var text = value.ToString("G29");
        var decimalIndex = text.IndexOf('.');
        if (decimalIndex < 0) return 0;
        return text.Length - decimalIndex - 1;
    }
}
