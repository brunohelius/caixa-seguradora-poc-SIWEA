using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Infrastructure.Data;
using CaixaSeguradora.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CaixaSeguradora.Infrastructure.Tests.Repositories;

/// <summary>
/// Comprehensive unit tests for generic Repository<T>
/// Tests basic CRUD operations across different entity types
/// Uses EF Core InMemory database for isolation
/// </summary>
public class RepositoryTests : IDisposable
{
    private readonly ClaimsDbContext _context;
    private readonly Repository<BranchMaster> _branchRepository;
    private readonly Repository<PolicyMaster> _policyRepository;
    private readonly string _dbName;

    public RepositoryTests()
    {
        _dbName = $"TestDb_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .Options;

        _context = new ClaimsDbContext(options);
        _branchRepository = new Repository<BranchMaster>(_context);
        _policyRepository = new Repository<PolicyMaster>(_context);

        SeedTestData();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Repository<BranchMaster>(null!));
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidSingleKey_ReturnsEntity()
    {
        // Arrange
        var id = new object[] { 200 };

        // Act
        var result = await _branchRepository.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Rmosin);
        Assert.Equal("Test Branch 200", result.Nomeramo);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidCompositeKey_ReturnsEntity()
    {
        // Arrange
        var id = new object[] { 100, 200, 1001 };

        // Act
        var result = await _policyRepository.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Orgapo);
        Assert.Equal(200, result.Rmoapo);
        Assert.Equal(1001, result.Numapol);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var id = new object[] { 9999 };

        // Act
        var result = await _branchRepository.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithData_ReturnsAllEntities()
    {
        // Act
        var result = await _branchRepository.GetAllAsync();

        // Assert
        var branches = result.ToList();
        Assert.NotEmpty(branches);
        Assert.True(branches.Count >= 3); // At least our seeded data
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyTable_ReturnsEmptyCollection()
    {
        // Arrange
        var currencyRepository = new Repository<CurrencyUnit>(_context);

        // Act
        var result = await currencyRepository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region FindAsync Tests

    [Fact]
    public async Task FindAsync_WithMatchingPredicate_ReturnsEntities()
    {
        // Arrange - Find all active branches

        // Act
        var result = await _branchRepository.FindAsync(b => b.Ativo == true);

        // Assert
        var branches = result.ToList();
        Assert.NotEmpty(branches);
        Assert.All(branches, b => Assert.True(b.Ativo));
    }

    [Fact]
    public async Task FindAsync_WithNonMatchingPredicate_ReturnsEmpty()
    {
        // Arrange - Find inactive branches (all our test data is active)

        // Act
        var result = await _branchRepository.FindAsync(b => b.Ativo == false);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindAsync_WithComplexPredicate_ReturnsMatchingEntities()
    {
        // Arrange - Find policies with specific name pattern

        // Act
        var result = await _policyRepository.FindAsync(p => p.Nome.Contains("Test Insured"));

        // Assert
        var policies = result.ToList();
        Assert.NotEmpty(policies);
        Assert.All(policies, p => Assert.Contains("Test Insured", p.Nome));
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidEntity_AddsEntity()
    {
        // Arrange
        var newBranch = new BranchMaster
        {
            Rmosin = 999,
            Nomeramo = "New Branch",
            Descramo = "New Description",
            Ativo = true
        };

        // Act
        var result = await _branchRepository.AddAsync(newBranch);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(999, result.Rmosin);

        var savedBranch = await _context.BranchMasters.FindAsync(999);
        Assert.NotNull(savedBranch);
        Assert.Equal("New Branch", savedBranch.Nomeramo);
    }

    [Fact]
    public async Task AddAsync_WithInvalidEntity_ThrowsValidationException()
    {
        // Arrange - Entity with missing required field
        var invalidBranch = new BranchMaster
        {
            Rmosin = 998,
            Nomeramo = null!, // Required field
            Ativo = true
        };

        // Act
        await _branchRepository.AddAsync(invalidBranch);

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_WithMultipleEntities_AddsAll()
    {
        // Arrange
        var branches = new List<BranchMaster>
        {
            new BranchMaster { Rmosin = 801, Nomeramo = "Branch 801", Ativo = true },
            new BranchMaster { Rmosin = 802, Nomeramo = "Branch 802", Ativo = true },
            new BranchMaster { Rmosin = 803, Nomeramo = "Branch 803", Ativo = true }
        };

        // Act
        await _branchRepository.AddRangeAsync(branches);
        await _context.SaveChangesAsync();

        // Assert
        var branch801 = await _context.BranchMasters.FindAsync(801);
        var branch802 = await _context.BranchMasters.FindAsync(802);
        var branch803 = await _context.BranchMasters.FindAsync(803);

        Assert.NotNull(branch801);
        Assert.NotNull(branch802);
        Assert.NotNull(branch803);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidEntity_UpdatesEntity()
    {
        // Arrange
        var branch = await _context.BranchMasters.FindAsync(200);
        Assert.NotNull(branch);

        // Detach to avoid tracking conflicts
        _context.Entry(branch).State = EntityState.Detached;

        branch.Nomeramo = "Updated Branch Name";
        branch.Descramo = "Updated Description";

        // Act
        var result = await _branchRepository.UpdateAsync(branch);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);

        var updatedBranch = await _context.BranchMasters.FindAsync(200);
        Assert.NotNull(updatedBranch);
        Assert.Equal("Updated Branch Name", updatedBranch.Nomeramo);
        Assert.Equal("Updated Description", updatedBranch.Descramo);
    }

    [Fact]
    public async Task UpdateAsync_WithConcurrencyConflict_InMemoryDoesNotThrow()
    {
        // Arrange
        // Note: InMemory database does not support concurrency exceptions
        // This test demonstrates that InMemory allows concurrent updates
        var branch1 = await _context.BranchMasters.FindAsync(201);
        var branch2 = await _context.BranchMasters.FindAsync(201);

        Assert.NotNull(branch1);
        Assert.NotNull(branch2);

        // Update first instance
        branch1.Nomeramo = "First Update";
        _context.BranchMasters.Update(branch1);
        await _context.SaveChangesAsync();

        // Detach first instance
        _context.Entry(branch1).State = EntityState.Detached;

        // Update second instance (would be stale in real database)
        branch2.Nomeramo = "Second Update";
        _context.BranchMasters.Update(branch2);

        // Act & Assert - InMemory doesn't throw concurrency exception
        await _context.SaveChangesAsync();
        Assert.Equal("Second Update", branch2.Nomeramo);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingEntity_DeletesEntity()
    {
        // Arrange
        var branch = await _context.BranchMasters.FindAsync(202);
        Assert.NotNull(branch);

        // Detach to avoid tracking conflicts
        _context.Entry(branch).State = EntityState.Detached;

        // Act
        await _branchRepository.DeleteAsync(branch);
        await _context.SaveChangesAsync();

        // Assert
        var deletedBranch = await _context.BranchMasters.FindAsync(202);
        Assert.Null(deletedBranch);
    }

    #endregion

    #region DeleteRangeAsync Tests

    [Fact]
    public async Task DeleteRangeAsync_WithMatchingPredicate_DeletesEntities()
    {
        // Arrange - Delete all branches with Rmosin > 300

        // Act
        var deletedCount = await _branchRepository.DeleteRangeAsync(b => b.Rmosin > 300);
        await _context.SaveChangesAsync();

        // Assert
        Assert.True(deletedCount >= 2); // At least branches 301 and 302

        var branch301 = await _context.BranchMasters.FindAsync(301);
        var branch302 = await _context.BranchMasters.FindAsync(302);

        Assert.Null(branch301);
        Assert.Null(branch302);
    }

    [Fact]
    public async Task DeleteRangeAsync_WithNonMatchingPredicate_ReturnsZero()
    {
        // Arrange - Try to delete branches with Rmosin > 9999 (none exist)

        // Act
        var deletedCount = await _branchRepository.DeleteRangeAsync(b => b.Rmosin > 9999);

        // Assert
        Assert.Equal(0, deletedCount);
    }

    #endregion

    #region CountAsync Tests

    [Fact]
    public async Task CountAsync_WithoutPredicate_ReturnsTotal()
    {
        // Act
        var count = await _branchRepository.CountAsync();

        // Assert
        Assert.True(count >= 5); // At least our seeded data
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsMatchingCount()
    {
        // Arrange - Count active branches

        // Act
        var count = await _branchRepository.CountAsync(b => b.Ativo == true);

        // Assert
        Assert.True(count >= 5); // All our test branches are active
    }

    [Fact]
    public async Task CountAsync_WithNullPredicate_ReturnsTotal()
    {
        // Act
        var count = await _branchRepository.CountAsync(null);

        // Assert
        Assert.True(count >= 5);
    }

    #endregion

    #region AnyAsync Tests

    [Fact]
    public async Task AnyAsync_WithMatchingPredicate_ReturnsTrue()
    {
        // Arrange

        // Act
        var exists = await _branchRepository.AnyAsync(b => b.Rmosin == 200);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_WithNonMatchingPredicate_ReturnsFalse()
    {
        // Arrange

        // Act
        var exists = await _branchRepository.AnyAsync(b => b.Rmosin == 9999);

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region Cross-Entity Type Tests

    [Fact]
    public async Task Repository_WorksWithDifferentEntityTypes()
    {
        // Test BranchMaster (single key)
        var branch = await _branchRepository.GetByIdAsync(new object[] { 200 });
        Assert.NotNull(branch);

        // Test PolicyMaster (composite key)
        var policy = await _policyRepository.GetByIdAsync(new object[] { 100, 200, 1001 });
        Assert.NotNull(policy);

        // Both repositories work independently
        Assert.NotSame(branch, policy);
    }

    #endregion

    #region Helper Methods

    private void SeedTestData()
    {
        // Add Branch Masters
        var branches = new List<BranchMaster>
        {
            new BranchMaster { Rmosin = 200, Nomeramo = "Test Branch 200", Descramo = "Description 200", Ativo = true },
            new BranchMaster { Rmosin = 201, Nomeramo = "Test Branch 201", Descramo = "Description 201", Ativo = true },
            new BranchMaster { Rmosin = 202, Nomeramo = "Test Branch 202", Descramo = "Description 202", Ativo = true },
            new BranchMaster { Rmosin = 301, Nomeramo = "Test Branch 301", Descramo = "Description 301", Ativo = true },
            new BranchMaster { Rmosin = 302, Nomeramo = "Test Branch 302", Descramo = "Description 302", Ativo = true }
        };
        _context.BranchMasters.AddRange(branches);

        // Add Policy Masters
        var policies = new List<PolicyMaster>
        {
            new PolicyMaster
            {
                Orgapo = 100,
                Rmoapo = 200,
                Numapol = 1001,
                Nome = "Test Insured 1",
                Cpfcnpj = "12345678901",
                Dtinivig = DateTime.Today.AddYears(-1),
                Dtfimvig = DateTime.Today.AddYears(1),
                Situacao = "A"
            },
            new PolicyMaster
            {
                Orgapo = 100,
                Rmoapo = 200,
                Numapol = 1002,
                Nome = "Test Insured 2",
                Cpfcnpj = "98765432109",
                Dtinivig = DateTime.Today.AddYears(-1),
                Dtfimvig = DateTime.Today.AddYears(1),
                Situacao = "A"
            },
            new PolicyMaster
            {
                Orgapo = 100,
                Rmoapo = 201,
                Numapol = 1003,
                Nome = "Test Insured 3",
                Cpfcnpj = "11122233344",
                Dtinivig = DateTime.Today.AddYears(-1),
                Dtfimvig = DateTime.Today.AddYears(1),
                Situacao = "A"
            }
        };
        _context.PolicyMasters.AddRange(policies);

        _context.SaveChanges();
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #endregion
}
