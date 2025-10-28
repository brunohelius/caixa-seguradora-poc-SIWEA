# SIWEA - Modelo de Dados Completo

**Documento:** Modelo de Dados Detalhado (13 Entidades)
**Versão:** 1.0
**Data:** 2025-10-27
**Documento:** 4 de 5
**Referência:** SISTEMA_LEGADO_VISAO_GERAL.md, SISTEMA_LEGADO_ARQUITETURA.md

---

## Índice

1. [Introdução](#introdução)
2. [Diagrama Entidade-Relacionamento](#diagrama-entidade-relacionamento)
3. [Entidades Legadas (10)](#entidades-legadas)
4. [Entidades do Dashboard (3)](#entidades-do-dashboard)
5. [Relacionamentos e Cardinalidades](#relacionamentos-e-cardinalidades)
6. [Índices e Otimizações](#índices-e-otimizações)
7. [Constraints e Regras de Integridade](#constraints-e-regras-de-integridade)
8. [Mapeamento para .NET EF Core](#mapeamento-para-net-ef-core)

---

## Introdução

### Propósito

Este documento fornece a especificação completa do modelo de dados do sistema SIWEA, incluindo:
- 10 tabelas legadas do sistema mainframe
- 3 tabelas novas para o dashboard de migração
- Todos os campos, tipos, constraints e relacionamentos
- Mapeamento para Entity Framework Core 9

### Convenções

**Nomenclatura Legada:**
- Tabelas: MAIÚSCULAS (ex: TMESTSIN)
- Campos: MAIÚSCULAS com abreviações (ex: SDOPAG, TOTPAG)
- Chaves: Compostas com múltiplas partes

**Nomenclatura .NET:**
- Classes C#: PascalCase (ex: ClaimMaster)
- Propriedades: PascalCase (ex: SaldoAPagar)
- Tabelas DB: Manter nomes legados via Fluent API

**Tipos de Dados:**
```
DECIMAL(15,2)   → decimal em C#
DECIMAL(18,8)   → decimal em C#
INT             → int em C#
VARCHAR(n)      → string em C#
CHAR(1)         → string em C# (length 1)
DATE            → DateTime em C# (apenas date)
DATETIME        → DateTime em C#
BINARY          → byte[] em C# (RowVersion)
GUID            → Guid em C#
```

---

## Diagrama Entidade-Relacionamento

### Diagrama Conceitual

```
                  ┌─────────────┐
                  │  TSISTEMA   │
                  │ (Controle)  │
                  └─────────────┘
                        │
                        │ Fornece DTMOVABE
                        │ (data de negócio)
                        ▼
┌─────────────┐   ┌─────────────────────────┐
│  TGERAMO    │◄──│      TMESTSIN           │
│  (Ramos)    │   │   (Claim Master)        │
└─────────────┘   │   PK: 4 partes          │
                  │   - TIPSEG              │
┌─────────────┐   │   - ORGSIN              │
│  TAPOLICE   │◄──│   - RMOSIN              │
│ (Policies)  │   │   - NUMSIN              │
└─────────────┘   └──┬───────────┬──────────┘
                     │           │
          ┌──────────┼───────────┼──────────┐
          │          │           │          │
          ▼          ▼           ▼          ▼
  ┌────────────┐ ┌────────┐  ┌────────┐ ┌────────────┐
  │ THISTSIN   │ │SI_ACOM │  │SI_SINI │ │EF_CONTR_   │
  │ (History)  │ │PANHA   │  │STRO    │ │SEG_HABIT   │
  │ PK: 5 pts  │ │_SINI   │  │_FASE   │ │(Contracts) │
  └────────────┘ │(Events)│  │(Phases)│ └────────────┘
          │      └────────┘  └────┬───┘
          │                       │
          │                       │
          │                       ▼
          │              ┌─────────────────┐
          │              │SI_REL_FASE_     │
          │              │EVENTO (Config)  │
          │              └─────────────────┘
          │
          ▼
    ┌──────────────┐
    │  TGEUNIMO    │
    │  (Currency)  │
    │  Conversion  │
    └──────────────┘


ENTIDADES DASHBOARD (Migração):

┌────────────────────┐
│ MIGRATION_STATUS   │
│ (User Stories)     │
└──────┬─────────────┘
       │
       │ 1:N
       ▼
┌─────────────────────────┐
│ COMPONENT_MIGRATION     │
│ (Components)            │
└──────┬──────────────────┘
       │
       │ 1:N
       ▼
┌─────────────────────────┐
│ PERFORMANCE_METRICS     │
│ (Benchmarking)          │
└─────────────────────────┘
```

---

## Entidades Legadas

### 1. TMESTSIN - Claim Master Record

**Propósito:** Registro mestre do sinistro com identificação, dados financeiros e referências

**Tabela DB2:** TMESTSIN

**Classe C#:** ClaimMaster

#### Estrutura Completa

```csharp
public class ClaimMaster
{
    // CHAVE PRIMÁRIA (4 partes)
    public int InsuranceType { get; set; }      // TIPSEG (PK Part 1)
    public int ClaimOrigin { get; set; }        // ORGSIN (PK Part 2)
    public int ClaimBranch { get; set; }        // RMOSIN (PK Part 3)
    public int ClaimNumber { get; set; }        // NUMSIN (PK Part 4)

    // IDENTIFICAÇÃO DE PROTOCOLO (Busca alternativa)
    public int ProtocolSource { get; set; }     // FONTE (Index)
    public int ProtocolNumber { get; set; }     // PROTSINI (Index)
    public int ProtocolCheckDigit { get; set; } // DAC (Index)

    // REFERÊNCIA DE APÓLICE
    public int PolicyOrigin { get; set; }       // ORGAPO
    public int PolicyBranch { get; set; }       // RMOAPO
    public int PolicyNumber { get; set; }       // NUMAPOL

    // DADOS FINANCEIROS
    public decimal ExpectedReserve { get; set; } // SDOPAG (Saldo a Pagar)
    public decimal TotalPaid { get; set; }       // TOTPAG (Total Pago)

    // PRODUTO E TIPO
    public int ProductCode { get; set; }        // CODPRODU (routing)
    public char PolicyTypeIndicator { get; set; } // TIPREG ('1' ou '2')
    public int InsuranceTypeFromPolicy { get; set; } // TPSEGU (beneficiary req)

    // CÓDIGO LÍDER (Resseguro) - OPCIONAL
    public int? LeaderCode { get; set; }        // CODLIDER (nullable)
    public int? LeaderClaimNumber { get; set; } // SINLID (nullable)

    // CONTADOR DE HISTÓRICO
    public int OccurrenceCounter { get; set; }  // OCORHIST

    // AUDITORIA
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; }      // Concurrency token

    // RELACIONAMENTOS
    public BranchMaster Branch { get; set; }
    public PolicyMaster Policy { get; set; }
    public ICollection<ClaimHistory> Histories { get; set; }
    public ICollection<ClaimAccompaniment> Accompaniments { get; set; }
    public ICollection<ClaimPhase> Phases { get; set; }

    // PROPRIEDADES CALCULADAS
    public decimal PendingValue => ExpectedReserve - TotalPaid;
    public string ProtocolFormatted => $"{ProtocolSource:000}/{ProtocolNumber:0000000}-{ProtocolCheckDigit}";
    public string ClaimNumberFormatted => $"{ClaimOrigin:00}/{ClaimBranch:00}/{ClaimNumber:000000}";
}
```

#### Mapeamento EF Core

```csharp
public class ClaimMasterConfiguration : IEntityTypeConfiguration<ClaimMaster>
{
    public void Configure(EntityTypeBuilder<ClaimMaster> builder)
    {
        builder.ToTable("TMESTSIN");

        // Chave primária composta
        builder.HasKey(e => new { e.InsuranceType, e.ClaimOrigin, e.ClaimBranch, e.ClaimNumber });

        // Propriedades
        builder.Property(e => e.InsuranceType).HasColumnName("TIPSEG").IsRequired();
        builder.Property(e => e.ClaimOrigin).HasColumnName("ORGSIN").IsRequired();
        builder.Property(e => e.ClaimBranch).HasColumnName("RMOSIN").IsRequired();
        builder.Property(e => e.ClaimNumber).HasColumnName("NUMSIN").IsRequired();

        builder.Property(e => e.ProtocolSource).HasColumnName("FONTE").IsRequired();
        builder.Property(e => e.ProtocolNumber).HasColumnName("PROTSINI").IsRequired();
        builder.Property(e => e.ProtocolCheckDigit).HasColumnName("DAC").IsRequired();

        builder.Property(e => e.PolicyOrigin).HasColumnName("ORGAPO").IsRequired();
        builder.Property(e => e.PolicyBranch).HasColumnName("RMOAPO").IsRequired();
        builder.Property(e => e.PolicyNumber).HasColumnName("NUMAPOL").IsRequired();

        builder.Property(e => e.ExpectedReserve)
            .HasColumnName("SDOPAG")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.TotalPaid)
            .HasColumnName("TOTPAG")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.ProductCode).HasColumnName("CODPRODU").IsRequired();
        builder.Property(e => e.PolicyTypeIndicator).HasColumnName("TIPREG").HasMaxLength(1).IsRequired();
        builder.Property(e => e.InsuranceTypeFromPolicy).HasColumnName("TPSEGU").IsRequired();

        builder.Property(e => e.LeaderCode).HasColumnName("CODLIDER");
        builder.Property(e => e.LeaderClaimNumber).HasColumnName("SINLID");

        builder.Property(e => e.OccurrenceCounter).HasColumnName("OCORHIST").IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(50);
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
        builder.Property(e => e.RowVersion).HasColumnName("ROW_VERSION").IsRowVersion();

        // Índices
        builder.HasIndex(e => new { e.ProtocolSource, e.ProtocolNumber, e.ProtocolCheckDigit })
            .HasDatabaseName("IX_TMESTSIN_Protocol");

        builder.HasIndex(e => new { e.LeaderCode, e.LeaderClaimNumber })
            .HasDatabaseName("IX_TMESTSIN_Leader");

        builder.HasIndex(e => new { e.PolicyOrigin, e.PolicyBranch, e.PolicyNumber })
            .HasDatabaseName("IX_TMESTSIN_Policy");

        // Relacionamentos
        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.ClaimBranch)
            .HasPrincipalKey(b => b.BranchCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Policy)
            .WithMany()
            .HasForeignKey(e => new { e.PolicyOrigin, e.PolicyBranch, e.PolicyNumber })
            .HasPrincipalKey(p => new { p.PolicyOrigin, p.PolicyBranch, p.PolicyNumber })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Histories)
            .WithOne(h => h.Claim)
            .HasForeignKey(h => new { h.InsuranceType, h.ClaimOrigin, h.ClaimBranch, h.ClaimNumber })
            .OnDelete(DeleteBehavior.Restrict);

        // Propriedades ignoradas (calculadas)
        builder.Ignore(e => e.PendingValue);
        builder.Ignore(e => e.ProtocolFormatted);
        builder.Ignore(e => e.ClaimNumberFormatted);
    }
}
```

#### Constraints

```sql
-- Checks financeiros
ALTER TABLE TMESTSIN
ADD CONSTRAINT CHK_SDOPAG_NONNEG CHECK (SDOPAG >= 0);

ALTER TABLE TMESTSIN
ADD CONSTRAINT CHK_TOTPAG_NONNEG CHECK (TOTPAG >= 0);

ALTER TABLE TMESTSIN
ADD CONSTRAINT CHK_TOTPAG_LE_SDOPAG CHECK (TOTPAG <= SDOPAG);

-- Checks de tipo
ALTER TABLE TMESTSIN
ADD CONSTRAINT CHK_TIPREG_VALID CHECK (TIPREG IN ('1', '2'));
```

---

### 2. THISTSIN - Payment History Record

**Propósito:** Registro individual de autorização de pagamento

**Tabela DB2:** THISTSIN

**Classe C#:** ClaimHistory

#### Estrutura Completa

```csharp
public class ClaimHistory
{
    // CHAVE PRIMÁRIA (5 partes)
    public int InsuranceType { get; set; }      // TIPSEG (PK Part 1)
    public int ClaimOrigin { get; set; }        // ORGSIN (PK Part 2)
    public int ClaimBranch { get; set; }        // RMOSIN (PK Part 3)
    public int ClaimNumber { get; set; }        // NUMSIN (PK Part 4)
    public int OccurrenceCounter { get; set; }  // OCORHIST (PK Part 5 - sequence)

    // OPERAÇÃO (FIXO)
    public int OperationCode { get; set; }      // OPERACAO (always 1098)

    // TIMESTAMPS
    public DateTime TransactionDate { get; set; } // DTMOVTO (business date)
    public TimeSpan TransactionTime { get; set; } // HORAOPER (HH:MM:SS)

    // VALORES ORIGINAIS
    public decimal PrincipalAmount { get; set; }   // VALPRI
    public decimal CorrectionAmount { get; set; }  // CRRMON

    // VALORES EM BTNF (Standardized Currency)
    public decimal PrincipalAmountBTNF { get; set; }  // VALPRIBT
    public decimal CorrectionAmountBTNF { get; set; } // CRRMONBT
    public decimal TotalAmountBTNF { get; set; }      // VALTOTBT

    // VALORES DIÁRIOS (para cálculos atuariais)
    public decimal PrincipalDailyBTNF { get; set; }   // PRIDIABT
    public decimal CorrectionDailyBTNF { get; set; }  // CRRDIABT
    public decimal TotalDailyBTNF { get; set; }       // TOTDIABT

    // BENEFICIÁRIO
    public string? BeneficiaryName { get; set; }   // NOMFAV (max 255)

    // TIPO DE CORREÇÃO (FIXO)
    public char CorrectionType { get; set; }       // TIPCRR (always '5')

    // STATUS
    public char AccountingStatus { get; set; }     // SITCONTB (init '0')
    public char OverallStatus { get; set; }        // SITUACAO (init '0')

    // OPERADOR (Auditoria)
    public string OperatorUserId { get; set; }     // EZEUSRID

    // AUDITORIA PADRÃO
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; }

    // RELACIONAMENTO
    public ClaimMaster Claim { get; set; }
}
```

#### Mapeamento EF Core

```csharp
public class ClaimHistoryConfiguration : IEntityTypeConfiguration<ClaimHistory>
{
    public void Configure(EntityTypeBuilder<ClaimHistory> builder)
    {
        builder.ToTable("THISTSIN");

        // Chave primária composta de 5 partes
        builder.HasKey(e => new {
            e.InsuranceType,
            e.ClaimOrigin,
            e.ClaimBranch,
            e.ClaimNumber,
            e.OccurrenceCounter
        });

        // Campos PK
        builder.Property(e => e.InsuranceType).HasColumnName("TIPSEG").IsRequired();
        builder.Property(e => e.ClaimOrigin).HasColumnName("ORGSIN").IsRequired();
        builder.Property(e => e.ClaimBranch).HasColumnName("RMOSIN").IsRequired();
        builder.Property(e => e.ClaimNumber).HasColumnName("NUMSIN").IsRequired();
        builder.Property(e => e.OccurrenceCounter).HasColumnName("OCORHIST").IsRequired();

        // Operação
        builder.Property(e => e.OperationCode).HasColumnName("OPERACAO").IsRequired();

        // Timestamps
        builder.Property(e => e.TransactionDate).HasColumnName("DTMOVTO").HasColumnType("date").IsRequired();
        builder.Property(e => e.TransactionTime).HasColumnName("HORAOPER").IsRequired();

        // Valores originais
        builder.Property(e => e.PrincipalAmount)
            .HasColumnName("VALPRI")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.CorrectionAmount)
            .HasColumnName("CRRMON")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        // Valores BTNF
        builder.Property(e => e.PrincipalAmountBTNF)
            .HasColumnName("VALPRIBT")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.CorrectionAmountBTNF)
            .HasColumnName("CRRMONBT")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.TotalAmountBTNF)
            .HasColumnName("VALTOTBT")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        // Valores diários
        builder.Property(e => e.PrincipalDailyBTNF)
            .HasColumnName("PRIDIABT")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.CorrectionDailyBTNF)
            .HasColumnName("CRRDIABT")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        builder.Property(e => e.TotalDailyBTNF)
            .HasColumnName("TOTDIABT")
            .HasColumnType("decimal(15,2)")
            .IsRequired();

        // Beneficiário
        builder.Property(e => e.BeneficiaryName)
            .HasColumnName("NOMFAV")
            .HasMaxLength(255);

        // Tipo correção
        builder.Property(e => e.CorrectionType).HasColumnName("TIPCRR").HasMaxLength(1).IsRequired();

        // Status
        builder.Property(e => e.AccountingStatus).HasColumnName("SITCONTB").HasMaxLength(1).IsRequired();
        builder.Property(e => e.OverallStatus).HasColumnName("SITUACAO").HasMaxLength(1).IsRequired();

        // Operador
        builder.Property(e => e.OperatorUserId).HasColumnName("EZEUSRID").HasMaxLength(50).IsRequired();

        // Auditoria
        builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(50);
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
        builder.Property(e => e.RowVersion).HasColumnName("ROW_VERSION").IsRowVersion();

        // Índice covering para queries de histórico
        builder.HasIndex(e => new { e.InsuranceType, e.ClaimOrigin, e.ClaimBranch, e.ClaimNumber, e.OccurrenceCounter })
            .HasDatabaseName("IX_THISTSIN_Claim_Occurrence")
            .IsDescending(false, false, false, false, true) // OCORHIST DESC
            .IncludeProperties(e => new {
                e.OperationCode,
                e.TransactionDate,
                e.TransactionTime,
                e.PrincipalAmount,
                e.CorrectionAmount,
                e.BeneficiaryName,
                e.TotalAmountBTNF,
                e.OperatorUserId
            });

        // Relacionamento com Claim Master
        builder.HasOne(e => e.Claim)
            .WithMany(c => c.Histories)
            .HasForeignKey(e => new { e.InsuranceType, e.ClaimOrigin, e.ClaimBranch, e.ClaimNumber })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

#### Constraints

```sql
-- Operação FIXA
ALTER TABLE THISTSIN
ADD CONSTRAINT CHK_OPERACAO_1098 CHECK (OPERACAO = 1098);

-- Tipo correção FIXO
ALTER TABLE THISTSIN
ADD CONSTRAINT CHK_TIPCRR_5 CHECK (TIPCRR = '5');

-- Valores não-negativos
ALTER TABLE THISTSIN
ADD CONSTRAINT CHK_VALPRI_NONNEG CHECK (VALPRI >= 0);

ALTER TABLE THISTSIN
ADD CONSTRAINT CHK_CRRMON_NONNEG CHECK (CRRMON >= 0);

-- Total BTNF = soma de principal + correção
ALTER TABLE THISTSIN
ADD CONSTRAINT CHK_VALTOTBT_CALC CHECK (VALTOTBT = VALPRIBT + CRRMONBT);
```

#### Trigger de Imutabilidade

```sql
CREATE TRIGGER TR_THISTSIN_NO_UPDATE_DTMOVTO
ON THISTSIN
FOR UPDATE
AS
BEGIN
    IF UPDATE(DTMOVTO)
    BEGIN
        RAISERROR('Campo DTMOVTO não pode ser alterado após criação', 16, 1)
        ROLLBACK TRANSACTION
    END
END
```

---

### 3. TGERAMO - Branch Master

**Propósito:** Tabela de lookup para nomes de ramos de seguro

**Tabela DB2:** TGERAMO

**Classe C#:** BranchMaster

#### Estrutura Completa

```csharp
public class BranchMaster
{
    // CHAVE PRIMÁRIA
    public int BranchCode { get; set; }         // RMOSIN (PK)

    // NOME DO RAMO
    public string BranchName { get; set; }      // NOMERAMO

    // AUDITORIA
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // RELACIONAMENTOS
    public ICollection<ClaimMaster> Claims { get; set; }
    public ICollection<PolicyMaster> Policies { get; set; }
}
```

#### Mapeamento EF Core

```csharp
public class BranchMasterConfiguration : IEntityTypeConfiguration<BranchMaster>
{
    public void Configure(EntityTypeBuilder<BranchMaster> builder)
    {
        builder.ToTable("TGERAMO");

        builder.HasKey(e => e.BranchCode);

        builder.Property(e => e.BranchCode).HasColumnName("RMOSIN").IsRequired();
        builder.Property(e => e.BranchName).HasColumnName("NOMERAMO").HasMaxLength(100).IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(50);
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

        builder.HasMany(e => e.Claims)
            .WithOne(c => c.Branch)
            .HasForeignKey(c => c.ClaimBranch)
            .HasPrincipalKey(b => b.BranchCode);

        builder.HasMany(e => e.Policies)
            .WithOne(p => p.Branch)
            .HasForeignKey(p => p.PolicyBranch)
            .HasPrincipalKey(b => b.BranchCode);
    }
}
```

#### Dados de Exemplo

```sql
INSERT INTO TGERAMO (RMOSIN, NOMERAMO) VALUES
(10, 'AUTOMÓVEIS'),
(20, 'RESIDENCIAL'),
(30, 'VIDA'),
(40, 'EMPRESARIAL'),
(50, 'TRANSPORTE');
```

---

### 4. TAPOLICE - Policy Master

**Propósito:** Informações da apólice e segurado

**Tabela DB2:** TAPOLICE

**Classe C#:** PolicyMaster

#### Estrutura Completa

```csharp
public class PolicyMaster
{
    // CHAVE PRIMÁRIA (3 partes)
    public int PolicyOrigin { get; set; }       // ORGAPO (PK Part 1)
    public int PolicyBranch { get; set; }       // RMOAPO (PK Part 2)
    public int PolicyNumber { get; set; }       // NUMAPOL (PK Part 3)

    // NOME DO SEGURADO
    public string InsuredName { get; set; }     // NOME

    // AUDITORIA
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // RELACIONAMENTOS
    public BranchMaster Branch { get; set; }
    public ICollection<ClaimMaster> Claims { get; set; }
}
```

#### Mapeamento EF Core

```csharp
public class PolicyMasterConfiguration : IEntityTypeConfiguration<PolicyMaster>
{
    public void Configure(EntityTypeBuilder<PolicyMaster> builder)
    {
        builder.ToTable("TAPOLICE");

        builder.HasKey(e => new { e.PolicyOrigin, e.PolicyBranch, e.PolicyNumber });

        builder.Property(e => e.PolicyOrigin).HasColumnName("ORGAPO").IsRequired();
        builder.Property(e => e.PolicyBranch).HasColumnName("RMOAPO").IsRequired();
        builder.Property(e => e.PolicyNumber).HasColumnName("NUMAPOL").IsRequired();

        builder.Property(e => e.InsuredName).HasColumnName("NOME").HasMaxLength(255).IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(50);
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

        builder.HasOne(e => e.Branch)
            .WithMany(b => b.Policies)
            .HasForeignKey(e => e.PolicyBranch)
            .HasPrincipalKey(b => b.BranchCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Claims)
            .WithOne(c => c.Policy)
            .HasForeignKey(c => new { c.PolicyOrigin, c.PolicyBranch, c.PolicyNumber })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

### 5. TGEUNIMO - Currency Unit Table

**Propósito:** Taxas de conversão monetária com vigência temporal

**Tabela DB2:** TGEUNIMO

**Classe C#:** CurrencyUnit

#### Estrutura Completa

```csharp
public class CurrencyUnit
{
    // CHAVE PRIMÁRIA (2 partes - date range)
    public DateTime EffectiveDateStart { get; set; } // DTINIVIG (PK Part 1)
    public DateTime EffectiveDateEnd { get; set; }   // DTTERVIG (PK Part 2)

    // TAXA DE CONVERSÃO (8 decimais de precisão)
    public decimal ConversionRate { get; set; }      // VLCRUZAD (18,8)

    // AUDITORIA
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // MÉTODO HELPER
    public bool IsValidForDate(DateTime date)
    {
        return date >= EffectiveDateStart && date <= EffectiveDateEnd;
    }
}
```

#### Mapeamento EF Core

```csharp
public class CurrencyUnitConfiguration : IEntityTypeConfiguration<CurrencyUnit>
{
    public void Configure(EntityTypeBuilder<CurrencyUnit> builder)
    {
        builder.ToTable("TGEUNIMO");

        builder.HasKey(e => new { e.EffectiveDateStart, e.EffectiveDateEnd });

        builder.Property(e => e.EffectiveDateStart)
            .HasColumnName("DTINIVIG")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.EffectiveDateEnd)
            .HasColumnName("DTTERVIG")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.ConversionRate)
            .HasColumnName("VLCRUZAD")
            .HasColumnType("decimal(18,8)")
            .IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(50);
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

        // Índice para busca por data
        builder.HasIndex(e => new { e.EffectiveDateStart, e.EffectiveDateEnd })
            .HasDatabaseName("IX_TGEUNIMO_DateRange");

        // Propriedades ignoradas
        builder.Ignore(e => e.IsValidForDate(default));
    }
}
```

#### Query de Lookup

```csharp
public async Task<decimal?> GetConversionRateAsync(DateTime transactionDate)
{
    var rate = await _context.CurrencyUnits
        .Where(cu => transactionDate >= cu.EffectiveDateStart
                  && transactionDate <= cu.EffectiveDateEnd)
        .Select(cu => cu.ConversionRate)
        .SingleOrDefaultAsync();

    return rate;
}
```

#### Constraints

```sql
-- Taxa sempre positiva
ALTER TABLE TGEUNIMO
ADD CONSTRAINT CHK_VLCRUZAD_POSITIVE CHECK (VLCRUZAD > 0);

-- Data fim >= data início
ALTER TABLE TGEUNIMO
ADD CONSTRAINT CHK_DTTERVIG_GE_DTINIVIG CHECK (DTTERVIG >= DTINIVIG);
```

---

### 6. TSISTEMA - System Control

**Propósito:** Data de negócio do sistema (business date)

**Tabela DB2:** TSISTEMA

**Classe C#:** SystemControl

#### Estrutura Completa

```csharp
public class SystemControl
{
    // CHAVE PRIMÁRIA
    public string SystemId { get; set; }        // IDSISTEM (PK) - always 'SI'

    // DATA DE NEGÓCIO
    public DateTime BusinessDate { get; set; }  // DTMOVABE

    // AUDITORIA
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

#### Mapeamento EF Core

```csharp
public class SystemControlConfiguration : IEntityTypeConfiguration<SystemControl>
{
    public void Configure(EntityTypeBuilder<SystemControl> builder)
    {
        builder.ToTable("TSISTEMA");

        builder.HasKey(e => e.SystemId);

        builder.Property(e => e.SystemId).HasColumnName("IDSISTEM").HasMaxLength(10).IsRequired();
        builder.Property(e => e.BusinessDate).HasColumnName("DTMOVABE").HasColumnType("date").IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(50);
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
    }
}
```

#### Query de Acesso

```csharp
public async Task<DateTime> GetBusinessDateAsync()
{
    var systemControl = await _context.SystemControls
        .Where(sc => sc.SystemId == "SI")
        .SingleOrDefaultAsync();

    if (systemControl == null)
        throw new InvalidOperationException("System control record not found for IDSISTEM='SI'");

    return systemControl.BusinessDate;
}
```

---

### 7. SI_ACOMPANHA_SINI - Claim Accompaniment (Event History)

**Propósito:** Trilha de auditoria de eventos do sinistro

**Tabela DB2:** SI_ACOMPANHA_SINI

**Classe C#:** ClaimAccompaniment

#### Estrutura Completa

```csharp
public class ClaimAccompaniment
{
    // CHAVE PRIMÁRIA (5 partes)
    public int ProtocolSource { get; set; }         // FONTE (PK Part 1)
    public int ProtocolNumber { get; set; }         // PROTSINI (PK Part 2)
    public int ProtocolCheckDigit { get; set; }     // DAC (PK Part 3)
    public int EventCode { get; set; }              // COD_EVENTO (PK Part 4)
    public DateTime TransactionDate { get; set; }   // DATA_MOVTO_SINIACO (PK Part 5)

    // SEQUÊNCIA E DESCRIÇÃO
    public int OccurrenceNumber { get; set; }       // NUM_OCORR_SINIACO
    public string? ComplementaryDescription { get; set; } // DESCR_COMPLEMENTAR

    // OPERADOR
    public string UserCode { get; set; }            // COD_USUARIO

    // DATA DE VIGÊNCIA (configuração)
    public DateTime EffectiveDateConfig { get; set; } // DATA_INIVIG_REFAEV

    // AUDITORIA
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // RELACIONAMENTOS
    public ClaimMaster Claim { get; set; }
    public PhaseEventRelationship PhaseEventConfig { get; set; }
}
```

#### Mapeamento EF Core

```csharp
public class ClaimAccompanimentConfiguration : IEntityTypeConfiguration<ClaimAccompaniment>
{
    public void Configure(EntityTypeBuilder<ClaimAccompaniment> builder)
    {
        builder.ToTable("SI_ACOMPANHA_SINI");

        builder.HasKey(e => new {
            e.ProtocolSource,
            e.ProtocolNumber,
            e.ProtocolCheckDigit,
            e.EventCode,
            e.TransactionDate
        });

        builder.Property(e => e.ProtocolSource).HasColumnName("FONTE").IsRequired();
        builder.Property(e => e.ProtocolNumber).HasColumnName("PROTSINI").IsRequired();
        builder.Property(e => e.ProtocolCheckDigit).HasColumnName("DAC").IsRequired();
        builder.Property(e => e.EventCode).HasColumnName("COD_EVENTO").IsRequired();
        builder.Property(e => e.TransactionDate).HasColumnName("DATA_MOVTO_SINIACO").HasColumnType("date").IsRequired();

        builder.Property(e => e.OccurrenceNumber).HasColumnName("NUM_OCORR_SINIACO").IsRequired();
        builder.Property(e => e.ComplementaryDescription).HasColumnName("DESCR_COMPLEMENTAR").HasMaxLength(500);
        builder.Property(e => e.UserCode).HasColumnName("COD_USUARIO").HasMaxLength(50).IsRequired();
        builder.Property(e => e.EffectiveDateConfig).HasColumnName("DATA_INIVIG_REFAEV").HasColumnType("date").IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(50);
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

        // Relacionamentos
        builder.HasOne(e => e.Claim)
            .WithMany(c => c.Accompaniments)
            .HasForeignKey(e => new { e.ProtocolSource, e.ProtocolNumber, e.ProtocolCheckDigit })
            .HasPrincipalKey(c => new { c.ProtocolSource, c.ProtocolNumber, c.ProtocolCheckDigit })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PhaseEventConfig)
            .WithMany()
            .HasForeignKey(e => new { e.EventCode, e.EffectiveDateConfig })
            .HasPrincipalKey(p => new { p.EventCode, p.EffectiveDate })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

### 8. SI_SINISTRO_FASE - Claim Phase

**Propósito:** Rastreamento de fases do sinistro

**Tabela DB2:** SI_SINISTRO_FASE

**Classe C#:** ClaimPhase

#### Estrutura Completa

```csharp
public class ClaimPhase
{
    // CHAVE PRIMÁRIA (7 partes)
    public int ProtocolSource { get; set; }         // FONTE (PK Part 1)
    public int ProtocolNumber { get; set; }         // PROTSINI (PK Part 2)
    public int ProtocolCheckDigit { get; set; }     // DAC (PK Part 3)
    public int PhaseCode { get; set; }              // COD_FASE (PK Part 4)
    public int EventCode { get; set; }              // COD_EVENTO (PK Part 5)
    public int OccurrenceNumber { get; set; }       // NUM_OCORR_SINIACO (PK Part 6)
    public DateTime EffectiveDate { get; set; }     // DATA_INIVIG_REFAEV (PK Part 7)

    // DATAS DE ABERTURA E FECHAMENTO
    public DateTime OpeningDate { get; set; }       // DATA_ABERTURA_SIFA
    public DateTime ClosingDate { get; set; }       // DATA_FECHA_SIFA ('9999-12-31' = OPEN)

    // AUDITORIA
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; }

    // RELACIONAMENTOS
    public ClaimMaster Claim { get; set; }
    public PhaseEventRelationship PhaseEventConfig { get; set; }

    // PROPRIEDADES CALCULADAS
    public bool IsOpen => ClosingDate == new DateTime(9999, 12, 31);
    public int DaysOpen
    {
        get
        {
            var endDate = IsOpen ? DateTime.Today : ClosingDate;
            return (endDate - OpeningDate).Days;
        }
    }
}
```

#### Mapeamento EF Core

```csharp
public class ClaimPhaseConfiguration : IEntityTypeConfiguration<ClaimPhase>
{
    public void Configure(EntityTypeBuilder<ClaimPhase> builder)
    {
        builder.ToTable("SI_SINISTRO_FASE");

        builder.HasKey(e => new {
            e.ProtocolSource,
            e.ProtocolNumber,
            e.ProtocolCheckDigit,
            e.PhaseCode,
            e.EventCode,
            e.OccurrenceNumber,
            e.EffectiveDate
        });

        builder.Property(e => e.ProtocolSource).HasColumnName("FONTE").IsRequired();
        builder.Property(e => e.ProtocolNumber).HasColumnName("PROTSINI").IsRequired();
        builder.Property(e => e.ProtocolCheckDigit).HasColumnName("DAC").IsRequired();
        builder.Property(e => e.PhaseCode).HasColumnName("COD_FASE").IsRequired();
        builder.Property(e => e.EventCode).HasColumnName("COD_EVENTO").IsRequired();
        builder.Property(e => e.OccurrenceNumber).HasColumnName("NUM_OCORR_SINIACO").IsRequired();
        builder.Property(e => e.EffectiveDate).HasColumnName("DATA_INIVIG_REFAEV").HasColumnType("date").IsRequired();

        builder.Property(e => e.OpeningDate).HasColumnName("DATA_ABERTURA_SIFA").HasColumnType("date").IsRequired();
        builder.Property(e => e.ClosingDate).HasColumnName("DATA_FECHA_SIFA").HasColumnType("date").IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(50);
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
        builder.Property(e => e.RowVersion).HasColumnName("ROW_VERSION").IsRowVersion();

        // Índice filtered para fases abertas
        builder.HasIndex(e => new { e.ProtocolSource, e.ProtocolNumber, e.ProtocolCheckDigit, e.PhaseCode, e.EventCode })
            .HasDatabaseName("IX_FASE_Aberta")
            .HasFilter("[DATA_FECHA_SIFA] = '9999-12-31'")
            .IsUnique();

        // Relacionamentos
        builder.HasOne(e => e.Claim)
            .WithMany(c => c.Phases)
            .HasForeignKey(e => new { e.ProtocolSource, e.ProtocolNumber, e.ProtocolCheckDigit })
            .HasPrincipalKey(c => new { c.ProtocolSource, c.ProtocolNumber, c.ProtocolCheckDigit })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PhaseEventConfig)
            .WithMany()
            .HasForeignKey(e => new { e.PhaseCode, e.EventCode, e.EffectiveDate })
            .HasPrincipalKey(p => new { p.PhaseCode, p.EventCode, p.EffectiveDate })
            .OnDelete(DeleteBehavior.Restrict);

        // Ignorar propriedades calculadas
        builder.Ignore(e => e.IsOpen);
        builder.Ignore(e => e.DaysOpen);
    }
}
```

---

### 9. SI_REL_FASE_EVENTO - Phase-Event Relationship (Configuration)

**Propósito:** Configuração de quais fases são afetadas por quais eventos

**Tabela DB2:** SI_REL_FASE_EVENTO

**Classe C#:** PhaseEventRelationship

#### Estrutura Completa

```csharp
public class PhaseEventRelationship
{
    // CHAVE PRIMÁRIA (3 partes)
    public int PhaseCode { get; set; }              // COD_FASE (PK Part 1)
    public int EventCode { get; set; }              // COD_EVENTO (PK Part 2)
    public DateTime EffectiveDate { get; set; }     // DATA_INIVIG_REFAEV (PK Part 3)

    // INDICADOR DE MUDANÇA
    public char PhaseChangeIndicator { get; set; }  // IND_ALTERACAO_FASE ('1'=open, '2'=close)

    // AUDITORIA
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // PROPRIEDADES CALCULADAS
    public bool IsOpenPhase => PhaseChangeIndicator == '1';
    public bool IsClosePhase => PhaseChangeIndicator == '2';
}
```

#### Mapeamento EF Core

```csharp
public class PhaseEventRelationshipConfiguration : IEntityTypeConfiguration<PhaseEventRelationship>
{
    public void Configure(EntityTypeBuilder<PhaseEventRelationship> builder)
    {
        builder.ToTable("SI_REL_FASE_EVENTO");

        builder.HasKey(e => new { e.PhaseCode, e.EventCode, e.EffectiveDate });

        builder.Property(e => e.PhaseCode).HasColumnName("COD_FASE").IsRequired();
        builder.Property(e => e.EventCode).HasColumnName("COD_EVENTO").IsRequired();
        builder.Property(e => e.EffectiveDate).HasColumnName("DATA_INIVIG_REFAEV").HasColumnType("date").IsRequired();

        builder.Property(e => e.PhaseChangeIndicator)
            .HasColumnName("IND_ALTERACAO_FASE")
            .HasMaxLength(1)
            .IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(50);
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

        // Índice para busca por evento
        builder.HasIndex(e => new { e.EventCode, e.EffectiveDate })
            .HasDatabaseName("IX_REL_FASE_EVENTO_EventDate");

        // Ignorar propriedades calculadas
        builder.Ignore(e => e.IsOpenPhase);
        builder.Ignore(e => e.IsClosePhase);
    }
}
```

#### Constraints

```sql
ALTER TABLE SI_REL_FASE_EVENTO
ADD CONSTRAINT CHK_IND_ALTERACAO_FASE_VALID
CHECK (IND_ALTERACAO_FASE IN ('1', '2'));
```

#### Dados de Exemplo

```sql
-- Evento 1098 (Autorização de Pagamento)
INSERT INTO SI_REL_FASE_EVENTO (COD_FASE, COD_EVENTO, DATA_INIVIG_REFAEV, IND_ALTERACAO_FASE)
VALUES
(5, 1098, '2020-01-01', '1'),  -- Abre fase 5 (Pagamento Autorizado)
(3, 1098, '2020-01-01', '2'),  -- Fecha fase 3 (Análise Pendente)
(7, 1098, '2020-01-01', '1');  -- Abre fase 7 (Aguardando Contabilização)
```

---

### 10. EF_CONTR_SEG_HABIT - Consortium Contract

**Propósito:** Contratos de consórcio para roteamento de validação

**Tabela DB2:** EF_CONTR_SEG_HABIT

**Classe C#:** ConsortiumContract

#### Estrutura Completa

```csharp
public class ConsortiumContract
{
    // CHAVE PRIMÁRIA (composta com apólice)
    public int PolicyOrigin { get; set; }       // ORGAPO (PK Part 1)
    public int PolicyBranch { get; set; }       // RMOAPO (PK Part 2)
    public int PolicyNumber { get; set; }       // NUMAPOL (PK Part 3)

    // NÚMERO DO CONTRATO (determina roteamento)
    public int? ContractNumber { get; set; }    // NUM_CONTRATO (nullable)

    // AUDITORIA
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // PROPRIEDADE CALCULADA
    public bool IsEFPContract => ContractNumber.HasValue && ContractNumber.Value > 0;
    public bool IsHBContract => !ContractNumber.HasValue || ContractNumber.Value == 0;
}
```

#### Mapeamento EF Core

```csharp
public class ConsortiumContractConfiguration : IEntityTypeConfiguration<ConsortiumContract>
{
    public void Configure(EntityTypeBuilder<ConsortiumContract> builder)
    {
        builder.ToTable("EF_CONTR_SEG_HABIT");

        builder.HasKey(e => new { e.PolicyOrigin, e.PolicyBranch, e.PolicyNumber });

        builder.Property(e => e.PolicyOrigin).HasColumnName("ORGAPO").IsRequired();
        builder.Property(e => e.PolicyBranch).HasColumnName("RMOAPO").IsRequired();
        builder.Property(e => e.PolicyNumber).HasColumnName("NUMAPOL").IsRequired();

        builder.Property(e => e.ContractNumber).HasColumnName("NUM_CONTRATO");

        builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(50);
        builder.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

        // Ignorar propriedades calculadas
        builder.Ignore(e => e.IsEFPContract);
        builder.Ignore(e => e.IsHBContract);
    }
}
```

#### Query de Roteamento

```csharp
public async Task<ValidationServiceRoute> DetermineValidationRouteAsync(
    int policyOrigin, int policyBranch, int policyNumber)
{
    var contract = await _context.ConsortiumContracts
        .Where(c => c.PolicyOrigin == policyOrigin
                 && c.PolicyBranch == policyBranch
                 && c.PolicyNumber == policyNumber)
        .SingleOrDefaultAsync();

    if (contract == null || contract.ContractNumber == null || contract.ContractNumber == 0)
        return ValidationServiceRoute.SIMDA; // HB

    return ValidationServiceRoute.SIPUA; // EFP
}
```

---

## Entidades do Dashboard

### 11. MIGRATION_STATUS - Project Progress Tracking

**Propósito:** Rastrear progresso das User Stories

**Tabela:** MIGRATION_STATUS (nova)

**Classe C#:** MigrationStatus

#### Estrutura Completa

```csharp
public class MigrationStatus
{
    public Guid Id { get; set; }                    // PK
    public string UserStoryCode { get; set; }       // UK (US1, US2, etc.)
    public string UserStoryName { get; set; }       // Nome da história
    public string Status { get; set; }              // Not Started, In Progress, Completed, Blocked
    public decimal CompletionPercentage { get; set; } // 0-100
    public int RequirementsCompleted { get; set; }
    public int RequirementsTotal { get; set; }
    public int TestsPassed { get; set; }
    public int TestsTotal { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EstimatedCompletion { get; set; }
    public DateTime? ActualCompletion { get; set; }
    public string? BlockingIssues { get; set; }

    // Auditoria
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Relacionamentos
    public ICollection<ComponentMigration> Components { get; set; }

    // Propriedades calculadas
    public bool IsCompleted => Status == "Completed";
    public bool IsBlocked => Status == "Blocked";
}
```

#### Mapeamento EF Core

```csharp
public class MigrationStatusConfiguration : IEntityTypeConfiguration<MigrationStatus>
{
    public void Configure(EntityTypeBuilder<MigrationStatus> builder)
    {
        builder.ToTable("MIGRATION_STATUS");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserStoryCode).HasMaxLength(10).IsRequired();
        builder.HasIndex(e => e.UserStoryCode).IsUnique();

        builder.Property(e => e.UserStoryName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(50).IsRequired();
        builder.Property(e => e.CompletionPercentage).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(e => e.AssignedTo).HasMaxLength(100);
        builder.Property(e => e.BlockingIssues).HasColumnType("nvarchar(max)");

        builder.Property(e => e.CreatedBy).HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(50);
        builder.Property(e => e.UpdatedAt);

        builder.HasMany(e => e.Components)
            .WithOne(c => c.MigrationStatus)
            .HasForeignKey(c => c.MigrationStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.IsCompleted);
        builder.Ignore(e => e.IsBlocked);
    }
}
```

---

### 12. COMPONENT_MIGRATION - Component-Level Tracking

**Propósito:** Rastrear componentes individuais

**Tabela:** COMPONENT_MIGRATION (nova)

**Classe C#:** ComponentMigration

#### Estrutura Completa

```csharp
public class ComponentMigration
{
    public Guid Id { get; set; }
    public Guid MigrationStatusId { get; set; }  // FK
    public string ComponentType { get; set; }    // screen, business_rule, database_entity, external_service
    public string ComponentName { get; set; }
    public string? LegacyReference { get; set; }
    public string Status { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public string Complexity { get; set; }       // Low, Medium, High
    public string? AssignedDeveloper { get; set; }
    public string? TechnicalNotes { get; set; }

    // Auditoria
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Relacionamentos
    public MigrationStatus MigrationStatus { get; set; }
    public ICollection<PerformanceMetric> PerformanceMetrics { get; set; }
}
```

#### Mapeamento EF Core

```csharp
public class ComponentMigrationConfiguration : IEntityTypeConfiguration<ComponentMigration>
{
    public void Configure(EntityTypeBuilder<ComponentMigration> builder)
    {
        builder.ToTable("COMPONENT_MIGRATION");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ComponentType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ComponentName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.LegacyReference).HasMaxLength(255);
        builder.Property(e => e.Status).HasMaxLength(50).IsRequired();
        builder.Property(e => e.EstimatedHours).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(e => e.ActualHours).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(e => e.Complexity).HasMaxLength(20).IsRequired();
        builder.Property(e => e.AssignedDeveloper).HasMaxLength(100);
        builder.Property(e => e.TechnicalNotes).HasColumnType("nvarchar(max)");

        builder.Property(e => e.CreatedBy).HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(50);
        builder.Property(e => e.UpdatedAt);

        builder.HasOne(e => e.MigrationStatus)
            .WithMany(m => m.Components)
            .HasForeignKey(e => e.MigrationStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.PerformanceMetrics)
            .WithOne(p => p.Component)
            .HasForeignKey(p => p.ComponentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

### 13. PERFORMANCE_METRICS - Benchmarking Data

**Propósito:** Comparar performance legacy vs novo sistema

**Tabela:** PERFORMANCE_METRICS (nova)

**Classe C#:** PerformanceMetric

#### Estrutura Completa

```csharp
public class PerformanceMetric
{
    public Guid Id { get; set; }
    public Guid ComponentId { get; set; }        // FK
    public string MetricType { get; set; }       // response_time, throughput, etc.
    public decimal LegacyValue { get; set; }
    public decimal NewValue { get; set; }
    public DateTime MeasurementTimestamp { get; set; }
    public string TestScenario { get; set; }
    public bool PassFail { get; set; }

    // Auditoria
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Relacionamento
    public ComponentMigration Component { get; set; }

    // Propriedades calculadas
    public decimal ImprovementPercentage => LegacyValue > 0
        ? ((LegacyValue - NewValue) / LegacyValue) * 100
        : 0;
}
```

#### Mapeamento EF Core

```csharp
public class PerformanceMetricConfiguration : IEntityTypeConfiguration<PerformanceMetric>
{
    public void Configure(EntityTypeBuilder<PerformanceMetric> builder)
    {
        builder.ToTable("PERFORMANCE_METRICS");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.MetricType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.LegacyValue).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(e => e.NewValue).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(e => e.MeasurementTimestamp).IsRequired();
        builder.Property(e => e.TestScenario).HasMaxLength(255).IsRequired();
        builder.Property(e => e.PassFail).IsRequired();

        builder.Property(e => e.CreatedBy).HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(50);
        builder.Property(e => e.UpdatedAt);

        builder.HasOne(e => e.Component)
            .WithMany(c => c.PerformanceMetrics)
            .HasForeignKey(e => e.ComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.ImprovementPercentage);
    }
}
```

---

## Relacionamentos e Cardinalidades

### Matriz de Relacionamentos

| Entidade Pai | Relacionamento | Entidade Filha | Cardinalidade | FK Delete |
|--------------|----------------|----------------|---------------|-----------|
| TMESTSIN | has many | THISTSIN | 1:N | Restrict |
| TMESTSIN | has many | SI_ACOMPANHA_SINI | 1:N | Restrict |
| TMESTSIN | has many | SI_SINISTRO_FASE | 1:N | Restrict |
| TGERAMO | has many | TMESTSIN | 1:N | Restrict |
| TGERAMO | has many | TAPOLICE | 1:N | Restrict |
| TAPOLICE | has many | TMESTSIN | 1:N | Restrict |
| SI_REL_FASE_EVENTO | has many | SI_ACOMPANHA_SINI | 1:N | Restrict |
| SI_REL_FASE_EVENTO | has many | SI_SINISTRO_FASE | 1:N | Restrict |
| MIGRATION_STATUS | has many | COMPONENT_MIGRATION | 1:N | Cascade |
| COMPONENT_MIGRATION | has many | PERFORMANCE_METRICS | 1:N | Cascade |

### Diagrama de Cascata de Delete

```
TMESTSIN (Claim Master)
  ├─ ON DELETE RESTRICT → THISTSIN
  ├─ ON DELETE RESTRICT → SI_ACOMPANHA_SINI
  └─ ON DELETE RESTRICT → SI_SINISTRO_FASE

MIGRATION_STATUS
  └─ ON DELETE CASCADE → COMPONENT_MIGRATION
      └─ ON DELETE CASCADE → PERFORMANCE_METRICS
```

**Justificativa:**
- **Entidades legadas:** DELETE RESTRICT para prevenir perda acidental de dados históricos
- **Entidades dashboard:** DELETE CASCADE para facilitar limpeza de dados de migração

---

## Índices e Otimizações

### Índices Críticos de Performance

#### Busca de Sinistros (< 3s)

```sql
-- Busca por protocolo
CREATE INDEX IX_TMESTSIN_Protocol
ON TMESTSIN (FONTE, PROTSINI, DAC);

-- Busca por número sinistro (PK já indexada)

-- Busca por líder
CREATE INDEX IX_TMESTSIN_Leader
ON TMESTSIN (CODLIDER, SINLID)
WHERE CODLIDER IS NOT NULL AND SINLID IS NOT NULL;
```

#### Histórico de Pagamentos (< 500ms)

```sql
-- Covering index para query de histórico
CREATE INDEX IX_THISTSIN_Claim_Occurrence
ON THISTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC)
INCLUDE (OPERACAO, DTMOVTO, HORAOPER, VALPRI, CRRMON, NOMFAV, VALTOTBT, EZEUSRID);
```

#### Taxa de Conversão (< 100ms)

```sql
CREATE INDEX IX_TGEUNIMO_DateRange
ON TGEUNIMO (DTINIVIG, DTTERVIG);
```

#### Fases Abertas (< 200ms)

```sql
-- Filtered index para fases abertas
CREATE UNIQUE INDEX IX_FASE_Aberta
ON SI_SINISTRO_FASE (FONTE, PROTSINI, DAC, COD_FASE, COD_EVENTO)
WHERE DATA_FECHA_SIFA = '9999-12-31';
```

### Estatísticas de Uso de Índices

```sql
-- Query para monitorar uso de índices
SELECT
    OBJECT_NAME(s.object_id) AS TableName,
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE OBJECTPROPERTY(s.object_id, 'IsUserTable') = 1
ORDER BY (s.user_seeks + s.user_scans + s.user_lookups) DESC;
```

---

## Constraints e Regras de Integridade

### Summary de Constraints por Tabela

| Tabela | PKs | FKs | Checks | Unique | Defaults |
|--------|-----|-----|--------|--------|----------|
| TMESTSIN | 4-part | 2 | 3 | 0 | 0 |
| THISTSIN | 5-part | 1 | 4 | 0 | 2 |
| TGERAMO | 1-part | 0 | 0 | 0 | 0 |
| TAPOLICE | 3-part | 1 | 0 | 0 | 0 |
| TGEUNIMO | 2-part | 0 | 2 | 0 | 0 |
| TSISTEMA | 1-part | 0 | 0 | 0 | 0 |
| SI_ACOMPANHA_SINI | 5-part | 2 | 0 | 0 | 0 |
| SI_SINISTRO_FASE | 7-part | 2 | 0 | 1 | 0 |
| SI_REL_FASE_EVENTO | 3-part | 0 | 1 | 0 | 0 |
| EF_CONTR_SEG_HABIT | 3-part | 0 | 0 | 0 | 0 |

### Constraints de Domínio

```sql
-- Valores fixos
ALTER TABLE THISTSIN ADD CONSTRAINT CHK_OPERACAO_1098 CHECK (OPERACAO = 1098);
ALTER TABLE THISTSIN ADD CONSTRAINT CHK_TIPCRR_5 CHECK (TIPCRR = '5');
ALTER TABLE TMESTSIN ADD CONSTRAINT CHK_TIPREG_VALID CHECK (TIPREG IN ('1', '2'));
ALTER TABLE SI_REL_FASE_EVENTO ADD CONSTRAINT CHK_IND_ALTERACAO_FASE_VALID CHECK (IND_ALTERACAO_FASE IN ('1', '2'));

-- Valores não-negativos
ALTER TABLE TMESTSIN ADD CONSTRAINT CHK_SDOPAG_NONNEG CHECK (SDOPAG >= 0);
ALTER TABLE TMESTSIN ADD CONSTRAINT CHK_TOTPAG_NONNEG CHECK (TOTPAG >= 0);
ALTER TABLE THISTSIN ADD CONSTRAINT CHK_VALPRI_NONNEG CHECK (VALPRI >= 0);
ALTER TABLE THISTSIN ADD CONSTRAINT CHK_CRRMON_NONNEG CHECK (CRRMON >= 0);
ALTER TABLE TGEUNIMO ADD CONSTRAINT CHK_VLCRUZAD_POSITIVE CHECK (VLCRUZAD > 0);

-- Consistência financeira
ALTER TABLE TMESTSIN ADD CONSTRAINT CHK_TOTPAG_LE_SDOPAG CHECK (TOTPAG <= SDOPAG);
ALTER TABLE THISTSIN ADD CONSTRAINT CHK_VALTOTBT_CALC CHECK (VALTOTBT = VALPRIBT + CRRMONBT);

-- Consistência temporal
ALTER TABLE TGEUNIMO ADD CONSTRAINT CHK_DTTERVIG_GE_DTINIVIG CHECK (DTTERVIG >= DTINIVIG);
```

---

## Mapeamento para .NET EF Core

### DbContext Principal

```csharp
public class SiweaDbContext : DbContext
{
    public SiweaDbContext(DbContextOptions<SiweaDbContext> options) : base(options) { }

    // Entidades legadas
    public DbSet<ClaimMaster> ClaimMasters { get; set; }
    public DbSet<ClaimHistory> ClaimHistories { get; set; }
    public DbSet<BranchMaster> BranchMasters { get; set; }
    public DbSet<PolicyMaster> PolicyMasters { get; set; }
    public DbSet<CurrencyUnit> CurrencyUnits { get; set; }
    public DbSet<SystemControl> SystemControls { get; set; }
    public DbSet<ClaimAccompaniment> ClaimAccompaniments { get; set; }
    public DbSet<ClaimPhase> ClaimPhases { get; set; }
    public DbSet<PhaseEventRelationship> PhaseEventRelationships { get; set; }
    public DbSet<ConsortiumContract> ConsortiumContracts { get; set; }

    // Entidades dashboard
    public DbSet<MigrationStatus> MigrationStatuses { get; set; }
    public DbSet<ComponentMigration> ComponentMigrations { get; set; }
    public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplicar configurações
        modelBuilder.ApplyConfiguration(new ClaimMasterConfiguration());
        modelBuilder.ApplyConfiguration(new ClaimHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new BranchMasterConfiguration());
        modelBuilder.ApplyConfiguration(new PolicyMasterConfiguration());
        modelBuilder.ApplyConfiguration(new CurrencyUnitConfiguration());
        modelBuilder.ApplyConfiguration(new SystemControlConfiguration());
        modelBuilder.ApplyConfiguration(new ClaimAccompanimentConfiguration());
        modelBuilder.ApplyConfiguration(new ClaimPhaseConfiguration());
        modelBuilder.ApplyConfiguration(new PhaseEventRelationshipConfiguration());
        modelBuilder.ApplyConfiguration(new ConsortiumContractConfiguration());
        modelBuilder.ApplyConfiguration(new MigrationStatusConfiguration());
        modelBuilder.ApplyConfiguration(new ComponentMigrationConfiguration());
        modelBuilder.ApplyConfiguration(new PerformanceMetricConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
```

### Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "SiweaDatabase": "Server=localhost;Database=SIWEA_DB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Registro no Program.cs

```csharp
builder.Services.AddDbContext<SiweaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SiweaDatabase"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(90);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        }));
```

---

## Conclusão

Este documento fornece a especificação completa do modelo de dados do SIWEA com:

✅ **13 entidades completas** (10 legadas + 3 dashboard)
✅ **Todos os campos mapeados** com tipos corretos
✅ **Relacionamentos e cardinalidades** definidos
✅ **Índices críticos de performance** especificados
✅ **Constraints de integridade** documentados
✅ **Mapeamento EF Core 9 completo** pronto para implementação

**Próximo documento:** SISTEMA_LEGADO_PROCESSOS.md (workflows e diagramas de sequência)

---

**FIM - DOCUMENTO 4/5 - MODELO DE DADOS COMPLETO**
