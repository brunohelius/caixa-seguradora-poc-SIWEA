using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CaixaSeguradora.Core.Tests.Services;

/// <summary>
/// T105 [US5] - Phase Management Service Unit Tests
/// Tests service business logic with mocked dependencies
/// </summary>
public class PhaseManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<PhaseManagementService>> _mockLogger;
    private readonly PhaseManagementService _service;
    private readonly Mock<IRepository<ClaimPhase>> _mockClaimPhaseRepository;
    private readonly Mock<IRepository<PhaseEventRelationship>> _mockPhaseEventRepository;

    public PhaseManagementServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<PhaseManagementService>>();
        _mockClaimPhaseRepository = new Mock<IRepository<ClaimPhase>>();
        _mockPhaseEventRepository = new Mock<IRepository<PhaseEventRelationship>>();

        _mockUnitOfWork.Setup(u => u.ClaimPhases).Returns(_mockClaimPhaseRepository.Object);
        _mockUnitOfWork.Setup(u => u.PhaseEventRelationships).Returns(_mockPhaseEventRepository.Object);

        _service = new PhaseManagementService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdatePhasesAsync_FindsMatchingRelationships()
    {
        // Arrange
        var fonte = 1;
        var protsini = 123456;
        var dac = 7;
        var codEvento = 1001;
        var dataMovto = DateTime.Today;
        var userId = "TEST_USER";

        var relationships = new List<PhaseEventRelationship>
        {
            new PhaseEventRelationship
            {
                CodFase = 10,
                CodEvento = codEvento,
                DataInivigRefaev = dataMovto.AddYears(-1),
                IndAlteracaoFase = "1", // Opening
                NomeFase = "Fase de Autorização",
                NomeEvento = "Pagamento Autorizado"
            },
            new PhaseEventRelationship
            {
                CodFase = 20,
                CodEvento = codEvento,
                DataInivigRefaev = dataMovto.AddYears(-1),
                IndAlteracaoFase = "2", // Closing
                NomeFase = "Fase de Análise",
                NomeEvento = "Análise Concluída"
            }
        };

        _mockPhaseEventRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PhaseEventRelationship, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(relationships);

        // For opening: no existing phase
        _mockClaimPhaseRepository
            .Setup(r => r.FindAsync(It.Is<Expression<Func<ClaimPhase, bool>>>(
                expr => expr.ToString().Contains("CodFase") && expr.ToString().Contains("10")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClaimPhase>());

        // For closing: find existing open phase
        var existingOpenPhase = new ClaimPhase
        {
            Fonte = fonte,
            Protsini = protsini,
            Dac = dac,
            CodFase = 20,
            CodEvento = codEvento,
            NumOcorrSiniaco = 0,
            DataInivigRefaev = dataMovto.AddYears(-1),
            DataAberturaSifa = dataMovto.AddDays(-30),
            DataFechaSifa = new DateTime(9999, 12, 31),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _mockClaimPhaseRepository
            .Setup(r => r.FindAsync(It.Is<Expression<Func<ClaimPhase, bool>>>(
                expr => expr.ToString().Contains("CodFase") && expr.ToString().Contains("20")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClaimPhase> { existingOpenPhase });

        // Act
        await _service.UpdatePhasesAsync(fonte, protsini, dac, codEvento, dataMovto, userId);

        // Assert
        _mockPhaseEventRepository.Verify(r => r.FindAsync(
            It.IsAny<Expression<Func<PhaseEventRelationship, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);

        // Verify phase opening was attempted
        _mockClaimPhaseRepository.Verify(r => r.AddAsync(
            It.Is<ClaimPhase>(p => p.CodFase == 10), It.IsAny<CancellationToken>()), Times.Once);

        // Verify phase closing was attempted
        _mockClaimPhaseRepository.Verify(r => r.UpdateAsync(
            It.Is<ClaimPhase>(p => p.CodFase == 20), It.IsAny<CancellationToken>()), Times.Once);

        // Verify SaveChanges was called
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePhaseOpeningAsync_SetsCorrectDates()
    {
        // Arrange
        var fonte = 1;
        var protsini = 123456;
        var dac = 7;
        var codFase = 10;
        var codEvento = 1001;
        var dataMovto = DateTime.Today;
        var userId = "TEST_USER";

        var newPhase = new ClaimPhase
        {
            Fonte = fonte,
            Protsini = protsini,
            Dac = dac,
            CodFase = codFase,
            CodEvento = codEvento,
            NumOcorrSiniaco = 0,
            DataInivigRefaev = dataMovto.AddYears(-1),
            DataAberturaSifa = dataMovto,
            DataFechaSifa = DateTime.MinValue, // Will be set by service
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        ClaimPhase capturedPhase = null!;
        _mockClaimPhaseRepository
            .Setup(r => r.AddAsync(It.IsAny<ClaimPhase>(), It.IsAny<CancellationToken>()))
            .Callback<ClaimPhase, CancellationToken>((p, ct) => capturedPhase = p)
            .ReturnsAsync((ClaimPhase p, CancellationToken ct) => p);

        // Act
        await _service.CreatePhaseOpeningAsync(newPhase);

        // Assert
        Assert.NotNull(capturedPhase);
        Assert.Equal(dataMovto, capturedPhase.DataAberturaSifa);
        Assert.Equal(new DateTime(9999, 12, 31), capturedPhase.DataFechaSifa); // Open phase marker
        Assert.True(capturedPhase.IsOpen);

        _mockClaimPhaseRepository.Verify(r => r.AddAsync(It.IsAny<ClaimPhase>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePhaseClosingAsync_FindsOpenPhase()
    {
        // Arrange
        var fonte = 1;
        var protsini = 123456;
        var dac = 7;
        var codFase = 20;
        var codEvento = 2001;
        var closingDate = DateTime.Today;
        var userId = "TEST_USER";

        var openPhase = new ClaimPhase
        {
            Fonte = fonte,
            Protsini = protsini,
            Dac = dac,
            CodFase = codFase,
            CodEvento = codEvento,
            NumOcorrSiniaco = 0,
            DataInivigRefaev = closingDate.AddYears(-1),
            DataAberturaSifa = closingDate.AddDays(-30),
            DataFechaSifa = new DateTime(9999, 12, 31), // Open
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        // Update the phase to close it
        openPhase.DataFechaSifa = closingDate;
        openPhase.UpdatedBy = userId;
        openPhase.UpdatedAt = DateTime.UtcNow;

        ClaimPhase capturedPhase = null!;
        _mockClaimPhaseRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ClaimPhase>(), It.IsAny<CancellationToken>()))
            .Callback<ClaimPhase, CancellationToken>((p, ct) => capturedPhase = p)
            .ReturnsAsync((ClaimPhase p, CancellationToken ct) => p);

        // Act
        await _service.UpdatePhaseClosingAsync(openPhase);

        // Assert
        Assert.NotNull(capturedPhase);
        Assert.Equal(closingDate, capturedPhase.DataFechaSifa);
        Assert.Equal(userId, capturedPhase.UpdatedBy);
        Assert.False(capturedPhase.IsOpen);
        Assert.NotNull(capturedPhase.DurationInDays);
        Assert.Equal(30, capturedPhase.DurationInDays);

        _mockClaimPhaseRepository.Verify(r => r.UpdateAsync(It.IsAny<ClaimPhase>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PreventDuplicatePhase_ReturnsTrue_WhenPhaseExists()
    {
        // Arrange
        var fonte = 1;
        var protsini = 123456;
        var dac = 7;
        var codFase = 10;
        var codEvento = 1001;

        var existingPhases = new List<ClaimPhase>
        {
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = codFase,
                CodEvento = codEvento,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-10),
                DataFechaSifa = new DateTime(9999, 12, 31), // Open
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockClaimPhaseRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClaimPhase, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPhases);

        // Act
        var result = await _service.PreventDuplicatePhase(fonte, protsini, dac, codFase, codEvento);

        // Assert
        Assert.True(result);
        _mockClaimPhaseRepository.Verify(r => r.FindAsync(
            It.IsAny<Expression<Func<ClaimPhase, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PreventDuplicatePhase_ReturnsFalse_WhenPhaseDoesNotExist()
    {
        // Arrange
        var fonte = 1;
        var protsini = 123456;
        var dac = 7;
        var codFase = 10;
        var codEvento = 1001;

        _mockClaimPhaseRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClaimPhase, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClaimPhase>()); // No existing phases

        // Act
        var result = await _service.PreventDuplicatePhase(fonte, protsini, dac, codFase, codEvento);

        // Assert
        Assert.False(result);
        _mockClaimPhaseRepository.Verify(r => r.FindAsync(
            It.IsAny<Expression<Func<ClaimPhase, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActivePhases_ReturnsOnlyOpenPhases()
    {
        // Arrange
        var fonte = 1;
        var protsini = 123456;
        var dac = 7;

        var allPhases = new List<ClaimPhase>
        {
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = 10,
                CodEvento = 1001,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-30),
                DataFechaSifa = new DateTime(9999, 12, 31), // Open
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            },
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = 20,
                CodEvento = 2001,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-60),
                DataFechaSifa = DateTime.Today.AddDays(-30), // Closed
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            },
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = 30,
                CodEvento = 3001,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-10),
                DataFechaSifa = new DateTime(9999, 12, 31), // Open
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockClaimPhaseRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClaimPhase, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(allPhases.Where(p => p.DataFechaSifa == new DateTime(9999, 12, 31)).ToList());

        // Act
        var activePhases = await _service.GetActivePhases(fonte, protsini, dac);

        // Assert
        Assert.Equal(2, activePhases.Count);
        Assert.All(activePhases, p => Assert.True(p.IsOpen));
        Assert.Contains(activePhases, p => p.CodFase == 10);
        Assert.Contains(activePhases, p => p.CodFase == 30);
        Assert.DoesNotContain(activePhases, p => p.CodFase == 20);
    }

    [Fact]
    public async Task GetAllPhasesAsync_ReturnsOrderedByDateDescending()
    {
        // Arrange
        var fonte = 1;
        var protsini = 123456;
        var dac = 7;

        var allPhases = new List<ClaimPhase>
        {
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = 10,
                CodEvento = 1001,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-60), // Oldest
                DataFechaSifa = DateTime.Today.AddDays(-30),
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            },
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = 20,
                CodEvento = 2001,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-10), // Most recent
                DataFechaSifa = new DateTime(9999, 12, 31),
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            },
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = 30,
                CodEvento = 3001,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-30), // Middle
                DataFechaSifa = DateTime.Today.AddDays(-15),
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockClaimPhaseRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClaimPhase, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(allPhases);

        // Act
        var result = await _service.GetAllPhasesAsync(fonte, protsini, dac);

        // Assert
        Assert.Equal(3, result.Count);
        // Should be ordered by DataAberturaSifa descending
        Assert.Equal(20, result[0].CodFase); // Most recent
        Assert.Equal(30, result[1].CodFase); // Middle
        Assert.Equal(10, result[2].CodFase); // Oldest
    }

    [Fact]
    public async Task GetPhaseStatisticsAsync_CalculatesCorrectStatistics()
    {
        // Arrange
        var fonte = 1;
        var protsini = 123456;
        var dac = 7;

        var allPhases = new List<ClaimPhase>
        {
            // Closed phase: 20 days duration
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = 10,
                CodEvento = 1001,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-100),
                DataFechaSifa = DateTime.Today.AddDays(-80),
                NomeFase = "Fase 1",
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            },
            // Closed phase: 40 days duration
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = 20,
                CodEvento = 2001,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-50),
                DataFechaSifa = DateTime.Today.AddDays(-10),
                NomeFase = "Fase 2",
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            },
            // Open phase: 15 days so far
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = 30,
                CodEvento = 3001,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-15),
                DataFechaSifa = new DateTime(9999, 12, 31),
                NomeFase = "Fase 3",
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            },
            // Open phase: 25 days so far (longest open)
            new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = 40,
                CodEvento = 4001,
                NumOcorrSiniaco = 0,
                DataInivigRefaev = DateTime.Today.AddYears(-1),
                DataAberturaSifa = DateTime.Today.AddDays(-25),
                DataFechaSifa = new DateTime(9999, 12, 31),
                NomeFase = "Fase 4",
                CreatedBy = "USER",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockClaimPhaseRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClaimPhase, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(allPhases);

        // Act
        var statistics = await _service.GetPhaseStatisticsAsync(fonte, protsini, dac);

        // Assert
        Assert.Equal(4, statistics.TotalPhases);
        Assert.Equal(2, statistics.OpenPhases);
        Assert.Equal(2, statistics.ClosedPhases);
        Assert.Equal(30.0, statistics.AverageDurationDays); // (20 + 40) / 2 = 30
        Assert.Equal(25, statistics.LongestOpenPhaseDays);
        Assert.Contains("Phase 40", statistics.LongestOpenPhaseName);
    }

    [Fact]
    public async Task UpdatePhasesAsync_NoRelationships_DoesNotCreatePhases()
    {
        // Arrange
        var fonte = 1;
        var protsini = 123456;
        var dac = 7;
        var codEvento = 9999; // Non-existent event
        var dataMovto = DateTime.Today;
        var userId = "TEST_USER";

        _mockPhaseEventRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PhaseEventRelationship, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PhaseEventRelationship>()); // No relationships

        // Act
        await _service.UpdatePhasesAsync(fonte, protsini, dac, codEvento, dataMovto, userId);

        // Assert
        _mockClaimPhaseRepository.Verify(r => r.AddAsync(It.IsAny<ClaimPhase>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockClaimPhaseRepository.Verify(r => r.UpdateAsync(It.IsAny<ClaimPhase>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
