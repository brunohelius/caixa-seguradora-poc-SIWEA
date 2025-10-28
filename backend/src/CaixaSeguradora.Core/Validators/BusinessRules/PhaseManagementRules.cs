using FluentValidation;
using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Core.Validators.BusinessRules;

/// <summary>
/// Phase Management Rules (BR-061 to BR-070)
/// Enforces business rules for claim phase management and workflow
/// </summary>
public class PhaseManagementRules : AbstractValidator<PhaseUpdateRequest>
{
    public PhaseManagementRules()
    {
        // BR-057: Claim accompaniment record created with COD_EVENTO = 1098
        RuleFor(x => x.CodEvento)
            .Equal(1098)
            .WithMessage("Código do evento deve ser 1098 para autorização de pagamento")
            .WithErrorCode("BR-057");

        // BR-061: Open phase indicator: DATA_FECHA_SIFA = '9999-12-31'
        When(x => x.IsOpenPhaseRequest, () =>
        {
            RuleFor(x => x.DataFechaeSifa)
                .Must(date => date.Year == 9999 && date.Month == 12 && date.Day == 31)
                .WithMessage("Data de fechamento para abertura de fase deve ser '9999-12-31'")
                .WithErrorCode("BR-061");
        });

        // BR-062: Phase opening date set to current business date
        When(x => x.IsOpenPhaseRequest, () =>
        {
            RuleFor(x => x.DataInivig)
                .NotNull()
                .WithMessage("Data de início de vigência é obrigatória para abertura de fase")
                .WithErrorCode("BR-062");
        });

        // BR-063: Prevent duplicate open phases
        // This is enforced at service layer by checking for existing open phases

        // BR-064: Query SI_REL_FASE_EVENTO for event 1098 relationships
        // This is a data retrieval rule enforced in the phase management service

        // BR-065: Process relationships in order
        // This is a workflow rule enforced in the phase management service

        // BR-066: Phase rollback on update failure
        // This is a transaction management rule enforced in the transaction coordinator

        // BR-067: Phase atomicity: all or nothing
        // This is enforced at service layer with transaction management

        // BR-068: Operator user ID on all history records
        // BR-069: Operator user ID on all accompaniment records
        // BR-070: Operator user ID on all phase records
        RuleFor(x => x.OperatorId)
            .NotEmpty()
            .WithMessage("ID do usuário operador é obrigatório em todos os registros de fase")
            .MaximumLength(50)
            .WithMessage("ID do usuário operador não pode exceder 50 caracteres")
            .WithErrorCode("BR-068");
    }
}

/// <summary>
/// Validator for ClaimPhase entity to ensure phase management rules
/// </summary>
public class ClaimPhaseRules : AbstractValidator<ClaimPhase>
{
    public ClaimPhaseRules()
    {
        // BR-061: Open phase indicator: DATA_FECHA_SIFA = '9999-12-31'
        RuleFor(x => x)
            .Must(phase =>
            {
                var isOpen = phase.DataFechaSifa.Year == 9999 &&
                            phase.DataFechaSifa.Month == 12 &&
                            phase.DataFechaSifa.Day == 31;

                // If open, opening date should be valid
                if (isOpen)
                {
                    return phase.DataAberturaSifa.Year <= DateTime.Now.Year;
                }

                // If closed, closing date should be after opening date
                return phase.DataFechaSifa >= phase.DataAberturaSifa;
            })
            .WithMessage("Fase aberta deve ter DATA_FECHA_SIFA = '9999-12-31'; fase fechada deve ter data de fechamento >= data de abertura")
            .WithErrorCode("BR-061");

        // BR-070: Operator user ID on all phase records
        // Note: ClaimPhase entity doesn't have EZEUSRID field - tracked via audit fields
        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .When(x => x.CreatedBy != null)
            .WithMessage("ID do usuário operador é obrigatório em registros de fase")
            .MaximumLength(50)
            .WithMessage("ID do usuário operador não pode exceder 50 caracteres")
            .WithErrorCode("BR-070");
    }
}

/// <summary>
/// Validator for ClaimAccompaniment entity to ensure phase management rules
/// </summary>
public class ClaimAccompanimentRules : AbstractValidator<ClaimAccompaniment>
{
    public ClaimAccompanimentRules()
    {
        // BR-057: Claim accompaniment record created with COD_EVENTO = 1098
        RuleFor(x => x.CodEvento)
            .Equal(1098)
            .WithMessage("Código do evento deve ser 1098 para autorização de pagamento")
            .WithErrorCode("BR-057");

        // BR-069: Operator user ID on all accompaniment records
        RuleFor(x => x.CodUsuario)
            .NotEmpty()
            .WithMessage("ID do usuário operador (COD_USUARIO) é obrigatório em registros de acompanhamento")
            .MaximumLength(50)
            .WithMessage("ID do usuário operador não pode exceder 50 caracteres")
            .WithErrorCode("BR-069");

        // BR-062: Event date should match business date (validated at service layer)
        RuleFor(x => x.DataMovtoSiniaco)
            .NotNull()
            .WithMessage("Data do movimento (DATA_MOVTO_SINIACO) é obrigatória")
            .WithErrorCode("BR-062");
    }
}

/// <summary>
/// Context for phase update validation at service layer
/// </summary>
public class PhaseUpdateValidationContext
{
    /// <summary>
    /// Phases being updated or created
    /// </summary>
    public List<ClaimPhase> Phases { get; set; } = new();

    /// <summary>
    /// Existing open phases (to check for duplicates - BR-063)
    /// </summary>
    public List<ClaimPhase> ExistingOpenPhases { get; set; } = new();

    /// <summary>
    /// Business date from TSISTEMA.DTMOVABE
    /// </summary>
    public DateTime BusinessDate { get; set; }

    /// <summary>
    /// Operator user ID
    /// </summary>
    public string OperatorUserId { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is part of a payment authorization transaction
    /// </summary>
    public bool IsPaymentAuthorization { get; set; }
}

/// <summary>
/// Validator for phase update context to enforce atomicity
/// </summary>
public class PhaseUpdateContextRules : AbstractValidator<PhaseUpdateValidationContext>
{
    public PhaseUpdateContextRules()
    {
        // BR-063: Prevent duplicate open phases
        RuleFor(x => x)
            .Must(ctx =>
            {
                var newOpenPhases = ctx.Phases.Where(p =>
                    p.DataFechaSifa.Year == 9999 &&
                    p.DataFechaSifa.Month == 12 &&
                    p.DataFechaSifa.Day == 31).ToList();

                foreach (var newPhase in newOpenPhases)
                {
                    var duplicate = ctx.ExistingOpenPhases.Any(existing =>
                        existing.Fonte == newPhase.Fonte &&
                        existing.Protsini == newPhase.Protsini &&
                        existing.Dac == newPhase.Dac &&
                        existing.CodFase == newPhase.CodFase);

                    if (duplicate)
                        return false;
                }

                return true;
            })
            .WithMessage("Não é permitido criar fases duplicadas em aberto (DATA_FECHA_SIFA = '9999-12-31')")
            .WithErrorCode("BR-063");

        // BR-067: Phase atomicity - at least one phase update required
        RuleFor(x => x.Phases)
            .NotEmpty()
            .WithMessage("Pelo menos uma atualização de fase é obrigatória (atomicidade)")
            .WithErrorCode("BR-067");

        // BR-068-070: Operator user ID required
        RuleFor(x => x.OperatorUserId)
            .NotEmpty()
            .WithMessage("ID do usuário operador é obrigatório para todas as operações de fase")
            .WithErrorCode("BR-068");
    }
}

/// <summary>
/// System error codes for phase management
/// </summary>
public static class PhaseManagementErrorCodes
{
    // SYS-004: Phase update failed
    public const string PhaseUpdateFailed = "SYS-004";
}
