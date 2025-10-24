using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data;

/// <summary>
/// Database context for the Claims System
/// Manages all entity sets and provides transaction support with audit trail functionality
/// </summary>
public class ClaimsDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private IDbContextTransaction? _currentTransaction;

    /// <summary>
    /// Initializes a new instance of the ClaimsDbContext
    /// </summary>
    /// <param name="options">DbContext configuration options</param>
    /// <param name="httpContextAccessor">HTTP context accessor for audit fields (optional)</param>
    public ClaimsDbContext(
        DbContextOptions<ClaimsDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    #region DbSet Properties - All 13 Entities

    /// <summary>
    /// Claim Master records (TMESTSIN)
    /// </summary>
    public DbSet<ClaimMaster> ClaimMasters { get; set; } = null!;

    /// <summary>
    /// Claim History records (THISTSIN)
    /// </summary>
    public DbSet<ClaimHistory> ClaimHistories { get; set; } = null!;

    /// <summary>
    /// Branch Master records (TGERAMO)
    /// </summary>
    public DbSet<BranchMaster> BranchMasters { get; set; } = null!;

    /// <summary>
    /// Currency Unit records (TMUNMOED)
    /// </summary>
    public DbSet<CurrencyUnit> CurrencyUnits { get; set; } = null!;

    /// <summary>
    /// System Control records (TCONTRASI)
    /// </summary>
    public DbSet<SystemControl> SystemControls { get; set; } = null!;

    /// <summary>
    /// Policy Master records (TAPOLICE)
    /// </summary>
    public DbSet<PolicyMaster> PolicyMasters { get; set; } = null!;

    /// <summary>
    /// Claim Accompaniment records (TACOMPASI)
    /// </summary>
    public DbSet<ClaimAccompaniment> ClaimAccompaniments { get; set; } = null!;

    /// <summary>
    /// Claim Phase records (TFASESIN)
    /// </summary>
    public DbSet<ClaimPhase> ClaimPhases { get; set; } = null!;

    /// <summary>
    /// Phase Event Relationship records (TFAEVENTO)
    /// </summary>
    public DbSet<PhaseEventRelationship> PhaseEventRelationships { get; set; } = null!;

    /// <summary>
    /// Consortium Contract records (TCONCONT)
    /// </summary>
    public DbSet<ConsortiumContract> ConsortiumContracts { get; set; } = null!;

    /// <summary>
    /// Migration Status records
    /// </summary>
    public DbSet<MigrationStatus> MigrationStatuses { get; set; } = null!;

    /// <summary>
    /// Component Migration records
    /// </summary>
    public DbSet<ComponentMigration> ComponentMigrations { get; set; } = null!;

    /// <summary>
    /// Performance Metrics records
    /// </summary>
    public DbSet<PerformanceMetric> PerformanceMetrics { get; set; } = null!;

    #endregion

    #region Model Configuration

    /// <summary>
    /// Configures entity mappings using Fluent API
    /// </summary>
    /// <param name="modelBuilder">Model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);
    }

    #endregion

    #region SaveChanges with Audit Trail

    /// <summary>
    /// Saves all changes with automatic audit field population
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        var timestamp = DateTime.UtcNow;

        // Process audit fields for all tracked entities
        foreach (var entry in ChangeTracker.Entries())
        {
            // Handle entities with audit fields
            if (entry.Entity is ClaimMaster ||
                entry.Entity is ClaimHistory ||
                entry.Entity is BranchMaster ||
                entry.Entity is CurrencyUnit ||
                entry.Entity is SystemControl ||
                entry.Entity is PolicyMaster ||
                entry.Entity is ClaimAccompaniment ||
                entry.Entity is ClaimPhase ||
                entry.Entity is PhaseEventRelationship ||
                entry.Entity is ConsortiumContract ||
                entry.Entity is MigrationStatus ||
                entry.Entity is ComponentMigration ||
                entry.Entity is PerformanceMetric)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        SetPropertyValue(entry, "CreatedBy", currentUserId);
                        SetPropertyValue(entry, "CreatedAt", timestamp);
                        SetPropertyValue(entry, "UpdatedBy", currentUserId);
                        SetPropertyValue(entry, "UpdatedAt", timestamp);
                        break;

                    case EntityState.Modified:
                        SetPropertyValue(entry, "UpdatedBy", currentUserId);
                        SetPropertyValue(entry, "UpdatedAt", timestamp);
                        break;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Sets a property value if the property exists on the entity
    /// </summary>
    private static void SetPropertyValue(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
        string propertyName,
        object? value)
    {
        var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);
        if (property != null)
        {
            property.CurrentValue = value;
        }
    }

    /// <summary>
    /// Gets the current user ID from HTTP context
    /// </summary>
    /// <returns>User ID or "System" if not available</returns>
    private string GetCurrentUserId()
    {
        try
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // Try to get user ID from claims
                var userId = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.FindFirst("userId")?.Value
                    ?? httpContext.User.Identity.Name;

                return userId ?? "System";
            }
        }
        catch
        {
            // Fallback to System if any error occurs
        }

        return "System";
    }

    #endregion

    #region Transaction Management

    /// <summary>
    /// Gets the current active transaction
    /// </summary>
    public IDbContextTransaction? CurrentTransaction => _currentTransaction;

    /// <summary>
    /// Indicates whether a transaction is currently active
    /// </summary>
    public bool HasActiveTransaction => _currentTransaction != null;

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The database transaction instance</returns>
    public async Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return _currentTransaction;
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    #endregion

    #region Disposal

    /// <summary>
    /// Disposes the context and any active transactions
    /// </summary>
    public override void Dispose()
    {
        _currentTransaction?.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Asynchronously disposes the context and any active transactions
    /// </summary>
    public override async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
        }
        await base.DisposeAsync();
    }

    #endregion
}
