#!/bin/bash

# This script creates all remaining foundation files for T017-T030
# Running from: /Users/brunosouza/Development/Caixa Seguradora/POC Visual Age

BASE_DIR="/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/backend"

echo "Creating remaining Phase 2 foundation files..."
echo "================================================"

# ComponentMigration entity
cat > "$BASE_DIR/src/CaixaSeguradora.Core/Entities/ComponentMigration.cs" << 'EOF'
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
EOF

echo "Created ComponentMigration.cs"

# PerformanceMetric entity  
cat > "$BASE_DIR/src/CaixaSeguradora.Core/Entities/PerformanceMetric.cs" << 'EOF'
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Performance Metrics - Performance comparison (New table: PERFORMANCE_METRICS)
/// Stores performance benchmarks for comparison between legacy Visual Age and new .NET 9 system.
/// </summary>
[Table("PERFORMANCE_METRICS")]
[Index(nameof(ComponentId), nameof(MetricType), Name = "IX_PERFORMANCE_METRICS_ComponentType")]
[Index(nameof(MeasurementTimestamp), Name = "IX_PERFORMANCE_METRICS_Timestamp")]
[Index(nameof(PassFail), Name = "IX_PERFORMANCE_METRICS_PassFail")]
public class PerformanceMetric
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [Column("COMPONENT_ID")]
    [Required]
    public Guid ComponentId { get; set; }

    [Column("METRIC_TYPE")]
    [Required]
    [StringLength(50)]
    public string MetricType { get; set; } = string.Empty;

    [Column("LEGACY_VALUE", TypeName = "decimal(18,4)")]
    [Required]
    public decimal LegacyValue { get; set; }

    [Column("NEW_VALUE", TypeName = "decimal(18,4)")]
    [Required]
    public decimal NewValue { get; set; }

    [Column("UNIT")]
    [StringLength(20)]
    public string? Unit { get; set; }

    [Column("MEASUREMENT_TIMESTAMP")]
    [Required]
    public DateTime MeasurementTimestamp { get; set; }

    [Column("TEST_SCENARIO")]
    [StringLength(500)]
    public string? TestScenario { get; set; }

    [Column("PASS_FAIL")]
    [Required]
    public bool PassFail { get; set; }

    [Column("TARGET_VALUE", TypeName = "decimal(18,4)")]
    public decimal? TargetValue { get; set; }

    [Column("ENVIRONMENT")]
    [StringLength(50)]
    public string? Environment { get; set; }

    [Column("NOTES")]
    [StringLength(1000)]
    public string? Notes { get; set; }

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
    [ForeignKey(nameof(ComponentId))]
    public virtual ComponentMigration? Component { get; set; }

    // Computed Properties
    [NotMapped]
    public decimal ImprovementPercentage
    {
        get
        {
            if (LegacyValue == 0)
                return 0;

            // For Response Time and Memory Usage: lower is better
            if (MetricType.Contains("Response Time", StringComparison.OrdinalIgnoreCase) ||
                MetricType.Contains("Memory", StringComparison.OrdinalIgnoreCase) ||
                MetricType.Contains("Error Rate", StringComparison.OrdinalIgnoreCase))
            {
                return Math.Round((LegacyValue - NewValue) / LegacyValue * 100, 2);
            }

            // For Throughput and Concurrent Users: higher is better
            return Math.Round((NewValue - LegacyValue) / LegacyValue * 100, 2);
        }
    }

    [NotMapped]
    public decimal Difference => NewValue - LegacyValue;

    [NotMapped]
    public bool IsImproved => ImprovementPercentage > 0;

    [NotMapped]
    public string PerformanceRating
    {
        get
        {
            var improvement = ImprovementPercentage;

            if (improvement >= 50)
                return "Excellent";
            if (improvement >= 25)
                return "Good";
            if (improvement >= 0)
                return "Fair";

            return "Poor";
        }
    }
}
EOF

echo "Created PerformanceMetric.cs"

echo ""
echo "All entity files created successfully!"
echo "Total entity files: 13 (10 legacy + 3 dashboard)"

