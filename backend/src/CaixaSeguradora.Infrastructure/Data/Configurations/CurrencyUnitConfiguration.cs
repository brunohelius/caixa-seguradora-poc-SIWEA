using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for CurrencyUnit (TGEUNIMO)
/// Configures date-based primary key and decimal precision for conversion rates
/// </summary>
public class CurrencyUnitConfiguration : IEntityTypeConfiguration<CurrencyUnit>
{
    public void Configure(EntityTypeBuilder<CurrencyUnit> builder)
    {
        // Table mapping
        builder.ToTable("TGEUNIMO");

        // Primary Key (Date-based)
        builder.HasKey(e => e.Dtinivig);

        // Column mappings
        builder.Property(e => e.Dtinivig)
            .HasColumnName("DTINIVIG")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.Dttervig)
            .HasColumnName("DTTERVIG")
            .HasColumnType("date")
            .IsRequired();

        // Decimal property with precision (18,6) for high-precision rates
        builder.Property(e => e.Vlcruzad)
            .HasColumnName("VLCRUZAD")
            .HasColumnType("decimal(18,6)")
            .IsRequired();

        builder.Property(e => e.Codmoeda)
            .HasColumnName("CODMOEDA")
            .HasMaxLength(10);

        builder.Property(e => e.Nomemoeda)
            .HasColumnName("NOMEMOEDA")
            .HasMaxLength(50);

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

        // Index for validity period queries
        builder.HasIndex(e => new { e.Dtinivig, e.Dttervig })
            .HasDatabaseName("IX_TGEUNIMO_ValidityPeriod");
    }
}
