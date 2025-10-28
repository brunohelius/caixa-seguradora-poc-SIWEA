# Implementation Plan: Visual Age Technical Documentation & Gap Implementation

**Branch**: `001-visualage-technical-docs` | **Date**: 2025-10-23 | **Spec**: [spec.md](./spec.md)
**Input**: Complete technical documentation of Visual Age legacy system + Gap analysis and implementation of missing functionality

## Summary

**Phase 1 - COMPLETE ✅**: Created 2,923 lines of comprehensive technical documentation extracting 100+ business rules from Visual Age EZEE 4.40 source code. Documentation organized in `docs/` and referenced in `CLAUDE.md`.

**Phase 2 - IN PROGRESS ⏳**: Systematically compare documented requirements against current .NET 9 + React 19 implementation, identify gaps, and implement all missing functionality to achieve 100% functional parity.

**Approach**: Documentation-driven implementation using Business Rules Index (BR-001 to BR-099) as checklist, prioritized by tier (System-Critical → Business-Critical → Operational).

## Technical Context

**Backend Stack**:
- Language/Version: C# with .NET 9.0
- Primary Dependencies: ASP.NET Core 9.0, Entity Framework Core 9.0, SoapCore 1.1.0, FluentValidation 11.9.0, AutoMapper 12.0.1, Serilog 4.0.0, Polly 8.2.0
- Storage: SQL Server (legacy schema, database-first approach)
- Testing: xUnit, Moq, Playwright
- Architecture: Clean Architecture (API → Core → Infrastructure)

**Frontend Stack**:
- Language/Version: TypeScript 5.9 with React 19.1.1
- Primary Dependencies: Vite 7.1.7, React Router DOM 7.9.4, Axios 1.12.2, TanStack React Query 5.90.5, shadcn/ui (Radix UI primitives), Tailwind CSS 4.x
- Testing: Playwright (E2E), Vitest (unit), React Testing Library
- UI Components: Fully migrated to shadcn/ui (22/22 tests passing)

**Platform**:
- Target Platform: Azure App Service (Web App + API) or Azure Kubernetes Service
- Performance Goals: Search < 3s, Payment Authorization < 90s, Dashboard < 30s
- Constraints: 100% functional parity with Visual Age, no database schema changes, preserve Site.css, Portuguese language
- Scale: 1000+ concurrent users, 13 database entities, 100+ business rules

## Constitution Check

*GATE: N/A - This is a documentation and migration project, not governed by constitution principles for new features.*

**Rationale**: This feature documents and migrates an existing legacy system rather than creating new architecture. The constitution would apply to new features built *after* the migration is complete.

## Project Structure

### Documentation (this feature)

```text
specs/001-visualage-technical-docs/
├── spec.md              # Feature specification (COMPLETE ✅)
├── plan.md              # This file (IN PROGRESS ⏳)
├── research.md          # Phase 0 output (N/A - no unknowns)
├── data-model.md        # Phase 1 output (EXISTS - see docs/LEGACY_SIWEA_COMPLETE_ANALYSIS.md)
├── quickstart.md        # Phase 1 output (WILL CREATE)
├── contracts/           # Phase 1 output (WILL CREATE - OpenAPI specs for missing APIs)
└── tasks.md             # Phase 2 output (PENDING - via /speckit.tasks)
```

### Source Code (repository root)

```text
# Existing structure (Web application - Option 2)
backend/
├── src/
│   ├── CaixaSeguradora.Api/              # REST/SOAP controllers, DTOs, middleware
│   │   ├── Controllers/                  # Payment, Search, Dashboard controllers
│   │   ├── DTOs/                         # Request/response models
│   │   └── Program.cs                    # DI configuration
│   ├── CaixaSeguradora.Core/             # Domain entities, business logic, validators
│   │   ├── Entities/                     # TMESTSIN, THISTSIN, etc. (13 entities)
│   │   ├── Interfaces/                   # Repository and service interfaces
│   │   ├── Validators/                   # FluentValidation rules
│   │   └── Services/                     # Business logic services
│   └── CaixaSeguradora.Infrastructure/   # EF Core, repositories, external clients
│       ├── Data/                         # DbContext, configurations
│       ├── Repositories/                 # Repository implementations
│       └── ExternalServices/             # CNOUA, SIPUA, SIMDA clients
└── tests/
    ├── CaixaSeguradora.Api.Tests/
    ├── CaixaSeguradora.Core.Tests/
    └── CaixaSeguradora.Infrastructure.Tests/

frontend/
├── src/
│   ├── components/                       # Reusable UI components (shadcn/ui)
│   │   ├── claims/                       # SearchForm, HistoryTable, ClaimInfoCard
│   │   ├── dashboard/                    # OverviewCards, PhaseTimeline
│   │   └── ui/                           # shadcn/ui components (Button, Input, etc.)
│   ├── pages/                            # Page-level components
│   │   ├── ClaimsSearchPage.tsx
│   │   ├── PaymentAuthorizationPage.tsx  # INCOMPLETE
│   │   ├── ClaimDetailsPage.tsx          # MISSING
│   │   └── MigrationDashboardPage.tsx
│   ├── services/                         # API clients (Axios)
│   │   ├── claimsApi.ts
│   │   └── paymentApi.ts                 # INCOMPLETE
│   ├── models/                           # TypeScript interfaces
│   └── utils/                            # Helpers, formatters
└── tests/
    ├── e2e/                              # Playwright E2E tests (22/22 passing ✅)
    ├── puppeteer/                        # Additional E2E tests
    └── component-validation/             # Component tests

docs/
├── README_ANALYSIS.md                    # Navigation guide (✅ COMPLETE)
├── LEGACY_SIWEA_COMPLETE_ANALYSIS.md     # Full technical spec (✅ COMPLETE)
├── BUSINESS_RULES_INDEX.md               # 100+ rules indexed (✅ COMPLETE)
└── ANALYSIS_SUMMARY.md                   # Executive summary (✅ COMPLETE)
```

**Structure Decision**: Existing web application structure (backend + frontend) is appropriate. All legacy documentation is in `docs/`. No structural changes needed - only gap implementation within existing directories.

## Complexity Tracking

> N/A - No constitution violations. This is a migration project following existing architecture patterns.

---

## Phase 0: Research ⏭️ SKIPPED

**Status**: Not required

**Rationale**: All technical unknowns were resolved during Visual Age source code analysis. The 2,923 lines of documentation in `docs/LEGACY_SIWEA_COMPLETE_ANALYSIS.md` contains all architectural decisions, formulas, business rules, and integration patterns extracted from the legacy system.

**Decisions Already Documented**:
- External service routing logic (CNOUA/SIPUA/SIMDA) → documented in `docs/product-validation-routing.md`
- Phase management workflow → documented in `docs/phase-management-workflow.md`
- Currency conversion formulas → documented in LEGACY_SIWEA_COMPLETE_ANALYSIS.md
- Database schema → 13 entities fully mapped in LEGACY_SIWEA_COMPLETE_ANALYSIS.md
- Performance requirements → documented in `docs/performance-notes.md`

**Output**: `research.md` **NOT CREATED** (no unknowns to research)

---

## Phase 1: Design & Contracts

**Status**: Partially Complete (data model exists, contracts/quickstart needed)

### 1.1 Data Model ✅ COMPLETE

**Source**: `docs/LEGACY_SIWEA_COMPLETE_ANALYSIS.md` (Section: "Database Schema")

**Entities** (13 total):
1. ClaimMaster (TMESTSIN) - Main claim record
2. ClaimHistory (THISTSIN) - Payment transactions
3. BranchMaster (TGERAMO) - Branch information
4. CurrencyUnit (TGEUNIMO) - Currency conversion rates
5. SystemControl (TSISTEMA) - Business date control
6. PolicyMaster (TAPOLICE) - Insured party info
7. ClaimAccompaniment (SI_ACOMPANHA_SINI) - Workflow events
8. ClaimPhase (SI_SINISTRO_FASE) - Processing phases
9. PhaseEventRelationship (SI_REL_FASE_EVENTO) - Phase configuration
10. ConsortiumContract (EF_CONTR_SEG_HABIT) - Consortium contracts
11. MigrationStatus - Migration progress tracking
12. ComponentMigrationTracking - Component status
13. PerformanceMetrics - Benchmarking data

**Note**: Full field mappings, relationships, and indexes documented in `docs/LEGACY_SIWEA_COMPLETE_ANALYSIS.md`. Will create simplified `data-model.md` referencing this primary source.

### 1.2 API Contracts ⏳ TO DO

**Required Contracts** (based on gap analysis):

1. **Payment Authorization API** (`/contracts/payment-authorization-api.yaml`)
   - POST `/api/claims/{claimId}/authorize-payment` - Authorize payment with validation
   - GET `/api/claims/{claimId}/payment-history` - Get THISTSIN records
   - POST `/api/claims/{claimId}/validate-beneficiary` - Validate beneficiary logic

2. **Claim Search API** (`/contracts/claim-search-api.yaml`)
   - POST `/api/claims/search/by-protocol` - Search by fonte/protsini/dac
   - POST `/api/claims/search/by-claim-number` - Search by orgsin/rmosin/numsin
   - POST `/api/claims/search/by-leader-code` - Search by codlider/sinlid

3. **External Services API** (`/contracts/external-services-api.yaml`)
   - POST `/api/external/cnoua/validate` - CNOUA consortium validation
   - POST `/api/external/sipua/validate` - SIPUA EFP contract validation
   - POST `/api/external/simda/validate` - SIMDA HB contract validation

4. **Phase Management API** (`/contracts/phase-management-api.yaml`)
   - POST `/api/claims/{claimId}/phases/open` - Open new phase (9999-12-31 marker)
   - PUT `/api/claims/{claimId}/phases/{phaseId}/close` - Close phase with actual date
   - GET `/api/claims/{claimId}/phases` - Get phase timeline

### 1.3 Quickstart Guide ⏳ TO DO

**File**: `quickstart.md`

**Contents**:
- How to use the technical documentation suite
- Gap analysis workflow (map BR-XXX rules to code)
- Implementation priority guide (System-Critical → Business-Critical → Operational)
- Testing strategy for Visual Age parity
- Quick reference: key formulas and critical business rules

---

## Phase 2: Tasks (via `/speckit.tasks`)

**Status**: NOT YET GENERATED

**Next Steps**:
1. Complete Phase 1 (create contracts/ and quickstart.md)
2. Run `/speckit.tasks` to generate implementation tasks
3. Tasks will be derived from gap analysis findings

**Expected Task Categories**:
- Backend: Implement missing business rules (BR-XXX)
- Backend: Complete external service clients (CNOUA, SIPUA, SIMDA)
- Backend: Implement phase management system
- Backend: Add transaction atomicity enforcement
- Frontend: Complete payment authorization form
- Frontend: Add claim details screen
- Frontend: Implement Portuguese error messages
- Testing: Create parity tests comparing Visual Age vs .NET outputs

---

## Gap Analysis Summary (Preview)

**Backend Gaps** (High Priority):
- ❌ Phase management (open/close with 9999-12-31 marker) - **BR-045**
- ❌ Transaction atomicity across all database updates - **BR-002**
- ❌ Complete audit trail (EZEUSRID operator tracking) - **BR-048**
- ❌ Consortium routing (products 6814/7701/7709 → CNOUA) - **BR-073**
- ❌ EFP contract routing (EF_CONTR_SEG_HABIT → SIPUA) - **BR-074**
- ❌ Operation code 1098 enforcement - **BR-031**
- ❌ Correction type '5' enforcement - **BR-032**
- ⚠️ Currency conversion date range validation - **BR-040** (partial)

**Frontend Gaps** (High Priority):
- ❌ Protocol search validation (fonte/protsini/dac) - **BR-011**
- ❌ Leader code search implementation - **BR-013**
- ❌ Beneficiary conditional logic (required if TPSEGU != 0) - **BR-022**
- ❌ Currency display with BTNF conversion - **BR-041**
- ❌ Payment history table (THISTSIN records) - UI only
- ❌ Claim details screen (SIHM020 equivalent) - UI only
- ❌ Portuguese error messages (24 messages) - **ER-001 to ER-024**

**Estimated Completion**: 40-60% of business logic implemented (infrastructure exists, critical rules missing)

---

## Next Actions

1. ✅ **COMPLETE**: Documentation phase (2,923 lines created)
2. ✅ **COMPLETE**: CLAUDE.md integration
3. ⏳ **IN PROGRESS**: Create `data-model.md` (simplified reference to docs/)
4. ⏳ **IN PROGRESS**: Create `contracts/` (4 OpenAPI specs for missing APIs)
5. ⏳ **IN PROGRESS**: Create `quickstart.md` (gap analysis workflow guide)
6. ⏳ **PENDING**: Run `/speckit.tasks` to generate implementation tasks
7. ⏳ **PENDING**: Implement all System-Critical gaps (BR tier 1)
8. ⏳ **PENDING**: Implement all Business-Critical gaps (BR tier 2)
9. ⏳ **PENDING**: Create parity tests (Visual Age vs .NET outputs)

---

## Success Metrics

**Documentation Success** ✅:
- 100% of database entities documented
- 100+ business rules extracted and indexed
- All 24 error messages captured
- 3 external service integrations specified
- CLAUDE.md updated with Technical Documentation section

**Implementation Success** ⏳:
- All System-Critical business rules implemented and tested
- All Business-Critical business rules implemented and tested
- Parity tests demonstrate identical behavior for core flows
- Frontend UI matches legacy functionality with modern UX

**Timeline**:
- Phase 1 (Contracts + Quickstart): 1-2 days
- Phase 2 (Gap Implementation): 2-4 weeks (depends on complexity of missing rules)
- Parity Testing: 1 week (continuous with implementation)

---

**Plan Status**: ✅ Phase 0 (Research) skipped - all unknowns resolved | ⏳ Phase 1 (Design & Contracts) in progress
**Next Command**: Complete Phase 1 artifacts, then run `/speckit.tasks` to generate implementation task list
