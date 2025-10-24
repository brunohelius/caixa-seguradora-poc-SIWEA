using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Component Migration - Individual component tracking (New table: COMPONENT_MIGRATION)
/// Tracks migration status for screens, business rules, database entities, and external services.
/// </summary>
[Table("COMPONENT_MIGRATION")]
[Index(nameof(MigrationStatusId), nameof(ComponentType), Name = "IX_COMPONENT_MIGRATION_StatusType")]
[Index(nameof(Status), nameof(AssignedDeveloper), Name = "IX_COMPONENT_MIGRATION_StatusDev")]
[Index(nameof(ComponentType), nameof(Complexity), Name = "IX_COMPONENT_MIGRATION_TypeComplexity")]
public class ComponentMigration
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [Column("MIGRATION_STATUS_ID")]
    [Required]
    public Guid MigrationStatusId { get; set; }

    [Column("COMPONENT_TYPE")]
    [Required]
    [StringLength(50)]
    public string ComponentType { get; set; } = string.Empty;

    [Column("COMPONENT_NAME")]
    [Required]
    [StringLength(200)]
    public string ComponentName { get; set; } = string.Empty;

    [Column("LEGACY_REFERENCE")]
    [StringLength(200)]
    public string? LegacyReference { get; set; }

    [Column("STATUS")]
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Not Started";

    [Column("ESTIMATED_HOURS", TypeName = "decimal(8,2)")]
    [Range(0, 9999.99)]
    public decimal? EstimatedHours { get; set; }

    [Column("ACTUAL_HOURS", TypeName = "decimal(8,2)")]
    [Range(0, 9999.99)]
    public decimal? ActualHours { get; set; }

    [Column("COMPLEXITY")]
    [StringLength(20)]
    public string? Complexity { get; set; }

    [Column("ASSIGNED_DEVELOPER")]
    [StringLength(100)]
    public string? AssignedDeveloper { get; set; }

    [Column("TECHNICAL_NOTES")]
    [StringLength(2000)]
    public string? TechnicalNotes { get; set; }

    [Column("START_DATE")]
    public DateTime? StartDate { get; set; }

    [Column("COMPLETION_DATE")]
    public DateTime? CompletionDate { get; set; }

    [Column("DEPENDENCIES")]
    [StringLength(500)]
    public string? Dependencies { get; set; }

    // Audit Fields
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    [Column("CREATED_AT")]
    [Required]
    public DateTime CreatedAt { get; set; }

    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    [Column("UPDATED_AT")]
    [Required]
    public DateTime UpdatedAt { get; set; }

    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(MigrationStatusId))]
    public virtual MigrationStatus? MigrationStatus { get; set; }

    public virtual ICollection<PerformanceMetric> PerformanceMetrics { get; set; } = new List<PerformanceMetric>();

    // Computed Properties
    [NotMapped]
    public decimal? EffortVariance
    {
        get
        {
            if (!EstimatedHours.HasValue || !ActualHours.HasValue)
                return null;

            return ActualHours.Value - EstimatedHours.Value;
        }
    }

    [NotMapped]
    public decimal? EffortVariancePercentage
    {
        get
        {
            if (!EstimatedHours.HasValue || !ActualHours.HasValue || EstimatedHours.Value == 0)
                return null;

            return Math.Round((ActualHours.Value - EstimatedHours.Value) / EstimatedHours.Value * 100, 2);
        }
    }

    [NotMapped]
    public int? DurationInDays
    {
        get
        {
            if (!StartDate.HasValue || !CompletionDate.HasValue)
                return null;

            return (CompletionDate.Value.Date - StartDate.Value.Date).Days;
        }
    }

    [NotMapped]
    public bool IsCompleted => Status == "Completed";

    [NotMapped]
    public bool IsBlocked => Status == "Blocked";
}
