# Tasks: Complete SIWEA Migration - Gaps Analysis and Implementation

**Input**: Design documents from `/specs/001-complete-migration-gaps/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: User Story 5 explicitly requests comprehensive testing - unit tests for business rules, integration tests for workflows, E2E tests for user journeys

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story. Tests integrated within each story phase (not separate phase) for TDD approach.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4, US5, US6)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `backend/src/CaixaSeguradora.{Layer}/`
- **Frontend**: `frontend/src/`
- **Tests**: `backend/tests/CaixaSeguradora.{Layer}.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure (already complete)

- [x] T001 Backend project structure with Clean Architecture (.NET 9.0) - **COMPLETE**
- [x] T002 Frontend project structure (React 19 + TypeScript) - **COMPLETE**
- [x] T003 [P] Database entities and EF Core configurations (13 entities) - **COMPLETE**
- [x] T004 [P] Repository pattern infrastructure (UnitOfWork, ClaimRepository) - **COMPLETE**
- [x] T005 [P] Basic DTOs and AutoMapper profiles - **COMPLETE**

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before user story gap implementations

**⚠️ CRITICAL**: External service integration and transaction management must be complete before payment authorization gaps can be filled

- [ ] T006 Create DTOs in backend/src/CaixaSeguradora.Core/DTOs/ExternalValidationRequest.cs
- [ ] T007 [P] Create DTOs in backend/src/CaixaSeguradora.Core/DTOs/ExternalValidationResponse.cs
- [ ] T008 [P] Create DTOs in backend/src/CaixaSeguradora.Core/DTOs/TransactionContext.cs
- [ ] T009 [P] Create DTOs in backend/src/CaixaSeguradora.Core/DTOs/BusinessRuleViolation.cs
- [ ] T010 [P] Create DTOs in backend/src/CaixaSeguradora.Core/DTOs/PhaseUpdateRequest.cs
- [ ] T011 [P] Create value object in backend/src/CaixaSeguradora.Core/ValueObjects/ClaimKey.cs
- [ ] T012 [P] Create value object in backend/src/CaixaSeguradora.Core/ValueObjects/CurrencyAmount.cs
- [ ] T013 Configure Polly resilience policies in backend/src/CaixaSeguradora.Api/Program.cs (retry, circuit breaker, timeout)
- [ ] T014 Configure external service endpoints in backend/src/CaixaSeguradora.Api/appsettings.Development.json

**Checkpoint**: Foundation ready for user story gap implementations

---

## Phase 3: User Story 1 - Complete External Service Validation Integration (Priority: P1) 🎯

**Goal**: Implement CNOUA, SIPUA, SIMDA validation clients with Polly resilience policies for consortium product and contract validation

**Independent Test**: Authorize payment for product code 6814, verify CNOUA service call, validate response handling (success EZERT8='00000000' or error codes), confirm retry/circuit breaker behavior

### Implementation for User Story 1

- [ ] T015 [P] [US1] Create interface in backend/src/CaixaSeguradora.Core/Interfaces/ICnouaValidationClient.cs
- [ ] T016 [P] [US1] Create interface in backend/src/CaixaSeguradora.Core/Interfaces/ISipuaValidationClient.cs
- [ ] T017 [P] [US1] Create interface in backend/src/CaixaSeguradora.Core/Interfaces/ISimdaValidationClient.cs
- [ ] T018 [US1] Implement CNOUA REST client in backend/src/CaixaSeguradora.Infrastructure/ExternalServices/CnouaValidationClient.cs with Polly policies
- [ ] T019 [US1] Implement SIPUA SOAP client in backend/src/CaixaSeguradora.Infrastructure/ExternalServices/SipuaValidationClient.cs with Polly policies
- [ ] T020 [US1] Implement SIMDA SOAP client in backend/src/CaixaSeguradora.Infrastructure/ExternalServices/SimdaValidationClient.cs with Polly policies
- [ ] T021 [US1] Register external service clients in DI container in backend/src/CaixaSeguradora.Api/Program.cs with HttpClientFactory
- [ ] T022 [US1] Implement external service routing logic in backend/src/CaixaSeguradora.Infrastructure/Services/ExternalServiceRouter.cs (CNOUA for 6814/7701/7709, SIPUA for NUM_CONTRATO>0, SIMDA for NUM_CONTRATO=0)
- [ ] T023 [US1] Add error code mapping (EZERT8 → CONS-001 through CONS-005) in backend/src/CaixaSeguradora.Core/Resources/ErrorMessages.pt-BR.resx
- [ ] T024 [US1] Add structured logging for external service calls (request/response payloads, timestamps, EZERT8 codes) in all validation clients

### Tests for User Story 1 (Unit + Integration)

- [ ] T025 [P] [US1] Unit test for CnouaValidationClient success scenario in backend/tests/CaixaSeguradora.Infrastructure.Tests/ExternalServices/CnouaValidationClientTests.cs
- [ ] T026 [P] [US1] Unit test for CnouaValidationClient timeout with retry in backend/tests/CaixaSeguradora.Infrastructure.Tests/ExternalServices/CnouaValidationClientTests.cs
- [ ] T027 [P] [US1] Unit test for circuit breaker behavior (5 failures → 30s break) in backend/tests/CaixaSeguradora.Infrastructure.Tests/ExternalServices/CnouaValidationClientTests.cs
- [ ] T028 [P] [US1] Unit test for SipuaValidationClient SOAP request/response in backend/tests/CaixaSeguradora.Infrastructure.Tests/ExternalServices/SipuaValidationClientTests.cs
- [ ] T029 [P] [US1] Unit test for SimdaValidationClient SOAP request/response in backend/tests/CaixaSeguradora.Infrastructure.Tests/ExternalServices/SimdaValidationClientTests.cs
- [ ] T030 [P] [US1] Unit test for ExternalServiceRouter routing logic (product code → service selection) in backend/tests/CaixaSeguradora.Infrastructure.Tests/Services/ExternalServiceRouterTests.cs
- [ ] T031 [US1] Integration test for payment authorization with CNOUA validation (mocked with WireMock) in backend/tests/CaixaSeguradora.Api.Tests/Integration/ExternalValidationIntegrationTests.cs

**Checkpoint**: User Story 1 complete - external service validation clients operational with resilience policies

---

## Phase 4: User Story 2 - Implement Complete Transaction Atomicity (Priority: P1) 🎯

**Goal**: Implement 4-step atomic transaction pipeline (THISTSIN insert, TMESTSIN update, SI_ACOMPANHA_SINI insert, SI_SINISTRO_FASE update) with complete rollback on any failure

**Independent Test**: Simulate phase update failure during payment authorization, verify all 4 operations rollback (no history record, no claim master update, no event record, no phase changes), verify SYS-004 error logged

### Implementation for User Story 2

- [ ] T032 [US2] Create TransactionCoordinator service in backend/src/CaixaSeguradora.Infrastructure/Services/TransactionCoordinator.cs with BeginTransaction/Commit/Rollback logic
- [ ] T033 [US2] Enhance PaymentAuthorizationService in backend/src/CaixaSeguradora.Infrastructure/Services/PaymentAuthorizationService.cs to use TransactionCoordinator for 4-step pipeline
- [ ] T034 [US2] Implement Step 1: Insert THISTSIN history record in PaymentAuthorizationService.AuthorizePaymentAsync (operation 1098, DTMOVABE, HORAOPER, EZEUSRID)
- [ ] T035 [US2] Implement Step 2: Update TMESTSIN claim master (increment TOTPAG, increment OCORHIST) in PaymentAuthorizationService.AuthorizePaymentAsync
- [ ] T036 [US2] Implement Step 3: Insert SI_ACOMPANHA_SINI event record (COD_EVENTO=1098) in PaymentAuthorizationService.AuthorizePaymentAsync
- [ ] T037 [US2] Implement Step 4: Update SI_SINISTRO_FASE phases via PhaseManagementService in PaymentAuthorizationService.AuthorizePaymentAsync
- [ ] T038 [US2] Add transaction rollback exception handling with step identification logging in TransactionCoordinator
- [ ] T039 [US2] Configure ReadCommitted isolation level for transaction in TransactionCoordinator
- [ ] T040 [US2] Add TransactionContext tracking (AuthorizationId, Step enum, RollbackReason) throughout 4-step pipeline

### Tests for User Story 2 (Unit + Integration)

- [ ] T041 [P] [US2] Unit test for TransactionCoordinator commit success path in backend/tests/CaixaSeguradora.Infrastructure.Tests/Services/TransactionCoordinatorTests.cs
- [ ] T042 [P] [US2] Unit test for TransactionCoordinator rollback on Step 1 failure in backend/tests/CaixaSeguradora.Infrastructure.Tests/Services/TransactionCoordinatorTests.cs
- [ ] T043 [P] [US2] Unit test for TransactionCoordinator rollback on Step 2 failure in backend/tests/CaixaSeguradora.Infrastructure.Tests/Services/TransactionCoordinatorTests.cs
- [ ] T044 [P] [US2] Unit test for TransactionCoordinator rollback on Step 3 failure in backend/tests/CaixaSeguradora.Infrastructure.Tests/Services/TransactionCoordinatorTests.cs
- [ ] T045 [P] [US2] Unit test for TransactionCoordinator rollback on Step 4 failure in backend/tests/CaixaSeguradora.Infrastructure.Tests/Services/TransactionCoordinatorTests.cs
- [ ] T046 [US2] Integration test for complete 4-step transaction success in backend/tests/CaixaSeguradora.Api.Tests/Integration/TransactionAtomicityTests.cs
- [ ] T047 [US2] Integration test for simulated phase update failure triggering complete rollback in backend/tests/CaixaSeguradora.Api.Tests/Integration/TransactionRollbackTests.cs

**Checkpoint**: User Story 2 complete - transaction atomicity enforced across 4-table payment authorization pipeline

---

## Phase 5: User Story 3 - Enforce All 100+ Business Rules (Priority: P2)

**Goal**: Implement FluentValidation validators for all business rules BR-001 through BR-099 across 10 categories with Portuguese error messages

**Independent Test**: Run comprehensive test suite with one test per business rule (100+ tests), verify each rule enforces expected behavior and displays correct Portuguese error message

### Implementation for User Story 3

- [ ] T048 [P] [US3] Create SearchValidationRules validator (BR-001 to BR-009) in backend/src/CaixaSeguradora.Core/Validators/BusinessRules/SearchValidationRules.cs
- [ ] T049 [P] [US3] Create PaymentValidationRules validator (BR-010 to BR-026) in backend/src/CaixaSeguradora.Core/Validators/BusinessRules/PaymentValidationRules.cs
- [ ] T050 [P] [US3] Create CurrencyConversionRules validator (BR-027 to BR-037) in backend/src/CaixaSeguradora.Core/Validators/BusinessRules/CurrencyConversionRules.cs
- [ ] T051 [P] [US3] Create TransactionRecordingRules validator (BR-038 to BR-046) in backend/src/CaixaSeguradora.Core/Validators/BusinessRules/TransactionRecordingRules.cs
- [ ] T052 [P] [US3] Create ProductValidationRules validator (BR-047 to BR-060) in backend/src/CaixaSeguradora.Core/Validators/BusinessRules/ProductValidationRules.cs
- [ ] T053 [P] [US3] Create PhaseManagementRules validator (BR-061 to BR-070) in backend/src/CaixaSeguradora.Core/Validators/BusinessRules/PhaseManagementRules.cs
- [ ] T054 [P] [US3] Create AuditTrailRules validator (BR-071 to BR-074) in backend/src/CaixaSeguradora.Core/Validators/BusinessRules/AuditTrailRules.cs
- [ ] T055 [P] [US3] Create DataValidationRules validator (BR-075 to BR-087) in backend/src/CaixaSeguradora.Core/Validators/BusinessRules/DataValidationRules.cs
- [ ] T056 [P] [US3] Create UiDisplayRules validator (BR-088 to BR-095) in backend/src/CaixaSeguradora.Core/Validators/BusinessRules/UiDisplayRules.cs
- [ ] T057 [P] [US3] Create PerformanceRules validator (BR-096 to BR-099) in backend/src/CaixaSeguradora.Core/Validators/BusinessRules/PerformanceRules.cs
- [ ] T058 [US3] Integrate all business rule validators into PaymentAuthorizationValidator in backend/src/CaixaSeguradora.Core/Validators/PaymentAuthorizationValidator.cs
- [ ] T059 [US3] Add all error messages (VAL-001 to VAL-008, CONS-001 to CONS-005, SYS-001 to SYS-005) to ErrorMessages.pt-BR.resx

### Tests for User Story 3 (Unit - 100+ Tests)

- [ ] T060 [P] [US3] Unit tests for BR-001 to BR-009 (Search rules) in backend/tests/CaixaSeguradora.Core.Tests/BusinessRules/SearchValidationRulesTests.cs
- [ ] T061 [P] [US3] Unit tests for BR-010 to BR-026 (Payment rules) in backend/tests/CaixaSeguradora.Core.Tests/BusinessRules/PaymentValidationRulesTests.cs
- [ ] T062 [P] [US3] Unit tests for BR-027 to BR-037 (Currency rules) in backend/tests/CaixaSeguradora.Core.Tests/BusinessRules/CurrencyConversionRulesTests.cs
- [ ] T063 [P] [US3] Unit tests for BR-038 to BR-046 (Transaction rules) in backend/tests/CaixaSeguradora.Core.Tests/BusinessRules/TransactionRecordingRulesTests.cs
- [ ] T064 [P] [US3] Unit tests for BR-047 to BR-060 (Product rules) in backend/tests/CaixaSeguradora.Core.Tests/BusinessRules/ProductValidationRulesTests.cs
- [ ] T065 [P] [US3] Unit tests for BR-061 to BR-070 (Phase rules) in backend/tests/CaixaSeguradora.Core.Tests/BusinessRules/PhaseManagementRulesTests.cs
- [ ] T066 [P] [US3] Unit tests for BR-071 to BR-074 (Audit rules) in backend/tests/CaixaSeguradora.Core.Tests/BusinessRules/AuditTrailRulesTests.cs
- [ ] T067 [P] [US3] Unit tests for BR-075 to BR-087 (Data validation rules) in backend/tests/CaixaSeguradora.Core.Tests/BusinessRules/DataValidationRulesTests.cs
- [ ] T068 [P] [US3] Unit tests for BR-088 to BR-095 (UI/Display rules) in backend/tests/CaixaSeguradora.Core.Tests/BusinessRules/UiDisplayRulesTests.cs
- [ ] T069 [P] [US3] Unit tests for BR-096 to BR-099 (Performance rules) in backend/tests/CaixaSeguradora.Core.Tests/BusinessRules/PerformanceRulesTests.cs
- [ ] T070 [US3] Verify all 100+ business rule tests pass with correct error codes and Portuguese messages

**Checkpoint**: User Story 3 complete - all 100+ business rules enforced with passing unit tests

---

## Phase 6: User Story 4 - Complete UI/Display Implementation (Priority: P3)

**Goal**: Apply Site.css exactly, display Portuguese error messages in red, show Caixa Seguradora logo, support mobile responsive design (850px)

**Independent Test**: Access application on desktop (1920px) and mobile (375px), verify Site.css applies without modifications, logo displays in header, error messages show in red with Portuguese text, layout remains functional on small screens

### Implementation for User Story 4

- [ ] T071 [US4] Import Site.css as global stylesheet in frontend/src/App.tsx without modifications
- [ ] T072 [P] [US4] Create Logo component displaying Caixa Seguradora logo from base64 PNG in frontend/src/components/common/Logo.tsx
- [ ] T073 [P] [US4] Create ErrorMessage component with red color and Portuguese text in frontend/src/components/common/ErrorMessage.tsx
- [ ] T074 [P] [US4] Create CurrencyDisplay component with format (###,###,###.##) in frontend/src/utils/formatters.ts
- [ ] T075 [P] [US4] Create DateDisplay component with format (DD/MM/YYYY) in frontend/src/utils/formatters.ts
- [ ] T076 [P] [US4] Create TimeDisplay component with format (HH:MM:SS) in frontend/src/utils/formatters.ts
- [ ] T077 [US4] Update PaymentAuthorizationForm to use ErrorMessage component for validation errors in frontend/src/components/claims/PaymentAuthorizationForm.tsx
- [ ] T078 [US4] Add Logo component to header in frontend/src/components/common/Header.tsx
- [ ] T079 [US4] Test mobile responsive layout (max-width 850px) for all pages in frontend/src/App.tsx
- [ ] T080 [US4] Verify all UI text displays in Portuguese (labels, buttons, messages) across all components

### Tests for User Story 4 (E2E)

- [ ] T081 [P] [US4] E2E test for Site.css application and styling in frontend/tests/e2e/ui-display.spec.ts
- [ ] T082 [P] [US4] E2E test for logo display in header in frontend/tests/e2e/ui-display.spec.ts
- [ ] T083 [P] [US4] E2E test for error message display (red color, Portuguese text) in frontend/tests/e2e/ui-display.spec.ts
- [ ] T084 [P] [US4] E2E test for currency format display in frontend/tests/e2e/ui-display.spec.ts
- [ ] T085 [P] [US4] E2E test for date format display (DD/MM/YYYY) in frontend/tests/e2e/ui-display.spec.ts
- [ ] T086 [P] [US4] E2E test for mobile responsive layout (850px width) in frontend/tests/e2e/responsive-design.spec.ts

**Checkpoint**: User Story 4 complete - UI/Display matches requirements with Site.css, Portuguese errors, logo, mobile support

---

## Phase 7: User Story 5 - Implement Comprehensive Testing (Priority: P3)

**Goal**: Complete test infrastructure with unit tests for all services, integration tests for workflows, E2E tests for user journeys, performance tests for targets (<3s search, <90s authorization, <500ms history)

**Independent Test**: Run dotnet test and verify all tests pass with >80% code coverage for Core and Infrastructure layers, run npx playwright test and verify E2E tests pass

### Implementation for User Story 5

- [ ] T087 [US5] Configure xUnit test infrastructure with coverage reporting in backend/tests/CaixaSeguradora.Core.Tests/CaixaSeguradora.Core.Tests.csproj
- [ ] T088 [US5] Configure TestServer for integration tests in backend/tests/CaixaSeguradora.Api.Tests/Integration/TestServerFixture.cs
- [ ] T089 [US5] Configure Playwright for E2E tests in frontend/playwright.config.ts
- [ ] T090 [US5] Create WireMock stubs for CNOUA/SIPUA/SIMDA services in backend/tests/CaixaSeguradora.Api.Tests/Mocks/ExternalServiceStubs.cs
- [ ] T091 [P] [US5] Integration test for complete payment authorization workflow (8-step pipeline) in backend/tests/CaixaSeguradora.Api.Tests/Integration/PaymentAuthorizationIntegrationTests.cs
- [ ] T092 [P] [US5] Integration test for transaction rollback scenarios in backend/tests/CaixaSeguradora.Api.Tests/Integration/TransactionRollbackTests.cs
- [ ] T093 [P] [US5] E2E test for complete user journey (search → authorize → confirm) in frontend/tests/e2e/claim-authorization-journey.spec.ts
- [ ] T094 [P] [US5] E2E test for error handling journey (validation errors, rollback) in frontend/tests/e2e/error-handling-journey.spec.ts
- [ ] T095 [P] [US5] Performance test for claim search (<3 seconds) in backend/tests/CaixaSeguradora.Api.Tests/Performance/SearchPerformanceTests.cs
- [ ] T096 [P] [US5] Performance test for payment authorization (<90 seconds) in backend/tests/CaixaSeguradora.Api.Tests/Performance/AuthorizationPerformanceTests.cs
- [ ] T097 [P] [US5] Performance test for history query (<500ms for 1000+ records) in backend/tests/CaixaSeguradora.Api.Tests/Performance/HistoryPerformanceTests.cs
- [ ] T098 [US5] Generate code coverage report and verify >80% coverage for Core and Infrastructure layers

**Checkpoint**: User Story 5 complete - comprehensive testing infrastructure with unit, integration, E2E, and performance tests

---

## Phase 8: User Story 6 - Database Schema and Index Optimization (Priority: P4)

**Goal**: Implement recommended database indexes for performance, verify foreign key constraints, enable optimistic concurrency handling

**Independent Test**: Execute performance benchmark queries, verify response times meet targets with proper index usage shown in execution plans (THISTSIN <500ms, TGEUNIMO instant, SI_SINISTRO_FASE optimized)

### Implementation for User Story 6

- [ ] T099 [P] [US6] Create covering index on THISTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC) via SQL script in backend/database/migrations/001_create_thistsin_index.sql
- [ ] T100 [P] [US6] Create index on TGEUNIMO (DTINIVIG, DTTERVIG) via SQL script in backend/database/migrations/002_create_tgeunimo_index.sql
- [ ] T101 [P] [US6] Create index on SI_SINISTRO_FASE (FONTE, PROTSINI, DAC, COD_FASE, DATA_FECHA_SIFA) via SQL script in backend/database/migrations/003_create_phase_index.sql
- [ ] T102 [P] [US6] Create index on EF_CONTR_SEG_HABIT (ORGSIN, RMOSIN, NUMSIN) via SQL script in backend/database/migrations/004_create_consortium_index.sql
- [ ] T103 [US6] Verify all foreign key constraints exist with proper cascade behavior via SQL script in backend/database/migrations/005_verify_foreign_keys.sql
- [ ] T104 [US6] Add optimistic concurrency handling (row versioning with RowVersion column) to ClaimMaster entity configuration in backend/src/CaixaSeguradora.Infrastructure/Data/Configurations/ClaimMasterConfiguration.cs
- [ ] T105 [US6] Add deadlock retry logic with exponential backoff to UnitOfWork.SaveChangesAsync in backend/src/CaixaSeguradora.Infrastructure/Repositories/UnitOfWork.cs
- [ ] T106 [P] [US6] Performance test verifying THISTSIN index usage (<500ms for 10,000+ records) in backend/tests/CaixaSeguradora.Api.Tests/Performance/IndexPerformanceTests.cs
- [ ] T107 [P] [US6] Performance test verifying concurrent claim updates with optimistic concurrency in backend/tests/CaixaSeguradora.Api.Tests/Performance/ConcurrencyTests.cs

**Checkpoint**: User Story 6 complete - database optimized with indexes, foreign keys verified, concurrency handling enabled

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements affecting multiple user stories and final validation

- [ ] T108 [P] Update CLAUDE.md with implementation notes and lessons learned
- [ ] T109 [P] Update quickstart.md with final setup instructions and troubleshooting
- [ ] T110 [P] Add comprehensive logging for all critical paths (payment authorization pipeline, external service calls, transaction rollback)
- [ ] T111 Code review and refactoring for code quality across all layers
- [ ] T112 Security audit for authentication, authorization, input validation, SQL injection prevention
- [ ] T113 Performance profiling and optimization across all endpoints
- [ ] T114 Final validation: Run all tests (dotnet test && npx playwright test) and verify 100% pass rate
- [ ] T115 Final validation: Verify all 10 success criteria (SC-001 through SC-010) met

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: COMPLETE - Already 60-65% implemented
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user story gap implementations (T006-T014)
- **User Story 1 (Phase 3)**: Depends on Foundational - External service validation (T015-T031)
- **User Story 2 (Phase 4)**: Depends on Foundational + US1 (external services needed for payment authorization) - Transaction atomicity (T032-T047)
- **User Story 3 (Phase 5)**: Depends on US2 (business rules validate payment authorization logic) - Business rules enforcement (T048-T070)
- **User Story 4 (Phase 6)**: Can start after Foundational - UI/Display (independent of US1-US3) (T071-T086)
- **User Story 5 (Phase 7)**: Can start after US1-US4 complete - Comprehensive testing (T087-T098)
- **User Story 6 (Phase 8)**: Can start after US1-US3 complete - Database optimization (T099-T107)
- **Polish (Phase 9)**: Depends on all user stories complete (T108-T115)

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - No dependencies on other stories
- **User Story 2 (P1)**: Depends on US1 (needs external validation before transaction execution)
- **User Story 3 (P2)**: Depends on US2 (business rules validate transaction logic)
- **User Story 4 (P3)**: Can start after Foundational - Independent of US1-US3 (UI only)
- **User Story 5 (P3)**: Depends on US1-US4 (tests validate all implementations)
- **User Story 6 (P4)**: Can start after US1-US3 - Independent of US4-US5 (database only)

### Critical Path (Minimum to 100% Migration)

1. **Foundational** (T006-T014): 9 tasks - **MUST COMPLETE FIRST**
2. **User Story 1** (T015-T031): 17 tasks - External service validation
3. **User Story 2** (T032-T047): 16 tasks - Transaction atomicity (depends on US1)
4. **User Story 3** (T048-T070): 23 tasks - Business rules enforcement (depends on US2)
5. **User Story 4** (T071-T086): 16 tasks - UI/Display (can parallel with US1-US3)
6. **User Story 5** (T087-T098): 12 tasks - Testing (verifies US1-US4)
7. **User Story 6** (T099-T107): 9 tasks - Database optimization
8. **Polish** (T108-T115): 8 tasks - Final validation

**Total Tasks**: 110 (excluding 5 already complete)
**Critical Path Length**: ~105 tasks (if sequential)

### Parallel Opportunities

**After Foundational Phase (T006-T014) completes:**

- **Track 1 (Backend Critical Path)**: US1 → US2 → US3 (T015-T070) - 56 tasks
- **Track 2 (Frontend)**: US4 (T071-T086) - 16 tasks (can run in parallel with Track 1)
- **Track 3 (Database)**: US6 (T099-T107) - 9 tasks (can start after US1-US3, parallel with US4-US5)

**Within Each User Story:**

- **US1**: T015-T017 (interfaces), T025-T030 (unit tests) can all run in parallel
- **US2**: T041-T045 (unit tests) can all run in parallel
- **US3**: T048-T057 (validators), T060-T069 (tests) can all run in parallel
- **US4**: T072-T076 (components), T081-T086 (E2E tests) can all run in parallel
- **US5**: T091-T097 (all tests) can all run in parallel
- **US6**: T099-T102 (indexes), T106-T107 (tests) can all run in parallel

---

## Parallel Example: User Story 1 (External Service Integration)

```bash
# Launch all interface definitions together:
Task: "Create interface in backend/src/CaixaSeguradora.Core/Interfaces/ICnouaValidationClient.cs"
Task: "Create interface in backend/src/CaixaSeguradora.Core/Interfaces/ISipuaValidationClient.cs"
Task: "Create interface in backend/src/CaixaSeguradora.Core/Interfaces/ISimdaValidationClient.cs"

# Then launch all unit tests together (after implementations complete):
Task: "Unit test for CnouaValidationClient success scenario"
Task: "Unit test for CnouaValidationClient timeout with retry"
Task: "Unit test for circuit breaker behavior"
Task: "Unit test for SipuaValidationClient SOAP request/response"
Task: "Unit test for SimdaValidationClient SOAP request/response"
Task: "Unit test for ExternalServiceRouter routing logic"
```

---

## Implementation Strategy

### MVP First (Critical Path Only)

1. Complete Phase 2: Foundational (T006-T014) - **9 tasks**
2. Complete Phase 3: User Story 1 (T015-T031) - **17 tasks**
3. Complete Phase 4: User Story 2 (T032-T047) - **16 tasks**
4. **STOP and VALIDATE**: Test US1+US2 independently (external services + transactions)
5. Deploy/demo if ready for consortium product validation

### Incremental Delivery

1. **Milestone 1**: Foundational + US1 → External service integration complete (26 tasks)
2. **Milestone 2**: Add US2 → Transaction atomicity complete (42 tasks total)
3. **Milestone 3**: Add US3 → Business rules enforcement complete (65 tasks total)
4. **Milestone 4**: Add US4 → UI/Display complete (81 tasks total)
5. **Milestone 5**: Add US5 → Comprehensive testing complete (93 tasks total)
6. **Milestone 6**: Add US6 → Database optimization complete (102 tasks total)
7. **Milestone 7**: Polish → 100% SIWEA migration complete (110 tasks total)

### Parallel Team Strategy (2-3 Developers)

**After Foundational Phase:**

- **Developer A (Backend Critical)**: US1 → US2 → US3 (T015-T070)
- **Developer B (Frontend)**: US4 (T071-T086) in parallel with Developer A
- **Developer C (Testing + Database)**: US5 after US1-US4, US6 in parallel

**Timeline Estimate**:

- **Foundational**: 1 week (T006-T014)
- **US1**: 2 weeks (T015-T031) - External services
- **US2**: 2 weeks (T032-T047) - Transaction atomicity
- **US3**: 2-3 weeks (T048-T070) - Business rules (100+ tests)
- **US4**: 1-2 weeks (T071-T086) - UI/Display (parallel with US3)
- **US5**: 1 week (T087-T098) - Testing (parallel with US6)
- **US6**: 1 week (T099-T107) - Database optimization
- **Polish**: 3-5 days (T108-T115)

**Total**: ~8-10 weeks with 2-3 developers (sequential critical path ~4-5 weeks for US1+US2+US3)

---

## Notes

- **[P] tasks**: Different files, no dependencies - can run in parallel
- **[Story] label**: Maps task to specific user story for traceability
- **Tests integrated**: Tests within each story phase (not separate), following TDD approach per User Story 5 requirement
- Each user story independently completable and testable
- Verify tests fail before implementing (Red-Green-Refactor)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- **Critical constraint**: Cannot modify existing entity models or database schema (database-first approach)
- **Portuguese language**: All error messages from ErrorMessages.pt-BR.resx
- **Site.css preservation**: Must apply exactly without modifications (BR-094)
