#!/usr/bin/env bash
# =============================================================================
#  uOrgHub — Unified Deploy Script
#
#  One script for every instance. The instance name selects an env file under
#  deploy/instances/, which is the single source of truth for that instance.
#
#  Usage:
#    sudo ./deploy/deploy.sh <instance> [options]
#
#  Examples:
#    sudo ./deploy/deploy.sh cfelbd-dev            # pull master, rebuild, restart
#    sudo ./deploy/deploy.sh cfelbd-live
#    sudo ./deploy/deploy.sh uorghub --no-pull     # deploy current checkout as-is
#    sudo ./deploy/deploy.sh cfelbd-dev --dry-run  # print what it would do
#
#  Options:
#    --no-pull    Skip git fetch/reset; deploy the current working tree.
#    --no-nginx   Skip the nginx test+reload at the end.
#    --dry-run    Print the commands instead of running them.
# =============================================================================
set -euo pipefail

# ── Locate the repo root from this script's own location ─────────────────────
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/docker-compose.yml"
INSTANCE_DIR="$SCRIPT_DIR/instances"

# ── Colours / logging ────────────────────────────────────────────────────────
GREEN='\033[0;32m'; CYAN='\033[0;36m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()  { echo -e "${GREEN}[✔]${NC} $*"; }
info() { echo -e "${CYAN}[→]${NC} $*"; }
warn() { echo -e "${YELLOW}[!]${NC} $*"; }
die()  { echo -e "${RED}[✘]${NC} $*" >&2; exit 1; }

# ── Parse args ───────────────────────────────────────────────────────────────
INSTANCE="${1:-}"
[[ -n "$INSTANCE" && "$INSTANCE" != --* ]] || {
  echo "Usage: sudo ./deploy/deploy.sh <instance> [--no-pull] [--no-nginx] [--dry-run]"
  echo ""
  echo "Available instances:"
  for f in "$INSTANCE_DIR"/*.env; do
    [[ -e "$f" ]] || continue
    echo "  - $(basename "${f%.env}")"
  done
  echo ""
  echo "Templates (copy to <name>.env and fill in secrets):"
  for f in "$INSTANCE_DIR"/*.env.example; do
    [[ -e "$f" ]] || continue
    echo "  - $(basename "$f")"
  done
  exit 1
}
shift || true

DO_PULL=1; DO_NGINX=1; DRY_RUN=0
for arg in "$@"; do
  case "$arg" in
    --no-pull)  DO_PULL=0 ;;
    --no-nginx) DO_NGINX=0 ;;
    --dry-run)  DRY_RUN=1 ;;
    *) die "Unknown option: $arg" ;;
  esac
done

ENV_FILE="$INSTANCE_DIR/$INSTANCE.env"

# ── Guards ───────────────────────────────────────────────────────────────────
if [[ $DRY_RUN -eq 0 ]]; then
  [[ $EUID -eq 0 ]] || die "Run as root: sudo ./deploy/deploy.sh $INSTANCE"
fi
[[ -f "$ENV_FILE" ]] || die "No env file for '$INSTANCE'. Expected: $ENV_FILE
Copy the template and fill in secrets:
  cp $INSTANCE_DIR/$INSTANCE.env.example $ENV_FILE
  nano $ENV_FILE"
[[ -f "$COMPOSE_FILE" ]] || die "Missing compose file: $COMPOSE_FILE"

# GIT_BRANCH is read from the env file (defaults to master if unset there).
GIT_BRANCH="$(grep -E '^GIT_BRANCH=' "$ENV_FILE" | tail -1 | cut -d= -f2- | tr -d '"' || true)"
GIT_BRANCH="${GIT_BRANCH:-master}"

run() {
  if [[ $DRY_RUN -eq 1 ]]; then
    echo -e "${YELLOW}[dry-run]${NC} $*"
  else
    eval "$@"
  fi
}

echo ""
info "Deploying instance '${INSTANCE}' from ${REPO_ROOT}"
info "  env file : $ENV_FILE"
info "  branch   : $GIT_BRANCH  (pull=$DO_PULL)"
echo ""

# ── 1. Sync code ─────────────────────────────────────────────────────────────
if [[ $DO_PULL -eq 1 ]]; then
  [[ -d "$REPO_ROOT/.git" ]] || die "$REPO_ROOT is not a git checkout; use --no-pull."
  run "git -C '$REPO_ROOT' fetch origin"
  run "git -C '$REPO_ROOT' reset --hard 'origin/$GIT_BRANCH'"
  log "Code synced to origin/$GIT_BRANCH."
else
  warn "Skipping git pull; deploying current working tree."
fi

# ── 2. Build this instance's images (unique tags — never shared) ─────────────
run "docker compose -f '$COMPOSE_FILE' --env-file '$ENV_FILE' build"
log "Images built."

# ── 3. Rolling restart: api first, then web ──────────────────────────────────
run "docker compose -f '$COMPOSE_FILE' --env-file '$ENV_FILE' up -d --no-deps db"
run "docker compose -f '$COMPOSE_FILE' --env-file '$ENV_FILE' up -d --no-deps api"
run "sleep 3"
run "docker compose -f '$COMPOSE_FILE' --env-file '$ENV_FILE' up -d --no-deps web"
log "Containers restarted."

# ── 4. Reload nginx ──────────────────────────────────────────────────────────
if [[ $DO_NGINX -eq 1 ]]; then
  run "nginx -t && nginx -s reload"
  log "Nginx reloaded."
fi

# ── Done ─────────────────────────────────────────────────────────────────────
DOMAIN="$(grep -E '^DOMAIN=' "$ENV_FILE" | tail -1 | cut -d= -f2- | tr -d '"' || true)"
echo ""
log "Deploy complete: ${DOMAIN:-$INSTANCE}"
[[ $DRY_RUN -eq 1 ]] || docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" ps
