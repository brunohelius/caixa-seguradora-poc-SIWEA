# SIWEA Legacy System - Complete Business Rules and Functionality Analysis

**Project**: Visual Age to .NET 9 Migration (Claims Indemnity Payment Authorization System)  
**Legacy System**: IBM VisualAge EZEE 4.40  
**Program Name**: SIWEA-V116  
**Original Development**: October 1989 (COSMO - Analyst, SOLANGE - Programmer)  
**Last Revision**: CAD73898 - February 11, 2014 (Version 90)  
**Analysis Date**: 2025-10-23  
**Purpose**: Complete reference document for implementing SIWEA functionality in .NET 9 + React

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Database Schema (13 Entities)](#database-schema)
3. [Search Functionality (User Story 1)](#search-functionality)
4. [Payment Authorization (User Story 2)](#payment-authorization)
5. [All 42+ Business Rules](#business-rules)
6. [External Service Integrations](#external-integrations)
7. [Phase & Workflow Management](#phase-workflow)
8. [User Interface Screens](#ui-screens)
9. [Transaction Processing](#transaction-processing)
10. [Error Handling & Messages](#error-handling)
11. [Audit & Compliance](#audit-compliance)
12. [Performance Requirements](#performance-requirements)

---

## System Overview

### Purpose
SIWEA (Sistema de Liberacao de Pagamento de Sinistros) - Claims Indemnity Payment Authorization System

Enables insurance operators to:
- Search existing insurance claims using multiple criteria
- Authorize payment for claims
- View payment history and audit trails
- Process consortium and standard insurance products
- Track claim workflow phases

### Core Business Function
When a claim is processed for payment authorization:
1. Operator searches for claim using protocol, claim number, or leader code
2. System retrieves claim details from master table (TMESTSIN)
3. Operator enters payment authorization details
4. System validates payment against pending balance
5. System routes validation to appropriate external service (CNOUA/SIPUA/SIMDA)
6. System creates history record (THISTSIN) with transaction details
7. System updates claim master with new payment total
8. System manages claim phases through event-driven workflow
9. System creates audit trail (SI_ACOMPANHA_SINI) and phases (SI_SINISTRO_FASE)

### System Principles
- **Portuguese Language**: All messages, field names, and UI in Portuguese
- **Data Integrity**: Transaction atomicity with rollback on validation failure
- **Audit Trail**: Complete operator tracking on all transactions
- **Legacy Preservation**: All business rules and calculations maintained exactly
- **Decimal Precision**: Currency calculations accurate to 2 decimal places
- **Standardized Currency**: All amounts converted to BTNF (Bônus do Tesouro Nacional Fiscal)

---

## Database Schema

### 13 Database Entities

#### LEGACY ENTITIES (10)

##### 1. TMESTSIN - Claim Master Record
**Purpose**: Main claim record with protocol identification, financial summary, policy references

**Primary Key**: (TIPSEG, ORGSIN, RMOSIN, NUMSIN) - 4-part composite key

**Field Mapping**:
```
TIPSEG          INT         Insurance Type (PK Part 1)
ORGSIN          INT         Origin/Branch (PK Part 2)
RMOSIN          INT         Claim Branch (PK Part 3)
NUMSIN          INT         Claim Number (PK Part 4)
FONTE           INT         Protocol Source (non-null)
PROTSINI        INT         Protocol Number (non-null)
DAC             INT         Check Digit (non-null)
ORGAPO          INT         Policy Origin (non-null)
RMOAPO          INT         Policy Branch (non-null)
NUMAPOL         INT         Policy Number (non-null)
CODPRODU        INT         Product Code (non-null)
SDOPAG      DECIMAL(15,2)   Saldo a Pagar - Expected Reserve Amount
TOTPAG      DECIMAL(15,2)   Total Pago - Total Payments Made
CODLIDER        INT         Leader Code (optional - reinsurance)
SINLID          INT         Leader Claim Number (optional)
OCORHIST        INT         Occurrence Counter - tracks history records
TIPREG          CHAR(1)     Policy Type Indicator ('1' or '2')
TPSEGU          INT         Insurance Type from policy (0=optional beneficiary, !=0=required)
CREATED_BY      VARCHAR(50) Audit - created by user ID
CREATED_AT      DATETIME    Audit - creation timestamp
UPDATED_BY      VARCHAR(50) Audit - last modified by user ID
UPDATED_AT      DATETIME    Audit - last modification timestamp
ROW_VERSION     BINARY      Concurrency token for optimistic locking
```

**Indexes**:
- IX_TMESTSIN_Protocol: (FONTE, PROTSINI, DAC)
- IX_TMESTSIN_Leader: (CODLIDER, SINLID)
- IX_TMESTSIN_Policy: (ORGAPO, RMOAPO, NUMAPOL)

**Relationships**:
- 1:N with THISTSIN (claim has many payment history records)
- 1:N with SI_ACOMPANHA_SINI (claim has many accompaniment/event records)
- 1:N with SI_SINISTRO_FASE (claim has many phase records)
- M:1 with TGERAMO (belongs to branch)
- M:1 with TAPOLICE (references policy)

**Key Formulas**:
```
PENDING_VALUE = SDOPAG - TOTPAG
```

**Critical Rules**:
- SDOPAG (expected reserve) must be >= 0
- TOTPAG (total payments) must be >= 0
- SDOPAG >= TOTPAG always (logical requirement)
- OCORHIST incremented by 1 with each payment authorization
- TPSEGU determines if beneficiary is required during payment
- TIPREG must be '1' or '2' (policy type indicator)

---

##### 2. THISTSIN - Payment History Record
**Purpose**: Individual payment authorization transaction record

**Primary Key**: (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST) - 5-part composite key

**Field Mapping**:
```
TIPSEG          INT         Insurance Type (PK Part 1)
ORGSIN          INT         Claim Origin (PK Part 2)
RMOSIN          INT         Claim Branch (PK Part 3)
NUMSIN          INT         Claim Number (PK Part 4)
OCORHIST        INT         Occurrence Counter (PK Part 5) - sequence number
OPERACAO        INT         Operation Code (always 1098 for payment authorization)
DTMOVTO         DATE        Transaction Date (YYYY-MM-DD)
HORAOPER        TIME        Transaction Time (HH:MM:SS)
VALPRI      DECIMAL(15,2)   Principal Amount in original currency
CRRMON      DECIMAL(15,2)   Correction Amount in original currency
NOMFAV          VARCHAR     Beneficiary Name
TIPCRR          CHAR(1)     Correction Type (always '5' for payment authorization)
VALPRIBT    DECIMAL(15,2)   Principal Amount in BTNF (standardized currency)
CRRMONBT    DECIMAL(15,2)   Correction Amount in BTNF
VALPRI_CRRMON_BT DECIMAL   VALPRI + CRRMON in original currency
VALTOTBT    DECIMAL(15,2)   Total in BTNF (VALPRIBT + CRRMONBT)
PRIDIABT    DECIMAL(15,2)   Principal daily amount in BTNF
CRRDIABT    DECIMAL(15,2)   Correction daily amount in BTNF
TOTDIABT    DECIMAL(15,2)   Total daily amount in BTNF
SITCONTB        CHAR(1)     Accounting Status (initialized as '0')
SITUACAO        CHAR(1)     Overall Status (initialized as '0')
EZEUSRID        VARCHAR(50) Operator User ID (audit trail)
CREATED_BY      VARCHAR(50) Audit - created by user ID
CREATED_AT      DATETIME    Audit - creation timestamp
UPDATED_BY      VARCHAR(50) Audit - last modified by
UPDATED_AT      DATETIME    Audit - last modification timestamp
ROW_VERSION     BINARY      Concurrency token
```

**Indexes**:
- IX_THISTSIN_Claim_Occurrence: (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC) - COVERING INDEX
  - Includes: (OPERACAO, DTMOVTO, HORAOPER, VALPRI, CRRMON, NOMFAV, TIPCRR, VALPRIBT, CRRMONBT, VALTOTBT, SITCONTB, SITUACAO, EZEUSRID)

**Relationships**:
- M:1 with TMESTSIN (payment belongs to claim)
- M:1 with TGEUNIMO (uses currency conversion rate)

**Key Formulas**:
```
VALPRIBT = VALPRI × VLCRUZAD
CRRMONBT = CRRMON × VLCRUZAD
VALTOTBT = VALPRIBT + CRRMONBT
```

Where VLCRUZAD is the currency conversion rate from TGEUNIMO table

**Critical Rules**:
- OPERACAO is always 1098 (fixed - payment authorization operation code)
- TIPCRR is always '5' (fixed - standard correction type)
- SITCONTB initialized as '0' (accounting status)
- SITUACAO initialized as '0' (overall status)
- DTMOVTO must be current system date from TSISTEMA
- HORAOPER must be current system time
- VALPRI >= 0 (principal amount is non-negative)
- CRRMON >= 0 (correction amount is non-negative, but typically 0)
- VALTOTBT must equal VALPRIBT + CRRMONBT
- NOMFAV required if TPSEGU != 0 (beneficiary required for certain insurance types)

---

##### 3. TGERAMO - Branch Master
**Purpose**: Branch descriptive information lookup

**Primary Key**: RMOSIN (INT)

**Field Mapping**:
```
RMOSIN          INT         Branch Code (PK)
NOMERAMO        VARCHAR     Branch Name
CREATED_BY      VARCHAR(50) Audit fields
CREATED_AT      DATETIME
UPDATED_BY      VARCHAR(50)
UPDATED_AT      DATETIME
```

**Relationships**:
- 1:N with TMESTSIN (branch has many claims)
- 1:N with TAPOLICE (branch has many policies)

---

##### 4. TAPOLICE - Policy Master
**Purpose**: Insured party information

**Primary Key**: (ORGAPO, RMOAPO, NUMAPOL) - 3-part composite key

**Field Mapping**:
```
ORGAPO          INT         Policy Origin (PK Part 1)
RMOAPO          INT         Policy Branch (PK Part 2)
NUMAPOL         INT         Policy Number (PK Part 3)
NOME            VARCHAR     Insured Name
CREATED_BY      VARCHAR(50) Audit fields
CREATED_AT      DATETIME
UPDATED_BY      VARCHAR(50)
UPDATED_AT      DATETIME
```

**Relationships**:
- M:1 with TGERAMO (policy belongs to branch)
- 1:N with TMESTSIN (policy has many claims)

---

##### 5. TGEUNIMO - Currency Unit Table
**Purpose**: Currency conversion rates with validity periods

**Primary Key**: (DTINIVIG, DTTERVIG) - date range composite key

**Field Mapping**:
```
DTINIVIG        DATE        Start Date of Validity (PK Part 1)
DTTERVIG        DATE        End Date of Validity (PK Part 2)
VLCRUZAD    DECIMAL(18,8)   Conversion Rate from working currency to BTNF
CREATED_BY      VARCHAR(50) Audit fields
CREATED_AT      DATETIME
UPDATED_BY      VARCHAR(50)
UPDATED_AT      DATETIME
```

**Critical Rules**:
- Must have valid rate for any date when processing payment
- Rates are maintained externally (not modified by SIWEA)
- Query must find rate where: DTINIVIG <= TRANSACTION_DATE <= DTTERVIG
- If no valid rate found for transaction date, payment authorization fails
- VLCRUZAD precision: 8 decimal places (critical for financial accuracy)

**Error Condition**:
- If no rate found: Display error "Taxa de conversão não disponível para a data do movimento"

---

##### 6. TSISTEMA - System Control
**Purpose**: Current business date for claims system

**Primary Key**: IDSISTEM (VARCHAR)

**Field Mapping**:
```
IDSISTEM        VARCHAR(10) System ID - always 'SI' for SIWEA
DTMOVABE        DATE        Current Business Date (YYYY-MM-DD)
CREATED_BY      VARCHAR(50) Audit fields
CREATED_AT      DATETIME
UPDATED_BY      VARCHAR(50)
UPDATED_AT      DATETIME
```

**Critical Rules**:
- Only one record with IDSISTEM='SI' exists
- DTMOVABE is the authoritative business date for all transactions
- All payment transactions use DTMOVTO = TSISTEMA.DTMOVABE (not system clock)
- Maintained by external process (bank operations)
- Must be consulted at payment authorization time

---

##### 7. SI_ACOMPANHA_SINI - Claim Accompaniment (Event History)
**Purpose**: Tracks claim workflow events for audit trail

**Primary Key**: (FONTE, PROTSINI, DAC, COD_EVENTO, DATA_MOVTO_SINIACO) - 5-part composite key

**Field Mapping**:
```
FONTE           INT         Protocol Source (PK Part 1)
PROTSINI        INT         Protocol Number (PK Part 2)
DAC             INT         Check Digit (PK Part 3)
COD_EVENTO      INT         Event Code (PK Part 4) - event type
DATA_MOVTO_SINIACO DATE     Transaction Date (PK Part 5) - YYYY-MM-DD
NUM_OCORR_SINIACO INT       Occurrence Number/Sequence
DESCR_COMPLEMENTAR VARCHAR  Complementary Description
COD_USUARIO     VARCHAR(50) User ID who created event
CREATED_BY      VARCHAR(50) Audit fields
CREATED_AT      DATETIME
UPDATED_BY      VARCHAR(50)
UPDATED_AT      DATETIME
```

**Relationships**:
- M:1 with TMESTSIN (accompaniment belongs to claim)
- M:1 with SI_REL_FASE_EVENTO (event may trigger phase changes)

**Event Codes** (Standard):
```
1098  = Payment Authorization (payment operation code)
2001  = Document Submission
2002  = Document Approval
3001  = External Validation Started
3002  = External Validation Completed
9001  = Claim Finalization
(others as defined in SI_REL_FASE_EVENTO)
```

**Critical Rules**:
- Created whenever payment is authorized (COD_EVENTO = 1098)
- Records exact operator who performed authorization (COD_USUARIO = EZEUSRID)
- Records exact date of transaction (DATA_MOVTO_SINIACO = business date from TSISTEMA)
- NUM_OCORR_SINIACO incremented for each event on same claim same date
- DESCR_COMPLEMENTAR should contain human-readable event description
- Used for complete audit trail reconstruction

---

##### 8. SI_SINISTRO_FASE - Claim Phase
**Purpose**: Tracks claim processing phases

**Primary Key**: (FONTE, PROTSINI, DAC, COD_FASE, COD_EVENTO, NUM_OCORR_SINIACO, DATA_INIVIG_REFAEV) - 7-part composite key

**Field Mapping**:
```
FONTE           INT         Protocol Source (PK Part 1)
PROTSINI        INT         Protocol Number (PK Part 2)
DAC             INT         Check Digit (PK Part 3)
COD_FASE        INT         Phase Code (PK Part 4)
COD_EVENTO      INT         Event Code (PK Part 5)
NUM_OCORR_SINIACO INT       Event Occurrence (PK Part 6)
DATA_INIVIG_REFAEV DATE     Phase Effective Date (PK Part 7)
DATA_ABERTURA_SIFA DATE     Phase Opening Date
DATA_FECHA_SIFA DATE        Phase Closing Date ('9999-12-31' = OPEN)
CREATED_BY      VARCHAR(50) Audit fields
CREATED_AT      DATETIME
UPDATED_BY      VARCHAR(50)
UPDATED_AT      DATETIME
ROW_VERSION     BINARY      Concurrency token
```

**Relationships**:
- M:1 with TMESTSIN (phase belongs to claim)
- M:1 with SI_REL_FASE_EVENTO (phase configuration)

**Phase State Indicators**:
```
IF DATA_FECHA_SIFA = '9999-12-31'  THEN Phase is OPEN
IF DATA_FECHA_SIFA < '9999-12-31'  THEN Phase is CLOSED
```

**Critical Rules**:
- Phase opening (IND_ALTERACAO_FASE='1'): Create record with DATA_FECHA_SIFA='9999-12-31'
- Phase closing (IND_ALTERACAO_FASE='2'): Update existing open phase with current date
- Must prevent duplicate open phases for same claim/phase/event combination
- Opening date always set to current business date
- Closing date must equal or be after opening date
- '9999-12-31' is sentinel value meaning "still open"

**Computed Properties**:
```
IsOpen = (DATA_FECHA_SIFA == '9999-12-31')
DaysOpen = IsOpen ? (TODAY - DATA_ABERTURA_SIFA).Days : (DATA_FECHA_SIFA - DATA_ABERTURA_SIFA).Days
Status = IsOpen ? "Aberta" : "Fechada"
```

---

##### 9. SI_REL_FASE_EVENTO - Phase-Event Relationship (Configuration)
**Purpose**: Defines which phases are affected by which events

**Primary Key**: (COD_FASE, COD_EVENTO, DATA_INIVIG_REFAEV) - 3-part composite key

**Field Mapping**:
```
COD_FASE        INT         Phase Code (PK Part 1)
COD_EVENTO      INT         Event Code (PK Part 2)
DATA_INIVIG_REFAEV DATE     Effective Date of Relationship (PK Part 3)
IND_ALTERACAO_FASE CHAR(1)  Phase Change Indicator ('1'=open, '2'=close)
CREATED_BY      VARCHAR(50) Audit fields
CREATED_AT      DATETIME
UPDATED_BY      VARCHAR(50)
UPDATED_AT      DATETIME
```

**Critical Rules**:
- Configuration table - rarely changes
- Multiple relationships can exist for single event
- Single event can open multiple phases or close multiple phases
- IND_ALTERACAO_FASE values:
  - '1' = ABERTURA (open phase)
  - '2' = FECHAMENTO (close phase)
- DATA_INIVIG_REFAEV allows date-range based configurations
- Must be queried when processing event (e.g., event 1098 = payment authorization)

**Example Configuration for Event 1098 (Payment Authorization)**:
```
Phase 10 (Payment Processing) opens    - IND_ALTERACAO_FASE='1'
Phase 5 (Pending Documentation) closes - IND_ALTERACAO_FASE='2'
```

---

##### 10. EF_CONTR_SEG_HABIT - Consortium Contract
**Purpose**: Consortium-specific contract information for product routing

**Primary Key**: (NUM_CONTRATO) or composite key with policy references

**Field Mapping**:
```
NUM_CONTRATO    INT         Contract Number (PK or part of key)
(additional fields depend on actual legacy schema)
CREATED_BY      VARCHAR(50) Audit fields
CREATED_AT      DATETIME
UPDATED_BY      VARCHAR(50)
UPDATED_AT      DATETIME
```

**Critical Rules**:
- Used to determine payment routing for non-consortium products
- Query: Does policy have record in EF_CONTR_SEG_HABIT with NUM_CONTRATO > 0?
- If NUM_CONTRATO > 0: Route to SIPUA (EFP contract validation)
- If NUM_CONTRATO = 0 or NOT FOUND: Route to SIMDA (HB contract validation)

---

#### DASHBOARD ENTITIES (3) - New Tables for Migration Tracking

##### 11. MIGRATION_STATUS - Project Progress Tracking
**Purpose**: Track overall migration project status

**Primary Key**: ID (GUID)

**Field Mapping**:
```
ID                      GUID            Primary Key
USER_STORY_CODE         VARCHAR(10)     UK - Unique identifier (US1, US2, etc.)
USER_STORY_NAME         VARCHAR(255)    Name of user story
STATUS                  VARCHAR(50)     "Not Started", "In Progress", "Completed", "Blocked"
COMPLETION_PERCENTAGE   DECIMAL(5,2)    0-100 percentage
REQUIREMENTS_COMPLETED  INT             Count of functional requirements completed
REQUIREMENTS_TOTAL      INT             Total functional requirements
TESTS_PASSED            INT             Count of tests passed
TESTS_TOTAL             INT             Total tests
ASSIGNED_TO             VARCHAR(100)    Team member responsible
START_DATE              DATETIME        When story started
ESTIMATED_COMPLETION    DATETIME        Planned completion date
ACTUAL_COMPLETION       DATETIME        Actual completion date (NULL if not done)
BLOCKING_ISSUES         TEXT            Description of any blocking issues
CREATED_BY              VARCHAR(50)     Audit fields
CREATED_AT              DATETIME
UPDATED_BY              VARCHAR(50)
UPDATED_AT              DATETIME
```

---

##### 12. COMPONENT_MIGRATION - Component-Level Tracking
**Purpose**: Track individual component migration status

**Primary Key**: ID (GUID)

**Field Mapping**:
```
ID                      GUID            Primary Key
MIGRATION_STATUS_ID     GUID            FK to MIGRATION_STATUS
COMPONENT_TYPE          VARCHAR(50)     "screen", "business_rule", "database_entity", "external_service"
COMPONENT_NAME          VARCHAR(255)    Name of component
LEGACY_REFERENCE        VARCHAR(255)    Reference in legacy system (e.g., function name)
STATUS                  VARCHAR(50)     "Not Started", "In Progress", "Completed", "Blocked"
ESTIMATED_HOURS         DECIMAL(10,2)   Estimated effort
ACTUAL_HOURS            DECIMAL(10,2)   Actual effort spent
COMPLEXITY              VARCHAR(20)     "Low", "Medium", "High"
ASSIGNED_DEVELOPER      VARCHAR(100)    Developer responsible
TECHNICAL_NOTES         TEXT            Implementation notes
CREATED_BY              VARCHAR(50)     Audit fields
CREATED_AT              DATETIME
UPDATED_BY              VARCHAR(50)
UPDATED_AT              DATETIME
```

---

##### 13. PERFORMANCE_METRICS - Benchmarking Data
**Purpose**: Store performance comparison metrics

**Primary Key**: ID (GUID)

**Field Mapping**:
```
ID                      GUID            Primary Key
COMPONENT_ID            GUID            FK to COMPONENT_MIGRATION
METRIC_TYPE             VARCHAR(50)     "response_time", "throughput", "concurrent_users", "memory_usage", "error_rate"
LEGACY_VALUE            DECIMAL(18,4)   Performance metric from legacy system
NEW_VALUE               DECIMAL(18,4)   Performance metric from new system
MEASUREMENT_TIMESTAMP   DATETIME        When measurement was taken
TEST_SCENARIO           VARCHAR(255)    Description of test scenario
PASS_FAIL               BOOLEAN         Whether metric meets acceptance criteria
CREATED_BY              VARCHAR(50)     Audit fields
CREATED_AT              DATETIME
UPDATED_BY              VARCHAR(50)
UPDATED_AT              DATETIME
```

---

## Search Functionality (User Story 1)

### Three Mutually Exclusive Search Modes

#### Mode 1: Protocol Number Search
**Input**: FONTE + PROTSINI + DAC  
**Format**: "001/0123456-7" (displayed format: FONTE/PROTSINI-DAC)

**SQL Query**:
```sql
SELECT * FROM TMESTSIN
WHERE FONTE = @fonte
  AND PROTSINI = @protsini
  AND DAC = @dac
```

**Validation**:
- FONTE must be numeric, valid range
- PROTSINI must be numeric, valid range
- DAC must be numeric, single digit (0-9)
- All three fields must be provided together
- At least one of the three search modes must be complete

**Error**: "PROTOCOLO XXXXXXX-X NAO ENCONTRADO" (Protocol not found)

---

#### Mode 2: Claim Number Search
**Input**: ORGSIN + RMOSIN + NUMSIN  
**Format**: "10/20/789012" (displayed format: ORGSIN/RMOSIN/NUMSIN)

**SQL Query**:
```sql
SELECT * FROM TMESTSIN
WHERE ORGSIN = @orgsin
  AND RMOSIN = @rmosin
  AND NUMSIN = @numsin
```

**Validation**:
- ORGSIN must be numeric, 2 digits (01-99)
- RMOSIN must be numeric, 2 digits (00-99)
- NUMSIN must be numeric, up to 6 digits (000001-999999)
- All three fields must be provided together

**Error**: "SINISTRO XXXXXXX NAO ENCONTRADO" (Claim not found)

---

#### Mode 3: Leader Code Search
**Input**: CODLIDER + SINLID  
**Format**: "001/0000001" (displayed format: CODLIDER/SINLID)

**SQL Query**:
```sql
SELECT * FROM TMESTSIN
WHERE CODLIDER = @codlider
  AND SINLID = @sinlid
```

**Validation**:
- CODLIDER must be numeric, 3 digits (001-999)
- SINLID must be numeric, up to 7 digits (0000001-9999999)
- Both fields must be provided together
- This mode identifies reinsurance/leader claims

**Error**: "LIDER XXXXXXX-XXXXXXX NAO ENCONTRADO" (Leader claim not found)

---

### Display of Claim Details After Search

Once claim is found via any search mode, display:

**Protocol Information**:
```
Protocolo: {FONTE}/{PROTSINI}-{DAC}
Sinistro:  {ORGSIN}/{RMOSIN}/{NUMSIN}
```

**Policy Information**:
```
Apólice:      {ORGAPO}/{RMOAPO}/{NUMAPOL}
Ramo:         {NOMERAMO} (from TGERAMO using RMOSIN)
Segurado:     {NOME} (from TAPOLICE using ORGAPO/RMOAPO/NUMAPOL)
```

**Financial Summary**:
```
Saldo a Pagar (Expected Reserve):  {SDOPAG} formatted as ###,###,###.##
Total Pago (Payments Made):        {TOTPAG} formatted as ###,###,###.##
Saldo Pendente (Pending Value):    {SDOPAG - TOTPAG} formatted as ###,###,###.##
```

**Additional Information**:
```
Tipo de Seguro (Insurance Type):   {TPSEGU} (determines beneficiary requirement)
Tipo de Registro (Policy Type):    {TIPREG} (1 or 2)
Código do Produto (Product Code):  {CODPRODU} (used for validation routing)
```

---

## Payment Authorization (User Story 2)

### Payment Entry Form Fields

**Mandatory Fields**:

1. **Tipo de Pagamento (Payment Type)**
   - Type: Numeric (1-5)
   - Values: 1, 2, 3, 4, 5 (exact values from legacy system)
   - Required: YES
   - Error if invalid: "Tipo de Pagamento deve ser 1, 2, 3, 4, ou 5"

2. **Valor Principal (Principal Amount)**
   - Type: Decimal(15,2)
   - Range: 0.00 to 999,999,999.99
   - Format: Display with comma thousands separator and 2 decimal places
   - Required: YES
   - Error if invalid: "Valor Principal inválido"
   - Validation: Must be <= Pending Value (SDOPAG - TOTPAG)

3. **Tipo de Apólice (Policy Type)**
   - Type: Numeric (1-2)
   - Values: 1 or 2
   - Required: YES
   - Error if invalid: "Tipo de Apólice deve ser 1 ou 2"

**Conditional Fields**:

4. **Valor da Correção (Correction Amount)**
   - Type: Decimal(15,2)
   - Range: 0.00 to 999,999,999.99
   - Required: NO (optional)
   - Default: 0.00 if not provided
   - Used in currency conversion: CRRMON

5. **Beneficiário (Beneficiary Name)**
   - Type: String (max 255 characters)
   - Required: ONLY if TPSEGU != 0 (conditional on insurance type)
   - Error if TPSEGU != 0 and empty: "Beneficiário é obrigatório para este tipo de seguro"

---

### Payment Authorization Processing Steps

#### Step 1: Validation
```
1. Validate all input fields
2. Check claim still exists and hasn't been finalized
3. Verify VALOR_PRINCIPAL <= (SDOPAG - TOTPAG)
4. If TPSEGU != 0: Verify BENEFICIARIO is not empty
5. Get current business date from TSISTEMA where IDSISTEM='SI'
6. Get currency rate from TGEUNIMO for transaction date
7. If no rate found for date: ERROR "Taxa não disponível"
```

#### Step 2: Calculate Standardized Amounts
```
VLCRUZAD = lookup from TGEUNIMO where DTINIVIG <= TODAY <= DTTERVIG
VALPRI = input amount (principal)
CRRMON = input amount (correction, or 0.00)
VALPRIBT = VALPRI × VLCRUZAD
CRRMONBT = CRRMON × VLCRUZAD
VALTOTBT = VALPRIBT + CRRMONBT
```

**Precision**: All calculations maintain 2 decimal places  
**Formula**: Standard BTNF conversion formula - no variations

#### Step 3: External Service Validation

**Routing Decision**:
```
IF CODPRODU IN (6814, 7701, 7709) THEN
    Route to CNOUA (Consortium validation)
ELSE
    Query EF_CONTR_SEG_HABIT for policy
    IF record found AND NUM_CONTRATO > 0 THEN
        Route to SIPUA (EFP contract validation)
    ELSE
        Route to SIMDA (HB contract validation)
    END
END
```

**Validation Call**:
- Call appropriate external service with claim and amount details
- Wait for response (timeout: 10 seconds per service)
- Check response code: EZERT8 field
- If EZERT8 != '00000000': Validation FAILED
- If validation fails: Display error and ABORT entire transaction

**Response Codes**:
```
CNOUA:
  00000000 = Success
  EZERT8001 = Invalid consortium contract
  EZERT8002 = Contract cancelled
  EZERT8003 = Group closed
  EZERT8004 = Quota not contemplated
  EZERT8005 = Beneficiary not authorized

SIPUA/SIMDA:
  (vendor-specific codes)
```

#### Step 4: Create Payment History Record

**Insert into THISTSIN**:
```sql
INSERT INTO THISTSIN (
    TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST,
    OPERACAO, DTMOVTO, HORAOPER,
    VALPRI, CRRMON, NOMFAV, TIPCRR,
    VALPRIBT, CRRMONBT, VALTOTBT,
    SITCONTB, SITUACAO,
    EZEUSRID,
    CREATED_BY, CREATED_AT
) VALUES (
    @tipseg, @orgsin, @rmosin, @numsin, @ocorhist_NEW,
    1098, @business_date, @current_time,
    @valpri, @crrmon, @nomfav, '5',
    @valpribt, @crrmonbt, @valtotbt,
    '0', '0',
    @current_user_id,
    @current_user_id, GETDATE()
)
```

**Automatic Values**:
- OPERACAO = 1098 (FIXED - payment authorization code)
- TIPCRR = '5' (FIXED - standard correction type)
- DTMOVTO = TSISTEMA.DTMOVABE (business date, not clock time)
- HORAOPER = current system time
- SITCONTB = '0' (accounting status initialized)
- SITUACAO = '0' (overall status initialized)
- EZEUSRID = current logged-in operator
- OCORHIST = new sequence number (TMESTSIN.OCORHIST + 1)

#### Step 5: Update Claim Master Record

**Update TMESTSIN**:
```sql
UPDATE TMESTSIN
SET TOTPAG = TOTPAG + @valtotbt,
    OCORHIST = OCORHIST + 1,
    UPDATED_BY = @current_user_id,
    UPDATED_AT = GETDATE()
WHERE TIPSEG = @tipseg
  AND ORGSIN = @orgsin
  AND RMOSIN = @rmosin
  AND NUMSIN = @numsin
```

**Changes**:
- TOTPAG += VALTOTBT (add standardized currency total)
- OCORHIST += 1 (increment occurrence counter)
- Update audit fields

#### Step 6: Create Accompaniment Record

**Insert into SI_ACOMPANHA_SINI**:
```sql
INSERT INTO SI_ACOMPANHA_SINI (
    FONTE, PROTSINI, DAC,
    COD_EVENTO, DATA_MOVTO_SINIACO,
    NUM_OCORR_SINIACO,
    DESCR_COMPLEMENTAR,
    COD_USUARIO,
    CREATED_BY, CREATED_AT
) VALUES (
    @fonte, @protsini, @dac,
    1098, @business_date,
    @ocorhist,
    'Autorização de Pagamento - Valor: ' + CONVERT(VARCHAR, @valtotbt, 103),
    @current_user_id,
    @current_user_id, GETDATE()
)
```

**Values**:
- COD_EVENTO = 1098 (FIXED - payment authorization event)
- Tracks exact event in audit trail with timestamp and operator

#### Step 7: Update Claim Phases

**Query SI_REL_FASE_EVENTO** for event 1098:
```sql
SELECT COD_FASE, IND_ALTERACAO_FASE, DATA_INIVIG_REFAEV
FROM SI_REL_FASE_EVENTO
WHERE COD_EVENTO = 1098
  AND DATA_INIVIG_REFAEV <= @business_date
ORDER BY DATA_INIVIG_REFAEV DESC
```

**For each relationship returned**:

**If IND_ALTERACAO_FASE = '1' (ABERTURA - Open Phase)**:
```
1. Check if phase already open (prevent duplicates)
2. If not exists:
   INSERT INTO SI_SINISTRO_FASE (
       FONTE, PROTSINI, DAC, COD_FASE, COD_EVENTO, NUM_OCORR_SINIACO, DATA_INIVIG_REFAEV,
       DATA_ABERTURA_SIFA, DATA_FECHA_SIFA,
       CREATED_BY, CREATED_AT
   ) VALUES (
       @fonte, @protsini, @dac, @cod_fase, 1098, @ocorhist, @data_inivig_refaev,
       @business_date, '9999-12-31',
       @current_user_id, GETDATE()
   )
```

**If IND_ALTERACAO_FASE = '2' (FECHAMENTO - Close Phase)**:
```
1. Find open phase (WHERE DATA_FECHA_SIFA = '9999-12-31')
2. If found:
   UPDATE SI_SINISTRO_FASE
   SET DATA_FECHA_SIFA = @business_date,
       UPDATED_BY = @current_user_id,
       UPDATED_AT = GETDATE()
   WHERE FONTE = @fonte AND PROTSINI = @protsini AND DAC = @dac
     AND COD_FASE = @cod_fase AND COD_EVENTO = 1098
     AND DATA_FECHA_SIFA = '9999-12-31'
```

#### Step 8: Commit Transaction

**All-or-Nothing Principle**:
```
BEGIN TRANSACTION
  -- Steps 4-7 above
  COMMIT TRANSACTION
CATCH
  ROLLBACK TRANSACTION
  EZEROLLB() -- Legacy rollback flag
  THROW -- Propagate error to caller
END
```

**Critical**: If ANY step fails, entire transaction rolls back

---

## All 42+ Business Rules

### Core Search Rules (FR-001 to FR-005)

**BR-001**: System allows three mutually exclusive search criteria
- Protocol: FONTE + PROTSINI + DAC
- Claim Number: ORGSIN + RMOSIN + NUMSIN
- Leader Code: CODLIDER + SINLID

**BR-002**: At least one complete search criterion must be provided

**BR-003**: Claim data retrieved from TMESTSIN table using composite key

**BR-004**: Protocol number displayed as: {FONTE}/{PROTSINI}-{DAC}

**BR-005**: Claim number displayed as: {ORGSIN}/{RMOSIN}/{NUMSIN}

**BR-006**: Claim not found error: "DOCUMENTO {ID} NAO CADASTRADO"

**BR-007**: Branch name retrieved from TGERAMO using RMOSIN foreign key

**BR-008**: Insured name retrieved from TAPOLICE using policy reference

**BR-009**: Currency amounts displayed with comma thousands separator and 2 decimals

---

### Payment Authorization Rules (FR-006 to FR-019)

**BR-010**: Payment type must be 1, 2, 3, 4, or 5

**BR-011**: Payment type invalid error: "Tipo de Pagamento deve ser 1, 2, 3, 4, ou 5"

**BR-012**: Principal amount must be numeric, non-negative

**BR-013**: Principal amount must not exceed pending value: VALPRI <= (SDOPAG - TOTPAG)

**BR-014**: Principal amount exceeds pending error: "Valor Superior ao Saldo Pendente"

**BR-015**: Policy type must be 1 or 2

**BR-016**: Policy type invalid error: "Tipo de Apólice deve ser 1 ou 2"

**BR-017**: Correction amount optional, defaults to 0.00 if omitted

**BR-018**: Correction amount if provided must be numeric and non-negative

**BR-019**: Beneficiary (Favorecido) required ONLY if TPSEGU != 0

**BR-020**: Beneficiary required error: "Favorecido é obrigatório para este tipo de seguro"

**BR-021**: Beneficiary field accepts up to 255 characters

**BR-022**: Special characters in beneficiary preserved as-is

---

### Currency Conversion Rules (FR-016, FR-017, SC-008)

**BR-023**: Currency conversion formula: AMOUNT_BTNF = AMOUNT_ORIGINAL × VLCRUZAD

**BR-024**: Conversion rate obtained from TGEUNIMO table

**BR-025**: Rate selection: Find record where DTINIVIG <= TRANSACTION_DATE <= DTTERVIG

**BR-026**: No rate for transaction date error: "Taxa de conversão não disponível para a data"

**BR-027**: Conversion rate precision: 8 decimal places

**BR-028**: All currency calculations maintain 2 decimal places in final result

**BR-029**: Principal conversion: VALPRIBT = VALPRI × VLCRUZAD

**BR-030**: Correction conversion: CRRMONBT = CRRMON × VLCRUZAD

**BR-031**: Total calculation: VALTOTBT = VALPRIBT + CRRMONBT

**BR-032**: Daily amounts calculated: PRIDIABT, CRRDIABT, TOTDIABT (business logic)

**BR-033**: Currency code always BTNF (Bônus do Tesouro Nacional Fiscal)

---

### Transaction Recording Rules (FR-012 to FR-015)

**BR-034**: Operation code always 1098 for payment authorization

**BR-035**: Transaction date = TSISTEMA.DTMOVABE (business date, not system clock)

**BR-036**: Transaction time = current system time at authorization moment

**BR-037**: Accounting status initialized to '0'

**BR-038**: Overall status initialized to '0'

**BR-039**: Correction type always '5' for payment authorizations

**BR-040**: Occurrence counter incremented: OCORHIST_NEW = OCORHIST_CURRENT + 1

**BR-041**: Operator user ID recorded: EZEUSRID = current logged-in user

**BR-042**: Audit fields populated: CREATED_BY, CREATED_AT, UPDATED_BY, UPDATED_AT

---

### Product Validation Rules (FR-020 to FR-024)

**BR-043**: Consortium products: 6814, 7701, 7709 → Route to CNOUA

**BR-044**: Query EF_CONTR_SEG_HABIT for policy contract number

**BR-045**: EFP contract (NUM_CONTRATO > 0) → Route to SIPUA

**BR-046**: HB contract (NUM_CONTRATO = 0 or not found) → Route to SIMDA

**BR-047**: External service response code EZERT8 checked for success

**BR-048**: EZERT8 != '00000000' indicates validation failure

**BR-049**: Validation error response contains descriptive message

**BR-050**: Payment authorization halted if validation fails

**BR-051**: Transaction rolled back if validation fails (atomicity)

**BR-052**: Consortium validation error: "Contrato de consórcio inválido" (EZERT8001)

**BR-053**: Contract cancelled error: "Contrato cancelado" (EZERT8002)

**BR-054**: Group closed error: "Grupo encerrado" (EZERT8003)

**BR-055**: Quota not contemplated error: "Cota não contemplada" (EZERT8004)

**BR-056**: Beneficiary not authorized error: "Beneficiário não autorizado" (EZERT8005)

---

### Phase Management Rules (FR-025 to FR-030)

**BR-057**: Claim accompaniment record created with COD_EVENTO = 1098

**BR-058**: Phase changes determined by SI_REL_FASE_EVENTO configuration table

**BR-059**: Phase opening (IND_ALTERACAO_FASE='1'): Create with DATA_FECHA_SIFA='9999-12-31'

**BR-060**: Phase closing (IND_ALTERACAO_FASE='2'): Update existing open phase with current date

**BR-061**: Open phase indicator: DATA_FECHA_SIFA = '9999-12-31' (sentinel value)

**BR-062**: Phase opening date set to current business date

**BR-063**: Prevent duplicate open phases for same claim/phase/event combination

**BR-064**: Query SI_REL_FASE_EVENTO to find all relationships for event 1098

**BR-065**: Process relationships in order (may open multiple phases, close multiple phases)

**BR-066**: Phase rollback: All database changes rolled back if phase update fails

**BR-067**: Phase atomicity: All or nothing transaction

---

### Audit Trail Rules (FR-051, FR-052, FR-053)

**BR-068**: Operator user ID recorded on all history records (EZEUSRID)

**BR-069**: Operator user ID recorded on all accompaniment records (COD_USUARIO)

**BR-070**: Operator user ID recorded on all phase records (CREATED_BY, UPDATED_BY)

**BR-071**: Timestamp recorded: CREATED_AT on insertion, UPDATED_AT on modification

**BR-072**: Complete audit trail reconstruction possible via SI_ACOMPANHA_SINI

**BR-073**: Transaction date immutable after creation (DTMOVTO)

**BR-074**: Referential integrity maintained across all tables

---

### Data Validation Rules

**BR-075**: TIPSEG must be numeric and consistent across claim records

**BR-076**: ORGSIN must be 2-digit code (01-99)

**BR-077**: RMOSIN must be 2-digit code (00-99)

**BR-078**: NUMSIN must be 1-6 digit claim number (000001-999999)

**BR-079**: FONTE must be numeric (1-9 typically)

**BR-080**: PROTSINI must be numeric (000001-999999 typically)

**BR-081**: DAC must be single digit (0-9)

**BR-082**: CODPRODU must be numeric and valid product code

**BR-083**: SDOPAG (reserve) must be >= 0

**BR-084**: TOTPAG (payments) must be >= 0 and <= SDOPAG

**BR-085**: OCORHIST (occurrence counter) non-negative integer

**BR-086**: VALPRI (principal) >= 0 and <= SDOPAG - TOTPAG

**BR-087**: CRRMON (correction) >= 0

---

### UI/Display Rules (FR-031 to FR-037)

**BR-088**: All UI text in Portuguese

**BR-089**: Numeric amounts formatted: ###,###,###.##

**BR-090**: Date format: DD/MM/YYYY for display, YYYY-MM-DD for storage

**BR-091**: Time format: HH:MM:SS

**BR-092**: Error messages displayed in red (#e80c4d)

**BR-093**: Caixa Seguradora logo displayed in header

**BR-094**: Site.css stylesheet applied without modifications

**BR-095**: Responsive design supporting desktop and mobile (max-width: 850px)

---

### Performance Rules (SC-001, SC-002)

**BR-096**: Claim search completes in < 3 seconds

**BR-097**: Payment authorization cycle completes in < 90 seconds

**BR-098**: History query returns < 500ms for 1000+ records

**BR-099**: Pagination: 20 records per page (default), max 100 per page

**BR-100**: History ordered by OCORHIST DESC (most recent first)

---

## External Integrations

### 1. CNOUA (Consortium Validation Service)

**Trigger**: Product code in (6814, 7701, 7709)

**Service Type**: REST API

**Protocol**: HTTP/HTTPS with TLS

**Base URL**: {CNOUA_BASE_URL}/validate (configured in appsettings)

**Timeout**: 10 seconds

**Request Format**:
```json
{
  "claimId": 12345,
  "productCode": "6814",
  "policyNumber": "001/0123456",
  "contractNumber": "CON-2024-001",
  "amount": 25000.00,
  "currencyCode": "BRL"
}
```

**Success Response**:
```json
{
  "status": "APPROVED",
  "ezert8Code": "00000000",
  "validatedAt": "2025-10-23T14:30:00Z",
  "responseTimeMs": 1250
}
```

**Error Response** (example):
```json
{
  "status": "REJECTED",
  "ezert8Code": "EZERT8001",
  "message": "Contrato de consórcio inválido",
  "responseTimeMs": 850
}
```

**Error Codes**:
```
00000000 - Success
EZERT8001 - Contrato de consórcio inválido (Invalid consortium contract)
EZERT8002 - Contrato cancelado (Contract cancelled)
EZERT8003 - Grupo encerrado (Group closed)
EZERT8004 - Cota não contemplada (Quota not contemplated)
EZERT8005 - Beneficiário não autorizado (Beneficiary not authorized)
```

**Resilience Policy**:
- Retry: 3 attempts with exponential backoff (2s, 4s, 8s)
- Circuit Breaker: Opens after 5 consecutive failures, break duration 30s
- Timeout: 10 seconds

---

### 2. SIPUA (EFP Contract Validation Service)

**Trigger**: Product not consortium AND EF_CONTR_SEG_HABIT.NUM_CONTRATO > 0

**Service Type**: SOAP Web Service

**Protocol**: SOAP 1.2 over HTTP/HTTPS

**WSDL URL**: {SIPUA_BASE_URL}/services/validation?wsdl (configured in appsettings)

**Timeout**: 10 seconds

**SOAP Request**:
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:val="http://sipua.validation.caixa.com.br">
   <soapenv:Header/>
   <soapenv:Body>
      <val:ValidateEFPContract>
         <val:contractNumber>12345</val:contractNumber>
         <val:policyNumber>001/0123456-7</val:policyNumber>
         <val:amount>10000.00</val:amount>
      </val:ValidateEFPContract>
   </soapenv:Body>
</soapenv:Envelope>
```

**SOAP Response**:
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
   <soap:Body>
      <ValidateEFPContractResponse>
         <status>APPROVED</status>
         <validationCode>VAL-OK-001</validationCode>
      </ValidateEFPContractResponse>
   </soap:Body>
</soap:Envelope>
```

**Resilience Policy**: Same as CNOUA

---

### 3. SIMDA (HB Contract Validation Service)

**Trigger**: Product not consortium AND (EF_CONTR_SEG_HABIT.NUM_CONTRATO = 0 OR not found)

**Service Type**: SOAP Web Service

**Protocol**: SOAP 1.2 over HTTP/HTTPS

**WSDL URL**: {SIMDA_BASE_URL}/services/validation?wsdl

**Timeout**: 10 seconds

**SOAP Request**:
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:val="http://simda.validation.caixa.com.br">
   <soapenv:Header/>
   <soapenv:Body>
      <val:ValidateHBContract>
         <val:claimNumber>001/0023456/001</val:claimNumber>
         <val:beneficiaryTaxId>12345678901</val:beneficiaryTaxId>
         <val:amount>5000.00</val:amount>
      </val:ValidateHBContract>
   </soapenv:Body>
</soapenv:Envelope>
```

**Resilience Policy**: Same as CNOUA

---

## Phase & Workflow Management

### Phase Management Sequence

**When Payment Authorization Occurs (Event 1098)**:

1. **Query Phase Relationships**:
   ```sql
   SELECT COD_FASE, IND_ALTERACAO_FASE, DATA_INIVIG_REFAEV
   FROM SI_REL_FASE_EVENTO
   WHERE COD_EVENTO = 1098
     AND DATA_INIVIG_REFAEV <= GETDATE()
   ORDER BY DATA_INIVIG_REFAEV DESC
   ```

2. **For Each Relationship**:

   **If Opening (IND_ALTERACAO_FASE = '1')**:
   - Check duplicate: Prevent multiple open phases for same claim/phase
   - Create new phase record with DATA_FECHA_SIFA = '9999-12-31'
   - Set opening date to current business date
   - Record operator ID

   **If Closing (IND_ALTERACAO_FASE = '2')**:
   - Find open phase (DATA_FECHA_SIFA = '9999-12-31')
   - Update closing date to current business date
   - Record operator ID

3. **Example Workflow for Event 1098**:
   ```
   Phase 10 (Payment Processing) OPENS
   Phase 5 (Pending Documentation) CLOSES
   (if configured in SI_REL_FASE_EVENTO)
   ```

### Phase Query for Display

To show all phases for a claim:
```sql
SELECT
    COD_FASE,
    COD_EVENTO,
    DATA_ABERTURA_SIFA,
    DATA_FECHA_SIFA,
    CASE
        WHEN DATA_FECHA_SIFA = '9999-12-31'
        THEN 'Aberta' 
        ELSE 'Fechada'
    END AS Status,
    CASE
        WHEN DATA_FECHA_SIFA = '9999-12-31'
        THEN DATEDIFF(DAY, DATA_ABERTURA_SIFA, GETDATE())
        ELSE DATEDIFF(DAY, DATA_ABERTURA_SIFA, DATA_FECHA_SIFA)
    END AS DiasAberta
FROM SI_SINISTRO_FASE
WHERE FONTE = @fonte
  AND PROTSINI = @protsini
  AND DAC = @dac
ORDER BY DATA_ABERTURA_SIFA DESC
```

---

## User Interface Screens

### Screen 1: Claim Search (SI11M010)

**Layout**:
```
┌─────────────────────────────────────────────┐
│         Caixa Seguradora Logo              │
├─────────────────────────────────────────────┤
│  BUSCA DE SINISTROS                         │
├─────────────────────────────────────────────┤
│                                             │
│  [ ] Por Protocolo:                         │
│      Fonte: ____  Protocolo: ____  DAC: __ │
│                                             │
│  [ ] Por Sinistro:                          │
│      Órgão: __  Ramo: __  Número: ____     │
│                                             │
│  [ ] Por Líder:                             │
│      Código: ___  Sinistro: _______        │
│                                             │
│                    [Pesquisar] [Limpar]    │
│                                             │
│  Mensagem de Erro (se houver):             │
│  {MENSAGEM}                                │
│                                             │
└─────────────────────────────────────────────┘
```

**Validation**:
- At least one search mode must have complete data
- Submit button disabled until valid

**Success**: Navigate to Screen 2 (Claim Details)
**Error**: Display message in red

---

### Screen 2: Claim Details & Payment (SIHM020)

**Layout - Top Section (Read-only)**:
```
┌─────────────────────────────────────────────┐
│  PROTOCOLO: 001/0123456-7                   │
│  SINISTRO:  10/20/789012                    │
│  APÓLICE:   01/05/0045678                   │
│  RAMO:      AUTOMÓVEL                       │
│  SEGURADO:  JOÃO SILVA DOS SANTOS           │
│                                             │
│  SALDO A PAGAR:     R$ 50,000.00           │
│  TOTAL PAGO:        R$ 10,000.00           │
│  SALDO PENDENTE:    R$ 40,000.00           │
│                                             │
│  TIPO DE SEGURO: 5                          │
│  CÓDIGO PRODUTO: 6814                       │
└─────────────────────────────────────────────┘
```

**Layout - Payment Form (Input)**:
```
┌─────────────────────────────────────────────┐
│  AUTORIZAÇÃO DE PAGAMENTO                   │
├─────────────────────────────────────────────┤
│                                             │
│  Tipo de Pagamento (1-5): [1___]           │
│  Tipo de Apólice (1 ou 2): [1___]          │
│  Valor Principal: [R$ ___,___,___.__ ]    │
│  Valor Correção: [R$ ___,___,___.__ ]    │
│  Beneficiário: [________________________]  │
│                                             │
│              [Autorizar] [Cancelar]        │
│                                             │
└─────────────────────────────────────────────┘
```

**Layout - History Section**:
```
┌─────────────────────────────────────────────┐
│  HISTÓRICO DE PAGAMENTOS                    │
├─────────────────────────────────────────────┤
│  Data      | Valor       | Operador | Status│
│  23/10/2025| R$ 5.000.00 | USER001  | OK   │
│  22/10/2025| R$ 3.500.00 | USER002  | OK   │
│  20/10/2025| R$ 1.500.00 | USER001  | OK   │
│                                             │
│  [Anterior] [Próxima]  Página 1 de 5       │
└─────────────────────────────────────────────┘
```

---

## Transaction Processing

### Transaction Flow Diagram

```
START Payment Authorization
│
├─ Input Validation
│  ├─ Check payment type (1-5)
│  ├─ Check principal amount
│  ├─ Check policy type (1-2)
│  └─ Check beneficiary (if required)
│
├─ Data Lookup
│  ├─ Get claim from TMESTSIN
│  ├─ Get pending value
│  ├─ Verify claim exists
│  ├─ Get business date from TSISTEMA
│  └─ Get currency rate from TGEUNIMO
│
├─ External Validation
│  ├─ Determine routing (CNOUA/SIPUA/SIMDA)
│  ├─ Call external service
│  ├─ Check EZERT8 code
│  └─ [IF FAILS: Abort, display error]
│
├─ BEGIN TRANSACTION
│  │
│  ├─ Create History Record (THISTSIN)
│  │  └─ Insert payment transaction details
│  │
│  ├─ Update Claim Master (TMESTSIN)
│  │  ├─ TOTPAG += VALTOTBT
│  │  ├─ OCORHIST += 1
│  │  └─ Set audit fields
│  │
│  ├─ Create Accompaniment (SI_ACOMPANHA_SINI)
│  │  └─ Record event 1098 with operator ID
│  │
│  ├─ Update Phases (SI_SINISTRO_FASE)
│  │  ├─ Query SI_REL_FASE_EVENTO for event 1098
│  │  ├─ Open phases (IND_ALTERACAO_FASE='1')
│  │  ├─ Close phases (IND_ALTERACAO_FASE='2')
│  │  └─ [IF FAILS: Rollback entire transaction]
│  │
│  └─ COMMIT TRANSACTION
│
├─ Success Response
│  ├─ Display confirmation message
│  ├─ Show updated claim details
│  └─ Refresh history
│
└─ [IF ANY ERROR: Rollback, display error message]

END
```

### Transaction Atomicity

**All-or-Nothing Principle**:
```csharp
using (var transaction = await _unitOfWork.BeginTransactionAsync())
{
    try
    {
        // Steps 1-4 above
        await transaction.CommitAsync();
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        throw; // Propagate error to caller
    }
}
```

**Failure Scenarios**:
1. Validation fails → ABORT before transaction
2. History insert fails → ROLLBACK entire transaction
3. Claim master update fails → ROLLBACK entire transaction
4. Phase update fails → ROLLBACK entire transaction

---

## Error Handling & Messages

### Error Message Format

All error messages in Portuguese, displayed prominently (red color #e80c4d)

### Common Error Messages

| Error Code | Portuguese Message | Trigger |
|------------|-------------------|---------|
| VAL-001 | "Protocolo XXXXXXX-X NAO ENCONTRADO" | Protocol search fails |
| VAL-002 | "SINISTRO XXXXXXX NAO ENCONTRADO" | Claim search fails |
| VAL-003 | "LIDER XXXXXXX-XXXXXXX NAO ENCONTRADO" | Leader search fails |
| VAL-004 | "Tipo de Pagamento deve ser 1, 2, 3, 4, ou 5" | Invalid payment type |
| VAL-005 | "Valor Superior ao Saldo Pendente" | Principal exceeds pending |
| VAL-006 | "Tipo de Apólice deve ser 1 ou 2" | Invalid policy type |
| VAL-007 | "Favorecido é obrigatório para este tipo de seguro" | Missing required beneficiary |
| VAL-008 | "Taxa de conversão não disponível para a data" | No currency rate for date |
| VAL-009 | "PROBLEMAS NA SUBROTINA: PTFASESS" | Phase update service fails |
| CONS-001 | "Contrato de consórcio inválido" | CNOUA returns EZERT8001 |
| CONS-002 | "Contrato cancelado" | CNOUA returns EZERT8002 |
| CONS-003 | "Grupo encerrado" | CNOUA returns EZERT8003 |
| CONS-004 | "Cota não contemplada" | CNOUA returns EZERT8004 |
| CONS-005 | "Beneficiário não autorizado" | CNOUA returns EZERT8005 |
| SYS-001 | "Erro ao buscar dados do sinistro" | Database query fails |
| SYS-002 | "Erro ao inserir histórico de pagamento" | Insert fails |
| SYS-003 | "Erro ao atualizar saldo do sinistro" | Update fails |
| SYS-004 | "Erro ao processar fases" | Phase logic fails |
| SYS-005 | "Serviço de validação indisponível" | External service timeout |

---

## Audit & Compliance

### Audit Trail Components

**1. Claim Accompaniment (SI_ACOMPANHA_SINI)**
- Records every event (event code 1098 = payment authorization)
- Captures operator user ID (COD_USUARIO)
- Captures exact transaction date
- Captures occurrence number sequence
- Allows event reconstruction and investigation

**2. History Records (THISTSIN)**
- Records every payment transaction
- Captures operator who authorized (EZEUSRID)
- Captures amounts in original and standardized currency
- Captures beneficiary name for payments
- Captures date/time of authorization
- Cannot be modified (insert-only for transactions)

**3. Claim Master (TMESTSIN)**
- Tracks total payments (TOTPAG) - running total
- Tracks occurrence counter (OCORHIST) - event sequence number
- Updated by: UPDATED_BY, UPDATED_AT fields

**4. Phase Records (SI_SINISTRO_FASE)**
- Records workflow state changes
- Captures dates of state changes
- Tracks which event triggered state change

**5. Complete Audit Trail Query**:
```sql
SELECT
    a.COD_EVENTO,
    a.DATA_MOVTO_SINIACO,
    a.COD_USUARIO,
    h.OPERACAO,
    h.VALPRIBT + h.CRRMONBT AS VALOR_AUTORIZADO,
    h.NOMFAV AS BENEFICIARIO,
    f.COD_FASE,
    f.DATA_ABERTURA_SIFA,
    f.DATA_FECHA_SIFA
FROM SI_ACOMPANHA_SINI a
LEFT JOIN THISTSIN h ON (
    a.FONTE = h.FONTE AND
    a.PROTSINI = h.PROTSINI AND
    a.DAC = h.DAC AND
    a.DATA_MOVTO_SINIACO = h.DTMOVTO
)
LEFT JOIN SI_SINISTRO_FASE f ON (
    a.FONTE = f.FONTE AND
    a.PROTSINI = f.PROTSINI AND
    a.DAC = f.DAC AND
    a.COD_EVENTO = f.COD_EVENTO
)
WHERE a.FONTE = @fonte
  AND a.PROTSINI = @protsini
  AND a.DAC = @dac
ORDER BY a.DATA_MOVTO_SINIACO DESC, a.NUM_OCORR_SINIACO DESC
```

---

## Performance Requirements

### Response Time Targets

| Operation | Requirement | Notes |
|-----------|-------------|-------|
| Claim Search | < 3 seconds | By any search criterion |
| Payment Authorization | < 90 seconds total | Including external validation |
| History Query | < 500ms | For 1000+ records |
| Phase Query | < 200ms | Typical claim with 5-10 phases |
| Currency Lookup | < 100ms | TGEUNIMO table query |

### Database Optimization

**Critical Index for History Performance**:
```sql
CREATE NONCLUSTERED INDEX IX_THISTSIN_Claim_Occurrence
ON THISTSIN(TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC)
INCLUDE (OPERACAO, DTMOVTO, HORAOPER, VALPRI, CRRMON, NOMFAV,
         TIPCRR, VALPRIBT, CRRMONBT, VALTOTBT, SITCONTB, SITUACAO, EZEUSRID)
WITH (FILLFACTOR = 90)
```

**Benefits**:
- Covering index (all columns in index)
- Composite key on claim + ordering column
- DESC order matches query ORDER BY clause
- Expected improvement: 80-90% reduction in query time

**Other Indexes**:
- IX_TMESTSIN_Protocol: (FONTE, PROTSINI, DAC)
- IX_TMESTSIN_Leader: (CODLIDER, SINLID)
- IX_TMESTSIN_Policy: (ORGAPO, RMOAPO, NUMAPOL)

---

## Implementation Checklist for .NET 9 Migration

### Frontend (React 19 + TypeScript)
- [ ] Claim search page (protocol/claim/leader inputs)
- [ ] Claim detail page (read-only display)
- [ ] Payment authorization form (inputs + validation)
- [ ] History pagination
- [ ] Phase timeline display
- [ ] Error message display (red #e80c4d)
- [ ] Caixa Seguradora logo in header
- [ ] Site.css integration
- [ ] Mobile responsive (850px max-width)
- [ ] Portuguese UI text

### Backend (.NET 9)
- [ ] ClaimMaster entity with all validations
- [ ] THISTSIN entity for payment history
- [ ] All 10 legacy entity models
- [ ] ClaimsDbContext with proper configurations
- [ ] ClaimService search implementation
- [ ] PaymentAuthorizationService
- [ ] CurrencyConversionService
- [ ] CNOUA, SIPUA, SIMDA external service clients
- [ ] Phase management service
- [ ] Accompaniment event service
- [ ] SOAP endpoint implementation (SoapCore)
- [ ] ClaimsController REST endpoints
- [ ] GlobalExceptionHandlerMiddleware
- [ ] AutoMapper profiles
- [ ] FluentValidation validators
- [ ] Transaction management (BEGIN/COMMIT/ROLLBACK)
- [ ] Audit logging (EZEUSRID tracking)
- [ ] Database indexes for performance
- [ ] Connection pooling for TSISTEMA queries

### Database
- [ ] Create all 13 tables
- [ ] Create all foreign keys
- [ ] Create all recommended indexes
- [ ] Set up view/function for currency lookup
- [ ] Verify legacy data exists

### Testing
- [ ] Unit tests for all services (xUnit)
- [ ] Integration tests for database operations
- [ ] E2E tests for complete workflows (Playwright)
- [ ] Performance tests (< 3s search, < 90s payment)
- [ ] External service integration tests (Polly mocks)
- [ ] Transaction rollback tests

### Documentation
- [ ] API documentation (Swagger)
- [ ] Business rules reference
- [ ] Database schema documentation
- [ ] Error code reference
- [ ] Deployment guide

---

## Critical Implementation Notes

### 1. Do NOT Modify Legacy Formulas
All currency conversion and calculation formulas must match legacy system exactly:
```
VALPRIBT = VALPRI × VLCRUZAD (no variations)
VALTOTBT = VALPRIBT + CRRMONBT (no intermediate rounding)
```

### 2. Business Date is Critical
**NOT** the system clock date. Must query TSISTEMA:
```
SELECT DTMOVABE FROM TSISTEMA WHERE IDSISTEM = 'SI'
```

### 3. Transaction Atomicity is Non-Negotiable
All four operations (history, master, accompaniment, phases) must succeed together or all roll back:
- No partial updates
- No orphaned records
- Complete ACID compliance

### 4. 42+ Business Rules Must be Preserved
Every rule in the specification section must be implemented exactly. No simplifications or "improvements" to legacy logic.

### 5. External Service Integration is Critical
- CNOUA, SIPUA, SIMDA must be called with correct parameters
- Response codes must be checked (EZERT8 != '00000000')
- Circuit breaker pattern required (Polly library)
- Timeout: 10 seconds per service

### 6. Audit Trail is Mandatory
Every payment authorization must create:
- THISTSIN record (payment transaction)
- SI_ACOMPANHA_SINI record (event record)
- SI_SINISTRO_FASE records (phase changes)

### 7. Currency Precision
- Conversion rates: 8 decimal places
- Final amounts: 2 decimal places
- No rounding errors - use DECIMAL type, not FLOAT

---

**END OF ANALYSIS**

This document provides complete specification for implementing SIWEA in .NET 9 + React. Every business rule, database operation, validation, error condition, and calculation is documented. Implementation teams should reference this document as the authoritative source for legacy system behavior.
