#!/bin/bash

# Claude Code - Execução Direta com Bypass de Permissões
# Executa o Claude diretamente sem usar venv

# Cores para output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║     🚀 CLAUDE - Execução com Bypass de Permissões          ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Executa Claude com bypass de permissões
echo -e "${GREEN}🚀 Iniciando Claude com bypass de permissões...${NC}"
echo ""

# Se houver argumentos, passa para o Claude
if [ $# -gt 0 ]; then
    claude --dangerously-skip-permissions "$@"
else
    claude --dangerously-skip-permissions
fi