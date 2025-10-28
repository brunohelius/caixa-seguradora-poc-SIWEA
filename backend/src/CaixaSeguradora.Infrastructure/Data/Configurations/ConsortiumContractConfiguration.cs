using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for ConsortiumContract (EF_CONTR_SEG_HABIT)
/// Configures simple integer primary key and indexes
/// </summary>
public class ConsortiumContractConfiguration : IEntityTypeConfiguration<ConsortiumContract>
{
    public void Configure(EntityTypeBuilder<ConsortiumContract> builder)
    {
        // Table mapping
        builder.ToTable("EF_CONTR_SEG_HABIT");

        // Primary Key
        builder.HasKey(e => e.NumContrato);

        // Column mappings
        builder.Property(e => e.NumContrato)
            .HasColumnName("NUM_CONTRATO")
            .IsRequired();

        builder.Property(e => e.NumProposta)
            .HasColumnName("NUM_PROPOSTA")
            .IsRequired();

        builder.Property(e => e.CpfCnpjSegurado)
            .HasColumnName("CPF_CNPJ_SEGURADO")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.NomeSegurado)
            .HasColumnName("NOME_SEGURADO")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.NumApolice)
            .HasColumnName("NUM_APOLICE");

        builder.Property(e => e.DataInicioVigencia)
            .HasColumnName("DATA_INICIO_VIGENCIA")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.DataFimVigencia)
            .HasColumnName("DATA_FIM_VIGENCIA")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.ValorSegurado)
            .HasColumnName("VALOR_SEGURADO")
            .HasColumnType("decimal(15,2)");

        builder.Property(e => e.CodProduto)
            .HasColumnName("COD_PRODUTO")
            .IsRequired();

        builder.Property(e => e.SituacaoContrato)
            .HasColumnName("SITUACAO_CONTRATO")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("A");

        builder.Property(e => e.TipoContrato)
            .HasColumnName("TIPO_CONTRATO")
            .HasMaxLength(10);

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
        builder.HasIndex(e => e.NumProposta)
            .HasDatabaseName("IX_EF_CONTR_SEG_HABIT_Proposal");

        builder.HasIndex(e => e.CpfCnpjSegurado)
            .HasDatabaseName("IX_EF_CONTR_SEG_HABIT_TaxId");

        // Ignore computed properties
        builder.Ignore(e => e.IsEfpContract);
        builder.Ignore(e => e.IsHbContract);
        builder.Ignore(e => e.IsActive);
    }
}
