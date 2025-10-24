#!/bin/bash

echo "🎯 Gerando PDF FINAL - Profissional, Bonito e COMPLETO"
echo "======================================================="
echo ""

cd "/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/specs/001-visual-age-migration-pdf"
source venv/bin/activate

echo "Etapa 1/2: Gerando PDF profissional formatado (21 páginas)..."
python3 scripts/generate-pdf/pdf_generator_complete.py
PDF1="output/migration-analysis-plan-COMPLETE.pdf"

echo ""
echo "Etapa 2/2: Gerando PDF completo do markdown (100+ páginas)..."
python3 scripts/generate-pdf/convert_markdown_to_pdf.py
PDF2="output/migration-analysis-ULTRA-COMPLETE.pdf"

echo ""
echo "✅ Dois PDFs gerados:"
echo "   📄 PDF Formatado: $PDF1 (21 páginas, profissional)"
echo "   📄 PDF Completo: $PDF2 (100+ páginas, todo conteúdo)"
echo ""
echo "💡 Recomendação: Use ambos em conjunto:"
echo "   - PDF Formatado para apresentação executiva"
echo "   - PDF Completo para referência técnica detalhada"
echo ""
echo "Abrindo ambos os PDFs..."
open "$PDF1"
sleep 2
open "$PDF2"
