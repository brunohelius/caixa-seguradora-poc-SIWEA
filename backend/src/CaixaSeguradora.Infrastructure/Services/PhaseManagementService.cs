using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CaixaSeguradora.Infrastructure.Services;

/// <summary>
/// T097: PhaseManagementService Implementation
/// Manages claim phase lifecycle based on events in SI_REL_FASE_EVENTO table
/// </summary>
public class PhaseManagementService : IPhaseManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PhaseManagementService> _logger;

    // SQL date max value for open phases (9999-12-31)
    private static readonly DateTime OpenPhaseEndDate = new DateTime(9999, 12, 31);

    public PhaseManagementService(
        IUnitOfWork unitOfWork,
        ILogger<PhaseManagementService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task UpdatePhasesAsync(
        int fonte,
        int protsini,
        int dac,
        int codEvento,
        DateTime dataMovto,
        string userId)
    {
        _logger.LogInformation(
            "Updating phases for protocol {Fonte}/{Protsini}-{Dac}, Event: {CodEvento}",
            fonte, protsini, dac, codEvento
        );

        // Query SI_REL_FASE_EVENTO to find relationships for this event
        var relationships = await _unitOfWork.PhaseEventRelationships.FindAsync(r =>
            r.CodEvento == codEvento &&
            r.DataInivigRefaev <= dataMovto // Event date must be within valid range
        );

        if (!relationships.Any())
        {
            _logger.LogWarning(
                "No phase-event relationships found for event {CodEvento}",
                codEvento
            );
            return;
        }

        foreach (var relationship in relationships)
        {
            _logger.LogDebug(
                "Processing relationship: Phase {CodFase}, Event {CodEvento}, Action {IndAlteracaoFase}",
                relationship.CodFase, relationship.CodEvento, relationship.IndAlteracaoFase
            );

            if (relationship.IndAlteracaoFase == "1")
            {
                // Opening phase
                await HandlePhaseOpening(
                    fonte, protsini, dac,
                    relationship.CodFase,
                    relationship.CodEvento,
                    dataMovto,
                    userId,
                    relationship
                );
            }
            else if (relationship.IndAlteracaoFase == "2")
            {
                // Closing phase
                await HandlePhaseClosing(
                    fonte, protsini, dac,
                    relationship.CodFase,
                    relationship.CodEvento,
                    dataMovto,
                    userId
                );
            }
            else
            {
                _logger.LogWarning(
                    "Unknown phase alteration indicator: {IndAlteracaoFase}",
                    relationship.IndAlteracaoFase
                );
            }
        }

        // Save all changes
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Phase updates completed for protocol {Fonte}/{Protsini}-{Dac}",
            fonte, protsini, dac
        );
    }

    public async Task CreatePhaseOpeningAsync(ClaimPhase phase)
    {
        // Ensure the phase end date is set to open phase marker
        phase.DataFechaSifa = OpenPhaseEndDate;

        await _unitOfWork.ClaimPhases.AddAsync(phase);

        _logger.LogInformation(
            "Created phase opening: Protocol {Fonte}/{Protsini}-{Dac}, Phase {CodFase}",
            phase.Fonte, phase.Protsini, phase.Dac, phase.CodFase
        );
    }

    public async Task UpdatePhaseClosingAsync(ClaimPhase phase)
    {
        await _unitOfWork.ClaimPhases.UpdateAsync(phase);

        _logger.LogInformation(
            "Updated phase closing: Protocol {Fonte}/{Protsini}-{Dac}, Phase {CodFase}, Closed on {DataFecha}",
            phase.Fonte, phase.Protsini, phase.Dac, phase.CodFase, phase.DataFechaSifa
        );
    }

    public async Task<List<ClaimPhase>> GetActivePhases(int fonte, int protsini, int dac)
    {
        var activePhases = await _unitOfWork.ClaimPhases.FindAsync(p =>
            p.Fonte == fonte &&
            p.Protsini == protsini &&
            p.Dac == dac &&
            p.DataFechaSifa == OpenPhaseEndDate
        );

        return activePhases.OrderBy(p => p.DataAberturaSifa).ToList();
    }

    public async Task<bool> PreventDuplicatePhase(
        int fonte,
        int protsini,
        int dac,
        int codFase,
        int codEvento)
    {
        var existingPhases = await _unitOfWork.ClaimPhases.FindAsync(p =>
            p.Fonte == fonte &&
            p.Protsini == protsini &&
            p.Dac == dac &&
            p.CodFase == codFase &&
            p.CodEvento == codEvento &&
            p.DataFechaSifa == OpenPhaseEndDate // Only check open phases
        );

        return existingPhases.Any();
    }

    public async Task<List<ClaimPhase>> GetAllPhasesAsync(int fonte, int protsini, int dac)
    {
        var allPhases = await _unitOfWork.ClaimPhases.FindAsync(p =>
            p.Fonte == fonte &&
            p.Protsini == protsini &&
            p.Dac == dac
        );

        return allPhases
            .OrderByDescending(p => p.DataAberturaSifa)
            .ToList();
    }

    public async Task<PhaseStatistics> GetPhaseStatisticsAsync(int fonte, int protsini, int dac)
    {
        var allPhases = await GetAllPhasesAsync(fonte, protsini, dac);

        var closedPhases = allPhases.Where(p => p.DataFechaSifa != OpenPhaseEndDate).ToList();
        var openPhases = allPhases.Where(p => p.DataFechaSifa == OpenPhaseEndDate).ToList();

        // Calculate average duration for closed phases
        double avgDuration = 0;
        if (closedPhases.Any())
        {
            avgDuration = closedPhases
                .Select(p => (p.DataFechaSifa - p.DataAberturaSifa).Days)
                .Average();
        }

        // Find longest open phase
        int? longestOpenDays = null;
        string? longestOpenPhaseName = null;

        if (openPhases.Any())
        {
            var longestOpen = openPhases
                .OrderByDescending(p => (DateTime.Today - p.DataAberturaSifa).Days)
                .First();

            longestOpenDays = (DateTime.Today - longestOpen.DataAberturaSifa).Days;
            longestOpenPhaseName = $"Phase {longestOpen.CodFase} / Event {longestOpen.CodEvento}";
        }

        return new PhaseStatistics
        {
            TotalPhases = allPhases.Count,
            OpenPhases = openPhases.Count,
            ClosedPhases = closedPhases.Count,
            AverageDurationDays = Math.Round(avgDuration, 2),
            LongestOpenPhaseDays = longestOpenDays,
            LongestOpenPhaseName = longestOpenPhaseName
        };
    }

    #region Private Helper Methods

    private async Task HandlePhaseOpening(
        int fonte,
        int protsini,
        int dac,
        int codFase,
        int codEvento,
        DateTime dataMovto,
        string userId,
        PhaseEventRelationship relationship)
    {
        // Check if phase already exists (prevent duplicates)
        bool exists = await PreventDuplicatePhase(fonte, protsini, dac, codFase, codEvento);

        if (exists)
        {
            _logger.LogWarning(
                "Phase {CodFase} for event {CodEvento} already exists (open). Skipping creation.",
                codFase, codEvento
            );
            return;
        }

        // Create new phase opening
        var newPhase = new ClaimPhase
        {
            Fonte = fonte,
            Protsini = protsini,
            Dac = dac,
            CodFase = codFase,
            CodEvento = codEvento,
            NumOcorrSiniaco = 0, // Will be set based on accompaniment
            DataInivigRefaev = relationship.DataInivigRefaev,
            DataAberturaSifa = dataMovto,
            DataFechaSifa = OpenPhaseEndDate, // 9999-12-31
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await CreatePhaseOpeningAsync(newPhase);

        _logger.LogInformation(
            "Opened phase {CodFase} for protocol {Fonte}/{Protsini}-{Dac}",
            codFase, fonte, protsini, dac
        );
    }

    private async Task HandlePhaseClosing(
        int fonte,
        int protsini,
        int dac,
        int codFase,
        int codEvento,
        DateTime dataMovto,
        string userId)
    {
        // Find the open phase to close
        var openPhases = await _unitOfWork.ClaimPhases.FindAsync(p =>
            p.Fonte == fonte &&
            p.Protsini == protsini &&
            p.Dac == dac &&
            p.CodFase == codFase &&
            p.CodEvento == codEvento &&
            p.DataFechaSifa == OpenPhaseEndDate
        );

        var phaseToClose = openPhases.FirstOrDefault();

        if (phaseToClose == null)
        {
            _logger.LogWarning(
                "No open phase {CodFase} found for event {CodEvento} to close. Skipping.",
                codFase, codEvento
            );
            return;
        }

        // Update closing date
        phaseToClose.DataFechaSifa = dataMovto;
        phaseToClose.UpdatedAt = DateTime.UtcNow;
        phaseToClose.UpdatedBy = userId;

        await UpdatePhaseClosingAsync(phaseToClose);

        _logger.LogInformation(
            "Closed phase {CodFase} for protocol {Fonte}/{Protsini}-{Dac} on {DataFecha}",
            codFase, fonte, protsini, dac, dataMovto
        );
    }

    #endregion
}
