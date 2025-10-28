using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for BranchMaster (TGERAMO)
/// Configures simple primary key and column mappings
/// </summary>
public class BranchMasterConfiguration : IEntityTypeConfiguration<BranchMaster>
{
    public void Configure(EntityTypeBuilder<BranchMaster> builder)
    {
        // Table mapping
        builder.ToTable("TGERAMO");

        // Primary Key
        builder.HasKey(e => e.Rmosin);

        // Column mappings
        builder.Property(e => e.Rmosin)
            .HasColumnName("RMOSIN")
            .IsRequired();

        builder.Property(e => e.Nomeramo)
            .HasColumnName("NOMERAMO")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Descramo)
            .HasColumnName("DESCRAMO")
            .HasMaxLength(500);

        builder.Property(e => e.Ativo)
            .HasColumnName("ATIVO")
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(e => e.CreatedBy)
            .HasColumnName("CREATED_BY")
            .HasMaxLength(50);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CREATED_AT");

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("UPDATED_BY")
            .HasMaxLength(50);

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UPDATED_AT");

        builder.Property(e => e.RowVersion)
            .HasColumnName("ROW_VERSION")
            .IsRowVersion();

        // Index on Name for search performance
        builder.HasIndex(e => e.Nomeramo)
            .HasDatabaseName("IX_TGERAMO_Name");
    }
}
