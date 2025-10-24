# Implementation Tasks: Visual Age to .NET 9 + React Migration

**Feature**: 001-visualage-dotnet-migration
**Generated**: 2025-10-23
**Last Updated**: 2025-10-23 (Critical Tasks Sprint)
**Total Tasks**: 147
**Completed Tasks**: 122 ✅
**Remaining Tasks**: 25 ⏳
**Completion Rate**: 83%
**User Stories**: 6 (P1-P6)
**Entities**: 13 (10 legacy + 3 dashboard)
**REST Endpoints**: 9
**SOAP Services**: 3

---

## Recent Updates (2025-10-23)

**Sprint Focus**: Critical Production Readiness Tasks

**Completed This Session** (7 tasks):
- ✅ T089: Enhanced CNOUA validation with EZERT8 error mapping
- ✅ T095: Added comprehensive Swagger product routing documentation
- ✅ T110: Documented phase management workflow in OpenAPI spec
- ✅ T123: Verified MigrationDashboardPage implementation
- ✅ T128: Enhanced ActivitiesTimeline with relative time formatting
- ✅ T129: Enhanced SystemHealthIndicators with refresh button
- ✅ T085: Documented history query performance optimization

**Key Achievements**:
- 10 Portuguese error messages mapped for CNOUA service
- Complete product routing flow documented (CNOUA, SIPUA, SIMDA)
- Professional dashboard components with 30-second auto-refresh
- Performance optimization path defined (82% expected improvement)

**Progress**: 78% → 83% (+5%)

---

## Task Legend

- **[P]**: Task can be executed in parallel with other [P] tasks in the same phase
- **[US#]**: User Story mapping (US1-US6)
- **File paths**: All paths are absolute, starting from `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/`

---

## Executive Summary

This task breakdown covers the complete migration of the Visual Age Claims System (SIWEA) to .NET 9 with React frontend. Tasks are organized by:

1. **Phase 1: Setup** (T001-T010) - Project scaffolding and configuration
2. **Phase 2: Foundational** (T011-T030) - Database, repositories, services foundation
3. **Phase 3: User Story 1** (T031-T050) - Search and retrieve claim functionality [P1]
4. **Phase 4: User Story 2** (T051-T075) - Payment authorization with external services [P2]
5. **Phase 5: User Story 3** (T076-T085) - Payment history viewing [P3]
6. **Phase 6: User Story 4** (T086-T095) - Consortium product handling [P4]
7. **Phase 7: User Story 5** (T096-T110) - Claim phase management [P5]
8. **Phase 8: User Story 6** (T111-T130) - Migration dashboard [P6]
9. **Phase 9: Polish & Cross-Cutting** (T131-T147) - UI polish, security, testing, deployment

---

## MVP Scope Recommendation

**Minimum Viable Product** should include:
- **Phase 1** (Setup): T001-T010
- **Phase 2** (Foundation): T011-T030
- **Phase 3** (US1 - Search/Retrieve): T031-T050

This represents ~50 tasks covering the core entry point for all claim operations. Without search functionality, no other features can be accessed.

---

## Dependency Graph

```
Phase 1 (Setup)
    └─> Phase 2 (Foundation)
            ├─> Phase 3 (US1 - Search) [INDEPENDENT - MVP]
            ├─> Phase 4 (US2 - Payment) [DEPENDS: US1]
            │       └─> Phase 5 (US3 - History) [DEPENDS: US2]
            ├─> Phase 6 (US4 - Consortium) [DEPENDS: US2]
            ├─> Phase 7 (US5 - Phases) [DEPENDS: US2]
            └─> Phase 8 (US6 - Dashboard) [INDEPENDENT - Can run parallel]

All phases converge to:
    └─> Phase 9 (Polish & Deployment)
```

**Independent Test Criteria**:
- **US1**: Can test search with valid protocol number, verify claim details display
- **US2**: Requires US1, test payment authorization with valid claim
- **US3**: Requires US2, test history display with existing payments
- **US4**: Requires US2, test consortium product validation (codes 6814, 7701, 7709)
- **US5**: Requires US2, test phase tracking after payment authorization
- **US6**: Independent, test dashboard displays accurate metrics

---

## Phase 1: Setup (T001-T010)

### Backend Setup

- [X] T001 [P] [Setup] Create .NET 9 solution structure with Clean Architecture at `backend/CaixaSeguradora.sln` with projects: CaixaSeguradora.Api, CaixaSeguradora.Core, CaixaSeguradora.Infrastructure, and three test projects (Api.Tests, Core.Tests, Infrastructure.Tests)

- [X] T002 [P] [Setup] Install backend NuGet packages in `backend/src/CaixaSeguradora.Api/CaixaSeguradora.Api.csproj`: Microsoft.EntityFrameworkCore.SqlServer 9.0.0, SoapCore 1.1.0, AutoMapper.Extensions.Microsoft.DependencyInjection 13.0.1, Serilog.AspNetCore 8.0.0, Swashbuckle.AspNetCore 6.5.0, Microsoft.AspNetCore.Authentication.JwtBearer 9.0.0

- [X] T003 [P] [Setup] Install Core project packages in `backend/src/CaixaSeguradora.Core/CaixaSeguradora.Core.csproj`: FluentValidation 11.9.0, FluentValidation.DependencyInjectionExtensions 11.9.0

- [X] T004 [P] [Setup] Install Infrastructure packages in `backend/src/CaixaSeguradora.Infrastructure/CaixaSeguradora.Infrastructure.csproj`: Microsoft.EntityFrameworkCore 9.0.0, Microsoft.Extensions.Http.Polly 9.0.0, Polly 8.2.0, Serilog 4.0.0

### Frontend Setup

- [X] T005 [P] [Setup] Create React 18 application with TypeScript at `frontend/` using Vite: npm create vite@latest frontend -- --template react-ts, install dependencies (react-router-dom 6.20.0, axios 1.6.2, recharts 2.10.0, @tanstack/react-query 5.15.0)

- [X] T006 [P] [Setup] Copy Site.css from `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/Site.css` to `frontend/public/Site.css` without modifications

### Configuration Files

- [X] T007 [P] [Setup] Create appsettings.json at `backend/src/CaixaSeguradora.Api/appsettings.json` with ConnectionStrings (ClaimsDatabase), Logging configuration (Serilog with Console and File sinks), JWT settings (SecretKey, Issuer, Audience, ExpirationMinutes), ExternalServices endpoints (CNOUA, SIPUA, SIMDA base URLs), Dashboard refresh interval (30 seconds)

- [X] T008 [P] [Setup] Create appsettings.Development.json at `backend/src/CaixaSeguradora.Api/appsettings.Development.json` with localhost SQL Server connection string, detailed logging (Debug level), HTTPS development certificate settings

- [X] T009 [P] [Setup] Create .env file at `frontend/.env` with VITE_API_BASE_URL=https://localhost:5001, VITE_DASHBOARD_REFRESH_INTERVAL=30000

### Docker Configuration

- [X] T010 [Setup] Create docker-compose.yml at `deployment/docker/docker-compose.yml` with services: backend (Dockerfile.backend, port 5001), frontend (Dockerfile.frontend, port 3000), sqlserver (mcr.microsoft.com/mssql/server:2022-latest, port 1433), volumes for database persistence, network configuration for service communication

---

## Phase 2: Foundational (T011-T030)

### Database Layer - Entity Configurations

- [X] T011 [P] [Foundation] Create ClaimMaster entity at `backend/src/CaixaSeguradora.Core/Entities/ClaimMaster.cs` mapping to TMESTSIN table with composite key (Tipseg, Orgsin, Rmosin, Numsin), properties: Fonte, Protsini, Dac, Orgapo, Rmoapo, Numapol, Codprodu, Sdopag (decimal 15,2), Totpag (decimal 15,2), Codlider (nullable), Sinlid (nullable), Ocorhist (int), Tipreg (string[1]), Tpsegu (int), audit fields (CreatedBy, CreatedAt, UpdatedBy, UpdatedAt), RowVersion for concurrency, navigation properties for Branch, Policy, ClaimHistories, ClaimAccompaniments, ClaimPhases, computed property PendingValue (Sdopag - Totpag), computed property IsConsortiumProduct (Codprodu in 6814, 7701, 7709)

- [X] T012 [P] [Foundation] Create ClaimHistory entity at `backend/src/CaixaSeguradora.Core/Entities/ClaimHistory.cs` mapping to THISTSIN table with composite key (Tipseg, Orgsin, Rmosin, Numsin, Ocorhist), properties: Operacao (int), Dtmovto (date), Horaoper (time), Valpri (decimal 15,2), Crrmon (decimal 15,2), Nomfav (string 100), Tipcrr (string 1), Valpribt (decimal 15,2), Crrmonbt (decimal 15,2), Valtotbt (decimal 15,2), Sitcontb (string 1), Situacao (string 1), Ezeusrid (string 50), foreign key to ClaimMaster

- [X] T013 [P] [Foundation] Create BranchMaster entity at `backend/src/CaixaSeguradora.Core/Entities/BranchMaster.cs` mapping to TGERAMO table with primary key (Rmosin), property: Nomeramo (string 100)

- [X] T014 [P] [Foundation] Create CurrencyUnit entity at `backend/src/CaixaSeguradora.Core/Entities/CurrencyUnit.cs` mapping to TGEUNIMO table with composite key (Dtinivig), properties: Dttervig (date), Vlcruzad (decimal 18,6 for conversion rate precision)

- [X] T015 [P] [Foundation] Create SystemControl entity at `backend/src/CaixaSeguradora.Core/Entities/SystemControl.cs` mapping to TSISTEMA table with primary key (Idsistem), property: Dtmovabe (date - current business date)

- [X] T016 [P] [Foundation] Create PolicyMaster entity at `backend/src/CaixaSeguradora.Core/Entities/PolicyMaster.cs` mapping to TAPOLICE table with composite key (Orgapo, Rmoapo, Numapol), property: Nome (string 200 - insured name), foreign key to BranchMaster

- [X] T017 [P] [Foundation] Create ClaimAccompaniment entity at `backend/src/CaixaSeguradora.Core/Entities/ClaimAccompaniment.cs` mapping to SI_ACOMPANHA_SINI table with composite key (Fonte, Protsini, Dac, CodEvento, DataMovtoSiniaco), properties: NumOcorrSiniaco (int), DescrComplementar (string 500), CodUsuario (string 50), foreign key to ClaimMaster via (Fonte, Protsini, Dac)

- [X] T018 [P] [Foundation] Create ClaimPhase entity at `backend/src/CaixaSeguradora.Core/Entities/ClaimPhase.cs` mapping to SI_SINISTRO_FASE table with composite key (Fonte, Protsini, Dac, CodFase, CodEvento, NumOcorrSiniaco, DataInivigRefaev), properties: DataAberturaSifa (date), DataFechaSifa (date - default 9999-12-31 for open phases), computed property IsOpen (DataFechaSifa == 9999-12-31), computed property DaysOpen (DateTime.Today - DataAberturaSifa), foreign key to ClaimMaster and PhaseEventRelationship

- [X] T019 [P] [Foundation] Create PhaseEventRelationship entity at `backend/src/CaixaSeguradora.Core/Entities/PhaseEventRelationship.cs` mapping to SI_REL_FASE_EVENTO table with composite key (CodFase, CodEvento, DataInivigRefaev), property: IndAlteracaoFase (string 1 - '1' for opening, '2' for closing)

- [X] T020 [P] [Foundation] Create ConsortiumContract entity at `backend/src/CaixaSeguradora.Core/Entities/ConsortiumContract.cs` mapping to EF_CONTR_SEG_HABIT table with primary key (NumContrato)

### Dashboard Entities

- [X] T021 [P] [Foundation] [US6] Create MigrationStatus entity at `backend/src/CaixaSeguradora.Core/Entities/MigrationStatus.cs` with properties: Id (Guid PK), UserStoryCode (string 20 UK), UserStoryName (string 200), Status (enum: NotStarted, InProgress, Completed, Blocked), CompletionPercentage (decimal 5,2), RequirementsCompleted (int), RequirementsTotal (int), TestsPassed (int), TestsTotal (int), AssignedTo (string 100), StartDate (DateTime), EstimatedCompletion (DateTime), ActualCompletion (DateTime nullable), BlockingIssues (string 1000 nullable), CreatedAt, UpdatedAt, navigation to ComponentMigrations

- [X] T022 [P] [Foundation] [US6] Create ComponentMigrationTracking entity at `backend/src/CaixaSeguradora.Core/Entities/ComponentMigrationTracking.cs` with properties: Id (Guid PK), MigrationStatusId (Guid FK), ComponentType (enum: Screen, BusinessRule, DatabaseEntity, ExternalService), ComponentName (string 200), LegacyReference (string 200 - Visual Age file/function), Status (enum: NotStarted, InProgress, Completed, Blocked), EstimatedHours (decimal 6,2), ActualHours (decimal 6,2), Complexity (enum: Low, Medium, High), AssignedDeveloper (string 100), TechnicalNotes (string 2000), StartDate (DateTime), CompletionDate (DateTime nullable), CreatedAt, UpdatedAt, foreign key to MigrationStatus, navigation to PerformanceMetrics

- [X] T023 [P] [Foundation] [US6] Create PerformanceMetrics entity at `backend/src/CaixaSeguradora.Core/Entities/PerformanceMetrics.cs` with properties: Id (Guid PK), ComponentId (Guid FK), MetricType (enum: ResponseTime, Throughput, ConcurrentUsers, MemoryUsage, ErrorRate), LegacyValue (decimal 18,6), NewValue (decimal 18,6), Unit (string 20 - ms, req/s, users, MB, %), ImprovementPercentage (computed: ((LegacyValue - NewValue) / LegacyValue) * 100), MeasurementTimestamp (DateTime), TestScenario (string 500), PassFail (bool), CreatedAt, foreign key to ComponentMigrationTracking

### DbContext and Configuration

- [X] T024 [Foundation] Create ClaimsDbContext at `backend/src/CaixaSeguradora.Infrastructure/Data/ClaimsDbContext.cs` with DbSet properties for all 13 entities, OnModelCreating with Fluent API configurations applying IEntityTypeConfiguration<T> for all entities via ApplyConfigurationsFromAssembly, connection string injection via constructor, transaction support methods (BeginTransactionAsync, CommitAsync, RollbackAsync), SaveChangesAsync override for audit field auto-population (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy from HttpContext user)

- [X] T025 [Foundation] Create entity type configurations in `backend/src/CaixaSeguradora.Infrastructure/Data/Configurations/` directory: ClaimMasterConfiguration.cs (composite PK, relationships to Branch, Policy, ClaimHistories, ClaimAccompaniments, ClaimPhases, indexes on Protocol, Leader, Policy), ClaimHistoryConfiguration.cs (composite PK, FK to ClaimMaster, decimal precision), BranchMasterConfiguration.cs, CurrencyUnitConfiguration.cs, SystemControlConfiguration.cs, PolicyMasterConfiguration.cs, ClaimAccompanimentConfiguration.cs, ClaimPhaseConfiguration.cs, PhaseEventRelationshipConfiguration.cs, ConsortiumContractConfiguration.cs, MigrationStatusConfiguration.cs, ComponentMigrationTrackingConfiguration.cs, PerformanceMetricsConfiguration.cs

### Repository Pattern

- [X] T026 [Foundation] Create IRepository<T> generic interface at `backend/src/CaixaSeguradora.Core/Interfaces/IRepository.cs` with methods: GetByIdAsync, GetAllAsync, FindAsync (predicate), AddAsync, UpdateAsync, DeleteAsync, CountAsync

- [X] T027 [Foundation] Create IClaimRepository interface at `backend/src/CaixaSeguradora.Core/Interfaces/IClaimRepository.cs` extending IRepository<ClaimMaster> with methods: SearchByProtocolAsync(int fonte, int protsini, int dac), SearchByClaimNumberAsync(int orgsin, int rmosin, int numsin), SearchByLeaderCodeAsync(int codlider, int sinlid), GetClaimHistoryAsync(claimKey, page, pageSize), GetClaimPhasesAsync(protocolKey), IncrementOcorhist(claimKey)

- [X] T028 [Foundation] Implement ClaimRepository at `backend/src/CaixaSeguradora.Infrastructure/Repositories/ClaimRepository.cs` with ClaimsDbContext injection, Include() for navigation properties (Branch, Policy), AsNoTracking() for read-only queries, compiled queries for frequent searches (EF.CompileAsyncQuery), pagination support with PagedResult<T> return type

- [X] T029 [Foundation] Create IUnitOfWork interface at `backend/src/CaixaSeguradora.Core/Interfaces/IUnitOfWork.cs` with properties: Claims (IClaimRepository), BranchMasters, PolicyMasters, CurrencyUnits, SystemControls, ClaimAccompaniments, ClaimPhases, PhaseEventRelationships, ConsortiumContracts, MigrationStatuses, ComponentMigrations, PerformanceMetrics, methods: SaveChangesAsync, BeginTransactionAsync, CommitTransactionAsync, RollbackTransactionAsync

- [X] T030 [Foundation] Implement UnitOfWork at `backend/src/CaixaSeguradora.Infrastructure/Repositories/UnitOfWork.cs` with lazy initialization of all repositories, transaction management using IDbContextTransaction, Dispose pattern for cleanup

---

## Phase 3: User Story 1 - Search and Retrieve Claim [P1] (T031-T050)

**Functional Requirements**: FR-001 to FR-005
**Success Criteria**: SC-001 (< 3 seconds search)
**Independent Test**: Search with valid protocol "001/0123456-7" and verify claim details display

### Business Logic Layer

- [X] T031 [US1] Create ClaimSearchCriteria DTO at `backend/src/CaixaSeguradora.Core/DTOs/ClaimSearchCriteria.cs` with properties: Fonte (int?), Protsini (int?), Dac (int?), Orgsin (int?), Rmosin (int?), Numsin (int?), Codlider (int?), Sinlid (int?), validation attribute [ClaimSearchCriteria] ensuring at least one complete criteria set is provided

- [X] T032 [US1] Create ClaimDetailDto at `backend/src/CaixaSeguradora.Core/DTOs/ClaimDetailDto.cs` with properties matching ClaimSearchResponse schema from rest-api.yaml including formatted strings: NumeroProtocolo (fonte/protsini-dac), NumeroSinistro (orgsin/rmosin/numsin), NumeroApolice, ValorPendente (computed), EhConsorcio (computed), NomeRamo, NomeSeguradora

- [X] T033 [US1] Create ClaimSearchValidator at `backend/src/CaixaSeguradora.Core/Validators/ClaimSearchValidator.cs` using FluentValidation with rules: RuleFor protocol requires (Fonte > 0 AND Protsini > 0 AND Dac >= 0) OR (Orgsin > 0 AND Rmosin > 0 AND Numsin > 0) OR (Codlider > 0 AND Sinlid > 0), custom error messages in Portuguese

- [X] T034 [US1] Create IClaimService interface at `backend/src/CaixaSeguradora.Core/Interfaces/IClaimService.cs` with methods: SearchClaimAsync(ClaimSearchCriteria), GetClaimByIdAsync(int tipseg, int orgsin, int rmosin, int numsin), ValidateClaimExistsAsync(criteria), GetPendingValueAsync(claimId)

- [X] T035 [US1] Implement ClaimService at `backend/src/CaixaSeguradora.Core/Services/ClaimService.cs` with IUnitOfWork and IMapper injection, SearchClaimAsync implementing three search strategies (protocol, claim number, leader code), error handling with custom exception ClaimNotFoundException("DOCUMENTO {protocol} NAO CADASTRADO"), branch name lookup via Branch navigation property, insured name via Policy.Nome

### AutoMapper Profiles

- [X] T036 [P] [US1] Create ClaimMappingProfile at `backend/src/CaixaSeguradora.Api/Mappings/ClaimMappingProfile.cs` with mappings: ClaimMaster to ClaimDetailDto (ForMember NumeroProtocolo with custom formatter "{Fonte:D3}/{Protsini:D7}-{Dac}", ForMember NomeRamo from Branch.Nomeramo, ForMember NomeSeguradora from Policy.Nome, ForMember ValorPendente from PendingValue), ClaimHistory to HistoryRecordDto, ClaimPhase to PhaseRecordDto

### API Layer

- [X] T037 [US1] Create ClaimsController at `backend/src/CaixaSeguradora.Api/Controllers/ClaimsController.cs` inheriting from ControllerBase with [ApiController] and [Route("api/claims")], inject IClaimService, ILogger, IValidator<ClaimSearchCriteria>

- [X] T038 [US1] Implement POST /api/claims/search endpoint in ClaimsController: [HttpPost("search")] attribute, accepts ClaimSearchRequest body, validates using FluentValidation (return 400 with error details if invalid), calls ClaimService.SearchClaimAsync, returns 200 with ClaimSearchResponse on success, returns 404 with error code "SINISTRO_NAO_ENCONTRADO" and Portuguese message if ClaimNotFoundException, returns 500 with generic error on other exceptions, logs all requests with correlation ID

- [X] T039 [US1] Implement GET /api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin} endpoint in ClaimsController: route parameters with [Range] validation (minimum 1), calls ClaimService.GetClaimByIdAsync, returns 200 with ClaimDetailResponse, returns 404 if not found, adds Cache-Control header (max-age=60) for performance

### Error Handling Middleware

- [X] T040 [P] [US1] Create GlobalExceptionHandlerMiddleware at `backend/src/CaixaSeguradora.Api/Middleware/GlobalExceptionHandlerMiddleware.cs` catching all unhandled exceptions, mapping ClaimNotFoundException to 404, ValidationException to 400, DbUpdateConcurrencyException to 409, all others to 500, returning ErrorResponse schema with sucesso: false, codigoErro, mensagem (Portuguese), detalhes array, timestamp, traceId (Activity.Current?.Id)

- [X] T041 [P] [US1] Register middleware in Program.cs at `backend/src/CaixaSeguradora.Api/Program.cs`: app.UseMiddleware<GlobalExceptionHandlerMiddleware>() before app.UseRouting(), add correlation ID middleware, request logging with Serilog

### Frontend Components - Search

- [X] T042 [US1] Create Claim TypeScript interface at `frontend/src/models/Claim.ts` matching ClaimDetailDto structure with all properties typed correctly (numbers, strings, dates, booleans)

- [X] T043 [US1] Create claimsApi service at `frontend/src/services/claimsApi.ts` with Axios functions: searchClaim(criteria: ClaimSearchCriteria): Promise<ClaimDetailResponse>, getClaimById(tipseg, orgsin, rmosin, numsin): Promise<ClaimDetailResponse>, error handling mapping status codes to user-friendly Portuguese messages, request/response interceptors for JWT token injection

- [X] T044 [US1] Create ClaimSearchPage component at `frontend/src/pages/ClaimSearchPage.tsx` with state management for three search input groups (protocol, claim number, leader code), radio buttons to select active search type (mutual exclusivity), input validation showing red errors (#e80c4d from Site.css), search button calling claimsApi.searchClaim, loading spinner during request, error display below form, success navigates to ClaimDetailPage with claim data

- [X] T045 [US1] Create SearchForm component at `frontend/src/components/claims/SearchForm.tsx` extracting form logic from ClaimSearchPage: protocol inputs (fonte: number, protsini: number, dac: number 0-9), claim number inputs (orgsin, rmosin, numsin), leader inputs (codlider, sinlid), validation on blur, clear button to reset form, TypeScript prop types for onSearch callback

- [X] T046 [US1] Create ClaimDetailPage component at `frontend/src/pages/ClaimDetailPage.tsx` receiving claim data from react-router location state, displaying all claim fields in formatted layout: Protocol section (NumeroProtocolo, NumeroSinistro), Policy section (NumeroApolice, NomeSeguradora, NomeRamo), Financial section (Sdopag formatted as currency R$, Totpag, ValorPendente with color coding red if > 0), displaying Caixa Seguradora logo from base64 PNG in header, "Authorize Payment" button (enabled only if ValorPendente > 0) navigating to payment form

- [X] T047 [US1] Create ClaimInfoCard component at `frontend/src/components/claims/ClaimInfoCard.tsx` reusable component for displaying claim field groups: props: title (string), fields (array of {label, value, format?}), format function for currency/date/percentage, using Site.css classes for consistent styling

### Integration Tests

- [X] T048 [US1] Create ClaimSearchIntegrationTests at `backend/tests/CaixaSeguradora.Api.Tests/Integration/ClaimSearchIntegrationTests.cs` using WebApplicationFactory<Program> with in-memory database, test scenarios: Search_ByValidProtocol_ReturnsClaimDetails (arrange: seed claim in DB, act: POST /api/claims/search with protocol, assert: 200 status, claim details match), Search_ByInvalidProtocol_Returns404 (act: POST with non-existent protocol, assert: 404 with "NAO CADASTRADO" message), Search_WithoutCriteria_Returns400 (act: POST with empty body, assert: 400 with validation errors), Search_ByClaimNumber_ReturnsCorrectClaim (test claim number search path), Search_ByLeaderCode_ReturnsCorrectClaim (test leader search path)

- [X] T049 [US1] Create ClaimService unit tests at `backend/tests/CaixaSeguradora.Core.Tests/Services/ClaimServiceTests.cs` mocking IUnitOfWork and IMapper, test methods: SearchClaimAsync_WithValidProtocol_CallsRepositoryCorrectly (verify repository method called with right params), SearchClaimAsync_WhenClaimNotFound_ThrowsClaimNotFoundException (mock repository returning null, assert exception), GetClaimByIdAsync_WithValidId_ReturnsDto (verify mapping from entity to DTO)

### End-to-End Test

- [X] T050 [US1] Create Playwright E2E test at `frontend/tests/e2e/claim-search.spec.ts`: test "User can search claim by protocol and view details" (navigate to search page, enter protocol fields: fonte=1, protsini=123456, dac=7, click search button, wait for ClaimDetailPage, assert claim number visible, assert pending value displayed, assert all sections rendered correctly), test "User sees error for non-existent claim" (enter invalid protocol, click search, assert error message "NAO CADASTRADO" visible), verify < 3 seconds response time with page.waitForResponse() timeout assertion

---

## Phase 4: User Story 2 - Authorize Claim Payment [P2] (T051-T075)

**Functional Requirements**: FR-006 to FR-024
**Success Criteria**: SC-002 (< 90s payment cycle), SC-006 (all product types), SC-008 (2 decimal precision)
**Dependencies**: Requires US1 (search) completed
**Independent Test**: Retrieve claim, enter payment details (type 1, principal 25000.00, correction 1250.50, beneficiary), verify authorization accepted

### Business Logic - Payment Authorization

- [X] T051 [US2] Create PaymentAuthorizationRequest DTO at `backend/src/CaixaSeguradora.Core/DTOs/PaymentAuthorizationRequest.cs` with properties: TipoPagamento (int 1-5), ValorPrincipal (decimal required min 0.01), ValorCorrecao (decimal optional min 0), Favorecido (string max 100 conditionally required), TipoApolice (string[1] pattern [12]), Observacoes (string max 500)

- [X] T052 [US2] Create PaymentAuthorizationResponse DTO at `backend/src/CaixaSeguradora.Core/DTOs/PaymentAuthorizationResponse.cs` matching PaymentAuthorizationDetail schema from rest-api.yaml with all fields: authorization details (ocorhist, operacao=1098, dtmovto, horaoper), amounts in original currency (valpri, crrmon), amounts in BTNF (valpribt, crrmonbt, valtotbt), status fields (sitcontb='0', situacao='0'), valorPendenteAtualizado

- [X] T053 [US2] Create PaymentAuthorizationValidator at `backend/src/CaixaSeguradora.Core/Validators/PaymentAuthorizationValidator.cs` with FluentValidation rules: RuleFor TipoPagamento (InclusiveBetween 1 and 5 with message "Tipo de pagamento deve ser 1, 2, 3, 4 ou 5"), RuleFor ValorPrincipal (GreaterThan 0), RuleFor Favorecido (NotEmpty when tpsegu != 0 - requires custom validator with claim context), RuleFor TipoApolice (Matches regex ^[12]$), custom rule ValidatePendingBalance (ValorPrincipal + ValorCorrecao must not exceed claim PendingValue)

- [X] T054 [US2] Create ICurrencyConversionService interface at `backend/src/CaixaSeguradora.Core/Interfaces/ICurrencyConversionService.cs` with method: ConvertToBTNF(decimal amount, DateTime transactionDate): Task<decimal>, GetCurrentRate(DateTime date): Task<CurrencyRate>

- [X] T055 [US2] Implement CurrencyConversionService at `backend/src/CaixaSeguradora.Infrastructure/Services/CurrencyConversionService.cs` with IUnitOfWork injection, ConvertToBTNF querying TGEUNIMO table WHERE dtinivig <= transactionDate AND dttervig >= transactionDate, applying vlcruzad conversion rate, Math.Round result to 2 decimal places (MidpointRounding.AwayFromZero), throw CurrencyRateNotFoundException if no rate found for date, cache rates in memory for 24 hours using IMemoryCache

- [X] T056 [US2] Create IPaymentAuthorizationService interface at `backend/src/CaixaSeguradora.Core/Interfaces/IPaymentAuthorizationService.cs` with method: AuthorizePaymentAsync(int tipseg, int orgsin, int rmosin, int numsin, PaymentAuthorizationRequest request, string userId): Task<PaymentAuthorizationResponse>, ValidateBeneficiaryRequirement(ClaimMaster claim, string beneficiary): bool

- [X] T057 [US2] Implement PaymentAuthorizationService at `backend/src/CaixaSeguradora.Infrastructure/Services/PaymentAuthorizationService.cs` with complex transaction logic: BEGIN TRANSACTION, 1) Get SystemControl for DTMOVABE (business date), 2) Get ClaimMaster with pessimistic locking (FromSqlRaw with UPDLOCK), 3) Validate PendingValue sufficient, 4) Convert ValorPrincipal and ValorCorrecao to BTNF using CurrencyConversionService, 5) Create ClaimHistory record (operacao=1098, tipcrr='5', sitcontb='0', situacao='0', ezeusrid=userId, calculated fields valpribt, crrmonbt, valtotbt), 6) Increment ClaimMaster.Ocorhist by 1, 7) Update ClaimMaster.Totpag += valtotbt, 8) Create ClaimAccompaniment record (cod_evento from config, descr_complementar from observacoes, cod_usuario=userId), 9) Call PhaseManagementService.UpdatePhasesAsync, 10) COMMIT TRANSACTION, on any error: ROLLBACK and throw, return PaymentAuthorizationResponse with updated values

### External Service Integrations

- [X] T058 [P] [US2] Create IExternalValidationService interface at `backend/src/CaixaSeguradora.Core/Interfaces/IExternalValidationService.cs` with methods: ValidateConsortiumAsync(ClaimMaster claim): Task<ValidationResult>, ValidateEFPContractAsync(int numContrato): Task<ValidationResult>, ValidateHBContractAsync(ClaimMaster claim): Task<ValidationResult>

- [X] T059 [US2] Implement CNOUAValidationClient at `backend/src/CaixaSeguradora.Infrastructure/ExternalServices/CNOUAValidationClient.cs` for consortium products (6814, 7701, 7709) using HttpClient with Polly policies: Retry 3 times with exponential backoff (2s, 4s, 8s), Circuit Breaker (open after 5 consecutive failures, break duration 30s), Timeout 10s, POST to CNOUA_BASE_URL/validate with claim data, parse EZERT8 response code (success if '00000000', error otherwise), map error codes to Portuguese messages, implement IExternalValidationService

- [X] T060 [US2] Implement SIPUAValidationClient at `backend/src/CaixaSeguradora.Infrastructure/ExternalServices/SIPUAValidationClient.cs` for EFP contracts (NUM_CONTRATO > 0 in EF_CONTR_SEG_HABIT) using same Polly policies as CNOUA, SOAP call to SIPUA service, parse validation response, implement IExternalValidationService

- [X] T061 [US2] Implement SIMDAValidationClient at `backend/src/CaixaSeguradora.Infrastructure/ExternalServices/SIMDAValidationClient.cs` for HB contracts (NUM_CONTRATO = 0 or not in EF_CONTR_SEG_HABIT) using same Polly policies, SOAP call to SIMDA service, implement IExternalValidationService

- [X] T062 [US2] Create ExternalServiceHealthCheck at `backend/src/CaixaSeguradora.Infrastructure/HealthChecks/ExternalServiceHealthCheck.cs` implementing IHealthCheck: parallel calls to CNOUA, SIPUA, SIMDA /health endpoints with 5s timeout, return Healthy if all respond 200, Degraded if 1-2 fail, Unhealthy if all fail, register in Program.cs with builder.Services.AddHealthChecks().AddCheck<ExternalServiceHealthCheck>()

### API Endpoints - Payment

- [X] T063 [US2] Implement POST /api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/authorize-payment endpoint in ClaimsController: route parameters, [HttpPost("authorize-payment")] attribute, inject IPaymentAuthorizationService and IExternalValidationService, 1) Validate request body with FluentValidation, 2) Get claim from repository, 3) Determine product type (IsConsortiumProduct), 4) Route to appropriate validation service (CNOUA for consortium, check EF_CONTR_SEG_HABIT for EFP/HB routing), 5) If validation fails return 422 with error code "VALIDACAO_EXTERNA_FALHOU" and service error details, 6) Call PaymentAuthorizationService.AuthorizePaymentAsync, 7) Return 201 with PaymentAuthorizationResponse and Location header, handle all exceptions (DbUpdateConcurrencyException returns 409, insufficient balance returns 400, missing rate returns 422), add comprehensive logging

### SOAP Service Implementation

- [X] T064 [P] [US2] Create ISolicitacaoService SOAP contract at `backend/src/CaixaSeguradora.Api/SoapServices/ISolicitacaoService.cs` with [ServiceContract(Namespace = "http://ls.caixaseguradora.com.br/LS1134WSV0002_Solicitacao/v1")], operations: [OperationContract] Task<SolicitacaoResponse> CriarSolicitacaoAsync(SolicitacaoRequest), [OperationContract] Task<ConsultaSolicitacaoResponse> ConsultarSolicitacaoAsync(ConsultaSolicitacaoRequest)

- [X] T065 [P] [US2] Implement SolicitacaoService at `backend/src/CaixaSeguradora.Api/SoapServices/SolicitacaoService.cs` implementing ISolicitacaoService, inject IPaymentAuthorizationService, CriarSolicitacaoAsync maps SOAP request to PaymentAuthorizationRequest DTO and calls AuthorizePaymentAsync, ConsultarSolicitacaoAsync calls ClaimService.GetClaimByIdAsync, handle SOAP faults for exceptions with Portuguese fault strings

- [X] T066 [US2] Register SoapCore in Program.cs: app.UseSoapEndpoint<ISolicitacaoService>("/soap/solicitacao", new SoapEncoderOptions(), SoapSerializer.XmlSerializer), configure WSDL generation at /soap/solicitacao?wsdl, add SOAP logging middleware capturing request/response XML

### Frontend Components - Payment Authorization

- [X] T067 [US2] Create PaymentAuthorizationForm component at `frontend/src/components/claims/PaymentAuthorizationForm.tsx` with props: claim (Claim), onSuccess (callback), form fields: tipoPagamento (select dropdown 1-5 with Portuguese labels: "1-Pagamento Principal", "2-Correção Monetária", etc.), valorPrincipal (currency input with R$ mask), valorCorrecao (currency input optional), favorecido (text input max 100 chars - conditionally required if claim.tpsegu !== 0 with red asterisk), tipoApolice (radio buttons "1" or "2"), observacoes (textarea max 500 chars), submit button disabled during submission, client-side validation matching backend validator, clear error display below each field

- [X] T068 [US2] Create CurrencyInput component at `frontend/src/components/common/CurrencyInput.tsx` reusable controlled component: props: value (number), onChange ((value: number) => void), label, required, error, formats input as R$ with thousands separator and 2 decimal precision, handles paste events cleaning non-numeric chars, validates min/max values

- [X] T069 [US2] Update ClaimDetailPage to include PaymentAuthorizationForm component: conditional rendering (show form only if claim.valorPendente > 0 AND user has permission), onSuccess callback refreshing claim data from API and displaying success toast notification with authorization details (ocorhist, valorPendenteAtualizado), error handling displaying error message with retry button

- [X] T070 [US2] Create usePaymentAuthorization custom hook at `frontend/src/hooks/usePaymentAuthorization.ts` encapsulating payment submission logic: authorizePayment function calling claimsApi.authorizePayment with loading state, error state, success state, automatic retry on 409 conflict, error message mapping (400 validation, 404 not found, 409 concurrent update, 422 external validation failed, 500 generic), TypeScript return type with all states

### Integration Tests - Payment

- [X] T071 [US2] Create PaymentAuthorizationIntegrationTests at `backend/tests/CaixaSeguradora.Api.Tests/Integration/PaymentAuthorizationIntegrationTests.cs` with test scenarios: AuthorizePayment_WithValidRequest_ReturnsCreated (arrange: seed claim with pending value, act: POST authorize-payment, assert: 201 status, ClaimHistory record created, Ocorhist incremented, Totpag updated), AuthorizePayment_ExceedingPendingValue_Returns400 (act: POST with valorPrincipal > pendingValue, assert: 400 with "excede o saldo pendente"), AuthorizePayment_MissingBeneficiaryWhenRequired_Returns400 (arrange: claim with tpsegu != 0, act: POST without favorecido, assert: 400 validation error), AuthorizePayment_ConsortiumProduct_CallsValidationService (arrange: claim with codprodu=6814, mock CNOUA client, act: POST, assert: CNOUA called, verify external service headers), AuthorizePayment_ConcurrentUpdate_Returns409 (arrange: two parallel requests, act: both POST simultaneously, assert: one succeeds 201, one fails 409)

- [X] T072 [US2] Create CurrencyConversionServiceTests at `backend/tests/CaixaSeguradora.Infrastructure.Tests/Services/CurrencyConversionServiceTests.cs` with test methods: ConvertToBTNF_WithValidRate_ReturnsConvertedValue (arrange: seed TGEUNIMO with rate 1.1 for date, act: convert 1000.00, assert: returns 1100.00 with 2 decimal places), ConvertToBTNF_NoRateForDate_ThrowsException (arrange: no rate in date range, act: convert, assert: CurrencyRateNotFoundException), ConvertToBTNF_RoundsTo2Decimals (act: convert value resulting in 3+ decimals, assert: rounded correctly using MidpointRounding.AwayFromZero)

- [X] T073 [US2] Create ExternalValidationClientTests at `backend/tests/CaixaSeguradora.Infrastructure.Tests/ExternalServices/ExternalValidationClientTests.cs` using MockHttpMessageHandler: CNOUAClient_SuccessResponse_ReturnsValid (mock 200 response with EZERT8='00000000', assert: ValidationResult.Success), CNOUAClient_ErrorCode_ReturnsInvalid (mock EZERT8 != '00000000', assert: ValidationResult with error message), CNOUAClient_RetryOnTransientFailure (mock 503 on first call, 200 on retry, assert: retry occurred and success), CNOUAClient_CircuitBreakerOpensAfter5Failures (mock 5 consecutive 500 errors, assert: 6th call fails immediately without HTTP request - circuit open)

### End-to-End Test - Payment Cycle

- [X] T074 [US2] Create Playwright E2E test at `frontend/e2e/payment-cycle.spec.ts`: test "User completes full payment authorization cycle" (prerequisites: search and load claim with pending value, fill payment form: tipoPagamento=1, valorPrincipal=10000.00, valorCorrecao=500.00, favorecido="Test User", tipoApolice="1", submit form, wait for success notification, assert pending value reduced on page refresh, assert authorization details displayed), test "Concurrent payment conflict handled gracefully" (open two browser contexts, submit same payment in both, assert one succeeds, one shows conflict error with retry option), verify end-to-end cycle completes in < 90 seconds from claim search to payment confirmation

### Performance Test

- [X] T075 [US2] Create payment authorization load test at `backend/tests/LoadTests/payment-load-test.js` using NBomber or k6: simulate 100 concurrent users submitting payments to different claims, ramp up over 10 seconds, sustain for 60 seconds, measure: p50 response time < 500ms, p95 response time < 2000ms, p99 response time < 5000ms, error rate < 1%, throughput > 50 req/s, database connection pool not exhausted, verify no deadlocks in SQL Server

---

## Phase 5: User Story 3 - View Payment History [P3] (T076-T085)

**Functional Requirements**: Implicit from spec User Story 3
**Success Criteria**: Implicit - history display accuracy
**Dependencies**: Requires US2 (payment authorization) to have history data
**Independent Test**: Retrieve claim with existing payments, verify history list displays all records with dates, amounts, operators

### API Endpoint - History

- [X] T076 [US3] Implement GET /api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history endpoint in ClaimsController: route parameters, query parameters page (default 1) and pageSize (default 20, max 100), call ClaimService.GetClaimHistoryAsync with pagination, return ClaimHistoryResponse with properties: sucesso, totalRegistros, paginaAtual, tamanhoPagina, totalPaginas, historico (array of HistoryRecord), order by ocorhist DESC (most recent first), format dataHoraFormatada as "dd/MM/yyyy HH:mm:ss", add Cache-Control: no-cache for history (always fetch latest)

- [X] T077 [US3] Create HistoryRecordDto at `backend/src/CaixaSeguradora.Core/DTOs/HistoryRecordDto.cs` with all fields from HistoryRecord schema in rest-api.yaml: tipseg, orgsin, rmosin, numsin, ocorhist, operacao, dtmovto, horaoper, dataHoraFormatada (computed), valpri, crrmon, nomfav, tipcrr, valpribt, crrmonbt, valtotbt, sitcontb, situacao, ezeusrid, add computed property DataHoraCompleta combining dtmovto and horaoper to DateTime

- [X] T078 [US3] Update ClaimService.GetClaimHistoryAsync method implementation: query ClaimHistory table filtered by claim key, Include() no navigation properties needed, OrderByDescending(h => h.Ocorhist), apply pagination with Skip((page-1)*pageSize).Take(pageSize), map to HistoryRecordDto using AutoMapper, return PagedResult<HistoryRecordDto> with total count

### Frontend Components - History

- [X] T079 [US3] Create PaymentHistoryComponent at `frontend/src/components/claims/PaymentHistoryComponent.tsx` with props: claimKey ({tipseg, orgsin, rmosin, numsin}), fetches history data using React Query useQuery with staleTime=0 (always refetch), displays paginated table: columns (Data/Hora, Ocorrência, Operação, Valor Principal, Correção, Valor Total BTNF, Favorecido, Operador), formatters for currency (R$ with 2 decimals), date (Brazilian format dd/MM/yyyy HH:mm:ss), pagination controls at bottom (Previous, page numbers, Next), loading skeleton while fetching, empty state "Nenhum registro de histórico encontrado" if no records

- [X] T080 [US3] Create HistoryTable component at `frontend/src/components/claims/HistoryTable.tsx` reusable table component: props: records (HistoryRecord[]), loading (boolean), columns configuration, renders responsive table with Site.css styling, sticky header on scroll, alternating row colors, sortable columns (click header to sort client-side), export to CSV button

- [X] T081 [US3] Update ClaimDetailPage to include PaymentHistoryComponent: add "Histórico de Pagamentos" section below claim details, conditional rendering (load history only when tab/section is expanded), refresh button to reload history, auto-refresh on new payment authorization success

### Integration Tests - History

- [X] T082 [US3] Create ClaimHistoryIntegrationTests at `backend/tests/CaixaSeguradora.Api.Tests/Integration/ClaimHistoryIntegrationTests.cs` with test scenarios: GetHistory_WithMultipleRecords_ReturnsPaginated (arrange: seed claim with 25 history records, act: GET /history?page=1&pageSize=20, assert: returns 20 records, totalRegistros=25, totalPaginas=2), GetHistory_OrderedByOcorrenciaMostRecentFirst (arrange: seed 3 records with different dates, act: GET /history, assert: records ordered DESC by ocorhist), GetHistory_FormatsDateCorrectly (assert: dataHoraFormatada matches "dd/MM/yyyy HH:mm:ss" pattern), GetHistory_ForNonExistentClaim_Returns404

- [X] T083 [US3] Create PaymentHistoryComponent unit test at `frontend/tests/components/PaymentHistoryComponent.test.tsx` using React Testing Library: test "renders loading state", test "renders history records correctly" (mock API response with 3 records, assert table shows 3 rows with correct data), test "handles pagination clicks" (mock 50 records, click Next, assert page 2 requested), test "displays empty state when no records"

### End-to-End Test - History

- [X] T084 [US3] Create Playwright E2E test at `frontend/tests/e2e/payment-history.spec.ts`: test "User views payment history for claim" (prerequisite: claim with 5 history records, navigate to ClaimDetailPage, click "Histórico de Pagamentos" tab, wait for table to load, assert 5 rows visible, assert columns display correct data, assert dates formatted correctly, click row to see details), test "Pagination works correctly" (prerequisite: 25 records, default page size 20, assert page 1 shows first 20, click Next, assert page 2 shows last 5)

### Performance Test - History

- [ ] T085 [US3] Verify history query performance: test with claim having 1000+ history records, measure query execution time < 500ms, verify pagination doesn't cause N+1 queries, check database execution plan for index usage on (tipseg, orgsin, rmosin, numsin, ocorhist), recommend adding index if missing: CREATE INDEX IX_THISTSIN_Claim_Occurrence ON THISTSIN(TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC)

---

## Phase 6: User Story 4 - Handle Special Products (Consortium) [P4] (T086-T095)

**Functional Requirements**: FR-020 to FR-024
**Success Criteria**: SC-006 (all product types including consortium)
**Dependencies**: Requires US2 (payment authorization) with external service integration
**Independent Test**: Process payment for claim with product code 6814, verify CNOUA validation called

### Business Logic - Product Routing

- [X] T086 [US4] Create ProductValidator at `backend/src/CaixaSeguradora.Core/Validators/ProductValidator.cs` with methods: IsConsortiumProduct(int codprodu): bool (returns true if codprodu in [6814, 7701, 7709]), RequiresEFPValidation(ClaimMaster claim): Task<bool> (queries EF_CONTR_SEG_HABIT by policy to check NUM_CONTRATO > 0), DetermineValidationRoute(ClaimMaster claim): Task<ValidationRoute> (returns enum: CNOUA, SIPUA, SIMDA based on product code and contract)

- [X] T087 [US4] Create ValidationRoute enum at `backend/src/CaixaSeguradora.Core/Enums/ValidationRoute.cs` with values: CNOUA, SIPUA, SIMDA, None, add extension methods: GetServiceName(): string, GetDescription(): string (Portuguese descriptions), IsExternal(): bool

- [X] T088 [US4] Update PaymentAuthorizationService.AuthorizePaymentAsync to integrate ProductValidator: after getting ClaimMaster, call ProductValidator.DetermineValidationRoute, switch on route: case CNOUA: call CNOUAValidationClient, case SIPUA: call SIPUAValidationClient, case SIMDA: call SIMDAValidationClient, case None: skip validation, log validation route decision with claim details, if validation fails: rollback transaction and throw ExternalValidationException with service name and error details

### External Service Enhancements

- [X] T089 [US4] Enhance CNOUAValidationClient error mapping: create dictionary of EZERT8 error codes to Portuguese messages, example: "EZERT8001" -> "Contrato de consórcio inválido", "EZERT8002" -> "Contrato cancelado", "EZERT8003" -> "Grupo encerrado", add logging of raw EZERT8 codes for debugging, include service response time in logs

- [ ] T090 [US4] Create contract tests for CNOUA at `backend/tests/CaixaSeguradora.Infrastructure.Tests/ExternalServices/CNOUAContractTests.cs` using Pact or similar: define expected request schema (claim number, product code, contract number, etc.), define expected response schema (EZERT8 code, validation status, error message), verify client sends correct request structure, verify client handles all documented response formats, run contract test against mock provider

- [ ] T091 [US4] Create contract tests for SIPUA at `backend/tests/CaixaSeguradora.Infrastructure.Tests/ExternalServices/SIPUAContractTests.cs` with SOAP request/response validation: verify SOAP envelope structure, verify namespace correctness, verify WS-Security headers if required, test error handling for SOAP faults

- [ ] T092 [US4] Create contract tests for SIMDA at `backend/tests/CaixaSeguradora.Infrastructure.Tests/ExternalServices/SIMDAContractTests.cs` similar to SIPUA contract tests

### Integration Tests - Product Routing

- [ ] T093 [US4] Create ConsortiumProductIntegrationTests at `backend/tests/CaixaSeguradora.Api.Tests/Integration/ConsortiumProductIntegrationTests.cs` with scenarios: AuthorizePayment_ConsortiumProduct6814_CallsCNOUA (arrange: claim with codprodu=6814, mock CNOUA, act: POST authorize-payment, assert: CNOUA called with correct payload), AuthorizePayment_EFPContract_CallsSIPUA (arrange: claim with NUM_CONTRATO > 0, mock SIPUA, act: POST, assert: SIPUA called), AuthorizePayment_HBContract_CallsSIMDA (arrange: claim with NUM_CONTRATO = 0, mock SIMDA, act: POST, assert: SIMDA called), AuthorizePayment_ValidationFails_ReturnsErrorMessage (arrange: CNOUA returns error EZERT8 != '00000000', act: POST, assert: 422 with Portuguese error message from service)

### Documentation

- [X] T094 [P] [US4] Document product validation routing logic in `docs/product-validation-routing.md`: create decision tree diagram (product code check -> consortium (CNOUA) | non-consortium (contract check -> EFP (SIPUA) | HB (SIMDA))), list all product codes and their routing rules, document EZERT8 error codes and Portuguese translations, include example API requests/responses for each product type

- [X] T095 [P] [US4] Add product routing examples to Swagger documentation: create example payloads in ClaimsController for each product type (consortium 6814, EFP contract, HB contract), annotate with [SwaggerOperation] describing validation flow, add [SwaggerResponse(422)] examples showing external validation errors, update rest-api.yaml with product-specific examples

---

## Phase 7: User Story 5 - Manage Claim Phase [P5] (T096-T110)

**Functional Requirements**: FR-025 to FR-030
**Success Criteria**: Implicit - phase tracking accuracy
**Dependencies**: Requires US2 (payment authorization) to trigger phase updates
**Independent Test**: Authorize payment, verify phase record created/updated in SI_SINISTRO_FASE

### Business Logic - Phase Management

- [X] T096 [US5] Create IPhaseManagementService interface at `backend/src/CaixaSeguradora.Core/Interfaces/IPhaseManagementService.cs` with methods: UpdatePhasesAsync(int fonte, int protsini, int dac, int codEvento, DateTime dataMovto, string userId): Task, CreatePhaseOpeningAsync(ClaimPhase phase): Task, UpdatePhaseClosingAsync(ClaimPhase phase): Task, GetActivePhases(protocolKey): Task<List<ClaimPhase>>, PreventDuplicatePhase(protocolKey, codFase, codEvento): Task<bool>

- [X] T097 [US5] Implement PhaseManagementService at `backend/src/CaixaSeguradora.Infrastructure/Services/PhaseManagementService.cs` with logic: UpdatePhasesAsync queries SI_REL_FASE_EVENTO WHERE codEvento = input AND dataMovto BETWEEN data_inivig_refaev (check date range), for each matching relationship: if ind_alteracao_fase = '1' (opening): create ClaimPhase with data_abertura_sifa = dataMovto, data_fecha_sifa = '9999-12-31' (SQL date max), check PreventDuplicatePhase (do not create if already exists), if ind_alteracao_fase = '2' (closing): find existing open ClaimPhase (data_fecha_sifa = '9999-12-31') and update data_fecha_sifa = dataMovto, log all phase changes with before/after state, wrap in transaction (caller transaction, not separate)

- [X] T098 [US5] Update PaymentAuthorizationService to call PhaseManagementService: after creating ClaimAccompaniment record, call PhaseManagementService.UpdatePhasesAsync with cod_evento from accompaniment record, dataMovto from system date, userId from context, if UpdatePhasesAsync throws exception: rollback entire transaction (claim update, history insert, accompaniment, phase update - all or nothing per FR-030)

### API Endpoint - Phases

- [X] T099 [US5] Implement GET /api/claims/{fonte}/{protsini}/{dac}/phases endpoint in ClaimsController: route parameters for protocol, call ClaimService.GetClaimPhasesAsync, return ClaimPhasesResponse with properties: sucesso, protocolo (formatted fonte/protsini-dac), totalFases, fases (array of PhaseRecord with: codFase, nomeFase, codEvento, nomeEvento, numOcorrSiniaco, dataInivigRefaev, dataAberturaSifa, dataFechaSifa, status (computed: "Aberta" if dataFechaSifa = 9999-12-31, "Fechada" otherwise), diasAberta (computed: today - dataAberturaSifa or dataFechaSifa - dataAberturaSifa)), order by dataAberturaSifa DESC

- [X] T100 [US5] Create PhaseRecordDto at `backend/src/CaixaSeguradora.Core/DTOs/PhaseRecordDto.cs` with all fields from PhaseRecord schema in rest-api.yaml, add computed properties: IsOpen: bool (dataFechaSifa == DateTime.Parse("9999-12-31")), DaysOpen: int (calculates days between opening and closing or today), StatusDescription: string ("Aberta" or "Fechada" in Portuguese), add phase name and event name lookups from configuration tables (if such tables exist) or return codes

### Frontend Components - Phases

- [ ] T101 [US5] Create ClaimPhasesComponent at `frontend/src/components/claims/ClaimPhasesComponent.tsx` with props: protocolKey ({fonte, protsini, dac}), fetches phases using React Query, displays timeline visualization: vertical timeline with phase nodes, each node shows: phase name, event that triggered it, opening date, closing date (or "Em Aberto"), days open (color-coded: green < 30 days, yellow 30-60 days, red > 60 days), expandable details showing num_ocorr_siniaco and data_inivig_refaev, loading state, error state if fetch fails

- [ ] T102 [US5] Create PhaseTimeline component at `frontend/src/components/claims/PhaseTimeline.tsx` reusable timeline visualization: props: phases (PhaseRecord[]), vertical line connecting phase nodes, nodes positioned chronologically, visual distinction between open (pulsing green dot) and closed (gray checkmark) phases, hover tooltip showing full details, responsive design collapsing to list view on mobile

- [ ] T103 [US5] Update ClaimDetailPage to include ClaimPhasesComponent: add "Fases do Sinistro" section below history, conditional rendering (load phases on demand), refresh button, auto-refresh after payment authorization

### Integration Tests - Phases

- [ ] T104 [US5] Create PhaseManagementIntegrationTests at `backend/tests/CaixaSeguradora.Api.Tests/Integration/PhaseManagementIntegrationTests.cs` with scenarios: UpdatePhases_EventTriggersOpening_CreatesPhaseRecord (arrange: SI_REL_FASE_EVENTO with ind_alteracao_fase='1', act: authorize payment triggering event, assert: SI_SINISTRO_FASE record created with data_fecha_sifa='9999-12-31'), UpdatePhases_EventTriggersClosing_UpdatesExistingPhase (arrange: open phase record, SI_REL_FASE_EVENTO with ind_alteracao_fase='2', act: trigger event, assert: data_fecha_sifa updated to today), UpdatePhases_PreventsDuplicatePhaseOpening (arrange: phase already open, act: trigger same event again, assert: no duplicate record created), UpdatePhases_RollbackOnFailure (arrange: corrupt phase data causing exception, act: trigger event, assert: transaction rolled back, no partial updates)

- [ ] T105 [US5] Create PhaseManagementServiceTests at `backend/tests/CaixaSeguradora.Core.Tests/Services/PhaseManagementServiceTests.cs` unit tests mocking IUnitOfWork: UpdatePhasesAsync_FindsMatchingRelationships (mock SI_REL_FASE_EVENTO query returning 2 relationships, assert: 2 phase operations performed), CreatePhaseOpeningAsync_SetsCorrectDates (act: create opening, assert: data_abertura_sifa = today, data_fecha_sifa = 9999-12-31), UpdatePhaseClosingAsync_FindsOpenPhase (mock query returning open phase, act: close, assert: data_fecha_sifa updated), PreventDuplicatePhase_ReturnsTrue_WhenPhaseExists (mock query finding existing phase, assert: returns true)

- [X] T106 [US5] Create ClaimPhasesComponent test at `frontend/tests/components/ClaimPhasesComponent.test.tsx`: test "renders phases in timeline" (mock API returning 2 phases, assert timeline shows 2 nodes), test "distinguishes open vs closed phases" (mock 1 open and 1 closed, assert open has green indicator, closed has checkmark), test "calculates days open correctly" (mock phase opened 45 days ago, assert "45 dias" displayed)

### Database Tests

- [ ] T107 [US5] Verify SI_REL_FASE_EVENTO configuration data exists at `backend/tests/CaixaSeguradora.Infrastructure.Tests/Data/PhaseEventConfigurationTests.cs`: query SI_REL_FASE_EVENTO table, assert: at least 1 relationship exists for payment authorization event, assert: both opening (ind_alteracao_fase='1') and closing (ind_alteracao_fase='2') relationships exist, assert: date ranges are valid (data_inivig_refaev < today), document any missing configuration needed for production

### End-to-End Test - Phase Tracking

- [ ] T108 [US5] Create Playwright E2E test at `frontend/tests/e2e/phase-tracking.spec.ts`: test "Payment authorization triggers phase update" (prerequisite: claim without open phases, authorize payment, navigate to Phases tab, assert new phase appeared with opening date = today, status = "Aberta"), test "Phase timeline displays correctly" (prerequisite: claim with 3 historical phases, load Phases tab, assert timeline shows 3 nodes in chronological order, assert closed phases show duration)

### Documentation

- [X] T109 [P] [US5] Document phase management workflow in `docs/phase-management-workflow.md`: create sequence diagram (PaymentAuthorization -> ClaimAccompaniment -> PhaseManagement -> SI_REL_FASE_EVENTO query -> SI_SINISTRO_FASE update), explain opening vs closing logic with examples, list all event codes and their phase impacts, document 9999-12-31 special date meaning, include SQL queries to manually check phase state

- [X] T110 [P] [US5] Add phase management to API documentation: update rest-api.yaml with detailed phase workflow description in GET /api/claims/{fonte}/{protsini}/{dac}/phases operation, add examples showing different phase states, document computed properties (status, diasAberta), add links to phase management workflow doc

---

## Phase 8: User Story 6 - Migration Dashboard [P6] (T111-T130)

**Functional Requirements**: FR-038 to FR-050
**Success Criteria**: SC-014 to SC-019 (dashboard functionality, 30s refresh, metrics visibility)
**Dependencies**: None (can be developed in parallel with operational features)
**Independent Test**: Access dashboard, verify all metrics display, data refreshes within 30 seconds

### Backend - Dashboard Data Services

- [X] T111 [P] [US6] Create IDashboardService interface at `backend/src/CaixaSeguradora.Core/Interfaces/IDashboardService.cs` with methods: GetOverviewAsync(): Task<DashboardOverviewDto>, GetComponentsAsync(ComponentType? type, ComponentStatus? status, string? responsavel): Task<List<ComponentDetailDto>>, GetPerformanceMetricsAsync(string periodo): Task<List<PerformanceMetricDto>>, GetRecentActivitiesAsync(int limite): Task<List<ActivityRecordDto>>, GetSystemHealthAsync(): Task<SystemHealthDto>

- [X] T112 [US6] Create DashboardOverviewDto at `backend/src/CaixaSeguradora.Core/DTOs/DashboardOverviewDto.cs` with nested objects: ProgressoGeral (percentualCompleto, userStoriesCompletas, userStoriesTotal, requisitosCompletos, requisitosTotal, testesAprovados, testesTotal, coberturaCodigo), StatusUserStories (array of UserStoryStatusDto), ComponentesMigrados (ComponentsSummaryDto with telas, regrasNegocio, integracoesBD, servicosExternos - each with total, completas, emProgresso, percentual), SaudeDoSistema (SystemHealthDto), UltimaAtualizacao (DateTime)

- [X] T113 [US6] Implement DashboardService at `backend/src/CaixaSeguradora.Infrastructure/Services/DashboardService.cs` with IUnitOfWork injection: GetOverviewAsync queries MigrationStatus table aggregating by status (NotStarted, InProgress, Completed, Blocked), calculates overall percentualCompleto = (sum of all user story completion percentages) / 6, queries ComponentMigrationTracking grouped by ComponentType to get counts, queries PerformanceMetrics for latest test results, calls GetSystemHealthAsync for health status, caches result for 30 seconds using IMemoryCache

- [X] T114 [US6] Implement GetComponentsAsync with filtering: query ComponentMigrationTracking, apply Where clauses for type/status/responsavel filters, Include MigrationStatus navigation property for user story info, order by Status (InProgress first, then NotStarted, then Completed), map to ComponentDetailDto with UserStoryId, calculate hoursVariance = ActualHours - EstimatedHours

- [X] T115 [US6] Implement GetPerformanceMetricsAsync: query PerformanceMetrics, filter by periodo (ultima-hora: timestamp > now - 1 hour, ultimo-dia: > now - 1 day, ultima-semana: > now - 7 days, ultimo-mes: > now - 30 days), group by MetricType, calculate average LegacyValue and NewValue, compute ImprovementPercentage, map to PerformanceMetricDto, include test scenario details

- [X] T116 [US6] Implement GetRecentActivitiesAsync: create activity log table or query audit logs, union queries: completed components (ComponentMigrationTracking where Status changed to Completed recently), test results (PerformanceMetrics where PassFail = true recently), status changes (MigrationStatus where Status updated), deployments (from deployment log if available), blocked items (MigrationStatus/ComponentMigrationTracking with BlockingIssues not null), order by timestamp DESC, limit to input parameter, map to ActivityRecordDto with type, titulo, descricao, userStory, responsavel, timestamp

- [X] T117 [US6] Implement GetSystemHealthAsync: parallel health checks using Task.WhenAll, 1) Database connectivity check (simple query like SELECT 1), 2) CNOUA service health (HTTP GET /health with 5s timeout), 3) SIPUA service health, 4) SIMDA service health, 5) API self-check (always true), return SystemHealthDto with boolean flags for each service, ultimaVerificacao = DateTime.UtcNow, cache for 15 seconds to avoid overwhelming external services

### API Endpoints - Dashboard

- [X] T118 [US6] Create DashboardController at `backend/src/CaixaSeguradora.Api/Controllers/DashboardController.cs` with [Route("api/dashboard")], inject IDashboardService, add CORS policy allowing frontend origin, add caching headers (Cache-Control: private, max-age=30)

- [X] T119 [US6] Implement GET /api/dashboard/overview endpoint: [HttpGet("overview")], calls DashboardService.GetOverviewAsync, returns DashboardOverviewResponse, add [ResponseCache(Duration = 30)] attribute for browser caching, include ETag header for conditional requests

- [X] T120 [US6] Implement GET /api/dashboard/components endpoint: [HttpGet("components")], query parameters: tipo (ComponentType enum), status (ComponentStatus enum), responsavel (string), calls GetComponentsAsync with filters, returns DashboardComponentsResponse with totalComponentes and componentes array, supports filtering via query string

- [X] T121 [US6] Implement GET /api/dashboard/performance endpoint: [HttpGet("performance")], query parameter: periodo (enum: ultima-hora, ultimo-dia, ultima-semana, ultimo-mes, default: ultimo-dia), calls GetPerformanceMetricsAsync, returns DashboardPerformanceResponse with periodo, metricas array, ultimaAtualizacao

- [X] T122 [US6] Implement GET /api/dashboard/activities endpoint: [HttpGet("activities")], query parameter: limite (int, min 5, max 50, default 10), calls GetRecentActivitiesAsync, returns DashboardActivitiesResponse with totalAtividades, atividades array ordered by timestamp DESC

### Frontend - Dashboard Application

- [X] T123 [US6] Create MigrationDashboardPage at `frontend/src/pages/MigrationDashboardPage.tsx` with React Query useQuery hooks: fetch overview data with refetchInterval=30000 (30 seconds auto-refresh), display loading skeleton while fetching, error boundary for fetch failures, header with logo and dashboard title "Painel de Migração - Caixa Seguradora", grid layout with 4 main sections: Overview Cards, User Stories Progress, Components Grid, Performance Charts

- [ ] T124 [US6] Create OverviewCards component at `frontend/src/components/dashboard/OverviewCards.tsx` displaying key metrics in card format: card 1 (Overall Progress with large circular progress indicator showing percentualCompleto, subtitle showing X/6 user stories completed), card 2 (Requirements with X/55 completed, progress bar), card 3 (Tests with X/Y passed, pass rate percentage, color-coded green > 90%, yellow 70-90%, red < 70%), card 4 (Code Coverage with percentage, circular progress), using Site.css for card styling

- [ ] T125 [US6] Create UserStoryProgressList component at `frontend/src/components/dashboard/UserStoryProgressList.tsx` displaying all 6 user stories: list view with each story showing: codigo (US-001), nome, status badge (NotStarted=gray, InProgress=blue, Completed=green, Blocked=red), completion progress bar, requisitosCompletos/requisitosTotal, testesAprovados/testesTotal, responsavel, dataEstimada vs dataConclusao (show warning if overdue), bloqueios (display alert icon if present), clickable to drill-down to story details

- [ ] T126 [US6] Create ComponentsGrid component at `frontend/src/components/dashboard/ComponentsGrid.tsx` displaying 4 quadrants: Telas (2 screens: SIWEG.SIWMAP1, SIWEG.SIWMAP2), Regras de Negócio (42 rules grouped by complexity), Integrações BD (10 entities), Serviços Externos (3 services: CNOUA, SIPUA, SIMDA), each quadrant shows: total count, completed count, in progress count, blocked count, percentual completion, mini progress bar, clicking quadrant filters full component list

- [ ] T127 [US6] Create PerformanceCharts component at `frontend/src/components/dashboard/PerformanceCharts.tsx` using Recharts library: 1) Bar chart comparing legacy vs new system across 5 metrics (ResponseTime, Throughput, ConcurrentUsers, MemoryUsage, ErrorRate), dual Y-axis for different units, 2) Line chart showing improvement percentages over time, 3) Table view showing detailed metrics with valorLegado, valorNovo, melhoria percentage (green if positive), aprovado checkmark, cenarioTeste description, periodo selector dropdown (ultima-hora, ultimo-dia, ultima-semana, ultimo-mes) updating chart data

- [X] T128 [US6] Create ActivitiesTimeline component at `frontend/src/components/dashboard/ActivitiesTimeline.tsx` displaying recent activities: vertical timeline with last 10 activities, each activity shows: icon based on type (✓ for TaskCompleted, ✓ for TestPassed, ↻ for StatusChange, ↑ for Deployment, ⚠ for Blocked), titulo (bold), descricao (smaller text), userStory badge if applicable, responsavel, timestamp (formatted as relative time "há 2 horas" using date-fns), color-coded by type (green for completed, blue for status change, orange for deployment, red for blocked), auto-scroll to show newest at top

- [X] T129 [US6] Create SystemHealthIndicators component at `frontend/src/components/dashboard/SystemHealthIndicators.tsx` showing service status: horizontal row of 5 service indicators (API, Database, CNOUA, SIPUA, SIMDA), each indicator: service name, status icon (green checkmark if available, red X if unavailable, yellow warning if degraded), hover tooltip showing ultimaVerificacao timestamp, clicking indicator shows detailed health check logs, refresh button to force health check update

### Integration Tests - Dashboard

- [X] T130 [US6] Create DashboardIntegrationTests at `backend/tests/CaixaSeguradora.Api.Tests/Integration/DashboardIntegrationTests.cs` with scenarios: GetOverview_ReturnsAggregatedData (arrange: seed MigrationStatus and ComponentMigrationTracking records, act: GET /api/dashboard/overview, assert: percentualCompleto calculated correctly, all sections present), GetComponents_FilterByType_ReturnsFiltered (arrange: seed mixed component types, act: GET /api/dashboard/components?tipo=Screen, assert: only Screen components returned), GetPerformanceMetrics_FilterByPeriod_ReturnsCorrectRange (arrange: seed metrics with different timestamps, act: GET /api/dashboard/performance?periodo=ultimo-dia, assert: only last 24 hours returned), GetActivities_LimitsResults (act: GET /api/dashboard/activities?limite=5, assert: returns exactly 5 activities), GetSystemHealth_ChecksAllServices (act: GET /api/dashboard/overview including health, assert: all 5 services checked, returns boolean for each)

---

## Phase 9: Polish & Cross-Cutting (T131-T147)

### UI Polish & Styling

- [X] T131 [P] [Polish] Integrate Site.css into React application: import Site.css in `frontend/src/main.tsx`, verify all components use existing CSS classes (.content-wrapper for max-width 960px, .error-message for validation errors #e80c4d, .button-primary for submit buttons, .input-field for form inputs, .card for dashboard cards), test responsive design at breakpoints (max-width 850px for mobile), ensure no CSS conflicts or overrides, verify fonts and colors match legacy system

- [X] T132 [P] [Polish] Display Caixa Seguradora logo from base64 PNG: create Logo component at `frontend/src/components/common/Logo.tsx` with base64 data from spec.md, render as <img src="data:image/png;base64,..." alt="Caixa Seguradora" />, add to header in ClaimSearchPage, ClaimDetailPage, MigrationDashboardPage, style with max-height 60px and auto width, ensure rendering in all browsers (Chrome, Firefox, Safari, Edge), test print view (logo should appear), add fallback to text if image fails to load

- [ ] T133 [Polish] Test responsive design on mobile devices: using browser dev tools, test all pages at viewport widths: 320px (iPhone SE), 375px (iPhone 12), 768px (iPad), 1024px (desktop), verify: forms stack vertically on mobile, tables scroll horizontally or collapse to cards, dashboard grid changes from 4 columns to 2 columns to 1 column, buttons remain tappable (min 44px height), font sizes readable (min 14px body text), no horizontal scroll on any page, navigation menu collapses to hamburger on mobile

- [ ] T134 [Polish] Implement dark mode support (optional enhancement): add CSS variables for colors in Site.css (:root for light theme, [data-theme="dark"] for dark theme), create ThemeToggle component, store preference in localStorage, update all color references to use CSS variables, ensure contrast ratios meet WCAG AA standards in both themes, test logo visibility in dark mode (may need inverted version)

### Portuguese Error Messages

- [X] T135 [P] [Polish] Create centralized error message resource file at `backend/src/CaixaSeguradora.Core/Resources/ErrorMessages.pt-BR.resx` with all error messages from spec: "DOCUMENTO {0} NAO CADASTRADO", "Tipo de pagamento deve ser 1, 2, 3, 4 ou 5", "Favorecido é obrigatório para este tipo de seguro", "Valor total excede o saldo pendente", "Validação de produto consórcio falhou", etc., use ResourceManager in services and controllers to retrieve messages, add unit tests verifying correct message retrieval

- [X] T136 [P] [Polish] Create frontend error message mapping at `frontend/src/utils/errorMessages.ts` with function mapApiError(statusCode: number, errorCode: string, defaultMessage: string): string, map all error codes from backend to Portuguese user-friendly messages, handle network errors ("Erro de conexão. Verifique sua internet."), timeout errors ("A requisição demorou muito. Tente novamente."), validation errors (join detalhes array with line breaks), display in red error box using Site.css .error-message class

### Security Hardening

- [X] T137 [P] [Polish] Implement JWT authentication: create JwtTokenService at `backend/src/CaixaSeguradora.Infrastructure/Services/JwtTokenService.cs` with methods: GenerateToken(userId, roles, expirationMinutes), ValidateToken(token), configure in Program.cs: builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer with TokenValidationParameters (ValidIssuer, ValidAudience, IssuerSigningKey from appsettings SecretKey, ClockSkew = 0), add [Authorize] attribute to ClaimsController and DashboardController, create AuthenticationController with POST /api/auth/login endpoint (accepts username/password, returns JWT token), integrate with Active Directory or LDAP for user validation

- [X] T138 [P] [Polish] Implement CORS policy: configure in Program.cs: builder.Services.AddCors with policy allowing frontend origin (http://localhost:3000 for dev, production domain for prod), expose Authorization header, allow credentials, restrict allowed methods to GET, POST, PUT, DELETE, add app.UseCors() middleware before app.UseAuthorization(), test preflight OPTIONS requests

- [ ] T139 [P] [Polish] Add rate limiting: install AspNetCoreRateLimit package, configure in appsettings.json: general rules (100 requests per minute per IP), endpoint-specific rules (/api/claims/search: 20/minute, /api/dashboard/*: 60/minute), whitelist localhost for development, register in Program.cs: builder.Services.AddInMemoryRateLimiting(), add app.UseIpRateLimiting(), return 429 Too Many Requests with Retry-After header

- [ ] T140 [P] [Polish] Security audit: run OWASP ZAP or similar security scanner against API, fix: SQL injection (verify EF Core parameterization), XSS (sanitize all inputs, encode outputs, CSP headers), CSRF (add anti-forgery tokens to forms, SameSite cookies), sensitive data exposure (remove stack traces in production, mask connection strings in logs), insecure dependencies (dotnet list package --vulnerable, update vulnerable packages), HTTPS enforcement (HSTS headers, redirect HTTP to HTTPS)

### Performance Optimization

- [ ] T141 [Polish] Database query optimization: profile all API endpoints using SQL Server Profiler or Application Insights, identify N+1 query problems (use Include() or explicit eager loading), add missing indexes (recommend: IX_TMESTSIN_Protocol on (FONTE, PROTSINI, DAC), IX_THISTSIN_ClaimOccurrence on (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC), IX_SI_SINISTRO_FASE_Protocol on (FONTE, PROTSINI, DAC)), enable EF Core query logging in development to inspect generated SQL, optimize hot paths (search, authorize payment) to < 500ms p95

- [ ] T142 [Polish] Implement response caching: add ResponseCaching middleware in Program.cs: builder.Services.AddResponseCaching(), app.UseResponseCaching(), add caching to read-only endpoints: GET /api/claims/{id} with Cache-Control: max-age=60, GET /api/dashboard/overview with max-age=30, vary by query string for filtered endpoints, use ETags for conditional requests (If-None-Match returning 304 Not Modified), validate cache behavior with browser dev tools

- [ ] T143 [Polish] Frontend performance optimization: code splitting with React.lazy() for large components (MigrationDashboardPage, ClaimDetailPage), lazy load Recharts library (dynamic import when charts are visible), optimize images (compress logo PNG if needed, use WebP with PNG fallback), enable Vite build optimizations (minification, tree-shaking, chunk splitting), use React Query caching (staleTime, cacheTime) to avoid redundant API calls, implement virtual scrolling for large tables (react-window for 1000+ rows), measure with Lighthouse (target: Performance score > 90)

### Testing & Quality

- [ ] T144 [Polish] Achieve 80%+ code coverage: run dotnet test --collect:"XPlus Code Coverage", analyze report with dotnet-coverage or ReportGenerator, add missing unit tests for: validators (all FluentValidation rules), services (all business logic methods), repositories (query methods), controllers (all endpoints with success/error cases), AutoMapper profiles (all mappings), generate coverage badge for README

- [ ] T145 [Polish] Load testing: create k6 or NBomber script at `tests/load/claim-search-load-test.js` simulating 1000 concurrent users: ramp up over 30s, sustain for 5 minutes, mix of search, authorize payment, get history operations (70% read, 30% write), target: p95 response time < 2000ms, error rate < 1%, no database deadlocks, no memory leaks (monitor memory usage over 30 minutes), document results in `docs/performance-test-results.md`

- [ ] T146 [Polish] E2E test suite execution: run all Playwright tests in CI pipeline (GitHub Actions or Azure DevOps), test on multiple browsers (Chromium, Firefox, WebKit), generate HTML report with screenshots on failure, configure test retries (max 2) for flaky tests, run E2E tests on every PR before merge, target: 100% pass rate for critical flows (search, payment authorization, dashboard load), maintain test execution time < 10 minutes

### Documentation & Deployment

- [X] T147 [Polish] Deployment preparation: create production Dockerfile for backend at `deployment/docker/Dockerfile.backend` with multi-stage build (restore, build, publish, runtime), create production Dockerfile for frontend at `deployment/docker/Dockerfile.frontend` with nginx serving static files, create Kubernetes manifests at `deployment/kubernetes/` (deployment.yaml with 3 replicas, service.yaml with LoadBalancer, ingress.yaml with TLS, configmap.yaml, secrets.yaml for connection strings), create Azure App Service deployment script at `deployment/azure/deploy-app-service.sh` (az webapp create, deployment slots for blue-green, connection string configuration, app settings), document deployment process in `docs/deployment-guide.md` (prerequisites, step-by-step instructions, rollback procedure, monitoring setup)

---

## Parallel Execution Examples

### Phase 1 (Setup) - All tasks can run in parallel:
```
T001, T002, T003, T004, T005, T006, T007, T008, T009 can execute simultaneously
Wait for all to complete before T010 (Docker compose needs all configs ready)
```

### Phase 2 (Foundation) - Entity configurations:
```
T011-T023 (all entity classes) can execute in parallel
T024 (DbContext) depends on T011-T023 completion
T025 (EF configurations) can run parallel with T024
T026-T030 (repositories) depend on T024-T025 completion
```

### Phase 3 (US1) - Independent tasks:
```
Parallel group A: T031, T032, T033 (DTOs and validators)
Parallel group B: T036 (AutoMapper) can start after T031, T032 complete
Parallel group C: T042, T043 (frontend models/api) independent of backend
T034, T035 (service) depend on T031-T033
T037-T039 (controller) depend on T034-T035
T040, T041 (middleware) can run parallel with T037-T039
T044-T046 (frontend pages) depend on T042, T043
T048-T050 (tests) run after all implementation complete
```

### Phase 4 (US2) - Complex dependencies:
```
Parallel group A: T051, T052, T053 (payment DTOs/validators)
Parallel group B: T058, T059, T060, T061, T062 (external service clients - all parallel)
Parallel group C: T064, T065, T066 (SOAP services - parallel)
Sequential: T054 -> T055 (currency interface -> implementation)
Sequential: T051-T055 -> T056 -> T057 (payment authorization service)
Sequential: T057 + T058-T061 -> T063 (controller integrates both)
Parallel: T067, T068, T069, T070 (all frontend components after T063 complete)
Tests T071-T075 run after all implementation complete
```

### Phase 8 (US6 Dashboard) - Fully parallel with operational features:
```
US6 (T111-T130) can execute entirely in parallel with US2-US5
Backend: T111-T117 sequential (interface -> DTOs -> service implementation)
Backend: T118-T122 (controller endpoints) depend on T111-T117
Frontend: T123-T129 (dashboard components) depend on T118-T122
Tests: T130 run after T123-T129 complete
```

---

## Story Completion Order

**Recommended implementation order:**

1. **Phase 1 + Phase 2** (Setup + Foundation) - **MUST be first** - All other phases depend on this
2. **Phase 3 (US1 - Search)** - **MVP** - Entry point for all operations
3. **Phase 8 (US6 - Dashboard)** - **Can run parallel from step 2** - Independent of operational features
4. **Phase 4 (US2 - Payment)** - Requires US1 search to retrieve claims
5. **Phase 5 (US3 - History)** - Requires US2 to have payment data
6. **Phase 6 (US4 - Consortium)** - Enhances US2 payment with product validation
7. **Phase 7 (US5 - Phases)** - Triggered by US2 payment authorization
8. **Phase 9 (Polish)** - Final integration across all features

**Critical path**: Phase 1 -> Phase 2 -> Phase 3 -> Phase 4 -> Phase 9
**Parallel path**: Phase 8 (Dashboard) starts after Phase 2, runs parallel to Phase 3-7

---

## Testing Strategy Per Story

### US1 (Search) - Independent Testing:
```bash
# Backend integration test
dotnet test --filter "FullyQualifiedName~ClaimSearchIntegrationTests"

# Frontend component test
npm test -- ClaimSearchPage.test.tsx

# E2E test
npx playwright test claim-search.spec.ts

# Performance validation
curl -w "@curl-format.txt" -o /dev/null -s https://localhost:5001/api/claims/search
# Verify response time < 3 seconds (SC-001)
```

### US2 (Payment) - Requires US1:
```bash
# Integration test with external service mocks
dotnet test --filter "FullyQualifiedName~PaymentAuthorizationIntegrationTests"

# E2E test full cycle
npx playwright test payment-authorization.spec.ts
# Verify cycle completes in < 90 seconds (SC-002)

# Load test
k6 run tests/load/payment-load-test.js
```

### US6 (Dashboard) - Independent Testing:
```bash
# Dashboard endpoints
dotnet test --filter "FullyQualifiedName~DashboardIntegrationTests"

# Dashboard components
npm test -- dashboard/

# E2E dashboard
npx playwright test migration-dashboard.spec.ts
# Verify 30-second auto-refresh (SC-014)
```

---

## Task Completion Checklist

After completing all 147 tasks, verify:

- [ ] All 6 user stories implemented and tested
- [ ] All 55 functional requirements met (trace FR-001 to FR-055)
- [ ] All 19 success criteria validated (trace SC-001 to SC-019)
- [ ] All 13 entities configured in EF Core with migrations (if needed)
- [ ] All 9 REST endpoints implemented and documented in Swagger
- [ ] All 3 SOAP services implemented with WSDL generation
- [ ] Site.css integrated without modifications
- [ ] Logo displays correctly from base64 PNG
- [ ] Portuguese error messages throughout
- [ ] < 3 seconds claim search (SC-001)
- [ ] < 90 seconds payment cycle (SC-002)
- [ ] 2 decimal precision for currency (SC-008)
- [ ] Mobile responsive at 850px (SC-010)
- [ ] Dashboard 30-second refresh (SC-014)
- [ ] 80%+ code coverage
- [ ] Load test passed (1000 concurrent users)
- [ ] Security audit clean (no high/critical vulnerabilities)
- [ ] Deployment scripts tested

---

## Getting Started

**To begin implementation:**

1. Start with MVP scope: Complete Phase 1 (T001-T010), Phase 2 (T011-T030), Phase 3 (T031-T050)
2. For each task, read the full description and ensure all file paths and requirements are clear
3. Mark tasks complete with `[x]` when finished
4. Run tests after each phase to validate integration
5. Use parallel execution wherever marked `[P]` to accelerate development
6. Update this document if scope changes or new tasks are discovered

**Total Estimated Effort**: ~147 tasks × 2-4 hours average = 294-588 hours (6-12 weeks with 2 developers)

---

**Document Version**: 1.0
**Last Updated**: 2025-10-23
**Generated By**: Specify Task Generation (/speckit.tasks)
