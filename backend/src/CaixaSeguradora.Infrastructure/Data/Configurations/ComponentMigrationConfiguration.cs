using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for ComponentMigration (COMPONENT_MIGRATION)
/// Configures Guid primary key, FK to MigrationStatus, and decimal precision for hours
/// </summary>
public class ComponentMigrationConfiguration : IEntityTypeConfiguration<ComponentMigration>
{
    public void Configure(EntityTypeBuilder<ComponentMigration> builder)
    {
        // Table mapping
        builder.ToTable("COMPONENT_MIGRATION");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Column mappings
        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .IsRequired();

        builder.Property(e => e.MigrationStatusId)
            .HasColumnName("MIGRATION_STATUS_ID")
            .IsRequired();

        builder.Property(e => e.ComponentType)
            .HasColumnName("COMPONENT_TYPE")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ComponentName)
            .HasColumnName("COMPONENT_NAME")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.LegacyReference)
            .HasColumnName("LEGACY_REFERENCE")
            .HasMaxLength(200);

        builder.Property(e => e.Status)
            .HasColumnName("STATUS")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("Not Started");

        builder.Property(e => e.EstimatedHours)
            .HasColumnName("ESTIMATED_HOURS")
            .HasColumnType("decimal(8,2)");

        builder.Property(e => e.ActualHours)
            .HasColumnName("ACTUAL_HOURS")
            .HasColumnType("decimal(8,2)");

        builder.Property(e => e.Complexity)
            .HasColumnName("COMPLEXITY")
            .HasMaxLength(20);

        builder.Property(e => e.AssignedDeveloper)
            .HasColumnName("ASSIGNED_DEVELOPER")
            .HasMaxLength(100);

        builder.Property(e => e.TechnicalNotes)
            .HasColumnName("TECHNICAL_NOTES")
            .HasMaxLength(2000);

        builder.Property(e => e.StartDate)
            .HasColumnName("START_DATE");

        builder.Property(e => e.CompletionDate)
            .HasColumnName("COMPLETION_DATE");

        builder.Property(e => e.Dependencies)
            .HasColumnName("DEPENDENCIES")
            .HasMaxLength(500);

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
        builder.HasIndex(e => new { e.MigrationStatusId, e.ComponentType })
            .HasDatabaseName("IX_COMPONENT_MIGRATION_StatusType");

        builder.HasIndex(e => new { e.Status, e.AssignedDeveloper })
            .HasDatabaseName("IX_COMPONENT_MIGRATION_StatusDev");

        builder.HasIndex(e => new { e.ComponentType, e.Complexity })
            .HasDatabaseName("IX_COMPONENT_MIGRATION_TypeComplexity");

        // Foreign Key to MigrationStatus
        builder.HasOne(e => e.MigrationStatus)
            .WithMany(m => m.Components)
            .HasForeignKey(e => e.MigrationStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation properties
        builder.HasMany(e => e.PerformanceMetrics)
            .WithOne(p => p.Component)
            .HasForeignKey(p => p.ComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties
        builder.Ignore(e => e.EffortVariance);
        builder.Ignore(e => e.EffortVariancePercentage);
        builder.Ignore(e => e.DurationInDays);
        builder.Ignore(e => e.IsCompleted);
        builder.Ignore(e => e.IsBlocked);
    }
}
