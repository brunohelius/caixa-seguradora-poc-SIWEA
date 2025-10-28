# Feature Specification: Visual Age Claims System Migration to .NET 9 + React

**Feature Branch**: `001-visualage-dotnet-migration`
**Created**: 2025-10-23
**Status**: Draft
**Input**: User description: "vamos criar a migracao desse projeto visual age /Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/#SIWEA-V116.esf para .net9 com react frontend. Migre ele 100% e use esse css /Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/Site.css"

## User Scenarios & Testing

### User Story 1 - Search and Retrieve Claim (Priority: P1)

Insurance operators need to search for existing claims using multiple criteria (protocol number, claim number, or leader code) to view claim details and initiate payment authorization processes.

**Why this priority**: This is the entry point for all claim operations - without the ability to search and retrieve claims, no other functionality can be accessed. This represents the core MVP.

**Independent Test**: Can be fully tested by entering a valid protocol number and verifying that the system displays the correct claim information (claim number, policy number, insured name, reserve amount, payments made, pending value).

**Acceptance Scenarios**:

1. **Given** an operator is on the claim search screen, **When** they enter a valid protocol number (fonte, protsini, dac), **Then** the system displays the claim details including claim number, policy, insured name, reserve amount, total payments, and pending value
2. **Given** an operator is on the claim search screen, **When** they enter a valid claim number (orgsin, rmosin, numsin), **Then** the system displays the same claim details
3. **Given** an operator is on the claim search screen, **When** they enter a valid leader code and leader claim number, **Then** the system displays the claim details
4. **Given** an operator enters an invalid or non-existent document number, **When** they submit the search, **Then** the system displays an error message "DOCUMENTO XXXXXXXXXXXXXXX NAO CADASTRADO" and allows them to try again
5. **Given** an operator is on the claim search screen, **When** they do not enter any search criteria, **Then** the system displays a validation error indicating at least one search field is required

---

### User Story 2 - Authorize Claim Payment (Priority: P2)

After retrieving a claim, operators need to enter payment authorization details including payment type, principal amount, correction value, and beneficiary information to authorize indemnity payments.

**Why this priority**: This is the primary business operation - authorizing payments. Depends on Story 1 (search) being completed first.

**Independent Test**: Can be tested by retrieving a valid claim, entering valid payment details (type 1-5, principal value, correction value, beneficiary name), and verifying the system accepts the authorization and updates claim history.

**Acceptance Scenarios**:

1. **Given** a claim is displayed with pending value greater than zero, **When** the operator enters valid payment type (1-5), principal amount, and correction value, **Then** the system accepts the payment authorization
2. **Given** a claim has insurance type (tpsegu) = 0, **When** the operator authorizes payment, **Then** the beneficiary field is optional
3. **Given** a claim has insurance type (tpsegu) != 0, **When** the operator authorizes payment without entering beneficiary, **Then** the system displays a validation error requiring beneficiary name
4. **Given** an operator enters payment details, **When** they submit the authorization, **Then** the system creates a history record (THISTSIN) with operation code 1098, current date/time, and entered values
5. **Given** an operator enters payment details, **When** the authorization is confirmed, **Then** the system increments the claim occurrence counter (ocorhist) by 1 in the master record (TMESTSIN)
6. **Given** an operator enters a non-numeric or invalid payment type, **When** they try to submit, **Then** the system displays a validation error indicating payment type must be 1, 2, 3, 4, or 5

---

### User Story 3 - View Payment History and Audit Trail (Priority: P3)

Operators need to view complete payment history for claims, including all previous authorizations, amounts, dates, and operators who made changes for audit and compliance purposes.

**Why this priority**: Important for audit and compliance, but not required for basic payment operations. Can be added after core payment functionality works.

**Independent Test**: Can be tested by retrieving a claim with existing payment history and verifying that all historical records are displayed with correct dates, amounts, operators, and operation codes.

**Acceptance Scenarios**:

1. **Given** a claim with payment history, **When** the operator views the claim, **Then** the system displays all historical payment records sorted by occurrence number
2. **Given** a payment history record, **When** displayed, **Then** it shows operation code, date, time, operator, principal value, correction value, beneficiary, and accounting status
3. **Given** multiple payments for the same claim, **When** viewing history, **Then** each record shows the cumulative effect on reserve and pending values

---

### User Story 4 - Handle Special Products (Consortium) (Priority: P4)

The system must handle special validation and document verification for consortium products (codes 6814, 7701, 7709) by routing to appropriate validation processes and checking contract tables.

**Why this priority**: Required for specific product types but not blocking for general claim payment functionality. Can be implemented after core stories.

**Independent Test**: Can be tested by processing claims for product code 6814 and verifying the system calls the correct validation routine (CNOUA) and checks consortium-specific tables (EF_CONTR_SEG_HABIT).

**Acceptance Scenarios**:

1. **Given** a claim for product code 6814, 7701, or 7709, **When** validating the claim, **Then** the system calls the consortium validation routine (CNOUA)
2. **Given** a claim with contract in EFP tables (NUM_CONTRATO > 0), **When** validating, **Then** the system calls the EFP validation process (SIPUA)
3. **Given** a claim with contract in HB tables (NUM_CONTRATO = 0), **When** validating, **Then** the system calls the HB validation process (SIMDA)
4. **Given** consortium validation fails, **When** processing the claim, **Then** the system displays the specific error message from the validation routine

---

### User Story 5 - Manage Claim Phase and Workflow (Priority: P5)

The system must track and update claim processing phases (abertura, fechamento) based on events, maintaining phase history and allowing operators to view current phase status.

**Why this priority**: Important for workflow management but not critical for basic payment authorization. Can be implemented incrementally.

**Independent Test**: Can be tested by authorizing a payment and verifying the system updates the claim phase, creates phase tracking records (SI_SINISTRO_FASE), and records the event in claim history (SI_ACOMPANHA_SINI).

**Acceptance Scenarios**:

1. **Given** a payment authorization is made, **When** the transaction completes, **Then** the system creates an accompanying record (SI_ACOMPANHA_SINI) with event code, date, user, and description
2. **Given** an event that opens a phase (IND_ALTERACAO_FASE = '1'), **When** processing, **Then** the system creates a phase record with opening date and future closing date (9999-12-31)
3. **Given** an event that closes a phase (IND_ALTERACAO_FASE = '2'), **When** processing, **Then** the system updates the existing phase record with actual closing date
4. **Given** a phase is already open or closed, **When** attempting to repeat the same operation, **Then** the system prevents duplicate phase records

---

### User Story 6 - Migration Status Dashboard (Priority: P6)

Technical and management teams need a visual dashboard to monitor the migration progress from Visual Age to .NET 9, tracking completed features, pending tasks, system health, and performance metrics in real-time.

**Why this priority**: Important for project management and stakeholder visibility, but not required for end-user operations. Can be developed in parallel with core functionality.

**Independent Test**: Can be tested by accessing the dashboard and verifying that all migration statistics, progress indicators, feature completion status, and system health metrics are displayed accurately and update in real-time.

**Acceptance Scenarios**:

1. **Given** the migration project is in progress, **When** accessing the dashboard, **Then** the system displays overall migration progress percentage with visual progress bar
2. **Given** multiple user stories are being implemented, **When** viewing the dashboard, **Then** each user story shows its completion status (Not Started, In Progress, Completed, Blocked) with progress indicators
3. **Given** system components are being migrated, **When** viewing the dashboard, **Then** it displays component-level migration status including screens, business rules, database integrations, and validation processes
4. **Given** automated tests are running, **When** viewing the dashboard, **Then** it shows test execution results, pass/fail rates, and code coverage metrics
5. **Given** performance benchmarks are established, **When** viewing the dashboard, **Then** it displays comparison metrics between legacy Visual Age system and new .NET 9 implementation (response times, throughput, resource usage)
6. **Given** the dashboard is being viewed, **When** data updates occur, **Then** the dashboard automatically refreshes without requiring manual page reload
7. **Given** the user needs detailed information, **When** clicking on any dashboard card or metric, **Then** the system displays drill-down details with historical trends and specific task breakdowns

---

### Edge Cases

- What happens when an operator enters a payment amount exceeding the pending value (SDOPAG - TOTPAG)?
- How does the system handle concurrent updates to the same claim by multiple operators?
- What happens if validation routines (CNOUA, SIPUA, SIMDA) are unavailable or return errors?
- How does the system handle date/time synchronization when recording operation timestamps?
- What happens when monetary conversion rates (VLCRUZAD from TGEUNIMO) are not available for the current date?
- How does the system handle rollback if phase update fails after payment authorization succeeds?
- What happens when maximum occurrence counter (ocorhist) reaches its limit?
- How does the system handle special characters or encoding issues in beneficiary names?

## Requirements

### Functional Requirements

#### Core Search and Retrieval
- **FR-001**: System MUST allow operators to search claims using one of three mutually exclusive criteria: protocol number (fonte + protsini + dac), claim number (orgsin + rmosin + numsin), or leader code + leader claim number (codlider + sinlid)
- **FR-002**: System MUST validate that at least one complete search criterion is provided before executing search
- **FR-003**: System MUST retrieve claim data from master table (TMESTSIN) and display protocol, branch, policy number, claim number, insured name, expected reserve, total payments, and pending value
- **FR-004**: System MUST display error message "DOCUMENTO XXXXXXXXXXXXXXX NAO CADASTRADO" when claim is not found
- **FR-005**: System MUST retrieve branch name from branch master table (TGERAMO) using branch code from claim

#### Payment Authorization
- **FR-006**: System MUST require payment type selection from values 1, 2, 3, 4, or 5
- **FR-007**: System MUST require principal amount (valor principal) as numeric mandatory field
- **FR-008**: System MUST allow correction value (valor da correcao) as optional numeric field
- **FR-009**: System MUST conditionally require beneficiary name (favorecido) based on insurance type: optional when tpsegu = 0, mandatory otherwise
- **FR-010**: System MUST validate all numeric fields accept only valid numeric input
- **FR-011**: System MUST validate policy type (tipo de apolice) as mandatory field accepting only values 1 or 2

#### Transaction Recording
- **FR-012**: System MUST create history record (THISTSIN) with operation code 1098 when payment is authorized
- **FR-013**: System MUST record current system date from system table (DTMOVABE from TSISTEMA where IDSISTEM='SI') as transaction date
- **FR-014**: System MUST record current system time (HORAOPER) when creating history record
- **FR-015**: System MUST increment claim occurrence counter (ocorhist) by 1 in master record (TMESTSIN) after successful authorization
- **FR-016**: System MUST convert principal and correction amounts to standardized currency (BTNF) using conversion rates from unit table (TGEUNIMO) valid for transaction date
- **FR-017**: System MUST calculate and store total value (VALTOTBT) as sum of principal (VALPRIBT) and correction (CRRMONBT) in standardized currency
- **FR-018**: System MUST initialize accounting status (SITCONTB) as '0' and situation (SITUACAO) as '0' for new history records
- **FR-019**: System MUST initialize correction type (TIPCRR) as '5' for all payment authorizations

#### Product-Specific Validation
- **FR-020**: System MUST identify consortium products by product codes 6814, 7701, or 7709 and route to consortium validation process (CNOUA)
- **FR-021**: System MUST check contract table (EF_CONTR_SEG_HABIT) for contract number and route to EFP validation (SIPUA) when NUM_CONTRATO > 0
- **FR-022**: System MUST route to HB validation process (SIMDA) for non-consortium products or when contract is not in EFP tables
- **FR-023**: System MUST display specific error messages from validation routines when validation fails
- **FR-024**: System MUST halt transaction processing if validation routine returns error code (EZERT8 != '00000000')

#### Phase and Workflow Management
- **FR-025**: System MUST create claim accompaniment record (SI_ACOMPANHA_SINI) with event code, transaction date, user ID, and description when payment is authorized
- **FR-026**: System MUST determine phase changes based on event configuration (SI_REL_FASE_EVENTO) for the event and transaction date
- **FR-027**: System MUST create phase opening record with future closing date (9999-12-31) when phase indication is '1' (abertura)
- **FR-028**: System MUST update phase closing date to actual date when phase indication is '2' (fechamento)
- **FR-029**: System MUST prevent duplicate phase records for the same claim, phase, and event combination
- **FR-030**: System MUST rollback all database changes if any phase update operation fails

#### User Interface
- **FR-031**: System MUST preserve existing visual styling from Site.css stylesheet for consistent user experience
- **FR-032**: System MUST display all claim information fields clearly labeled with Portuguese field names matching original system
- **FR-033**: System MUST provide clear visual distinction between search screen (initial entry) and claim detail screen (after successful search)
- **FR-034**: System MUST display validation errors in red color (#e80c4d) consistent with existing stylesheet
- **FR-035**: System MUST maintain responsive design supporting both desktop and mobile viewports as defined in Site.css
- **FR-036**: System MUST display Caixa Seguradora logo in the header using the provided base64-encoded PNG image data
- **FR-037**: System MUST position the logo prominently in the header area consistent with insurance industry web application standards

#### Migration Dashboard
- **FR-038**: System MUST provide a migration status dashboard accessible to technical and management teams showing overall project progress
- **FR-039**: Dashboard MUST display migration progress as percentage with visual progress bar showing completed vs total work
- **FR-040**: Dashboard MUST show individual user story status (Not Started, In Progress, Completed, Blocked) with color-coded indicators
- **FR-041**: Dashboard MUST display component-level migration status for: screens (2 screens), business rules (42 rules), database integrations (10 entities), validation processes (3 external services)
- **FR-042**: Dashboard MUST show automated test results including total tests, passed tests, failed tests, and code coverage percentage
- **FR-043**: Dashboard MUST display performance comparison metrics between legacy Visual Age and new .NET 9 system including average response time, peak concurrent users, and throughput
- **FR-044**: Dashboard MUST auto-refresh data at configurable intervals (default: 30 seconds) without requiring manual page reload
- **FR-045**: Dashboard MUST provide drill-down capability allowing users to click on any metric to view detailed breakdowns and historical trends
- **FR-046**: Dashboard MUST display system health indicators including API availability, database connectivity, and external service status (CNOUA, SIPUA, SIMDA)
- **FR-047**: Dashboard MUST follow visual design similar to the reference dashboard (https://sicoob-sge3-jv1x.vercel.app/dashboard) with card-based layout, charts, and progress indicators
- **FR-048**: Dashboard MUST use consistent branding with Caixa Seguradora logo and Site.css color scheme
- **FR-049**: Dashboard MUST be responsive and functional on both desktop and mobile viewports
- **FR-050**: Dashboard MUST display recent migration activities timeline showing last 10 completed tasks with timestamps and responsible team members

#### Security and Audit
- **FR-051**: System MUST record operator user ID (EZEUSRID) on all transaction history records
- **FR-052**: System MUST record operator user ID on all phase and accompaniment records for audit trail
- **FR-053**: System MUST maintain referential integrity across claim master (TMESTSIN), history (THISTSIN), phase (SI_SINISTRO_FASE), and accompaniment (SI_ACOMPANHA_SINI) tables
- **FR-054**: System MUST implement database transaction rollback capability (EZEROLLB) for error conditions
- **FR-055**: System MUST preserve all original business rules, validations, and data transformations from legacy system without modification

### Key Entities

- **Claim Master (TMESTSIN)**: Represents the main claim record containing protocol identification (fonte, protsini, dac), claim number (orgsin, rmosin, numsin), policy information (orgapo, rmoapo, numapol), insurance type (tipseg, tpsegu), product code (codprodu), financial summary (sdopag - expected reserve, totpag - total payments), leader information (codlider, sinlid), occurrence counter (ocorhist), and policy type indicator (tipreg)

- **Claim History (THISTSIN)**: Represents individual payment authorization transactions containing insurance type (tipseg), claim identification (orgsin, rmosin, numsin), occurrence sequence (ocorhist), operation code (operacao - always 1098 for payment authorization), transaction date/time (dtmovto, horaoper), principal amount in original currency (valpri), correction amount in original currency (crrmon), beneficiary name (nomfav), correction type (tipcrr - always '5'), amounts in standardized currency (valpribt, crrmonbt, valpri converted), daily amounts (pridiabt, crrdiabt), total values (valtotbt, totdiabt), accounting status (sitcontb), and overall status (situacao)

- **Branch Master (TGERAMO)**: Provides branch descriptive information, specifically branch name (nomeramo) linked to branch code (rmosin) from claim master

- **Currency Unit Table (TGEUNIMO)**: Provides currency conversion rates (vlcruzad) with validity periods (dtinivig, dttervig) used to convert claim amounts from working currency to standardized currency (BTNF - "Bônus do Tesouro Nacional Fiscal")

- **System Control (TSISTEMA)**: Provides current business date (dtmovabe) for the claims system (idsistem = 'SI') used to timestamp all transactions

- **Policy Master (TAPOLICE)**: Provides insured party information, specifically insured name (nome) linked via policy number from claim

- **Claim Accompaniment (SI_ACOMPANHA_SINI)**: Tracks claim workflow events containing protocol identification, event code (cod_evento), transaction date (data_movto_siniaco), occurrence sequence (num_ocorr_siniaco), complementary description (descr_complementar), and operator (cod_usuario)

- **Claim Phase (SI_SINISTRO_FASE)**: Tracks claim processing phases containing protocol identification, phase code (cod_fase), event code (cod_evento), event occurrence (num_ocorr_siniaco), phase effective date (data_inivig_refaev), phase opening date (data_abertura_sifa), and phase closing date (data_fecha_sifa - '9999-12-31' for open phases)

- **Phase-Event Relationship (SI_REL_FASE_EVENTO)**: Configuration table defining which phases are affected by each event type, including phase code (cod_fase), event code (cod_evento), effective date range (data_inivig_refaev), and phase change indicator (ind_alteracao_fase - '1' for opening, '2' for closing)

- **Consortium Contract (EF_CONTR_SEG_HABIT)**: Contains consortium-specific contract information including contract number (num_contrato) used to determine routing to EFP or HB validation processes

- **Migration Status**: Tracks the overall migration project progress containing user story identifier, user story name, current status (Not Started, In Progress, Completed, Blocked), completion percentage, number of functional requirements completed vs total, number of tests passed vs total, assigned team member, start date, estimated completion date, actual completion date, and blocking issues if any

- **Component Migration Tracking**: Tracks individual component migration status containing component type (screen, business rule, database entity, external service), component name, legacy system reference (Visual Age file/function name), migration status, estimated effort hours, actual effort hours, complexity rating (Low, Medium, High), assigned developer, and technical notes

- **Performance Metrics**: Stores performance benchmarks for comparison containing metric type (response time, throughput, concurrent users, memory usage), legacy system value, new system value, measurement timestamp, test scenario description, and pass/fail status based on acceptance criteria

## Success Criteria

### Measurable Outcomes

- **SC-001**: Operators can search and retrieve any claim in under 3 seconds using protocol number, claim number, or leader code
- **SC-002**: Operators can complete a full payment authorization cycle (search, validate, enter details, confirm) in under 90 seconds for standard claims
- **SC-003**: System maintains 100% data accuracy when migrating existing claim records from legacy system tables to new system
- **SC-004**: System preserves 100% of existing business rules and validations from legacy application without functional regressions
- **SC-005**: User interface renders identically to legacy system appearance using migrated Site.css stylesheet on desktop browsers
- **SC-006**: System successfully processes payment authorizations for all product types including consortium products (6814, 7701, 7709) and standard insurance products
- **SC-007**: System creates complete audit trail with operator ID, timestamp, and operation details for 100% of payment authorizations
- **SC-008**: System handles monetary conversions with accuracy to 2 decimal places in standardized currency (BTNF) for all transactions
- **SC-009**: System prevents data corruption by successfully rolling back transactions when validation or phase update operations fail
- **SC-010**: User interface remains responsive and functional on mobile devices (max-width: 850px) as defined in Site.css media queries
- **SC-011**: System integrates successfully with all three validation processes (CNOUA for consortium, SIPUA for EFP contracts, SIMDA for HB contracts) maintaining existing integration contracts
- **SC-012**: 95% of operators successfully complete their first payment authorization without training beyond existing legacy system knowledge
- **SC-013**: Caixa Seguradora logo displays clearly and professionally in all supported browsers and viewport sizes
- **SC-014**: Migration dashboard displays accurate real-time status with data refresh occurring within 30 seconds of any project update
- **SC-015**: Dashboard provides complete visibility into all 6 user stories, 55 functional requirements, 10 database entities, and 3 external service integrations
- **SC-016**: Technical and management teams can identify project bottlenecks and blocking issues within 5 minutes of accessing the dashboard
- **SC-017**: Dashboard performance metrics show side-by-side comparison of legacy Visual Age vs new .NET 9 system with at least 5 key metrics (response time, throughput, concurrent users, memory usage, error rate)
- **SC-018**: 100% of dashboard cards and metrics support drill-down functionality to view detailed information and historical trends
- **SC-019**: Dashboard visual design follows reference implementation with card-based layout, progress bars, charts, and status indicators rendering correctly on desktop and mobile viewports

## Branding Assets

### Logo Specification

The Caixa Seguradora logo MUST be displayed in the application header using the following base64-encoded PNG image:

```
data:image/png;base64,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
```

This logo represents Caixa Seguradora's brand identity and MUST be rendered in the application header to maintain brand consistency across the migrated application.

### Dashboard Reference Design

The migration status dashboard MUST follow the visual design and UX patterns demonstrated in the reference implementation at: https://sicoob-sge3-jv1x.vercel.app/dashboard

**Key Design Elements from Reference**:
- **Card-based Layout**: Dashboard content organized into distinct cards for different metric categories
- **Color Palette**: Green-themed design (#00A859 primary color) with neutral text colors for readability
- **Typography**: Roboto font family with multiple weights (300, 400, 500) for hierarchy
- **Spacing System**: Consistent spacing increments (4px to 48px) for visual harmony
- **Visual Components**: Progress bars, status indicators, charts, and metric cards with shadows and border-radius for depth
- **Responsive Design**: Adaptive layout that works seamlessly across desktop and mobile viewports
- **Material Design Icons**: Icon integration for visual clarity and navigation

**Dashboard Layout Requirements**:
- Header with branding (Caixa Seguradora logo) and navigation
- Overview section with key metrics in card format
- User story progress tracking with visual indicators
- Component migration status grid showing screens, rules, entities, and services
- Performance comparison section with side-by-side legacy vs new system metrics
- Recent activities timeline with chronological task list
- System health status indicators for external dependencies

## Assumptions

- The existing database schema for all referenced tables (TMESTSIN, THISTSIN, TGERAMO, TGEUNIMO, TSISTEMA, TAPOLICE, SI_ACOMPANHA_SINI, SI_SINISTRO_FASE, SI_REL_FASE_EVENTO, EF_CONTR_SEG_HABIT) will remain unchanged and accessible to the new .NET 9 application
- External validation processes (CNOUA, SIPUA, SIMDA) can be integrated via web service calls or will be migrated to .NET services with compatible interfaces
- Current business date logic (TSISTEMA table with IDSISTEM='SI') will be available either through existing table or equivalent .NET service
- Currency conversion rates in TGEUNIMO table are maintained by external processes and will continue to be available with valid date ranges
- Operator authentication and authorization will be handled by existing enterprise authentication system or equivalent (user ID captured as EZEUSRID in legacy will map to current user principal)
- The Site.css stylesheet represents the complete visual design requirements and no additional UI/UX design is needed
- Database transaction support (commit/rollback) is available through standard .NET transaction mechanisms (TransactionScope or EF Core transactions)
- Legacy CICS map groups (SIWEG, SIWEGH) referenced in the Visual Age program represent screen layouts that will be recreated as React components using Site.css styling
- The maximum width of 960px defined in .content-wrapper class represents the desired desktop layout constraint
- Legacy error codes and messages should be preserved exactly for operator familiarity and existing documentation
- The operation code 1098 is the standard code for payment authorization and should not be changed
- Correction type '5' represents a standard correction method that should be maintained
- The insurance type flag (tipseg) from master record and policy type (tipreg) business rules remain current and should not be modified
- The provided Caixa Seguradora logo PNG image is the current approved brand asset for use in all applications
- Migration dashboard will track project progress in real-time and does not require historical data from the Visual Age system
- Dashboard metrics and status tracking can be stored in a separate database schema or storage mechanism independent of legacy claim processing tables
- The reference dashboard design at https://sicoob-sge3-jv1x.vercel.app/dashboard provides visual design inspiration but the implementation can use different charting libraries or components as long as the visual appearance is similar

## Dependencies

- Access to existing mainframe database tables (DB2 or equivalent) containing claim, policy, and configuration data
- Web service endpoints or migration strategy for external validation processes (CNOUA, SIPUA, SIMDA)
- .NET 9 runtime environment with appropriate database drivers (e.g., IBM.Data.DB2 or Entity Framework Core provider)
- React 18+ framework and build tooling for frontend application
- Enterprise authentication service integration for operator identity management
- Currency conversion rate maintenance process to ensure TGEUNIMO table remains current
- System date management process to ensure TSISTEMA table provides accurate business dates
- Charting and data visualization libraries for dashboard (e.g., Chart.js, Recharts, or similar)
- Real-time data update mechanism for dashboard refresh (WebSockets, SignalR, or polling)
- Storage mechanism for migration tracking data (can be separate from legacy claim database)

## Out of Scope

- Modification of existing business rules, validation logic, or data transformation formulas from the legacy system
- Migration or modification of external validation processes (CNOUA, SIPUA, SIMDA) - these are treated as external dependencies
- Database schema changes or optimization of existing table structures
- Redesign of user interface beyond faithful reproduction of existing Site.css styling
- Implementation of new features not present in the legacy SIWEA application
- Performance optimization beyond meeting the success criteria response times
- Integration with systems other than those already integrated in the legacy application
- Data migration or conversion of historical claim records (assumed to remain in existing database)
- Changes to currency conversion methodology or correction type logic
- Modification of operation codes, status codes, or other enumerated values used in legacy system
- Training materials or documentation beyond code comments and standard .NET XML documentation
- Deployment infrastructure, CI/CD pipelines, or cloud hosting configuration
- Internationalization or localization beyond existing Portuguese language content
- Accessibility features beyond those inherent in the semantic HTML and Site.css responsive design
- Advanced reporting or analytics capabilities not present in legacy system
- Logo redesign or creation of alternative brand assets

## References

- Legacy Source: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/#SIWEA-V116.esf`
- Stylesheet: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/Site.css`
- Logo: Base64-encoded PNG image data (see Branding Assets section)
- Legacy Program: SIWEA (Claims Indemnity Payment Authorization System)
- Original System: IBM VisualAge EZEE 4.40
- Original Developers: COSMO (Analyst), SOLANGE (Programmer)
- Original Creation Date: October 1989
- Last Major Revision: CAD73898 - February 11, 2014 (Version 90)
