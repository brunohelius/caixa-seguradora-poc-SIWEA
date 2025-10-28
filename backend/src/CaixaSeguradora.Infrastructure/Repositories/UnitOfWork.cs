using System;
using System.Threading;
using System.Threading.Tasks;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Interfaces;
using CaixaSeguradora.Infrastructure.Data;

namespace CaixaSeguradora.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation
/// Coordinates the work of multiple repositories and maintains transaction consistency
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ClaimsDbContext _context;
    private bool _disposed;

    // Lazy-initialized repositories
    private IClaimRepository? _claims;
    private IRepository<BranchMaster>? _branchMasters;
    private IRepository<PolicyMaster>? _policyMasters;
    private IRepository<CurrencyUnit>? _currencyUnits;
    private IRepository<SystemControl>? _systemControls;
    private IClaimHistoryRepository? _claimHistories;
    private IRepository<ClaimAccompaniment>? _claimAccompaniments;
    private IRepository<ClaimPhase>? _claimPhases;
    private IRepository<PhaseEventRelationship>? _phaseEventRelationships;
    private IRepository<ConsortiumContract>? _consortiumContracts;
    private IRepository<MigrationStatus>? _migrationStatuses;
    private IRepository<ComponentMigration>? _componentMigrations;
    private IRepository<PerformanceMetric>? _performanceMetrics;

    /// <summary>
    /// Initializes a new instance of UnitOfWork
    /// </summary>
    /// <param name="context">Database context</param>
    public UnitOfWork(ClaimsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Repository Properties

    /// <summary>
    /// Repository for ClaimMaster entities with specialized claim queries
    /// </summary>
    public IClaimRepository Claims
    {
        get
        {
            _claims ??= new ClaimRepository(_context);
            return _claims;
        }
    }

    /// <summary>
    /// Repository for BranchMaster entities
    /// </summary>
    public IRepository<BranchMaster> BranchMasters
    {
        get
        {
            _branchMasters ??= new Repository<BranchMaster>(_context);
            return _branchMasters;
        }
    }

    /// <summary>
    /// Repository for PolicyMaster entities
    /// </summary>
    public IRepository<PolicyMaster> PolicyMasters
    {
        get
        {
            _policyMasters ??= new Repository<PolicyMaster>(_context);
            return _policyMasters;
        }
    }

    /// <summary>
    /// Repository for CurrencyUnit entities
    /// </summary>
    public IRepository<CurrencyUnit> CurrencyUnits
    {
        get
        {
            _currencyUnits ??= new Repository<CurrencyUnit>(_context);
            return _currencyUnits;
        }
    }

    /// <summary>
    /// Repository for SystemControl entities
    /// </summary>
    public IRepository<SystemControl> SystemControls
    {
        get
        {
            _systemControls ??= new Repository<SystemControl>(_context);
            return _systemControls;
        }
    }

    /// <summary>
    /// Repository for ClaimHistory entities with optimized pagination (T085 [US3])
    /// </summary>
    public IClaimHistoryRepository ClaimHistories
    {
        get
        {
            _claimHistories ??= new ClaimHistoryRepository(_context);
            return _claimHistories;
        }
    }

    /// <summary>
    /// Repository for ClaimAccompaniment entities
    /// </summary>
    public IRepository<ClaimAccompaniment> ClaimAccompaniments
    {
        get
        {
            _claimAccompaniments ??= new Repository<ClaimAccompaniment>(_context);
            return _claimAccompaniments;
        }
    }

    /// <summary>
    /// Repository for ClaimPhase entities
    /// </summary>
    public IRepository<ClaimPhase> ClaimPhases
    {
        get
        {
            _claimPhases ??= new Repository<ClaimPhase>(_context);
            return _claimPhases;
        }
    }

    /// <summary>
    /// Repository for PhaseEventRelationship entities
    /// </summary>
    public IRepository<PhaseEventRelationship> PhaseEventRelationships
    {
        get
        {
            _phaseEventRelationships ??= new Repository<PhaseEventRelationship>(_context);
            return _phaseEventRelationships;
        }
    }

    /// <summary>
    /// Repository for ConsortiumContract entities
    /// </summary>
    public IRepository<ConsortiumContract> ConsortiumContracts
    {
        get
        {
            _consortiumContracts ??= new Repository<ConsortiumContract>(_context);
            return _consortiumContracts;
        }
    }

    /// <summary>
    /// Repository for MigrationStatus entities
    /// </summary>
    public IRepository<MigrationStatus> MigrationStatuses
    {
        get
        {
            _migrationStatuses ??= new Repository<MigrationStatus>(_context);
            return _migrationStatuses;
        }
    }

    /// <summary>
    /// Repository for ComponentMigration entities
    /// </summary>
    public IRepository<ComponentMigration> ComponentMigrations
    {
        get
        {
            _componentMigrations ??= new Repository<ComponentMigration>(_context);
            return _componentMigrations;
        }
    }

    /// <summary>
    /// Repository for PerformanceMetric entities
    /// </summary>
    public IRepository<PerformanceMetric> PerformanceMetrics
    {
        get
        {
            _performanceMetrics ??= new Repository<PerformanceMetric>(_context);
            return _performanceMetrics;
        }
    }

    #endregion

    #region Transaction Methods

    /// <summary>
    /// Gets whether a transaction is currently active
    /// </summary>
    public bool HasActiveTransaction => _context.HasActiveTransaction;

    /// <summary>
    /// Saves all pending changes to the database
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.HasActiveTransaction)
        {
            return;
        }

        await _context.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction with specified isolation level
    /// </summary>
    public async Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (_context.HasActiveTransaction)
        {
            return;
        }

        // Call ClaimsDbContext.BeginTransactionAsync with isolation level
        await _context.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction and saves all changes
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (!_context.HasActiveTransaction)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        try
        {
            await _context.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Rolls back the current transaction, discarding all changes
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (!_context.HasActiveTransaction)
        {
            return;
        }

        await _context.RollbackAsync(cancellationToken);
    }

    #endregion

    #region Dispose Pattern

    /// <summary>
    /// Disposes the unit of work and the database context
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern
    /// </summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }
    }

    #endregion
}
