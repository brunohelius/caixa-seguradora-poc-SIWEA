using AutoMapper;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Exceptions;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CaixaSeguradora.Core.Tests.Services;

/// <summary>
/// T086-T110 [US1, US2] - Unit tests for ClaimService
/// Tests search operations, validation, and claim retrieval business logic
/// </summary>
public class ClaimServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ClaimService>> _mockLogger;
    private readonly ClaimService _claimService;
    private readonly Mock<IClaimRepository> _mockClaimRepository;
    private readonly Mock<IClaimHistoryRepository> _mockClaimHistoryRepository;

    public ClaimServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ClaimService>>();
        _mockClaimRepository = new Mock<IClaimRepository>();
        _mockClaimHistoryRepository = new Mock<IClaimHistoryRepository>();

        _mockUnitOfWork.Setup(u => u.Claims).Returns(_mockClaimRepository.Object);
        _mockUnitOfWork.Setup(u => u.ClaimHistories).Returns(_mockClaimHistoryRepository.Object);

        _claimService = new ClaimService(_mockUnitOfWork.Object, _mockMapper.Object, _mockLogger.Object);
    }

    #region SearchClaimAsync - Protocol Tests

    [Fact]
    public async Task SearchClaimAsync_WithValidProtocol_ReturnsClaimDetails()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = 7
        };

        var claimEntity = CreateMockClaimEntity();
        var claimDto = CreateMockClaimDto();

        _mockClaimRepository.Setup(r => r.SearchByProtocolAsync(
                criteria.Fonte.Value, criteria.Protsini.Value, criteria.Dac.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimEntity);

        _mockMapper.Setup(m => m.Map<ClaimDetailDto>(claimEntity))
            .Returns(claimDto);

        // Act
        var result = await _claimService.SearchClaimAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(claimDto.NumeroProtocolo, result.NumeroProtocolo);
        Assert.Equal(claimDto.NumeroSinistro, result.NumeroSinistro);
        _mockClaimRepository.Verify(r => r.SearchByProtocolAsync(
            criteria.Fonte.Value, criteria.Protsini.Value, criteria.Dac.Value, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchClaimAsync_WithProtocolNotFound_ThrowsClaimNotFoundException()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = 999999,
            Dac = 9
        };

        _mockClaimRepository.Setup(r => r.SearchByProtocolAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimMaster?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ClaimNotFoundException>(() =>
            _claimService.SearchClaimAsync(criteria));

        Assert.Contains("Protocol", exception.Message);
        Assert.Contains("999999", exception.Message);
    }

    [Fact]
    public async Task SearchClaimAsync_WithInvalidProtocolDAC_ThrowsClaimNotFoundException()
    {
        // Arrange - DAC is valid (0-9) but claim not found
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1,
            Protsini = 123456,
            Dac = 10 // Invalid DAC
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _claimService.SearchClaimAsync(criteria));
    }

    #endregion

    #region SearchClaimAsync - ClaimNumber Tests

    [Fact]
    public async Task SearchClaimAsync_WithValidClaimNumber_ReturnsClaimDetails()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 100,
            Rmosin = 200,
            Numsin = 12345
        };

        var claimEntity = CreateMockClaimEntity();
        var claimDto = CreateMockClaimDto();

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(
                criteria.Orgsin.Value, criteria.Rmosin.Value, criteria.Numsin.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimEntity);

        _mockMapper.Setup(m => m.Map<ClaimDetailDto>(claimEntity))
            .Returns(claimDto);

        // Act
        var result = await _claimService.SearchClaimAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(criteria.Orgsin, result.Orgsin);
        Assert.Equal(criteria.Rmosin, result.Rmosin);
        Assert.Equal(criteria.Numsin, result.Numsin);
    }

    [Fact]
    public async Task SearchClaimAsync_WithClaimNumberNotFound_ThrowsClaimNotFoundException()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 100,
            Rmosin = 200,
            Numsin = 99999
        };

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimMaster?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ClaimNotFoundException>(() =>
            _claimService.SearchClaimAsync(criteria));

        Assert.Contains("ClaimNumber", exception.Message);
        Assert.Contains("99999", exception.Message);
    }

    [Fact]
    public async Task SearchClaimAsync_WithZeroClaimNumber_ThrowsArgumentException()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 0,
            Rmosin = 200,
            Numsin = 12345
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _claimService.SearchClaimAsync(criteria));
    }

    #endregion

    #region SearchClaimAsync - LeaderCode Tests

    [Fact]
    public async Task SearchClaimAsync_WithValidLeaderCode_ReturnsFirstClaim()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = 500,
            Sinlid = 67890
        };

        var claimEntities = new List<ClaimMaster> { CreateMockClaimEntity(), CreateMockClaimEntity() };
        var claimDto = CreateMockClaimDto();

        _mockClaimRepository.Setup(r => r.SearchByLeaderCodeAsync(
                criteria.Codlider.Value, criteria.Sinlid.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimEntities);

        _mockMapper.Setup(m => m.Map<ClaimDetailDto>(It.IsAny<ClaimMaster>()))
            .Returns(claimDto);

        // Act
        var result = await _claimService.SearchClaimAsync(criteria);

        // Assert
        Assert.NotNull(result);
        _mockClaimRepository.Verify(r => r.SearchByLeaderCodeAsync(
            criteria.Codlider.Value, criteria.Sinlid.Value, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchClaimAsync_WithLeaderCodeNotFound_ThrowsClaimNotFoundException()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = 999,
            Sinlid = 88888
        };

        _mockClaimRepository.Setup(r => r.SearchByLeaderCodeAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClaimMaster>()); // Empty list

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ClaimNotFoundException>(() =>
            _claimService.SearchClaimAsync(criteria));

        Assert.Contains("LeaderCode", exception.Message);
        Assert.Contains("999", exception.Message);
    }

    [Fact]
    public async Task SearchClaimAsync_WithLeaderCodeMultipleResults_ReturnsFirstOnly()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Codlider = 500,
            Sinlid = 67890
        };

        var firstClaim = CreateMockClaimEntity();
        firstClaim.Numsin = 1001;
        var secondClaim = CreateMockClaimEntity();
        secondClaim.Numsin = 1002;

        var claimEntities = new List<ClaimMaster> { firstClaim, secondClaim };
        var claimDto = CreateMockClaimDto();
        claimDto.Numsin = 1001;

        _mockClaimRepository.Setup(r => r.SearchByLeaderCodeAsync(
                criteria.Codlider.Value, criteria.Sinlid.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimEntities);

        _mockMapper.Setup(m => m.Map<ClaimDetailDto>(firstClaim))
            .Returns(claimDto);

        // Act
        var result = await _claimService.SearchClaimAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1001, result.Numsin); // First claim
    }

    #endregion

    #region SearchClaimAsync - Validation Tests

    [Fact]
    public async Task SearchClaimAsync_WithNullCriteria_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _claimService.SearchClaimAsync(null!));
    }

    [Fact]
    public async Task SearchClaimAsync_WithInvalidCriteria_ThrowsArgumentException()
    {
        // Arrange - incomplete criteria (only Fonte, missing Protsini and Dac)
        var criteria = new ClaimSearchCriteria
        {
            Fonte = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _claimService.SearchClaimAsync(criteria));
    }

    [Fact]
    public async Task SearchClaimAsync_WithEmptyCriteria_ThrowsArgumentException()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _claimService.SearchClaimAsync(criteria));
    }

    #endregion

    #region GetClaimByIdAsync Tests

    [Fact]
    public async Task GetClaimByIdAsync_WithValidId_ReturnsClaimDetails()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;

        var claimEntity = CreateMockClaimEntity();
        var claimDto = CreateMockClaimDto();

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(
                orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimEntity);

        _mockMapper.Setup(m => m.Map<ClaimDetailDto>(claimEntity))
            .Returns(claimDto);

        // Act
        var result = await _claimService.GetClaimByIdAsync(tipseg, orgsin, rmosin, numsin);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orgsin, result.Orgsin);
        Assert.Equal(rmosin, result.Rmosin);
        Assert.Equal(numsin, result.Numsin);
    }

    [Fact]
    public async Task GetClaimByIdAsync_WithNonExistentId_ThrowsClaimNotFoundException()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 99999;

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimMaster?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ClaimNotFoundException>(() =>
            _claimService.GetClaimByIdAsync(tipseg, orgsin, rmosin, numsin));
    }

    #endregion

    #region ValidateClaimExistsAsync Tests

    [Fact]
    public async Task ValidateClaimExistsAsync_WithExistingClaim_ReturnsTrue()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 100,
            Rmosin = 200,
            Numsin = 12345
        };

        var claimEntity = CreateMockClaimEntity();
        var claimDto = CreateMockClaimDto();

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimEntity);

        _mockMapper.Setup(m => m.Map<ClaimDetailDto>(claimEntity))
            .Returns(claimDto);

        // Act
        var result = await _claimService.ValidateClaimExistsAsync(criteria);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateClaimExistsAsync_WithNonExistingClaim_ReturnsFalse()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria
        {
            Orgsin = 100,
            Rmosin = 200,
            Numsin = 99999
        };

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimMaster?)null);

        // Act
        var result = await _claimService.ValidateClaimExistsAsync(criteria);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateClaimExistsAsync_WithNullCriteria_ReturnsFalse()
    {
        // Act
        var result = await _claimService.ValidateClaimExistsAsync(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateClaimExistsAsync_WithInvalidCriteria_ReturnsFalse()
    {
        // Arrange
        var criteria = new ClaimSearchCriteria(); // Empty criteria

        // Act
        var result = await _claimService.ValidateClaimExistsAsync(criteria);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetPendingValueAsync Tests

    [Fact]
    public async Task GetPendingValueAsync_WithValidClaim_ReturnsPendingValue()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;

        var claimEntity = CreateMockClaimEntity();
        claimEntity.Sdopag = 10000.00m;
        claimEntity.Totpag = 3500.00m;

        var claimDto = CreateMockClaimDto();
        claimDto.Sdopag = 10000.00m;
        claimDto.Totpag = 3500.00m;
        claimDto.ValorPendente = 6500.00m;

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(
                orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimEntity);

        _mockMapper.Setup(m => m.Map<ClaimDetailDto>(claimEntity))
            .Returns(claimDto);

        // Act
        var result = await _claimService.GetPendingValueAsync(tipseg, orgsin, rmosin, numsin);

        // Assert
        Assert.Equal(6500.00m, result);
    }

    [Fact]
    public async Task GetPendingValueAsync_WithFullyPaidClaim_ReturnsZero()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;

        var claimEntity = CreateMockClaimEntity();
        claimEntity.Sdopag = 10000.00m;
        claimEntity.Totpag = 10000.00m;

        var claimDto = CreateMockClaimDto();
        claimDto.Sdopag = 10000.00m;
        claimDto.Totpag = 10000.00m;
        claimDto.ValorPendente = 0.00m;

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(
                orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimEntity);

        _mockMapper.Setup(m => m.Map<ClaimDetailDto>(claimEntity))
            .Returns(claimDto);

        // Act
        var result = await _claimService.GetPendingValueAsync(tipseg, orgsin, rmosin, numsin);

        // Assert
        Assert.Equal(0.00m, result);
    }

    [Fact]
    public async Task GetPendingValueAsync_WithNonExistentClaim_ThrowsClaimNotFoundException()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 99999;

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimMaster?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ClaimNotFoundException>(() =>
            _claimService.GetPendingValueAsync(tipseg, orgsin, rmosin, numsin));
    }

    #endregion

    #region Helper Methods

    private ClaimMaster CreateMockClaimEntity()
    {
        return new ClaimMaster
        {
            Tipseg = 1,
            Orgsin = 100,
            Rmosin = 200,
            Numsin = 12345,
            Fonte = 1,
            Protsini = 123456,
            Dac = 7,
            Orgapo = 100,
            Rmoapo = 200,
            Numapol = 54321,
            Codprodu = 1001,
            Sdopag = 10000.00m,
            Totpag = 2500.00m,
            Ocorhist = 5,
            Tipreg = "1",
            Tpsegu = 1,
            Codlider = 500,
            Sinlid = 67890,
            CreatedBy = "SYSTEM",
            CreatedAt = DateTime.UtcNow
        };
    }

    private ClaimDetailDto CreateMockClaimDto()
    {
        return new ClaimDetailDto
        {
            Tipseg = 1,
            Orgsin = 100,
            Rmosin = 200,
            Numsin = 12345,
            Fonte = 1,
            Protsini = 123456,
            Dac = 7,
            NumeroProtocolo = "001/0123456-7",
            NumeroSinistro = "100/200/12345",
            Orgapo = 100,
            Rmoapo = 200,
            Numapol = 54321,
            NumeroApolice = "100/200/54321",
            NomeSeguradora = "João da Silva",
            NomeRamo = "Seguro Habitacional",
            Codprodu = 1001,
            EhConsorcio = false,
            Sdopag = 10000.00m,
            Totpag = 2500.00m,
            ValorPendente = 7500.00m,
            Codlider = 500,
            Sinlid = 67890,
            Ocorhist = 5,
            Tipreg = "1",
            Tpsegu = 1,
            CreatedBy = "SYSTEM",
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
