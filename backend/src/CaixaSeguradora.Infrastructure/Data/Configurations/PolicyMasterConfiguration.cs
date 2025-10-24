using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for PolicyMaster (TAPOLICE)
/// Configures composite primary key and foreign key to BranchMaster
/// </summary>
public class PolicyMasterConfiguration : IEntityTypeConfiguration<PolicyMaster>
{
    public void Configure(EntityTypeBuilder<PolicyMaster> builder)
    {
        // Table mapping
        builder.ToTable("TAPOLICE");

        // Composite Primary Key (3 components)
        builder.HasKey(e => new { e.Orgapo, e.Rmoapo, e.Numapol });

        // Column mappings
        builder.Property(e => e.Orgapo)
            .HasColumnName("ORGAPO")
            .IsRequired();

        builder.Property(e => e.Rmoapo)
            .HasColumnName("RMOAPO")
            .IsRequired();

        builder.Property(e => e.Numapol)
            .HasColumnName("NUMAPOL")
            .IsRequired();

        builder.Property(e => e.Nome)
            .HasColumnName("NOME")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Cpfcnpj)
            .HasColumnName("CPFCNPJ")
            .HasMaxLength(20);

        builder.Property(e => e.Dtinivig)
            .HasColumnName("DTINIVIG")
            .HasColumnType("date");

        builder.Property(e => e.Dtfimvig)
            .HasColumnName("DTFIMVIG")
            .HasColumnType("date");

        builder.Property(e => e.Situacao)
            .HasColumnName("SITUACAO")
            .HasMaxLength(1);

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

        // Index on insured name
        builder.HasIndex(e => e.Nome)
            .HasDatabaseName("IX_TAPOLICE_InsuredName");

        // Index on CPF/CNPJ
        builder.HasIndex(e => e.Cpfcnpj)
            .HasDatabaseName("IX_TAPOLICE_TaxId")
            .HasFilter("[CPFCNPJ] IS NOT NULL");

        // Foreign Key to BranchMaster
        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.Rmoapo)
            .HasPrincipalKey(b => b.Rmosin)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed properties
        builder.Ignore(e => e.IsActive);
    }
}
