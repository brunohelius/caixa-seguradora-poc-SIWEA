using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for PhaseEventRelationship (SI_REL_FASE_EVENTO)
/// Configures composite primary key (3 components)
/// </summary>
public class PhaseEventRelationshipConfiguration : IEntityTypeConfiguration<PhaseEventRelationship>
{
    public void Configure(EntityTypeBuilder<PhaseEventRelationship> builder)
    {
        // Table mapping
        builder.ToTable("SI_REL_FASE_EVENTO");

        // Composite Primary Key (3 components)
        builder.HasKey(e => new { e.CodFase, e.CodEvento, e.DataInivigRefaev });

        // Column mappings
        builder.Property(e => e.CodFase)
            .HasColumnName("COD_FASE")
            .IsRequired();

        builder.Property(e => e.CodEvento)
            .HasColumnName("COD_EVENTO")
            .IsRequired();

        builder.Property(e => e.DataInivigRefaev)
            .HasColumnName("DATA_INIVIG_REFAEV")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.DataTervigRefaev)
            .HasColumnName("DATA_TERVIG_REFAEV")
            .HasColumnType("date");

        builder.Property(e => e.IndAlteracaoFase)
            .HasColumnName("IND_ALTERACAO_FASE")
            .HasMaxLength(1)
            .IsRequired();

        builder.Property(e => e.NomeFase)
            .HasColumnName("NOME_FASE")
            .HasMaxLength(100);

        builder.Property(e => e.NomeEvento)
            .HasColumnName("NOME_EVENTO")
            .HasMaxLength(100);

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

        // Index
        builder.HasIndex(e => new { e.CodEvento, e.DataInivigRefaev })
            .HasDatabaseName("IX_SI_REL_FASE_EVENTO_EventDate");

        // Ignore computed properties
        builder.Ignore(e => e.OpensPhase);
        builder.Ignore(e => e.ClosesPhase);
    }
}
