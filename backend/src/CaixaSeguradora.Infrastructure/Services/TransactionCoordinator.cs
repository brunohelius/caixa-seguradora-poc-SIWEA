using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Enums;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Infrastructure.Services;

/// <summary>
/// T032: TransactionCoordinator service for managing atomic database transactions
/// with ReadCommitted isolation level and complete rollback on any failure
///
/// Provides centralized transaction management for 4-step payment authorization pipeline:
/// 1. Insert THISTSIN (ClaimHistory)
/// 2. Update TMESTSIN (ClaimMaster)
/// 3. Insert SI_ACOMPANHA_SINI (ClaimAccompaniment)
/// 4. Update SI_SINISTRO_FASE (ClaimPhase)
///
/// Features:
/// - Transaction isolation: ReadCommitted
/// - Rollback behavior: Complete (all-or-nothing)
/// - Step tracking: TransactionContext progression
/// - Logging: Structured logging for begin/commit/rollback with step identification
/// - Error handling: Captures exception details and failed step
/// </summary>
public class TransactionCoordinator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionCoordinator> _logger;

    public TransactionCoordinator(
        IUnitOfWork unitOfWork,
        ILogger<TransactionCoordinator> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Begins a new database transaction with ReadCommitted isolation level
    /// </summary>
    /// <param name="context">Transaction context for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public async Task BeginTransactionAsync(
        TransactionContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.IsValid())
        {
            throw new InvalidOperationException(
                "Cannot begin transaction with invalid TransactionContext");
        }

        _logger.LogInformation(
            "TransactionCoordinator: BEGIN - AuthId={AuthId}, ClaimKey={ClaimKey}, Step={Step}",
            context.AuthorizationId, context.ClaimKey, context.Step);

        // Begin transaction with ReadCommitted isolation level
        await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction after successful completion of all steps
    /// </summary>
    /// <param name="context">Transaction context (must be in Committed step)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public async Task CommitTransactionAsync(
        TransactionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Step != TransactionStep.Committed)
        {
            throw new InvalidOperationException(
                $"Cannot commit transaction in step {context.Step}. Must be in Committed step.");
        }

        if (context.RollbackReason != null)
        {
            throw new InvalidOperationException(
                $"Cannot commit transaction marked for rollback: {context.RollbackReason}");
        }

        _logger.LogInformation(
            "TransactionCoordinator: COMMIT - AuthId={AuthId}, ClaimKey={ClaimKey}, Duration={Duration}ms",
            context.AuthorizationId,
            context.ClaimKey,
            (DateTime.UtcNow - context.RequestTimestamp).TotalMilliseconds);

        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Rolls back the current transaction and logs the failure reason
    /// </summary>
    /// <param name="context">Transaction context with rollback reason</param>
    /// <param name="exception">Exception that triggered rollback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public async Task RollbackTransactionAsync(
        TransactionContext context,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        context.MarkForRollback($"Failed at step {context.Step}: {exception.Message}");

        _logger.LogError(
            exception,
            "TransactionCoordinator: ROLLBACK - AuthId={AuthId}, FailedStep={Step}, Reason={Reason}",
            context.AuthorizationId,
            context.Step,
            context.RollbackReason);

        try
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "TransactionCoordinator: ROLLBACK COMPLETED - AuthId={AuthId}",
                context.AuthorizationId);
        }
        catch (Exception rollbackEx)
        {
            _logger.LogCritical(
                rollbackEx,
                "TransactionCoordinator: ROLLBACK FAILED - AuthId={AuthId}. Data may be corrupted!",
                context.AuthorizationId);

            throw new InvalidOperationException(
                $"Transaction rollback failed for {context.AuthorizationId}. Original error: {exception.Message}",
                rollbackEx);
        }
    }

    /// <summary>
    /// Executes a transaction step with automatic rollback on failure
    /// </summary>
    /// <param name="context">Transaction context</param>
    /// <param name="stepName">Human-readable step name for logging</param>
    /// <param name="stepAction">Action to execute for this step</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public async Task ExecuteStepAsync(
        TransactionContext context,
        string stepName,
        Func<Task> stepAction,
        CancellationToken cancellationToken = default)
    {
        var stepStartTime = DateTime.UtcNow;

        _logger.LogInformation(
            "TransactionCoordinator: STEP START - {StepName} (Step={Step}), AuthId={AuthId}",
            stepName, context.Step, context.AuthorizationId);

        try
        {
            await stepAction();

            var elapsedMs = (DateTime.UtcNow - stepStartTime).TotalMilliseconds;

            _logger.LogInformation(
                "TransactionCoordinator: STEP COMPLETE - {StepName} completed in {ElapsedMs}ms",
                stepName, elapsedMs);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "TransactionCoordinator: STEP FAILED - {StepName} at step {Step}: {Error}",
                stepName, context.Step, ex.Message);

            throw;
        }
    }

    /// <summary>
    /// Checks if a transaction is currently active
    /// </summary>
    /// <returns>True if transaction is active, false otherwise</returns>
    public bool IsTransactionActive()
    {
        return _unitOfWork.HasActiveTransaction;
    }

    /// <summary>
    /// Gets transaction statistics for monitoring
    /// </summary>
    /// <param name="context">Transaction context</param>
    /// <returns>Dictionary with transaction metrics</returns>
    public Dictionary<string, object> GetTransactionStats(TransactionContext context)
    {
        var stats = new Dictionary<string, object>
        {
            ["AuthorizationId"] = context.AuthorizationId,
            ["CurrentStep"] = context.Step.ToString(),
            ["OperationCode"] = context.OperationCode,
            ["OperatorId"] = context.OperatorId,
            ["TransactionDate"] = context.TransactionDate,
            ["IsRolledBack"] = context.RollbackReason != null,
            ["IsActive"] = IsTransactionActive()
        };

        if (context.RollbackReason != null)
        {
            stats["RollbackReason"] = context.RollbackReason;
        }

        return stats;
    }
}
