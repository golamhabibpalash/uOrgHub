# uOrgHub CFL Instance Deployment Info

## Server
- **Host**: vmi3304620 (62.84.181.246)
- **OS**: Ubuntu

## Repo
- **URL**: https://github.com/golamhabibpalash/uOrgHub.git
- **Branch**: `master`
- **Clone path**: `/tmp/orghub-source`

---

## CFL Dev Instance

| Item | Value |
|---|---|
| **Domain** | `https://cfelbd-dev.hekzor.com` |
| **API internal** | `http://127.0.0.1:5180` |
| **Web internal** | `http://127.0.0.1:5181` |
| **DB internal** | `127.0.0.1:5438` → `postgres-cfelbd-dev:5432` |
| **DB name** | `orghub_cfl_dev` |
| **DB user** | `postgres` |
| **DB password** | `DevStr0ng!Passw0rd` |
| **JWT secret** | `uOrgHubCFLDevSecretKey2025AtLeast32Chars!` |
| **JWT issuer** | `uOrgHubCFLDev` |
| **JWT audience** | `uOrgHubCFLDev-clients` |
| **API container** | `orghub-cfl-dev-api` |
| **Web container** | `orghub-cfl-dev-web` |
| **DB container** | `postgres-cfelbd-dev` |
| **API image** | `orghub-api:latest` |
| **Web image** | `orghub-web:latest` |
| **VITE_API_URL** | `https://cfelbd-dev.hekzor.com/api/v1` |
| **AllowedOrigins** | `https://cfelbd-dev.hekzor.com` |
| **Nginx site** | `/etc/nginx/sites-enabled/cfelbd-dev` |
| **SSL cert** | `/etc/letsencrypt/live/cfelbd-dev.hekzor.com/` |
| **Docker compose** | `/tmp/orghub-source/docker-compose.cfelbd-dev.yml` |
| **Env file** | `/tmp/orghub-source/.env.cfelbd-dev` |

### Commands

```bash
# Start/restart
cd /tmp/orghub-source
docker compose -f docker-compose.cfelbd-dev.yml --env-file .env.cfelbd-dev up -d

# Stop
docker compose -f docker-compose.cfelbd-dev.yml down

# View logs
docker logs orghub-cfl-dev-api --tail 50
docker logs orghub-cfl-dev-web --tail 50

# Rebuild API image
docker build -f Dockerfile.api -t orghub-api:latest .

# Rebuild web image
docker build -f Dockerfile.web --build-arg VITE_API_URL=https://cfelbd-dev.hekzor.com/api/v1 -t orghub-web:latest .
```

---

## CFL Live Instance

| Item | Value |
|---|---|
| **Domain** | `https://cfelbd.hekzor.com` |
| **API internal** | `http://127.0.0.1:5178` |
| **Web internal** | `http://127.0.0.1:5179` |
| **DB internal** | `127.0.0.1:5437` → `postgres-cfelbd:5432` |
| **DB name** | `orghub_cfl_live` |
| **DB user** | `postgres` |
| **DB password** | `YourStrongLivePassword!` |
| **JWT secret** | `uOrgHubCFLLiveSecretKey2025AtLeast32Chars!` |
| **JWT issuer** | `uOrgHubCFL` |
| **JWT audience** | `uOrgHubCFL-clients` |
| **API container** | `orghub-cfl-api` |
| **Web container** | `orghub-cfl-web` |
| **DB container** | `postgres-cfelbd` |
| **API image** | `orghub-api:latest` |
| **Web image** | `orghub-web:latest` |
| **VITE_API_URL** | `https://cfelbd.hekzor.com/api/v1` |
| **AllowedOrigins** | `https://cfelbd.hekzor.com` |
| **Nginx site** | `/etc/nginx/sites-enabled/cfelbd` |
| **SSL cert** | `/etc/letsencrypt/live/cfelbd.hekzor.com/` |
| **Docker compose** | `/tmp/orghub-source/docker-compose.cfelbd-live.yml` |
| **Env file** | `/tmp/orghub-source/.env.cfelbd-live` |

### Commands

```bash
# Start/restart
cd /tmp/orghub-source
docker compose -f docker-compose.cfelbd-live.yml --env-file .env.cfelbd-live up -d

# Stop
docker compose -f docker-compose.cfelbd-live.yml down

# View logs
docker logs orghub-cfl-api --tail 50
docker logs orghub-cfl-web --tail 50

# Rebuild API image
docker build -f Dockerfile.api -t orghub-api:latest .

# Rebuild web image
docker build -f Dockerfile.web --build-arg VITE_API_URL=https://cfelbd.hekzor.com/api/v1 -t orghub-web:latest .
```

---

## Existing uOrgHub Instance (NOT modified)

| Item | Value |
|---|---|
| **Domain** | `https://uorghub.hekzor.com` / `https://uorghub.unitymicrofund.com` |
| **API internal** | `http://127.0.0.1:5177` |
| **Web internal** | `http://127.0.0.1:3001` |
| **DB internal** | `127.0.0.1:5436` |
| **Deploy path** | `/opt/uorghub` |

---

## Port Map

| Service | uOrgHub | CFL Live | CFL Dev |
|---|---|---|---|
| **DB** | 5436 | 5437 | 5438 |
| **API** | 5177 | 5178 | 5180 |
| **Web** | 3001 | 5179 | 5181 |

---

## Useful Commands

```bash
# View all running containers
docker ps

# Nginx checks
nginx -t
nginx -s reload

# SSL renewal (auto via cron, but can manually run)
certbot renew

# Update code from repo
cd /tmp/orghub-source
git pull origin master

# Rebuild all images and restart a specific instance
docker compose -f docker-compose.cfelbd-dev.yml --env-file .env.cfelbd-dev up -d --build
```
