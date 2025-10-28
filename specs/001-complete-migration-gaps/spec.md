# Feature Specification: Complete SIWEA Migration - Gaps Analysis and Implementation

**Feature Branch**: `001-complete-migration-gaps`
**Created**: 2025-10-27
**Status**: Draft
**Input**: User description: "analise toda a documentacao /Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/docs, identifique os gaps do que anda falta implementar da migracao e finalize a migracao 100%. /Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/#SIWEA-V116.esf"

## Executive Summary

Based on comprehensive analysis of documentation in `/docs` and existing codebase, this specification identifies and addresses gaps required to achieve 100% migration of the SIWEA (Claims Indemnity Payment Authorization System) from IBM Visual Age EZEE 4.40 to .NET 9 + React. The analysis reveals 5 critical gap areas across 13 database entities, 100+ business rules, and 6 user stories.

### Current Implementation Status

**Completed (60-65%)**:
- Database entities and EF Core configurations (13 entities)
- Repository pattern infrastructure
- Basic ClaimService with search functionality
- Payment authorization service skeleton
- Currency conversion service
- Phase management service
- Basic React components (search, authorization forms)
- Dashboard infrastructure

**Critical Gaps Identified (35-40%)**:
1. External Service Integration (0%): CNOUA, SIPUA, SIMDA validation clients not implemented
2. Transaction Atomicity (30%): Multi-table transaction rollback incomplete
3. Business Rules Enforcement (45%): 55+ rules missing validation
4. UI/Display Completion (50%): Site.css integration, error messages, mobile responsiveness
5. Testing Coverage (25%): Unit tests placeholder only, no integration or E2E tests

## User Scenarios & Testing

### User Story 1 - Complete External Service Validation Integration (Priority: P1)

Payment authorization requires validation through three external services (CNOUA for consortium products, SIPUA for EFP contracts, SIMDA for HB contracts). These services must be integrated with Polly resilience policies to ensure reliable validation before payment processing.

**Why this priority**: System-critical functionality blocking 100% payment authorization capability. Without external validation, payments for consortium products and contracts cannot be processed, violating business rules BR-043 through BR-046.

**Independent Test**: Can be tested by authorizing payment for consortium product 6814 and verifying system calls CNOUA service, receives validation response, and either proceeds or displays appropriate error message based on response code.

**Acceptance Scenarios**:

1. **Given** a claim for product code 6814, **When** authorizing payment, **Then** system calls CNOUA REST API endpoint with claim details and receives validation response with EZERT8 code
2. **Given** a claim with EFP contract (NUM_CONTRATO > 0 in EF_CONTR_SEG_HABIT), **When** authorizing payment, **Then** system calls SIPUA SOAP service and validates contract status
3. **Given** a claim with HB contract (NUM_CONTRATO = 0 or not found), **When** authorizing payment, **Then** system calls SIMDA SOAP service for validation
4. **Given** external service returns EZERT8 != '00000000', **When** processing validation response, **Then** system halts payment authorization and displays descriptive error message from service
5. **Given** external service times out after 10 seconds, **When** calling service, **Then** system retries 3 times with exponential backoff before failing with SYS-005 error
6. **Given** external service fails repeatedly, **When** circuit breaker opens, **Then** system fails fast for subsequent requests and displays service unavailable error
7. **Given** validation service succeeds with EZERT8 = '00000000', **When** response received, **Then** system proceeds with payment authorization transaction

---

### User Story 2 - Implement Complete Transaction Atomicity (Priority: P1)

Payment authorization must execute as atomic transaction across 4 database operations: insert THISTSIN history, update TMESTSIN claim master, insert SI_ACOMPANHA_SINI event, update SI_SINISTRO_FASE phases. If any operation fails, entire transaction must rollback with no partial updates.

**Why this priority**: System-critical for data integrity. Violating BR-067 (transaction atomicity) causes data corruption and audit trail inconsistencies that cannot be recovered.

**Independent Test**: Can be tested by simulating phase update failure during payment authorization and verifying all operations rollback (no history record, no claim master update, no event record, no phase changes).

**Acceptance Scenarios**:

1. **Given** payment authorization passes validation, **When** creating history record fails, **Then** entire transaction rolls back with no changes to any table
2. **Given** history record created successfully, **When** updating claim master fails, **Then** transaction rolls back including history record deletion
3. **Given** claim master updated successfully, **When** creating accompaniment event fails, **Then** transaction rolls back all previous operations
4. **Given** accompaniment event created, **When** phase update fails, **Then** transaction rolls back all operations and displays SYS-004 error
5. **Given** all 4 operations succeed, **When** transaction commits, **Then** all changes persist and success confirmation displays
6. **Given** transaction rollback occurs, **When** displaying error, **Then** system logs complete error trace with operation that triggered failure

---

### User Story 3 - Enforce All 100+ Business Rules (Priority: P2)

System must enforce all business rules documented in BUSINESS_RULES_INDEX.md (BR-001 through BR-099) covering validation, calculations, workflows, audit trail, and data integrity. Currently 45% of rules lack enforcement.

**Why this priority**: Business-critical for correct system behavior and compliance. Missing rules cause incorrect calculations, invalid data, and audit failures.

**Independent Test**: Can be tested by running comprehensive test suite with one test per business rule, verifying each rule enforces expected behavior and rejects invalid scenarios.

**Acceptance Scenarios**:

1. **Given** payment with TPSEGU != 0, **When** beneficiary field empty, **Then** system displays VAL-007 error "Favorecido é obrigatório para este tipo de seguro" (BR-019)
2. **Given** payment amount exceeding SDOPAG - TOTPAG, **When** submitting authorization, **Then** system displays VAL-005 error "Valor Superior ao Saldo Pendente" (BR-013)
3. **Given** payment authorization, **When** calculating VALPRIBT, **Then** system applies exact formula VALPRI × VLCRUZAD with 8-decimal rate and 2-decimal result (BR-023, BR-027, BR-028)
4. **Given** payment authorization, **When** recording transaction date, **Then** system uses DTMOVABE from TSISTEMA table, never system clock (BR-035)
5. **Given** payment authorization, **When** recording operation code, **Then** system always uses 1098, never any other value (BR-034)
6. **Given** payment authorization, **When** recording correction type, **Then** system always uses '5', never any other value (BR-039)
7. **Given** phase opening event (IND='1'), **When** creating phase record, **Then** system sets DATA_FECHA_SIFA to '9999-12-31' (BR-059, BR-061)
8. **Given** duplicate open phase exists, **When** attempting to open same phase, **Then** system prevents duplicate creation (BR-063)
9. **Given** monetary values display, **When** formatting amounts, **Then** system uses format with thousands separator and 2 decimals (BR-089)
10. **Given** any transaction, **When** recording audit trail, **Then** system captures EZEUSRID operator ID on all history, accompaniment, and phase records (BR-068, BR-069, BR-070)

---

### User Story 4 - Complete UI/Display Implementation (Priority: P3)

System must apply Site.css stylesheet exactly as provided, display all error messages in Portuguese with red color, show Caixa Seguradora logo, and support mobile responsive design (max-width 850px).

**Why this priority**: Operational requirement for user acceptance and branding compliance. Not blocking core functionality but required for production deployment.

**Independent Test**: Can be tested by accessing application on desktop and mobile devices, verifying Site.css applies correctly, logo displays in header, error messages show in Portuguese with correct color, and UI remains usable on small screens.

**Acceptance Scenarios**:

1. **Given** user accesses application, **When** page loads, **Then** Site.css stylesheet applies without modifications (BR-094)
2. **Given** validation error occurs, **When** displaying message, **Then** text shows in red and uses Portuguese message from ErrorMessages.pt-BR resource (BR-092)
3. **Given** user views header, **When** page renders, **Then** Caixa Seguradora logo displays from base64 PNG (BR-093)
4. **Given** user accesses on mobile device with 850px or less width, **When** viewing pages, **Then** layout adapts responsively and remains fully functional (BR-095)
5. **Given** numeric amounts display, **When** rendering currency values, **Then** format shows with thousands separator and 2 decimals (BR-089)
6. **Given** dates display, **When** rendering date values, **Then** format shows as DD/MM/YYYY (BR-090)
7. **Given** time displays, **When** rendering time values, **Then** format shows as HH:MM:SS (BR-091)
8. **Given** all UI text, **When** rendering any label or message, **Then** displays in Portuguese (BR-088)

---

### User Story 5 - Implement Comprehensive Testing (Priority: P3)

System must include unit tests for all services, integration tests for workflows, E2E tests for complete user journeys, and performance tests for targets.

**Why this priority**: Essential for quality assurance and regression prevention but can be implemented incrementally alongside features.

**Independent Test**: Can be tested by running test suite with dotnet test and verifying all tests pass with adequate coverage (>80% for critical paths).

**Acceptance Scenarios**:

1. **Given** unit test suite, **When** executing tests, **Then** all 100+ business rules have corresponding test cases that pass
2. **Given** integration tests, **When** testing payment authorization workflow, **Then** tests verify complete 8-step pipeline with external service mocks
3. **Given** E2E tests, **When** executing complete user journey, **Then** tests verify search, authorization, history viewing, and phase updates work end-to-end
4. **Given** performance tests, **When** measuring claim search, **Then** response time less than 3 seconds for all search modes (BR-096)
5. **Given** performance tests, **When** measuring payment authorization cycle, **Then** end-to-end time less than 90 seconds including external validation (BR-097)
6. **Given** performance tests, **When** querying history with 1000+ records, **Then** response time less than 500ms with pagination (BR-098)
7. **Given** test execution, **When** running dotnet test, **Then** code coverage report shows greater than 80% coverage for Core and Infrastructure layers

---

### User Story 6 - Database Schema and Index Optimization (Priority: P4)

System must implement recommended database indexes for performance, verify all foreign key constraints exist, and ensure currency rate lookup optimizations meet performance targets.

**Why this priority**: Important for performance but not blocking core functionality. Can be implemented after basic features work.

**Independent Test**: Can be tested by executing performance benchmark queries and verifying response times meet targets with proper index usage shown in execution plans.

**Acceptance Scenarios**:

1. **Given** THISTSIN table with 10,000+ records, **When** querying claim history, **Then** system uses covering index on composite key for sub-500ms response
2. **Given** TGEUNIMO currency rate lookup, **When** querying by date range, **Then** system uses index on date columns for instant retrieval
3. **Given** SI_SINISTRO_FASE queries, **When** searching open phases, **Then** system efficiently filters open phase marker using index
4. **Given** all entity configurations, **When** validating schema, **Then** all foreign key relationships exist with proper cascade behavior
5. **Given** concurrent claim operations, **When** multiple users update same claim, **Then** optimistic concurrency handling prevents data loss

---

### Edge Cases

- What happens when VLCRUZAD currency rate not found for transaction date (DTMOVABE)?
- How does system handle OCORHIST counter reaching maximum integer value?
- What happens when external validation service returns unknown EZERT8 code not documented?
- How does system handle special characters in beneficiary name (UTF-8 encoding, SQL injection prevention)?
- What happens when TSISTEMA table has multiple records or IDSISTEM='SI' record missing?
- How does system handle timezone differences between DTMOVABE business date and HORAOPER system time?
- What happens when EF_CONTR_SEG_HABIT consortium lookup returns multiple contracts for same policy?
- How does system handle phase configuration gaps in SI_REL_FASE_EVENTO for event code 1098?
- What happens when circuit breaker is open and operator needs immediate payment authorization?
- How does system handle database connection loss mid-transaction during 8-step payment pipeline?
- What happens when beneficiary name exceeds 255 character limit (BR-021)?
- How does system handle null or negative values in SDOPAG (reserve) or TOTPAG (payments)?

## Requirements

### Functional Requirements

#### External Service Integration

- **FR-001**: System MUST implement CNOUA REST API client for consortium products 6814, 7701, 7709 with HTTPS, JSON payload, 10-second timeout
- **FR-002**: System MUST implement SIPUA SOAP client for EFP contract validation when NUM_CONTRATO > 0 in EF_CONTR_SEG_HABIT
- **FR-003**: System MUST implement SIMDA SOAP client for HB contract validation when NUM_CONTRATO = 0 or not found
- **FR-004**: System MUST apply Polly resilience policies: 3 retries with exponential backoff, circuit breaker (5 failures open for 30s)
- **FR-005**: System MUST parse EZERT8 response code from all validation services, proceeding only when code equals '00000000'
- **FR-006**: System MUST display service-specific error messages for validation failures: CONS-001 through CONS-005
- **FR-007**: System MUST log all external service calls with request and response payloads, timestamps, and response codes for audit trail

#### Transaction Atomicity and Rollback

- **FR-008**: System MUST execute payment authorization as single database transaction with isolation level ReadCommitted
- **FR-009**: System MUST insert THISTSIN record with all fields: operation code 1098, DTMOVABE date, HORAOPER time, operator ID, amounts in BTNF currency
- **FR-010**: System MUST update TMESTSIN claim master incrementing TOTPAG by payment amount, incrementing OCORHIST occurrence counter by 1
- **FR-011**: System MUST insert SI_ACOMPANHA_SINI event record with COD_EVENTO equals 1098, DATA_MOVTO, operator ID, description
- **FR-012**: System MUST query SI_REL_FASE_EVENTO for event 1098 phase relationships and update SI_SINISTRO_FASE accordingly
- **FR-013**: System MUST rollback entire transaction if any of 4 operations fail, with explicit error logging indicating failed operation
- **FR-014**: System MUST verify transaction commit success before returning payment authorization confirmation to user

#### Business Rules Enforcement (Critical Subset)

- **FR-015**: System MUST validate beneficiary required when TPSEGU not equal to 0, optional when TPSEGU equal to 0, displaying VAL-007 error if missing
- **FR-016**: System MUST validate payment amount less than or equal to (SDOPAG - TOTPAG) pending value, displaying VAL-005 error if exceeded
- **FR-017**: System MUST apply currency conversion formula VALPRIBT equals VALPRI multiplied by VLCRUZAD with 8-decimal rate precision, 2-decimal result precision
- **FR-018**: System MUST query TGEUNIMO for VLCRUZAD rate where DTINIVIG less than or equal to DTMOVABE less than or equal to DTTERVIG, displaying VAL-008 error if no rate found
- **FR-019**: System MUST always record operation code 1098 for payment authorization transactions, never any other code
- **FR-020**: System MUST always record correction type '5' for all payment authorizations, never any other type
- **FR-021**: System MUST query TSISTEMA table with IDSISTEM equals 'SI' for DTMOVABE business date, never using system clock for transaction date
- **FR-022**: System MUST open phases with DATA_FECHA_SIFA equals '9999-12-31' when IND_ALTERACAO_FASE equals '1' in SI_REL_FASE_EVENTO
- **FR-023**: System MUST close phases by updating DATA_FECHA_SIFA to current DTMOVABE when IND_ALTERACAO_FASE equals '2'
- **FR-024**: System MUST prevent duplicate open phases for same FONTE, PROTSINI, DAC, COD_FASE combination (reject if open phase exists)
- **FR-025**: System MUST record operator user ID (EZEUSRID) on all THISTSIN history, SI_ACOMPANHA_SINI events, SI_SINISTRO_FASE phase records

#### UI/Display and Styling

- **FR-026**: System MUST apply Site.css stylesheet exactly as provided without any modifications to styling rules
- **FR-027**: System MUST display Caixa Seguradora logo from base64-encoded PNG image in application header
- **FR-028**: System MUST display all validation and system error messages in Portuguese from ErrorMessages.pt-BR resource file
- **FR-029**: System MUST render error messages in red color with appropriate icon
- **FR-030**: System MUST format all monetary amounts with thousands separator and exactly 2 decimal places
- **FR-031**: System MUST format all dates as DD/MM/YYYY for display, storing as YYYY-MM-DD in database
- **FR-032**: System MUST format all times as HH:MM:SS in 24-hour format
- **FR-033**: System MUST support responsive layout for mobile devices with max-width 850px, maintaining full functionality

#### Testing and Quality Assurance

- **FR-034**: System MUST include unit tests for all 100+ business rules with test IDs matching rule IDs (BR-001 through BR-099)
- **FR-035**: System MUST include integration tests for complete payment authorization pipeline with mocked external services
- **FR-036**: System MUST include E2E tests covering search, authorization, history viewing, phase updates user journeys
- **FR-037**: System MUST include performance tests verifying search less than 3 seconds, payment authorization less than 90 seconds, history query less than 500ms targets
- **FR-038**: System MUST achieve minimum 80% code coverage for Core and Infrastructure layers in unit tests

#### Database Schema and Performance

- **FR-039**: System MUST implement covering index on THISTSIN for composite primary key to optimize history queries
- **FR-040**: System MUST implement index on TGEUNIMO date range columns for currency rate date range lookups
- **FR-041**: System MUST implement index on SI_SINISTRO_FASE including open phase marker column for open phase queries
- **FR-042**: System MUST verify all foreign key constraints exist with appropriate cascade behavior (restrict or cascade based on business rules)
- **FR-043**: System MUST implement optimistic concurrency handling using row versioning or timestamp for concurrent claim updates

### Key Entities

All 13 entities already defined in existing codebase with complete EF Core configurations:

- **ClaimMaster (TMESTSIN)**: Main claim record with 4-part composite primary key (TIPSEG, ORGSIN, RMOSIN, NUMSIN), financial summary fields
- **ClaimHistory (THISTSIN)**: Payment authorization transactions with 5-part composite primary key including OCORHIST sequence
- **BranchMaster (TGERAMO)**: Branch information for claim organization lookup
- **PolicyMaster (TAPOLICE)**: Insured party information with 3-part composite primary key
- **CurrencyUnit (TGEUNIMO)**: Currency conversion rates with date range validity (DTINIVIG, DTTERVIG)
- **SystemControl (TSISTEMA)**: Current business date (DTMOVABE) with IDSISTEM equals 'SI'
- **ClaimAccompaniment (SI_ACOMPANHA_SINI)**: Event history with event codes and operator tracking
- **ClaimPhase (SI_SINISTRO_FASE)**: Processing phase tracking with open and close indicators
- **PhaseEventRelationship (SI_REL_FASE_EVENTO)**: Phase configuration defining automatic phase changes per event
- **ConsortiumContract (EF_CONTR_SEG_HABIT)**: Consortium contract lookup for validation routing
- **MigrationStatus**: Project progress tracking for dashboard (new entity for migration tracking)
- **ComponentMigration**: Component-level migration status tracking (new entity for dashboard)
- **PerformanceMetric**: Benchmarking data comparing legacy versus new system (new entity for dashboard)

## Success Criteria

### Measurable Outcomes

- **SC-001**: All 100% of business rules (BR-001 through BR-099) implemented with passing unit tests demonstrating correct behavior
- **SC-002**: External service integration complete with successful CNOUA, SIPUA, SIMDA validation responses for test scenarios
- **SC-003**: Transaction atomicity verified by rollback tests showing zero partial updates under failure conditions
- **SC-004**: Claim search operations complete in under 3 seconds for all three search modes (protocol, claim number, leader code) with 1000+ claims
- **SC-005**: Payment authorization cycle completes in under 90 seconds end-to-end including external validation with network latency
- **SC-006**: History query with 1000+ payment records completes in under 500ms with pagination enabled
- **SC-007**: Test suite achieves minimum 80% code coverage for Core and Infrastructure layers with all tests passing
- **SC-008**: UI matches Site.css styling exactly with Caixa Seguradora logo visible and responsive design functional on mobile devices
- **SC-009**: All error messages display in Portuguese with red color and appropriate business rule validation enforcement
- **SC-010**: Zero data integrity violations when running concurrent payment authorization operations on same claim
