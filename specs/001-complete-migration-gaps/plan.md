# Implementation Plan: Complete SIWEA Migration - Gaps Analysis and Implementation

**Branch**: `001-complete-migration-gaps` | **Date**: 2025-10-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-complete-migration-gaps/spec.md`

**Note**: This plan addresses the identified 35-40% gaps required to achieve 100% SIWEA migration from IBM Visual Age EZEE 4.40 to .NET 9 + React.

## Summary

Complete the remaining 35-40% of SIWEA migration by implementing 5 critical gap areas: (1) External service validation integration with CNOUA, SIPUA, SIMDA using Polly resilience policies, (2) Complete transaction atomicity across 4-table payment authorization pipeline with rollback handling, (3) Enforcement of 55+ missing business rules from BR-001 to BR-099, (4) UI/Display completion including Site.css integration and Portuguese error messages, (5) Comprehensive testing infrastructure with 80%+ code coverage. Technical approach uses existing .NET 9 Clean Architecture backend with EF Core, React 19 frontend with TypeScript, and leverages existing entity models and repository infrastructure to complete missing service implementations.

## Technical Context

**Language/Version**: .NET 9.0 (C# 13) backend, React 19.1.1 with TypeScript 5.9 frontend
**Primary Dependencies**: ASP.NET Core 9.0, Entity Framework Core 9.0, SoapCore 1.1.0, Polly 8.2.0, FluentValidation 11.9.0, AutoMapper 12.0.1, Serilog 4.0.0, React Router 7.9.4, Axios 1.12.2, TanStack Query 5.90.5
**Storage**: SQL Server (legacy schema database-first approach, no migrations permitted)
**Testing**: xUnit with Moq for unit tests, TestServer for integration tests, Playwright for E2E tests
**Target Platform**: Azure App Service (Linux containers), modern browsers (Chrome, Firefox, Safari, Edge)
**Project Type**: Web application (backend REST/SOAP API + frontend React SPA)
**Performance Goals**: Search < 3 seconds all modes, Payment authorization < 90 seconds end-to-end, History query < 500ms for 1000+ records
**Constraints**: 100% backward compatibility with legacy database schema (10 legacy tables), preserve Site.css exactly, all error messages in Portuguese, no modifications to existing entity models
**Scale/Scope**: 13 database entities, 100+ business rules, 6 user stories, 43 functional requirements, supporting hundreds of concurrent insurance operators

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: PASS (No project constitution defined - using Clean Architecture principles and existing codebase conventions)

This feature completion work adheres to the following architectural principles already established in the codebase:

✅ **Clean Architecture Layers**: Core (domain entities, interfaces) → Infrastructure (repositories, external services) → API (controllers, middleware)
✅ **Database-First Approach**: Existing legacy schema preserved, EF Core configurations already defined, no migrations
✅ **Test-Driven Development**: Unit tests for business rules, integration tests for workflows, E2E tests for user journeys
✅ **Separation of Concerns**: Business rules in validators, transaction logic in services, external integrations in dedicated clients
✅ **Portuguese Localization**: All error messages from ErrorMessages.pt-BR resource file
✅ **Performance Requirements**: Documented SLAs with performance tests verifying compliance

No violations or exceptions required. All gap implementations follow existing patterns established in 60-65% completed codebase.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── CaixaSeguradora.Api/
│   │   ├── Controllers/
│   │   │   ├── ClaimsController.cs (✅ exists)
│   │   │   ├── PaymentAuthorizationController.cs (✅ exists)
│   │   │   └── DashboardController.cs (✅ exists)
│   │   ├── SoapServices/
│   │   │   ├── ISolicitacaoService.cs (✅ exists)
│   │   │   └── SolicitacaoService.cs (✅ exists)
│   │   ├── Middleware/
│   │   │   └── GlobalExceptionHandlerMiddleware.cs (✅ exists)
│   │   ├── Mappings/
│   │   │   └── ClaimMappingProfile.cs (✅ exists)
│   │   └── Program.cs (✅ exists)
│   ├── CaixaSeguradora.Core/
│   │   ├── Entities/ (✅ 13 entities exist)
│   │   ├── Interfaces/
│   │   │   ├── IClaimRepository.cs (✅ exists)
│   │   │   ├── IExternalValidationService.cs (✅ exists)
│   │   │   └── [NEW] ICnouaValidationClient.cs (❌ to implement)
│   │   ├── Services/
│   │   │   └── ClaimService.cs (✅ exists, needs enhancement)
│   │   ├── Validators/
│   │   │   ├── ClaimSearchValidator.cs (✅ exists)
│   │   │   ├── PaymentAuthorizationValidator.cs (✅ exists)
│   │   │   └── [NEW] BusinessRulesValidator.cs (❌ to implement)
│   │   ├── DTOs/ (✅ exists)
│   │   ├── Exceptions/ (✅ exists)
│   │   └── Resources/
│   │       └── ErrorMessages.pt-BR.resx (✅ exists)
│   └── CaixaSeguradora.Infrastructure/
│       ├── Services/
│       │   ├── PaymentAuthorizationService.cs (🔶 partial - needs atomicity)
│       │   ├── CurrencyConversionService.cs (✅ exists)
│       │   ├── PhaseManagementService.cs (✅ exists)
│       │   ├── [NEW] TransactionCoordinator.cs (❌ to implement)
│       │   └── DashboardService.cs (✅ exists)
│       ├── ExternalServices/
│       │   └── [NEW] ExternalValidationClient.cs (❌ to implement)
│       │   └── [NEW] CnouaValidationClient.cs (❌ to implement)
│       │   └── [NEW] SipuaValidationClient.cs (❌ to implement)
│       │   └── [NEW] SimdaValidationClient.cs (❌ to implement)
│       ├── Repositories/
│       │   ├── ClaimRepository.cs (✅ exists)
│       │   ├── Repository.cs (✅ exists)
│       │   └── UnitOfWork.cs (✅ exists)
│       ├── Data/
│       │   ├── ClaimsDbContext.cs (✅ exists)
│       │   └── Configurations/ (✅ 13 entity configs exist)
│       └── HealthChecks/
│           └── ExternalServiceHealthCheck.cs (✅ exists)
└── tests/
    ├── CaixaSeguradora.Api.Tests/
    │   └── Integration/ (🔶 partial - needs completion)
    ├── CaixaSeguradora.Core.Tests/
    │   └── [NEW] BusinessRules/ (❌ 100+ rule tests to implement)
    └── CaixaSeguradora.Infrastructure.Tests/
        └── Services/ (🔶 partial - needs completion)

frontend/
├── src/
│   ├── components/
│   │   ├── claims/
│   │   │   ├── PaymentAuthorizationForm.tsx (✅ exists)
│   │   │   ├── ClaimInfoCard.tsx (✅ exists)
│   │   │   └── ClaimPhasesComponent.tsx (✅ exists)
│   │   ├── common/
│   │   │   ├── Logo.tsx (✅ exists)
│   │   │   └── CurrencyInput.tsx (✅ exists)
│   │   ├── dashboard/ (✅ exists - 6 components)
│   │   └── ui/ (✅ exists - shadcn components)
│   ├── pages/
│   │   └── [NEW] Search, Authorization, History pages (❌ to enhance)
│   ├── services/
│   │   └── [NEW] api.ts, claims.ts (❌ to implement)
│   ├── models/
│   │   └── Claim.ts (✅ exists)
│   ├── utils/
│   │   └── errorMessages.ts (✅ exists)
│   └── App.tsx (✅ exists)
└── public/
    └── Site.css (✅ exists - must preserve exactly)
```

**Structure Decision**: Web application with Clean Architecture backend (.NET 9) and React SPA frontend. Existing structure retained with gaps filled in Infrastructure/ExternalServices layer (validation clients), Core/Validators layer (business rules), and tests directories. Legend: ✅ exists, 🔶 partial implementation, ❌ to implement.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

N/A - No constitutional violations. Implementation follows existing Clean Architecture patterns already established in codebase.
