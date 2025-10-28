using FluentValidation;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Core.Validators.BusinessRules;

/// <summary>
/// Audit Trail Rules (BR-071 to BR-074)
/// Enforces business rules for audit trail and data integrity
/// </summary>
public class AuditTrailRules : AbstractValidator<AuditTrailContext>
{
    public AuditTrailRules()
    {
        // BR-071: Timestamp recorded on insertion/modification
        When(x => x.IsNewRecord, () =>
        {
            RuleFor(x => x.CreatedAt)
                .NotNull()
                .WithMessage("Timestamp de criação é obrigatório para novos registros")
                .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
                .WithMessage("Timestamp de criação não pode estar muito no futuro")
                .WithErrorCode("BR-071");
        });

        When(x => !x.IsNewRecord, () =>
        {
            RuleFor(x => x.UpdatedAt)
                .NotNull()
                .WithMessage("Timestamp de atualização é obrigatório para modificações")
                .GreaterThanOrEqualTo(x => x.CreatedAt ?? DateTime.MinValue)
                .WithMessage("Timestamp de atualização deve ser >= timestamp de criação")
                .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
                .WithMessage("Timestamp de atualização não pode estar muito no futuro")
                .WithErrorCode("BR-071");
        });

        // BR-072: Complete audit trail reconstruction via SI_ACOMPANHA_SINI
        // This is enforced by ensuring all events are recorded with proper metadata

        // BR-073: Transaction date immutable after creation
        When(x => !x.IsNewRecord && x.TransactionDate.HasValue, () =>
        {
            RuleFor(x => x.TransactionDate)
                .Equal(x => x.OriginalTransactionDate)
                .When(x => x.OriginalTransactionDate.HasValue)
                .WithMessage("Data da transação é imutável após criação")
                .WithErrorCode("BR-073");
        });

        // BR-074: Referential integrity maintained across tables
        // This is enforced by database foreign key constraints and application-level validation
        RuleFor(x => x.ClaimKey)
            .NotNull()
            .WithMessage("Chave do sinistro é obrigatória para integridade referencial")
            .WithErrorCode("BR-074");

        When(x => x.ClaimKey != null, () =>
        {
            RuleFor(x => x.ClaimKey!.Tipseg)
                .GreaterThan(0)
                .WithMessage("TIPSEG deve ser maior que 0 para integridade referencial")
                .WithErrorCode("BR-074");

            RuleFor(x => x.ClaimKey!.Orgsin)
                .InclusiveBetween(1, 99)
                .WithMessage("ORGSIN deve estar entre 01 e 99 para integridade referencial")
                .WithErrorCode("BR-074");

            RuleFor(x => x.ClaimKey!.Rmosin)
                .InclusiveBetween(0, 99)
                .WithMessage("RMOSIN deve estar entre 00 e 99 para integridade referencial")
                .WithErrorCode("BR-074");

            RuleFor(x => x.ClaimKey!.Numsin)
                .InclusiveBetween(1, 999999)
                .WithMessage("NUMSIN deve estar entre 1 e 999999 para integridade referencial")
                .WithErrorCode("BR-074");
        });
    }
}

/// <summary>
/// Context for audit trail validation
/// Contains all data needed to validate audit trail rules
/// </summary>
public class AuditTrailContext
{
    /// <summary>
    /// Whether this is a new record
    /// </summary>
    public bool IsNewRecord { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Transaction date (current)
    /// </summary>
    public DateTime? TransactionDate { get; set; }

    /// <summary>
    /// Original transaction date (for immutability check)
    /// </summary>
    public DateTime? OriginalTransactionDate { get; set; }

    /// <summary>
    /// Claim composite key for referential integrity
    /// </summary>
    public ClaimKey? ClaimKey { get; set; }
}

/// <summary>
/// Claim composite key for referential integrity validation
/// </summary>
public class ClaimKey
{
    public int Tipseg { get; set; }
    public int Orgsin { get; set; }
    public int Rmosin { get; set; }
    public int Numsin { get; set; }

    public ClaimKey(int tipseg, int orgsin, int rmosin, int numsin)
    {
        Tipseg = tipseg;
        Orgsin = orgsin;
        Rmosin = rmosin;
        Numsin = numsin;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ClaimKey other)
            return false;

        return Tipseg == other.Tipseg &&
               Orgsin == other.Orgsin &&
               Rmosin == other.Rmosin &&
               Numsin == other.Numsin;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Tipseg, Orgsin, Rmosin, Numsin);
    }

    public override string ToString()
    {
        return $"{Tipseg}/{Orgsin:D2}/{Rmosin:D2}/{Numsin}";
    }
}

/// <summary>
/// Validator for entities with audit trail requirements
/// </summary>
public class EntityAuditTrailRules<TEntity> : AbstractValidator<TEntity> where TEntity : class
{
    public EntityAuditTrailRules()
    {
        // BR-071: Timestamps required
        // This would be applied to entities with timestamp properties
        // Implementation depends on entity structure
    }
}
