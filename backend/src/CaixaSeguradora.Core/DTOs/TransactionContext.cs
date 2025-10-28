using CaixaSeguradora.Core.Enums;
using CaixaSeguradora.Core.ValueObjects;

namespace CaixaSeguradora.Core.DTOs;

/// <summary>
/// Context object passed through 4-step payment authorization transaction for atomicity tracking
/// Validation Rules:
/// - Required: AuthorizationId, ClaimKey, OperatorId, TransactionDate, TransactionTime, OperationCode, CorrectionType, Step
/// - OperationCode must equal 1098 (constant)
/// - CorrectionType must equal "5" (constant)
/// - Step must progress: History → ClaimMaster → Accompaniment → Phases → Committed (no backwards)
/// - If Step != Committed, RollbackReason must be null
/// </summary>
public class TransactionContext
{
    /// <summary>
    /// Unique identifier for this authorization attempt
    /// </summary>
    public Guid AuthorizationId { get; set; }

    /// <summary>
    /// Composite key identifying claim (Tipseg, Orgsin, Rmosin, Numsin)
    /// </summary>
    public ClaimKey ClaimKey { get; set; } = null!;

    /// <summary>
    /// EZEUSRID operator executing transaction
    /// </summary>
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// DTMOVABE business date from TSISTEMA
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// HORAOPER system time
    /// </summary>
    public TimeSpan TransactionTime { get; set; }

    /// <summary>
    /// Always 1098 for payment authorization
    /// </summary>
    public int OperationCode { get; set; } = 1098;

    /// <summary>
    /// Always '5' for standard correction
    /// </summary>
    public string CorrectionType { get; set; } = "5";

    /// <summary>
    /// Current step in pipeline (enum: History, ClaimMaster, Accompaniment, Phases, Committed)
    /// </summary>
    public TransactionStep Step { get; set; }

    /// <summary>
    /// If rollback occurred, which step failed
    /// </summary>
    public string? RollbackReason { get; set; }

    /// <summary>
    /// When the transaction request was initiated
    /// </summary>
    public DateTime RequestTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates that all required fields are set and rules are met
    /// </summary>
    public bool IsValid()
    {
        if (AuthorizationId == Guid.Empty) return false;
        if (ClaimKey == null) return false;
        if (string.IsNullOrWhiteSpace(OperatorId)) return false;
        if (TransactionDate == default) return false;
        if (OperationCode != 1098) return false;
        if (CorrectionType != "5") return false;
        if (Step == TransactionStep.Committed && RollbackReason != null) return false;

        return true;
    }

    /// <summary>
    /// Advances transaction to next step
    /// </summary>
    public void AdvanceStep()
    {
        Step = Step switch
        {
            TransactionStep.History => TransactionStep.ClaimMaster,
            TransactionStep.ClaimMaster => TransactionStep.Accompaniment,
            TransactionStep.Accompaniment => TransactionStep.Phases,
            TransactionStep.Phases => TransactionStep.Committed,
            _ => throw new InvalidOperationException($"Cannot advance from step {Step}")
        };
    }

    /// <summary>
    /// Marks transaction as requiring rollback
    /// </summary>
    public void MarkForRollback(string reason)
    {
        RollbackReason = reason;
    }
}
