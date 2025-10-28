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
/// Comprehensive unit tests for ClaimRepository
/// Tests all specialized query methods and basic CRUD operations
/// Uses EF Core InMemory database for isolation
/// </summary>
public class ClaimRepositoryTests : IDisposable
{
    private readonly ClaimsDbContext _context;
    private readonly ClaimRepository _repository;
    private readonly string _dbName;

    public ClaimRepositoryTests()
    {
        _dbName = $"TestDb_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .Options;

        _context = new ClaimsDbContext(options);
        _repository = new ClaimRepository(_context);

        SeedTestData();
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidCompositeKey_ReturnsClaim()
    {
        // Arrange
        var id = new object[] { 1, 100, 200, 12345 };

        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Tipseg);
        Assert.Equal(100, result.Orgsin);
        Assert.Equal(200, result.Rmosin);
        Assert.Equal(12345, result.Numsin);
        Assert.NotNull(result.Branch);
        Assert.NotNull(result.Policy);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var id = new object[] { 9, 999, 999, 99999 };

        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithNullId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetByIdAsync(null!));
    }

    [Fact]
    public async Task GetByIdAsync_WithIncorrectKeyCount_ThrowsArgumentException()
    {
        // Arrange
        var id = new object[] { 1, 100 }; // Only 2 components instead of 4

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetByIdAsync(id));

        Assert.Contains("4 key components", exception.Message);
    }

    #endregion

    #region SearchByProtocolAsync Tests

    [Fact]
    public async Task SearchByProtocolAsync_WithValidProtocol_ReturnsClaim()
    {
        // Arrange
        var fonte = 1;
        var protsini = 1001;
        var dac = 5;

        // Act
        var result = await _repository.SearchByProtocolAsync(fonte, protsini, dac);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fonte, result.Fonte);
        Assert.Equal(protsini, result.Protsini);
        Assert.Equal(dac, result.Dac);
        Assert.NotNull(result.Branch);
        Assert.NotNull(result.Policy);
    }

    [Fact]
    public async Task SearchByProtocolAsync_WithNonExistentProtocol_ReturnsNull()
    {
        // Arrange
        var fonte = 9;
        var protsini = 9999;
        var dac = 9;

        // Act
        var result = await _repository.SearchByProtocolAsync(fonte, protsini, dac);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchByProtocolAsync_WithInvalidDAC_ReturnsNull()
    {
        // Arrange
        var fonte = 1;
        var protsini = 1001;
        var dac = 3; // Wrong DAC for this protocol

        // Act
        var result = await _repository.SearchByProtocolAsync(fonte, protsini, dac);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchByProtocolAsync_WithInvalidFonte_ReturnsNull()
    {
        // Arrange
        var fonte = 99; // Invalid fonte
        var protsini = 1001;
        var dac = 5;

        // Act
        var result = await _repository.SearchByProtocolAsync(fonte, protsini, dac);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region SearchByClaimNumberAsync Tests

    [Fact]
    public async Task SearchByClaimNumberAsync_WithValidClaimNumber_ReturnsClaim()
    {
        // Arrange
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;

        // Act
        var result = await _repository.SearchByClaimNumberAsync(orgsin, rmosin, numsin);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orgsin, result.Orgsin);
        Assert.Equal(rmosin, result.Rmosin);
        Assert.Equal(numsin, result.Numsin);
        Assert.NotNull(result.Branch);
        Assert.NotNull(result.Policy);
    }

    [Fact]
    public async Task SearchByClaimNumberAsync_WithNonExistentClaimNumber_ReturnsNull()
    {
        // Arrange
        var orgsin = 999;
        var rmosin = 999;
        var numsin = 99999;

        // Act
        var result = await _repository.SearchByClaimNumberAsync(orgsin, rmosin, numsin);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchByClaimNumberAsync_WithInvalidOrgsin_ReturnsNull()
    {
        // Arrange
        var orgsin = 0; // Invalid
        var rmosin = 200;
        var numsin = 12345;

        // Act
        var result = await _repository.SearchByClaimNumberAsync(orgsin, rmosin, numsin);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region SearchByLeaderCodeAsync Tests

    [Fact]
    public async Task SearchByLeaderCodeAsync_WithValidLeaderCode_ReturnsClaims()
    {
        // Arrange
        var codlider = 500;
        var sinlid = 7001;

        // Act
        var result = await _repository.SearchByLeaderCodeAsync(codlider, sinlid);

        // Assert
        Assert.NotNull(result);
        var claims = result.ToList();
        Assert.NotEmpty(claims);
        Assert.All(claims, c => Assert.Equal(codlider, c.Codlider));
        Assert.All(claims, c => Assert.Equal(sinlid, c.Sinlid));
    }

    [Fact]
    public async Task SearchByLeaderCodeAsync_WithNonExistentLeaderCode_ReturnsEmpty()
    {
        // Arrange
        var codlider = 999;
        var sinlid = 9999;

        // Act
        var result = await _repository.SearchByLeaderCodeAsync(codlider, sinlid);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchByLeaderCodeAsync_WithMultipleClaims_ReturnsAllMatching()
    {
        // Arrange
        var codlider = 500;
        var sinlid = 7001;

        // Add another claim with same leader code
        var claim = CreateTestClaim(1, 100, 200, 54321, 1, 1003, 7);
        claim.Codlider = codlider;
        claim.Sinlid = sinlid;
        await _context.ClaimMasters.AddAsync(claim);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByLeaderCodeAsync(codlider, sinlid);

        // Assert
        var claims = result.ToList();
        Assert.Equal(2, claims.Count);
        Assert.All(claims, c => Assert.Equal(codlider, c.Codlider));
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllClaims()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var claims = result.ToList();
        Assert.NotEmpty(claims);
        Assert.True(claims.Count >= 3); // At least our seeded data
    }

    #endregion

    #region FindAsync Tests

    [Fact]
    public async Task FindAsync_WithPredicate_ReturnsMatchingClaims()
    {
        // Arrange - Find all claims with product code 6814 (consortium)
        var productCode = 6814;

        // Act
        var result = await _repository.FindAsync(c => c.Codprodu == productCode);

        // Assert
        var claims = result.ToList();
        Assert.NotEmpty(claims);
        Assert.All(claims, c => Assert.Equal(productCode, c.Codprodu));
    }

    [Fact]
    public async Task FindAsync_WithNoMatches_ReturnsEmpty()
    {
        // Arrange
        var productCode = 9999; // Non-existent

        // Act
        var result = await _repository.FindAsync(c => c.Codprodu == productCode);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidClaim_AddsClaim()
    {
        // Arrange
        var newClaim = CreateTestClaim(1, 100, 200, 99999, 1, 9999, 1);

        // Act
        var result = await _repository.AddAsync(newClaim);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(99999, result.Numsin);

        var savedClaim = await _context.ClaimMasters.FindAsync(1, 100, 200, 99999);
        Assert.NotNull(savedClaim);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidClaim_UpdatesClaim()
    {
        // Arrange
        var claim = await _context.ClaimMasters.FindAsync(1, 100, 200, 12345);
        Assert.NotNull(claim);
        var originalTotpag = claim.Totpag;

        // Detach to avoid tracking conflicts
        _context.Entry(claim).State = EntityState.Detached;

        // Create a new instance with updated values
        claim.Totpag = 5000.00m;

        // Act
        await _repository.UpdateAsync(claim);
        await _context.SaveChangesAsync();

        // Assert
        var updatedClaim = await _repository.SearchByClaimNumberAsync(100, 200, 12345);
        Assert.NotNull(updatedClaim);
        Assert.Equal(5000.00m, updatedClaim.Totpag);
        Assert.NotEqual(originalTotpag, updatedClaim.Totpag);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingClaim_DeletesClaim()
    {
        // Arrange
        var claim = await _context.ClaimMasters.FindAsync(1, 100, 200, 23456);
        Assert.NotNull(claim);

        // Detach to avoid tracking conflicts
        _context.Entry(claim).State = EntityState.Detached;

        // Act
        await _repository.DeleteAsync(claim);
        await _context.SaveChangesAsync();

        // Assert
        var deletedClaim = await _repository.SearchByClaimNumberAsync(100, 200, 23456);
        Assert.Null(deletedClaim);
    }

    #endregion

    #region CountAsync Tests

    [Fact]
    public async Task CountAsync_WithoutPredicate_ReturnsTotal()
    {
        // Act
        var count = await _repository.CountAsync();

        // Assert
        Assert.True(count >= 3); // At least our seeded data
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsMatchingCount()
    {
        // Arrange - Count consortium products
        var productCode = 6814;

        // Act
        var count = await _repository.CountAsync(c => c.Codprodu == productCode);

        // Assert
        Assert.True(count >= 1);
    }

    #endregion

    #region AnyAsync Tests

    [Fact]
    public async Task AnyAsync_WithMatchingPredicate_ReturnsTrue()
    {
        // Arrange
        var productCode = 6814;

        // Act
        var exists = await _repository.AnyAsync(c => c.Codprodu == productCode);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_WithNonMatchingPredicate_ReturnsFalse()
    {
        // Arrange
        var productCode = 9999;

        // Act
        var exists = await _repository.AnyAsync(c => c.Codprodu == productCode);

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region IncrementOcorhistAsync Tests

    [Fact]
    public async Task IncrementOcorhistAsync_WithValidClaim_IncrementsCounter()
    {
        // Arrange
        var tipseg = 1;
        var orgsin = 100;
        var rmosin = 200;
        var numsin = 12345;

        var claim = await _repository.SearchByClaimNumberAsync(orgsin, rmosin, numsin);
        Assert.NotNull(claim);
        var originalOcorhist = claim.Ocorhist;

        // Act
        var newOcorhist = await _repository.IncrementOcorhistAsync(tipseg, orgsin, rmosin, numsin);

        // Assert
        Assert.Equal(originalOcorhist + 1, newOcorhist);

        // Verify persistence
        var updatedClaim = await _repository.SearchByClaimNumberAsync(orgsin, rmosin, numsin);
        Assert.NotNull(updatedClaim);
        Assert.Equal(newOcorhist, updatedClaim.Ocorhist);
    }

    [Fact]
    public async Task IncrementOcorhistAsync_WithNonExistentClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        var tipseg = 9;
        var orgsin = 999;
        var rmosin = 999;
        var numsin = 99999;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _repository.IncrementOcorhistAsync(tipseg, orgsin, rmosin, numsin));
    }

    #endregion

    #region Helper Methods

    private void SeedTestData()
    {
        // Add Branch Masters
        var branch200 = new BranchMaster
        {
            Rmosin = 200,
            Nomeramo = "Test Branch 200",
            Descramo = "Test Description",
            Ativo = true
        };
        _context.BranchMasters.Add(branch200);

        // Add Policy Masters
        var policy1 = new PolicyMaster
        {
            Orgapo = 100,
            Rmoapo = 200,
            Numapol = 1001,
            Nome = "Test Insured 1",
            Cpfcnpj = "12345678901",
            Dtinivig = DateTime.Today.AddYears(-1),
            Dtfimvig = DateTime.Today.AddYears(1),
            Situacao = "A"
        };

        var policy2 = new PolicyMaster
        {
            Orgapo = 100,
            Rmoapo = 200,
            Numapol = 1002,
            Nome = "Test Insured 2",
            Cpfcnpj = "98765432109",
            Dtinivig = DateTime.Today.AddYears(-1),
            Dtfimvig = DateTime.Today.AddYears(1),
            Situacao = "A"
        };

        var policy3 = new PolicyMaster
        {
            Orgapo = 100,
            Rmoapo = 200,
            Numapol = 1003,
            Nome = "Test Insured 3",
            Cpfcnpj = "11122233344",
            Dtinivig = DateTime.Today.AddYears(-1),
            Dtfimvig = DateTime.Today.AddYears(1),
            Situacao = "A"
        };

        _context.PolicyMasters.AddRange(policy1, policy2, policy3);

        // Add Claim Masters
        var claim1 = CreateTestClaim(1, 100, 200, 12345, 1, 1001, 5);
        claim1.Codprodu = 6814; // Consortium product
        claim1.Codlider = 500;
        claim1.Sinlid = 7001;

        var claim2 = CreateTestClaim(1, 100, 200, 23456, 1, 1002, 7);
        claim2.Codprodu = 1234; // Regular product

        var claim3 = CreateTestClaim(1, 100, 200, 34567, 1, 1003, 9);
        claim3.Codprodu = 7701; // Consortium product

        _context.ClaimMasters.AddRange(claim1, claim2, claim3);
        _context.SaveChanges();
    }

    private ClaimMaster CreateTestClaim(int tipseg, int orgsin, int rmosin, int numsin, int fonte, int protsini, int dac)
    {
        return new ClaimMaster
        {
            Tipseg = tipseg,
            Orgsin = orgsin,
            Rmosin = rmosin,
            Numsin = numsin,
            Fonte = fonte,
            Protsini = protsini,
            Dac = dac,
            Orgapo = 100,
            Rmoapo = 200,
            Numapol = protsini,
            Codprodu = 1234,
            Sdopag = 10000.00m,
            Totpag = 2000.00m,
            Ocorhist = 1,
            Tipreg = "1",
            Tpsegu = 0
        };
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
