using FluentValidation;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Core.Validators.BusinessRules;

/// <summary>
/// Transaction Recording Rules (BR-038 to BR-046)
/// Enforces business rules for transaction recording in payment authorization
/// </summary>
public class TransactionRecordingRules : AbstractValidator<TransactionRecordingContext>
{
    public TransactionRecordingRules()
    {
        // BR-038: Overall status initialized to '0'
        RuleFor(x => x.OverallStatus)
            .Equal('0')
            .When(x => x.OverallStatus.HasValue)
            .WithMessage("Status geral deve ser inicializado como '0'")
            .WithErrorCode("BR-038");

        // BR-039: Correction type always '5'
        RuleFor(x => x.CorrectionType)
            .Equal('5')
            .When(x => x.CorrectionType.HasValue)
            .WithMessage("Tipo de correção deve ser sempre '5'")
            .WithErrorCode("BR-039");

        // BR-040: Occurrence counter incremented: OCORHIST_NEW = OCORHIST + 1
        When(x => x.OldOccurrenceCount.HasValue && x.NewOccurrenceCount.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(ctx => ctx.NewOccurrenceCount == ctx.OldOccurrenceCount + 1)
                .WithMessage("Contador de ocorrências (OCORHIST) deve ser incrementado em 1")
                .WithErrorCode("BR-040");
        });

        // BR-041: Operator user ID recorded: EZEUSRID
        RuleFor(x => x.OperatorUserId)
            .NotEmpty()
            .WithMessage("ID do usuário operador (EZEUSRID) deve ser registrado")
            .MaximumLength(50)
            .WithMessage("ID do usuário operador não pode exceder 50 caracteres")
            .WithErrorCode("BR-041");

        // BR-042: Audit fields: CREATED_BY, CREATED_AT, UPDATED_BY, UPDATED_AT
        When(x => x.IsNewRecord, () =>
        {
            RuleFor(x => x.CreatedBy)
                .NotEmpty()
                .WithMessage("Campo CREATED_BY é obrigatório para novos registros")
                .WithErrorCode("BR-042");

            RuleFor(x => x.CreatedAt)
                .NotNull()
                .WithMessage("Campo CREATED_AT é obrigatório para novos registros")
                .WithErrorCode("BR-042");
        });

        When(x => !x.IsNewRecord, () =>
        {
            RuleFor(x => x.UpdatedBy)
                .NotEmpty()
                .WithMessage("Campo UPDATED_BY é obrigatório para atualizações")
                .WithErrorCode("BR-042");

            RuleFor(x => x.UpdatedAt)
                .NotNull()
                .WithMessage("Campo UPDATED_AT é obrigatório para atualizações")
                .WithErrorCode("BR-042");
        });

        // BR-043: Consortium products: 6814, 7701, 7709 → CNOUA
        // BR-044: Query EF_CONTR_SEG_HABIT for contract number
        // BR-045: EFP contract (NUM_CONTRATO > 0) → SIPUA
        // BR-046: HB contract (NUM_CONTRATO = 0 or not found) → SIMDA
        // These are routing rules validated in ProductValidationRules
    }
}

/// <summary>
/// Context for transaction recording validation
/// Contains all data needed to validate transaction recording rules
/// </summary>
public class TransactionRecordingContext
{
    /// <summary>
    /// Overall status (should be '0' initially)
    /// </summary>
    public char? OverallStatus { get; set; }

    /// <summary>
    /// Correction type (should be '5' always)
    /// </summary>
    public char? CorrectionType { get; set; }

    /// <summary>
    /// Old occurrence count from TMESTSIN.OCORHIST
    /// </summary>
    public int? OldOccurrenceCount { get; set; }

    /// <summary>
    /// New occurrence count (should be old + 1)
    /// </summary>
    public int? NewOccurrenceCount { get; set; }

    /// <summary>
    /// Operator user ID (EZEUSRID)
    /// </summary>
    public string? OperatorUserId { get; set; }

    /// <summary>
    /// Whether this is a new record (for audit field validation)
    /// </summary>
    public bool IsNewRecord { get; set; }

    /// <summary>
    /// Created by user (for new records)
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Created timestamp (for new records)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Updated by user (for updates)
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Updated timestamp (for updates)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Validator for ClaimHistory entity to ensure transaction recording rules
/// </summary>
public class ClaimHistoryTransactionRules : AbstractValidator<ClaimHistory>
{
    public ClaimHistoryTransactionRules()
    {
        // BR-034: Operation code always 1098
        RuleFor(x => x.Operacao)
            .Equal(1098)
            .WithMessage("Código de operação deve ser sempre 1098 para autorização de pagamento")
            .WithErrorCode("BR-034");

        // BR-039: Correction type always '5'
        RuleFor(x => x.Tipcrr)
            .Equal("5")
            .WithMessage("Tipo de correção deve ser sempre '5'")
            .WithErrorCode("BR-039");

        // BR-041: Operator user ID recorded
        RuleFor(x => x.Ezeusrid)
            .NotEmpty()
            .WithMessage("ID do usuário operador (EZEUSRID) deve ser registrado")
            .MaximumLength(50)
            .WithMessage("ID do usuário operador não pode exceder 50 caracteres")
            .WithErrorCode("BR-041");
    }
}
