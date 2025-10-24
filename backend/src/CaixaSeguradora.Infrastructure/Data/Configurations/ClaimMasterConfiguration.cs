using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Type Configuration for ClaimMaster (TMESTSIN)
/// Configures composite primary key, relationships, and indexes
/// </summary>
public class ClaimMasterConfiguration : IEntityTypeConfiguration<ClaimMaster>
{
    public void Configure(EntityTypeBuilder<ClaimMaster> builder)
    {
        // Table mapping
        builder.ToTable("TMESTSIN");

        // Composite Primary Key (4 components)
        builder.HasKey(e => new { e.Tipseg, e.Orgsin, e.Rmosin, e.Numsin });

        // Column mappings with explicit names
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

        builder.Property(e => e.Fonte)
            .HasColumnName("FONTE")
            .IsRequired();

        builder.Property(e => e.Protsini)
            .HasColumnName("PROTSINI")
            .IsRequired();

        builder.Property(e => e.Dac)
            .HasColumnName("DAC")
            .IsRequired();

        builder.Property(e => e.Orgapo)
            .HasColumnName("ORGAPO")
            .IsRequired();

        builder.Property(e => e.Rmoapo)
            .HasColumnName("RMOAPO")
            .IsRequired();

        builder.Property(e => e.Numapol)
            .HasColumnName("NUMAPOL")
            .IsRequired();

        builder.Property(e => e.Codprodu)
            .HasColumnName("CODPRODU")
            .IsRequired();

        // Decimal properties with precision (15,2)
        builder.Property(e => e.Sdopag)
            .HasColumnName("SDOPAG")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.Totpag)
            .HasColumnName("TOTPAG")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.Codlider)
            .HasColumnName("CODLIDER");

        builder.Property(e => e.Sinlid)
            .HasColumnName("SINLID");

        builder.Property(e => e.Ocorhist)
            .HasColumnName("OCORHIST")
            .IsRequired();

        builder.Property(e => e.Tipreg)
            .HasColumnName("TIPREG")
            .HasMaxLength(1)
            .IsRequired();

        builder.Property(e => e.Tpsegu)
            .HasColumnName("TPSEGU")
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

        // Indexes for performance-critical queries
        builder.HasIndex(e => new { e.Fonte, e.Protsini, e.Dac })
            .HasDatabaseName("IX_TMESTSIN_Protocol");

        builder.HasIndex(e => new { e.Codlider, e.Sinlid })
            .HasDatabaseName("IX_TMESTSIN_LeaderClaim")
            .HasFilter("[CODLIDER] IS NOT NULL AND [SINLID] IS NOT NULL");

        builder.HasIndex(e => new { e.Orgapo, e.Rmoapo, e.Numapol })
            .HasDatabaseName("IX_TMESTSIN_Policy");

        // Relationships
        // Relationship with BranchMaster
        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.Rmosin)
            .HasPrincipalKey(b => b.Rmosin)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with PolicyMaster
        builder.HasOne(e => e.Policy)
            .WithMany()
            .HasForeignKey(e => new { e.Orgapo, e.Rmoapo, e.Numapol })
            .HasPrincipalKey(p => new { p.Orgapo, p.Rmoapo, p.Numapol })
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation properties - One-to-Many relationships
        builder.HasMany(e => e.ClaimHistories)
            .WithOne(h => h.Claim)
            .HasForeignKey(h => new { h.Tipseg, h.Orgsin, h.Rmosin, h.Numsin })
            .OnDelete(DeleteBehavior.Cascade);

        // Note: ClaimAccompaniments and ClaimPhases use protocol keys (Fonte, Protsini, Dac)
        // not the ClaimMaster primary key (Tipseg, Orgsin, Rmosin, Numsin)
        // The relationships exist conceptually but cannot be enforced by FK constraints
        // These will be managed through application-level logic

        // Ignore computed properties
        builder.Ignore(e => e.PendingValue);
        builder.Ignore(e => e.IsConsortiumProduct);
    }
}
