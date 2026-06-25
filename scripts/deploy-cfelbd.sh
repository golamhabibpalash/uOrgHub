#!/bin/bash
set -e

PLATFORM_DIR="/opt/platform"
SOURCE_DIR="/tmp/orghub-source"
NGINX_DIR="$PLATFORM_DIR/nginx"
LOGS_DIR="$PLATFORM_DIR/logs"

deploy_instance() {
  local INSTANCE=$1
  local DOMAIN=$2
  local API_PORT=$3
  local ENV_NAME=$4

  local DOCKER_DIR="$PLATFORM_DIR/docker/apps/orghub/$INSTANCE"

  echo ""
  echo "--- Deploying $INSTANCE ($DOMAIN) ---"

  mkdir -p "$DOCKER_DIR"
  mkdir -p "$NGINX_DIR"
  mkdir -p "$LOGS_DIR/nginx"
  mkdir -p "$LOGS_DIR/apps/orghub/$INSTANCE"

  cp "$SOURCE_DIR/docker/apps/orghub/$INSTANCE/docker-compose.yml" "$DOCKER_DIR/"
  cp "$SOURCE_DIR/docker/apps/orghub/$INSTANCE/.env" "$DOCKER_DIR/"
  cp "$SOURCE_DIR/docker/apps/orghub/$INSTANCE/appsettings.$ENV_NAME.json" "$DOCKER_DIR/"
  cp "$SOURCE_DIR/nginx/$DOMAIN.conf" "$NGINX_DIR/"

  cd "$DOCKER_DIR"
  set -a
  source .env
  set +a

  docker compose down --remove-orphans 2>/dev/null || true
  docker compose up -d

  echo "$INSTANCE deployed on port $API_PORT"
}

# ============================================
# Step 1: Build shared image ONCE
# ============================================
echo "============================================"
echo "Building shared image: orghub-api:latest"
echo "============================================"
docker build \
  -f "$SOURCE_DIR/docker/apps/orghub/Dockerfile" \
  -t "orghub-api:latest" \
  "$SOURCE_DIR"

# ============================================
# Step 2: Deploy each instance using the shared image
# ============================================
echo ""
echo "============================================"
echo "Deploying instances"
echo "============================================"

deploy_instance \
  "orghub_cfl_live" \
  "cfelbd.hekzor.com" \
  "5178" \
  "CFLLive"

deploy_instance \
  "orghub_cfl_dev" \
  "cfelbd-dev.hekzor.com" \
  "5180" \
  "CFLDev"

# ============================================
# Step 3: Reload Nginx
# ============================================
echo ""
echo "--- Reloading Nginx ---"
if docker ps --format '{{.Names}}' | grep -q "^nginx$"; then
  docker exec nginx nginx -t && docker exec nginx nginx -s reload
elif command -v nginx &> /dev/null; then
  nginx -t && nginx -s reload
else
  echo "WARNING: Could not reload nginx. Reload manually."
fi

echo ""
echo "============================================"
echo "Deployment complete!"
echo ""
echo "Shared image: orghub-api:latest"
echo ""
echo "Live: https://cfelbd.hekzor.com        (API :5178)"
echo "Dev:  https://cfelbd-dev.hekzor.com    (API :5180)"
echo "============================================"
