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
