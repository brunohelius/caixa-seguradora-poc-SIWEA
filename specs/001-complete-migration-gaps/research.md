# Research: Complete SIWEA Migration - Gaps Analysis and Implementation

**Feature**: Complete 100% SIWEA Migration
**Branch**: 001-complete-migration-gaps
**Date**: 2025-10-27

## Overview

This research document consolidates technical decisions for implementing the remaining 35-40% of SIWEA migration gaps. All decisions leverage existing architectural patterns from the 60-65% completed codebase.

## Research Areas

### 1. External Service Integration Architecture

**Decision**: Implement dedicated validation client per service (CNOUA, SIPUA, SIMDA) with Polly resilience policies

**Rationale**:
- **Separation of Concerns**: Each validation service has distinct protocol (REST vs SOAP), request/response formats, and error codes
- **Resilience Requirements**: FR-004 specifies 3 retries with exponential backoff, circuit breaker pattern (5 failures → 30s open), 10-second timeout per call
- **Existing Pattern**: IExternalValidationService interface already exists in Core layer, infrastructure clients implement this interface
- **Testability**: Independent clients allow unit testing with mocked HTTP/SOAP responses, integration testing with WireMock stubs

**Alternatives Considered**:
- **Single Generic External Service Client**: Rejected because CNOUA (REST/JSON), SIPUA (SOAP 1.2), and SIMDA (SOAP 1.2) have incompatible protocols and response structures. Generic client would require complex conditional logic violating Single Responsibility Principle.
- **Direct HttpClient Usage Without Polly**: Rejected because FR-004 mandates specific resilience patterns (retry count, backoff timing, circuit breaker thresholds). Polly provides battle-tested implementations meeting these requirements.

**Implementation Pattern**:
```text
Infrastructure/ExternalServices/
├── CnouaValidationClient.cs (REST + JSON, products 6814/7701/7709)
├── SipuaValidationClient.cs (SOAP 1.2, EFP contracts where NUM_CONTRATO > 0)
└── SimdaValidationClient.cs (SOAP 1.2, HB contracts where NUM_CONTRATO = 0)

Each client implements:
- IExternalValidationService interface from Core
- Polly policies: Retry(3, exponential backoff 2s/4s/8s), CircuitBreaker(5 failures, 30s break), Timeout(10s)
- Structured logging: request payload, response code (EZERT8), elapsed time
- Error mapping: EZERT8 codes → CONS-001 through CONS-005 Portuguese error messages
```

---

### 2. Transaction Atomicity and Rollback Strategy

**Decision**: Use EF Core TransactionScope with explicit BeginTransaction/Commit/Rollback for 4-table payment authorization pipeline

**Rationale**:
- **ACID Compliance**: FR-008 requires ReadCommitted isolation level ensuring dirty read prevention while allowing concurrent reads
- **Atomicity Requirement**: FR-013 mandates all-or-nothing behavior for 4 operations: (1) Insert THISTSIN history, (2) Update TMESTSIN claim master, (3) Insert SI_ACOMPANHA_SINI event, (4) Update SI_SINISTRO_FASE phases
- **Existing Infrastructure**: UnitOfWork pattern already implemented with SaveChangesAsync, extending with transaction management maintains consistency
- **Error Diagnostics**: Explicit rollback with logging identifies exact failed operation for troubleshooting (FR-013 requirement)

**Alternatives Considered**:
- **TransactionScope with Ambient Transactions**: Rejected because requires MSDTC (Microsoft Distributed Transaction Coordinator) for distributed transactions, adds infrastructure complexity unnecessary for single-database operations. Explicit EF Core transactions simpler and equally robust.
- **Database-Level Triggers**: Rejected because violates database-first constraint - cannot modify legacy schema. Also reduces testability and violates Clean Architecture (business logic belongs in application layer, not database layer).
- **Eventual Consistency with Saga Pattern**: Rejected because system requires strong consistency per BR-067. Insurance claims cannot tolerate partial updates (e.g., history created but claim master not updated) even temporarily.

**Implementation Pattern**:
```text
PaymentAuthorizationService.AuthorizePaymentAsync():
  using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
  try {
    // Step 1: Insert THISTSIN (operation 1098, DTMOVABE, HORAOPER, EZEUSRID)
    var history = CreateHistoryRecord(request);
    await _unitOfWork.ClaimHistories.AddAsync(history);
    await _unitOfWork.SaveChangesAsync();

    // Step 2: Update TMESTSIN (increment TOTPAG, increment OCORHIST)
    claim.Totpag += request.ValorPrincipal;
    claim.Ocorhist++;
    await _unitOfWork.SaveChangesAsync();

    // Step 3: Insert SI_ACOMPANHA_SINI (COD_EVENTO=1098, operator ID)
    var event = CreateAccompanimentEvent(request);
    await _unitOfWork.ClaimAccompaniments.AddAsync(event);
    await _unitOfWork.SaveChangesAsync();

    // Step 4: Update SI_SINISTRO_FASE (query SI_REL_FASE_EVENTO, open/close phases)
    await _phaseManagementService.ProcessPhaseChanges(request, transaction);
    await _unitOfWork.SaveChangesAsync();

    await transaction.CommitAsync();
    _logger.LogInformation("Payment authorization transaction committed: {AuthId}", authId);
  }
  catch (Exception ex) {
    await transaction.RollbackAsync();
    _logger.LogError(ex, "Transaction rollback triggered at operation: {Operation}", GetFailedOperation(ex));
    throw;
  }
```

---

### 3. Business Rules Enforcement Strategy

**Decision**: Implement FluentValidation validators for each business rule category with dedicated validator classes

**Rationale**:
- **Existing Pattern**: PaymentAuthorizationValidator and ClaimSearchValidator already use FluentValidation, extending this pattern maintains consistency
- **Testability**: Each business rule becomes independently testable unit (BR-001 through BR-099) with clear Given-When-Then scenarios
- **Error Message Integration**: FluentValidation integrates with ErrorMessages.pt-BR resource file for Portuguese error messages (VAL-001 through VAL-008, CONS-001 through CONS-005, SYS-001 through SYS-005)
- **Composability**: Complex validation scenarios compose multiple rule validators (e.g., PaymentAuthorizationValidator uses CurrencyConversionRulesValidator + BeneficiaryRulesValidator)

**Alternatives Considered**:
- **Domain Entity Validation Methods**: Rejected because entities in current codebase are anemic (DTOs with properties only), adding validation logic would violate existing pattern and require entity refactoring
- **Middleware Validation Pipeline**: Rejected because validation logic varies by operation (search vs authorization vs history) and consolidating in single middleware increases coupling
- **Manual If-Then Checks in Services**: Rejected because reduces testability, creates code duplication across services, and makes business rule traceability difficult (can't easily map BR-019 to validation code)

**Implementation Pattern**:
```text
Core/Validators/
├── BusinessRules/
│   ├── SearchValidationRules.cs (BR-001 through BR-009)
│   ├── PaymentValidationRules.cs (BR-010 through BR-026)
│   ├── CurrencyConversionRules.cs (BR-027 through BR-037)
│   ├── TransactionRecordingRules.cs (BR-038 through BR-046)
│   ├── ProductValidationRules.cs (BR-047 through BR-060)
│   ├── PhaseManagementRules.cs (BR-061 through BR-070)
│   ├── AuditTrailRules.cs (BR-071 through BR-074)
│   ├── DataValidationRules.cs (BR-075 through BR-087)
│   ├── UiDisplayRules.cs (BR-088 through BR-095)
│   └── PerformanceRules.cs (BR-096 through BR-099)

Example: BeneficiaryRules.cs
public class BeneficiaryRules : AbstractValidator<PaymentAuthorizationRequest> {
  public BeneficiaryRules() {
    // BR-019: Beneficiary required if TPSEGU != 0
    RuleFor(x => x.Beneficiario)
      .NotEmpty()
      .When(x => x.TipoSeguro != 0)
      .WithMessage(ErrorMessages.VAL_007) // "Favorecido é obrigatório para este tipo de seguro"
      .WithErrorCode("BR-019");

    // BR-021: Max 255 characters
    RuleFor(x => x.Beneficiario)
      .MaximumLength(255)
      .WithMessage("Beneficiário excede 255 caracteres")
      .WithErrorCode("BR-021");
  }
}
```

---

### 4. UI/Display Integration with Site.css

**Decision**: Apply Site.css as global stylesheet without modifications, use CSS modules for component-specific styling

**Rationale**:
- **Constraint Compliance**: FR-026 mandates Site.css applied exactly as provided without modifications
- **Responsive Design**: Site.css already contains media queries for max-width 850px mobile responsiveness (BR-095), no additional CSS framework needed
- **Existing React Components**: Logo, CurrencyInput, dashboard components already exist and work with Site.css
- **Error Message Styling**: ErrorMessages.pt-BR already contains Portuguese text, CSS applies red color (#e80c4d per BR-092) via .error class in Site.css

**Alternatives Considered**:
- **Tailwind CSS or CSS-in-JS**: Rejected because FR-026 prohibits modifications to Site.css and adding competing CSS framework creates styling conflicts and increases bundle size
- **Site.css Modification for Modern Layout**: Rejected because violates hard constraint FR-026. Legacy stylesheet must be preserved exactly for consistency with existing insurance company branding
- **Separate Modern Stylesheet**: Rejected because creates maintenance burden (two stylesheets), CSS specificity conflicts, and violates "preserve Site.css" constraint intent

**Implementation Pattern**:
```text
frontend/public/Site.css (✅ exists, preserved exactly)
frontend/src/App.tsx:
  import '../public/Site.css'; // Global import

Component-specific styling (when needed beyond Site.css):
  frontend/src/components/claims/PaymentForm.module.css
  Uses CSS modules to avoid Site.css conflicts

Error display:
  <div className="error">{errorMessages.VAL_007}</div>
  // .error class from Site.css applies red color #e80c4d
```

---

### 5. Comprehensive Testing Strategy

**Decision**: Three-tier testing approach: Unit (xUnit), Integration (TestServer), E2E (Playwright)

**Rationale**:
- **Existing Infrastructure**: xUnit already configured for tests, extending with integration and E2E tests maintains tooling consistency
- **Business Rule Coverage**: FR-034 requires unit test per business rule (BR-001 through BR-099), xUnit with [Theory] and [InlineData] provides efficient parameterized testing
- **Performance Verification**: FR-037 mandates performance tests for < 3s search, < 90s authorization, < 500ms history - xUnit with Stopwatch assertions validates these targets
- **External Service Mocking**: TestServer with WebApplicationFactory provides in-memory API for integration tests, WireMock stubs external CNOUA/SIPUA/SIMDA services

**Alternatives Considered**:
- **BDD Framework (SpecFlow)**: Rejected because adds dependency overhead and team already familiar with xUnit. Given-When-Then scenarios can be expressed clearly in xUnit test names and AAA (Arrange-Act-Assert) pattern.
- **Load Testing with K6 or JMeter**: Rejected for initial implementation because FR-037 focuses on response time targets, not throughput. Load testing deferred to post-100% migration performance tuning phase.
- **Integration Tests with Real External Services**: Rejected because external services (CNOUA/SIPUA/SIMDA) unavailable in test environments, have rate limits, and introduce test fragility. WireMock provides deterministic stubs with configurable responses.

**Implementation Pattern**:
```text
tests/CaixaSeguradora.Core.Tests/BusinessRules/
├── BR001_SearchByProtocolTests.cs (RuleFor: protocol search)
├── BR019_BeneficiaryRequirementTests.cs (RuleFor: TPSEGU != 0 → beneficiary required)
├── BR023_CurrencyConversionTests.cs (RuleFor: VALPRIBT = VALPRI × VLCRUZAD)
└── ... (100+ rule test files)

tests/CaixaSeguradora.Api.Tests/Integration/
├── PaymentAuthorizationIntegrationTests.cs (TestServer, mocked external services)
├── TransactionRollbackTests.cs (verify 4-table atomicity)
└── PerformanceTests.cs (measure search < 3s, authorization < 90s)

tests/E2E/ (Playwright)
├── ClaimSearchJourney.spec.ts (end-to-end search → authorize → confirm)
├── ErrorHandlingJourney.spec.ts (validation errors, rollback scenarios)
└── MobileResponsiveTests.spec.ts (verify 850px layout)

Example Unit Test:
[Theory]
[InlineData(0, null, true)]  // TPSEGU=0, no beneficiary → valid
[InlineData(1, "Joao Silva", true)]  // TPSEGU=1, has beneficiary → valid
[InlineData(1, null, false)] // TPSEGU=1, no beneficiary → invalid (BR-019)
public async Task BR019_BeneficiaryRequired_WhenTpSeguNotZero(
  int tpSegu, string beneficiario, bool expectedValid) {
  // Arrange
  var request = new PaymentAuthorizationRequest {
    TipoSeguro = tpSegu,
    Beneficiario = beneficiario,
    ...
  };
  var validator = new BeneficiaryRules();

  // Act
  var result = await validator.ValidateAsync(request);

  // Assert
  Assert.Equal(expectedValid, result.IsValid);
  if (!expectedValid) {
    Assert.Contains(result.Errors, e => e.ErrorCode == "BR-019");
  }
}
```

---

## Research Decisions Summary

| Area | Decision | Key Technology | Justification |
|------|----------|---------------|---------------|
| External Services | Dedicated client per service | Polly 8.2.0 resilience | Separation of concerns, protocol differences (REST vs SOAP), independent testability |
| Transaction Atomicity | EF Core explicit transactions | TransactionScope ReadCommitted | ACID compliance, existing UnitOfWork pattern, explicit rollback diagnostics |
| Business Rules | FluentValidation validators | FluentValidation 11.9.0 | Existing pattern consistency, independent testability, Portuguese error messages |
| UI Integration | Site.css as-is, CSS modules | Site.css (legacy), CSS Modules | Hard constraint compliance, no modifications permitted, responsive already included |
| Testing Strategy | Three-tier: Unit + Integration + E2E | xUnit + TestServer + Playwright | Existing xUnit infrastructure, external service mocking, performance measurement |

## Implementation Order

Based on research findings and feature dependencies:

**Phase 1 (P1 - System Critical)**:
1. External service validation clients (CNOUA, SIPUA, SIMDA) with Polly policies
2. Transaction atomicity implementation in PaymentAuthorizationService
3. Unit tests for external services and transaction rollback

**Phase 2 (P2 - Business Critical)**:
4. Business rules validators (100+ rules across 10 categories)
5. Integration tests for complete payment authorization pipeline
6. Unit tests for all business rules (BR-001 through BR-099)

**Phase 3 (P3 - Operational)**:
7. UI/Display enhancements (Site.css integration, Portuguese errors, mobile responsive)
8. E2E tests for user journeys
9. Performance tests validating < 3s search, < 90s authorization, < 500ms history

**Phase 4 (P4 - Performance)**:
10. Database index optimization (covering index on THISTSIN, date range index on TGEUNIMO)
11. Optimistic concurrency handling for concurrent claim updates
12. Performance tuning and query optimization

## Risk Mitigation

**External Service Unavailability**:
- **Risk**: CNOUA/SIPUA/SIMDA services down or returning errors
- **Mitigation**: Circuit breaker pattern (5 failures → 30s break), health checks, fallback error messages (SYS-005 "Serviço de validação indisponível")

**Transaction Deadlocks**:
- **Risk**: Concurrent updates to same claim causing database deadlock
- **Mitigation**: ReadCommitted isolation level (allows concurrent reads), exponential backoff retry on deadlock detection (SQL error 1205), optimistic concurrency with row versioning

**Performance Degradation**:
- **Risk**: Search > 3s or authorization > 90s violating performance targets
- **Mitigation**: Covering indexes on THISTSIN composite key, TGEUNIMO date range index, query plan analysis, EF Core compiled queries for hot paths

**Test Environment Gaps**:
- **Risk**: External services unavailable in test environment
- **Mitigation**: WireMock stubs with configurable responses, contract testing verifying request/response formats match documentation

## Dependencies

**External**:
- CNOUA REST API endpoint (product validation for 6814, 7701, 7709)
- SIPUA SOAP 1.2 endpoint (EFP contract validation)
- SIMDA SOAP 1.2 endpoint (HB contract validation)
- Service contracts/WSDLs for request/response format documentation

**Internal**:
- ErrorMessages.pt-BR resource file with all 24 error messages (VAL-001 through VAL-008, CONS-001 through CONS-005, SYS-001 through SYS-005)
- TGEUNIMO currency rates populated with VLCRUZAD conversion rates for all transaction dates
- SI_REL_FASE_EVENTO configuration complete for event code 1098 phase relationships
- TSISTEMA table with IDSISTEM='SI' record containing current DTMOVABE business date

## Open Questions

None - all technical decisions resolved through research. Implementation can proceed with Phase 1.

---

**Research Status**: COMPLETE
**Next Phase**: Phase 1 - Design data model and contracts
**Approval**: Ready for implementation
