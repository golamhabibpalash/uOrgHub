# uOrgHub Instance Deployment Info

> **Deploying is now one command** — see [`deploy/README.md`](deploy/README.md):
> ```bash
> sudo ./deploy/deploy.sh <instance>     # cfelbd-live | cfelbd-dev | uorghub
> ```
> Everything that differs between instances lives in
> `deploy/instances/<instance>.env` (the real files hold secrets and are
> gitignored). This page is now just a **reference** for hosts, ports, and
> server-side paths.

## Server
- **Host**: vmi3304620 (62.84.181.246)
- **OS**: Ubuntu

## Repo
- **URL**: https://github.com/golamhabibpalash/uOrgHub.git
- **Branch**: `master`

## Secrets — action required

Passwords and JWT keys used to be listed in this file in plain text. They have
been moved to the per-instance env files and redacted here. **Because the old
values are in this repo's git history, rotate them:**

| Instance | Rotate |
|---|---|
| CFL Dev | `orghub_cfl_dev` DB password, `uOrgHubCFLDev` JWT secret |
| CFL Live | `orghub_cfl_live` DB password, `uOrgHubCFL` JWT secret |
| uOrgHub Main | `orgHub` DB password, `uOrgHub` JWT secret |

Rotating a JWT secret logs everyone out (existing tokens stop validating), which
is the point. To rotate a DB password: change it in the running Postgres
container, then update `DB_PASSWORD` in that instance's `.env` and redeploy.

---

## Port map

| Service | uOrgHub Main | CFL Live | CFL Dev |
|---|---|---|---|
| **Web** | 3001 | 5179 | 5181 |
| **API** | 5177 | 5178 | 5180 |
| **DB** | 5436 | 5437 | 5438 |

All bound to `127.0.0.1`; nginx terminates SSL and proxies each public domain to
its `Web` port.

## Instance reference

### CFL Dev — `https://cfelbd-dev.hekzor.com`

| Item | Value |
|---|---|
| **Env file** | `deploy/instances/cfelbd-dev.env` |
| **DB name** | `orghub_cfl_dev` |
| **DB password** | *(in env file — ROTATE)* |
| **JWT secret** | *(in env file — ROTATE)* |
| **JWT issuer / audience** | `uOrgHubCFLDev` / `uOrgHubCFLDev-clients` |
| **Containers** | `orghub-cfl-dev-api`, `orghub-cfl-dev-web`, `postgres-cfelbd-dev` |
| **Images** | `orghub-api:cfelbd-dev`, `orghub-web:cfelbd-dev` |
| **Nginx site** | `/etc/nginx/sites-enabled/cfelbd-dev` |
| **SSL cert** | `/etc/letsencrypt/live/cfelbd-dev.hekzor.com/` |

### CFL Live — `https://cfelbd.hekzor.com`

| Item | Value |
|---|---|
| **Env file** | `deploy/instances/cfelbd-live.env` |
| **DB name** | `orghub_cfl_live` |
| **DB password** | *(in env file — ROTATE)* |
| **JWT secret** | *(in env file — ROTATE)* |
| **JWT issuer / audience** | `uOrgHubCFL` / `uOrgHubCFL-clients` |
| **Containers** | `orghub-cfl-api`, `orghub-cfl-web`, `postgres-cfelbd` |
| **Images** | `orghub-api:cfelbd-live`, `orghub-web:cfelbd-live` |
| **Nginx site** | `/etc/nginx/sites-enabled/cfelbd` |
| **SSL cert** | `/etc/letsencrypt/live/cfelbd.hekzor.com/` |

### uOrgHub Main — `https://uorghub.hekzor.com` / `https://uorghub.unitymicrofund.com`

| Item | Value |
|---|---|
| **Env file** | `deploy/instances/uorghub.env` |
| **DB name** | `orgHub` |
| **DB password** | *(in env file — ROTATE)* |
| **JWT secret** | *(in env file — ROTATE)* |
| **JWT issuer / audience** | `uOrgHub` / `uOrgHub-clients` |
| **Containers** | `uorghub-api`, `uorghub-web`, `uorghub-db` |
| **Deploy path** | `/opt/uorghub` |

> The main instance still runs via `docker-compose.prod.yml`. Migrating it to the
> unified flow (`deploy/instances/uorghub.env`) is optional and can be done last.

---

## Useful commands

```bash
# Deploy (pull, build, restart, reload nginx)
sudo ./deploy/deploy.sh cfelbd-dev
sudo ./deploy/deploy.sh cfelbd-live
sudo ./deploy/deploy.sh uorghub

# Preview without doing anything
./deploy/deploy.sh cfelbd-live --dry-run

# Logs (container names from the tables above)
docker logs orghub-cfl-api --tail 50

# Containers / nginx / SSL
docker ps
nginx -t && nginx -s reload
certbot renew
```
