using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for ClaimPhase (SI_SINISTRO_FASE)
/// Configures complex composite primary key (7 components) with FK to ClaimMaster and PhaseEventRelationship
/// </summary>
public class ClaimPhaseConfiguration : IEntityTypeConfiguration<ClaimPhase>
{
    public void Configure(EntityTypeBuilder<ClaimPhase> builder)
    {
        // Table mapping
        builder.ToTable("SI_SINISTRO_FASE");

        // Composite Primary Key (7 components)
        builder.HasKey(e => new
        {
            e.Fonte,
            e.Protsini,
            e.Dac,
            e.CodFase,
            e.CodEvento,
            e.NumOcorrSiniaco,
            e.DataInivigRefaev
        });

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

        builder.Property(e => e.CodFase)
            .HasColumnName("COD_FASE")
            .IsRequired();

        builder.Property(e => e.CodEvento)
            .HasColumnName("COD_EVENTO")
            .IsRequired();

        builder.Property(e => e.NumOcorrSiniaco)
            .HasColumnName("NUM_OCORR_SINIACO")
            .IsRequired();

        builder.Property(e => e.DataInivigRefaev)
            .HasColumnName("DATA_INIVIG_REFAEV")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.DataAberturaSifa)
            .HasColumnName("DATA_ABERTURA_SIFA")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.DataFechaSifa)
            .HasColumnName("DATA_FECHA_SIFA")
            .HasColumnType("date")
            .IsRequired()
            .HasDefaultValue(new DateTime(9999, 12, 31));

        builder.Property(e => e.NomeFase)
            .HasColumnName("NOME_FASE")
            .HasMaxLength(100);

        builder.Property(e => e.Observacoes)
            .HasColumnName("OBSERVACOES")
            .HasMaxLength(1000);

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
        builder.HasIndex(e => new { e.CodFase, e.DataAberturaSifa })
            .HasDatabaseName("IX_SI_SINISTRO_FASE_PhaseOpening");

        builder.HasIndex(e => e.DataFechaSifa)
            .HasDatabaseName("IX_SI_SINISTRO_FASE_Closing");

        // Foreign Key to PhaseEventRelationship
        builder.HasOne(e => e.PhaseEventRelationship)
            .WithMany()
            .HasForeignKey(e => new { e.CodFase, e.CodEvento, e.DataInivigRefaev })
            .OnDelete(DeleteBehavior.Restrict);

        // Note: Foreign Key to ClaimMaster cannot be configured because:
        // - ClaimPhase uses protocol keys (Fonte, Protsini, Dac)
        // - ClaimMaster primary key is (Tipseg, Orgsin, Rmosin, Numsin)
        // The relationship must be managed at application level through protocol lookup
        builder.Ignore(e => e.Claim);

        // Ignore computed properties
        builder.Ignore(e => e.IsOpen);
        builder.Ignore(e => e.DurationInDays);
    }
}
