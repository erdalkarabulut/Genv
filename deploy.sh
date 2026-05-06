#!/bin/bash
set -e

# === CryoFlow Deployment Script ===
# Kullanim: ./deploy.sh
# Mevcut 'smartready_oqlt-network' network'u ve PostgreSQL kurulu olmali.
# Bu betik sadece cryoflow-api ve cryoflow-frontend container'larini deploy eder.
# Mevcut diger container'lara dokunmaz.

echo "[1/5] Git pull..."
git pull origin main

echo "[2/5] Eski container'lari temizle..."
(docker stop cryoflow-api 2>/dev/null && docker rm cryoflow-api 2>/dev/null) || true
(docker stop cryoflow-frontend 2>/dev/null && docker rm cryoflow-frontend 2>/dev/null) || true

echo "[3/5] Backend build..."
docker build -t cryoflow-api -f Dockerfile.backend .

echo "[4/5] Frontend build..."
docker build -t cryoflow-frontend -f Dockerfile.frontend .

echo "[5/5] Container'lari baslat..."
docker compose -f docker-compose.prod.yml up -d

echo ""
echo "=== CryoFlow basariyla deploy edildi ==="
echo "API:       cryoflow-api (internal:5001)"
echo "Frontend:  cryoflow-frontend (internal:80)"
echo ""
echo "nginx-proxy-manager (ayni Docker network'unde):"
echo "  API:       http://cryoflow-api:5001"
echo "  Frontend:  http://cryoflow-frontend:80"
echo "  Not: NPM hedefi konteyner ICI PORT'tur (80). 6001 host map'idir;"
echo "       cryoflow-frontend:8001 veya :6001 YANLISTIR."
echo ""
echo "API file log (Serilog gunluk dosya adi YYYYMMDD.txt, log-*.txt degil):"
echo "  docker exec cryoflow-api ls -la /app/logs"
echo "  docker exec cryoflow-api tail -100 /app/logs/\$(date +%Y%m%d).txt"
echo ""
echo "frontend/nginx.conf degisti ama imaj eski kaldiysa (cache):"
echo "  docker build --no-cache -t cryoflow-frontend -f Dockerfile.frontend ."
