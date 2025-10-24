# Data Model Specification: Claims System Migration

**Project**: Visual Age to .NET 9 Migration
**Feature**: 001-visualage-dotnet-migration
**Created**: 2025-10-23
**Status**: Production-Ready
**Framework**: .NET 9, Entity Framework Core 9

---

## Table of Contents

1. [Overview](#overview)
2. [Entity Relationship Diagram](#entity-relationship-diagram)
3. [Legacy Entities (10)](#legacy-entities)
4. [Dashboard Entities (3)](#dashboard-entities)
5. [DbContext Configuration](#dbcontext-configuration)
6. [Index Recommendations](#index-recommendations)
7. [Migration Strategy](#migration-strategy)
8. [Usage Examples](#usage-examples)

---

## Overview

This document defines 13 entities for the Claims System migration from IBM Visual Age to .NET 9:

- **10 Legacy Entities**: Map to existing DB2/SQL Server tables
- **3 Dashboard Entities**: New tables for migration tracking

All entities follow .NET 9 and EF Core 9 best practices with:
- Fluent API configuration for legacy table mapping
- Data annotations for validation
- Navigation properties for relationships
- Audit fields (CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
- Concurrency handling with RowVersion
- XML documentation for Portuguese field names

---

## Entity Relationship Diagram

```mermaid
erDiagram
    TMESTSIN ||--o{ THISTSIN : "has history"
    TMESTSIN ||--o{ SI_ACOMPANHA_SINI : "has accompaniment"
    TMESTSIN ||--o{ SI_SINISTRO_FASE : "has phases"
    TMESTSIN }o--|| TGERAMO : "belongs to branch"
    TMESTSIN }o--|| TAPOLICE : "references policy"
    TAPOLICE }o--|| TGERAMO : "belongs to branch"
    SI_ACOMPANHA_SINI }o--|| SI_REL_FASE_EVENTO : "triggers phase change"
    SI_SINISTRO_FASE }o--|| SI_REL_FASE_EVENTO : "defined by"
    THISTSIN }o--|| TGEUNIMO : "uses currency rate"
    TMESTSIN }o--o| EF_CONTR_SEG_HABIT : "may have consortium contract"

    MIGRATION_STATUS ||--o{ COMPONENT_MIGRATION : "tracks components"
    COMPONENT_MIGRATION ||--o{ PERFORMANCE_METRICS : "measures performance"

    TMESTSIN {
        int TIPSEG PK "Insurance Type"
        int ORGSIN PK "Origin"
        int RMOSIN PK "Branch"
        int NUMSIN PK "Claim Number"
        int FONTE "Protocol Source"
        int PROTSINI "Protocol Number"
        int DAC "Check Digit"
        int ORGAPO "Policy Origin"
        int RMOAPO "Policy Branch"
        int NUMAPOL "Policy Number"
        int CODPRODU "Product Code"
        decimal SDOPAG "Expected Reserve"
        decimal TOTPAG "Total Payments"
        int CODLIDER "Leader Code"
        int SINLID "Leader Claim"
        int OCORHIST "Occurrence Counter"
        string TIPREG "Policy Type"
    }

    THISTSIN {
        int TIPSEG PK "Insurance Type"
        int ORGSIN PK "Origin"
        int RMOSIN PK "Branch"
        int NUMSIN PK "Claim Number"
        int OCORHIST PK "Occurrence"
        int OPERACAO "Operation Code"
        date DTMOVTO "Transaction Date"
        time HORAOPER "Transaction Time"
        decimal VALPRI "Principal Amount"
        decimal CRRMON "Correction Amount"
        string NOMFAV "Beneficiary Name"
        string TIPCRR "Correction Type"
        decimal VALPRIBT "Principal in BTNF"
        decimal CRRMONBT "Correction in BTNF"
        decimal VALTOTBT "Total in BTNF"
        string SITCONTB "Accounting Status"
        string SITUACAO "Overall Status"
        string EZEUSRID "Operator ID"
    }

    TGERAMO {
        int RMOSIN PK "Branch Code"
        string NOMERAMO "Branch Name"
    }

    TGEUNIMO {
        date DTINIVIG PK "Start Date"
        date DTTERVIG "End Date"
        decimal VLCRUZAD "Conversion Rate"
    }

    TSISTEMA {
        string IDSISTEM PK "System ID"
        date DTMOVABE "Business Date"
    }

    TAPOLICE {
        int ORGAPO PK "Origin"
        int RMOAPO PK "Branch"
        int NUMAPOL PK "Policy Number"
        string NOME "Insured Name"
    }

    SI_ACOMPANHA_SINI {
        int FONTE PK "Protocol Source"
        int PROTSINI PK "Protocol Number"
        int DAC PK "Check Digit"
        int COD_EVENTO PK "Event Code"
        date DATA_MOVTO_SINIACO PK "Transaction Date"
        int NUM_OCORR_SINIACO "Occurrence"
        string DESCR_COMPLEMENTAR "Description"
        string COD_USUARIO "User ID"
    }

    SI_SINISTRO_FASE {
        int FONTE PK "Protocol Source"
        int PROTSINI PK "Protocol Number"
        int DAC PK "Check Digit"
        int COD_FASE PK "Phase Code"
        int COD_EVENTO PK "Event Code"
        int NUM_OCORR_SINIACO PK "Event Occurrence"
        date DATA_INIVIG_REFAEV PK "Effective Date"
        date DATA_ABERTURA_SIFA "Opening Date"
        date DATA_FECHA_SIFA "Closing Date"
    }

    SI_REL_FASE_EVENTO {
        int COD_FASE PK "Phase Code"
        int COD_EVENTO PK "Event Code"
        date DATA_INIVIG_REFAEV PK "Effective Date"
        string IND_ALTERACAO_FASE "Phase Indicator"
    }

    EF_CONTR_SEG_HABIT {
        int NUM_CONTRATO PK "Contract Number"
    }

    MIGRATION_STATUS {
        guid ID PK
        string USER_STORY_CODE UK
        string USER_STORY_NAME
        string STATUS
        decimal COMPLETION_PERCENTAGE
        int REQUIREMENTS_COMPLETED
        int REQUIREMENTS_TOTAL
        int TESTS_PASSED
        int TESTS_TOTAL
        string ASSIGNED_TO
        datetime START_DATE
        datetime ESTIMATED_COMPLETION
        datetime ACTUAL_COMPLETION
        string BLOCKING_ISSUES
    }

    COMPONENT_MIGRATION {
        guid ID PK
        guid MIGRATION_STATUS_ID FK
        string COMPONENT_TYPE
        string COMPONENT_NAME
        string LEGACY_REFERENCE
        string STATUS
        decimal ESTIMATED_HOURS
        decimal ACTUAL_HOURS
        string COMPLEXITY
        string ASSIGNED_DEVELOPER
        string TECHNICAL_NOTES
    }

    PERFORMANCE_METRICS {
        guid ID PK
        guid COMPONENT_ID FK
        string METRIC_TYPE
        decimal LEGACY_VALUE
        decimal NEW_VALUE
        datetime MEASUREMENT_TIMESTAMP
        string TEST_SCENARIO
        bool PASS_FAIL
    }
```

---

## Legacy Entities

### 1. Claim Master (TMESTSIN)

**Purpose**: Main claim record containing protocol identification, financial summary, and policy references.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Claim Master - Main claim record (Legacy table: TMESTSIN)
/// Represents the primary claim entity with protocol identification, policy references, and financial summary.
/// </summary>
[Table("TMESTSIN")]
[Index(nameof(Fonte), nameof(Protsini), nameof(Dac), Name = "IX_TMESTSIN_Protocol")]
[Index(nameof(Codlider), nameof(Sinlid), Name = "IX_TMESTSIN_Leader")]
[Index(nameof(Orgapo), nameof(Rmoapo), nameof(Numapol), Name = "IX_TMESTSIN_Policy")]
public class ClaimMaster
{
    /// <summary>
    /// Tipo de Seguro - Insurance Type (Primary Key)
    /// </summary>
    [Key]
    [Column("TIPSEG", Order = 0)]
    [Required]
    public int Tipseg { get; set; }

    /// <summary>
    /// Órgão do Sinistro - Claim Origin (Primary Key)
    /// </summary>
    [Key]
    [Column("ORGSIN", Order = 1)]
    [Required]
    public int Orgsin { get; set; }

    /// <summary>
    /// Ramo do Sinistro - Claim Branch (Primary Key)
    /// </summary>
    [Key]
    [Column("RMOSIN", Order = 2)]
    [Required]
    public int Rmosin { get; set; }

    /// <summary>
    /// Número do Sinistro - Claim Number (Primary Key)
    /// </summary>
    [Key]
    [Column("NUMSIN", Order = 3)]
    [Required]
    public int Numsin { get; set; }

    /// <summary>
    /// Fonte do Protocolo - Protocol Source
    /// </summary>
    [Column("FONTE")]
    [Required]
    public int Fonte { get; set; }

    /// <summary>
    /// Número do Protocolo - Protocol Number
    /// </summary>
    [Column("PROTSINI")]
    [Required]
    public int Protsini { get; set; }

    /// <summary>
    /// Dígito de Controle - Check Digit
    /// </summary>
    [Column("DAC")]
    [Required]
    public int Dac { get; set; }

    /// <summary>
    /// Órgão da Apólice - Policy Origin
    /// </summary>
    [Column("ORGAPO")]
    [Required]
    public int Orgapo { get; set; }

    /// <summary>
    /// Ramo da Apólice - Policy Branch
    /// </summary>
    [Column("RMOAPO")]
    [Required]
    public int Rmoapo { get; set; }

    /// <summary>
    /// Número da Apólice - Policy Number
    /// </summary>
    [Column("NUMAPOL")]
    [Required]
    public int Numapol { get; set; }

    /// <summary>
    /// Código do Produto - Product Code
    /// </summary>
    [Column("CODPRODU")]
    [Required]
    public int Codprodu { get; set; }

    /// <summary>
    /// Saldo a Pagar - Expected Reserve Amount
    /// </summary>
    [Column("SDOPAG", TypeName = "decimal(15,2)")]
    [Required]
    [Range(0, 999999999999.99)]
    public decimal Sdopag { get; set; }

    /// <summary>
    /// Total Pago - Total Payments Made
    /// </summary>
    [Column("TOTPAG", TypeName = "decimal(15,2)")]
    [Required]
    [Range(0, 999999999999.99)]
    public decimal Totpag { get; set; }

    /// <summary>
    /// Código do Líder - Leader Code (for reinsurance)
    /// </summary>
    [Column("CODLIDER")]
    public int? Codlider { get; set; }

    /// <summary>
    /// Sinistro do Líder - Leader Claim Number
    /// </summary>
    [Column("SINLID")]
    public int? Sinlid { get; set; }

    /// <summary>
    /// Ocorrência do Histórico - History Occurrence Counter
    /// Used to track the number of history records
    /// </summary>
    [Column("OCORHIST")]
    [Required]
    public int Ocorhist { get; set; }

    /// <summary>
    /// Tipo de Registro - Policy Type Indicator
    /// Values: '1' or '2'
    /// </summary>
    [Column("TIPREG")]
    [Required]
    [StringLength(1)]
    [RegularExpression("^[12]$", ErrorMessage = "Policy Type must be '1' or '2'")]
    public string Tipreg { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de Seguro (from policy) - Insurance Type from Policy
    /// Used for beneficiary validation: 0 = optional, != 0 = mandatory
    /// </summary>
    [Column("TPSEGU")]
    [Required]
    public int Tpsegu { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Branch information
    /// </summary>
    [ForeignKey(nameof(Rmosin))]
    public virtual BranchMaster? Branch { get; set; }

    /// <summary>
    /// Policy information
    /// </summary>
    [ForeignKey($"{nameof(Orgapo)},{nameof(Rmoapo)},{nameof(Numapol)}")]
    public virtual PolicyMaster? Policy { get; set; }

    /// <summary>
    /// Claim history records
    /// </summary>
    public virtual ICollection<ClaimHistory> ClaimHistories { get; set; } = new List<ClaimHistory>();

    /// <summary>
    /// Claim accompaniment records
    /// </summary>
    public virtual ICollection<ClaimAccompaniment> ClaimAccompaniments { get; set; } = new List<ClaimAccompaniment>();

    /// <summary>
    /// Claim phase records
    /// </summary>
    public virtual ICollection<ClaimPhase> ClaimPhases { get; set; } = new List<ClaimPhase>();

    /// <summary>
    /// Calculated pending value (Sdopag - Totpag)
    /// </summary>
    [NotMapped]
    public decimal PendingValue => Sdopag - Totpag;

    /// <summary>
    /// Check if this is a consortium product
    /// </summary>
    [NotMapped]
    public bool IsConsortiumProduct => Codprodu == 6814 || Codprodu == 7701 || Codprodu == 7709;
}
```

**EF Core Fluent API Configuration:**

```csharp
public class ClaimMasterConfiguration : IEntityTypeConfiguration<ClaimMaster>
{
    public void Configure(EntityTypeBuilder<ClaimMaster> builder)
    {
        // Composite Primary Key
        builder.HasKey(e => new { e.Tipseg, e.Orgsin, e.Rmosin, e.Numsin });

        // Relationships
        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.Rmosin)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Policy)
            .WithMany()
            .HasForeignKey(e => new { e.Orgapo, e.Rmoapo, e.Numapol })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ClaimHistories)
            .WithOne(h => h.Claim)
            .HasForeignKey(h => new { h.Tipseg, h.Orgsin, h.Rmosin, h.Numsin })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ClaimAccompaniments)
            .WithOne(a => a.Claim)
            .HasForeignKey(a => new { a.Fonte, a.Protsini, a.Dac })
            .HasPrincipalKey(c => new { c.Fonte, c.Protsini, c.Dac })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ClaimPhases)
            .WithOne(p => p.Claim)
            .HasForeignKey(p => new { p.Fonte, p.Protsini, p.Dac })
            .HasPrincipalKey(c => new { c.Fonte, c.Protsini, c.Dac })
            .OnDelete(DeleteBehavior.Cascade);

        // Default Values
        builder.Property(e => e.Ocorhist).HasDefaultValue(0);
        builder.Property(e => e.Sdopag).HasDefaultValue(0m);
        builder.Property(e => e.Totpag).HasDefaultValue(0m);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
    }
}
```

---

### 2. Claim History (THISTSIN)

**Purpose**: Individual payment authorization transactions with financial details and audit information.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Claim History - Payment authorization transactions (Legacy table: THISTSIN)
/// Records each payment authorization with amounts, beneficiary, and operator information.
/// </summary>
[Table("THISTSIN")]
[Index(nameof(Dtmovto), nameof(Operacao), Name = "IX_THISTSIN_DateOperation")]
[Index(nameof(Ezeusrid), nameof(Dtmovto), Name = "IX_THISTSIN_UserDate")]
public class ClaimHistory
{
    /// <summary>
    /// Tipo de Seguro - Insurance Type (Primary Key)
    /// </summary>
    [Key]
    [Column("TIPSEG", Order = 0)]
    [Required]
    public int Tipseg { get; set; }

    /// <summary>
    /// Órgão do Sinistro - Claim Origin (Primary Key)
    /// </summary>
    [Key]
    [Column("ORGSIN", Order = 1)]
    [Required]
    public int Orgsin { get; set; }

    /// <summary>
    /// Ramo do Sinistro - Claim Branch (Primary Key)
    /// </summary>
    [Key]
    [Column("RMOSIN", Order = 2)]
    [Required]
    public int Rmosin { get; set; }

    /// <summary>
    /// Número do Sinistro - Claim Number (Primary Key)
    /// </summary>
    [Key]
    [Column("NUMSIN", Order = 3)]
    [Required]
    public int Numsin { get; set; }

    /// <summary>
    /// Ocorrência do Histórico - Occurrence Sequence (Primary Key)
    /// </summary>
    [Key]
    [Column("OCORHIST", Order = 4)]
    [Required]
    public int Ocorhist { get; set; }

    /// <summary>
    /// Operação - Operation Code
    /// Always 1098 for payment authorization
    /// </summary>
    [Column("OPERACAO")]
    [Required]
    public int Operacao { get; set; }

    /// <summary>
    /// Data do Movimento - Transaction Date
    /// </summary>
    [Column("DTMOVTO", TypeName = "date")]
    [Required]
    public DateTime Dtmovto { get; set; }

    /// <summary>
    /// Hora da Operação - Transaction Time
    /// Format: HHmmss
    /// </summary>
    [Column("HORAOPER")]
    [Required]
    [StringLength(6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Time must be in HHmmss format")]
    public string Horaoper { get; set; } = string.Empty;

    /// <summary>
    /// Valor Principal - Principal Amount (original currency)
    /// </summary>
    [Column("VALPRI", TypeName = "decimal(15,2)")]
    [Required]
    [Range(0.01, 999999999999.99)]
    public decimal Valpri { get; set; }

    /// <summary>
    /// Correção Monetária - Correction Amount (original currency)
    /// </summary>
    [Column("CRRMON", TypeName = "decimal(15,2)")]
    [Range(0, 999999999999.99)]
    public decimal Crrmon { get; set; }

    /// <summary>
    /// Nome do Favorecido - Beneficiary Name
    /// Mandatory when claim insurance type (tpsegu) != 0
    /// </summary>
    [Column("NOMFAV")]
    [StringLength(100)]
    public string? Nomfav { get; set; }

    /// <summary>
    /// Tipo de Correção - Correction Type
    /// Always '5' for payment authorizations
    /// </summary>
    [Column("TIPCRR")]
    [Required]
    [StringLength(1)]
    public string Tipcrr { get; set; } = "5";

    /// <summary>
    /// Valor Principal em BTNF - Principal in standardized currency
    /// </summary>
    [Column("VALPRIBT", TypeName = "decimal(15,2)")]
    [Required]
    public decimal Valpribt { get; set; }

    /// <summary>
    /// Correção em BTNF - Correction in standardized currency
    /// </summary>
    [Column("CRRMONBT", TypeName = "decimal(15,2)")]
    [Required]
    public decimal Crrmonbt { get; set; }

    /// <summary>
    /// Valor Principal Convertido - Converted Principal
    /// </summary>
    [Column("VALPRICONV", TypeName = "decimal(15,2)")]
    [Required]
    public decimal Valpriconv { get; set; }

    /// <summary>
    /// Principal Diário em BTNF - Daily Principal in BTNF
    /// </summary>
    [Column("PRIDIABT", TypeName = "decimal(15,2)")]
    [Required]
    public decimal Pridiabt { get; set; }

    /// <summary>
    /// Correção Diária em BTNF - Daily Correction in BTNF
    /// </summary>
    [Column("CRRDIABT", TypeName = "decimal(15,2)")]
    [Required]
    public decimal Crrdiabt { get; set; }

    /// <summary>
    /// Valor Total em BTNF - Total Value in BTNF
    /// Sum of VALPRIBT + CRRMONBT
    /// </summary>
    [Column("VALTOTBT", TypeName = "decimal(15,2)")]
    [Required]
    public decimal Valtotbt { get; set; }

    /// <summary>
    /// Total Diário em BTNF - Daily Total in BTNF
    /// </summary>
    [Column("TOTDIABT", TypeName = "decimal(15,2)")]
    [Required]
    public decimal Totdiabt { get; set; }

    /// <summary>
    /// Situação Contábil - Accounting Status
    /// Initialized as '0'
    /// </summary>
    [Column("SITCONTB")]
    [Required]
    [StringLength(1)]
    public string Sitcontb { get; set; } = "0";

    /// <summary>
    /// Situação - Overall Status
    /// Initialized as '0'
    /// </summary>
    [Column("SITUACAO")]
    [Required]
    [StringLength(1)]
    public string Situacao { get; set; } = "0";

    /// <summary>
    /// ID do Usuário - Operator User ID
    /// </summary>
    [Column("EZEUSRID")]
    [Required]
    [StringLength(50)]
    public string Ezeusrid { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de Pagamento - Payment Type
    /// Values: 1, 2, 3, 4, or 5
    /// </summary>
    [Column("TIPPAG")]
    [Required]
    [Range(1, 5, ErrorMessage = "Payment Type must be between 1 and 5")]
    public int Tippag { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Parent claim record
    /// </summary>
    [ForeignKey($"{nameof(Tipseg)},{nameof(Orgsin)},{nameof(Rmosin)},{nameof(Numsin)}")]
    public virtual ClaimMaster? Claim { get; set; }

    /// <summary>
    /// Combined transaction timestamp
    /// </summary>
    [NotMapped]
    public DateTime TransactionTimestamp
    {
        get
        {
            if (string.IsNullOrEmpty(Horaoper) || Horaoper.Length != 6)
                return Dtmovto;

            int hour = int.Parse(Horaoper.Substring(0, 2));
            int minute = int.Parse(Horaoper.Substring(2, 2));
            int second = int.Parse(Horaoper.Substring(4, 2));

            return Dtmovto.Date.AddHours(hour).AddMinutes(minute).AddSeconds(second);
        }
    }
}
```

**EF Core Fluent API Configuration:**

```csharp
public class ClaimHistoryConfiguration : IEntityTypeConfiguration<ClaimHistory>
{
    public void Configure(EntityTypeBuilder<ClaimHistory> builder)
    {
        // Composite Primary Key
        builder.HasKey(e => new { e.Tipseg, e.Orgsin, e.Rmosin, e.Numsin, e.Ocorhist });

        // Relationships
        builder.HasOne(e => e.Claim)
            .WithMany(c => c.ClaimHistories)
            .HasForeignKey(e => new { e.Tipseg, e.Orgsin, e.Rmosin, e.Numsin })
            .OnDelete(DeleteBehavior.Cascade);

        // Default Values
        builder.Property(e => e.Operacao).HasDefaultValue(1098);
        builder.Property(e => e.Tipcrr).HasDefaultValue("5");
        builder.Property(e => e.Sitcontb).HasDefaultValue("0");
        builder.Property(e => e.Situacao).HasDefaultValue("0");
        builder.Property(e => e.Crrmon).HasDefaultValue(0m);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

        // Check Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_THISTSIN_PaymentType", "[TIPPAG] >= 1 AND [TIPPAG] <= 5"));
    }
}
```

---

### 3. Branch Master (TGERAMO)

**Purpose**: Branch descriptive information for organizational hierarchy.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Branch Master - Branch descriptive information (Legacy table: TGERAMO)
/// Contains branch names and organizational information.
/// </summary>
[Table("TGERAMO")]
public class BranchMaster
{
    /// <summary>
    /// Código do Ramo - Branch Code (Primary Key)
    /// </summary>
    [Key]
    [Column("RMOSIN")]
    [Required]
    public int Rmosin { get; set; }

    /// <summary>
    /// Nome do Ramo - Branch Name
    /// </summary>
    [Column("NOMERAMO")]
    [Required]
    [StringLength(100)]
    public string Nomeramo { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do Ramo - Branch Description
    /// </summary>
    [Column("DESCRAMO")]
    [StringLength(500)]
    public string? Descramo { get; set; }

    /// <summary>
    /// Ativo - Active Status
    /// </summary>
    [Column("ATIVO")]
    [Required]
    public bool Ativo { get; set; } = true;

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }
}
```

**EF Core Fluent API Configuration:**

```csharp
public class BranchMasterConfiguration : IEntityTypeConfiguration<BranchMaster>
{
    public void Configure(EntityTypeBuilder<BranchMaster> builder)
    {
        builder.HasKey(e => e.Rmosin);

        builder.Property(e => e.Ativo).HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
    }
}
```

---

### 4. Currency Unit (TGEUNIMO)

**Purpose**: Currency conversion rates for standardized monetary calculations.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Currency Unit Table - Conversion rates (Legacy table: TGEUNIMO)
/// Provides currency conversion rates to BTNF (Bônus do Tesouro Nacional Fiscal) with validity periods.
/// </summary>
[Table("TGEUNIMO")]
[Index(nameof(Dtinivig), nameof(Dttervig), Name = "IX_TGEUNIMO_ValidityPeriod")]
public class CurrencyUnit
{
    /// <summary>
    /// Data de Início de Vigência - Start Date (Primary Key)
    /// </summary>
    [Key]
    [Column("DTINIVIG", TypeName = "date", Order = 0)]
    [Required]
    public DateTime Dtinivig { get; set; }

    /// <summary>
    /// Data de Término de Vigência - End Date
    /// </summary>
    [Column("DTTERVIG", TypeName = "date")]
    [Required]
    public DateTime Dttervig { get; set; }

    /// <summary>
    /// Valor em Cruzados - Conversion Rate to BTNF
    /// Used to convert claim amounts to standardized currency
    /// </summary>
    [Column("VLCRUZAD", TypeName = "decimal(18,6)")]
    [Required]
    [Range(0.000001, 999999999999.999999)]
    public decimal Vlcruzad { get; set; }

    /// <summary>
    /// Código da Moeda - Currency Code
    /// </summary>
    [Column("CODMOEDA")]
    [StringLength(10)]
    public string? Codmoeda { get; set; }

    /// <summary>
    /// Nome da Moeda - Currency Name
    /// </summary>
    [Column("NOMEMOEDA")]
    [StringLength(50)]
    public string? Nomemoeda { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Check if date is within validity period
    /// </summary>
    public bool IsValidForDate(DateTime date)
    {
        return date >= Dtinivig && date <= Dttervig;
    }
}
```

**EF Core Fluent API Configuration:**

```csharp
public class CurrencyUnitConfiguration : IEntityTypeConfiguration<CurrencyUnit>
{
    public void Configure(EntityTypeBuilder<CurrencyUnit> builder)
    {
        builder.HasKey(e => e.Dtinivig);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

        // Check Constraint for valid date range
        builder.ToTable(t => t.HasCheckConstraint("CK_TGEUNIMO_DateRange", "[DTTERVIG] >= [DTINIVIG]"));
    }
}
```

---

### 5. System Control (TSISTEMA)

**Purpose**: System-wide configuration including current business date.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// System Control - System-wide configuration (Legacy table: TSISTEMA)
/// Provides current business date for the claims system.
/// </summary>
[Table("TSISTEMA")]
public class SystemControl
{
    /// <summary>
    /// ID do Sistema - System ID (Primary Key)
    /// For claims system: 'SI'
    /// </summary>
    [Key]
    [Column("IDSISTEM")]
    [Required]
    [StringLength(10)]
    public string Idsistem { get; set; } = string.Empty;

    /// <summary>
    /// Data de Movimento em Aberto - Current Business Date
    /// Used to timestamp all transactions
    /// </summary>
    [Column("DTMOVABE", TypeName = "date")]
    [Required]
    public DateTime Dtmovabe { get; set; }

    /// <summary>
    /// Nome do Sistema - System Name
    /// </summary>
    [Column("NOMESIST")]
    [StringLength(100)]
    public string? Nomesist { get; set; }

    /// <summary>
    /// Sistema Ativo - System Active Status
    /// </summary>
    [Column("ATIVO")]
    [Required]
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Data de Fechamento - Last Closing Date
    /// </summary>
    [Column("DTFECHA", TypeName = "date")]
    public DateTime? Dtfecha { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }
}
```

**EF Core Fluent API Configuration:**

```csharp
public class SystemControlConfiguration : IEntityTypeConfiguration<SystemControl>
{
    public void Configure(EntityTypeBuilder<SystemControl> builder)
    {
        builder.HasKey(e => e.Idsistem);

        builder.Property(e => e.Ativo).HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

        // Seed data for Claims System
        builder.HasData(new SystemControl
        {
            Idsistem = "SI",
            Nomesist = "Sistema de Sinistros",
            Dtmovabe = new DateTime(2025, 10, 23),
            Ativo = true
        });
    }
}
```

---

### 6. Policy Master (TAPOLICE)

**Purpose**: Policy information including insured party details.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Policy Master - Policy and insured information (Legacy table: TAPOLICE)
/// Contains policy details and insured party name.
/// </summary>
[Table("TAPOLICE")]
[Index(nameof(Nome), Name = "IX_TAPOLICE_InsuredName")]
public class PolicyMaster
{
    /// <summary>
    /// Órgão da Apólice - Policy Origin (Primary Key)
    /// </summary>
    [Key]
    [Column("ORGAPO", Order = 0)]
    [Required]
    public int Orgapo { get; set; }

    /// <summary>
    /// Ramo da Apólice - Policy Branch (Primary Key)
    /// </summary>
    [Key]
    [Column("RMOAPO", Order = 1)]
    [Required]
    public int Rmoapo { get; set; }

    /// <summary>
    /// Número da Apólice - Policy Number (Primary Key)
    /// </summary>
    [Key]
    [Column("NUMAPOL", Order = 2)]
    [Required]
    public int Numapol { get; set; }

    /// <summary>
    /// Nome do Segurado - Insured Name
    /// </summary>
    [Column("NOME")]
    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF/CNPJ do Segurado - Insured Tax ID
    /// </summary>
    [Column("CPFCNPJ")]
    [StringLength(20)]
    public string? Cpfcnpj { get; set; }

    /// <summary>
    /// Data de Início de Vigência - Policy Start Date
    /// </summary>
    [Column("DTINIVIG", TypeName = "date")]
    public DateTime? Dtinivig { get; set; }

    /// <summary>
    /// Data de Fim de Vigência - Policy End Date
    /// </summary>
    [Column("DTFIMVIG", TypeName = "date")]
    public DateTime? Dtfimvig { get; set; }

    /// <summary>
    /// Situação da Apólice - Policy Status
    /// </summary>
    [Column("SITUACAO")]
    [StringLength(1)]
    public string? Situacao { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Branch information
    /// </summary>
    [ForeignKey(nameof(Rmoapo))]
    public virtual BranchMaster? Branch { get; set; }

    /// <summary>
    /// Check if policy is active
    /// </summary>
    [NotMapped]
    public bool IsActive
    {
        get
        {
            if (!Dtinivig.HasValue || !Dtfimvig.HasValue)
                return false;

            var today = DateTime.Today;
            return today >= Dtinivig.Value && today <= Dtfimvig.Value;
        }
    }
}
```

**EF Core Fluent API Configuration:**

```csharp
public class PolicyMasterConfiguration : IEntityTypeConfiguration<PolicyMaster>
{
    public void Configure(EntityTypeBuilder<PolicyMaster> builder)
    {
        // Composite Primary Key
        builder.HasKey(e => new { e.Orgapo, e.Rmoapo, e.Numapol });

        // Relationships
        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.Rmoapo)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

        // Check Constraint for valid date range
        builder.ToTable(t => t.HasCheckConstraint("CK_TAPOLICE_DateRange", "[DTFIMVIG] IS NULL OR [DTFIMVIG] >= [DTINIVIG]"));
    }
}
```

---

### 7. Claim Accompaniment (SI_ACOMPANHA_SINI)

**Purpose**: Tracks claim workflow events and activities.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Claim Accompaniment - Workflow event tracking (Legacy table: SI_ACOMPANHA_SINI)
/// Records claim events, activities, and workflow progression.
/// </summary>
[Table("SI_ACOMPANHA_SINI")]
[Index(nameof(CodEvento), nameof(DataMovtoSiniaco), Name = "IX_SI_ACOMPANHA_EventDate")]
[Index(nameof(CodUsuario), nameof(DataMovtoSiniaco), Name = "IX_SI_ACOMPANHA_UserDate")]
public class ClaimAccompaniment
{
    /// <summary>
    /// Fonte do Protocolo - Protocol Source (Primary Key)
    /// </summary>
    [Key]
    [Column("FONTE", Order = 0)]
    [Required]
    public int Fonte { get; set; }

    /// <summary>
    /// Número do Protocolo - Protocol Number (Primary Key)
    /// </summary>
    [Key]
    [Column("PROTSINI", Order = 1)]
    [Required]
    public int Protsini { get; set; }

    /// <summary>
    /// Dígito de Controle - Check Digit (Primary Key)
    /// </summary>
    [Key]
    [Column("DAC", Order = 2)]
    [Required]
    public int Dac { get; set; }

    /// <summary>
    /// Código do Evento - Event Code (Primary Key)
    /// </summary>
    [Key]
    [Column("COD_EVENTO", Order = 3)]
    [Required]
    public int CodEvento { get; set; }

    /// <summary>
    /// Data do Movimento - Transaction Date (Primary Key)
    /// </summary>
    [Key]
    [Column("DATA_MOVTO_SINIACO", TypeName = "date", Order = 4)]
    [Required]
    public DateTime DataMovtoSiniaco { get; set; }

    /// <summary>
    /// Número da Ocorrência - Occurrence Number
    /// </summary>
    [Column("NUM_OCORR_SINIACO")]
    [Required]
    public int NumOcorrSiniaco { get; set; }

    /// <summary>
    /// Descrição Complementar - Complementary Description
    /// </summary>
    [Column("DESCR_COMPLEMENTAR")]
    [StringLength(500)]
    public string? DescrComplementar { get; set; }

    /// <summary>
    /// Código do Usuário - User ID
    /// </summary>
    [Column("COD_USUARIO")]
    [Required]
    [StringLength(50)]
    public string CodUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Hora do Evento - Event Time
    /// Format: HHmmss
    /// </summary>
    [Column("HORA_EVENTO")]
    [StringLength(6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Time must be in HHmmss format")]
    public string? HoraEvento { get; set; }

    /// <summary>
    /// Nome do Evento - Event Name
    /// </summary>
    [Column("NOME_EVENTO")]
    [StringLength(100)]
    public string? NomeEvento { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Parent claim record
    /// </summary>
    [ForeignKey($"{nameof(Fonte)},{nameof(Protsini)},{nameof(Dac)}")]
    public virtual ClaimMaster? Claim { get; set; }

    /// <summary>
    /// Combined event timestamp
    /// </summary>
    [NotMapped]
    public DateTime EventTimestamp
    {
        get
        {
            if (string.IsNullOrEmpty(HoraEvento) || HoraEvento.Length != 6)
                return DataMovtoSiniaco;

            int hour = int.Parse(HoraEvento.Substring(0, 2));
            int minute = int.Parse(HoraEvento.Substring(2, 2));
            int second = int.Parse(HoraEvento.Substring(4, 2));

            return DataMovtoSiniaco.Date.AddHours(hour).AddMinutes(minute).AddSeconds(second);
        }
    }
}
```

**EF Core Fluent API Configuration:**

```csharp
public class ClaimAccompanimentConfiguration : IEntityTypeConfiguration<ClaimAccompaniment>
{
    public void Configure(EntityTypeBuilder<ClaimAccompaniment> builder)
    {
        // Composite Primary Key
        builder.HasKey(e => new { e.Fonte, e.Protsini, e.Dac, e.CodEvento, e.DataMovtoSiniaco });

        // Relationships
        builder.HasOne(e => e.Claim)
            .WithMany(c => c.ClaimAccompaniments)
            .HasForeignKey(e => new { e.Fonte, e.Protsini, e.Dac })
            .HasPrincipalKey(c => new { c.Fonte, c.Protsini, c.Dac })
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
    }
}
```

---

### 8. Claim Phase (SI_SINISTRO_FASE)

**Purpose**: Tracks claim processing phases with opening and closing dates.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Claim Phase - Phase tracking with dates (Legacy table: SI_SINISTRO_FASE)
/// Tracks claim processing phases including opening, closing, and effective dates.
/// </summary>
[Table("SI_SINISTRO_FASE")]
[Index(nameof(CodFase), nameof(DataAberturaSifa), Name = "IX_SI_SINISTRO_FASE_PhaseOpening")]
[Index(nameof(DataFechaSifa), Name = "IX_SI_SINISTRO_FASE_Closing")]
public class ClaimPhase
{
    /// <summary>
    /// Fonte do Protocolo - Protocol Source (Primary Key)
    /// </summary>
    [Key]
    [Column("FONTE", Order = 0)]
    [Required]
    public int Fonte { get; set; }

    /// <summary>
    /// Número do Protocolo - Protocol Number (Primary Key)
    /// </summary>
    [Key]
    [Column("PROTSINI", Order = 1)]
    [Required]
    public int Protsini { get; set; }

    /// <summary>
    /// Dígito de Controle - Check Digit (Primary Key)
    /// </summary>
    [Key]
    [Column("DAC", Order = 2)]
    [Required]
    public int Dac { get; set; }

    /// <summary>
    /// Código da Fase - Phase Code (Primary Key)
    /// </summary>
    [Key]
    [Column("COD_FASE", Order = 3)]
    [Required]
    public int CodFase { get; set; }

    /// <summary>
    /// Código do Evento - Event Code (Primary Key)
    /// </summary>
    [Key]
    [Column("COD_EVENTO", Order = 4)]
    [Required]
    public int CodEvento { get; set; }

    /// <summary>
    /// Número da Ocorrência - Event Occurrence (Primary Key)
    /// </summary>
    [Key]
    [Column("NUM_OCORR_SINIACO", Order = 5)]
    [Required]
    public int NumOcorrSiniaco { get; set; }

    /// <summary>
    /// Data de Início de Vigência - Effective Date (Primary Key)
    /// </summary>
    [Key]
    [Column("DATA_INIVIG_REFAEV", TypeName = "date", Order = 6)]
    [Required]
    public DateTime DataInivigRefaev { get; set; }

    /// <summary>
    /// Data de Abertura da Fase - Phase Opening Date
    /// </summary>
    [Column("DATA_ABERTURA_SIFA", TypeName = "date")]
    [Required]
    public DateTime DataAberturaSifa { get; set; }

    /// <summary>
    /// Data de Fechamento da Fase - Phase Closing Date
    /// Default: '9999-12-31' for open phases
    /// </summary>
    [Column("DATA_FECHA_SIFA", TypeName = "date")]
    [Required]
    public DateTime DataFechaSifa { get; set; }

    /// <summary>
    /// Nome da Fase - Phase Name
    /// </summary>
    [Column("NOME_FASE")]
    [StringLength(100)]
    public string? NomeFase { get; set; }

    /// <summary>
    /// Observações - Observations
    /// </summary>
    [Column("OBSERVACOES")]
    [StringLength(1000)]
    public string? Observacoes { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Parent claim record
    /// </summary>
    [ForeignKey($"{nameof(Fonte)},{nameof(Protsini)},{nameof(Dac)}")]
    public virtual ClaimMaster? Claim { get; set; }

    /// <summary>
    /// Phase-event relationship configuration
    /// </summary>
    [ForeignKey($"{nameof(CodFase)},{nameof(CodEvento)},{nameof(DataInivigRefaev)}")]
    public virtual PhaseEventRelationship? PhaseEventRelationship { get; set; }

    /// <summary>
    /// Check if phase is currently open
    /// </summary>
    [NotMapped]
    public bool IsOpen => DataFechaSifa.Year == 9999;

    /// <summary>
    /// Phase duration in days (null if still open)
    /// </summary>
    [NotMapped]
    public int? DurationInDays
    {
        get
        {
            if (IsOpen)
                return null;

            return (DataFechaSifa - DataAberturaSifa).Days;
        }
    }
}
```

**EF Core Fluent API Configuration:**

```csharp
public class ClaimPhaseConfiguration : IEntityTypeConfiguration<ClaimPhase>
{
    public void Configure(EntityTypeBuilder<ClaimPhase> builder)
    {
        // Composite Primary Key
        builder.HasKey(e => new { e.Fonte, e.Protsini, e.Dac, e.CodFase, e.CodEvento, e.NumOcorrSiniaco, e.DataInivigRefaev });

        // Relationships
        builder.HasOne(e => e.Claim)
            .WithMany(c => c.ClaimPhases)
            .HasForeignKey(e => new { e.Fonte, e.Protsini, e.Dac })
            .HasPrincipalKey(c => new { c.Fonte, c.Protsini, c.Dac })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PhaseEventRelationship)
            .WithMany()
            .HasForeignKey(e => new { e.CodFase, e.CodEvento, e.DataInivigRefaev })
            .OnDelete(DeleteBehavior.Restrict);

        // Default Values
        builder.Property(e => e.DataFechaSifa).HasDefaultValue(new DateTime(9999, 12, 31));
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

        // Check Constraint for valid date range
        builder.ToTable(t => t.HasCheckConstraint("CK_SI_SINISTRO_FASE_DateRange",
            "[DATA_FECHA_SIFA] >= [DATA_ABERTURA_SIFA] OR YEAR([DATA_FECHA_SIFA]) = 9999"));
    }
}
```

---

### 9. Phase-Event Relationship (SI_REL_FASE_EVENTO)

**Purpose**: Configuration table defining which phases are affected by each event type.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Phase-Event Relationship - Configuration for phase changes (Legacy table: SI_REL_FASE_EVENTO)
/// Defines which phases are affected by each event type and whether they open or close.
/// </summary>
[Table("SI_REL_FASE_EVENTO")]
[Index(nameof(CodEvento), nameof(DataInivigRefaev), Name = "IX_SI_REL_FASE_EVENTO_EventDate")]
public class PhaseEventRelationship
{
    /// <summary>
    /// Código da Fase - Phase Code (Primary Key)
    /// </summary>
    [Key]
    [Column("COD_FASE", Order = 0)]
    [Required]
    public int CodFase { get; set; }

    /// <summary>
    /// Código do Evento - Event Code (Primary Key)
    /// </summary>
    [Key]
    [Column("COD_EVENTO", Order = 1)]
    [Required]
    public int CodEvento { get; set; }

    /// <summary>
    /// Data de Início de Vigência - Effective Start Date (Primary Key)
    /// </summary>
    [Key]
    [Column("DATA_INIVIG_REFAEV", TypeName = "date", Order = 2)]
    [Required]
    public DateTime DataInivigRefaev { get; set; }

    /// <summary>
    /// Data de Término de Vigência - Effective End Date
    /// </summary>
    [Column("DATA_TERVIG_REFAEV", TypeName = "date")]
    public DateTime? DataTervigRefaev { get; set; }

    /// <summary>
    /// Indicador de Alteração de Fase - Phase Change Indicator
    /// '1' = Opens phase (abertura)
    /// '2' = Closes phase (fechamento)
    /// </summary>
    [Column("IND_ALTERACAO_FASE")]
    [Required]
    [StringLength(1)]
    [RegularExpression("^[12]$", ErrorMessage = "Phase Indicator must be '1' (open) or '2' (close)")]
    public string IndAlteracaoFase { get; set; } = string.Empty;

    /// <summary>
    /// Nome da Fase - Phase Name
    /// </summary>
    [Column("NOME_FASE")]
    [StringLength(100)]
    public string? NomeFase { get; set; }

    /// <summary>
    /// Nome do Evento - Event Name
    /// </summary>
    [Column("NOME_EVENTO")]
    [StringLength(100)]
    public string? NomeEvento { get; set; }

    /// <summary>
    /// Ativo - Active Status
    /// </summary>
    [Column("ATIVO")]
    [Required]
    public bool Ativo { get; set; } = true;

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Check if this relationship opens a phase
    /// </summary>
    [NotMapped]
    public bool OpensPhase => IndAlteracaoFase == "1";

    /// <summary>
    /// Check if this relationship closes a phase
    /// </summary>
    [NotMapped]
    public bool ClosesPhase => IndAlteracaoFase == "2";

    /// <summary>
    /// Check if configuration is valid for date
    /// </summary>
    public bool IsValidForDate(DateTime date)
    {
        if (date < DataInivigRefaev)
            return false;

        if (DataTervigRefaev.HasValue && date > DataTervigRefaev.Value)
            return false;

        return Ativo;
    }
}
```

**EF Core Fluent API Configuration:**

```csharp
public class PhaseEventRelationshipConfiguration : IEntityTypeConfiguration<PhaseEventRelationship>
{
    public void Configure(EntityTypeBuilder<PhaseEventRelationship> builder)
    {
        // Composite Primary Key
        builder.HasKey(e => new { e.CodFase, e.CodEvento, e.DataInivigRefaev });

        builder.Property(e => e.Ativo).HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

        // Check Constraint for valid date range
        builder.ToTable(t => t.HasCheckConstraint("CK_SI_REL_FASE_EVENTO_DateRange",
            "[DATA_TERVIG_REFAEV] IS NULL OR [DATA_TERVIG_REFAEV] >= [DATA_INIVIG_REFAEV]"));

        // Check Constraint for phase indicator
        builder.ToTable(t => t.HasCheckConstraint("CK_SI_REL_FASE_EVENTO_Indicator",
            "[IND_ALTERACAO_FASE] IN ('1', '2')"));
    }
}
```

---

### 10. Consortium Contract (EF_CONTR_SEG_HABIT)

**Purpose**: Consortium-specific contract information for validation routing.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Consortium Contract - Consortium housing insurance contracts (Legacy table: EF_CONTR_SEG_HABIT)
/// Contains consortium-specific contract information used to determine validation routing.
/// </summary>
[Table("EF_CONTR_SEG_HABIT")]
[Index(nameof(NumProposta), Name = "IX_EF_CONTR_SEG_HABIT_Proposal")]
[Index(nameof(CpfCnpjSegurado), Name = "IX_EF_CONTR_SEG_HABIT_TaxId")]
public class ConsortiumContract
{
    /// <summary>
    /// Número do Contrato - Contract Number (Primary Key)
    /// </summary>
    [Key]
    [Column("NUM_CONTRATO")]
    [Required]
    public int NumContrato { get; set; }

    /// <summary>
    /// Número da Proposta - Proposal Number
    /// </summary>
    [Column("NUM_PROPOSTA")]
    [Required]
    public int NumProposta { get; set; }

    /// <summary>
    /// CPF/CNPJ do Segurado - Insured Tax ID
    /// </summary>
    [Column("CPF_CNPJ_SEGURADO")]
    [Required]
    [StringLength(20)]
    public string CpfCnpjSegurado { get; set; } = string.Empty;

    /// <summary>
    /// Nome do Segurado - Insured Name
    /// </summary>
    [Column("NOME_SEGURADO")]
    [Required]
    [StringLength(200)]
    public string NomeSegurado { get; set; } = string.Empty;

    /// <summary>
    /// Número da Apólice - Policy Number
    /// </summary>
    [Column("NUM_APOLICE")]
    public int? NumApolice { get; set; }

    /// <summary>
    /// Data de Início de Vigência - Contract Start Date
    /// </summary>
    [Column("DATA_INICIO_VIGENCIA", TypeName = "date")]
    [Required]
    public DateTime DataInicioVigencia { get; set; }

    /// <summary>
    /// Data de Fim de Vigência - Contract End Date
    /// </summary>
    [Column("DATA_FIM_VIGENCIA", TypeName = "date")]
    [Required]
    public DateTime DataFimVigencia { get; set; }

    /// <summary>
    /// Valor Segurado - Insured Amount
    /// </summary>
    [Column("VALOR_SEGURADO", TypeName = "decimal(15,2)")]
    [Range(0, 999999999999.99)]
    public decimal? ValorSegurado { get; set; }

    /// <summary>
    /// Código do Produto - Product Code
    /// </summary>
    [Column("COD_PRODUTO")]
    [Required]
    public int CodProduto { get; set; }

    /// <summary>
    /// Situação do Contrato - Contract Status
    /// A=Active, C=Cancelled, S=Suspended
    /// </summary>
    [Column("SITUACAO_CONTRATO")]
    [Required]
    [StringLength(1)]
    [RegularExpression("^[ACS]$", ErrorMessage = "Status must be 'A' (Active), 'C' (Cancelled), or 'S' (Suspended)")]
    public string SituacaoContrato { get; set; } = "A";

    /// <summary>
    /// Tipo de Contrato - Contract Type
    /// EFP or HB
    /// </summary>
    [Column("TIPO_CONTRATO")]
    [StringLength(10)]
    public string? TipoContrato { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Check if contract is EFP type (routes to SIPUA validation)
    /// </summary>
    [NotMapped]
    public bool IsEfpContract => NumContrato > 0 && TipoContrato == "EFP";

    /// <summary>
    /// Check if contract is HB type (routes to SIMDA validation)
    /// </summary>
    [NotMapped]
    public bool IsHbContract => NumContrato == 0 || TipoContrato == "HB";

    /// <summary>
    /// Check if contract is active
    /// </summary>
    [NotMapped]
    public bool IsActive
    {
        get
        {
            if (SituacaoContrato != "A")
                return false;

            var today = DateTime.Today;
            return today >= DataInicioVigencia && today <= DataFimVigencia;
        }
    }
}
```

**EF Core Fluent API Configuration:**

```csharp
public class ConsortiumContractConfiguration : IEntityTypeConfiguration<ConsortiumContract>
{
    public void Configure(EntityTypeBuilder<ConsortiumContract> builder)
    {
        builder.HasKey(e => e.NumContrato);

        builder.Property(e => e.SituacaoContrato).HasDefaultValue("A");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

        // Check Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_EF_CONTR_SEG_HABIT_DateRange",
            "[DATA_FIM_VIGENCIA] >= [DATA_INICIO_VIGENCIA]"));

        builder.ToTable(t => t.HasCheckConstraint("CK_EF_CONTR_SEG_HABIT_Status",
            "[SITUACAO_CONTRATO] IN ('A', 'C', 'S')"));
    }
}
```

---

## Dashboard Entities

### 11. Migration Status

**Purpose**: Tracks overall migration project progress and user story completion.

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Migration Status - Overall project tracking (New table: MIGRATION_STATUS)
/// Tracks migration progress for each user story including requirements, tests, and timeline.
/// </summary>
[Table("MIGRATION_STATUS")]
[Index(nameof(UserStoryCode), Name = "IX_MIGRATION_STATUS_Code", IsUnique = true)]
[Index(nameof(Status), nameof(EstimatedCompletion), Name = "IX_MIGRATION_STATUS_StatusDate")]
public class MigrationStatus
{
    /// <summary>
    /// Unique identifier (Primary Key)
    /// </summary>
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    /// <summary>
    /// User Story Code - Unique identifier for user story (e.g., "US-001")
    /// </summary>
    [Column("USER_STORY_CODE")]
    [Required]
    [StringLength(20)]
    public string UserStoryCode { get; set; } = string.Empty;

    /// <summary>
    /// User Story Name - Descriptive name
    /// </summary>
    [Column("USER_STORY_NAME")]
    [Required]
    [StringLength(200)]
    public string UserStoryName { get; set; } = string.Empty;

    /// <summary>
    /// Current Status - Not Started, In Progress, Completed, Blocked
    /// </summary>
    [Column("STATUS")]
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Not Started";

    /// <summary>
    /// Completion Percentage (0-100)
    /// </summary>
    [Column("COMPLETION_PERCENTAGE", TypeName = "decimal(5,2)")]
    [Required]
    [Range(0, 100)]
    public decimal CompletionPercentage { get; set; }

    /// <summary>
    /// Number of Functional Requirements Completed
    /// </summary>
    [Column("REQUIREMENTS_COMPLETED")]
    [Required]
    [Range(0, int.MaxValue)]
    public int RequirementsCompleted { get; set; }

    /// <summary>
    /// Total Number of Functional Requirements
    /// </summary>
    [Column("REQUIREMENTS_TOTAL")]
    [Required]
    [Range(1, int.MaxValue)]
    public int RequirementsTotal { get; set; }

    /// <summary>
    /// Number of Tests Passed
    /// </summary>
    [Column("TESTS_PASSED")]
    [Required]
    [Range(0, int.MaxValue)]
    public int TestsPassed { get; set; }

    /// <summary>
    /// Total Number of Tests
    /// </summary>
    [Column("TESTS_TOTAL")]
    [Required]
    [Range(0, int.MaxValue)]
    public int TestsTotal { get; set; }

    /// <summary>
    /// Assigned Team Member
    /// </summary>
    [Column("ASSIGNED_TO")]
    [StringLength(100)]
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Start Date
    /// </summary>
    [Column("START_DATE")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Estimated Completion Date
    /// </summary>
    [Column("ESTIMATED_COMPLETION")]
    public DateTime? EstimatedCompletion { get; set; }

    /// <summary>
    /// Actual Completion Date
    /// </summary>
    [Column("ACTUAL_COMPLETION")]
    public DateTime? ActualCompletion { get; set; }

    /// <summary>
    /// Blocking Issues (if status is Blocked)
    /// </summary>
    [Column("BLOCKING_ISSUES")]
    [StringLength(1000)]
    public string? BlockingIssues { get; set; }

    /// <summary>
    /// Priority Level (P1-P6)
    /// </summary>
    [Column("PRIORITY")]
    [StringLength(10)]
    public string? Priority { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Related component migrations
    /// </summary>
    public virtual ICollection<ComponentMigration> Components { get; set; } = new List<ComponentMigration>();

    // Computed Properties
    /// <summary>
    /// Test pass rate percentage
    /// </summary>
    [NotMapped]
    public decimal TestPassRate
    {
        get
        {
            if (TestsTotal == 0)
                return 0;

            return Math.Round((decimal)TestsPassed / TestsTotal * 100, 2);
        }
    }

    /// <summary>
    /// Requirements completion rate percentage
    /// </summary>
    [NotMapped]
    public decimal RequirementsCompletionRate
    {
        get
        {
            if (RequirementsTotal == 0)
                return 0;

            return Math.Round((decimal)RequirementsCompleted / RequirementsTotal * 100, 2);
        }
    }

    /// <summary>
    /// Days remaining until estimated completion (negative if overdue)
    /// </summary>
    [NotMapped]
    public int? DaysRemaining
    {
        get
        {
            if (!EstimatedCompletion.HasValue || ActualCompletion.HasValue)
                return null;

            return (EstimatedCompletion.Value.Date - DateTime.Today).Days;
        }
    }

    /// <summary>
    /// Is user story overdue
    /// </summary>
    [NotMapped]
    public bool IsOverdue
    {
        get
        {
            if (!EstimatedCompletion.HasValue || ActualCompletion.HasValue)
                return false;

            return DateTime.Today > EstimatedCompletion.Value.Date && Status != "Completed";
        }
    }

    /// <summary>
    /// Is user story blocked
    /// </summary>
    [NotMapped]
    public bool IsBlocked => Status == "Blocked";
}
```

**EF Core Fluent API Configuration:**

```csharp
public class MigrationStatusConfiguration : IEntityTypeConfiguration<MigrationStatus>
{
    public void Configure(EntityTypeBuilder<MigrationStatus> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.UserStoryCode).IsUnique();

        builder.HasMany(e => e.Components)
            .WithOne(c => c.MigrationStatus)
            .HasForeignKey(c => c.MigrationStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        // Default Values
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");
        builder.Property(e => e.Status).HasDefaultValue("Not Started");
        builder.Property(e => e.CompletionPercentage).HasDefaultValue(0m);
        builder.Property(e => e.RequirementsCompleted).HasDefaultValue(0);
        builder.Property(e => e.TestsPassed).HasDefaultValue(0);
        builder.Property(e => e.TestsTotal).HasDefaultValue(0);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

        // Check Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_MIGRATION_STATUS_Percentage",
            "[COMPLETION_PERCENTAGE] >= 0 AND [COMPLETION_PERCENTAGE] <= 100"));

        builder.ToTable(t => t.HasCheckConstraint("CK_MIGRATION_STATUS_Requirements",
            "[REQUIREMENTS_COMPLETED] <= [REQUIREMENTS_TOTAL]"));

        builder.ToTable(t => t.HasCheckConstraint("CK_MIGRATION_STATUS_Tests",
            "[TESTS_PASSED] <= [TESTS_TOTAL]"));

        builder.ToTable(t => t.HasCheckConstraint("CK_MIGRATION_STATUS_Status",
            "[STATUS] IN ('Not Started', 'In Progress', 'Completed', 'Blocked')"));
    }
}
```

---

### 12. Component Migration Tracking

**Purpose**: Tracks individual component migration status and effort.

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Component Migration - Individual component tracking (New table: COMPONENT_MIGRATION)
/// Tracks migration status for screens, business rules, database entities, and external services.
/// </summary>
[Table("COMPONENT_MIGRATION")]
[Index(nameof(MigrationStatusId), nameof(ComponentType), Name = "IX_COMPONENT_MIGRATION_StatusType")]
[Index(nameof(Status), nameof(AssignedDeveloper), Name = "IX_COMPONENT_MIGRATION_StatusDev")]
[Index(nameof(ComponentType), nameof(Complexity), Name = "IX_COMPONENT_MIGRATION_TypeComplexity")]
public class ComponentMigration
{
    /// <summary>
    /// Unique identifier (Primary Key)
    /// </summary>
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    /// <summary>
    /// Parent Migration Status ID (Foreign Key)
    /// </summary>
    [Column("MIGRATION_STATUS_ID")]
    [Required]
    public Guid MigrationStatusId { get; set; }

    /// <summary>
    /// Component Type - Screen, Business Rule, Database Entity, External Service
    /// </summary>
    [Column("COMPONENT_TYPE")]
    [Required]
    [StringLength(50)]
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>
    /// Component Name - Descriptive name
    /// </summary>
    [Column("COMPONENT_NAME")]
    [Required]
    [StringLength(200)]
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>
    /// Legacy System Reference - Visual Age file/function name
    /// </summary>
    [Column("LEGACY_REFERENCE")]
    [StringLength(200)]
    public string? LegacyReference { get; set; }

    /// <summary>
    /// Migration Status - Not Started, In Progress, Completed, Blocked, Skipped
    /// </summary>
    [Column("STATUS")]
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Not Started";

    /// <summary>
    /// Estimated Effort in Hours
    /// </summary>
    [Column("ESTIMATED_HOURS", TypeName = "decimal(8,2)")]
    [Range(0, 9999.99)]
    public decimal? EstimatedHours { get; set; }

    /// <summary>
    /// Actual Effort in Hours
    /// </summary>
    [Column("ACTUAL_HOURS", TypeName = "decimal(8,2)")]
    [Range(0, 9999.99)]
    public decimal? ActualHours { get; set; }

    /// <summary>
    /// Complexity Rating - Low, Medium, High
    /// </summary>
    [Column("COMPLEXITY")]
    [StringLength(20)]
    public string? Complexity { get; set; }

    /// <summary>
    /// Assigned Developer
    /// </summary>
    [Column("ASSIGNED_DEVELOPER")]
    [StringLength(100)]
    public string? AssignedDeveloper { get; set; }

    /// <summary>
    /// Technical Notes
    /// </summary>
    [Column("TECHNICAL_NOTES")]
    [StringLength(2000)]
    public string? TechnicalNotes { get; set; }

    /// <summary>
    /// Start Date
    /// </summary>
    [Column("START_DATE")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Completion Date
    /// </summary>
    [Column("COMPLETION_DATE")]
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Dependencies (comma-separated component IDs)
    /// </summary>
    [Column("DEPENDENCIES")]
    [StringLength(500)]
    public string? Dependencies { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Parent migration status
    /// </summary>
    [ForeignKey(nameof(MigrationStatusId))]
    public virtual MigrationStatus? MigrationStatus { get; set; }

    /// <summary>
    /// Performance metrics for this component
    /// </summary>
    public virtual ICollection<PerformanceMetric> PerformanceMetrics { get; set; } = new List<PerformanceMetric>();

    // Computed Properties
    /// <summary>
    /// Effort variance (Actual - Estimated)
    /// </summary>
    [NotMapped]
    public decimal? EffortVariance
    {
        get
        {
            if (!EstimatedHours.HasValue || !ActualHours.HasValue)
                return null;

            return ActualHours.Value - EstimatedHours.Value;
        }
    }

    /// <summary>
    /// Effort variance percentage
    /// </summary>
    [NotMapped]
    public decimal? EffortVariancePercentage
    {
        get
        {
            if (!EstimatedHours.HasValue || !ActualHours.HasValue || EstimatedHours.Value == 0)
                return null;

            return Math.Round((ActualHours.Value - EstimatedHours.Value) / EstimatedHours.Value * 100, 2);
        }
    }

    /// <summary>
    /// Duration in days (if completed)
    /// </summary>
    [NotMapped]
    public int? DurationInDays
    {
        get
        {
            if (!StartDate.HasValue || !CompletionDate.HasValue)
                return null;

            return (CompletionDate.Value.Date - StartDate.Value.Date).Days;
        }
    }

    /// <summary>
    /// Is component completed
    /// </summary>
    [NotMapped]
    public bool IsCompleted => Status == "Completed";

    /// <summary>
    /// Is component blocked
    /// </summary>
    [NotMapped]
    public bool IsBlocked => Status == "Blocked";
}
```

**EF Core Fluent API Configuration:**

```csharp
public class ComponentMigrationConfiguration : IEntityTypeConfiguration<ComponentMigration>
{
    public void Configure(EntityTypeBuilder<ComponentMigration> builder)
    {
        builder.HasKey(e => e.Id);

        // Relationships
        builder.HasOne(e => e.MigrationStatus)
            .WithMany(m => m.Components)
            .HasForeignKey(e => e.MigrationStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.PerformanceMetrics)
            .WithOne(p => p.Component)
            .HasForeignKey(p => p.ComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Default Values
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");
        builder.Property(e => e.Status).HasDefaultValue("Not Started");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

        // Check Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_COMPONENT_MIGRATION_Status",
            "[STATUS] IN ('Not Started', 'In Progress', 'Completed', 'Blocked', 'Skipped')"));

        builder.ToTable(t => t.HasCheckConstraint("CK_COMPONENT_MIGRATION_Complexity",
            "[COMPLEXITY] IS NULL OR [COMPLEXITY] IN ('Low', 'Medium', 'High')"));

        builder.ToTable(t => t.HasCheckConstraint("CK_COMPONENT_MIGRATION_ComponentType",
            "[COMPONENT_TYPE] IN ('Screen', 'Business Rule', 'Database Entity', 'External Service', 'Validation', 'Integration')"));
    }
}
```

---

### 13. Performance Metrics

**Purpose**: Stores performance benchmarks comparing legacy and new systems.

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CaixaSeguradora.Core.Entities;

/// <summary>
/// Performance Metrics - Performance comparison (New table: PERFORMANCE_METRICS)
/// Stores performance benchmarks for comparison between legacy Visual Age and new .NET 9 system.
/// </summary>
[Table("PERFORMANCE_METRICS")]
[Index(nameof(ComponentId), nameof(MetricType), Name = "IX_PERFORMANCE_METRICS_ComponentType")]
[Index(nameof(MeasurementTimestamp), Name = "IX_PERFORMANCE_METRICS_Timestamp")]
[Index(nameof(PassFail), Name = "IX_PERFORMANCE_METRICS_PassFail")]
public class PerformanceMetric
{
    /// <summary>
    /// Unique identifier (Primary Key)
    /// </summary>
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    /// <summary>
    /// Component ID (Foreign Key)
    /// </summary>
    [Column("COMPONENT_ID")]
    [Required]
    public Guid ComponentId { get; set; }

    /// <summary>
    /// Metric Type - Response Time, Throughput, Concurrent Users, Memory Usage, Error Rate
    /// </summary>
    [Column("METRIC_TYPE")]
    [Required]
    [StringLength(50)]
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Legacy System Value
    /// </summary>
    [Column("LEGACY_VALUE", TypeName = "decimal(18,4)")]
    [Required]
    public decimal LegacyValue { get; set; }

    /// <summary>
    /// New System Value
    /// </summary>
    [Column("NEW_VALUE", TypeName = "decimal(18,4)")]
    [Required]
    public decimal NewValue { get; set; }

    /// <summary>
    /// Unit of Measurement (ms, requests/sec, MB, %, etc.)
    /// </summary>
    [Column("UNIT")]
    [StringLength(20)]
    public string? Unit { get; set; }

    /// <summary>
    /// Measurement Timestamp
    /// </summary>
    [Column("MEASUREMENT_TIMESTAMP")]
    [Required]
    public DateTime MeasurementTimestamp { get; set; }

    /// <summary>
    /// Test Scenario Description
    /// </summary>
    [Column("TEST_SCENARIO")]
    [StringLength(500)]
    public string? TestScenario { get; set; }

    /// <summary>
    /// Pass/Fail Status (based on acceptance criteria)
    /// </summary>
    [Column("PASS_FAIL")]
    [Required]
    public bool PassFail { get; set; }

    /// <summary>
    /// Target Value (acceptance criteria threshold)
    /// </summary>
    [Column("TARGET_VALUE", TypeName = "decimal(18,4)")]
    public decimal? TargetValue { get; set; }

    /// <summary>
    /// Environment (Development, Staging, Production)
    /// </summary>
    [Column("ENVIRONMENT")]
    [StringLength(50)]
    public string? Environment { get; set; }

    /// <summary>
    /// Additional Notes
    /// </summary>
    [Column("NOTES")]
    [StringLength(1000)]
    public string? Notes { get; set; }

    // Audit Fields
    /// <summary>
    /// Created by user ID
    /// </summary>
    [Column("CREATED_BY")]
    [StringLength(50)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [Column("CREATED_AT")]
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated by user ID
    /// </summary>
    [Column("UPDATED_BY")]
    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Column("UPDATED_AT")]
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking
    /// </summary>
    [Timestamp]
    [Column("ROW_VERSION")]
    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    /// <summary>
    /// Related component
    /// </summary>
    [ForeignKey(nameof(ComponentId))]
    public virtual ComponentMigration? Component { get; set; }

    // Computed Properties
    /// <summary>
    /// Improvement percentage (positive = better, negative = worse)
    /// Formula varies by metric type
    /// </summary>
    [NotMapped]
    public decimal ImprovementPercentage
    {
        get
        {
            if (LegacyValue == 0)
                return 0;

            // For Response Time and Memory Usage: lower is better
            if (MetricType.Contains("Response Time", StringComparison.OrdinalIgnoreCase) ||
                MetricType.Contains("Memory", StringComparison.OrdinalIgnoreCase) ||
                MetricType.Contains("Error Rate", StringComparison.OrdinalIgnoreCase))
            {
                return Math.Round((LegacyValue - NewValue) / LegacyValue * 100, 2);
            }

            // For Throughput and Concurrent Users: higher is better
            return Math.Round((NewValue - LegacyValue) / LegacyValue * 100, 2);
        }
    }

    /// <summary>
    /// Absolute difference between new and legacy values
    /// </summary>
    [NotMapped]
    public decimal Difference => NewValue - LegacyValue;

    /// <summary>
    /// Is new system faster/better than legacy
    /// </summary>
    [NotMapped]
    public bool IsImproved => ImprovementPercentage > 0;

    /// <summary>
    /// Performance rating (Excellent, Good, Fair, Poor)
    /// </summary>
    [NotMapped]
    public string PerformanceRating
    {
        get
        {
            var improvement = ImprovementPercentage;

            if (improvement >= 50)
                return "Excellent";
            if (improvement >= 25)
                return "Good";
            if (improvement >= 0)
                return "Fair";

            return "Poor";
        }
    }
}
```

**EF Core Fluent API Configuration:**

```csharp
public class PerformanceMetricConfiguration : IEntityTypeConfiguration<PerformanceMetric>
{
    public void Configure(EntityTypeBuilder<PerformanceMetric> builder)
    {
        builder.HasKey(e => e.Id);

        // Relationships
        builder.HasOne(e => e.Component)
            .WithMany(c => c.PerformanceMetrics)
            .HasForeignKey(e => e.ComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Default Values
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");
        builder.Property(e => e.MeasurementTimestamp).HasDefaultValueSql("GETDATE()");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

        // Check Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_PERFORMANCE_METRICS_MetricType",
            "[METRIC_TYPE] IN ('Response Time', 'Throughput', 'Concurrent Users', 'Memory Usage', 'Error Rate', 'CPU Usage', 'Database Query Time')"));

        builder.ToTable(t => t.HasCheckConstraint("CK_PERFORMANCE_METRICS_Environment",
            "[ENVIRONMENT] IS NULL OR [ENVIRONMENT] IN ('Development', 'Staging', 'Production')"));
    }
}
```

---

## DbContext Configuration

```csharp
using Microsoft.EntityFrameworkCore;
using CaixaSeguradora.Core.Entities;

namespace CaixaSeguradora.Infrastructure.Data;

/// <summary>
/// Claims System Database Context
/// </summary>
public class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : base(options)
    {
    }

    // Legacy Entities
    public DbSet<ClaimMaster> ClaimMasters { get; set; }
    public DbSet<ClaimHistory> ClaimHistories { get; set; }
    public DbSet<BranchMaster> BranchMasters { get; set; }
    public DbSet<CurrencyUnit> CurrencyUnits { get; set; }
    public DbSet<SystemControl> SystemControls { get; set; }
    public DbSet<PolicyMaster> PolicyMasters { get; set; }
    public DbSet<ClaimAccompaniment> ClaimAccompaniments { get; set; }
    public DbSet<ClaimPhase> ClaimPhases { get; set; }
    public DbSet<PhaseEventRelationship> PhaseEventRelationships { get; set; }
    public DbSet<ConsortiumContract> ConsortiumContracts { get; set; }

    // Dashboard Entities
    public DbSet<MigrationStatus> MigrationStatuses { get; set; }
    public DbSet<ComponentMigration> ComponentMigrations { get; set; }
    public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new ClaimMasterConfiguration());
        modelBuilder.ApplyConfiguration(new ClaimHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new BranchMasterConfiguration());
        modelBuilder.ApplyConfiguration(new CurrencyUnitConfiguration());
        modelBuilder.ApplyConfiguration(new SystemControlConfiguration());
        modelBuilder.ApplyConfiguration(new PolicyMasterConfiguration());
        modelBuilder.ApplyConfiguration(new ClaimAccompanimentConfiguration());
        modelBuilder.ApplyConfiguration(new ClaimPhaseConfiguration());
        modelBuilder.ApplyConfiguration(new PhaseEventRelationshipConfiguration());
        modelBuilder.ApplyConfiguration(new ConsortiumContractConfiguration());
        modelBuilder.ApplyConfiguration(new MigrationStatusConfiguration());
        modelBuilder.ApplyConfiguration(new ComponentMigrationConfiguration());
        modelBuilder.ApplyConfiguration(new PerformanceMetricConfiguration());

        // Global query filters (optional - soft delete support)
        // modelBuilder.Entity<ClaimMaster>().HasQueryFilter(e => !e.IsDeleted);

        // Disable cascade delete globally (optional)
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    /// <summary>
    /// Override SaveChanges to automatically update audit fields
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update audit fields
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                // Set CreatedAt and CreatedBy for new entities
                var createdAtProp = entry.Property("CreatedAt");
                if (createdAtProp != null && createdAtProp.CurrentValue == null)
                {
                    createdAtProp.CurrentValue = DateTime.Now;
                }

                var createdByProp = entry.Property("CreatedBy");
                if (createdByProp != null && createdByProp.CurrentValue == null)
                {
                    // Set from current user context
                    createdByProp.CurrentValue = GetCurrentUserId();
                }
            }

            if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
            {
                // Always update UpdatedAt and UpdatedBy
                var updatedAtProp = entry.Property("UpdatedAt");
                if (updatedAtProp != null)
                {
                    updatedAtProp.CurrentValue = DateTime.Now;
                }

                var updatedByProp = entry.Property("UpdatedBy");
                if (updatedByProp != null)
                {
                    updatedByProp.CurrentValue = GetCurrentUserId();
                }
            }
        }
    }

    private string? GetCurrentUserId()
    {
        // TODO: Implement logic to get current user ID from HttpContext or claims
        // This is a placeholder - actual implementation depends on authentication setup
        return "SYSTEM";
    }
}
```

---

## Index Recommendations

### Performance-Critical Indexes

```sql
-- Claim Master Indexes
CREATE INDEX IX_TMESTSIN_Protocol ON TMESTSIN(FONTE, PROTSINI, DAC);
CREATE INDEX IX_TMESTSIN_Leader ON TMESTSIN(CODLIDER, SINLID) WHERE CODLIDER IS NOT NULL;
CREATE INDEX IX_TMESTSIN_Policy ON TMESTSIN(ORGAPO, RMOAPO, NUMAPOL);
CREATE INDEX IX_TMESTSIN_Product ON TMESTSIN(CODPRODU);

-- Claim History Indexes
CREATE INDEX IX_THISTSIN_DateOperation ON THISTSIN(DTMOVTO, OPERACAO);
CREATE INDEX IX_THISTSIN_UserDate ON THISTSIN(EZEUSRID, DTMOVTO);
CREATE INDEX IX_THISTSIN_Accounting ON THISTSIN(SITCONTB, SITUACAO);

-- Currency Unit Indexes
CREATE INDEX IX_TGEUNIMO_ValidityPeriod ON TGEUNIMO(DTINIVIG, DTTERVIG);

-- Policy Master Indexes
CREATE INDEX IX_TAPOLICE_InsuredName ON TAPOLICE(NOME);
CREATE INDEX IX_TAPOLICE_TaxId ON TAPOLICE(CPFCNPJ) WHERE CPFCNPJ IS NOT NULL;
CREATE INDEX IX_TAPOLICE_Validity ON TAPOLICE(DTINIVIG, DTFIMVIG);

-- Claim Accompaniment Indexes
CREATE INDEX IX_SI_ACOMPANHA_EventDate ON SI_ACOMPANHA_SINI(COD_EVENTO, DATA_MOVTO_SINIACO);
CREATE INDEX IX_SI_ACOMPANHA_UserDate ON SI_ACOMPANHA_SINI(COD_USUARIO, DATA_MOVTO_SINIACO);

-- Claim Phase Indexes
CREATE INDEX IX_SI_SINISTRO_FASE_PhaseOpening ON SI_SINISTRO_FASE(COD_FASE, DATA_ABERTURA_SIFA);
CREATE INDEX IX_SI_SINISTRO_FASE_Closing ON SI_SINISTRO_FASE(DATA_FECHA_SIFA) WHERE YEAR(DATA_FECHA_SIFA) <> 9999;

-- Phase-Event Relationship Indexes
CREATE INDEX IX_SI_REL_FASE_EVENTO_EventDate ON SI_REL_FASE_EVENTO(COD_EVENTO, DATA_INIVIG_REFAEV);

-- Consortium Contract Indexes
CREATE INDEX IX_EF_CONTR_SEG_HABIT_Proposal ON EF_CONTR_SEG_HABIT(NUM_PROPOSTA);
CREATE INDEX IX_EF_CONTR_SEG_HABIT_TaxId ON EF_CONTR_SEG_HABIT(CPF_CNPJ_SEGURADO);
CREATE INDEX IX_EF_CONTR_SEG_HABIT_Status ON EF_CONTR_SEG_HABIT(SITUACAO_CONTRATO, DATA_INICIO_VIGENCIA);

-- Migration Status Indexes
CREATE UNIQUE INDEX IX_MIGRATION_STATUS_Code ON MIGRATION_STATUS(USER_STORY_CODE);
CREATE INDEX IX_MIGRATION_STATUS_StatusDate ON MIGRATION_STATUS(STATUS, ESTIMATED_COMPLETION);
CREATE INDEX IX_MIGRATION_STATUS_Overdue ON MIGRATION_STATUS(ESTIMATED_COMPLETION) WHERE STATUS <> 'Completed' AND ESTIMATED_COMPLETION < GETDATE();

-- Component Migration Indexes
CREATE INDEX IX_COMPONENT_MIGRATION_StatusType ON COMPONENT_MIGRATION(MIGRATION_STATUS_ID, COMPONENT_TYPE);
CREATE INDEX IX_COMPONENT_MIGRATION_StatusDev ON COMPONENT_MIGRATION(STATUS, ASSIGNED_DEVELOPER);
CREATE INDEX IX_COMPONENT_MIGRATION_TypeComplexity ON COMPONENT_MIGRATION(COMPONENT_TYPE, COMPLEXITY);

-- Performance Metrics Indexes
CREATE INDEX IX_PERFORMANCE_METRICS_ComponentType ON PERFORMANCE_METRICS(COMPONENT_ID, METRIC_TYPE);
CREATE INDEX IX_PERFORMANCE_METRICS_Timestamp ON PERFORMANCE_METRICS(MEASUREMENT_TIMESTAMP);
CREATE INDEX IX_PERFORMANCE_METRICS_PassFail ON PERFORMANCE_METRICS(PASS_FAIL, METRIC_TYPE);
```

---

## Migration Strategy

### Database Migration Order

1. **Legacy Tables** (read-only, map to existing):
   - TGERAMO (Branch Master)
   - TSISTEMA (System Control)
   - TGEUNIMO (Currency Unit)
   - TAPOLICE (Policy Master)
   - TMESTSIN (Claim Master)
   - THISTSIN (Claim History)
   - SI_REL_FASE_EVENTO (Phase-Event Relationship)
   - SI_ACOMPANHA_SINI (Claim Accompaniment)
   - SI_SINISTRO_FASE (Claim Phase)
   - EF_CONTR_SEG_HABIT (Consortium Contract)

2. **New Dashboard Tables** (create fresh):
   - MIGRATION_STATUS
   - COMPONENT_MIGRATION
   - PERFORMANCE_METRICS

### EF Core Migration Commands

```bash
# Add initial migration for dashboard tables only
dotnet ef migrations add InitialDashboardTables --context ClaimsDbContext --output-dir Infrastructure/Migrations

# Update database
dotnet ef database update --context ClaimsDbContext

# Add migration for audit fields to legacy tables (if modifying)
dotnet ef migrations add AddAuditFieldsToLegacyTables --context ClaimsDbContext

# Generate SQL script for review
dotnet ef migrations script --context ClaimsDbContext --output migration.sql
```

### Data Seeding

```csharp
public static class DataSeeder
{
    public static void SeedMigrationData(ClaimsDbContext context)
    {
        // Seed User Stories
        if (!context.MigrationStatuses.Any())
        {
            var userStories = new[]
            {
                new MigrationStatus
                {
                    UserStoryCode = "US-001",
                    UserStoryName = "Search and Retrieve Claim",
                    Status = "In Progress",
                    Priority = "P1",
                    RequirementsTotal = 5,
                    RequirementsCompleted = 3,
                    TestsTotal = 10,
                    TestsPassed = 7,
                    CompletionPercentage = 60m,
                    StartDate = new DateTime(2025, 10, 1),
                    EstimatedCompletion = new DateTime(2025, 10, 15),
                    AssignedTo = "Development Team"
                },
                new MigrationStatus
                {
                    UserStoryCode = "US-002",
                    UserStoryName = "Authorize Claim Payment",
                    Status = "In Progress",
                    Priority = "P2",
                    RequirementsTotal = 6,
                    RequirementsCompleted = 2,
                    TestsTotal = 15,
                    TestsPassed = 5,
                    CompletionPercentage = 33m,
                    StartDate = new DateTime(2025, 10, 10),
                    EstimatedCompletion = new DateTime(2025, 10, 25),
                    AssignedTo = "Development Team"
                },
                // Add remaining user stories...
            };

            context.MigrationStatuses.AddRange(userStories);
            context.SaveChanges();
        }

        // Seed Component Migrations
        if (!context.ComponentMigrations.Any())
        {
            var us001Id = context.MigrationStatuses.First(m => m.UserStoryCode == "US-001").Id;

            var components = new[]
            {
                new ComponentMigration
                {
                    MigrationStatusId = us001Id,
                    ComponentType = "Screen",
                    ComponentName = "Claim Search Screen",
                    LegacyReference = "SIWEA Map Group SIWEG",
                    Status = "Completed",
                    Complexity = "Medium",
                    EstimatedHours = 16m,
                    ActualHours = 18m,
                    AssignedDeveloper = "Developer 1",
                    CompletionDate = new DateTime(2025, 10, 12)
                },
                // Add more components...
            };

            context.ComponentMigrations.AddRange(components);
            context.SaveChanges();
        }
    }
}
```

---

## Usage Examples

### 1. Search Claim by Protocol

```csharp
public async Task<ClaimMaster?> SearchClaimByProtocolAsync(int fonte, int protsini, int dac)
{
    return await _context.ClaimMasters
        .Include(c => c.Branch)
        .Include(c => c.Policy)
        .Include(c => c.ClaimHistories.OrderByDescending(h => h.Ocorhist))
        .FirstOrDefaultAsync(c => c.Fonte == fonte && c.Protsini == protsini && c.Dac == dac);
}
```

### 2. Authorize Payment

```csharp
public async Task<Result> AuthorizePaymentAsync(PaymentAuthorizationDto dto)
{
    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        // Get claim
        var claim = await _context.ClaimMasters
            .FirstOrDefaultAsync(c => c.Tipseg == dto.Tipseg &&
                                     c.Orgsin == dto.Orgsin &&
                                     c.Rmosin == dto.Rmosin &&
                                     c.Numsin == dto.Numsin);

        if (claim == null)
            return Result.Fail("DOCUMENTO NAO CADASTRADO");

        // Get business date
        var systemControl = await _context.SystemControls
            .FirstAsync(s => s.Idsistem == "SI");

        // Get currency conversion rate
        var currencyRate = await _context.CurrencyUnits
            .Where(u => u.Dtinivig <= systemControl.Dtmovabe &&
                       u.Dttervig >= systemControl.Dtmovabe)
            .FirstOrDefaultAsync();

        if (currencyRate == null)
            return Result.Fail("Currency rate not found for business date");

        // Increment occurrence counter
        claim.Ocorhist++;

        // Create history record
        var history = new ClaimHistory
        {
            Tipseg = claim.Tipseg,
            Orgsin = claim.Orgsin,
            Rmosin = claim.Rmosin,
            Numsin = claim.Numsin,
            Ocorhist = claim.Ocorhist,
            Operacao = 1098,
            Dtmovto = systemControl.Dtmovabe,
            Horaoper = DateTime.Now.ToString("HHmmss"),
            Valpri = dto.PrincipalAmount,
            Crrmon = dto.CorrectionAmount,
            Nomfav = dto.BeneficiaryName,
            Tippag = dto.PaymentType,
            Ezeusrid = _currentUserService.GetUserId(),
            // Calculate BTNF values
            Valpribt = dto.PrincipalAmount * currencyRate.Vlcruzad,
            Crrmonbt = dto.CorrectionAmount * currencyRate.Vlcruzad,
            Valpriconv = dto.PrincipalAmount * currencyRate.Vlcruzad,
            // Calculate totals
            Valtotbt = (dto.PrincipalAmount + dto.CorrectionAmount) * currencyRate.Vlcruzad
        };

        _context.ClaimHistories.Add(history);

        // Update claim totals
        claim.Totpag += history.Valtotbt;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Result.Ok();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return Result.Fail(ex.Message);
    }
}
```

### 3. Track Phase Changes

```csharp
public async Task UpdateClaimPhaseAsync(int fonte, int protsini, int dac, int eventCode)
{
    var today = DateTime.Today;

    // Get phase-event relationships for this event
    var phaseRelationships = await _context.PhaseEventRelationships
        .Where(r => r.CodEvento == eventCode &&
                   r.DataInivigRefaev <= today &&
                   (r.DataTervigRefaev == null || r.DataTervigRefaev >= today) &&
                   r.Ativo)
        .ToListAsync();

    foreach (var relationship in phaseRelationships)
    {
        if (relationship.OpensPhase)
        {
            // Open new phase
            var newPhase = new ClaimPhase
            {
                Fonte = fonte,
                Protsini = protsini,
                Dac = dac,
                CodFase = relationship.CodFase,
                CodEvento = eventCode,
                NumOcorrSiniaco = 1, // Get from claim accompaniment
                DataInivigRefaev = relationship.DataInivigRefaev,
                DataAberturaSifa = today,
                DataFechaSifa = new DateTime(9999, 12, 31)
            };

            _context.ClaimPhases.Add(newPhase);
        }
        else if (relationship.ClosesPhase)
        {
            // Close existing phase
            var existingPhase = await _context.ClaimPhases
                .Where(p => p.Fonte == fonte &&
                           p.Protsini == protsini &&
                           p.Dac == dac &&
                           p.CodFase == relationship.CodFase &&
                           p.IsOpen)
                .FirstOrDefaultAsync();

            if (existingPhase != null)
            {
                existingPhase.DataFechaSifa = today;
            }
        }
    }

    await _context.SaveChangesAsync();
}
```

### 4. Dashboard Queries

```csharp
public async Task<MigrationDashboardDto> GetDashboardDataAsync()
{
    var migrationStatuses = await _context.MigrationStatuses
        .Include(m => m.Components)
        .ThenInclude(c => c.PerformanceMetrics)
        .ToListAsync();

    var overallProgress = migrationStatuses.Average(m => m.CompletionPercentage);

    var componentsSummary = await _context.ComponentMigrations
        .GroupBy(c => c.ComponentType)
        .Select(g => new
        {
            Type = g.Key,
            Total = g.Count(),
            Completed = g.Count(c => c.Status == "Completed"),
            InProgress = g.Count(c => c.Status == "In Progress"),
            NotStarted = g.Count(c => c.Status == "Not Started"),
            Blocked = g.Count(c => c.Status == "Blocked")
        })
        .ToListAsync();

    var performanceSummary = await _context.PerformanceMetrics
        .Where(p => p.PassFail)
        .GroupBy(p => p.MetricType)
        .Select(g => new
        {
            MetricType = g.Key,
            AverageImprovement = g.Average(p =>
                (p.LegacyValue - p.NewValue) / p.LegacyValue * 100)
        })
        .ToListAsync();

    return new MigrationDashboardDto
    {
        OverallProgress = overallProgress,
        UserStories = migrationStatuses,
        ComponentsSummary = componentsSummary,
        PerformanceSummary = performanceSummary
    };
}
```

---

## Conclusion

This data model provides a complete, production-ready foundation for the Claims System migration from IBM Visual Age to .NET 9. All 13 entities are:

- **Fully documented** with XML comments explaining Portuguese field names and business rules
- **EF Core 9 compliant** with Fluent API configurations and data annotations
- **Performance-optimized** with strategic indexes and query patterns
- **Audit-ready** with comprehensive audit fields and concurrency handling
- **Relationship-complete** with properly configured navigation properties

The model supports both legacy system integration (read-only mapping to existing tables) and new dashboard functionality (migration tracking and performance monitoring), enabling a smooth transition while maintaining full operational capability.

