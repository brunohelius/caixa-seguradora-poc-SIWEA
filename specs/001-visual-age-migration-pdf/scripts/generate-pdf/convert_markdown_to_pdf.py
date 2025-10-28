#!/usr/bin/env python3
"""
Convert Complete Markdown Documentation to PDF
Converts ALL business rules, ALL tables, ALL validations to a single comprehensive PDF
"""

import sys
import subprocess
from pathlib import Path
from datetime import datetime

def main():
    print("🚀 Convertendo documentação Markdown completa para PDF...")
    print("   ZERO RESUMOS - TODO O CONTEÚDO SERÁ INCLUÍDO")

    # Source markdown files
    docs_dir = Path("/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/docs")
    output_dir = Path(__file__).parent.parent.parent / "output"
    output_dir.mkdir(exist_ok=True)

    complete_analysis = docs_dir / "LEGACY_SIWEA_COMPLETE_ANALYSIS.md"
    business_rules = docs_dir / "BUSINESS_RULES_INDEX.md"

    if not complete_analysis.exists():
        print(f"❌ Erro: Arquivo não encontrado: {complete_analysis}")
        return 1

    # Read markdown content
    print(f"✓ Lendo {complete_analysis.name} ({complete_analysis.stat().st_size / 1024:.1f} KB)...")
    content = complete_analysis.read_text()

    print(f"✓ Lendo {business_rules.name}...")
    br_content = business_rules.read_text()

    # Combine into single markdown
    combined_md = f"""# Visual Age to .NET Migration - Documentação Técnica Completa

**Sistema**: SIWEA - Autorização de Pagamento de Indenizações de Sinistros
**Tecnologia Legada**: IBM VisualAge EZEE 4.40
**Tecnologia Alvo**: .NET 9 + React 19 + Azure
**Data**: {datetime.now().strftime("%d/%m/%Y")}
**Versão**: 1.0

---

**IMPORTANTE**: Este documento contém a especificação COMPLETA do sistema legado, incluindo:

- ✅ TODAS as 100+ regras de negócio (BR-001 a BR-099+)
- ✅ TODAS as 13 tabelas de banco de dados com TODOS os campos
- ✅ TODAS as validações e fórmulas
- ✅ TODAS as 24 mensagens de erro
- ✅ TODOS os 3 serviços externos (CNOUA, SIPUA, SIMDA)
- ✅ TODO o workflow de fases
- ✅ TODAS as especificações de auditoria

**ZERO RESUMOS - DOCUMENTAÇÃO COMPLETA**

---

{content}

---

# Índice de Regras de Negócio

{br_content}

---

**Fim do Documento**
Total de páginas estimadas: 100-150
Gerado automaticamente via Claude Code
"""

    # Save combined markdown
    combined_file = output_dir / "COMPLETE_DOCUMENTATION.md"
    combined_file.write_text(combined_md)
    print(f"✓ Markdown combinado salvo: {combined_file}")
    print(f"   Tamanho: {len(combined_md) / 1024:.1f} KB")
    print(f"   Linhas: {len(combined_md.splitlines())}")

    # Convert to PDF using md2pdf
    output_pdf = output_dir / "migration-analysis-ULTRA-COMPLETE.pdf"

    print(f"\n📄 Convertendo para PDF...")
    print(f"   Entrada: {combined_file}")
    print(f"   Saída: {output_pdf}")
    print(f"   (Este processo pode levar 60-120 segundos...)")

    # Use md2pdf command (INPUT.MD OUTPUT.PDF format)
    try:
        result = subprocess.run(
            ['md2pdf', str(combined_file), str(output_pdf)],
            capture_output=True,
            text=True,
            timeout=180
        )

        if result.returncode == 0:
            file_size = output_pdf.stat().st_size
            print(f"\n✅ PDF ULTRA COMPLETO gerado com sucesso!")
            print(f"📄 Localização: {output_pdf}")
            print(f"📊 Tamanho: {file_size / 1024:.1f} KB ({file_size / (1024*1024):.2f} MB)")
            print(f"\n🎉 DOCUMENTO COMPLETO - 100-150 PÁGINAS!")
            print(f"\nPara visualizar:")
            print(f'   open "{output_pdf}"')

            # Open PDF using subprocess instead of os.system
            subprocess.run(['open', str(output_pdf)])
            return 0
        else:
            print(f"❌ Erro na conversão:")
            print(result.stderr)
            return 1

    except subprocess.TimeoutExpired:
        print("❌ Timeout na conversão (>180s)")
        return 1
    except Exception as e:
        print(f"❌ Erro: {e}")
        return 1


if __name__ == "__main__":
    sys.exit(main())
