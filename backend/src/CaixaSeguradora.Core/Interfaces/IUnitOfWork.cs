using System;
using System.Threading;
using System.Threading.Tasks;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// Unit of Work pattern interface
/// Coordinates the work of multiple repositories and maintains transaction consistency
/// </summary>
public interface IUnitOfWork : IDisposable
{
    #region Repository Properties

    /// <summary>
    /// Repository for ClaimMaster entities with specialized claim queries
    /// </summary>
    IClaimRepository Claims { get; }

    /// <summary>
    /// Repository for BranchMaster entities
    /// </summary>
    IRepository<BranchMaster> BranchMasters { get; }

    /// <summary>
    /// Repository for PolicyMaster entities
    /// </summary>
    IRepository<PolicyMaster> PolicyMasters { get; }

    /// <summary>
    /// Repository for CurrencyUnit entities
    /// </summary>
    IRepository<CurrencyUnit> CurrencyUnits { get; }

    /// <summary>
    /// Repository for SystemControl entities
    /// </summary>
    IRepository<SystemControl> SystemControls { get; }

    /// <summary>
    /// Repository for ClaimHistory entities
    /// </summary>
    IRepository<ClaimHistory> ClaimHistories { get; }

    /// <summary>
    /// Repository for ClaimAccompaniment entities
    /// </summary>
    IRepository<ClaimAccompaniment> ClaimAccompaniments { get; }

    /// <summary>
    /// Repository for ClaimPhase entities
    /// </summary>
    IRepository<ClaimPhase> ClaimPhases { get; }

    /// <summary>
    /// Repository for PhaseEventRelationship entities
    /// </summary>
    IRepository<PhaseEventRelationship> PhaseEventRelationships { get; }

    /// <summary>
    /// Repository for ConsortiumContract entities
    /// </summary>
    IRepository<ConsortiumContract> ConsortiumContracts { get; }

    /// <summary>
    /// Repository for MigrationStatus entities
    /// </summary>
    IRepository<MigrationStatus> MigrationStatuses { get; }

    /// <summary>
    /// Repository for ComponentMigration entities
    /// </summary>
    IRepository<ComponentMigration> ComponentMigrations { get; }

    /// <summary>
    /// Repository for PerformanceMetric entities
    /// </summary>
    IRepository<PerformanceMetric> PerformanceMetrics { get; }

    #endregion

    #region Transaction Methods

    /// <summary>
    /// Saves all pending changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction and saves all changes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction, discarding all changes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    #endregion
}
