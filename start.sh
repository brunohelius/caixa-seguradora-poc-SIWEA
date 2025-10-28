#!/bin/sh
set -e

echo "==================================="
echo "Starting Vite Preview Server"
echo "==================================="
echo "Environment:"
echo "  PORT: ${PORT:-5174}"
echo "  NODE_ENV: ${NODE_ENV:-production}"
echo "  PWD: $(pwd)"
echo "==================================="

# Check if dist folder exists
if [ ! -d "dist" ]; then
  echo "ERROR: dist folder not found!"
  echo "Contents of current directory:"
  ls -la
  exit 1
fi

echo "dist folder found. Starting preview server..."
echo "Command: npx vite preview --host 0.0.0.0 --port ${PORT:-5174}"
echo "==================================="

exec npx vite preview --host 0.0.0.0 --port ${PORT:-5174}
