#!/usr/bin/env bash
# =============================================================================
#  uOrgHub Main — Deploy Script
#  Runs:  bash scripts/deploy-uorghub.sh
#  From:  /opt/uorghub
# =============================================================================
set -euo pipefail

DEPLOY_DIR="/opt/uorghub"
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.prod.yml"
ENV_FILE="$DEPLOY_DIR/.env"

GREEN='\033[0;32m'; CYAN='\033[0;36m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()  { echo -e "${GREEN}[✔]${NC} $*"; }
info() { echo -e "${CYAN}[→]${NC} $*"; }
warn() { echo -e "${YELLOW}[!]${NC} $*"; }
die()  { echo -e "${RED}[✘]${NC} $*" >&2; exit 1; }

[[ $EUID -eq 0 ]] || die "Run as root."
[[ -d "$DEPLOY_DIR/.git" ]] || die "$DEPLOY_DIR is not a git repo."

echo ""
info "uOrgHub Main — Deploying..."

cd "$DEPLOY_DIR"
git fetch origin
git reset --hard origin/master
log "Code updated."

docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" build
log "Images built."

docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --no-deps uorghub-api
sleep 3
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --no-deps uorghub-web
log "Containers restarted."

nginx -t && nginx -s reload
log "Nginx reloaded."

echo ""
log "Deploy complete: https://uorghub.hekzor.com"
docker compose -f "$COMPOSE_FILE" ps
