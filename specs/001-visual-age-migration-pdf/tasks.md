# Tasks: Comprehensive Visual Age Migration Analysis & Planning Document

**Feature Branch**: `001-visual-age-migration-pdf`
**Generated**: 2025-10-23
**User Story**: US1 - Generate Comprehensive Migration Analysis Document (P1)
**Total Tasks**: 90 tasks across 4 phases
**Estimated Effort**: 12 weeks (3 months)

---

## Task Organization

### Phase Breakdown
- **Phase 1: Setup & Foundation** (T001-T005) - Week 1, 5 tasks
- **Phase 2: Foundational Prerequisites** (T006-T015) - Week 1-2, 10 tasks (blocking)
- **Phase 3: User Story Implementation** (T016-T085) - Weeks 2-11, 70 tasks
- **Phase 4: Polish & Cross-Cutting** (T086-T090) - Week 12, 5 tasks

### Task Format
```
- [ ] [TaskID] [P?] [Story?] Description with file path
```
- `[TaskID]`: T001-T090 sequential identifier
- `[P]`: Optional priority marker for blocking tasks
- `[US1]`: User Story 1 label for feature tasks
- Description: Actionable task with target file/directory

---

## Phase 1: Setup & Foundation (Week 1)

**Goal**: Establish project structure, install dependencies, verify tooling

- [X] [T001] Create feature directory structure at `specs/001-visual-age-migration-pdf/`
- [X] [T002] Create contracts/ subdirectories: `section-templates/`, `diagram-definitions/`, `assets/`
- [X] [T003] Create output/ directory with subdirectories: `intermediate/`, `diagrams/`
- [X] [T004] Create scripts/generate-pdf/ with subdirectories: `utils/`
- [X] [T005] Create templates/document-generation/ with subdirectories: `sections/`, `styles/`

---

## Phase 2: Foundational Prerequisites (Weeks 1-2)

**Goal**: Implement core infrastructure required by all document sections (BLOCKING TASKS)

- [X] [T006] [P] Install Python 3.11+ and verify with `python --version` or `python3 --version`
- [ ] [T007] [P] Install LaTeX distribution (TeX Live on Linux, MacTeX on macOS, MiKTeX on Windows)
- [ ] [T008] [P] Verify pdflatex with `pdflatex --version` and check required packages: booktabs, fancyhdr, xcolor, hyperref, siunitx
- [X] [T009] [P] Install Java Runtime Environment for PlantUML: `java -version`
- [X] [T010] [P] Download PlantUML JAR: `wget https://github.com/plantuml/plantuml/releases/download/v1.2024.3/plantuml-1.2024.3.jar -O plantuml.jar`
- [X] [T011] [P] Create Python virtual environment and install dependencies: `pip install markdown2 pyyaml jinja2 pypdf2 pillow`
- [X] [T012] [P] Extract Caixa Seguradora logo from base64 PNG in `specs/001-visualage-dotnet-migration/spec.md` to `contracts/assets/caixa-logo.png`
- [X] [T013] [P] Create document configuration file at `config/document-config.yaml` with metadata, brand settings, section ordering
- [X] [T014] [P] Implement content extraction utility `scripts/generate-pdf/content-extractor.py` to parse markdown files from `specs/001-visualage-dotnet-migration/`
- [X] [T015] [P] Implement markdown parser utility `scripts/generate-pdf/utils/markdown-parser.py` for section extraction

---

## Phase 3: User Story Implementation (Weeks 2-11)

### Category 1: LaTeX Template Development (T016-T030)

**Goal**: Create LaTeX templates for all 10 document sections

- [ ] [T016] [US1] Create master LaTeX document template at `templates/document-generation/master-template.tex` with document class, includes, and section ordering
- [ ] [T017] [US1] Create LaTeX preamble at `templates/document-generation/preamble.tex` with packages: booktabs, fancyhdr, xcolor, hyperref, graphicx, geometry, siunitx
- [ ] [T018] [US1] Create headings style file at `templates/document-generation/styles/headings.sty` with H1 (18pt), H2 (16pt), H3 (14pt), H4 (12pt) definitions
- [ ] [T019] [US1] Create colors style file at `templates/document-generation/styles/colors.sty` with Caixa brand colors: primary #0066CC, success #00A859, warning #FFCC00, error #E80C4D
- [ ] [T020] [US1] Create diagrams style file at `templates/document-generation/styles/diagrams.sty` with figure placement and caption formatting
- [ ] [T021] [US1] Create Executive Summary template at `contracts/section-templates/01-executive-summary.tex` with Jinja2 variables for project context, drivers, solution, timeline, budget
- [ ] [T022] [US1] Create Legacy Analysis template at `contracts/section-templates/02-legacy-analysis.tex` with sections for architecture, business rules, database schema, integrations
- [ ] [T023] [US1] Create Target Architecture template at `contracts/section-templates/03-target-architecture.tex` with Clean Architecture layers, technology stack, service designs
- [ ] [T024] [US1] Create Function Point Analysis template at `contracts/section-templates/04-function-points.tex` with booktabs tables for EI/EO/EQ/ILF/EIF breakdown
- [ ] [T025] [US1] Create Project Timeline template at `contracts/section-templates/05-timeline.tex` with phase descriptions and Gantt chart inclusion
- [ ] [T026] [US1] Create MIGRAI Methodology template at `contracts/section-templates/06-migrai-methodology.tex` with 6 principles subsections
- [ ] [T027] [US1] Create Budget and ROI template at `contracts/section-templates/07-budget-roi.tex` with financial tables using siunitx for currency formatting
- [ ] [T028] [US1] Create Component Specifications template at `contracts/section-templates/08-component-specs.tex` with React and backend service subsections
- [ ] [T029] [US1] Create Risk Management template at `contracts/section-templates/09-risk-management.tex` with risk categorization (high/medium/low) and mitigation strategies
- [ ] [T030] [US1] Create Appendices template at `contracts/section-templates/10-appendices.tex` with glossary, bibliography, contacts, version history

### Category 2: Diagram Generation (T031-T040)

**Goal**: Create PlantUML definitions and generation scripts for 7 diagrams

- [ ] [T031] [US1] Create High-Level Architecture diagram definition at `contracts/diagram-definitions/architecture.puml` showing 6-tier layered view
- [ ] [T032] [US1] Create Clean Architecture Onion diagram at `contracts/diagram-definitions/clean-architecture-onion.puml` with 4 concentric circles
- [ ] [T033] [US1] Create Entity-Relationship diagram at `contracts/diagram-definitions/er-diagram.puml` for 13 database tables with relationships
- [ ] [T034] [US1] Create React Component Hierarchy tree diagram at `contracts/diagram-definitions/component-hierarchy.puml`
- [ ] [T035] [US1] Create Payment Authorization Sequence diagram at `contracts/diagram-definitions/sequence-payment-auth.puml` with 10 participants
- [ ] [T036] [US1] Create Gantt Timeline chart definition at `contracts/diagram-definitions/gantt-timeline.puml` or use LaTeX pgfgantt package
- [ ] [T037] [US1] Create Deployment Architecture diagram at `contracts/diagram-definitions/deployment-azure.puml` showing Azure resources
- [ ] [T038] [US1] Implement diagram generator script `scripts/generate-pdf/diagram-generator.py` calling PlantUML: `java -jar plantuml.jar -tpdf -o output/diagrams/ diagram.puml`
- [ ] [T039] [US1] Test all diagram generation with `diagram-generator.py --all` and verify PDF outputs in `output/diagrams/`
- [ ] [T040] [US1] Optimize diagram sizing and quality: minimum 100mm width/height, vector PDF format

### Category 3: Content Extraction & Processing (T041-T055)

**Goal**: Extract content from existing migration specs and populate templates

- [ ] [T041] [US1] Implement user story extraction in `content-extractor.py` parsing spec.md for 6 user stories (P1-P6)
- [ ] [T042] [US1] Implement functional requirements extraction parsing FR-001 through FR-062 with category grouping
- [ ] [T043] [US1] Implement business rules extraction from spec.md Key Entities section (42 rules with ESQL samples)
- [ ] [T044] [US1] Implement database entity extraction from data-model.md (13 entities with fields, relationships, PKs)
- [ ] [T045] [US1] Implement success criteria extraction from spec.md (SC-001 through SC-015)
- [ ] [T046] [US1] Implement assumptions extraction from spec.md Assumptions section
- [ ] [T047] [US1] Implement timeline phase extraction from plan.md Phase 0-6 with tasks, durations, deliverables
- [ ] [T048] [US1] Implement technology stack extraction from research.md 8 technology decisions
- [ ] [T049] [US1] Implement component specification extraction for React components from spec.md FR-020 through FR-024
- [ ] [T050] [US1] Create template processor script `scripts/generate-pdf/template-processor.py` using Jinja2 to populate LaTeX templates
- [ ] [T051] [US1] Implement template variable mapping from extracted content to template placeholders (e.g., `{{project_context}}`, `{{total_investment}}`)
- [ ] [T052] [US1] Handle LaTeX special character escaping in template processor: `&`, `%`, `$`, `#`, `_`, `{`, `}`, `~`, `^`, `\`
- [ ] [T053] [US1] Implement section assembly in template processor combining populated templates in correct order
- [ ] [T054] [US1] Test template processing with sample data from content extractor
- [ ] [T055] [US1] Validate populated templates compile with pdflatex without errors

### Category 4: Function Point Analysis (T056-T065)

**Goal**: Implement automated FPA calculation using IFPUG 4.3.1 methodology

- [ ] [T056] [US1] Create function point calculator utility `scripts/generate-pdf/utils/function-point-calculator.py`
- [ ] [T057] [US1] Implement External Inputs (EI) classification with complexity matrix: Low (1-4 DETs, 0-1 FTRs) = 3 FP, Average (5-15 DETs, 2 FTRs) = 4 FP, High (16+ DETs, 3+ FTRs) = 6 FP
- [ ] [T058] [US1] Implement External Outputs (EO) classification with derived data processing logic
- [ ] [T059] [US1] Implement External Inquiries (EQ) classification with input/output DET counting
- [ ] [T060] [US1] Implement Internal Logical Files (ILF) classification: Low (1 RET, 1-19 DETs) = 7 FP, Average (1 RET, 20-50 DETs) = 10 FP, High (1 RET, 51+ DETs or 2-5 RETs, 51+ DETs) = 15 FP
- [ ] [T061] [US1] Implement External Interface Files (EIF) classification for external service references
- [ ] [T062] [US1] Calculate Unadjusted Function Points (UFP) as sum of all EI + EO + EQ + ILF + EIF
- [ ] [T063] [US1] Implement 14 General System Characteristics (GSC) rating with degree of influence 0-5 for each factor
- [ ] [T064] [US1] Calculate Value Adjustment Factor (VAF) using formula: 0.65 + (0.01 × sum of all GSC degrees)
- [ ] [T065] [US1] Calculate Adjusted Function Points (AFP) = UFP × VAF and generate detailed breakdown tables for LaTeX template

### Category 5: Timeline & Gantt Chart (T066-T070)

**Goal**: Generate project timeline visualization

- [ ] [T066] [US1] Create Gantt generator utility `scripts/generate-pdf/utils/gantt-generator.py`
- [ ] [T067] [US1] Implement LaTeX pgfgantt chart generation with 12-week timeline (Weeks 1-12)
- [ ] [T068] [US1] Add 6 phase swimlanes (Phase 0-5 Weeks 1-8, Phase 6 Weeks 9-12) with colored bars for tasks
- [ ] [T069] [US1] Add 8 milestones as diamond symbols: M1-M8 with dates and deliverable labels
- [ ] [T070] [US1] Highlight critical path in red color: Research → Foundation → Core → Testing → Homologation → Go-Live

### Category 6: Budget Calculation (T071-T075)

**Goal**: Generate financial analysis tables

- [ ] [T071] [US1] Calculate development cost: AFP (from function-point-calculator.py) × R$ 750 per FP
- [ ] [T072] [US1] Calculate infrastructure costs: Azure App Service P1v3 (R$ 2,500/mo × 3) + SQL Database S3 (R$ 1,200/mo × 3) + Key Vault + Application Insights + Dev/Staging environments = R$ 15,500
- [ ] [T073] [US1] Calculate additional costs: training (R$ 5,000) + licenses (R$ 3,000) + DevOps subscription (R$ 1,000) + testing tools (R$ 500) = R$ 9,500
- [ ] [T074] [US1] Calculate contingency reserve: 15% of (development + infrastructure + additional)
- [ ] [T075] [US1] Generate total investment sum and payment milestone table (5 milestones: 20%, 20%, 30%, 20%, 10%) with LaTeX booktabs formatting

### Category 7: PDF Assembly & Formatting (T076-T080)

**Goal**: Compile LaTeX to final PDF with branding

- [ ] [T076] [US1] Create PDF assembler script `scripts/generate-pdf/pdf-assembler.py` calling pdflatex command
- [ ] [T077] [US1] Implement 3-pass LaTeX compilation: 1st pass for content, 2nd pass for TOC/refs, 3rd pass for hyperlinks
- [ ] [T078] [US1] Configure header with Caixa logo (left), document title (center) using fancyhdr package
- [ ] [T079] [US1] Configure footer with page numbers "Page X of Y" (left), version "v1.0" (center), confidentiality notice (right)
- [ ] [T080] [US1] Generate table of contents with hyperlinks using `\tableofcontents` and hyperref package

### Category 8: Quality Validation (T081-T085)

**Goal**: Automated quality checks for PDF output

- [ ] [T081] [US1] Create validators script `scripts/generate-pdf/validators.py` using PyPDF2 for PDF structure inspection
- [ ] [T082] [US1] Implement page count validation: assert page_count >= 50 and page_count <= 70
- [ ] [T083] [US1] Implement section presence validation: verify all 10 sections exist by searching for section titles in PDF text
- [ ] [T084] [US1] Implement hyperlink validation: verify table of contents links functional, minimum 10 hyperlinks present
- [ ] [T085] [US1] Implement PDF/A-1b compliance check using verapdf: fonts embedded, searchable text, metadata present

---

## Phase 4: Polish & Cross-Cutting (Week 12)

**Goal**: Final integration, documentation, and delivery preparation

- [ ] [T086] Create main entry point script `scripts/generate-pdf/main.py` orchestrating full pipeline: extract → process → generate diagrams → assemble PDF → validate
- [ ] [T087] Add command-line arguments to main.py: `--config`, `--section` (for regenerating single section), `--skip-validation`, `--output`
- [ ] [T088] Create comprehensive validation report generator outputting `output/validation-report.md` with checklist: page count ✓/✗, sections ✓/✗, diagrams ✓/✗, links ✓/✗, PDF/A ✓/✗
- [ ] [T089] Write unit tests in `tests/unit/` for content-extractor, template-processor, function-point-calculator with pytest
- [ ] [T090] Create integration test in `tests/integration/test_pdf_generation.py` executing full pipeline and comparing output against reference PDF

---

## Task Dependencies

### Critical Path (Blocking Tasks)
```
T001-T005 (Setup)
  → T006-T015 (Prerequisites - BLOCKING)
  → T016-T030 (Templates)
  → T031-T040 (Diagrams)
  → T041-T055 (Content Extraction)
  → T056-T065 (Function Points)
  → T066-T070 (Timeline)
  → T071-T075 (Budget)
  → T076-T080 (PDF Assembly)
  → T081-T085 (Validation)
  → T086-T090 (Integration)
```

### Parallel Execution Examples

**Week 1 Parallel Tasks**:
- T001-T005 (Setup) can run in parallel (independent directory creation)
- T006, T007, T008, T009, T010 (Tool installation) can run in parallel

**Week 2-3 Parallel Tasks**:
- T016-T030 (LaTeX templates) can be developed in parallel by multiple team members (each person owns 3-4 templates)
- T031-T037 (Diagram definitions) can be created in parallel once PlantUML installed

**Week 4-6 Parallel Tasks**:
- T041-T049 (Content extractors) can be implemented in parallel (each extracts different section)
- T056-T065 (FPA calculation) can be developed in parallel with content extraction

**Week 7-8 Parallel Tasks**:
- T066-T070 (Gantt generation) parallel with T071-T075 (Budget calculation)
- T076-T080 (PDF assembly) parallel with T081-T085 (Validation)

### Dependency Graph
```
Prerequisites (T006-T015)
  ├── Templates (T016-T030) ──┐
  ├── Diagrams (T031-T040) ───┼─→ PDF Assembly (T076-T080)
  ├── Content (T041-T055) ────┤
  ├── FPA (T056-T065) ────────┤
  ├── Timeline (T066-T070) ───┤
  └── Budget (T071-T075) ─────┘
                              │
                              └─→ Validation (T081-T085) → Integration (T086-T090)
```

---

## Estimated Effort Distribution

### By Phase
- **Phase 1 (Setup)**: 5 tasks, 0.5 days
- **Phase 2 (Prerequisites)**: 10 tasks, 2 days
- **Phase 3 (Implementation)**: 70 tasks, 55 days (11 weeks)
- **Phase 4 (Polish)**: 5 tasks, 2.5 days

**Total**: 90 tasks, 60 days (12 weeks)

### By Category
- Template Development: 15 tasks (T016-T030), 7 days
- Diagram Generation: 10 tasks (T031-T040), 4 days
- Content Extraction: 15 tasks (T041-T055), 8 days
- Function Point Analysis: 10 tasks (T056-T065), 5 days
- Timeline & Gantt: 5 tasks (T066-T070), 3 days
- Budget Calculation: 5 tasks (T071-T075), 2 days
- PDF Assembly: 5 tasks (T076-T080), 3 days
- Quality Validation: 5 tasks (T081-T085), 3 days
- Integration & Testing: 5 tasks (T086-T090), 3 days
- Setup & Prerequisites: 15 tasks (T001-T015), 2.5 days

---

## Success Metrics

**Task Completion Tracking**:
- Week 1 end: T001-T015 complete (15 tasks, 17% of total)
- Week 3 end: T016-T040 complete (40 tasks, 44% of total)
- Week 6 end: T041-T070 complete (70 tasks, 78% of total)
- Week 9 end: T071-T085 complete (85 tasks, 94% of total)
- Week 12 end: T086-T090 complete (90 tasks, 100%)

**Quality Gates**:
- All LaTeX templates compile without errors (T055 validation)
- All diagrams render as vector PDFs (T039 validation)
- Function point total within range 180-220 AFP (T065 validation)
- Final PDF meets all 15 success criteria SC-001 through SC-015 (T088 validation)

**Deliverable Milestones**:
- M1: Prerequisites installed and verified (T015 complete)
- M2: All templates created (T030 complete)
- M3: All diagrams generated (T040 complete)
- M4: Content extraction working (T055 complete)
- M5: FPA calculation accurate (T065 complete)
- M6: PDF assembly producing output (T080 complete)
- M7: Validation passing (T085 complete)
- M8: Integration complete, final PDF delivered (T090 complete)

---

**Tasks Definition Complete**: 2025-10-23
**Ready for Implementation**: Phase 1 tasks T001-T005 can begin immediately
