using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Core.Interfaces;

/// <summary>
/// T096: IPhaseManagementService Interface
/// Manages claim phase lifecycle including opening, closing, and tracking phases
/// based on events recorded in SI_REL_FASE_EVENTO
/// </summary>
public interface IPhaseManagementService
{
    /// <summary>
    /// Updates claim phases based on a recorded event
    /// Queries SI_REL_FASE_EVENTO to determine which phases should be opened or closed
    /// </summary>
    /// <param name="fonte">Protocol source</param>
    /// <param name="protsini">Protocol number</param>
    /// <param name="dac">Protocol DAC</param>
    /// <param name="codEvento">Event code triggering phase changes</param>
    /// <param name="dataMovto">Event date</param>
    /// <param name="userId">User ID performing the operation</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdatePhasesAsync(
        int fonte,
        int protsini,
        int dac,
        int codEvento,
        DateTime dataMovto,
        string userId
    );

    /// <summary>
    /// Creates a new phase opening record
    /// Sets data_fecha_sifa to 9999-12-31 (SQL date max) to indicate open phase
    /// </summary>
    /// <param name="phase">Phase entity to create</param>
    /// <returns>Task representing the async operation</returns>
    Task CreatePhaseOpeningAsync(ClaimPhase phase);

    /// <summary>
    /// Updates an existing phase to close it
    /// Sets data_fecha_sifa to the closing date
    /// </summary>
    /// <param name="phase">Phase entity to update</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdatePhaseClosingAsync(ClaimPhase phase);

    /// <summary>
    /// Gets all active (open) phases for a claim
    /// </summary>
    /// <param name="fonte">Protocol source</param>
    /// <param name="protsini">Protocol number</param>
    /// <param name="dac">Protocol DAC</param>
    /// <returns>List of active phases</returns>
    Task<List<ClaimPhase>> GetActivePhases(int fonte, int protsini, int dac);

    /// <summary>
    /// Checks if a phase already exists to prevent duplicates
    /// </summary>
    /// <param name="fonte">Protocol source</param>
    /// <param name="protsini">Protocol number</param>
    /// <param name="dac">Protocol DAC</param>
    /// <param name="codFase">Phase code</param>
    /// <param name="codEvento">Event code</param>
    /// <returns>True if phase exists, false otherwise</returns>
    Task<bool> PreventDuplicatePhase(
        int fonte,
        int protsini,
        int dac,
        int codFase,
        int codEvento
    );

    /// <summary>
    /// Gets all phases for a claim including closed phases
    /// </summary>
    /// <param name="fonte">Protocol source</param>
    /// <param name="protsini">Protocol number</param>
    /// <param name="dac">Protocol DAC</param>
    /// <returns>List of all phases ordered by opening date</returns>
    Task<List<ClaimPhase>> GetAllPhasesAsync(int fonte, int protsini, int dac);

    /// <summary>
    /// Gets phase statistics for a claim
    /// </summary>
    /// <param name="fonte">Protocol source</param>
    /// <param name="protsini">Protocol number</param>
    /// <param name="dac">Protocol DAC</param>
    /// <returns>Statistics including count of open/closed phases, average duration, etc.</returns>
    Task<PhaseStatistics> GetPhaseStatisticsAsync(int fonte, int protsini, int dac);
}

/// <summary>
/// Statistics about claim phases
/// </summary>
public class PhaseStatistics
{
    public int TotalPhases { get; set; }
    public int OpenPhases { get; set; }
    public int ClosedPhases { get; set; }
    public double AverageDurationDays { get; set; }
    public int? LongestOpenPhaseDays { get; set; }
    public string? LongestOpenPhaseName { get; set; }
}
