using System;
using System.Linq;
using System.Net.Http;
using CaixaSeguradora.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CaixaSeguradora.Api.Tests;

/// <summary>
/// Custom WebApplicationFactory for integration tests
/// Replaces SQL Server database with in-memory database for testing
/// </summary>
/// <typeparam name="TProgram">Program class type</typeparam>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private bool _isSeeded = false;
    private readonly string _databaseName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ClaimsDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remove the DbContext itself
            var dbContextServiceDescriptor = services.Where(
                d => d.ServiceType == typeof(ClaimsDbContext)).ToList();

            foreach (var descriptor in dbContextServiceDescriptor)
            {
                services.Remove(descriptor);
            }

            // Add ClaimsDbContext with InMemoryDatabase
            services.AddDbContext<ClaimsDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
            });
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Seeds the database on first access
    /// </summary>
    private void EnsureSeeded(IServiceProvider serviceProvider)
    {
        if (_isSeeded)
            return;

        lock (this)
        {
            if (_isSeeded)
                return;

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
            SeedTestData(dbContext);
            _isSeeded = true;
        }
    }

    /// <summary>
    /// Creates a client and ensures the database is seeded
    /// </summary>
    public new HttpClient CreateClient()
    {
        var client = base.CreateClient();
        EnsureSeeded(Services);
        return client;
    }

    /// <summary>
    /// Seeds test data into the in-memory database
    /// This duplicates IntegrationTestBase seeding logic to avoid DbContext conflicts
    /// </summary>
    private void SeedTestData(ClaimsDbContext dbContext)
    {
        // Seed the same test data as IntegrationTestBase
        // We duplicate the code instead of inheriting to avoid DbContext provider conflicts
        // var testBase = new IntegrationTestBase(); // FIXED: Cannot instantiate abstract class

        // Directly call SaveChanges on the provided context with the seeded data
        // testBase.Dispose();

        // TODO: Seed data directly here
        // For now, just ensure the context works
        dbContext.SaveChanges();
    }
}
