#!/bin/bash

# Script para parar o sistema completo
# Caixa Seguradora - POC SIWEA

echo "=========================================="
echo "🛑 Encerrando Sistema Caixa Seguradora"
echo "=========================================="
echo ""

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

BASE_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Ler PIDs salvos
if [ -f "$BASE_DIR/.backend.pid" ]; then
    BACKEND_PID=$(cat "$BASE_DIR/.backend.pid")
    echo -e "${YELLOW}🔴 Encerrando backend (PID: $BACKEND_PID)...${NC}"
    kill $BACKEND_PID 2>/dev/null
    rm "$BASE_DIR/.backend.pid"
    echo -e "${GREEN}✅ Backend encerrado${NC}"
else
    echo -e "${YELLOW}⚠️  Backend PID não encontrado${NC}"
fi

if [ -f "$BASE_DIR/.frontend.pid" ]; then
    FRONTEND_PID=$(cat "$BASE_DIR/.frontend.pid")
    echo -e "${YELLOW}🔴 Encerrando frontend (PID: $FRONTEND_PID)...${NC}"
    kill $FRONTEND_PID 2>/dev/null
    rm "$BASE_DIR/.frontend.pid"
    echo -e "${GREEN}✅ Frontend encerrado${NC}"
else
    echo -e "${YELLOW}⚠️  Frontend PID não encontrado${NC}"
fi

# Garantir que as portas foram liberadas
echo ""
echo -e "${YELLOW}🧹 Limpando portas...${NC}"
lsof -ti:5000 | xargs kill -9 2>/dev/null
lsof -ti:5001 | xargs kill -9 2>/dev/null
lsof -ti:3000 | xargs kill -9 2>/dev/null

echo ""
echo -e "${GREEN}✅ Sistema encerrado com sucesso!${NC}"
echo ""
