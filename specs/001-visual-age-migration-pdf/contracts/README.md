# Contracts Directory

This directory contains all templates, schemas, and assets required for PDF document generation.

## Structure

```
contracts/
├── document-schema.json          # JSON schema defining PDF requirements
├── section-templates/            # LaTeX templates for each section
│   ├── 01-executive-summary.tex
│   ├── 02-legacy-analysis.tex
│   ├── 03-target-architecture.tex
│   ├── 04-function-points.tex
│   ├── 05-timeline.tex
│   ├── 06-migrai-methodology.tex
│   ├── 07-budget-roi.tex
│   ├── 08-component-specs.tex
│   ├── 09-risk-management.tex
│   └── 10-appendices.tex
├── diagram-definitions/          # PlantUML diagram definitions
│   ├── architecture.puml
│   ├── clean-architecture-onion.puml
│   ├── er-diagram.puml
│   ├── component-hierarchy.puml
│   ├── sequence-payment-auth.puml
│   ├── gantt-timeline.puml
│   └── deployment-azure.puml
└── assets/                       # Static resources
    └── caixa-logo.png           # Caixa Seguradora logo (extract from existing spec)
```

## Template Variable Conventions

All LaTeX templates use Jinja2 syntax for variable substitution:

- `{{ variable_name }}`: Simple variable substitution
- `{% for item in items %}...{% endfor %}`: Loop over collections
- `{% if condition %}...{% endif %}`: Conditional content

Example variables:
- `{{project_context}}`: Executive summary project context
- `{{total_investment}}`: Total budget amount
- `{{afp_total}}`: Adjusted function points total

## Diagram Generation

Diagrams are defined in PlantUML `.puml` files and compiled to PDF using:

```bash
java -jar plantuml.jar -tpdf -o/absolute/output/path diagram-file.puml
```

## Next Steps

1. Implement section templates (T016-T030 in tasks.md)
2. Create PlantUML diagram definitions (T031-T040)
3. Extract Caixa logo from existing spec base64 data
4. Test template rendering with sample data
