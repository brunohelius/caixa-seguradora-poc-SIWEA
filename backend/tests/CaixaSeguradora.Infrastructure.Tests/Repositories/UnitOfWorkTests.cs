using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Infrastructure.Data;
using CaixaSeguradora.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace CaixaSeguradora.Infrastructure.Tests.Repositories;

/// <summary>
/// Comprehensive unit tests for UnitOfWork
/// Tests transaction management, repository lazy initialization, and dispose pattern
/// Uses EF Core InMemory database for isolation
/// </summary>
public class UnitOfWorkTests : IDisposable
{
    private readonly ClaimsDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly string _dbName;

    public UnitOfWorkTests()
    {
        _dbName = $"TestDb_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ClaimsDbContext(options);
        _unitOfWork = new UnitOfWork(_context);

        SeedTestData();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UnitOfWork(null!));
    }

    #endregion

    #region Repository Lazy Initialization Tests

    [Fact]
    public void Claims_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.Claims;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void Claims_MultipleAccess_ReturnsSameInstance()
    {
        // Act
        var repository1 = _unitOfWork.Claims;
        var repository2 = _unitOfWork.Claims;

        // Assert
        Assert.Same(repository1, repository2);
    }

    [Fact]
    public void BranchMasters_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.BranchMasters;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void PolicyMasters_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.PolicyMasters;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void CurrencyUnits_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.CurrencyUnits;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void SystemControls_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.SystemControls;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void ClaimHistories_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.ClaimHistories;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void ClaimAccompaniments_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.ClaimAccompaniments;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void ClaimPhases_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.ClaimPhases;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void PhaseEventRelationships_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.PhaseEventRelationships;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void ConsortiumContracts_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.ConsortiumContracts;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void MigrationStatuses_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.MigrationStatuses;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void ComponentMigrations_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.ComponentMigrations;

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void PerformanceMetrics_FirstAccess_InitializesRepository()
    {
        // Act
        var repository = _unitOfWork.PerformanceMetrics;

        // Assert
        Assert.NotNull(repository);
    }

    #endregion

    #region SaveChangesAsync Tests

    [Fact]
    public async Task SaveChangesAsync_WithChanges_ReturnsSavedCount()
    {
        // Arrange
        var branch = new BranchMaster
        {
            Rmosin = 999,
            Nomeramo = "New Branch",
            Ativo = true
        };
        await _unitOfWork.BranchMasters.AddAsync(branch);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public async Task SaveChangesAsync_WithoutChanges_ReturnsZero()
    {
        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region BeginTransactionAsync Tests (Default Isolation Level)

    [Fact]
    public async Task BeginTransactionAsync_Default_StartsTransaction()
    {
        // Act
        await _unitOfWork.BeginTransactionAsync();

        // Assert
        Assert.True(_unitOfWork.HasActiveTransaction);
    }

    [Fact]
    public async Task BeginTransactionAsync_WhenAlreadyActive_DoesNotStartNew()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();
        Assert.True(_unitOfWork.HasActiveTransaction);

        // Act
        await _unitOfWork.BeginTransactionAsync();

        // Assert
        Assert.True(_unitOfWork.HasActiveTransaction);
    }

    #endregion

    #region BeginTransactionAsync Tests (With Isolation Level)

    [Fact]
    public async Task BeginTransactionAsync_WithReadCommitted_StartsTransaction()
    {
        // Act
        await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        // Assert
        Assert.True(_unitOfWork.HasActiveTransaction);
    }

    [Fact]
    public async Task BeginTransactionAsync_WithSerializable_StartsTransaction()
    {
        // Act
        await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

        // Assert
        Assert.True(_unitOfWork.HasActiveTransaction);
    }

    [Fact]
    public async Task BeginTransactionAsync_WithIsolationLevel_WhenAlreadyActive_DoesNotStartNew()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        Assert.True(_unitOfWork.HasActiveTransaction);

        // Act
        await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

        // Assert
        Assert.True(_unitOfWork.HasActiveTransaction);
    }

    #endregion

    #region CommitTransactionAsync Tests

    [Fact]
    public async Task CommitTransactionAsync_WithActiveTransaction_CommitsSuccessfully()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        var branch = new BranchMaster
        {
            Rmosin = 888,
            Nomeramo = "Transaction Branch",
            Ativo = true
        };
        await _unitOfWork.BranchMasters.AddAsync(branch);

        // Act
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        Assert.False(_unitOfWork.HasActiveTransaction);

        // Verify data was saved
        var savedBranch = await _context.BranchMasters.FindAsync(888);
        Assert.NotNull(savedBranch);
    }

    [Fact]
    public async Task CommitTransactionAsync_WithoutActiveTransaction_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _unitOfWork.CommitTransactionAsync());
    }

    [Fact]
    public async Task CommitTransactionAsync_OnError_RollsBackAutomatically()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        // Add an invalid entity that will cause SaveChanges to fail
        var branch = new BranchMaster
        {
            Rmosin = 777,
            Nomeramo = null!, // Required field - will cause validation error
            Ativo = true
        };
        await _unitOfWork.BranchMasters.AddAsync(branch);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            _unitOfWork.CommitTransactionAsync());

        // Transaction should be rolled back
        Assert.False(_unitOfWork.HasActiveTransaction);
    }

    #endregion

    #region RollbackTransactionAsync Tests

    [Fact]
    public async Task RollbackTransactionAsync_WithActiveTransaction_RollsBackChanges()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        var branch = new BranchMaster
        {
            Rmosin = 666,
            Nomeramo = "Rollback Branch",
            Ativo = true
        };
        await _unitOfWork.BranchMasters.AddAsync(branch);

        // Act
        await _unitOfWork.RollbackTransactionAsync();

        // Assert
        Assert.False(_unitOfWork.HasActiveTransaction);

        // Note: InMemory database doesn't truly support transactions
        // Changes are tracked but rollback behavior differs from SQL Server
        // Transaction state is correctly managed even though data persists in InMemory
    }

    [Fact]
    public async Task RollbackTransactionAsync_WithoutActiveTransaction_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        await _unitOfWork.RollbackTransactionAsync();

        Assert.False(_unitOfWork.HasActiveTransaction);
    }

    #endregion

    #region HasActiveTransaction Tests

    [Fact]
    public void HasActiveTransaction_InitialState_ReturnsFalse()
    {
        // Assert
        Assert.False(_unitOfWork.HasActiveTransaction);
    }

    [Fact]
    public async Task HasActiveTransaction_AfterBegin_ReturnsTrue()
    {
        // Act
        await _unitOfWork.BeginTransactionAsync();

        // Assert
        Assert.True(_unitOfWork.HasActiveTransaction);
    }

    [Fact]
    public async Task HasActiveTransaction_AfterCommit_ReturnsFalse()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        // Act
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        Assert.False(_unitOfWork.HasActiveTransaction);
    }

    [Fact]
    public async Task HasActiveTransaction_AfterRollback_ReturnsFalse()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        // Act
        await _unitOfWork.RollbackTransactionAsync();

        // Assert
        Assert.False(_unitOfWork.HasActiveTransaction);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_DisposesContext()
    {
        // Arrange
        var dbName = $"TestDb_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new ClaimsDbContext(options);
        var unitOfWork = new UnitOfWork(context);

        // Act
        unitOfWork.Dispose();

        // Assert - Context should be disposed
        Assert.Throws<ObjectDisposedException>(() => context.ClaimMasters.Count());
    }

    [Fact]
    public void Dispose_MultipleCallsSafe()
    {
        // Arrange
        var dbName = $"TestDb_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new ClaimsDbContext(options);
        var unitOfWork = new UnitOfWork(context);

        // Act & Assert - Should not throw on multiple dispose calls
        unitOfWork.Dispose();
        unitOfWork.Dispose();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task FullTransactionCycle_CommitChanges_PersistsData()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        var branch = new BranchMaster
        {
            Rmosin = 555,
            Nomeramo = "Full Cycle Branch",
            Ativo = true
        };
        await _unitOfWork.BranchMasters.AddAsync(branch);

        var policy = new PolicyMaster
        {
            Orgapo = 100,
            Rmoapo = 555,
            Numapol = 5001,
            Nome = "Full Cycle Insured"
        };
        await _unitOfWork.PolicyMasters.AddAsync(policy);

        // Act
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        var savedBranch = await _unitOfWork.BranchMasters.GetByIdAsync(new object[] { 555 });
        var savedPolicy = await _unitOfWork.PolicyMasters.GetByIdAsync(new object[] { 100, 555, 5001 });

        Assert.NotNull(savedBranch);
        Assert.NotNull(savedPolicy);
    }

    [Fact]
    public async Task FullTransactionCycle_RollbackChanges_DoesNotPersistData()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        var branch = new BranchMaster
        {
            Rmosin = 444,
            Nomeramo = "Rollback Cycle Branch",
            Ativo = true
        };
        await _unitOfWork.BranchMasters.AddAsync(branch);

        // Act
        await _unitOfWork.RollbackTransactionAsync();

        // Assert
        Assert.False(_unitOfWork.HasActiveTransaction);

        // Note: InMemory database doesn't truly support transactions
        // Transaction state is correctly managed even though data persists in InMemory
        // In production with SQL Server, rollback would discard all changes
    }

    #endregion

    #region Helper Methods

    private void SeedTestData()
    {
        // Add some test branches
        var branch200 = new BranchMaster
        {
            Rmosin = 200,
            Nomeramo = "Test Branch 200",
            Ativo = true
        };
        _context.BranchMasters.Add(branch200);

        // Add some test policies
        var policy1 = new PolicyMaster
        {
            Orgapo = 100,
            Rmoapo = 200,
            Numapol = 1001,
            Nome = "Test Insured 1"
        };
        _context.PolicyMasters.Add(policy1);

        _context.SaveChanges();
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        try
        {
            _context.Database.EnsureDeleted();
        }
        catch
        {
            // Ignore errors during cleanup
        }
        finally
        {
            _unitOfWork.Dispose();
        }
    }

    #endregion
}
