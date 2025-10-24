# Data Model: PDF Document Content Structure

**Feature**: Comprehensive Visual Age Migration Analysis & Planning Document
**Date**: 2025-10-23
**Phase**: Phase 1 - Design & Contracts

## Overview

This document defines the content model for extracting information from existing migration specifications and mapping it to PDF document sections. Each entity represents either a source of content (from existing specs) or a structured section in the generated PDF.

---

## Content Source Entities

These entities are extracted from existing specification files in `specs/001-visualage-dotnet-migration/`.

### SourceSpecification

**Purpose**: Container for all content extracted from existing spec.md
**File**: `specs/001-visualage-dotnet-migration/spec.md`

**Fields**:
- `user_stories`: List[UserStory] - 6 prioritized user stories (P1-P6)
- `functional_requirements`: List[FunctionalRequirement] - 62 FR with descriptions
- `business_rules`: List[BusinessRule] - 42 business rules from Visual Age
- `entities`: List[Entity] - 13 database entities with fields
- `success_criteria`: List[SuccessCriterion] - 15 measurable outcomes
- `key_entities_sections`: Dict[str, str] - Large text blocks from Key Entities section

**Extraction Method**: Python markdown2 parsing with section-based extraction

---

### UserStory

**Fields**:
- `id`: str (e.g., "US-001")
- `priority`: str (e.g., "P1", "P2")
- `title`: str
- `description`: str (plain language user need)
- `acceptance_scenarios`: List[str] (Given-When-Then scenarios)

**Usage**: Maps to "Project Scope" subsection of Executive Summary

---

### FunctionalRequirement

**Fields**:
- `id`: str (e.g., "FR-001", "FR-062")
- `category`: str (e.g., "Document Structure", "Legacy System Analysis", "Function Point Analysis")
- `requirement`: str (full text starting with "Document MUST...")
- `related_success_criteria`: List[str] (e.g., ["SC-001", "SC-004"])

**Usage**: Drives content generation for each section, validates completeness

---

### BusinessRule

**Fields**:
- `id`: str (e.g., "BR-001", "BR-042")
- `description`: str
- `esql_code`: str | None (ESQL sample if available)
- `migration_notes`: str | None

**Usage**: Populates "Legacy System Analysis" section, "Business Rules (42 rules)" subsection

---

### Entity

**Fields**:
- `name`: str (e.g., "ClaimMaster", "ClaimHistory")
- `table_name`: str (legacy name, e.g., "TMESTSIN", "THISTSIN")
- `fields`: List[Field]
- `relationships`: List[Relationship]
- `primary_key`: List[str]

**Sub-entities**:
- **Field**: `{name: str, column_name: str, type: str, nullable: bool, description: str}`
- **Relationship**: `{related_entity: str, cardinality: str, foreign_key: str}`

**Usage**: Generates ER Diagram (PlantUML) and "Database Schema" subsection

---

## Document Section Entities

These entities represent sections in the generated PDF, populated from source entities.

### DocumentMetadata

**Purpose**: PDF properties and branding
**Section**: N/A (PDF metadata fields)

**Fields**:
- `title`: str = "Visual Age to .NET Migration - Comprehensive Analysis & Plan"
- `author`: str = "Caixa Seguradora Migration Team"
- `subject`: str = "Legacy System Modernization"
- `keywords`: List[str] = ["Visual Age", ".NET 9", "React", "MIGRAI", "Function Points", "Insurance Claims"]
- `creation_date`: date = 2025-10-23
- `version`: str = "1.0"
- `confidentiality`: str = "CONFIDENTIAL - Internal Use Only"

**LaTeX Mapping**: `\hypersetup{pdftitle=..., pdfauthor=..., ...}`

---

### ExecutiveSummary

**Purpose**: 2-3 page C-level overview
**Section**: 1. Executive Summary

**Fields**:
- `project_context`: str (legacy IBM VisualAge EZEE system description)
- `business_drivers`: List[str] (mainframe cost reduction, technical debt, productivity, faster features)
- `solution_approach`: str (.NET 9 backend, React 19 frontend, Azure cloud)
- `scope_summary`: str (6 user stories, 55 FR overview)
- `timeline_summary`: str (3 months: 2 dev + 1 homologation)
- `budget_summary`: str (R$ 201,250 total investment)
- `roi_projection`: ROIProjection (annual savings, payback period)
- `key_risks`: List[Risk] (high-priority risks with mitigation)
- `decision_framework`: str (go/no-go criteria)

**Content Source**:
- `project_context`: FR-001 through FR-006 synthesis
- `business_drivers`: Assumptions section + existing migration spec context
- `solution_approach`: FR-012 through FR-019 (Target Architecture)
- `budget_summary`: BudgetAndROI entity
- `roi_projection`: ROIProjection entity

**LaTeX Template**: `contracts/section-templates/01-executive-summary.tex`

---

### LegacySystemAnalysis

**Purpose**: 15+ page comprehensive Visual Age SIWEA analysis
**Section**: 2. Legacy System Analysis

**Fields**:
- `architecture_overview`: str (CICS, DB2, ESQL, SOAP web services)
- `business_rules`: List[BusinessRule] (42 rules from source)
- `database_schema`: DatabaseSchema (13 entities from source)
- `transaction_flows`: List[TransactionFlow] (CICS map flows)
- `external_integrations`: List[ExternalIntegration] (CNOUA/SIPUA/SIMDA)
- `performance_metrics`: PerformanceMetrics (response times, concurrent users, TPS)
- `technical_debt`: TechnicalDebt (undocumented rules, no tests, mainframe costs)

**Content Source**:
- `business_rules`: Extracted BusinessRule entities
- `database_schema`: Extracted Entity entities
- `external_integrations`: FR-010, existing spec "External Service Integration" section

**LaTeX Template**: `contracts/section-templates/02-legacy-analysis.tex`

---

### TargetArchitecture

**Purpose**: 20+ page .NET 9 + React 19 solution specification
**Section**: 3. Target Architecture Specification

**Fields**:
- `clean_architecture_pattern`: CleanArchitectureDesign (layers, dependencies)
- `technology_stack`: TechnologyStack (.NET 9, React 19, EF Core, SoapCore)
- `service_designs`: List[ServiceDesign] (IClaimService, IPaymentService, etc.)
- `component_specifications`: List[ComponentSpec] (React components, backend services)
- `api_contracts`: APIContracts (OpenAPI 3.0 REST, WSDL SOAP)
- `database_strategy`: DatabaseStrategy (EF Core Fluent API mappings)
- `deployment_architecture`: DeploymentArchitecture (Docker, Azure)

**Content Source**:
- `clean_architecture_pattern`: FR-012
- `service_designs`: FR-014, Component Specification Section from source spec
- `component_specifications`: FR-020 through FR-024

**LaTeX Template**: `contracts/section-templates/03-target-architecture.tex`

---

### FunctionPointAnalysis

**Purpose**: 5+ page IFPUG 4.3.1 detailed calculation
**Section**: 4. Function Point Analysis

**Fields**:
- `component_breakdown`: Dict[str, List[FPComponent]] (EI/EO/EQ/ILF/EIF)
- `ufp_total`: int (unadjusted function points sum)
- `gsc_assessment`: List[GSCFactor] (14 factors with degrees 0-5)
- `vaf`: float (Value Adjustment Factor: 0.65 + 0.01 × sum of GSC)
- `afp_total`: int (Adjusted Function Points: UFP × VAF)

**Sub-entities**:
- **FPComponent**: `{name: str, dets: int, ftrs: int | rets: int, complexity: str, function_points: int}`
- **GSCFactor**: `{id: int, name: str, degree: int (0-5), rationale: str}`

**Content Source**:
- `component_breakdown`: Extracted from "Function Point Analysis Section" in source spec Key Entities
- `gsc_assessment`: FR-030 (14 GSC factors with degrees)
- Calculated via: `function-point-calculator.py`

**LaTeX Template**: `contracts/section-templates/04-function-points.tex` (includes tables)

---

### ProjectTimeline

**Purpose**: 4+ page scheduling with Gantt chart
**Section**: 5. Project Timeline and Scheduling

**Fields**:
- `phases`: List[Phase] (Phase 0 through Phase 6)
- `milestones`: List[Milestone] (M1 through M8 with dates)
- `critical_path`: List[str] (task IDs on critical path)
- `resource_allocation`: List[ResourceAssignment]

**Sub-entities**:
- **Phase**: `{id: int, name: str, weeks: str, duration_days: int, tasks: List[Task], deliverables: str}`
- **Task**: `{id: str, name: str, start_date: date, end_date: date, dependencies: List[str], resource: str}`
- **Milestone**: `{id: str, name: str, date: date, deliverable: str}`
- **ResourceAssignment**: `{role: str, fte: float, start_week: int, end_week: int}`

**Content Source**:
- `phases`: Extracted from plan.md FR-032 through FR-038
- `milestones`: FR-056 (Gantt chart section)

**LaTeX/PlantUML**: Gantt chart generated via `contracts/diagram-definitions/gantt-timeline.puml` or pgfgantt

---

### MIGRAIMethodology

**Purpose**: 6+ page framework explanation
**Section**: 6. MIGRAI Methodology Framework

**Fields**:
- `principles`: List[MIGRAIPrinciple] (6 principles)

**Sub-entity**:
- **MIGRAIPrinciple**: `{name: str, description: str, examples: List[str], benefits: List[str]}`

**Content Source**:
- Extracted from FR-039 through FR-046
- Principles: Modernization, Intelligence (LLM), Gradual migration, Resilience, Automation, Integration

**LaTeX Template**: `contracts/section-templates/06-migrai-methodology.tex`

---

### BudgetAndROI

**Purpose**: 4+ page financial analysis
**Section**: 7. Budget and ROI Analysis

**Fields**:
- `development_cost`: Cost (AFP × rate)
- `infrastructure_costs`: List[Cost] (Azure services)
- `additional_costs`: List[Cost] (training, licenses)
- `contingency_reserve`: Cost (15% of subtotal)
- `total_investment`: Cost (sum of all)
- `payment_milestones`: List[Milestone] (5 milestones with percentages)
- `roi_projection`: ROIProjection (annual savings, payback)

**Sub-entities**:
- **Cost**: `{category: str, description: str, amount: Decimal, currency: str = "BRL"}`
- **Milestone**: `{number: int, deliverable: str, percentage: int, amount: Decimal}`
- **ROIProjection**: `{annual_savings: Decimal, payback_period_years: float, five_year_net_value: Decimal}`

**Content Source**:
- Extracted from FR-047 through FR-050
- Source spec "Budget and Cost Section" from Key Entities

**LaTeX Template**: `contracts/section-templates/07-budget-roi.tex` (includes booktabs financial tables)

---

### ComponentSpecifications

**Purpose**: 8+ page detailed component designs
**Section**: 8. Component and Service Specifications

**Fields**:
- `react_components`: List[ReactComponent]
- `backend_services`: List[BackendService]
- `api_integrations`: List[APIIntegration]

**Sub-entities**:
- **ReactComponent**: `{name: str, props: Dict, state: Dict, hooks: List[str], validation: str, api_calls: List[str], styling: List[str]}`
- **BackendService**: `{interface_name: str, methods: List[Method], dependencies: List[str], transaction_handling: str}`
- **Method**: `{name: str, parameters: List[Parameter], return_type: str, description: str}`

**Content Source**:
- Extracted from FR-020 through FR-024
- Source spec "Component Specification Section" from Key Entities

**LaTeX Template**: `contracts/section-templates/08-component-specs.tex`

---

### DiagramCollection

**Purpose**: 7 professional diagrams
**Section**: Embedded throughout document

**Fields**:
- `diagrams`: List[Diagram]

**Sub-entity**:
- **Diagram**: `{id: str, type: str, title: str, description: str, plantuml_definition: str, output_path: str}`

**Diagrams**:
1. High-Level Architecture (6-tier layered)
2. Clean Architecture Onion (4 concentric circles)
3. ER Diagram (13 tables with relationships)
4. React Component Hierarchy (tree structure)
5. Payment Authorization Sequence (10 participants)
6. Gantt Timeline (12-week chart with critical path)
7. Deployment Architecture (Azure resources)

**Content Source**:
- Generated from FR-051 through FR-056
- PlantUML definitions in `contracts/diagram-definitions/*.puml`

**Generation**: `scripts/generate-pdf/diagram-generator.py` calls PlantUML

---

## Data Flow

```
Source Specs (spec.md, plan.md, research.md)
    ↓
[content-extractor.py]
    ↓
Structured Entities (Python objects)
    ↓
[template-processor.py with Jinja2]
    ↓
Populated LaTeX Templates (*.tex files with content)
    ↓
[diagram-generator.py with PlantUML]
    ↓
Generated Diagrams (*.pdf)
    ↓
[pdf-assembler.py with pdflatex]
    ↓
Final PDF Document (migration-analysis-plan.pdf)
    ↓
[validators.py with PyPDF2/verapdf]
    ↓
Validation Report (validation-report.md)
```

---

## Entity Relationship Summary

- **SourceSpecification** contains → **UserStory**, **FunctionalRequirement**, **BusinessRule**, **Entity**
- **ExecutiveSummary** references → **BudgetAndROI** (for budget summary), **ProjectTimeline** (for duration), **FunctionPointAnalysis** (for AFP total)
- **LegacySystemAnalysis** contains → **BusinessRule** (42 rules), **Entity** (13 tables), **ExternalIntegration** (3 services)
- **TargetArchitecture** contains → **ComponentSpecifications** (detailed designs), **DeploymentArchitecture**
- **FunctionPointAnalysis** calculates → **BudgetAndROI**.development_cost (AFP × R$ 750)
- **ProjectTimeline** aligns with → **BudgetAndROI**.payment_milestones (phase completion gates)
- **DiagramCollection** visualizes → **LegacySystemAnalysis**, **TargetArchitecture**, **ProjectTimeline**

---

## JSON Schema Example

For validation and contract testing:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "PDF Document Content Model",
  "type": "object",
  "required": ["metadata", "executive_summary", "legacy_analysis", "target_architecture", "function_points", "timeline", "migrai", "budget_roi"],
  "properties": {
    "metadata": {
      "type": "object",
      "properties": {
        "title": {"type": "string"},
        "author": {"type": "string"},
        "version": {"type": "string", "pattern": "^\\d+\\.\\d+$"}
      }
    },
    "executive_summary": {
      "type": "object",
      "properties": {
        "project_context": {"type": "string", "minLength": 100},
        "budget_summary": {"type": "string"},
        "roi_projection": {
          "type": "object",
          "properties": {
            "annual_savings": {"type": "number", "minimum": 0},
            "payback_period_years": {"type": "number", "minimum": 0}
          }
        }
      }
    },
    "function_points": {
      "type": "object",
      "properties": {
        "afp_total": {"type": "integer", "minimum": 180, "maximum": 220},
        "vaf": {"type": "number", "minimum": 0.65, "maximum": 1.35}
      }
    }
  }
}
```

---

**Data Model Complete**: 2025-10-23
**Next**: Create contracts/ directory with templates and schemas
