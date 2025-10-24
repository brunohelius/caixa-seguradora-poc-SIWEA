using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for ClaimAccompaniment (SI_ACOMPANHA_SINI)
/// Configures composite primary key with protocol + event + date, and FK to ClaimMaster
/// </summary>
public class ClaimAccompanimentConfiguration : IEntityTypeConfiguration<ClaimAccompaniment>
{
    public void Configure(EntityTypeBuilder<ClaimAccompaniment> builder)
    {
        // Table mapping
        builder.ToTable("SI_ACOMPANHA_SINI");

        // Composite Primary Key (5 components)
        builder.HasKey(e => new { e.Fonte, e.Protsini, e.Dac, e.CodEvento, e.DataMovtoSiniaco });

        // Column mappings
        builder.Property(e => e.Fonte)
            .HasColumnName("FONTE")
            .IsRequired();

        builder.Property(e => e.Protsini)
            .HasColumnName("PROTSINI")
            .IsRequired();

        builder.Property(e => e.Dac)
            .HasColumnName("DAC")
            .IsRequired();

        builder.Property(e => e.CodEvento)
            .HasColumnName("COD_EVENTO")
            .IsRequired();

        builder.Property(e => e.DataMovtoSiniaco)
            .HasColumnName("DATA_MOVTO_SINIACO")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.NumOcorrSiniaco)
            .HasColumnName("NUM_OCORR_SINIACO")
            .IsRequired();

        builder.Property(e => e.DescrComplementar)
            .HasColumnName("DESCR_COMPLEMENTAR")
            .HasMaxLength(500);

        builder.Property(e => e.CodUsuario)
            .HasColumnName("COD_USUARIO")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.HoraEvento)
            .HasColumnName("HORA_EVENTO")
            .HasMaxLength(6);

        builder.Property(e => e.NomeEvento)
            .HasColumnName("NOME_EVENTO")
            .HasMaxLength(100);

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

        // Indexes
        builder.HasIndex(e => new { e.CodEvento, e.DataMovtoSiniaco })
            .HasDatabaseName("IX_SI_ACOMPANHA_EventDate");

        builder.HasIndex(e => new { e.CodUsuario, e.DataMovtoSiniaco })
            .HasDatabaseName("IX_SI_ACOMPANHA_UserDate");

        // Note: Foreign Key to ClaimMaster cannot be configured because:
        // - ClaimAccompaniment uses protocol keys (Fonte, Protsini, Dac)
        // - ClaimMaster primary key is (Tipseg, Orgsin, Rmosin, Numsin)
        // The relationship must be managed at application level through protocol lookup

        // Ignore the Claim navigation property since no FK can be established
        builder.Ignore(e => e.Claim);

        // Ignore computed properties
        builder.Ignore(e => e.EventTimestamp);
    }
}
