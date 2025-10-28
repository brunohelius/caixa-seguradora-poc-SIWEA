# Data Model: Complete SIWEA Migration - Gaps Analysis and Implementation

**Feature**: Complete 100% SIWEA Migration
**Branch**: 001-complete-migration-gaps
**Date**: 2025-10-27

## Overview

This document defines data structures for implementing migration gaps. **Note**: All 13 core entities already exist in codebase with complete EF Core configurations. This document focuses on new DTOs and validation models required for gap implementation.

## Existing Entities (Reference Only - No Changes)

All entities already implemented in `CaixaSeguradora.Core/Entities/` with EF Core configurations in `CaixaSeguradora.Infrastructure/Data/Configurations/`:

1. **ClaimMaster** (TMESTSIN) - 4-part composite PK
2. **ClaimHistory** (THISTSIN) - 5-part composite PK
3. **BranchMaster** (TGERAMO)
4. **PolicyMaster** (TAPOLICE) - 3-part composite PK
5. **CurrencyUnit** (TGEUNIMO) - Date range-based PK
6. **SystemControl** (TSISTEMA) - IDSISTEM='SI'
7. **ClaimAccompaniment** (SI_ACOMPANHA_SINI)
8. **ClaimPhase** (SI_SINISTRO_FASE)
9. **PhaseEventRelationship** (SI_REL_FASE_EVENTO)
10. **ConsortiumContract** (EF_CONTR_SEG_HABIT)
11. **MigrationStatus** (dashboard tracking)
12. **ComponentMigration** (dashboard tracking)
13. **PerformanceMetric** (dashboard tracking)

**Reference**: See `/docs/LEGACY_SIWEA_COMPLETE_ANALYSIS.md` for complete entity field mappings.

## New Data Transfer Objects (DTOs)

### 1. ExternalValidationRequest

**Purpose**: Payload sent to CNOUA, SIPUA, SIMDA validation services

**Fields**:
- `Fonte` (int): Claim source identifier
- `Protsini` (int): Protocol number
- `Dac` (int): DAC verification digit
- `Orgsin` (int): Origin code
- `Rmosin` (int): Branch code
- `Numsin` (int): Claim number
- `CodProdu` (int): Product code (6814, 7701, 7709 for consortium)
- `NumContrato` (int?): Contract number (nullable, from EF_CONTR_SEG_HABIT lookup)
- `TipoPagamento` (int): Payment type (1-5)
- `ValorPrincipal` (decimal): Principal amount in original currency
- `ValorCorrecao` (decimal): Correction/interest amount
- `Beneficiario` (string?): Beneficiary name (max 255 chars, nullable)
- `OperatorId` (string): EZEUSRID operator making request

**Validation Rules**:
- Required: Fonte, Protsini, Dac, Orgsin, Rmosin, Numsin, CodProdu, TipoPagamento, ValorPrincipal, OperatorId
- Range: TipoPagamento ∈ {1, 2, 3, 4, 5}
- Range: ValorPrincipal > 0
- Range: ValorCorrecao >= 0
- Length: Beneficiario <= 255 characters

### 2. ExternalValidationResponse

**Purpose**: Response received from CNOUA, SIPUA, SIMDA validation services

**Fields**:
- `Ezert8` (string): Response code ('00000000' = success, others = error)
- `ErrorMessage` (string?): Descriptive error message in Portuguese (if Ezert8 != '00000000')
- `IsSuccess` (bool): Computed property: Ezert8 == '00000000'
- `ValidationService` (string): Service name ('CNOUA', 'SIPUA', 'SIMDA')
- `RequestTimestamp` (DateTime): When request was sent
- `ResponseTimestamp` (DateTime): When response received
- `ElapsedMilliseconds` (long): Response time measurement

**Validation Rules**:
- Required: Ezert8, ValidationService, RequestTimestamp, ResponseTimestamp
- Ezert8 must be 8-character string
- If IsSuccess = false, ErrorMessage must not be null

**Error Code Mapping**:
```
Ezert8 Code → Portuguese Error Message (from ErrorMessages.pt-BR)
------------------------------------------------------------------
'00000000'  → Success (no error)
'EZERT8001' → CONS-001: "Contrato de consórcio inválido"
'EZERT8002' → CONS-002: "Contrato cancelado"
'EZERT8003' → CONS-003: "Grupo encerrado"
'EZERT8004' → CONS-004: "Cota não contemplada"
'EZERT8005' → CONS-005: "Beneficiário não autorizado"
Other       → SYS-005: "Serviço de validação indisponível"
```

### 3. TransactionContext

**Purpose**: Context object passed through 4-step payment authorization transaction for atomicity tracking

**Fields**:
- `AuthorizationId` (Guid): Unique identifier for this authorization attempt
- `ClaimKey` (ClaimKey): Composite key identifying claim (Tipseg, Orgsin, Rmosin, Numsin)
- `OperatorId` (string): EZEUSRID operator executing transaction
- `TransactionDate` (DateTime): DTMOVABE business date from TSISTEMA
- `TransactionTime` (TimeSpan): HORAOPER system time
- `OperationCode` (int): Always 1098 for payment authorization
- `CorrectionType` (string): Always '5' for standard correction
- `Step` (TransactionStep): Current step in pipeline (enum: History, ClaimMaster, Accompaniment, Phases, Committed)
- `RollbackReason` (string?): If rollback occurred, which step failed

**Validation Rules**:
- Required: AuthorizationId, ClaimKey, OperatorId, TransactionDate, TransactionTime, OperationCode, CorrectionType, Step
- OperationCode must equal 1098 (constant)
- CorrectionType must equal "5" (constant)
- Step must progress: History → ClaimMaster → Accompaniment → Phases → Committed (no backwards)
- If Step != Committed, RollbackReason must be null

**Transaction Step Enum**:
```csharp
public enum TransactionStep {
  History = 1,        // Insert THISTSIN
  ClaimMaster = 2,    // Update TMESTSIN
  Accompaniment = 3,  // Insert SI_ACOMPANHA_SINI
  Phases = 4,         // Update SI_SINISTRO_FASE
  Committed = 5       // Transaction committed successfully
}
```

### 4. BusinessRuleViolation

**Purpose**: Represents a single business rule validation failure

**Fields**:
- `RuleId` (string): Business rule identifier (e.g., "BR-019")
- `RuleName` (string): Human-readable rule name (e.g., "Beneficiary Required")
- `ErrorCode` (string): Error code from ErrorMessages.pt-BR (e.g., "VAL-007")
- `ErrorMessage` (string): Portuguese error message
- `FailedValue` (object?): Actual value that violated rule (for diagnostics)
- `ExpectedValue` (object?): Expected value or condition (for diagnostics)
- `Severity` (Severity): Critical (blocks transaction) vs Warning (logged only)

**Validation Rules**:
- Required: RuleId, RuleName, ErrorCode, ErrorMessage, Severity
- RuleId must match pattern "BR-\d{3}" (BR-001 through BR-099)
- ErrorCode must match pattern "VAL-\d{3}" or "CONS-\d{3}" or "SYS-\d{3}"

**Severity Enum**:
```csharp
public enum Severity {
  Critical = 1,  // Blocks transaction, must be resolved
  Warning = 2    // Logged but doesn't block (for audit trail)
}
```

### 5. PhaseUpdateRequest

**Purpose**: Request to open or close claim phases based on event

**Fields**:
- `Fonte` (int): Claim source
- `Protsini` (int): Protocol number
- `Dac` (int): DAC digit
- `CodEvento` (int): Event code (1098 for payment authorization)
- `CodFase` (int): Phase code to open or close
- `IndicadorAlteracao` (string): '1' = open phase, '2' = close phase
- `DataInivig` (DateTime): Phase effective start date
- `DataFechaeSifa` (DateTime): Phase closing date ('9999-12-31' if open)
- `NumOcorrencia` (int): Occurrence sequence within phase
- `OperatorId` (string): EZEUSRID operator

**Validation Rules**:
- Required: All fields
- IndicadorAlteracao ∈ {'1', '2'}
- If IndicadorAlteracao = '1', DataFechaeSifa must equal DateTime.Parse("9999-12-31") (open phase marker)
- If IndicadorAlteracao = '2', DataFechaeSifa must equal current DTMOVABE (closing today)
- CodEvento must equal 1098 (payment authorization event)

**Phase States**:
```
Open Phase:   DATA_FECHA_SIFA = '9999-12-31'
Closed Phase: DATA_FECHA_SIFA < '9999-12-31' (actual closing date)

Example for Event 1098 (from SI_REL_FASE_EVENTO):
- Phase 10 (Payment Processing):  IND_ALTERACAO_FASE = '1' → OPENS
- Phase 5 (Pending Documentation): IND_ALTERACAO_FASE = '2' → CLOSES
```

## Value Objects

### ClaimKey (Composite Key)

**Purpose**: Immutable value object representing claim composite primary key

**Fields**:
- `Tipseg` (int): Insurance type
- `Orgsin` (int): Origin code (2 digits, 01-99)
- `Rmosin` (int): Branch code (2 digits, 00-99)
- `Numsin` (int): Claim number (1-6 digits)

**Methods**:
- `ToString()`: Returns formatted "ORGSIN/RMOSIN/NUMSIN" (e.g., "01/05/123456")
- `Equals(ClaimKey other)`: Value-based equality (all 4 fields match)
- `GetHashCode()`: Hash based on all 4 fields

**Validation Rules**:
- Tipseg >= 0
- Orgsin ∈ [1, 99]
- Rmosin ∈ [0, 99]
- Numsin ∈ [1, 999999]

### CurrencyAmount (Money Pattern)

**Purpose**: Immutable value object representing monetary amount with currency and precision

**Fields**:
- `Amount` (decimal): Monetary value
- `Currency` (string): Currency code (always "BTNF" for standardized amounts)
- `Precision` (int): Decimal places (2 for display amounts, 8 for conversion rates)

**Methods**:
- `Multiply(decimal rate)`: Returns new CurrencyAmount with amount × rate
- `Add(CurrencyAmount other)`: Returns new CurrencyAmount with sum (validates same currency)
- `ToString()`: Returns formatted "###,###,###.##" with thousands separator

**Validation Rules**:
- Amount >= 0
- Currency must equal "BTNF" for all converted amounts
- Precision ∈ {2, 8} (2 for final amounts, 8 for conversion rates)

**Usage Example**:
```csharp
var vlcruzad = new CurrencyAmount(1.23456789m, "BTNF", 8);  // Conversion rate
var valpri = new CurrencyAmount(1000.00m, "BRL", 2);         // Original amount
var valpribt = valpri.Multiply(vlcruzad.Amount)
                     .WithCurrency("BTNF")
                     .WithPrecision(2);  // Result: 1234.57 BTNF
```

## API Request/Response Models

### PaymentAuthorizationRequest (Enhanced)

**Purpose**: Request DTO for payment authorization endpoint (existing, needs enhancement)

**New Fields to Add**:
- `ExternalValidationRequired` (bool): Whether to call CNOUA/SIPUA/SIMDA (based on product code)
- `RoutingService` (string?): Which service to route to ('CNOUA', 'SIPUA', 'SIMDA', null for skip)

**Existing Fields** (preserved):
- ClaimId, TipoPagamento, ValorPrincipal, ValorCorrecao, TipoApolice, Beneficiario, CurrencyCode, TargetCurrencyCode

### PaymentAuthorizationResponse (Enhanced)

**Purpose**: Response DTO for payment authorization endpoint (existing, needs enhancement)

**New Fields to Add**:
- `TransactionId` (Guid): Links to TransactionContext.AuthorizationId for audit trail
- `ValidationResults` (List<ExternalValidationResponse>): Results from CNOUA/SIPUA/SIMDA calls
- `RollbackOccurred` (bool): Whether transaction rolled back
- `FailedStep` (TransactionStep?): If rollback, which step failed

**Existing Fields** (preserved):
- AuthorizationId, Sucesso, Mensagem, Erros

## State Transitions

### Transaction Pipeline State Machine

```
[START]
  ↓
[Validate Request] → If invalid → [Return ValidationErrors]
  ↓ (valid)
[External Validation] → If fails → [Return ExternalValidationError]
  ↓ (success EZERT8='00000000')
[Begin Transaction]
  ↓
[Step 1: Insert THISTSIN] → If fails → [Rollback] → [Return TransactionError]
  ↓ (success)
[Step 2: Update TMESTSIN] → If fails → [Rollback] → [Return TransactionError]
  ↓ (success)
[Step 3: Insert SI_ACOMPANHA_SINI] → If fails → [Rollback] → [Return TransactionError]
  ↓ (success)
[Step 4: Update SI_SINISTRO_FASE] → If fails → [Rollback] → [Return TransactionError]
  ↓ (success)
[Commit Transaction]
  ↓
[Return Success Response]
```

### Phase State Transitions

```
Phase Opening (IND_ALTERACAO_FASE='1'):
[Check Phase NOT Already Open] → If open → [Skip]
  ↓ (not open)
[Create SI_SINISTRO_FASE Record]
  - DATA_INIVIG_SIFA = DTMOVABE (current business date)
  - DATA_FECHA_SIFA = '9999-12-31' (open marker)
  - NUM_OCORR = 1 (first occurrence)
  ↓
[Phase Now OPEN]

Phase Closing (IND_ALTERACAO_FASE='2'):
[Find Open Phase WHERE DATA_FECHA_SIFA='9999-12-31'] → If not found → [Skip]
  ↓ (found)
[Update SI_SINISTRO_FASE Record]
  - DATA_FECHA_SIFA = DTMOVABE (close today)
  ↓
[Phase Now CLOSED]
```

## Relationships (New DTOs Only)

```
ExternalValidationRequest
  → sent to → CNOUA/SIPUA/SIMDA services
  ← receives ← ExternalValidationResponse

TransactionContext
  → contains → ClaimKey (identifies which claim)
  → contains → TransactionStep (current pipeline position)
  → produces → BusinessRuleViolation[] (if validation fails)

PhaseUpdateRequest
  → creates/updates → SI_SINISTRO_FASE entity
  → references → SI_REL_FASE_EVENTO entity (config lookup)

PaymentAuthorizationRequest
  → produces → TransactionContext
  → produces → ExternalValidationRequest
  ← receives ← PaymentAuthorizationResponse
    → contains → ExternalValidationResponse[]
    → contains → BusinessRuleViolation[]
```

## Validation Summary

| DTO | Validator Class | Rules Enforced | Error Codes |
|-----|----------------|----------------|-------------|
| ExternalValidationRequest | ExternalValidationRequestValidator | Required fields, range checks, length limits | VAL-004, VAL-007 |
| ExternalValidationResponse | ExternalValidationResponseValidator | Ezert8 format, success/error consistency | SYS-005, CONS-001-005 |
| TransactionContext | TransactionContextValidator | Operation code 1098, correction type '5', step progression | SYS-002, SYS-003, SYS-004 |
| BusinessRuleViolation | BusinessRuleViolationValidator | Rule ID format, error code format | N/A (validation object itself) |
| PhaseUpdateRequest | PhaseUpdateRequestValidator | Indicator values, date logic, event code | SYS-004 |
| ClaimKey | ClaimKeyValidator | Range checks on all 4 components | VAL-002, VAL-003 |
| CurrencyAmount | CurrencyAmountValidator | Non-negative, currency BTNF, precision 2 or 8 | VAL-008 |

## Indexes Required (Database)

No new entities, but recommended indexes for existing entities to meet performance targets:

1. **THISTSIN (ClaimHistory)**:
   - Covering index on (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC) for history queries < 500ms

2. **TGEUNIMO (CurrencyUnit)**:
   - Index on (DTINIVIG, DTTERVIG) for date range lookups (currency conversion)

3. **SI_SINISTRO_FASE (ClaimPhase)**:
   - Index on (FONTE, PROTSINI, DAC, COD_FASE, DATA_FECHA_SIFA) for open phase queries (WHERE DATA_FECHA_SIFA='9999-12-31')

4. **EF_CONTR_SEG_HABIT (ConsortiumContract)**:
   - Index on (ORGSIN, RMOSIN, NUMSIN) for contract lookup by claim number

## Data Model Summary

**New DTOs**: 7 (ExternalValidationRequest/Response, TransactionContext, BusinessRuleViolation, PhaseUpdateRequest, ClaimKey, CurrencyAmount)

**Enhanced DTOs**: 2 (PaymentAuthorizationRequest/Response with new fields)

**Existing Entities**: 13 (no changes, all already implemented with EF Core configs)

**Validators**: 7 FluentValidation classes for new DTOs

**State Machines**: 2 (Transaction Pipeline, Phase Transitions)

**Performance Indexes**: 4 recommended covering indexes on existing tables

---

**Data Model Status**: COMPLETE
**Next Step**: Phase 1 - Generate API contracts
**Dependencies**: None (all existing entities already implemented)
