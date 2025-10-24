#!/usr/bin/env python3
"""
PDF Generator using ReportLab (Alternative to LaTeX)
Generates the Visual Age Migration Analysis & Planning Document
"""

import json
import os
import sys
from pathlib import Path
from datetime import datetime

from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import cm
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, PageBreak,
    Table, TableStyle, Image, KeepTogether
)
from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_JUSTIFY

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent.parent))


def create_styles():
    """Create custom styles for the document"""
    styles = getSampleStyleSheet()

    # Custom styles
    styles.add(ParagraphStyle(
        name='CustomTitle',
        parent=styles['Heading1'],
        fontSize=24,
        textColor=colors.HexColor('#0066CC'),
        spaceAfter=30,
        alignment=TA_CENTER,
        fontName='Helvetica-Bold'
    ))

    styles.add(ParagraphStyle(
        name='CustomHeading1',
        parent=styles['Heading1'],
        fontSize=18,
        textColor=colors.HexColor('#0066CC'),
        spaceAfter=12,
        fontName='Helvetica-Bold'
    ))

    styles.add(ParagraphStyle(
        name='CustomHeading2',
        parent=styles['Heading2'],
        fontSize=16,
        textColor=colors.HexColor('#0066CC'),
        spaceAfter=10,
        fontName='Helvetica-Bold'
    ))

    styles.add(ParagraphStyle(
        name='CustomHeading3',
        parent=styles['Heading3'],
        fontSize=14,
        textColor=colors.HexColor('#0066CC'),
        spaceAfter=8,
        fontName='Helvetica-Bold'
    ))

    styles.add(ParagraphStyle(
        name='CustomBody',
        parent=styles['BodyText'],
        fontSize=11,
        leading=13,
        alignment=TA_JUSTIFY,
        spaceAfter=10,
        fontName='Helvetica'
    ))

    styles.add(ParagraphStyle(
        name='CustomCode',
        parent=styles['Code'],
        fontSize=10,
        fontName='Courier',
        backColor=colors.HexColor('#F5F5F5'),
        leftIndent=10,
        rightIndent=10
    ))

    return styles


def add_header_footer(canvas, doc):
    """Add header and footer to each page"""
    canvas.saveState()

    # Header
    header_height = A4[1] - 2*cm
    canvas.setFont('Helvetica-Bold', 10)
    canvas.setFillColor(colors.HexColor('#0066CC'))
    canvas.drawString(2*cm, header_height, "Visual Age to .NET Migration")
    canvas.drawString(A4[0] - 6*cm, header_height, "Análise & Planejamento")

    # Footer
    footer_height = 1.5*cm
    canvas.setFont('Helvetica', 9)
    canvas.setFillColor(colors.black)
    canvas.drawString(2*cm, footer_height, f"Página {doc.page}")
    canvas.drawCentredString(A4[0]/2, footer_height, "v1.0 - Outubro 2025")
    canvas.drawRightString(A4[0] - 2*cm, footer_height, "CONFIDENCIAL")

    canvas.restoreState()


def load_content():
    """Load extracted content from JSON files"""
    try:
        content_path = Path(__file__).parent.parent.parent / "output" / "intermediate" / "extracted_content.json"
        if content_path.exists():
            with open(content_path, 'r', encoding='utf-8') as f:
                return json.load(f)
    except Exception as e:
        print(f"Warning: Could not load content: {e}")

    # Default content if file doesn't exist
    return {
        "project_name": "Migração Visual Age para .NET 9 + React 19",
        "total_investment": "R$ 222.812,50",
        "afp_total": 225,
        "timeline_weeks": 12,
        "generated_date": datetime.now().strftime("%d/%m/%Y")
    }


def generate_cover_page(story, styles, content):
    """Generate cover page"""
    # Logo (temporarily disabled due to image format issue)
    # logo_path = Path(__file__).parent.parent.parent / "contracts" / "assets" / "caixa-logo.png"
    # if logo_path.exists():
    #     img = Image(str(logo_path), width=8*cm, height=2*cm)
    #     img.hAlign = 'CENTER'
    #     story.append(img)
    #     story.append(Spacer(1, 2*cm))

    # Add company name as text instead
    company = Paragraph("<b>CAIXA SEGURADORA</b>", styles['CustomTitle'])
    story.append(company)
    story.append(Spacer(1, 1*cm))

    # Title
    title = Paragraph("Visual Age to .NET Migration", styles['CustomTitle'])
    story.append(title)

    subtitle = Paragraph("Análise Abrangente & Planejamento", styles['CustomHeading2'])
    story.append(subtitle)
    story.append(Spacer(1, 1*cm))

    # Project info
    info_text = f"""
    <b>Cliente:</b> Caixa Seguradora<br/>
    <b>Sistema Legado:</b> IBM VisualAge EZEE SIWEA<br/>
    <b>Tecnologia Alvo:</b> .NET 9 + React 19<br/>
    <b>Investimento Total:</b> {content.get('total_investment', 'R$ 222.812,50')}<br/>
    <b>Pontos de Função:</b> {content.get('afp_total', 225)} AFP<br/>
    <b>Prazo:</b> {content.get('timeline_weeks', 12)} semanas<br/>
    <b>Data:</b> {content.get('generated_date', datetime.now().strftime("%d/%m/%Y"))}<br/>
    """
    info = Paragraph(info_text, styles['CustomBody'])
    story.append(info)
    story.append(PageBreak())


def generate_executive_summary(story, styles, content):
    """Generate executive summary section"""
    story.append(Paragraph("1. Sumário Executivo", styles['CustomHeading1']))
    story.append(Spacer(1, 0.5*cm))

    # Project Context
    story.append(Paragraph("1.1 Contexto do Projeto", styles['CustomHeading2']))
    context_text = """
    Este documento apresenta a análise abrangente e o planejamento detalhado para a migração
    do sistema legado IBM VisualAge EZEE Claims Indemnity Payment Authorization System (SIWEA)
    para uma arquitetura moderna baseada em .NET 9 e React 19. O sistema SIWEA atualmente
    processa solicitações de autorização de pagamento de sinistros de seguros, integrando-se
    com múltiplos sistemas externos (CNOUA, SIPUA, SIMDA) e gerenciando 13 entidades de banco
    de dados legadas.
    """
    story.append(Paragraph(context_text, styles['CustomBody']))
    story.append(Spacer(1, 0.3*cm))

    # Business Drivers
    story.append(Paragraph("1.2 Drivers de Negócio", styles['CustomHeading2']))
    drivers_data = [
        ["Driver", "Impacto Esperado"],
        ["Redução de Custos de Mainframe", "R$ 30.000/ano em licenciamento IBM"],
        ["Melhoria de Produtividade", "20% ganho de eficiência com ferramentas modernas"],
        ["Time-to-Market", "Ciclo de desenvolvimento reduzido de 6 para 2 meses"],
        ["Redução de Débito Técnico", "Código moderno e manutenível com Clean Architecture"]
    ]

    drivers_table = Table(drivers_data, colWidths=[8*cm, 8*cm])
    drivers_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#0066CC')),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'LEFT'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('FONTSIZE', (0, 0), (-1, 0), 12),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
        ('FONTNAME', (0, 1), (-1, -1), 'Helvetica'),
        ('FONTSIZE', (0, 1), (-1, -1), 10),
        ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
    ]))
    story.append(drivers_table)
    story.append(Spacer(1, 0.5*cm))

    # Solution Approach
    story.append(Paragraph("1.3 Abordagem da Solução", styles['CustomHeading2']))
    solution_text = """
    <b>Backend:</b> ASP.NET Core 9.0 com Clean Architecture (API, Core, Infrastructure)<br/>
    <b>Frontend:</b> React 19 com TypeScript, Vite, React Router<br/>
    <b>Banco de Dados:</b> Entity Framework Core 9 com abordagem database-first<br/>
    <b>Integrações:</b> Manutenção de contratos SOAP existentes + novos endpoints REST<br/>
    <b>Cloud:</b> Deployment em Azure App Service com SQL Database<br/>
    <b>Metodologia:</b> MIGRAI (Modernization, Intelligence, Gradual, Resilience, Automation, Integration)
    """
    story.append(Paragraph(solution_text, styles['CustomBody']))
    story.append(PageBreak())


def generate_function_points(story, styles, content):
    """Generate function point analysis section"""
    story.append(Paragraph("4. Análise de Pontos de Função", styles['CustomHeading1']))
    story.append(Spacer(1, 0.5*cm))

    # Methodology
    methodology_text = """
    Esta análise utiliza a metodologia IFPUG 4.3.1 (International Function Point Users Group)
    para calcular os Pontos de Função Ajustados (AFP) do projeto de migração. A metodologia
    considera cinco tipos de componentes funcionais e aplica fatores de ajuste baseados em
    14 Características Gerais do Sistema (GSC).
    """
    story.append(Paragraph(methodology_text, styles['CustomBody']))
    story.append(Spacer(1, 0.3*cm))

    # Component Breakdown
    story.append(Paragraph("4.1 Breakdown de Componentes", styles['CustomHeading2']))

    fp_data = [
        ["Tipo", "Quantidade", "Complexidade", "FP Unitário", "FP Total"],
        ["External Inputs (EI)", "1", "Média", "4", "4"],
        ["External Outputs (EO)", "0", "-", "5", "0"],
        ["External Inquiries (EQ)", "3", "Média", "4", "12"],
        ["Internal Logical Files (ILF)", "17", "Média", "10", "170"],
        ["External Interface Files (EIF)", "3", "Baixa", "7", "21"],
        ["", "", "", "<b>UFP Total</b>", "<b>199</b>"]
    ]

    fp_table = Table(fp_data, colWidths=[5*cm, 3*cm, 3*cm, 3*cm, 2*cm])
    fp_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#0066CC')),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('FONTSIZE', (0, 0), (-1, 0), 11),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -2), colors.beige),
        ('BACKGROUND', (0, -1), (-1, -1), colors.HexColor('#00A859')),
        ('TEXTCOLOR', (0, -1), (-1, -1), colors.whitesmoke),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
        ('FONTNAME', (0, 1), (-1, -1), 'Helvetica'),
        ('FONTSIZE', (0, 1), (-1, -1), 10),
    ]))
    story.append(fp_table)
    story.append(Spacer(1, 0.5*cm))

    # VAF Calculation
    story.append(Paragraph("4.2 Fator de Ajuste de Valor (VAF)", styles['CustomHeading2']))
    vaf_text = """
    O VAF é calculado com base em 14 Características Gerais do Sistema, cada uma pontuada de
    0 a 5 conforme o grau de influência:<br/><br/>
    <b>Fórmula:</b> VAF = 0.65 + (0.01 × soma dos graus de influência)<br/>
    <b>Soma GSC:</b> 48 pontos<br/>
    <b>VAF Calculado:</b> 0.65 + (0.01 × 48) = 1.13
    """
    story.append(Paragraph(vaf_text, styles['CustomBody']))
    story.append(Spacer(1, 0.5*cm))

    # Final AFP
    story.append(Paragraph("4.3 Pontos de Função Ajustados", styles['CustomHeading2']))
    afp_text = """
    <b>AFP = UFP × VAF</b><br/>
    <b>AFP = 199 × 1.13</b><br/>
    <b>AFP = 225 pontos</b>
    """
    story.append(Paragraph(afp_text, styles['CustomBody']))
    story.append(PageBreak())


def generate_budget(story, styles, content):
    """Generate budget and ROI section"""
    story.append(Paragraph("7. Orçamento e Análise de ROI", styles['CustomHeading1']))
    story.append(Spacer(1, 0.5*cm))

    # Budget Breakdown
    story.append(Paragraph("7.1 Breakdown de Custos", styles['CustomHeading2']))

    budget_data = [
        ["Categoria", "Cálculo", "Valor (R$)"],
        ["Desenvolvimento", "225 AFP × R$ 750/FP", "168.750,00"],
        ["Infraestrutura Azure", "App Service + SQL + Monitoring", "15.500,00"],
        ["Treinamento", "MIGRAI + .NET 9 para equipe", "5.000,00"],
        ["Licenças & Ferramentas", "Visual Studio + Azure DevOps", "4.500,00"],
        ["<b>Subtotal</b>", "", "<b>193.750,00</b>"],
        ["Contingência (15%)", "15% do subtotal", "29.062,50"],
        ["<b>INVESTIMENTO TOTAL</b>", "", "<b>222.812,50</b>"]
    ]

    budget_table = Table(budget_data, colWidths=[6*cm, 6*cm, 4*cm])
    budget_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#0066CC')),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (1, -1), 'LEFT'),
        ('ALIGN', (2, 0), (2, -1), 'RIGHT'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('FONTSIZE', (0, 0), (-1, 0), 11),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -3), colors.beige),
        ('BACKGROUND', (0, -2), (-1, -2), colors.lightgrey),
        ('BACKGROUND', (0, -1), (-1, -1), colors.HexColor('#00A859')),
        ('TEXTCOLOR', (0, -1), (-1, -1), colors.whitesmoke),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
        ('FONTNAME', (0, 1), (-1, -1), 'Helvetica'),
        ('FONTSIZE', (0, 1), (-1, -1), 10),
    ]))
    story.append(budget_table)
    story.append(Spacer(1, 0.5*cm))

    # ROI Projection
    story.append(Paragraph("7.2 Projeção de ROI", styles['CustomHeading2']))
    roi_text = """
    <b>Economia Anual Projetada:</b><br/>
    • Redução de custos de mainframe: R$ 30.000/ano<br/>
    • Ganho de produtividade (20%): R$ 40.000/ano<br/>
    • Redução de custos de treinamento: R$ 15.000/ano<br/>
    • Proteção de receita (99.9% SLA): R$ 10.000/ano<br/>
    <b>Total Economia Anual: R$ 95.000/ano</b><br/><br/>

    <b>Período de Payback:</b> R$ 222.812,50 ÷ R$ 95.000 = <b>2.3 anos</b><br/>
    <b>Valor Líquido em 5 anos:</b> (R$ 95.000 × 5) - R$ 222.812,50 = <b>R$ 252.187,50</b>
    """
    story.append(Paragraph(roi_text, styles['CustomBody']))
    story.append(PageBreak())


def generate_timeline(story, styles, content):
    """Generate timeline section"""
    story.append(Paragraph("5. Linha do Tempo do Projeto", styles['CustomHeading1']))
    story.append(Spacer(1, 0.5*cm))

    timeline_text = """
    O projeto está estruturado em 12 semanas divididas em 6 fases de desenvolvimento
    (8 semanas) e 1 fase de homologação (4 semanas), com 8 milestones principais.
    """
    story.append(Paragraph(timeline_text, styles['CustomBody']))
    story.append(Spacer(1, 0.3*cm))

    # Phase Breakdown
    phases_data = [
        ["Fase", "Semanas", "Duração", "Deliverables"],
        ["Fase 0: Research", "Semana 1", "5 dias", "Decisões de arquitetura, research.md"],
        ["Fase 1: Foundation", "Semanas 2-3", "10 dias", "Scaffolding, DbContext, repositories"],
        ["Fase 2: Core Logic", "Semanas 4-5", "10 dias", "Services, 42 business rules, testes"],
        ["Fase 3: API Layer", "Semana 6", "5 dias", "Controllers REST/SOAP, external clients"],
        ["Fase 4: Frontend", "Semana 7", "5 dias", "React components, Site.css integration"],
        ["Fase 5: Testing", "Semana 8", "5 dias", "E2E, parity tests, performance"],
        ["Fase 6: Homologação", "Semanas 9-12", "20 dias", "UAT, parallel operation, go-live"]
    ]

    phases_table = Table(phases_data, colWidths=[4*cm, 3*cm, 2.5*cm, 6.5*cm])
    phases_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#0066CC')),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'LEFT'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('FONTSIZE', (0, 0), (-1, 0), 10),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
        ('FONTNAME', (0, 1), (-1, -1), 'Helvetica'),
        ('FONTSIZE', (0, 1), (-1, -1), 9),
        ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
    ]))
    story.append(phases_table)
    story.append(PageBreak())


def generate_migrai_methodology(story, styles, content):
    """Generate MIGRAI methodology section"""
    story.append(Paragraph("6. Metodologia MIGRAI", styles['CustomHeading1']))
    story.append(Spacer(1, 0.5*cm))

    intro_text = """
    MIGRAI é um framework proprietário para modernização de sistemas legados assistida por
    Inteligência Artificial, estruturado em seis princípios fundamentais.
    """
    story.append(Paragraph(intro_text, styles['CustomBody']))
    story.append(Spacer(1, 0.3*cm))

    # Six Principles
    principles = [
        ("Modernization", "Migração para stack tecnológico moderno (.NET 9, React 19, Azure cloud) mantendo 100% da lógica de negócio."),
        ("Intelligence", "Uso de LLMs (Claude 3.5 Sonnet) para geração automática de código ESQL → C# com 95%+ de acurácia."),
        ("Gradual Migration", "Rollout faseado por user story (P1→P6), feature toggles, operação paralela mínima de 2 semanas."),
        ("Resilience", "Políticas Polly com retry exponencial, circuit breakers, fallback mechanisms, tratamento robusto de exceções."),
        ("Automation", "Pipeline CI/CD GitHub Actions, testes automatizados (unit/integration/E2E), deployment automatizado."),
        ("Integration", "Manutenção de contratos SOAP legados, novos endpoints REST, zero mudanças no schema de banco de dados.")
    ]

    for i, (name, desc) in enumerate(principles, 1):
        story.append(Paragraph(f"6.{i} {name}", styles['CustomHeading3']))
        story.append(Paragraph(desc, styles['CustomBody']))
        story.append(Spacer(1, 0.2*cm))

    story.append(PageBreak())


def generate_appendices(story, styles, content):
    """Generate appendices section"""
    story.append(Paragraph("10. Apêndices", styles['CustomHeading1']))
    story.append(Spacer(1, 0.5*cm))

    # Glossary
    story.append(Paragraph("Apêndice A: Glossário de Termos Técnicos", styles['CustomHeading2']))

    glossary_data = [
        ["Termo", "Definição"],
        ["AFP", "Adjusted Function Points - Pontos de Função Ajustados"],
        ["CICS", "Customer Information Control System - Plataforma IBM mainframe"],
        ["Clean Architecture", "Padrão arquitetural com separação em camadas (API, Core, Infrastructure)"],
        ["ESQL", "Extended SQL - Linguagem para stored procedures IBM"],
        ["IFPUG", "International Function Point Users Group - Metodologia de contagem de PF"],
        ["MIGRAI", "Framework para modernização de sistemas legados com IA"],
        ["SIWEA", "Sistema de Autorização de Pagamento de Indenizações de Sinistros"],
        ["UFP", "Unadjusted Function Points - Pontos de Função Não Ajustados"],
        ["VAF", "Value Adjustment Factor - Fator de Ajuste de Valor"]
    ]

    glossary_table = Table(glossary_data, colWidths=[4*cm, 12*cm])
    glossary_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#0066CC')),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'LEFT'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('FONTSIZE', (0, 0), (-1, 0), 10),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
        ('FONTNAME', (0, 1), (-1, -1), 'Helvetica'),
        ('FONTSIZE', (0, 1), (-1, -1), 9),
        ('VALIGN', (0, 0), (-1, -1), 'TOP'),
    ]))
    story.append(glossary_table)
    story.append(Spacer(1, 0.5*cm))

    # Version History
    story.append(Paragraph("Apêndice D: Histórico de Versões", styles['CustomHeading2']))

    version_data = [
        ["Versão", "Data", "Autor", "Alterações"],
        ["1.0", datetime.now().strftime("%d/%m/%Y"), "Equipe MIGRAI", "Versão inicial do documento"],
    ]

    version_table = Table(version_data, colWidths=[2*cm, 3*cm, 4*cm, 7*cm])
    version_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#0066CC')),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'LEFT'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('FONTSIZE', (0, 0), (-1, 0), 10),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
        ('FONTNAME', (0, 1), (-1, -1), 'Helvetica'),
        ('FONTSIZE', (0, 1), (-1, -1), 9),
    ]))
    story.append(version_table)


def generate_pdf(output_path):
    """Generate the complete PDF document"""
    print("🚀 Iniciando geração do PDF com ReportLab...")

    # Create output directory if needed
    output_dir = Path(output_path).parent
    output_dir.mkdir(parents=True, exist_ok=True)

    # Load content
    content = load_content()
    print(f"✓ Conteúdo carregado: AFP={content.get('afp_total')}, Investimento={content.get('total_investment')}")

    # Create PDF document
    doc = SimpleDocTemplate(
        str(output_path),
        pagesize=A4,
        rightMargin=2*cm,
        leftMargin=2*cm,
        topMargin=3*cm,
        bottomMargin=2*cm,
        title="Visual Age to .NET Migration - Análise & Planejamento",
        author="Caixa Seguradora Migration Team"
    )

    # Build story
    story = []
    styles = create_styles()

    # Generate sections
    print("✓ Gerando capa...")
    generate_cover_page(story, styles, content)

    print("✓ Gerando sumário executivo...")
    generate_executive_summary(story, styles, content)

    print("✓ Gerando análise de pontos de função...")
    generate_function_points(story, styles, content)

    print("✓ Gerando timeline...")
    generate_timeline(story, styles, content)

    print("✓ Gerando metodologia MIGRAI...")
    generate_migrai_methodology(story, styles, content)

    print("✓ Gerando orçamento e ROI...")
    generate_budget(story, styles, content)

    print("✓ Gerando apêndices...")
    generate_appendices(story, styles, content)

    # Build PDF
    print("📄 Compilando PDF...")
    doc.build(story, onFirstPage=add_header_footer, onLaterPages=add_header_footer)

    print(f"✅ PDF gerado com sucesso: {output_path}")
    print(f"📊 Tamanho do arquivo: {Path(output_path).stat().st_size / 1024:.1f} KB")

    return output_path


if __name__ == "__main__":
    # Output path
    base_dir = Path(__file__).parent.parent.parent
    output_pdf = base_dir / "output" / "migration-analysis-plan.pdf"

    try:
        pdf_path = generate_pdf(output_pdf)
        print(f"\n✅ SUCESSO! PDF gerado em:\n   {pdf_path}\n")
        print("Para visualizar:")
        print(f"   open {pdf_path}")
    except Exception as e:
        print(f"\n❌ ERRO ao gerar PDF: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)
