#!/bin/bash

echo "🚀 Gerando PDF ULTRA COMPLETO com TODAS as regras de negócio..."
echo "   Lendo documentação completa do sistema legado..."

# Activate venv
source ../../venv/bin/activate

# Run Python generator
python3 pdf_generator_ultra_complete.py

# Open PDF
if [ $? -eq 0 ]; then
    open "../../output/migration-analysis-ULTRA-COMPLETE.pdf"
fi
