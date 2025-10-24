# Quickstart Guide: PDF Document Generation

**Feature**: Comprehensive Visual Age Migration Analysis & Planning Document
**Last Updated**: 2025-10-23

## Overview

This guide provides step-by-step instructions for setting up the PDF document generation environment, generating the comprehensive migration analysis document, and validating the output quality.

---

## Prerequisites

### Required Software

1. **LaTeX Distribution**
   - **Linux (Debian/Ubuntu)**: `sudo apt install texlive-full` (2-3 GB download)
   - **macOS**: `brew install --cask mactex` (4+ GB download)
   - **Windows**: Download MiKTeX from https://miktex.org/download (auto-installs packages on demand)

2. **Python 3.11+**
   - Verify: `python --version` or `python3 --version`
   - Install: https://www.python.org/downloads/

3. **Java Runtime Environment** (for PlantUML diagram generation)
   - Verify: `java -version`
   - Install: `sudo apt install default-jre` (Linux) or `brew install java` (macOS)

4. **PlantUML**
   - Download JAR: `wget https://github.com/plantuml/plantuml/releases/download/v1.2024.3/plantuml-1.2024.3.jar -O plantuml.jar`
   - Place in project root or system PATH

5. **Git** (to access existing migration specs)
   - Verify: `git --version`

### Optional Tools

- **verapdf** (for PDF/A-1b validation): https://verapdf.org/software/
- **PDF viewer** with search capability (Adobe Acrobat, macOS Preview, Evince)

---

## Installation

### 1. Clone Repository and Checkout Branch

```bash
# Navigate to project root
cd "/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age"

# Checkout feature branch
git checkout 001-visual-age-migration-pdf

# Verify you're on correct branch
git branch --show-current
# Should output: 001-visual-age-migration-pdf
```

### 2. Install Python Dependencies

```bash
# Create virtual environment (recommended)
python3 -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install markdown2 pyyaml jinja2 pypdf2 pillow

# Or if requirements.txt exists:
pip install -r scripts/generate-pdf/requirements.txt
```

**Dependencies**:
- `markdown2`: Markdown parsing for spec files
- `pyyaml`: YAML configuration loading
- `jinja2`: Template processing
- `pypdf2`: PDF structure validation
- `pillow`: Image processing (logo extraction)

### 3. Verify LaTeX Installation

```bash
# Check pdflatex is available
pdflatex --version
# Should output: pdfTeX 3.x ...

# Check required packages (should be included in texlive-full)
kpsewhich booktabs.sty    # Professional tables
kpsewhich fancyhdr.sty    # Headers/footers
kpsewhich xcolor.sty      # Brand colors
kpsewhich hyperref.sty    # Hyperlinks
kpsewhich siunitx.sty     # Number formatting
```

### 4. Verify PlantUML

```bash
# Test PlantUML
java -jar plantuml.jar -version
# Should output: PlantUML version 1.2024.x

# Test diagram generation (optional)
echo "@startuml\nAlice -> Bob: Hello\n@enduml" > test.puml
java -jar plantuml.jar -tpdf test.puml
# Should create test.pdf
rm test.puml test.pdf
```

---

## Usage

### Generate Complete PDF Document

```bash
# Navigate to feature directory
cd specs/001-visual-age-migration-pdf

# Run document generation pipeline
python ../../scripts/generate-pdf/main.py --config config/document-config.yaml

# Output will be created at:
# specs/001-visual-age-migration-pdf/output/migration-analysis-plan.pdf
```

**Expected Runtime**: 3-5 minutes for full 50+ page document

**What Happens**:
1. Content extraction from existing specs (10s)
2. Diagram generation from PlantUML (35s for 7 diagrams)
3. Template processing with Jinja2 (5s)
4. LaTeX compilation (3 passes, 120s)
5. Quality validation (20s)

### Generate Specific Section Only

```bash
# Regenerate only budget section (faster iteration)
python ../../scripts/generate-pdf/main.py \
  --section budget-roi \
  --config config/document-config.yaml

# Or regenerate executive summary after content updates
python ../../scripts/generate-pdf/main.py \
  --section executive-summary \
  --config config/document-config.yaml
```

**Available Sections**:
- `executive-summary`
- `legacy-analysis`
- `target-architecture`
- `function-points`
- `timeline`
- `migrai-methodology`
- `budget-roi`
- `component-specs`
- `risk-management`
- `appendices`

### Generate Individual Diagram

```bash
# Regenerate specific diagram
python ../../scripts/generate-pdf/diagram-generator.py \
  --diagram gantt-timeline

# Output: output/diagrams/gantt-timeline.pdf

# Generate all diagrams
python ../../scripts/generate-pdf/diagram-generator.py --all
```

### Validate Generated PDF

```bash
# Run quality checks
python ../../scripts/generate-pdf/validators.py \
  output/migration-analysis-plan.pdf

# View validation report
cat output/validation-report.md
```

**Validation Checks** (15 criteria):
- Page count >= 50
- All 10 sections present
- Hyperlinks functional
- PDF/A-1b compliant
- Fonts embedded
- Searchable text layer
- 7+ diagrams present
- Contrast ratio >= 4.5:1

---

## Common Workflows

### Workflow 1: Update Budget and Regenerate

```bash
# 1. Update source spec
vim ../001-visualage-dotnet-migration/spec.md
# Edit Budget and Cost Section with new amounts

# 2. Re-extract content
python ../../scripts/generate-pdf/content-extractor.py \
  --source ../001-visualage-dotnet-migration

# 3. Regenerate budget section only
python ../../scripts/generate-pdf/main.py --section budget-roi

# 4. Validate
python ../../scripts/generate-pdf/validators.py output/migration-analysis-plan.pdf
```

### Workflow 2: Add New Business Rule

```bash
# 1. Add business rule to source spec
vim ../001-visualage-dotnet-migration/spec.md
# Add BR-043 in Legacy System Analysis section

# 2. Regenerate legacy analysis section
python ../../scripts/generate-pdf/main.py --section legacy-analysis

# 3. Verify rule appears in PDF
pdfgrep "BR-043" output/migration-analysis-plan.pdf
```

### Workflow 3: Update Timeline

```bash
# 1. Edit Gantt chart PlantUML definition
vim contracts/diagram-definitions/gantt-timeline.puml
# Adjust task dates or add new tasks

# 2. Regenerate diagram
python ../../scripts/generate-pdf/diagram-generator.py --diagram gantt-timeline

# 3. Regenerate timeline section
python ../../scripts/generate-pdf/main.py --section timeline

# 4. Visual verification
open output/migration-analysis-plan.pdf
# Navigate to Project Timeline section, verify Gantt chart
```

### Workflow 4: Customize Brand Colors

```bash
# 1. Edit colors style file
vim templates/document-generation/styles/colors.sty
# Change #0066CC to new brand blue

# 2. Regenerate entire document (colors affect all sections)
python ../../scripts/generate-pdf/main.py

# 3. Visual verification
open output/migration-analysis-plan.pdf
# Check headers are new color
```

---

## Troubleshooting

### LaTeX Compilation Errors

**Problem**: `! LaTeX Error: File 'booktabs.sty' not found.`

**Solution**: Install missing package
```bash
# Debian/Ubuntu
sudo apt install texlive-latex-extra

# macOS (reinstall MacTeX full)
brew reinstall --cask mactex

# Windows MiKTeX (auto-installs on first use)
# Just run pdflatex again, MiKTeX will prompt to install
```

**Problem**: `! Undefined control sequence. \ganttbar`

**Solution**: Install pgfgantt package
```bash
sudo apt install texlive-pictures  # Contains pgfgantt
```

### Diagram Generation Issues

**Problem**: `java.lang.OutOfMemoryError` when generating diagrams

**Solution**: Increase Java heap size
```bash
java -Xmx2048m -jar plantuml.jar -tpdf diagram.puml
```

**Problem**: PlantUML syntax error in `.puml` file

**Solution**: Test PlantUML syntax at https://www.plantuml.com/plantuml/uml/
```bash
# Copy .puml content to web editor for syntax validation
# Fix errors, then regenerate locally
```

### Font Embedding Issues

**Problem**: PDF validation fails "Fonts not embedded"

**Solution**: Ensure PDF/A mode active in preamble
```latex
% In templates/document-generation/preamble.tex
\usepackage[a-1b]{pdfx}  % Enforces font embedding
```

**Problem**: Arial font not found

**Solution**: Use Helvetica substitute
```latex
% In templates/document-generation/styles/headings.sty
\usepackage{helvet}  % Arial-like font
\renewcommand{\familydefault}{\sfdefault}
```

### Content Extraction Failures

**Problem**: `KeyError: 'business_rules'` when extracting content

**Solution**: Verify source spec structure
```bash
# Check if section exists
grep -n "## Legacy System Analysis" ../001-visualage-dotnet-migration/spec.md

# If missing, extraction script needs to handle gracefully
# Edit content-extractor.py to use .get() instead of direct access
```

### PDF Page Count Too Low

**Problem**: Generated PDF only 35 pages (need >= 50)

**Solution**: Check for missing sections
```bash
# Run validation to identify missing content
python ../../scripts/generate-pdf/validators.py output/migration-analysis-plan.pdf

# Example output:
# FAIL: Missing sections: Risk Management, Appendices

# Implement missing section templates
# Then regenerate
```

### Validation Fails: Hyperlinks Not Working

**Problem**: PDF validation reports "Only 3 hyperlinks, expected >= 10"

**Solution**: Ensure hyperref package configured correctly
```latex
% In templates/document-generation/preamble.tex
\usepackage{hyperref}
\hypersetup{
    colorlinks=true,
    linkcolor=CaixaPrimary,
    bookmarks=true,
    bookmarksopen=true
}

% Ensure TOC uses \hyperref
\tableofcontents  % Should auto-create links
```

---

## Performance Optimization

### Speed Up Incremental Builds

1. **Cache Compiled Diagrams**: Don't regenerate unchanged diagrams
   ```bash
   # Check diagram modification time before regenerating
   # Only regenerate if .puml newer than .pdf
   ```

2. **Skip Validation in Development**: Run validation only before delivery
   ```bash
   python ../../scripts/generate-pdf/main.py --skip-validation
   ```

3. **Use Single-Pass LaTeX**: For quick previews (TOC/refs may be wrong)
   ```bash
   pdflatex -interaction=nonstopmode document.tex
   # Skip 2nd and 3rd passes
   ```

### Reduce PDF File Size

If PDF exceeds 10 MB target:

1. **Compress Images**: Use lower resolution logo (150 DPI vs 300 DPI)
2. **Optimize Diagrams**: Generate PNG instead of PDF for raster-like diagrams
3. **Remove Redundant Fonts**: Ensure only used fonts embedded

```bash
# Check PDF size
ls -lh output/migration-analysis-plan.pdf

# If too large, compress with Ghostscript
gs -sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS=/ebook \
   -dNOPAUSE -dQUIET -dBATCH \
   -sOutputFile=output/migration-analysis-plan-compressed.pdf \
   output/migration-analysis-plan.pdf
```

---

## Next Steps

1. ✅ **Environment Setup Complete**: All dependencies installed
2. **Generate First PDF**: Run full generation pipeline
3. **Review Output**: Open PDF, verify all sections present and formatted correctly
4. **Iterate on Templates**: Customize LaTeX templates for better formatting
5. **Stakeholder Review**: Share PDF for feedback on content and presentation
6. **Automate in CI/CD**: Set up GitHub Actions workflow for automated generation

---

## Additional Resources

- **LaTeX Documentation**: https://www.latex-project.org/help/documentation/
- **PlantUML Guide**: https://plantuml.com/guide
- **Jinja2 Templates**: https://jinja.palletsprojects.com/en/3.1.x/templates/
- **PDF/A Standard**: https://www.pdfa.org/
- **verapdf Validation**: https://docs.verapdf.org/

---

**Quickstart Guide Complete**: 2025-10-23
**Ready**: Environment setup and document generation
