# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **proof-of-concept migration** of a legacy IBM VisualAge EZEE Claims Indemnity Payment Authorization System (SIWEA) to a modern stack:
- **Backend**: .NET 9.0 Web API with Clean Architecture
- **Frontend**: React 19 + TypeScript with Vite
- **Purpose**: Migrate legacy mainframe insurance claims system to cloud-native architecture while preserving 100% business logic

The legacy system (`#SIWEA-V116.esf`) handles insurance claim searches, payment authorizations, workflow phases, and integrations with external validation services.

## Technology Stack

### Backend (.NET 9.0)
- **Framework**: ASP.NET Core 9.0
- **ORM**: Entity Framework Core 9.0 (database-first approach)
- **SOAP Support**: SoapCore 1.1.0
- **Validation**: FluentValidation 11.9.0
- **Mapping**: AutoMapper 12.0.1
- **Logging**: Serilog 4.0.0
- **Resilience**: Polly 8.2.0
- **Authentication**: JWT Bearer tokens
- **Testing**: xUnit, Moq

### Frontend (React 19)
- **Framework**: React 19.1.1 with TypeScript 5.9
- **Build Tool**: Vite 7.1.7
- **Router**: React Router DOM 7.9.4
- **HTTP Client**: Axios 1.12.2
- **State/Data**: TanStack React Query 5.90.5
- **Charts**: Recharts 3.3.0
- **Testing**: Jest, React Testing Library (to be configured)

### Architecture
- **Pattern**: Clean Architecture (API → Core → Infrastructure)
- **Layers**:
  - `CaixaSeguradora.Api`: REST/SOAP controllers, DTOs, middleware
  - `CaixaSeguradora.Core`: Domain entities, business logic, validators, interfaces
  - `CaixaSeguradora.Infrastructure`: EF Core, repositories, external service clients

## Project Structure

```
backend/
├── src/
│   ├── CaixaSeguradora.Api/              # Web API + SOAP endpoints
│   ├── CaixaSeguradora.Core/             # Domain logic (framework-agnostic)
│   └── CaixaSeguradora.Infrastructure/   # Data access + external services
└── tests/
    ├── CaixaSeguradora.Api.Tests/
    ├── CaixaSeguradora.Core.Tests/
    └── CaixaSeguradora.Infrastructure.Tests/

frontend/
├── src/
│   ├── components/    # Reusable React components
│   ├── pages/         # Page-level components
│   ├── services/      # API clients (Axios)
│   ├── models/        # TypeScript interfaces
│   └── utils/         # Helpers, formatters
└── public/
    └── Site.css       # Legacy stylesheet (must be preserved)

specs/001-visualage-dotnet-migration/
├── spec.md           # Feature specification (6 user stories, 55 requirements)
├── plan.md           # Implementation plan
├── research.md       # Architectural decisions (Phase 0)
├── data-model.md     # Entity models (Phase 1)
├── tasks.md          # Task breakdown (Phase 2)
└── contracts/        # OpenAPI specs, SOAP WSDLs, JSON schemas
```

## Common Development Commands

### Backend

```bash
# Navigate to backend
cd backend

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run API (development)
cd src/CaixaSeguradora.Api
dotnet run

# Run with hot reload
dotnet watch run

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/CaixaSeguradora.Core.Tests/

# Run single test
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Clean and rebuild
dotnet clean
dotnet build --configuration Release
```

**API Endpoints**:
- API: https://localhost:5001
- Swagger: https://localhost:5001/swagger
- SOAP endpoints: https://localhost:5001/soap/*

### Frontend

```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Run development server (http://localhost:3000)
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run linter
npm run lint

# Run tests (when configured)
npm test
```

### Full Stack (Docker)

```bash
# Build and run all services
docker-compose up --build

# Run in background
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f
```

## Key Architecture Patterns

### Clean Architecture Flow
```
HTTP Request → Controller (API) → Service (Core) → Repository (Infrastructure) → Database
                    ↓
                  DTO ← AutoMapper ← Entity
```

### Dependency Injection
All services are registered in `Program.cs`:
- DbContext with connection pooling
- Repository pattern implementations
- Domain services with business logic
- External service clients with Polly policies
- AutoMapper profiles
- Serilog configuration

### Database Access
- **Database-first approach**: Existing legacy schema, no migrations
- **Fluent API**: Map legacy table/column names to C# entities
- **Transaction support**: Multi-table updates (claim master + history + phase)
- **Providers**: SQL Server via Microsoft.Data.SqlClient (DB2 support planned)

Example entity configuration:
```csharp
modelBuilder.Entity<ClaimMaster>(entity =>
{
    entity.ToTable("TMESTSIN"); // Legacy table name
    entity.HasKey(e => e.NumSinistro);
    entity.Property(e => e.ValorPrincipal)
        .HasColumnName("VALPRI")
        .HasColumnType("decimal(15,2)");
});
```

### SOAP Endpoint Support
SoapCore library provides SOAP endpoints alongside REST API:
- Namespace: `http://ls.caixaseguradora.com.br/LS1134WSV0001_Autenticacao/v1`
- Endpoints: `/soap/autenticacao`, `/soap/solicitacao`, `/soap/assunto`
- Authentication: Session-based tokens (sessionId)

### External Service Integration
Three validation services with Polly resilience:
- **CNOUA**: Consortium product validation (codes 6814, 7701, 7709)
- **SIPUA**: EFP contract validation
- **SIMDA**: HB contract validation

Configured with circuit breaker, retry policies, and timeout handling.

## Important Constraints

1. **No Schema Modifications**: Must work with existing database tables (13 entities)
2. **Preserve Site.css**: UI must use legacy stylesheet without changes (960px max-width)
3. **100% Business Logic Parity**: All 42 business rules from Visual Age must be preserved
4. **Portuguese Language**: All error messages and UI text in Portuguese
5. **Decimal Precision**: Currency calculations accurate to 2 decimal places
6. **Logo Display**: Caixa Seguradora logo from base64 PNG (in spec.md)
7. **Performance**: Search < 3s, authorization cycle < 90s, dashboard refresh < 30s

## Testing Strategy

### Backend Testing
- **Unit Tests**: Business logic in Core layer (xUnit + Moq)
- **Integration Tests**: API endpoints with TestServer
- **Repository Tests**: Database operations with in-memory provider
- **Contract Tests**: External service integrations (Pact)
- **Parity Tests**: Compare Visual Age outputs with .NET outputs

### Frontend Testing
- **Component Tests**: Jest + React Testing Library
- **E2E Tests**: Playwright or Cypress for critical user journeys
- **Visual Regression**: Compare UI with legacy system screenshots

### Critical Test Scenarios
1. Search claim by protocol/claim number/leader code
2. Payment authorization with validation
3. Currency conversion accuracy (BTNF standardized currency)
4. Transaction rollback on validation failure
5. Phase/workflow state changes
6. Consortium product routing (CNOUA validation)
7. Dashboard real-time updates

## Configuration Management

### Backend Configuration
- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Local overrides
- **Secrets**: Azure Key Vault in production, User Secrets in development
- **Connection String**: SQL Server (DB2 support planned)

```bash
# Set user secrets (development)
dotnet user-secrets init --project src/CaixaSeguradora.Api
dotnet user-secrets set "ConnectionStrings:ClaimsDatabase" "Server=..." --project src/CaixaSeguradora.Api
```

### Frontend Configuration
Environment variables in `.env` files:
```
VITE_API_BASE_URL=https://localhost:5001
VITE_DASHBOARD_REFRESH_INTERVAL=30000
```

## Migration Dashboard

A dedicated dashboard (`/dashboard` route) tracks migration progress:
- Overall progress percentage
- User story status (6 stories: search, authorization, history, consortium, workflow, dashboard)
- Component migration (screens, business rules, database entities, external services)
- Performance comparison (Visual Age vs .NET metrics)
- Recent activities timeline
- System health indicators

Dashboard follows reference design: https://sicoob-sge3-jv1x.vercel.app/dashboard

## Development Workflow

1. **Feature Development**: Work in feature branch `001-visualage-dotnet-migration`
2. **Specification-Driven**: All work driven by `specs/001-visualage-dotnet-migration/spec.md`
3. **Task Tracking**: Use `tasks.md` for implementation tasks (generated via `/speckit.tasks`)
4. **Contracts First**: Define OpenAPI/JSON schemas before implementation
5. **Test-Driven**: Write tests for business rules before implementation
6. **Parity Validation**: Compare every output with Visual Age system

## Known Technical Debt / TODOs

- DB2 database provider configuration (currently SQL Server only)
- Complete E2E test suite (Playwright/Cypress)
- Production deployment scripts (Azure App Service / AKS)
- SOAP WSDL generation and documentation
- Performance benchmarking against Visual Age system
- Load testing for 1000+ concurrent users
- Migration runbook documentation

## References

- **Legacy Source**: `#SIWEA-V116.esf` (IBM VisualAge EZEE 4.40)
- **Stylesheet**: `Site.css` (must be preserved exactly)
- **Feature Spec**: `specs/001-visualage-dotnet-migration/spec.md`
- **Implementation Plan**: `specs/001-visualage-dotnet-migration/plan.md`
- **Research Decisions**: `specs/001-visualage-dotnet-migration/research.md`
- **Data Model**: `specs/001-visualage-dotnet-migration/data-model.md`

## Technical Documentation (Complete Business Rules)

**START HERE** for complete understanding of the legacy system and all business rules:

- **📖 README**: `docs/README_ANALYSIS.md` - Quick navigation guide and how to use the documentation
- **📊 Complete Analysis**: `docs/LEGACY_SIWEA_COMPLETE_ANALYSIS.md` - Full technical specification (1,725 lines)
  - 13 database entities with complete field mappings
  - 100+ business rules extracted from Visual Age source code
  - Complete payment authorization pipeline (8 steps)
  - External service integration specs (CNOUA, SIPUA, SIMDA)
  - Phase/workflow management system
  - Currency conversion formulas
  - All 24 error messages (Portuguese)
- **🎯 Business Rules Index**: `docs/BUSINESS_RULES_INDEX.md` - All rules indexed with IDs (BR-001 to BR-099)
  - Tier classification (System-Critical, Business-Critical, Operational)
  - Quick lookup tables
  - Testing strategy
- **📝 Executive Summary**: `docs/ANALYSIS_SUMMARY.md` - High-level overview for stakeholders
  - Key findings
  - Implementation checklist
  - Data flow diagrams

**Additional Documentation**:
- **Product Validation Routing**: `docs/product-validation-routing.md` - Consortium product routing logic
- **Phase Management Workflow**: `docs/phase-management-workflow.md` - Phase/event workflow system
- **Performance Notes**: `docs/performance-notes.md` - Performance requirements and benchmarks

## Entity Model Summary

13 key database entities (legacy schema):
1. **ClaimMaster** (TMESTSIN): Main claim record with protocol, policy, financial summary
2. **ClaimHistory** (THISTSIN): Payment authorization transactions
3. **BranchMaster** (TGERAMO): Branch information
4. **CurrencyUnit** (TGEUNIMO): Currency conversion rates (BTNF)
5. **SystemControl** (TSISTEMA): Current business date
6. **PolicyMaster** (TAPOLICE): Insured party information
7. **ClaimAccompaniment** (SI_ACOMPANHA_SINI): Workflow events
8. **ClaimPhase** (SI_SINISTRO_FASE): Processing phases
9. **PhaseEventRelationship** (SI_REL_FASE_EVENTO): Phase configuration
10. **ConsortiumContract** (EF_CONTR_SEG_HABIT): Consortium contracts

Dashboard entities (new):
11. **MigrationStatus**: Project progress tracking
12. **ComponentMigrationTracking**: Component-level status
13. **PerformanceMetrics**: Benchmarking data

## Critical Business Rules

- **Payment Types**: Must be 1, 2, 3, 4, or 5
- **Beneficiary**: Required if insurance type (tpsegu) != 0
- **Operation Code**: Always 1098 for payment authorization
- **Correction Type**: Always '5' for all authorizations
- **Currency Conversion**: VALPRIBT = VALPRI × VLCRUZAD (from TGEUNIMO with date range validation)
- **Transaction Atomicity**: Rollback all changes if any phase update fails
- **Audit Trail**: Record operator ID (EZEUSRID) on all transactions
- **Phase Management**: Open phase with 9999-12-31 closing date, update on closure
- **Consortium Routing**: Products 6814, 7701, 7709 → CNOUA validation

## Support Commands

```bash
# View solution structure
dotnet sln backend/CaixaSeguradora.sln list

# Add new project
dotnet new classlib -n CaixaSeguradora.NewProject -o backend/src/CaixaSeguradora.NewProject
dotnet sln backend/CaixaSeguradora.sln add backend/src/CaixaSeguradora.NewProject

# Update all NuGet packages
dotnet list backend/CaixaSeguradora.sln package --outdated
dotnet add package PackageName --version x.y.z

# EF Core commands (from Infrastructure project directory)
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef dbcontext scaffold "ConnectionString" Provider --output-dir Entities
```

## Active Technologies
- .NET 9.0 (C# 13) backend, React 19.1.1 with TypeScript 5.9 frontend + ASP.NET Core 9.0, Entity Framework Core 9.0, SoapCore 1.1.0, Polly 8.2.0, FluentValidation 11.9.0, AutoMapper 12.0.1, Serilog 4.0.0, React Router 7.9.4, Axios 1.12.2, TanStack Query 5.90.5 (001-complete-migration-gaps)
- SQL Server (legacy schema database-first approach, no migrations permitted) (001-complete-migration-gaps)

## Recent Changes
- 001-complete-migration-gaps: Added .NET 9.0 (C# 13) backend, React 19.1.1 with TypeScript 5.9 frontend + ASP.NET Core 9.0, Entity Framework Core 9.0, SoapCore 1.1.0, Polly 8.2.0, FluentValidation 11.9.0, AutoMapper 12.0.1, Serilog 4.0.0, React Router 7.9.4, Axios 1.12.2, TanStack Query 5.90.5
