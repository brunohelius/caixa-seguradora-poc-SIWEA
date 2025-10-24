using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for ClaimHistory (THISTSIN)
/// Configures composite primary key, foreign key to ClaimMaster, and decimal precision
/// </summary>
public class ClaimHistoryConfiguration : IEntityTypeConfiguration<ClaimHistory>
{
    public void Configure(EntityTypeBuilder<ClaimHistory> builder)
    {
        // Table mapping
        builder.ToTable("THISTSIN");

        // Composite Primary Key (5 components)
        builder.HasKey(e => new { e.Tipseg, e.Orgsin, e.Rmosin, e.Numsin, e.Ocorhist });

        // Column mappings
        builder.Property(e => e.Tipseg)
            .HasColumnName("TIPSEG")
            .IsRequired();

        builder.Property(e => e.Orgsin)
            .HasColumnName("ORGSIN")
            .IsRequired();

        builder.Property(e => e.Rmosin)
            .HasColumnName("RMOSIN")
            .IsRequired();

        builder.Property(e => e.Numsin)
            .HasColumnName("NUMSIN")
            .IsRequired();

        builder.Property(e => e.Ocorhist)
            .HasColumnName("OCORHIST")
            .IsRequired();

        builder.Property(e => e.Operacao)
            .HasColumnName("OPERACAO")
            .IsRequired();

        builder.Property(e => e.Dtmovto)
            .HasColumnName("DTMOVTO")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.Horaoper)
            .HasColumnName("HORAOPER")
            .HasMaxLength(6)
            .IsRequired();

        // Decimal properties with precision (15,2)
        builder.Property(e => e.Valpri)
            .HasColumnName("VALPRI")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.Crrmon)
            .HasColumnName("CRRMON")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.Nomfav)
            .HasColumnName("NOMFAV")
            .HasMaxLength(100);

        builder.Property(e => e.Tipcrr)
            .HasColumnName("TIPCRR")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("5");

        builder.Property(e => e.Valpribt)
            .HasColumnName("VALPRIBT")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.Crrmonbt)
            .HasColumnName("CRRMONBT")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.Valtotbt)
            .HasColumnName("VALTOTBT")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.Sitcontb)
            .HasColumnName("SITCONTB")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("0");

        builder.Property(e => e.Situacao)
            .HasColumnName("SITUACAO")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("0");

        builder.Property(e => e.Ezeusrid)
            .HasColumnName("EZEUSRID")
            .HasMaxLength(50)
            .IsRequired();

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
        builder.HasIndex(e => new { e.Dtmovto, e.Operacao })
            .HasDatabaseName("IX_THISTSIN_DateOperation");

        builder.HasIndex(e => new { e.Ezeusrid, e.Dtmovto })
            .HasDatabaseName("IX_THISTSIN_UserDate");

        // Foreign Key to ClaimMaster
        builder.HasOne(e => e.Claim)
            .WithMany(c => c.ClaimHistories)
            .HasForeignKey(e => new { e.Tipseg, e.Orgsin, e.Rmosin, e.Numsin })
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties
        builder.Ignore(e => e.TransactionTimestamp);
    }
}
