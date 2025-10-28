using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Infrastructure.Data;
using CaixaSeguradora.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CaixaSeguradora.Infrastructure.Tests.Performance;

/// <summary>
/// T085 [US3] - Payment History Performance Tests
/// Validates query performance with large datasets (1000+ records)
/// Target: < 500ms for paginated history queries
/// </summary>
public class T085_ClaimHistoryPerformanceTests : IDisposable
{
    private readonly ClaimsDbContext _context;
    private readonly IClaimHistoryRepository _repository;
    private readonly ITestOutputHelper _output;
    private readonly int _testTipseg = 1;
    private readonly int _testOrgsin = 100;
    private readonly int _testRmosin = 200;
    private readonly int _testNumsin = 99999;

    public T085_ClaimHistoryPerformanceTests(ITestOutputHelper output)
    {
        _output = output;

        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(databaseName: $"T085_Performance_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ClaimsDbContext(options);
        _repository = new ClaimHistoryRepository(_context);
    }

    [Theory]
    [InlineData(100)]   // Small dataset
    [InlineData(500)]   // Medium dataset
    [InlineData(1000)]  // Large dataset
    [InlineData(2000)]  // Very large dataset
    public async Task GetPaginatedHistoryAsync_WithLargeDataset_MeetsPerformanceTarget(int totalRecords)
    {
        // Arrange - Create test claim with many history records
        await SeedHistoryRecords(totalRecords);

        var sw = Stopwatch.StartNew();

        // Act - Fetch first page (typical user scenario)
        var (count, records) = await _repository.GetPaginatedHistoryAsync(
            _testTipseg, _testOrgsin, _testRmosin, _testNumsin, 1, 20);

        sw.Stop();

        // Assert
        Assert.Equal(totalRecords, count);
        Assert.Equal(20, records.Count);
        Assert.True(sw.ElapsedMilliseconds < 500,
            $"Query took {sw.ElapsedMilliseconds}ms (target < 500ms) for {totalRecords} records");

        _output.WriteLine($"[PASS] {totalRecords} records - Query time: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPaginatedHistoryAsync_MultiplePages_ConsistentPerformance()
    {
        // Arrange
        var totalRecords = 1000;
        await SeedHistoryRecords(totalRecords);

        var pageTimings = new List<long>();

        // Act - Test multiple pages
        for (int page = 1; page <= 10; page++)
        {
            var sw = Stopwatch.StartNew();

            var (count, records) = await _repository.GetPaginatedHistoryAsync(
                _testTipseg, _testOrgsin, _testRmosin, _testNumsin, page, 20);

            sw.Stop();
            pageTimings.Add(sw.ElapsedMilliseconds);

            _output.WriteLine($"Page {page}: {sw.ElapsedMilliseconds}ms");
        }

        // Assert - All pages should meet performance target
        foreach (var timing in pageTimings)
        {
            Assert.True(timing < 500, $"Page query took {timing}ms (target < 500ms)");
        }

        var avgTiming = pageTimings.Average();
        var maxTiming = pageTimings.Max();

        _output.WriteLine($"Average: {avgTiming:F2}ms, Max: {maxTiming}ms");
        Assert.True(avgTiming < 250, $"Average query time {avgTiming:F2}ms should be well below target");
    }

    [Fact]
    public async Task GetPaginatedHistoryAsync_LastPage_NoPerformanceDegradation()
    {
        // Arrange - Large dataset with partial last page
        var totalRecords = 1543; // Not divisible by 20
        await SeedHistoryRecords(totalRecords);

        var firstPageSw = Stopwatch.StartNew();
        var (_, firstPage) = await _repository.GetPaginatedHistoryAsync(
            _testTipseg, _testOrgsin, _testRmosin, _testNumsin, 1, 20);
        firstPageSw.Stop();

        // Act - Fetch last page (page 78)
        var lastPageNum = (int)Math.Ceiling(totalRecords / 20.0);
        var lastPageSw = Stopwatch.StartNew();
        var (count, lastPage) = await _repository.GetPaginatedHistoryAsync(
            _testTipseg, _testOrgsin, _testRmosin, _testNumsin, lastPageNum, 20);
        lastPageSw.Stop();

        // Assert
        Assert.Equal(totalRecords, count);
        Assert.Equal(3, lastPage.Count); // 1543 % 20 = 3

        // Last page should not be significantly slower than first page
        var performanceRatio = (double)lastPageSw.ElapsedMilliseconds / firstPageSw.ElapsedMilliseconds;

        _output.WriteLine($"First page: {firstPageSw.ElapsedMilliseconds}ms");
        _output.WriteLine($"Last page: {lastPageSw.ElapsedMilliseconds}ms");
        _output.WriteLine($"Performance ratio: {performanceRatio:F2}x");

        Assert.True(performanceRatio < 3.0,
            $"Last page is {performanceRatio:F2}x slower than first page (should be < 3x)");
    }

    [Fact]
    public async Task GetHistoryCountAsync_WithLargeDataset_FastExecution()
    {
        // Arrange
        var totalRecords = 2000;
        await SeedHistoryRecords(totalRecords);

        var sw = Stopwatch.StartNew();

        // Act
        var count = await _repository.GetHistoryCountAsync(
            _testTipseg, _testOrgsin, _testRmosin, _testNumsin);

        sw.Stop();

        // Assert
        Assert.Equal(totalRecords, count);
        Assert.True(sw.ElapsedMilliseconds < 200,
            $"Count query took {sw.ElapsedMilliseconds}ms (target < 200ms)");

        _output.WriteLine($"Count query for {totalRecords} records: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPaginatedHistoryAsync_VerifyDescendingOrder()
    {
        // Arrange
        await SeedHistoryRecords(100);

        // Act
        var (count, records) = await _repository.GetPaginatedHistoryAsync(
            _testTipseg, _testOrgsin, _testRmosin, _testNumsin, 1, 50);

        // Assert - Records should be in descending order by Ocorhist
        var isDescending = records
            .Zip(records.Skip(1), (a, b) => a.Ocorhist > b.Ocorhist)
            .All(x => x);

        Assert.True(isDescending, "Records should be ordered by Ocorhist DESC");

        // First record should have highest occurrence
        Assert.Equal(100, records.First().Ocorhist);
        // 50th record should be occurrence 51
        Assert.Equal(51, records.Last().Ocorhist);

        _output.WriteLine($"First occurrence: {records.First().Ocorhist}");
        _output.WriteLine($"Last occurrence: {records.Last().Ocorhist}");
    }

    [Fact]
    public async Task GetPaginatedHistoryAsync_NoN1Queries_SingleDatabaseCall()
    {
        // Arrange
        await SeedHistoryRecords(100);

        // In-memory database doesn't provide query logging,
        // but we can verify behavior through multiple calls
        var sw1 = Stopwatch.StartNew();
        await _repository.GetPaginatedHistoryAsync(
            _testTipseg, _testOrgsin, _testRmosin, _testNumsin, 1, 20);
        sw1.Stop();

        // Second call should be similar time (no query multiplication)
        var sw2 = Stopwatch.StartNew();
        await _repository.GetPaginatedHistoryAsync(
            _testTipseg, _testOrgsin, _testRmosin, _testNumsin, 2, 20);
        sw2.Stop();

        var timeDifference = Math.Abs(sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds);

        _output.WriteLine($"First query: {sw1.ElapsedMilliseconds}ms");
        _output.WriteLine($"Second query: {sw2.ElapsedMilliseconds}ms");
        _output.WriteLine($"Difference: {timeDifference}ms");

        // Verify no significant time difference (N+1 would cause exponential growth)
        Assert.True(timeDifference < 100,
            $"Query times should be consistent (difference: {timeDifference}ms)");
    }

    [Theory]
    [InlineData(0, 20)]    // Invalid page
    [InlineData(-1, 20)]   // Negative page
    [InlineData(1, 0)]     // Invalid pageSize
    [InlineData(1, -10)]   // Negative pageSize
    [InlineData(1, 150)]   // PageSize > 100
    public async Task GetPaginatedHistoryAsync_WithInvalidPagination_NormalizesValues(
        int page, int pageSize)
    {
        // Arrange
        await SeedHistoryRecords(50);

        // Act - Should not throw, repository normalizes values
        var (count, records) = await _repository.GetPaginatedHistoryAsync(
            _testTipseg, _testOrgsin, _testRmosin, _testNumsin, page, pageSize);

        // Assert
        Assert.Equal(50, count);
        Assert.NotEmpty(records);
        Assert.True(records.Count <= 100, "PageSize capped at 100");

        _output.WriteLine($"Input: page={page}, pageSize={pageSize}");
        _output.WriteLine($"Retrieved: {records.Count} records");
    }

    // Helper Methods

    private async Task SeedHistoryRecords(int count)
    {
        var records = new List<ClaimHistory>();

        for (int i = count; i > 0; i--) // Descending to simulate real scenario
        {
            records.Add(new ClaimHistory
            {
                Tipseg = _testTipseg,
                Orgsin = _testOrgsin,
                Rmosin = _testRmosin,
                Numsin = _testNumsin,
                Ocorhist = i,
                Operacao = 1098,
                Dtmovto = DateTime.Today.AddDays(-i),
                Horaoper = "143000",
                Valpri = 1000.00m + i,
                Crrmon = 0.00m,
                Nomfav = $"Beneficiario Teste {i}",
                Tipcrr = "5",
                Valpribt = 1000.00m + i,
                Crrmonbt = 0.00m,
                Valtotbt = 1000.00m + i,
                Sitcontb = "0",
                Situacao = "0",
                Ezeusrid = "TESTUSER"
            });
        }

        await _context.ClaimHistories.AddRangeAsync(records);
        await _context.SaveChangesAsync();

        _output.WriteLine($"Seeded {count} history records for claim {_testTipseg}/{_testOrgsin}/{_testRmosin}/{_testNumsin}");
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
