#!/usr/bin/env bash
# =============================================================================
#  CFL Dev — Deploy Script
#  Runs:  bash scripts/deploy-cfelbd-dev.sh
#  From:  /tmp/orghub-source
# =============================================================================
set -euo pipefail

SRC_DIR="/tmp/orghub-source"
COMPOSE_FILE="$SRC_DIR/docker-compose.cfelbd-dev.yml"
ENV_FILE="$SRC_DIR/.env.cfelbd-dev"

GREEN='\033[0;32m'; CYAN='\033[0;36m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()  { echo -e "${GREEN}[✔]${NC} $*"; }
info() { echo -e "${CYAN}[→]${NC} $*"; }
die()  { echo -e "${RED}[✘]${NC} $*" >&2; exit 1; }

[[ $EUID -eq 0 ]] || die "Run as root."
[[ -d "$SRC_DIR/.git" ]] || die "$SRC_DIR is not a git repo."

echo ""
info "CFL Dev — Deploying..."

cd "$SRC_DIR"
git fetch origin
git reset --hard origin/master
log "Code updated."

docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" build
log "Images built."

docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --no-deps orghub-cfl-dev-api
sleep 3
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --no-deps orghub-cfl-dev-web
log "Containers restarted."

nginx -t && nginx -s reload
log "Nginx reloaded."

echo ""
log "Deploy complete: https://cfelbd-dev.hekzor.com"
docker compose -f "$COMPOSE_FILE" ps
