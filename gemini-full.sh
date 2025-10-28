#!/bin/bash

# Gemini CLI - Execução direta em YOLO mode

echo "🚀 Iniciando Gemini CLI em YOLO mode..."

if [ $# -gt 0 ]; then
    gemini --approval-mode yolo "$@"
else
    gemini --approval-mode yolo
fi