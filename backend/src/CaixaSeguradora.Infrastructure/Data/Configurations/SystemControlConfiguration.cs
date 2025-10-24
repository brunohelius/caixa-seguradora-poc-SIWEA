using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for SystemControl (TSISTEMA)
/// Configures simple string primary key and date columns
/// </summary>
public class SystemControlConfiguration : IEntityTypeConfiguration<SystemControl>
{
    public void Configure(EntityTypeBuilder<SystemControl> builder)
    {
        // Table mapping
        builder.ToTable("TSISTEMA");

        // Primary Key
        builder.HasKey(e => e.Idsistem);

        // Column mappings
        builder.Property(e => e.Idsistem)
            .HasColumnName("IDSISTEM")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.Dtmovabe)
            .HasColumnName("DTMOVABE")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.Nomesist)
            .HasColumnName("NOMESIST")
            .HasMaxLength(100);

        builder.Property(e => e.Ativo)
            .HasColumnName("ATIVO")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.Dtfecha)
            .HasColumnName("DTFECHA")
            .HasColumnType("date");

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
    }
}
