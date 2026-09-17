#!/usr/bin/env bash
# =============================================================================
#  uOrgHub — One-command update for the uorghub instance (legacy /opt/uorghub
#  docker-compose.prod.yml stack).
#
#  Usage:
#    ./deploy/update-uorghub.sh            # pull master, rebuild changed images, rolling restart
#
#  This is the fast path for the live instance at https://uorghub.hekzor.com.
#  It replaces the old multi-step ritual (git reset + build + up -d) with a
#  single `docker compose up -d --build`, which rebuilds only what changed and
#  rolling-restarts the containers in place — data volumes are untouched.
# =============================================================================
set -euo pipefail

REMOTE="${REMOTE:-umf-vps}"
DIR="${DIR:-/opt/uorghub}"
DOMAIN="${DOMAIN:-https://uorghub.hekzor.com}"

GREEN='\033[0;32m'; CYAN='\033[0;36m'; RED='\033[0;31m'; NC='\033[0m'
log()  { echo -e "${GREEN}[✔]${NC} $*"; }
info() { echo -e "${CYAN}[→]${NC} $*"; }
die()  { echo -e "${RED}[✘]${NC} $*" >&2; exit 1; }

info "Updating uorghub on ${REMOTE}:${DIR}"
ssh -t "$REMOTE" "
  set -euo pipefail
  cd '$DIR' &&
  git fetch origin &&
  git reset --hard origin/master &&
  docker compose -f docker-compose.prod.yml --env-file .env up -d --build &&
  sleep 20 &&
  docker ps --format 'table {{.Names}}\t{{.Status}}' | grep uorghub &&
  echo '--- health check ---' &&
  curl -s -o /dev/null -w '$DOMAIN -> %{http_code}\n' '$DOMAIN'/
"

log "Deploy complete. Verify at $DOMAIN"