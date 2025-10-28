namespace CaixaSeguradora.Core.Enums;

/// <summary>
/// Transaction pipeline step for payment authorization
/// Steps must progress: History → ClaimMaster → Accompaniment → Phases → Committed (no backwards)
/// </summary>
public enum TransactionStep
{
    /// <summary>
    /// Insert THISTSIN (history record)
    /// </summary>
    History = 1,

    /// <summary>
    /// Update TMESTSIN (claim master)
    /// </summary>
    ClaimMaster = 2,

    /// <summary>
    /// Insert SI_ACOMPANHA_SINI (accompaniment event)
    /// </summary>
    Accompaniment = 3,

    /// <summary>
    /// Update SI_SINISTRO_FASE (phase records)
    /// </summary>
    Phases = 4,

    /// <summary>
    /// Transaction committed successfully
    /// </summary>
    Committed = 5
}
