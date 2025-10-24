using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for MigrationStatus (MIGRATION_STATUS)
/// Configures Guid primary key, unique index on UserStoryCode, and decimal precision
/// </summary>
public class MigrationStatusConfiguration : IEntityTypeConfiguration<MigrationStatus>
{
    public void Configure(EntityTypeBuilder<MigrationStatus> builder)
    {
        // Table mapping
        builder.ToTable("MIGRATION_STATUS");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Column mappings
        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .IsRequired();

        builder.Property(e => e.UserStoryCode)
            .HasColumnName("USER_STORY_CODE")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.UserStoryName)
            .HasColumnName("USER_STORY_NAME")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("STATUS")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("Not Started");

        builder.Property(e => e.CompletionPercentage)
            .HasColumnName("COMPLETION_PERCENTAGE")
            .HasColumnType("decimal(5,2)")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.RequirementsCompleted)
            .HasColumnName("REQUIREMENTS_COMPLETED")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.RequirementsTotal)
            .HasColumnName("REQUIREMENTS_TOTAL")
            .IsRequired();

        builder.Property(e => e.TestsPassed)
            .HasColumnName("TESTS_PASSED")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.TestsTotal)
            .HasColumnName("TESTS_TOTAL")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.AssignedTo)
            .HasColumnName("ASSIGNED_TO")
            .HasMaxLength(100);

        builder.Property(e => e.StartDate)
            .HasColumnName("START_DATE");

        builder.Property(e => e.EstimatedCompletion)
            .HasColumnName("ESTIMATED_COMPLETION");

        builder.Property(e => e.ActualCompletion)
            .HasColumnName("ACTUAL_COMPLETION");

        builder.Property(e => e.BlockingIssues)
            .HasColumnName("BLOCKING_ISSUES")
            .HasMaxLength(1000);

        builder.Property(e => e.Priority)
            .HasColumnName("PRIORITY")
            .HasMaxLength(10);

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

        // Unique index on UserStoryCode
        builder.HasIndex(e => e.UserStoryCode)
            .HasDatabaseName("IX_MIGRATION_STATUS_Code")
            .IsUnique();

        // Composite index on Status and EstimatedCompletion
        builder.HasIndex(e => new { e.Status, e.EstimatedCompletion })
            .HasDatabaseName("IX_MIGRATION_STATUS_StatusDate");

        // Navigation properties
        builder.HasMany(e => e.Components)
            .WithOne(c => c.MigrationStatus)
            .HasForeignKey(c => c.MigrationStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties
        builder.Ignore(e => e.TestPassRate);
        builder.Ignore(e => e.RequirementsCompletionRate);
        builder.Ignore(e => e.DaysRemaining);
        builder.Ignore(e => e.IsOverdue);
        builder.Ignore(e => e.IsBlocked);
    }
}
