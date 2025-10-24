using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for PerformanceMetric (PERFORMANCE_METRICS)
/// Configures Guid primary key, FK to ComponentMigration, and decimal precision (18,4)
/// </summary>
public class PerformanceMetricConfiguration : IEntityTypeConfiguration<PerformanceMetric>
{
    public void Configure(EntityTypeBuilder<PerformanceMetric> builder)
    {
        // Table mapping
        builder.ToTable("PERFORMANCE_METRICS");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Column mappings
        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .IsRequired();

        builder.Property(e => e.ComponentId)
            .HasColumnName("COMPONENT_ID")
            .IsRequired();

        builder.Property(e => e.MetricType)
            .HasColumnName("METRIC_TYPE")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.LegacyValue)
            .HasColumnName("LEGACY_VALUE")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(e => e.NewValue)
            .HasColumnName("NEW_VALUE")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(e => e.Unit)
            .HasColumnName("UNIT")
            .HasMaxLength(20);

        builder.Property(e => e.MeasurementTimestamp)
            .HasColumnName("MEASUREMENT_TIMESTAMP")
            .IsRequired();

        builder.Property(e => e.TestScenario)
            .HasColumnName("TEST_SCENARIO")
            .HasMaxLength(500);

        builder.Property(e => e.PassFail)
            .HasColumnName("PASS_FAIL")
            .IsRequired();

        builder.Property(e => e.TargetValue)
            .HasColumnName("TARGET_VALUE")
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.Environment)
            .HasColumnName("ENVIRONMENT")
            .HasMaxLength(50);

        builder.Property(e => e.Notes)
            .HasColumnName("NOTES")
            .HasMaxLength(1000);

        // Audit fields
        builder.Property(e => e.CreatedBy)
            .HasColumnName("CREATED_BY")
            .HasMaxLength(50);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("UPDATED_BY")
            .HasMaxLength(50);

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UPDATED_AT")
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .HasColumnName("ROW_VERSION")
            .IsRowVersion();

        // Indexes
        builder.HasIndex(e => new { e.ComponentId, e.MetricType })
            .HasDatabaseName("IX_PERFORMANCE_METRICS_ComponentType");

        builder.HasIndex(e => e.MeasurementTimestamp)
            .HasDatabaseName("IX_PERFORMANCE_METRICS_Timestamp");

        builder.HasIndex(e => e.PassFail)
            .HasDatabaseName("IX_PERFORMANCE_METRICS_PassFail");

        // Foreign Key to ComponentMigration
        builder.HasOne(e => e.Component)
            .WithMany(c => c.PerformanceMetrics)
            .HasForeignKey(e => e.ComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties
        builder.Ignore(e => e.ImprovementPercentage);
        builder.Ignore(e => e.Difference);
        builder.Ignore(e => e.IsImproved);
        builder.Ignore(e => e.PerformanceRating);
    }
}
