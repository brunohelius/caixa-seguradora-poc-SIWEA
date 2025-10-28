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
/// T078-T085 [US3] - Unit tests for ClaimService.GetClaimHistoryAsync
/// Tests pagination logic, performance optimization, and error handling
/// </summary>
public class ClaimHistoryServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ClaimService>> _mockLogger;
    private readonly ClaimService _claimService;
    private readonly Mock<IClaimRepository> _mockClaimRepository;
    private readonly Mock<IClaimHistoryRepository> _mockClaimHistoryRepository;

    public ClaimHistoryServiceTests()
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

    [Fact]
    public async Task GetClaimHistoryAsync_WithValidClaim_ReturnsFirstPage()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;
        var page = 1;
        var pageSize = 20;

        // Mock claim exists
        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimMaster { Tipseg = tipseg, Orgsin = orgsin, Rmosin = rmosin, Numsin = numsin });

        // Mock history data (50 total records)
        var historyEntities = CreateMockHistoryRecords(tipseg, orgsin, rmosin, numsin, 50);
        var firstPageEntities = historyEntities.Take(pageSize).ToList();

        _mockClaimHistoryRepository.Setup(r => r.GetPaginatedHistoryAsync(
                tipseg, orgsin, rmosin, numsin, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((50, firstPageEntities));

        var firstPageDtos = CreateMockHistoryDtos(firstPageEntities);
        _mockMapper.Setup(m => m.Map<List<HistoryRecordDto>>(firstPageEntities))
            .Returns(firstPageDtos);

        // Act
        var result = await _claimService.GetClaimHistoryAsync(tipseg, orgsin, rmosin, numsin, page, pageSize);

        // Assert
        Assert.True(result.Sucesso);
        Assert.Equal(50, result.TotalRegistros);
        Assert.Equal(page, result.PaginaAtual);
        Assert.Equal(pageSize, result.TamanhoPagina);
        Assert.Equal(3, result.TotalPaginas); // ceiling(50/20) = 3
        Assert.Equal(20, result.Historico.Count);

        // Verify repository called with optimized method
        _mockClaimHistoryRepository.Verify(r => r.GetPaginatedHistoryAsync(
            tipseg, orgsin, rmosin, numsin, page, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetClaimHistoryAsync_WithLargeDataset_Returns1000PlusRecords()
    {
        // Arrange - T085 performance test scenario
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;
        var page = 1;
        var pageSize = 100; // Max page size
        var totalRecords = 1500; // Large dataset

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimMaster { Tipseg = tipseg, Orgsin = orgsin, Rmosin = rmosin, Numsin = numsin });

        var historyEntities = CreateMockHistoryRecords(tipseg, orgsin, rmosin, numsin, pageSize);
        var historyDtos = CreateMockHistoryDtos(historyEntities);

        _mockClaimHistoryRepository.Setup(r => r.GetPaginatedHistoryAsync(
                tipseg, orgsin, rmosin, numsin, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((totalRecords, historyEntities));

        _mockMapper.Setup(m => m.Map<List<HistoryRecordDto>>(historyEntities))
            .Returns(historyDtos);

        // Act
        var result = await _claimService.GetClaimHistoryAsync(tipseg, orgsin, rmosin, numsin, page, pageSize);

        // Assert
        Assert.True(result.Sucesso);
        Assert.Equal(totalRecords, result.TotalRegistros);
        Assert.Equal(15, result.TotalPaginas); // ceiling(1500/100) = 15
        Assert.Equal(pageSize, result.Historico.Count);
    }

    [Fact]
    public async Task GetClaimHistoryAsync_WithMiddlePage_ReturnsCorrectPage()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;
        var page = 3; // Middle page
        var pageSize = 20;
        var totalRecords = 100;

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimMaster { Tipseg = tipseg, Orgsin = orgsin, Rmosin = rmosin, Numsin = numsin });

        var historyEntities = CreateMockHistoryRecords(tipseg, orgsin, rmosin, numsin, pageSize);
        var historyDtos = CreateMockHistoryDtos(historyEntities);

        _mockClaimHistoryRepository.Setup(r => r.GetPaginatedHistoryAsync(
                tipseg, orgsin, rmosin, numsin, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((totalRecords, historyEntities));

        _mockMapper.Setup(m => m.Map<List<HistoryRecordDto>>(historyEntities))
            .Returns(historyDtos);

        // Act
        var result = await _claimService.GetClaimHistoryAsync(tipseg, orgsin, rmosin, numsin, page, pageSize);

        // Assert
        Assert.Equal(3, result.PaginaAtual);
        Assert.Equal(5, result.TotalPaginas); // ceiling(100/20) = 5
        Assert.Equal(20, result.Historico.Count);
    }

    [Fact]
    public async Task GetClaimHistoryAsync_WithLastPagePartial_ReturnsRemainingRecords()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;
        var page = 3; // Last page
        var pageSize = 20;
        var totalRecords = 45; // Last page has only 5 records
        var lastPageSize = 5;

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimMaster { Tipseg = tipseg, Orgsin = orgsin, Rmosin = rmosin, Numsin = numsin });

        var historyEntities = CreateMockHistoryRecords(tipseg, orgsin, rmosin, numsin, lastPageSize);
        var historyDtos = CreateMockHistoryDtos(historyEntities);

        _mockClaimHistoryRepository.Setup(r => r.GetPaginatedHistoryAsync(
                tipseg, orgsin, rmosin, numsin, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((totalRecords, historyEntities));

        _mockMapper.Setup(m => m.Map<List<HistoryRecordDto>>(historyEntities))
            .Returns(historyDtos);

        // Act
        var result = await _claimService.GetClaimHistoryAsync(tipseg, orgsin, rmosin, numsin, page, pageSize);

        // Assert
        Assert.Equal(3, result.PaginaAtual);
        Assert.Equal(3, result.TotalPaginas); // ceiling(45/20) = 3
        Assert.Equal(5, result.Historico.Count); // Partial last page
    }

    [Fact]
    public async Task GetClaimHistoryAsync_WithEmptyHistory_ReturnsEmptyList()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimMaster { Tipseg = tipseg, Orgsin = orgsin, Rmosin = rmosin, Numsin = numsin });

        _mockClaimHistoryRepository.Setup(r => r.GetPaginatedHistoryAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, new List<ClaimHistory>()));

        _mockMapper.Setup(m => m.Map<List<HistoryRecordDto>>(It.IsAny<List<ClaimHistory>>()))
            .Returns(new List<HistoryRecordDto>());

        // Act
        var result = await _claimService.GetClaimHistoryAsync(tipseg, orgsin, rmosin, numsin, 1, 20);

        // Assert
        Assert.True(result.Sucesso);
        Assert.Equal(0, result.TotalRegistros);
        Assert.Equal(0, result.TotalPaginas);
        Assert.Empty(result.Historico);
    }

    [Fact]
    public async Task GetClaimHistoryAsync_WithNonExistentClaim_ThrowsClaimNotFoundException()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 99999;

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimMaster?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ClaimNotFoundException>(() =>
            _claimService.GetClaimHistoryAsync(tipseg, orgsin, rmosin, numsin, 1, 20));
    }

    [Theory]
    [InlineData(0, 20)]   // Invalid page (< 1)
    [InlineData(-1, 20)]  // Negative page
    [InlineData(1, 0)]    // Invalid pageSize (< 1)
    [InlineData(1, -10)]  // Negative pageSize
    public async Task GetClaimHistoryAsync_WithInvalidPagination_HandlesGracefully(int page, int pageSize)
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimMaster { Tipseg = tipseg, Orgsin = orgsin, Rmosin = rmosin, Numsin = numsin });

        // Repository should normalize invalid values
        _mockClaimHistoryRepository.Setup(r => r.GetPaginatedHistoryAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((10, CreateMockHistoryRecords(tipseg, orgsin, rmosin, numsin, 10)));

        _mockMapper.Setup(m => m.Map<List<HistoryRecordDto>>(It.IsAny<List<ClaimHistory>>()))
            .Returns((List<ClaimHistory> entities) => CreateMockHistoryDtos(entities));

        // Act
        var result = await _claimService.GetClaimHistoryAsync(tipseg, orgsin, rmosin, numsin, page, pageSize);

        // Assert - Should not throw, repository normalizes values
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetClaimHistoryAsync_VerifiesNoN1QueryIssues()
    {
        // Arrange - T085 verification: single query for both count and data
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;

        _mockClaimRepository.Setup(r => r.SearchByClaimNumberAsync(orgsin, rmosin, numsin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimMaster { Tipseg = tipseg, Orgsin = orgsin, Rmosin = rmosin, Numsin = numsin });

        var historyEntities = CreateMockHistoryRecords(tipseg, orgsin, rmosin, numsin, 20);
        _mockClaimHistoryRepository.Setup(r => r.GetPaginatedHistoryAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((100, historyEntities));

        _mockMapper.Setup(m => m.Map<List<HistoryRecordDto>>(It.IsAny<List<ClaimHistory>>()))
            .Returns(CreateMockHistoryDtos(historyEntities));

        // Act
        await _claimService.GetClaimHistoryAsync(tipseg, orgsin, rmosin, numsin, 1, 20);

        // Assert - GetPaginatedHistoryAsync called exactly once (not separate count query)
        _mockClaimHistoryRepository.Verify(r => r.GetPaginatedHistoryAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);

        // Verify no separate FindAsync or CountAsync calls
        _mockClaimHistoryRepository.Verify(r => r.FindAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ClaimHistory, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _mockClaimHistoryRepository.Verify(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ClaimHistory, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    // Helper methods
    private List<ClaimHistory> CreateMockHistoryRecords(int tipseg, int orgsin, int rmosin, int numsin, int count)
    {
        var records = new List<ClaimHistory>();
        for (int i = count; i > 0; i--) // Descending order
        {
            records.Add(new ClaimHistory
            {
                Tipseg = tipseg,
                Orgsin = orgsin,
                Rmosin = rmosin,
                Numsin = numsin,
                Ocorhist = i,
                Operacao = 1098,
                Dtmovto = DateTime.Today.AddDays(-i),
                Horaoper = "143000",
                Valpri = 1000.00m + i,
                Crrmon = 0.00m,
                Nomfav = $"Beneficiario {i}",
                Tipcrr = "5",
                Valpribt = 1000.00m + i,
                Crrmonbt = 0.00m,
                Valtotbt = 1000.00m + i,
                Sitcontb = "0",
                Situacao = "0",
                Ezeusrid = "TESTUSER"
            });
        }
        return records;
    }

    private List<HistoryRecordDto> CreateMockHistoryDtos(List<ClaimHistory> entities)
    {
        return entities.Select(e => new HistoryRecordDto
        {
            Tipseg = e.Tipseg,
            Orgsin = e.Orgsin,
            Rmosin = e.Rmosin,
            Numsin = e.Numsin,
            Ocorhist = e.Ocorhist,
            Operacao = e.Operacao,
            Dtmovto = e.Dtmovto,
            Horaoper = new TimeSpan(14, 30, 0),
            Valpri = e.Valpri,
            Crrmon = e.Crrmon,
            Nomfav = e.Nomfav,
            Tipcrr = e.Tipcrr,
            Valpribt = e.Valpribt,
            Crrmonbt = e.Crrmonbt,
            Valtotbt = e.Valtotbt,
            Sitcontb = e.Sitcontb,
            Situacao = e.Situacao,
            Ezeusrid = e.Ezeusrid
        }).ToList();
    }
}
