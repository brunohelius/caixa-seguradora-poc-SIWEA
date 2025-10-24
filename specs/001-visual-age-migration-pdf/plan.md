# Implementation Plan: Comprehensive Visual Age Migration Analysis & Planning Document

**Branch**: `001-visual-age-migration-pdf` | **Date**: 2025-10-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-visual-age-migration-pdf/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This plan outlines the creation of a comprehensive PDF document (50+ pages) that provides extensive analysis of the legacy IBM VisualAge EZEE Claims System (SIWEA) and complete migration planning to .NET 9 with React. The document includes detailed function point analysis (180-220 AFP using IFPUG 4.3.1), MIGRAI methodology implementation with LLM framework integration, realistic 3-month timeline (2 months development + 1 month homologation), and complete budget calculation (R$ 201,250 total investment based on R$ 750 per function point).

**Primary Technical Approach**: Document generation using LaTeX or Markdown-to-PDF pipeline with programmatic content assembly from existing migration artifacts (specs/001-visualage-dotnet-migration/), automated diagram generation (Lucidchart API or PlantUML), and template-based formatting for professional PDF output (PDF/A-1b format with embedded fonts, branded headers/footers, table of contents, hyperlinks).

## Technical Context

**Language/Version**: Python 3.11+ (for document generation scripts) or Node.js 18+ (for JavaScript-based tooling)
**Primary Dependencies**:
- LaTeX (pdflatex from TeX Live or MiKTeX) for PDF generation, OR
- Markdown processor (Pandoc 3.x) with PDF export via LaTeX, OR
- JavaScript tooling (Puppeteer for HTML-to-PDF from React-based document builder)
- Diagram generation: PlantUML 1.2024.x for UML diagrams, OR Lucidchart API for cloud-based diagrams
- YAML/JSON parsing for reading existing spec files

**Storage**:
- Input: Existing spec files (spec.md, plan.md, research.md, data-model.md) from specs/001-visualage-dotnet-migration/
- Output: Generated PDF file (50+ pages) at specs/001-visual-age-migration-pdf/output/migration-analysis-plan.pdf
- Intermediate: LaTeX .tex files or Markdown .md files in specs/001-visual-age-migration-pdf/templates/

**Testing**:
- PDF generation validation: Automated checks for page count (>= 50), section presence, diagram rendering
- Content validation: Verify all 62 functional requirements mapped to document sections
- Quality checks: PDF/A-1b compliance validation, embedded font verification, hyperlink testing
- Visual regression: Compare generated diagrams against reference images

**Target Platform**:
- Document generation: Linux, macOS, or Windows with LaTeX distribution installed
- Output PDF: Cross-platform (readable on all major PDF viewers: Adobe Acrobat, macOS Preview, Chrome PDF viewer)
- Archival: PDF/A-1b format for long-term preservation and regulatory compliance

**Project Type**: Document generation pipeline (single project structure with templates/, scripts/, output/)

**Performance Goals**:
- PDF generation time: < 5 minutes for complete 50+ page document
- Diagram generation: < 30 seconds per diagram (7 diagrams total)
- Incremental updates: < 2 minutes when regenerating after content changes
- Template processing: < 10 seconds for variable substitution and section assembly

**Constraints**:
- PDF file size: < 10 MB (with embedded fonts and diagrams)
- No external dependencies at runtime (self-contained PDF with embedded resources)
- Professional formatting: Consistent with Caixa Seguradora brand guidelines
- Accessibility: PDF/A-1b compliance, searchable text, alt text for diagrams, 4.5:1 contrast ratio

**Scale/Scope**:
- Document sections: 10 major sections (Executive Summary through Appendices)
- Functional requirements coverage: 62 FR mapped to document content
- Diagrams: 7 high-quality diagrams (architecture, ER, sequence, Gantt, etc.)
- Page count: 50-70 pages estimated
- Content sources: 4 existing spec files (spec.md, plan.md, research.md, data-model.md from 001-visualage-dotnet-migration)
- Function point breakdown: Detailed calculation for 5 component types (EI/EO/EQ/ILF/EIF)
- Timeline visualization: 12-week Gantt chart with 8 milestones
- Budget tables: 5 cost categories with detailed line items

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: ✅ PASSED - No constitution file exists; project follows standard document generation best practices

**Notes**:
- The constitution file (`.specify/memory/constitution.md`) is an empty template with no defined principles
- This document generation project follows industry-standard practices for technical documentation:
  - **Single Responsibility**: Each template section focuses on one aspect (architecture, budget, timeline)
  - **Separation of Concerns**: Content extraction (from source specs) separated from formatting (LaTeX/Markdown templates) separated from rendering (PDF generation)
  - **DRY Principle**: Reuse existing migration spec content rather than duplicating analysis
  - **Testability**: Automated validation for PDF structure, content completeness, and quality standards
  - **Version Control**: Templates and scripts under Git for reproducibility and updates
  - **Documentation**: README explaining generation process, template customization, and troubleshooting

**Post-Design Re-check**: Will validate that chosen document generation approach (LaTeX vs Markdown vs HTML-to-PDF) aligns with maintainability and update frequency requirements

## Project Structure

### Documentation (this feature)

```text
specs/001-visual-age-migration-pdf/
├── plan.md              # This file (/speckit.plan command output)
├── spec.md              # Feature specification (already complete)
├── research.md          # Phase 0 output - technology decisions for document generation
├── data-model.md        # Phase 1 output - document content model and section mappings
├── quickstart.md        # Phase 1 output - how to generate and update the PDF
├── contracts/           # Phase 1 output - document schema and template structure
│   ├── document-schema.json        # JSON schema defining required sections and metadata
│   ├── section-templates/          # LaTeX or Markdown templates for each section
│   │   ├── 01-executive-summary.tex
│   │   ├── 02-legacy-analysis.tex
│   │   ├── 03-target-architecture.tex
│   │   ├── 04-function-points.tex
│   │   ├── 05-timeline.tex
│   │   ├── 06-migrai-methodology.tex
│   │   ├── 07-budget-roi.tex
│   │   ├── 08-component-specs.tex
│   │   ├── 09-risk-management.tex
│   │   └── 10-appendices.tex
│   ├── diagram-definitions/        # PlantUML or diagram generation scripts
│   │   ├── architecture.puml
│   │   ├── clean-architecture-onion.puml
│   │   ├── er-diagram.puml
│   │   ├── component-hierarchy.puml
│   │   ├── sequence-payment-auth.puml
│   │   ├── gantt-timeline.puml
│   │   └── deployment-azure.puml
│   └── assets/                     # Static resources
│       └── caixa-logo.png          # Decoded from base64
├── checklists/
│   └── requirements.md  # Quality validation checklist (already complete)
└── output/              # Generated artifacts
    ├── migration-analysis-plan.pdf      # Final PDF output
    ├── intermediate/                    # Build artifacts (.aux, .log, .toc for LaTeX)
    └── validation-report.md             # Automated quality check results
```

### Source Code (repository root)

```text
# Document Generation Pipeline (single project structure)
scripts/
├── generate-pdf/
│   ├── main.py or generate.js          # Entry point for PDF generation
│   ├── content-extractor.py            # Extracts content from existing specs
│   ├── template-processor.py           # Populates templates with extracted data
│   ├── diagram-generator.py            # Generates diagrams from PlantUML definitions
│   ├── pdf-assembler.py                # Compiles LaTeX/Markdown to PDF
│   ├── validators.py                   # Quality checks (page count, sections, links)
│   └── utils/
│       ├── markdown-parser.py          # Parse spec.md, plan.md, research.md
│       ├── function-point-calculator.py # Generate FPA tables from requirements
│       ├── gantt-generator.py          # Create Gantt chart from timeline data
│       └── brand-formatter.py          # Apply Caixa Seguradora styling

templates/
├── document-generation/
│   ├── master-template.tex             # Main LaTeX document structure
│   ├── preamble.tex                    # LaTeX packages, formatting, brand colors
│   ├── sections/                       # Individual section templates
│   └── styles/
│       ├── headings.sty                # Typography definitions
│       ├── colors.sty                  # Brand color palette
│       └── diagrams.sty                # Diagram styling

tests/
├── unit/
│   ├── test_content_extractor.py      # Unit tests for content extraction
│   ├── test_template_processor.py     # Unit tests for template population
│   └── test_function_point_calc.py    # Unit tests for FPA calculations
├── integration/
│   ├── test_pdf_generation.py         # End-to-end PDF generation test
│   └── test_diagram_rendering.py      # Diagram generation integration tests
└── fixtures/
    ├── sample-spec.md                  # Test input data
    └── expected-output.pdf             # Reference PDF for comparison

config/
└── document-config.yaml                # Configuration for document generation
    # - PDF metadata (title, author, keywords)
    # - Brand settings (logo path, colors, fonts)
    # - Section ordering and inclusion flags
    # - Diagram generation settings
    # - Output paths and naming
```

**Structure Decision**: Single project structure chosen because this is a focused document generation utility, not a multi-service application. All components (content extraction, template processing, diagram generation, PDF assembly) are tightly coupled to the single output artifact (the comprehensive PDF document). This structure:
- Simplifies dependency management (single requirements.txt or package.json)
- Enables straightforward testing (clear input/output boundaries)
- Facilitates version control (all templates and scripts in one place)
- Supports incremental development (add one section template at a time)
- Allows easy updates when migration spec changes (re-run extraction and regeneration)

## Complexity Tracking

**Status**: ✅ NO VIOLATIONS

This document generation project does not introduce unjustified complexity:

- **No Custom Framework**: Uses standard LaTeX/Pandoc for PDF generation, established industry tools
- **No Novel Architecture**: Straightforward pipeline (extract → template → render → validate)
- **No Premature Optimization**: Generate on-demand, no caching or incremental rendering complexity
- **No Over-Engineering**: Each script has single responsibility (content extraction separate from diagram generation separate from PDF assembly)
- **Standard Tools**: PlantUML for diagrams (widely used), LaTeX for professional typesetting (academic/industry standard), or Pandoc as simpler alternative

All architectural decisions are justified by functional requirements:
- **Template-Based Generation**: Required for FR-057 (branded header/footer on every page), FR-059 (consistent typography), FR-060 (color coding)
- **Programmatic Diagram Generation**: Required for FR-051 through FR-056 (7 different diagram types with specific content)
- **Automated Content Extraction**: Required for FR-007 through FR-024 (pulling business rules, entity definitions, component specs from existing migration spec)
- **Quality Validation**: Required for SC-001 through SC-015 (measurable quality criteria like page count, section completeness, PDF/A compliance)

## Phase 0: Research & Decisions

### Research Areas

The following technical decisions require investigation and documentation in `research.md`:

#### 1. PDF Generation Technology
**Question**: What technology stack should be used for generating professional PDF documents with complex formatting?

**Research Tasks**:
- Evaluate LaTeX with pdflatex: Professional typesetting, complex layouts, academic standard
- Evaluate Pandoc with Markdown: Simpler syntax, good for text-heavy documents, LaTeX backend for PDF
- Evaluate HTML-to-PDF (Puppeteer/Playwright): Web technology familiarity, CSS styling, screenshot approach
- Evaluate Python libraries (ReportLab, WeasyPrint): Programmatic PDF construction, fine-grained control
- Compare template syntax complexity, learning curve, and documentation quality
- Test branded header/footer generation, table of contents with hyperlinks, and multi-column layouts

**Decision Criteria**:
- Support for professional formatting (branded headers, consistent typography, color coding per FR-057, FR-059, FR-060)
- Diagram embedding quality (vector graphics for architecture diagrams per FR-051-056)
- PDF/A-1b compliance for archival (per FR-062)
- Template maintainability (ease of updating sections when spec changes)
- Build speed (< 5 minutes for 50-page document per performance goals)

#### 2. Diagram Generation Approach
**Question**: How to programmatically generate 7 different diagram types with professional quality?

**Research Tasks**:
- PlantUML evaluation: Text-based diagram definitions, support for UML/ER/Gantt, PNG/SVG output
- Lucidchart API: Cloud-based, professional templates, collaborative editing, export to PDF/PNG
- Graphviz/DOT: Graph layouts, good for architecture diagrams, lower-level control
- Mermaid.js: Markdown-embedded diagrams, simpler syntax, web-based rendering
- Python libraries (diagrams, schemdraw): Programmatic diagram construction, code-based approach
- Manual diagram creation in Draw.io/Visio: One-time creation with highest quality control

**Decision Criteria**:
- Support for all 7 diagram types (architecture layers, onion diagram, ER diagram, component hierarchy, sequence diagram, Gantt chart, deployment diagram per FR-051-056)
- Professional quality output (clear labels, consistent notation, appropriate detail level per SC-009)
- Maintainability (can update diagrams when architecture changes)
- Integration with PDF generation workflow (vector format embedding)

#### 3. Content Extraction Strategy
**Question**: How to extract and structure content from existing migration spec files?

**Research Tasks**:
- Markdown parsing libraries: Python markdown2/mistune, JavaScript marked/remark
- YAML front matter extraction for metadata
- Table extraction from markdown (function point breakdown, budget tables)
- Code block extraction for ESQL/C# examples
- Programmatic section identification using headings
- Data transformation from spec format to document format

**Decision Criteria**:
- Accurately extract all 42 business rules from spec.md (per FR-007)
- Parse function point breakdown from Key Entities section (per FR-025-031)
- Extract component specifications with TypeScript/C# code (per FR-020-024)
- Handle nested sections and maintain content relationships
- Preserve formatting (bold, italic, code blocks) in output

#### 4. Function Point Calculation Automation
**Question**: How to automate function point calculation and table generation from requirements?

**Research Tasks**:
- IFPUG 4.3.1 complexity matrix implementation (DETs, FTRs, RETs lookup tables)
- EI/EO/EQ/ILF/EIF classification from requirement keywords (search form = EI, detail report = EO, lookup = EQ)
- GSC (General System Characteristics) assessment automation from architecture descriptions
- VAF (Value Adjustment Factor) formula: VAF = 0.65 + (0.01 × sum of GSC degrees)
- AFP (Adjusted Function Points) calculation: AFP = UFP × VAF
- Table formatting in LaTeX (tabular environment, column alignment, totals row)

**Decision Criteria**:
- Produce 180-220 AFP estimate matching spec (per FR-004, SC-004)
- Generate detailed breakdown tables for all 5 component types (per FR-025-029)
- Calculate VAF from 14 GSC factors (per FR-030)
- Output professional FPA tables with complexity ratings

#### 5. Timeline Visualization (Gantt Chart)
**Question**: How to generate a professional Gantt chart for the 3-month timeline?

**Research Tasks**:
- PlantUML Gantt syntax evaluation: Simple text format, date-based tasks, dependencies
- Python libraries: matplotlib-gantt, python-gantt, plotly for interactive charts
- JavaScript libraries: dhtmlxGantt, FusionCharts, Google Charts
- Microsoft Project XML export (if using Project for planning)
- LaTeX pgfgantt package: Native LaTeX integration, professional output
- Manual Gantt creation in Excel/Google Sheets: Export as image

**Decision Criteria**:
- Show 12-week timeline with weekly granularity (Weeks 1-12 per FR-032-038)
- Display 6 phases as swimlanes (Phase 0 through Phase 6)
- Show task dependencies with arrows (e.g., foundation → core → API)
- Highlight 8 milestones with diamond symbols (M1 through M8 per FR-056)
- Show critical path in red color
- Include resource allocation swimlanes (2 backend, 2 frontend, 1 QA, 1 DevOps, 1 lead, 1 PM)

#### 6. Budget Table Formatting
**Question**: How to create professional financial tables with currency formatting?

**Research Tasks**:
- LaTeX table packages: booktabs for professional tables, siunitx for number formatting
- Currency symbol handling: R$ with proper spacing
- Number formatting: Thousand separators (R$ 201,250), two decimal precision
- Subtotal and total row styling (bold, horizontal rules)
- Multi-column headers for cost categories
- Percentage calculations for payment milestones (20%, 20%, 30%, 20%, 10%)

**Decision Criteria**:
- Display total project cost R$ 201,250 prominently (per FR-047)
- Break down into 5 categories: development, infrastructure, additional, contingency, total (per FR-048)
- Show payment milestone schedule with percentages and amounts (per FR-049)
- Present ROI projection with annual savings (per FR-050)
- Professional accounting table styling (aligned decimals, clear totals)

#### 7. Brand Compliance (Logo, Colors, Fonts)
**Question**: How to apply Caixa Seguradora branding to the PDF document?

**Research Tasks**:
- Base64 PNG decoding for logo (from existing spec.md in 001-visualage-dotnet-migration)
- LaTeX fancyhdr package for custom headers/footers
- Color definition in LaTeX (xcolor package): #e80c4d for errors, #0066cc for technical sections, #00a859 for success
- Font selection: Arial or Helvetica for headings, standard serif for body (per FR-059)
- Header/footer layout: Logo top-left, title center, page numbers bottom-right (per FR-057)
- PDF metadata: Title, author, subject, keywords (per FR-062)

**Decision Criteria**:
- Caixa Seguradora logo on every page header (per FR-057)
- Consistent brand colors throughout (per FR-060)
- Professional typography hierarchy (18pt/16pt/14pt/12pt headings per FR-059)
- Proper PDF metadata for corporate archival

#### 8. Quality Validation Automation
**Question**: How to automatically validate generated PDF against quality criteria?

**Research Tasks**:
- PDF parsing libraries: PyPDF2, pdfplumber (Python), pdf-lib (JavaScript)
- Page count verification (>= 50 pages per SC-001)
- Section presence checking via text search (all 10 major sections)
- Hyperlink validation (table of contents links working)
- PDF/A-1b compliance verification (verapdf tool)
- Font embedding check (no external font dependencies)
- Contrast ratio validation for accessibility (4.5:1 per SC-015)
- Image alt text verification

**Decision Criteria**:
- Automated validation for all 15 success criteria (SC-001 through SC-015)
- Generate validation report showing pass/fail for each check
- Fail build if critical quality criteria not met (page count, section completeness)
- Provide actionable error messages for failures

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

Define the document content model extracting from existing migration specs:

**Primary Entities**:

1. **DocumentMetadata**: Title ("Visual Age to .NET Migration - Comprehensive Analysis & Plan"), author ("Caixa Seguradora Migration Team"), creation date (2025-10-23), version (1.0), keywords (["Visual Age", "NET 9", "React", "MIGRAI", "Function Points", "Insurance Claims"])

2. **ExecutiveSummary**: Project context (legacy IBM VisualAge EZEE system), business drivers (mainframe cost reduction, technical debt elimination, developer productivity), solution approach (.NET 9 + React), scope (6 user stories, 55 FR), timeline (3 months), budget (R$ 201,250), ROI (R$ 95K annual savings, 2.1 year payback), risks (external services, currency conversion, concurrent users), decision framework

3. **LegacySystemAnalysis**: Architecture overview (CICS, DB2, ESQL, SOAP), business rules (42 rules with ESQL samples), database schema (13 tables with ER diagram), transaction flows (SIWEG/SIWEGH CICS maps), external integrations (CNOUA/SIPUA/SIMDA protocols), performance metrics (response times, concurrent users, TPS), technical debt (undocumented rules, no tests, mainframe costs)

4. **TargetArchitecture**: Clean Architecture pattern (API/Core/Infrastructure layers), technology stack (.NET 9, React 19, EF Core 9, SoapCore), service designs (IClaimService, IPaymentService, IValidationService), component specs (ClaimSearchPage, ClaimDetailPage, MigrationDashboardPage), API contracts (OpenAPI 3.0 for REST, WSDL for SOAP), database strategy (Fluent API legacy mappings), deployment (Docker, Azure App Service, SQL Database)

5. **FunctionPointAnalysis**: Component breakdown (EI: 4+6 FP, EO: 5+4 FP, EQ: 4+3 FP, ILF: 10+10+7 FP, EIF: 5+5+5+5 FP), complexity ratings (Low/Average/High DETs/FTRs/RETs), unadjusted FP (UFP = 150-180), GSC assessment (14 factors totaling 59 influence points), VAF calculation (0.65 + 0.01×59 = 1.24), adjusted FP (AFP = 186-223, rounded to 200)

6. **ProjectTimeline**: Phase 0 (Week 1: Research 5 days), Phase 1 (Weeks 2-3: Foundation 10 days), Phase 2 (Weeks 4-5: Core Logic 10 days), Phase 3 (Week 6: API Layer 5 days), Phase 4 (Week 7: Frontend 5 days), Phase 5 (Week 8: Testing 5 days), Phase 6 (Weeks 9-12: Homologation 20 days), milestones (M1-M8 with dates and deliverables), critical path (Research → Foundation → Core → Testing → Homologation), resource allocation (2 backend, 2 frontend, 1 QA, 1 DevOps, 1 lead, 1 PM)

7. **MIGRAIMethodology**: Modernization principle (.NET 9, React 19, Docker, cloud-native), Intelligence principle (Claude 3.5 Sonnet LLM for ESQL→C# 95% accuracy), Gradual migration (P1→P6 phased rollout, feature toggles, parallel operation), Resilience (Polly retry, circuit breaker, fallback), Automation (GitHub Actions CI/CD, automated testing, FPA tracking), Integration (SOAP backward compatibility, REST APIs, AD auth, zero schema changes)

8. **BudgetAndROI**: Development cost (200 AFP × R$ 750 = R$ 150K), infrastructure (Azure P1v3 + SQL S3 + Key Vault + App Insights = R$ 16.1K), additional (training + licenses = R$ 9.5K), contingency (15% = R$ 26.25K), total investment (R$ 201,250), payment milestones (20%/20%/30%/20%/10%), annual savings (R$ 95K), payback period (2.1 years), 5-year TCO (R$ 273.75K net value)

9. **ComponentSpecifications**: ClaimSearchPage (TypeScript interface, props/state, validation, API integration, Site.css classes), PaymentAuthorizationForm (conditional beneficiary, SOAP calls, error handling), ClaimService (IClaimService interface, SearchByProtocolAsync, GetClaimDetailsAsync), PaymentService (transaction workflow, validation calls, rollback), ValidationService (CNOUA/SIPUA/SIMDA clients, Polly policies, error mapping)

10. **DiagramCollection**: High-Level Architecture (6-tier layered view with tech labels), Clean Architecture Onion (4 concentric circles with dependencies), ER Diagram (13 tables UML notation with relationships), React Component Hierarchy (tree from App.tsx to shared components), Payment Authorization Sequence (10 participants with retry logic), Gantt Chart (12-week timeline with critical path), External Service Integration (Polly wrapper with fallback), Deployment Architecture (Azure resources with security)

**Relationships**:
- ExecutiveSummary references FunctionPointAnalysis (for AFP total), BudgetAndROI (for investment), ProjectTimeline (for duration)
- TargetArchitecture contains ComponentSpecifications (detailed design of services/components)
- LegacySystemAnalysis informs TargetArchitecture (business rules migration mapping)
- MIGRAIMethodology applies to ProjectTimeline (principles guide phase execution)
- DiagramCollection visualizes LegacySystemAnalysis, TargetArchitecture, and ProjectTimeline

### API Contracts (contracts/)

Generate document schema and template structure:

**Document Schema** (`contracts/document-schema.json`):
```json
{
  "schema_version": "1.0",
  "document": {
    "metadata": {
      "title": "string (required)",
      "author": "string (required)",
      "creation_date": "ISO 8601 date (required)",
      "version": "semantic version (required)",
      "keywords": "array of strings"
    },
    "sections": [
      {
        "id": "executive-summary",
        "title": "Executive Summary",
        "min_pages": 2,
        "max_pages": 3,
        "required_subsections": ["project-context", "business-drivers", "solution-approach", "timeline", "budget", "roi", "risks", "decision-framework"]
      },
      {
        "id": "legacy-system-analysis",
        "title": "Legacy System Analysis",
        "min_pages": 15,
        "required_subsections": ["architecture-overview", "business-rules-42", "database-schema-13-tables", "transaction-flows", "external-integrations-3", "performance-metrics", "technical-debt"]
      },
      {
        "id": "target-architecture",
        "title": "Target Architecture Specification",
        "min_pages": 20,
        "required_subsections": ["clean-architecture-pattern", "technology-stack", "service-designs", "component-specs", "api-contracts-rest-soap", "database-strategy", "deployment-architecture"]
      },
      {
        "id": "function-point-analysis",
        "title": "Function Point Analysis",
        "min_pages": 5,
        "required_subsections": ["component-breakdown-ei-eo-eq-ilf-eif", "complexity-ratings", "ufp-calculation", "gsc-assessment-14-factors", "vaf-calculation", "afp-result-180-220"]
      },
      {
        "id": "project-timeline",
        "title": "Project Timeline and Scheduling",
        "min_pages": 4,
        "required_subsections": ["phase-0-research", "phase-1-foundation", "phase-2-core", "phase-3-api", "phase-4-frontend", "phase-5-testing", "phase-6-homologation", "milestones-m1-m8", "critical-path", "resource-allocation"]
      },
      {
        "id": "migrai-methodology",
        "title": "MIGRAI Methodology Framework",
        "min_pages": 6,
        "required_subsections": ["modernization-principle", "intelligence-llm-principle", "gradual-migration-principle", "resilience-principle", "automation-principle", "integration-principle"]
      },
      {
        "id": "budget-roi",
        "title": "Budget and ROI Analysis",
        "min_pages": 4,
        "required_subsections": ["development-cost-calculation", "infrastructure-costs-azure", "additional-costs-training-licenses", "contingency-reserve", "total-investment-r201250", "payment-milestones-5", "annual-savings-r95k", "payback-period-2.1-years", "5-year-tco"]
      },
      {
        "id": "component-specifications",
        "title": "Component and Service Specifications",
        "min_pages": 8,
        "required_subsections": ["react-components-typescript", "backend-services-csharp", "api-integrations-soap-rest", "validation-services-polly"]
      },
      {
        "id": "risk-management",
        "title": "Risk Assessment and Mitigation",
        "min_pages": 3,
        "required_subsections": ["high-risks-15-min", "mitigation-strategies", "contingency-plans", "go-live-criteria"]
      },
      {
        "id": "appendices",
        "title": "Appendices",
        "min_pages": 2,
        "required_subsections": ["glossary-technical-terms", "bibliography-references", "project-team-contacts", "version-history-changelog"]
      }
    ],
    "diagrams": [
      {"id": "high-level-architecture", "type": "layered-architecture", "format": "vector-pdf-or-svg", "min_width_mm": 180, "min_height_mm": 120},
      {"id": "clean-architecture-onion", "type": "onion-diagram", "format": "vector-pdf-or-svg", "min_width_mm": 150, "min_height_mm": 150},
      {"id": "er-diagram-13-tables", "type": "entity-relationship", "format": "vector-pdf-or-svg", "min_width_mm": 200, "min_height_mm": 150},
      {"id": "react-component-hierarchy", "type": "tree-diagram", "format": "vector-pdf-or-svg", "min_width_mm": 180, "min_height_mm": 140},
      {"id": "payment-auth-sequence", "type": "uml-sequence", "format": "vector-pdf-or-svg", "min_width_mm": 200, "min_height_mm": 180},
      {"id": "gantt-12-week-timeline", "type": "gantt-chart", "format": "vector-pdf-or-svg", "min_width_mm": 240, "min_height_mm": 160},
      {"id": "deployment-architecture-azure", "type": "deployment-diagram", "format": "vector-pdf-or-svg", "min_width_mm": 200, "min_height_mm": 150}
    ],
    "formatting": {
      "page_size": "A4",
      "margins": {"top_mm": 25, "bottom_mm": 25, "left_mm": 20, "right_mm": 20},
      "header": {"logo_left": "caixa-logo.png", "title_center": "Visual Age to .NET Migration - Comprehensive Analysis & Plan", "height_mm": 15},
      "footer": {"page_numbers_format": "Page X of Y", "version_left": "v1.0", "confidentiality_right": "CONFIDENTIAL - Internal Use Only", "height_mm": 10},
      "typography": {
        "heading1": {"font": "Arial", "size_pt": 18, "weight": "bold", "color": "#0066cc"},
        "heading2": {"font": "Arial", "size_pt": 16, "weight": "bold", "color": "#0066cc"},
        "heading3": {"font": "Arial", "size_pt": 14, "weight": "bold", "color": "#333333"},
        "heading4": {"font": "Arial", "size_pt": 12, "weight": "bold", "color": "#333333"},
        "body": {"font": "Arial", "size_pt": 11, "line_spacing": 1.15, "color": "#000000"},
        "code": {"font": "Consolas", "size_pt": 10, "background": "#f5f5f5"}
      },
      "colors": {
        "technical": "#0066cc",
        "success": "#00a859",
        "warning": "#ffcc00",
        "error": "#e80c4d",
        "neutral": "#666666"
      }
    },
    "quality_checks": {
      "min_pages": 50,
      "max_pages": 70,
      "pdf_format": "PDF/A-1b",
      "embedded_fonts": true,
      "searchable_text": true,
      "hyperlinked_toc": true,
      "min_contrast_ratio": 4.5,
      "diagram_alt_text": true
    }
  }
}
```

**Template Structure**:
- Each section has dedicated template file in `contracts/section-templates/`
- Templates use placeholders like `{{project_context}}`, `{{business_drivers}}`, `{{total_investment}}`
- Content extraction scripts populate placeholders from existing spec files
- Diagram definitions in PlantUML or equivalent declarative format
- Master template assembles all sections with consistent formatting

### Quickstart Guide (quickstart.md)

Developer onboarding for PDF generation:

1. **Prerequisites**:
   - LaTeX distribution: TeX Live (Linux/macOS) or MiKTeX (Windows) with pdflatex
   - Python 3.11+ with pip (for content extraction and automation scripts)
   - PlantUML 1.2024.x for diagram generation (requires Java Runtime)
   - Git (to access existing migration spec files)

2. **Installation**:
   ```bash
   # Clone repository
   git clone [repo-url]
   cd "POC Visual Age"
   git checkout 001-visual-age-migration-pdf

   # Install Python dependencies
   pip install -r scripts/generate-pdf/requirements.txt
   # Dependencies: markdown2, pyyaml, jinja2, pypdf2, pillow

   # Verify LaTeX installation
   pdflatex --version

   # Verify PlantUML installation
   java -jar plantuml.jar -version
   ```

3. **Generate PDF (Full Document)**:
   ```bash
   cd specs/001-visual-age-migration-pdf

   # Run complete generation pipeline
   python ../../scripts/generate-pdf/main.py --config config/document-config.yaml

   # Output: specs/001-visual-age-migration-pdf/output/migration-analysis-plan.pdf
   ```

4. **Generate PDF (Incremental - Single Section)**:
   ```bash
   # Regenerate only budget section after cost updates
   python ../../scripts/generate-pdf/main.py --section budget-roi --config config/document-config.yaml

   # Regenerate specific diagram
   python ../../scripts/generate-pdf/diagram-generator.py --diagram gantt-12-week-timeline
   ```

5. **Validate Generated PDF**:
   ```bash
   # Run quality checks
   python ../../scripts/generate-pdf/validators.py --pdf output/migration-analysis-plan.pdf

   # Output: specs/001-visual-age-migration-pdf/output/validation-report.md
   # Checks: page count (>=50), section presence (10 sections), hyperlinks, PDF/A compliance
   ```

6. **Update Content**:
   ```bash
   # When migration spec changes (e.g., new business rule added):
   # 1. Update source spec file
   vim ../001-visualage-dotnet-migration/spec.md

   # 2. Re-extract content
   python ../../scripts/generate-pdf/content-extractor.py --source ../001-visualage-dotnet-migration

   # 3. Regenerate affected sections
   python ../../scripts/generate-pdf/main.py --section legacy-system-analysis

   # 4. Validate
   python ../../scripts/generate-pdf/validators.py --pdf output/migration-analysis-plan.pdf
   ```

7. **Customize Templates**:
   ```bash
   # Edit section template
   vim contracts/section-templates/02-legacy-analysis.tex

   # Modify brand colors
   vim templates/document-generation/styles/colors.sty

   # Update document metadata
   vim config/document-config.yaml

   # Regenerate with changes
   python ../../scripts/generate-pdf/main.py --config config/document-config.yaml
   ```

8. **Troubleshooting**:
   - **LaTeX compilation errors**: Check `output/intermediate/*.log` for error details
   - **Missing diagrams**: Verify PlantUML Java version compatibility, check `.puml` syntax
   - **Font embedding issues**: Ensure fonts available in system font paths, check `preamble.tex`
   - **Page count mismatch**: Review content extraction, check for missing sections in template assembly
   - **PDF/A validation failures**: Use `verapdf` tool for detailed compliance report

### Agent Context Update

After completing Phase 1 artifacts, run:

```bash
.specify/scripts/bash/update-agent-context.sh claude
```

This will update `.specify/memory/claude-context.md` with:
- Document generation technology choice (LaTeX with pdflatex, or Pandoc Markdown, or HTML-to-PDF)
- Diagram generation approach (PlantUML for UML diagrams, pgfgantt for timeline)
- Content extraction strategy (Python markdown parsing, YAML frontmatter)
- Template processing (Jinja2 for variable substitution, LaTeX macros)
- Quality validation tooling (PyPDF2 for structure checks, verapdf for PDF/A compliance)
- Brand compliance implementation (fancyhdr for headers, xcolor for palette, custom style files)

## Phase 2: Task Breakdown (NOT in this command)

Phase 2 (task generation via `/speckit.tasks`) will create `tasks.md` with dependency-ordered implementation tasks. Preview of task categories:

1. **Setup & Dependencies** (T001-T005):
   - Install LaTeX distribution (TeX Live or MiKTeX)
   - Install Python dependencies (markdown2, jinja2, pypdf2)
   - Install PlantUML and configure Java runtime
   - Set up project directory structure
   - Initialize configuration files

2. **Content Extraction** (T006-T015):
   - Implement markdown parser for spec.md
   - Extract 42 business rules from legacy analysis
   - Parse function point breakdown from Key Entities
   - Extract component specifications (TypeScript/C# interfaces)
   - Parse timeline phases and milestones
   - Extract MIGRAI methodology principles
   - Parse budget and ROI data
   - Extract diagram requirements
   - Implement YAML frontmatter parser
   - Create data transformation utilities

3. **Template Development** (T016-T030):
   - Create master LaTeX template with preamble
   - Develop executive summary template
   - Develop legacy system analysis template (15+ pages)
   - Develop target architecture template (20+ pages)
   - Develop function point analysis template with tables
   - Develop timeline template with Gantt integration
   - Develop MIGRAI methodology template
   - Develop budget/ROI template with financial tables
   - Develop component specifications template
   - Develop risk management template
   - Develop appendices template
   - Create header/footer templates with logo
   - Create typography style definitions
   - Create color palette style definitions
   - Create table formatting macros

4. **Diagram Generation** (T031-T040):
   - Create PlantUML definition for high-level architecture (6-tier)
   - Create PlantUML definition for Clean Architecture onion
   - Create PlantUML definition for ER diagram (13 tables)
   - Create PlantUML definition for React component hierarchy
   - Create PlantUML definition for payment authorization sequence (10 participants)
   - Create pgfgantt definition for 12-week timeline
   - Create PlantUML definition for external service integration
   - Create PlantUML definition for Azure deployment architecture
   - Implement diagram generation script (PlantUML compiler)
   - Implement diagram embedding in LaTeX

5. **Function Point Automation** (T041-T050):
   - Implement IFPUG complexity lookup tables (DETs, FTRs, RETs)
   - Implement EI classification and calculation
   - Implement EO classification and calculation
   - Implement EQ classification and calculation
   - Implement ILF classification and calculation
   - Implement EIF classification and calculation
   - Implement UFP summation
   - Implement GSC assessment (14 factors)
   - Implement VAF calculation (0.65 + 0.01 × sum)
   - Implement AFP calculation and table generation

6. **PDF Assembly** (T051-T060):
   - Implement template processor (Jinja2 variable substitution)
   - Implement section assembly in correct order
   - Implement table of contents generation with hyperlinks
   - Implement LaTeX compilation pipeline (pdflatex 3-pass for TOC)
   - Implement brand asset embedding (logo PNG)
   - Implement font embedding configuration
   - Implement PDF/A-1b metadata injection
   - Implement page numbering (Page X of Y format)
   - Implement section color coding
   - Handle LaTeX compilation errors and logging

7. **Quality Validation** (T061-T070):
   - Implement page count validator (>= 50 pages)
   - Implement section presence validator (10 required sections)
   - Implement hyperlink validator (TOC links functional)
   - Implement PDF/A-1b compliance checker (verapdf integration)
   - Implement font embedding validator
   - Implement contrast ratio checker (4.5:1 minimum)
   - Implement diagram rendering validator
   - Implement searchable text validator
   - Generate validation report markdown
   - Implement CI/CD quality gates

8. **Documentation** (T071-T075):
   - Write quickstart.md with installation instructions
   - Document template customization guide
   - Document content update procedures
   - Document troubleshooting common issues
   - Create example configuration files

9. **Testing** (T076-T085):
   - Unit tests for content extractor (business rules, FPA, timeline)
   - Unit tests for template processor (variable substitution)
   - Unit tests for function point calculator (IFPUG formulas)
   - Unit tests for diagram generator (PlantUML compilation)
   - Integration test for full PDF generation
   - Integration test for incremental section regeneration
   - Visual regression test (compare diagrams to references)
   - PDF structure test (section order, TOC, hyperlinks)
   - Performance test (< 5 minutes generation time)
   - Quality validation test suite

10. **Deployment & Maintenance** (T086-T090):
    - Set up CI/CD pipeline for automated generation
    - Configure version control for templates and output
    - Create update procedures for spec changes
    - Document release process (versioning, changelog)
    - Set up automated quality checks in CI

## Implementation Sequence

**Recommended order**:

1. ✅ **Phase 0 Complete**: Research all technology decisions → `research.md`
2. ✅ **Phase 1 Complete**: Design document model, templates, contracts → `data-model.md`, `contracts/`, `quickstart.md`
3. **Phase 2** (via `/speckit.tasks`): Generate detailed task list → `tasks.md`
4. **Implementation**: Execute tasks in dependency order (setup → extraction → templates → diagrams → assembly → validation)
5. **Testing**: Validate generated PDF against all 15 success criteria
6. **Delivery**: Provide final PDF to stakeholders for project approval

## Success Criteria Mapping

Each success criterion from spec maps to verifiable outcomes:

- **SC-001 (50+ pages)**: Automated page count validator in quality checks
- **SC-002 (legacy system comprehension)**: Content extraction from existing spec ensures complete coverage of 42 rules, 13 tables
- **SC-003 (sufficient technical detail)**: Template population from component specifications in existing spec
- **SC-004 (180-220 AFP)**: Automated FPA calculation with IFPUG 4.3.1 formulas
- **SC-005 (R$ 180K-220K budget)**: Financial table generation from budget breakdown
- **SC-006 (realistic 3-month timeline)**: Gantt chart generation from timeline phases
- **SC-007 (MIGRAI methodology understanding)**: Template section for each of 6 principles
- **SC-008 (90% implementation without clarification)**: Detailed component specs in templates
- **SC-009 (professional diagram quality)**: PlantUML vector graphics with clear labels
- **SC-010 (professional formatting)**: LaTeX branded templates with consistent typography
- **SC-011 (executive decision in 15 min)**: Concise 2-3 page executive summary
- **SC-012 (15+ risks identified)**: Risk management section template with mitigation strategies
- **SC-013 (milestone-aligned payments)**: Payment schedule table in budget section
- **SC-014 (programmatic generation)**: Python automation scripts for updates
- **SC-015 (PDF/A-1b compliance)**: verapdf validation in quality checks

## Risk Mitigation

### High Risks

1. **LaTeX Compilation Complexity**:
   - **Risk**: LaTeX syntax errors, package conflicts, or font issues may prevent PDF generation
   - **Mitigation**: Use proven LaTeX templates, comprehensive error logging, fallback to simpler Pandoc Markdown if needed

2. **Diagram Generation Quality**:
   - **Risk**: PlantUML may not produce professional-quality diagrams matching expectations
   - **Mitigation**: Create reference diagrams manually in Lucidchart/Visio as templates, evaluate PlantUML output quality early, fall back to static PNG embeddings if needed

3. **Content Extraction Accuracy**:
   - **Risk**: Automated parsing may miss or misinterpret content from existing spec files
   - **Mitigation**: Implement comprehensive unit tests for extraction logic, manual review of extracted data, maintain extraction mappings in configuration

4. **Function Point Calculation Correctness**:
   - **Risk**: IFPUG formula implementation may produce incorrect AFP totals
   - **Mitigation**: Validate against manual FPA calculation, implement unit tests for complexity matrices, get certified FPA practitioner review

5. **Timeline Over-Estimation**:
   - **Risk**: Document generation may take longer than planned (5 minutes performance goal)
   - **Mitigation**: Profile LaTeX compilation, optimize diagram generation (cache compiled diagrams), implement incremental regeneration

### Medium Risks

6. **Brand Compliance Verification**:
   - **Risk**: Generated PDF may not match Caixa Seguradora brand guidelines exactly
   - **Mitigation**: Stakeholder review of first PDF output, iterative refinement of templates, maintain brand style guide reference

7. **PDF/A-1b Compliance**:
   - **Risk**: Generated PDF may fail PDF/A-1b archival format validation
   - **Mitigation**: Use verapdf tool early in development, configure LaTeX for PDF/A output, test with sample documents

8. **Template Maintainability**:
   - **Risk**: LaTeX templates may become complex and hard to update
   - **Mitigation**: Modular section templates, clear documentation, use Jinja2 for variable substitution to separate logic from presentation

### Low Risks

9. **Diagram Refresh Frequency**:
   - **Risk**: Diagrams may become stale if architecture changes
   - **Mitigation**: Document diagram update procedures, maintain .puml source files in version control, automated regeneration in CI/CD

10. **Spec Changes Requiring Manual Updates**:
    - **Risk**: Some spec changes may require manual template updates beyond automated extraction
    - **Mitigation**: Document template customization points, maintain change log, implement validation checks for missing content

## Next Steps

After `/speckit.plan` completion:

1. **Review and approve** this plan with technical leads
2. **Execute Phase 0**: Create `research.md` by researching all 8 technology decision areas
3. **Execute Phase 1**: Create `data-model.md`, `contracts/`, and `quickstart.md`
4. **Run** `/speckit.tasks` to generate detailed task breakdown in `tasks.md`
5. **Run** `/speckit.implement` to begin executing tasks (or manual implementation)
6. **Deliver** final PDF to stakeholders for migration project approval

## References

- Feature Specification: [spec.md](./spec.md)
- Existing Migration Spec: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/specs/001-visualage-dotnet-migration/spec.md`
- Existing Migration Plan: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/specs/001-visualage-dotnet-migration/plan.md`
- Existing Research Decisions: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/specs/001-visualage-dotnet-migration/research.md`
- LaTeX Documentation: https://www.latex-project.org/help/documentation/
- PlantUML Documentation: https://plantuml.com/
- Pandoc User Guide: https://pandoc.org/MANUAL.html
- IFPUG Function Point Counting Manual 4.3.1: http://www.ifpug.org/
- PDF/A Standard (ISO 19005-1): https://www.pdfa.org/
- Python markdown2 Library: https://github.com/trentm/python-markdown2
- Jinja2 Template Engine: https://jinja.palletsprojects.com/
- PyPDF2 Library: https://pypdf2.readthedocs.io/
- verapdf Validation Tool: https://verapdf.org/
