using System;
using System.Collections.Generic;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Api.Tests;

/// <summary>
/// Base class for integration tests providing in-memory database setup and test data seeding.
/// Creates a fresh database instance for each test to ensure isolation.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected ClaimsDbContext DbContext { get; set; }
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new test instance with in-memory database
    /// </summary>
    protected IntegrationTestBase()
    {
        DbContext = CreateInMemoryDbContext();
        SeedTestData();
    }

    /// <summary>
    /// Creates a ClaimsDbContext with InMemoryDatabase provider
    /// Uses a unique database name per test instance to ensure isolation
    /// </summary>
    /// <returns>Configured DbContext instance</returns>
    protected ClaimsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        return new ClaimsDbContext(options, httpContextAccessor: null);
    }

    /// <summary>
    /// Seeds essential test data for integration tests
    /// Includes: ClaimMaster, BranchMaster, CurrencyUnit, SystemControl, PolicyMaster
    /// </summary>
    protected virtual void SeedTestData()
    {
        // 1. BranchMaster - Must be seeded first (referenced by other entities)
        var branches = new List<BranchMaster>
        {
            new BranchMaster
            {
                Rmosin = 1,
                Nomeramo = "Test Branch 1",
                Descramo = "Primary test branch",
                Ativo = true,
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            },
            new BranchMaster
            {
                Rmosin = 5,
                Nomeramo = "Test Branch 5",
                Descramo = "Secondary test branch",
                Ativo = true,
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            }
        };
        DbContext.BranchMasters.AddRange(branches);

        // 2. PolicyMaster - Must be seeded before ClaimMaster
        var policies = new List<PolicyMaster>
        {
            new PolicyMaster
            {
                Orgapo = 10,
                Rmoapo = 5,
                Numapol = 12345,
                Nome = "Test Policy Holder",
                Cpfcnpj = "12345678901",
                Dtinivig = DateTime.Today.AddYears(-1),
                Dtfimvig = DateTime.Today.AddYears(1),
                Situacao = "A",
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            },
            new PolicyMaster
            {
                Orgapo = 10,
                Rmoapo = 1,
                Numapol = 67890,
                Nome = "Consortium Policy Holder",
                Cpfcnpj = "98765432100",
                Dtinivig = DateTime.Today.AddYears(-1),
                Dtfimvig = DateTime.Today.AddYears(1),
                Situacao = "A",
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            }
        };
        DbContext.PolicyMasters.AddRange(policies);

        // 3. SystemControl - Business date configuration
        var systemControls = new List<SystemControl>
        {
            new SystemControl
            {
                Idsistem = "SI",
                Dtmovabe = DateTime.Today,
                Nomesist = "Claims System",
                Ativo = true,
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            }
        };
        DbContext.SystemControls.AddRange(systemControls);

        // 4. CurrencyUnit - Currency conversion rates
        var currencyUnits = new List<CurrencyUnit>
        {
            new CurrencyUnit
            {
                Dtinivig = DateTime.Today.AddYears(-2),
                Dttervig = DateTime.Today.AddYears(2),
                Vlcruzad = 1.00m,
                Codmoeda = "BRL",
                Nomemoeda = "Real Brasileiro",
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            },
            new CurrencyUnit
            {
                Dtinivig = DateTime.Today.AddYears(-5),
                Dttervig = DateTime.Today.AddYears(-2),
                Vlcruzad = 0.95m,
                Codmoeda = "BRL",
                Nomemoeda = "Real Brasileiro (Historical)",
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            }
        };
        DbContext.CurrencyUnits.AddRange(currencyUnits);

        // 5. ClaimMaster - Primary test claims
        var claims = new List<ClaimMaster>
        {
            // Standard claim for basic tests
            new ClaimMaster
            {
                Tipseg = 1,
                Orgsin = 1,
                Rmosin = 1,
                Numsin = 1,
                Fonte = 1,
                Protsini = 111111,
                Dac = 5,
                Orgapo = 10,
                Rmoapo = 5,
                Numapol = 12345,
                Codprodu = 1001,
                Sdopag = 50000.00m,
                Totpag = 10000.00m,
                Ocorhist = 0,
                Tipreg = "1",
                Tpsegu = 1,
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            },
            // Consortium product claim (product code 6814)
            new ClaimMaster
            {
                Tipseg = 1,
                Orgsin = 2,
                Rmosin = 1,
                Numsin = 1,
                Fonte = 1,
                Protsini = 222222,
                Dac = 3,
                Orgapo = 10,
                Rmoapo = 1,
                Numapol = 67890,
                Codprodu = 6814, // Consortium product
                Sdopag = 75000.00m,
                Totpag = 20000.00m,
                Ocorhist = 0,
                Tipreg = "1",
                Tpsegu = 0, // Insurance type 0 = optional beneficiary
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            },
            // Claim with existing history
            new ClaimMaster
            {
                Tipseg = 1,
                Orgsin = 3,
                Rmosin = 1,
                Numsin = 1,
                Fonte = 1,
                Protsini = 333333,
                Dac = 7,
                Orgapo = 10,
                Rmoapo = 5,
                Numapol = 12345,
                Codprodu = 2001,
                Sdopag = 100000.00m,
                Totpag = 30000.00m,
                Ocorhist = 2,
                Tipreg = "1",
                Tpsegu = 1,
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            }
        };
        DbContext.ClaimMasters.AddRange(claims);

        // 6. ClaimHistory - Sample history records for claim 3
        var histories = new List<ClaimHistory>
        {
            new ClaimHistory
            {
                Tipseg = 1,
                Orgsin = 3,
                Rmosin = 1,
                Numsin = 1,
                Ocorhist = 1,
                Operacao = 1098,
                Dtmovto = DateTime.Today.AddDays(-30),
                Horaoper = "143000",
                Valpri = 15000.00m,
                Valpribt = 15000.00m,
                Valtotbt = 15000.00m,
                Sitcontb = "C",
                Situacao = "APROVADO",
                Nomfav = "Test Beneficiary 1",
                Ezeusrid = "TESTUSER1",
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            },
            new ClaimHistory
            {
                Tipseg = 1,
                Orgsin = 3,
                Rmosin = 1,
                Numsin = 1,
                Ocorhist = 2,
                Operacao = 1098,
                Dtmovto = DateTime.Today.AddDays(-15),
                Horaoper = "101500",
                Valpri = 15000.00m,
                Valpribt = 15000.00m,
                Valtotbt = 15000.00m,
                Sitcontb = "C",
                Situacao = "APROVADO",
                Nomfav = "Test Beneficiary 2",
                Ezeusrid = "TESTUSER2",
                CreatedBy = "TestSeeder",
                CreatedAt = DateTime.UtcNow
            }
        };
        DbContext.ClaimHistories.AddRange(histories);

        // 7. MigrationStatus - Dashboard tracking data
        var migrationStatus1 = new MigrationStatus
        {
            Id = Guid.NewGuid(),
            UserStoryCode = "US-001",
            UserStoryName = "Claim Search",
            Status = "Completed",
            CompletionPercentage = 100,
            RequirementsCompleted = 10,
            RequirementsTotal = 10,
            TestsPassed = 15,
            TestsTotal = 15,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var migrationStatus2 = new MigrationStatus
        {
            Id = Guid.NewGuid(),
            UserStoryCode = "US-002",
            UserStoryName = "Payment Authorization",
            Status = "In Progress",
            CompletionPercentage = 75,
            RequirementsCompleted = 6,
            RequirementsTotal = 8,
            TestsPassed = 12,
            TestsTotal = 16,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            UpdatedAt = DateTime.UtcNow.AddHours(-2)
        };

        DbContext.MigrationStatuses.AddRange(new[] { migrationStatus1, migrationStatus2 });

        // 8. ComponentMigration - Component-level tracking
        var componentMigrations = new List<ComponentMigration>
        {
            new ComponentMigration
            {
                Id = Guid.NewGuid(),
                MigrationStatusId = migrationStatus1.Id,
                ComponentType = "Screen",
                ComponentName = "Claim Search Screen",
                Status = "Completed",
                EstimatedHours = 40,
                ActualHours = 38,
                Complexity = "Medium",
                CreatedAt = DateTime.UtcNow.AddDays(-9),
                UpdatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new ComponentMigration
            {
                Id = Guid.NewGuid(),
                MigrationStatusId = migrationStatus2.Id,
                ComponentType = "BusinessRule",
                ComponentName = "Payment Validation",
                Status = "In Progress",
                EstimatedHours = 32,
                ActualHours = 28,
                Complexity = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };
        DbContext.ComponentMigrations.AddRange(componentMigrations);

        // 9. PerformanceMetric - Performance tracking
        var performanceMetrics = new List<PerformanceMetric>
        {
            new PerformanceMetric
            {
                Id = Guid.NewGuid(),
                ComponentId = componentMigrations[0].Id,
                MetricType = "Response Time",
                LegacyValue = 3.5m,
                NewValue = 2.5m,
                Unit = "seconds",
                MeasurementTimestamp = DateTime.UtcNow.AddDays(-1),
                PassFail = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new PerformanceMetric
            {
                Id = Guid.NewGuid(),
                ComponentId = componentMigrations[1].Id,
                MetricType = "Authorization Time",
                LegacyValue = 95.0m,
                NewValue = 85.0m,
                Unit = "seconds",
                MeasurementTimestamp = DateTime.UtcNow.AddHours(-6),
                PassFail = true,
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                UpdatedAt = DateTime.UtcNow.AddHours(-6)
            }
        };
        DbContext.PerformanceMetrics.AddRange(performanceMetrics);

        // Save all changes
        DbContext.SaveChanges();
    }

    /// <summary>
    /// Helper method to create additional test claims
    /// </summary>
    protected ClaimMaster CreateTestClaim(
        int tipseg = 1,
        int orgsin = 100,
        int rmosin = 1,
        int numsin = 100,
        int fonte = 1,
        int protsini = 999999,
        int dac = 9,
        decimal sdopag = 10000.00m,
        decimal totpag = 0.00m,
        int codprodu = 1001,
        int tpsegu = 1)
    {
        var claim = new ClaimMaster
        {
            Tipseg = tipseg,
            Orgsin = orgsin,
            Rmosin = rmosin,
            Numsin = numsin,
            Fonte = fonte,
            Protsini = protsini,
            Dac = dac,
            Orgapo = 10,
            Rmoapo = 5,
            Numapol = 12345,
            Codprodu = codprodu,
            Sdopag = sdopag,
            Totpag = totpag,
            Ocorhist = 0,
            Tipreg = "1",
            Tpsegu = tpsegu,
            CreatedBy = "TestHelper",
            CreatedAt = DateTime.UtcNow
        };

        DbContext.ClaimMasters.Add(claim);
        DbContext.SaveChanges();

        return claim;
    }

    /// <summary>
    /// Helper method to create test claim history
    /// </summary>
    protected ClaimHistory CreateTestHistory(
        int tipseg,
        int orgsin,
        int rmosin,
        int numsin,
        int ocorhist,
        decimal valpri = 5000.00m)
    {
        var history = new ClaimHistory
        {
            Tipseg = tipseg,
            Orgsin = orgsin,
            Rmosin = rmosin,
            Numsin = numsin,
            Ocorhist = ocorhist,
            Operacao = 1098,
            Dtmovto = DateTime.Today,
            Horaoper = DateTime.Now.ToString("HHmmss"),
            Valpri = valpri,
            Valpribt = valpri,
            Valtotbt = valpri,
            Sitcontb = "C",
            Situacao = "APROVADO",
            Nomfav = "Test Beneficiary",
            Ezeusrid = "TESTHELPER",
            CreatedBy = "TestHelper",
            CreatedAt = DateTime.UtcNow
        };

        DbContext.ClaimHistories.Add(history);
        DbContext.SaveChanges();

        return history;
    }

    /// <summary>
    /// Dispose pattern implementation
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DbContext?.Dispose();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Public dispose method
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
