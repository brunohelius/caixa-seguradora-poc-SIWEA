# Implementation Plan: Visual Age Claims System Migration to .NET 9 + React

**Branch**: `001-visualage-dotnet-migration` | **Date**: 2025-10-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-visualage-dotnet-migration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This plan outlines the complete migration of the legacy IBM VisualAge EZEE Claims Indemnity Payment Authorization System (SIWEA) to a modern .NET 9 backend with React frontend. The migration must preserve 100% of existing business logic while providing a migration status dashboard for project visibility. The system handles insurance claim searches, payment authorizations, history tracking, consortium product validation, and workflow phase management across multiple database entities and external validation services.

**Primary Technical Approach**:
- **Backend**: .NET 9 Web API with Clean Architecture pattern (API, Core, Infrastructure layers)
- **Frontend**: React 18+ with TypeScript for type safety and maintainability
- **SOAP Support**: SoapCore library for legacy SOAP endpoint compatibility
- **Database**: Entity Framework Core with existing DB2/SQL Server schema (no migrations)
- **Styling**: Existing Site.css stylesheet for UI consistency
- **Dashboard**: Real-time migration tracking with card-based layout, charts, and progress indicators

## Technical Context

**Language/Version**:
- Backend: C# 12 with .NET 9.0
- Frontend: TypeScript 5.x with React 18+

**Primary Dependencies**:
- Backend: ASP.NET Core 9.0, SoapCore 1.x, Entity Framework Core 9.0, AutoMapper 13.x, Serilog 4.x
- Frontend: React 18+, React Router 6.x, Axios 1.x, Recharts/Chart.js for dashboard visualizations
- Database: IBM.Data.DB2 or Microsoft.Data.SqlClient (depending on target database)
- Testing: xUnit, Moq, React Testing Library

**Storage**:
- Legacy database tables (DB2 or SQL Server) - read/write via EF Core
- Separate storage for migration dashboard metrics (SQL Server, PostgreSQL, or JSON files)
- No schema changes to existing claim processing tables

**Testing**:
- Backend: xUnit with Moq for unit tests, integration tests with TestServer
- Frontend: Jest + React Testing Library for component tests
- E2E: Playwright or Cypress for critical user journeys
- Legacy parity tests: Compare outputs between Visual Age and .NET implementations

**Target Platform**:
- Backend: Linux containers (Docker) or Windows Server, deployable to Azure App Service/AKS
- Frontend: Modern browsers (Chrome, Firefox, Safari, Edge) - desktop and mobile viewports
- Development: Cross-platform (Windows, macOS, Linux)

**Project Type**: Web application (backend + frontend monorepo)

**Performance Goals**:
- Search/retrieve claim: < 3 seconds (SC-001)
- Payment authorization cycle: < 90 seconds (SC-002)
- Dashboard refresh: < 30 seconds (SC-014)
- Support 1000+ concurrent users without degradation
- API response times: < 500ms p95 for CRUD operations

**Constraints**:
- Zero data loss during migration
- 100% functional parity with Visual Age system
- Preserve all 42 business rules without modification
- Support 3 external validation services (CNOUA, SIPUA, SIMDA) via HTTP/SOAP
- Maintain Portuguese language and error messages
- Site.css must be used without modifications (960px max-width constraint)
- Logo must render from base64 PNG data

**Scale/Scope**:
- 6 user stories (5 operational + 1 dashboard)
- 55 functional requirements
- 13 database entities (10 legacy + 3 dashboard)
- 2 main screens (search + detail), 1 dashboard
- 3 external service integrations
- ~50-100 concurrent operators during business hours
- Historical data volume: unknown (assume millions of claim records)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: ✅ PASSED - No constitution file exists yet; project follows industry-standard Clean Architecture principles

**Notes**:
- Constitution file (`.specify/memory/constitution.md`) is empty template
- This migration follows well-established patterns for legacy system modernization:
  - Clean Architecture (Ports & Adapters pattern)
  - TDD where applicable for new business logic
  - Contract testing for external service integrations
  - Comprehensive integration tests for database operations
  - E2E tests for critical user workflows

**Post-Design Re-check**: Will validate that chosen architecture aligns with .NET and React best practices

## Project Structure

### Documentation (this feature)

```text
specs/001-visualage-dotnet-migration/
├── plan.md              # This file (/speckit.plan command output)
├── spec.md              # Feature specification (complete)
├── research.md          # Phase 0 output (architectural decisions, library choices)
├── data-model.md        # Phase 1 output (entity models, relationships, validations)
├── quickstart.md        # Phase 1 output (setup, run, test instructions)
├── contracts/           # Phase 1 output (OpenAPI specs, SOAP WSDLs, schemas)
│   ├── rest-api.yaml    # REST API contract (OpenAPI 3.0)
│   ├── soap-autenticacao.wsdl
│   ├── soap-solicitacao.wsdl
│   ├── soap-assunto.wsdl
│   └── schemas/         # JSON schemas for request/response validation
└── checklists/
    └── requirements.md  # Quality validation checklist (complete)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── CaixaSeguradora.Api/              # ASP.NET Core Web API + SOAP endpoints
│   │   ├── Controllers/                   # REST controllers (ClaimsController, DashboardController)
│   │   ├── SoapServices/                  # SOAP service implementations
│   │   ├── Middleware/                    # Error handling, logging, auth
│   │   ├── DTOs/                          # Data transfer objects
│   │   ├── Mappings/                      # AutoMapper profiles
│   │   ├── Program.cs                     # Application entry point
│   │   └── appsettings.json               # Configuration
│   │
│   ├── CaixaSeguradora.Core/             # Domain layer (business logic)
│   │   ├── Entities/                      # Domain entities (Claim, ClaimHistory, etc.)
│   │   ├── Interfaces/                    # Repository and service interfaces
│   │   ├── Services/                      # Business logic services
│   │   ├── Validators/                    # Business rule validators
│   │   └── Enums/                         # Domain enumerations
│   │
│   └── CaixaSeguradora.Infrastructure/   # Data access and external services
│       ├── Data/                          # EF Core DbContext, configurations
│       ├── Repositories/                  # Repository implementations
│       ├── ExternalServices/              # CNOUA, SIPUA, SIMDA clients
│       ├── Logging/                       # Serilog configuration
│       └── Migrations/                    # EF Core migrations (if needed)
│
└── tests/
    ├── CaixaSeguradora.Api.Tests/        # API integration tests
    ├── CaixaSeguradora.Core.Tests/       # Business logic unit tests
    └── CaixaSeguradora.Infrastructure.Tests/  # Repository and service tests

frontend/
├── public/
│   ├── index.html
│   └── Site.css                          # Migrated stylesheet (from POC)
│
├── src/
│   ├── components/                       # Reusable React components
│   │   ├── common/                       # Buttons, inputs, modals
│   │   ├── claims/                       # Claim-specific components
│   │   └── dashboard/                    # Dashboard cards, charts, metrics
│   │
│   ├── pages/                            # Page-level components
│   │   ├── ClaimSearchPage.tsx           # P1: Search and retrieve
│   │   ├── ClaimDetailPage.tsx           # P2: Authorize payment + P3: View history
│   │   └── MigrationDashboardPage.tsx    # P6: Migration status
│   │
│   ├── services/                         # API clients
│   │   ├── claimsApi.ts                  # REST/SOAP claim operations
│   │   ├── dashboardApi.ts               # Migration metrics
│   │   └── httpClient.ts                 # Axios configuration
│   │
│   ├── models/                           # TypeScript interfaces
│   │   ├── Claim.ts
│   │   ├── ClaimHistory.ts
│   │   └── MigrationStatus.ts
│   │
│   ├── utils/                            # Utility functions
│   │   ├── formatters.ts                 # Date, currency formatting
│   │   └── validators.ts                 # Client-side validation
│   │
│   ├── App.tsx                           # Root component
│   ├── index.tsx                         # Application entry point
│   └── routes.tsx                        # React Router configuration
│
└── tests/
    ├── components/                       # Component unit tests
    ├── integration/                      # Integration tests
    └── e2e/                              # End-to-end tests (Playwright/Cypress)

deployment/
├── docker/
│   ├── Dockerfile.backend
│   ├── Dockerfile.frontend
│   └── docker-compose.yml                # Local development stack
│
├── kubernetes/                           # K8s manifests (if deploying to AKS)
│   ├── backend-deployment.yaml
│   ├── frontend-deployment.yaml
│   └── ingress.yaml
│
└── azure/                                # Azure deployment scripts
    └── deploy-app-service.sh
```

**Structure Decision**:
Web application monorepo with separate backend and frontend directories. This structure:
- Clearly separates concerns (API vs UI)
- Allows independent deployment if needed
- Supports different development workflows (C# vs TypeScript)
- Aligns with Clean Architecture in backend (API → Core → Infrastructure)
- Follows React best practices in frontend (components, pages, services pattern)

## Complexity Tracking

**Status**: ✅ NO VIOLATIONS

This migration follows industry-standard patterns and does not introduce unjustified complexity:

- **Clean Architecture**: Well-documented pattern for enterprise applications
- **Monorepo**: Common for full-stack applications, simplifies versioning and CI/CD
- **EF Core**: Standard ORM for .NET, handles existing schema without migrations
- **SoapCore**: Necessary for SOAP compatibility, minimal API surface
- **React + TypeScript**: Industry standard for modern web UIs with type safety
- **Site.css preservation**: Requirement from spec, no additional complexity

All architectural decisions are justified by functional requirements and success criteria.

## Phase 0: Research & Decisions

### Research Areas

The following technical decisions require investigation and documentation in `research.md`:

#### 1. Database Access Strategy
**Question**: How to connect .NET 9 to legacy DB2/SQL Server tables without schema modifications?

**Research Tasks**:
- Investigate EF Core providers: IBM.Data.DB2.Core vs Microsoft.Data.SqlClient
- Database-first approach with reverse engineering existing schema
- Fluent API configurations for legacy table/column mappings
- Connection string security (Azure Key Vault vs appsettings)
- Transaction handling for multi-table updates (claim master + history + phase)

**Decision Criteria**:
- Zero schema changes (FR-053 referential integrity requirement)
- Support for DB2 stored procedures if needed
- Performance for large result sets (millions of claim records)

#### 2. SOAP Endpoint Implementation
**Question**: Best approach to expose SOAP endpoints alongside REST API?

**Research Tasks**:
- SoapCore vs manual WSDL generation
- SOAP authentication mechanism (same as REST or separate?)
- Namespace mapping: `http://ls.caixaseguradora.com.br/LS1134WSV0001_Autenticacao/v1`
- SOAP fault handling and error responses
- Compatibility with legacy CICS SOAP clients

**Decision Criteria**:
- Functional parity with Visual Age SOAP interfaces
- Support for session-based auth (sessionId tokens)
- TLS/HTTPS requirement

#### 3. External Service Integration
**Question**: How to integrate with CNOUA, SIPUA, SIMDA validation services?

**Research Tasks**:
- Service discovery: Are these HTTP, SOAP, or message queue based?
- Error code mapping: EZERT8 != '00000000' handling
- Retry policies and circuit breakers (Polly library)
- Timeout configurations
- Logging and monitoring for external calls

**Decision Criteria**:
- Preserve exact error messages (FR-023)
- Handle service unavailability gracefully
- Support for rollback if validation fails (FR-024, FR-030)

#### 4. Currency Conversion Logic
**Question**: How to implement BTNF conversion with date-based rates?

**Research Tasks**:
- TGEUNIMO table structure and rate lookup logic
- Date range validation: DTINIVIG <= DTMOVABE <= DTTERVIG
- Decimal precision handling (2 decimal places per SC-008)
- Missing rate scenarios (edge case from spec)
- Formula verification: VALPRIBT = VALPRI * VLCRUZAD

**Decision Criteria**:
- Accuracy to 2 decimal places
- Handle missing rates for transaction date
- Support for currency conversion audit trail

#### 5. Dashboard Real-Time Updates
**Question**: Best technology for auto-refresh without page reload?

**Research Tasks**:
- SignalR for WebSocket-based push updates
- Polling with Axios intervals (simpler, more compatible)
- Server-Sent Events (SSE) as middle ground
- State management: Redux vs Context API vs Zustand

**Decision Criteria**:
- 30-second refresh requirement (FR-044)
- Browser compatibility
- Server resource usage at scale
- Implementation complexity

#### 6. Charting Library Selection
**Question**: Which library for dashboard visualizations?

**Research Tasks**:
- Recharts (React-native, composable)
- Chart.js with react-chartjs-2 wrapper
- Nivo (D3-based, customizable)
- Victory (Formidable, mobile-friendly)

**Decision Criteria**:
- Card-based layout support
- Progress bars, pie charts, line charts, bar charts
- Green color theme (#00A859) customization
- Responsive design
- TypeScript support

#### 7. Testing Strategy
**Question**: How to verify 100% functional parity with Visual Age?

**Research Tasks**:
- Snapshot testing: Record Visual Age outputs, compare with .NET
- Legacy system access for parallel testing
- Test data generation: Synthetic vs production copy
- Contract testing for external services (Pact)
- E2E test scenarios: Map to acceptance criteria from spec

**Decision Criteria**:
- Coverage for all 55 functional requirements
- Automated regression tests
- Performance benchmarking (Visual Age vs .NET metrics)

#### 8. Authentication & Authorization
**Question**: How to map EZEUSRID to modern auth system?

**Research Tasks**:
- Enterprise authentication integration: Active Directory, OAuth2, SAML
- JWT token generation and validation
- User ID mapping: EZEUSRID → current user principal
- Role-based access control if needed
- Session management

**Decision Criteria**:
- Seamless operator experience
- Audit trail preservation (FR-051, FR-052)
- Compliance with enterprise security policies

### Research Output Format

Each research area will be documented in `research.md` with:

```markdown
### [Research Area]

**Decision**: [What was chosen]

**Rationale**: [Why this choice was made, benefits]

**Alternatives Considered**:
- [Option A]: [Pros/cons, why not chosen]
- [Option B]: [Pros/cons, why not chosen]

**Implementation Notes**: [Key configuration details, gotchas, references]

**Risks/Tradeoffs**: [Known limitations, technical debt, future considerations]
```

## Phase 1: Design & Contracts

### Data Model (data-model.md)

Extract all entities from spec Key Entities section and define:
- C# class structure with properties
- EF Core Fluent API configurations for legacy table mapping
- Validation attributes (Required, Range, StringLength)
- Relationships (ForeignKey, navigation properties)
- State transitions (if applicable for workflow phases)

**Primary Entities**:
1. ClaimMaster (maps to TMESTSIN)
2. ClaimHistory (maps to THISTSIN)
3. BranchMaster (maps to TGERAMO)
4. CurrencyUnit (maps to TGEUNIMO)
5. SystemControl (maps to TSISTEMA)
6. PolicyMaster (maps to TAPOLICE)
7. ClaimAccompaniment (maps to SI_ACOMPANHA_SINI)
8. ClaimPhase (maps to SI_SINISTRO_FASE)
9. PhaseEventRelationship (maps to SI_REL_FASE_EVENTO)
10. ConsortiumContract (maps to EF_CONTR_SEG_HABIT)

**Dashboard Entities** (new tables/storage):
11. MigrationStatus
12. ComponentMigrationTracking
13. PerformanceMetrics

### API Contracts (contracts/)

Generate OpenAPI 3.0 specification for REST API:

**Endpoints**:
- `POST /api/claims/search` - Search by protocol, claim number, or leader code (FR-001, FR-002)
- `GET /api/claims/{id}` - Retrieve claim details (FR-003, FR-004)
- `POST /api/claims/{id}/authorize-payment` - Create payment authorization (FR-006-FR-011)
- `GET /api/claims/{id}/history` - Get payment history (P3 user story)
- `GET /api/claims/{id}/phases` - Get workflow phases (P5 user story)
- `GET /api/dashboard/overview` - Migration dashboard overview (FR-038-FR-050)
- `GET /api/dashboard/components` - Component migration status (FR-041)
- `GET /api/dashboard/performance` - Performance comparison metrics (FR-043)
- `GET /api/dashboard/activities` - Recent activities timeline (FR-050)

**SOAP Endpoints** (WSDL generation):
- `/soap/autenticacao` - Authentication service (maps to Visual Age namespace)
- `/soap/solicitacao` - Request/solicitation service
- `/soap/assunto` - Subject/topic management service

**JSON Schemas**:
- ClaimSearchRequest, ClaimSearchResponse
- PaymentAuthorizationRequest, PaymentAuthorizationResponse
- DashboardOverviewResponse
- Error response schemas with Portuguese messages

### Quickstart Guide (quickstart.md)

Developer onboarding document with:

1. **Prerequisites**:
   - .NET 9 SDK installation
   - Node.js 18+ and npm/yarn
   - Docker Desktop (for local DB)
   - Visual Studio Code or Visual Studio 2022
   - Git

2. **Clone and Setup**:
   ```bash
   git clone [repo-url]
   cd "POC Visual Age"
   git checkout 001-visualage-dotnet-migration
   ```

3. **Backend Setup**:
   ```bash
   cd backend
   dotnet restore
   dotnet build
   # Configure connection string in appsettings.Development.json
   dotnet run --project src/CaixaSeguradora.Api
   ```

4. **Frontend Setup**:
   ```bash
   cd frontend
   npm install
   npm start  # Runs on http://localhost:3000
   ```

5. **Run Tests**:
   ```bash
   # Backend tests
   cd backend
   dotnet test

   # Frontend tests
   cd frontend
   npm test
   ```

6. **Docker Compose** (full stack):
   ```bash
   docker-compose up
   ```

7. **Access Points**:
   - Frontend: http://localhost:3000
   - Backend API: https://localhost:5001
   - Swagger UI: https://localhost:5001/swagger
   - SOAP endpoints: https://localhost:5001/soap/*
   - Migration Dashboard: http://localhost:3000/dashboard

### Agent Context Update

After completing Phase 1 artifacts, run:

```bash
.specify/scripts/bash/update-agent-context.sh claude
```

This will update `.specify/memory/claude-context.md` with:
- Technology stack choices (.NET 9, React 18, EF Core, SoapCore)
- Architectural patterns (Clean Architecture, Repository pattern)
- Key libraries and their purposes
- Testing frameworks and strategies
- Deployment targets (Docker, Azure)

## Phase 2: Task Breakdown (NOT in this command)

Phase 2 (task generation via `/speckit.tasks`) will create `tasks.md` with dependency-ordered implementation tasks. Preview of task categories:

1. **Infrastructure Setup** (T001-T010):
   - Project scaffolding, solution structure
   - NuGet/npm package installation
   - CI/CD pipeline configuration
   - Docker setup

2. **Database Layer** (T011-T025):
   - EF Core DbContext and entity configurations
   - Repository implementations
   - Connection string management
   - Integration tests for data access

3. **Core Business Logic** (T026-T045):
   - Domain entities and validators
   - Service implementations (ClaimService, PaymentService, PhaseService)
   - Business rule logic (42 rules from spec)
   - Currency conversion calculations
   - Unit tests for all services

4. **API Layer** (T046-T060):
   - REST controllers with DTOs
   - SOAP service implementations
   - AutoMapper profiles
   - Middleware (error handling, logging, auth)
   - API integration tests

5. **External Service Integration** (T061-T070):
   - CNOUA, SIPUA, SIMDA clients
   - Circuit breaker and retry policies
   - Error mapping
   - Contract tests

6. **Frontend Components** (T071-T090):
   - ClaimSearchPage with search form
   - ClaimDetailPage with payment authorization
   - Payment history display
   - Dashboard cards, charts, metrics
   - Shared components (buttons, inputs, modals)

7. **Styling and Branding** (T091-T095):
   - Site.css integration
   - Logo display
   - Responsive layout testing
   - Mobile viewport verification

8. **Dashboard Features** (T096-T110):
   - Real-time data refresh mechanism
   - Migration status tracking
   - Component migration grid
   - Performance comparison charts
   - Activities timeline

9. **Testing & Quality** (T111-T125):
   - E2E tests for critical flows
   - Parity tests (Visual Age vs .NET)
   - Performance benchmarking
   - Load testing
   - Security testing

10. **Deployment & Documentation** (T126-T135):
    - Docker images and compose files
    - Azure deployment scripts
    - API documentation (Swagger)
    - User guide updates
    - Migration runbook

## Implementation Sequence

**Recommended order**:

1. ✅ **Phase 0 Complete**: Research all technical decisions → `research.md`
2. ✅ **Phase 1 Complete**: Design data model, API contracts → `data-model.md`, `contracts/`, `quickstart.md`
3. **Phase 2** (via `/speckit.tasks`): Generate detailed task list → `tasks.md`
4. **Implementation** (via `/speckit.implement`): Execute tasks in dependency order
5. **Testing**: Validate functional parity and performance
6. **Deployment**: Roll out to staging, then production

## Success Criteria Mapping

Each success criterion from spec maps to verifiable outcomes:

- **SC-001 (< 3s search)**: Performance test with timer assertions
- **SC-002 (< 90s authorization)**: E2E test measuring full cycle time
- **SC-003 (100% data accuracy)**: Data comparison tests between systems
- **SC-004 (100% rule preservation)**: Business logic unit tests covering all 42 rules
- **SC-005 (UI identical)**: Visual regression tests with Site.css
- **SC-006 (all product types)**: Integration tests for products 6814, 7701, 7709, and standard
- **SC-007 (complete audit trail)**: Database assertion tests for EZEUSRID in all records
- **SC-008 (2 decimal precision)**: Currency conversion tests with exact comparisons
- **SC-009 (rollback prevention)**: Transaction rollback tests
- **SC-010 (mobile responsive)**: Viewport tests at 850px and below
- **SC-011 (external integrations)**: Contract tests for CNOUA, SIPUA, SIMDA
- **SC-012 (95% operator success)**: User acceptance testing metrics
- **SC-013 (logo display)**: Visual test for logo rendering
- **SC-014 (dashboard 30s refresh)**: Auto-refresh timing test
- **SC-015 (dashboard visibility)**: UI test verifying all metrics displayed
- **SC-016 (bottleneck identification)**: Dashboard UX test
- **SC-017 (5 key metrics)**: Dashboard content assertions
- **SC-018 (100% drill-down)**: Dashboard interaction tests
- **SC-019 (visual design)**: Dashboard layout comparison with reference

## Risk Mitigation

### High Risks

1. **Database Schema Unknowns**:
   - **Risk**: Legacy DB2 schema may have undocumented constraints or triggers
   - **Mitigation**: Reverse engineer full schema, test all CRUD operations in dev environment

2. **External Service Availability**:
   - **Risk**: CNOUA, SIPUA, SIMDA may be unavailable or have changed APIs
   - **Mitigation**: Create mock services for development, implement circuit breakers, plan fallback workflows

3. **Business Rule Complexity**:
   - **Risk**: 42 business rules may have hidden dependencies or edge cases
   - **Mitigation**: Extract all ESQL logic from Visual Age source, create comprehensive test matrix, parallel testing

4. **Performance Under Load**:
   - **Risk**: .NET implementation may be slower than optimized mainframe COBOL/ESQL
   - **Mitigation**: Early performance benchmarking, database query optimization, caching strategy

5. **SOAP Compatibility**:
   - **Risk**: SoapCore may not fully replicate Visual Age SOAP behavior
   - **Mitigation**: Thorough SOAP contract testing, validate with actual Visual Age clients if available

### Medium Risks

6. **Currency Conversion Edge Cases**:
   - **Risk**: Missing rates for transaction dates, leap year handling, timezone issues
   - **Mitigation**: Comprehensive test data covering all date scenarios, clear error messages

7. **Concurrent User Handling**:
   - **Risk**: Optimistic concurrency violations when multiple operators edit same claim
   - **Mitigation**: EF Core concurrency tokens, clear user messaging, retry logic

8. **Dashboard Data Volume**:
   - **Risk**: Dashboard queries may become slow with large migration datasets
   - **Mitigation**: Implement pagination, caching, database indexing

### Low Risks

9. **Site.css Compatibility**:
   - **Risk**: CSS may not work well with React component structure
   - **Mitigation**: Early UI prototyping, CSS scoping strategy

10. **Logo Display**:
    - **Risk**: Base64 PNG may not render correctly in all browsers
    - **Mitigation**: Cross-browser testing, fallback to file-based logo

## Next Steps

After `/speckit.plan` completion:

1. **Review and approve** this plan with technical leads and stakeholders
2. **Execute Phase 0**: Create `research.md` by researching all 8 decision areas
3. **Execute Phase 1**: Create `data-model.md`, `contracts/`, and `quickstart.md`
4. **Run** `/speckit.tasks` to generate detailed task breakdown in `tasks.md`
5. **Run** `/speckit.implement` to begin executing tasks
6. **Track progress** via migration dashboard (becomes self-referential once built!)

## References

- Feature Specification: [spec.md](./spec.md)
- Visual Age Source: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/#SIWEA-V116.esf`
- Stylesheet: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/Site.css`
- Dashboard Reference: https://sicoob-sge3-jv1x.vercel.app/dashboard
- .NET 9 Documentation: https://learn.microsoft.com/en-us/dotnet/
- React Documentation: https://react.dev/
- SoapCore GitHub: https://github.com/DigDes/SoapCore
- EF Core Documentation: https://learn.microsoft.com/en-us/ef/core/
