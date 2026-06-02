#!/usr/bin/env bash
# =============================================================================
#  uOrgHub — Update (Re-Deploy) Script
#  Pulls latest code from git, rebuilds images, and restarts containers.
#
#  Usage  : bash scripts/update.sh
#  Run as : root  (from /opt/uorghub)
# =============================================================================
set -euo pipefail

DEPLOY_DIR="/opt/uorghub"   # cloned from https://github.com/golamhabibpalash/uOrgHub.git
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.prod.yml"
ENV_FILE="$DEPLOY_DIR/.env"

GREEN='\033[0;32m'; CYAN='\033[0;36m'; NC='\033[0m'
log()  { echo -e "${GREEN}[✔]${NC} $*"; }
info() { echo -e "${CYAN}[→]${NC} $*"; }

[[ $EUID -eq 0 ]] || { echo "Run as root." >&2; exit 1; }
[[ -d "$DEPLOY_DIR/.git" ]] || { echo "$DEPLOY_DIR is not a git repo." >&2; exit 1; }

echo ""
info "Pulling latest code..."
git -C "$DEPLOY_DIR" fetch origin
git -C "$DEPLOY_DIR" reset --hard origin/master
log "Code updated."

info "Rebuilding and restarting containers (zero-downtime rolling)..."
cd "$DEPLOY_DIR"

# Rebuild images without taking down the running containers first
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" build

# Restart services one by one to minimise downtime
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --no-deps uorghub-api
sleep 5
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --no-deps uorghub-web

log "Deployment complete."
echo ""
info "Running containers:"
docker compose -f "$COMPOSE_FILE" ps
