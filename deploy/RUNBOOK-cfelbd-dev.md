# Runbook — cfelbd-dev

Deploy / publish guide for **https://cfelbd-dev.hekzor.com**. Written from the
deploy that actually worked on 2026-07-25.

> **TL;DR — the routine deploy is two steps:**
> 1. Push your code to the branch (from your PC).
> 2. On the server: `cd /opt/orghub-source && sudo ./deploy/deploy.sh cfelbd-dev`
>
> That's the whole thing. "Publish" is not a separate step — the deploy rebuilds
> the images, restarts the containers, and reloads nginx, which is what makes the
> new version live at the public domain.

---

## Instance facts

| Item | Value |
|------|-------|
| Public URL | https://cfelbd-dev.hekzor.com |
| Server | `62.84.181.246` (hekzor), SSH as `root` |
| Checkout on server | `/opt/orghub-source` |
| Branch it deploys | `feat/project-financial-control` *(set `master` after the merge)* |
| Env file (server only, gitignored) | `deploy/instances/cfelbd-dev.env` |
| Containers | `orghub-cfl-dev-web`, `orghub-cfl-dev-api`, `postgres-cfelbd-dev` |
| Images | `orghub-web:cfelbd-dev`, `orghub-api:cfelbd-dev` |
| Ports (localhost) | web `5181`, api `5180`, db `5438` |
| Database | `orghub_cfl_dev` (user `postgres`) |
| Data volumes | `orghub-source_postgres-cfelbd-dev-data`, `orghub-source_uorghub-cfl-dev-uploads` |
| nginx site | `/etc/nginx/sites-enabled/cfelbd-dev` |
| SSL cert | `/etc/letsencrypt/live/cfelbd-dev.hekzor.com/` |

Secrets (DB password, JWT) live **only** in the server env file, never in git.
If you need to read the DB password:
`docker inspect postgres-cfelbd-dev --format '{{range .Config.Env}}{{println .}}{{end}}' | grep POSTGRES_PASSWORD`

---

## Routine deploy (every time)

### 1. Push your changes (from your local PC)

```bash
git add -A
git commit -m "your change"
git push
```

Whatever branch the server env's `GIT_BRANCH` points at is what deploys. Right
now that's `feat/project-financial-control`, so push there. (After you merge to
`master` and switch the env, you'll just push to `master`.)

### 2. (Recommended) Back up the dev DB first

```bash
# on the server
docker exec postgres-cfelbd-dev pg_dump -U postgres -Fc orghub_cfl_dev \
  > /root/cfelbd-dev-$(date +%F-%H%M).dump
```

Pull it to your PC if you want an off-server copy:
`scp 'root@62.84.181.246:/root/cfelbd-dev-*.dump' ~/Downloads/`

### 3. Deploy

```bash
# on the server, as root
cd /opt/orghub-source
./deploy/deploy.sh cfelbd-dev --dry-run     # optional: preview, runs nothing
sudo ./deploy/deploy.sh cfelbd-dev
```

This pulls the branch, rebuilds the two images, rolling-restarts db → api → web,
and reloads nginx. Takes a few minutes (the .NET + Vite build).

### 4. Verify

```bash
docker logs orghub-cfl-dev-api --tail 40          # migrations applied, no errors
curl -sI https://cfelbd-dev.hekzor.com | head -1  # expect: HTTP/2 200
```

Then open **https://cfelbd-dev.hekzor.com** and check your change is live and the
data looks right.

---

## Backup / restore (safety net)

Backup (custom format):
```bash
docker exec postgres-cfelbd-dev pg_dump -U postgres -Fc orghub_cfl_dev > backup.dump
```

Restore into the running dev DB (this overwrites — be sure):
```bash
cat backup.dump | docker exec -i postgres-cfelbd-dev \
  pg_restore -U postgres -d orghub_cfl_dev --clean --if-exists
```

---

## First-time / disaster recovery

You only need this if the server checkout is gone (e.g. it was in `/tmp`, which
gets wiped) or you're setting the instance up fresh. Routine deploys **don't**
need any of it.

```bash
# 1. Fresh checkout on a persistent path
cd /opt
git clone https://github.com/golamhabibpalash/uOrgHub.git orghub-source
cd /opt/orghub-source
git checkout feat/project-financial-control

# 2. Create the env file and fill the secrets
cp deploy/instances/cfelbd-dev.env.example deploy/instances/cfelbd-dev.env
nano deploy/instances/cfelbd-dev.env
#   DB_PASSWORD   = the dev DB's existing password (must match the data volume)
#   JWT_SECRET    = the dev JWT secret
#   GIT_BRANCH    = feat/project-financial-control   (until merged to master)
#   the DB_VOLUME / *_EXTERNAL lines already point at the existing data — leave them

# 3. If old containers exist under these names, remove them (KEEPS the volumes)
docker rm -f orghub-cfl-dev-api orghub-cfl-dev-web postgres-cfelbd-dev

# 4. Deploy
sudo ./deploy/deploy.sh cfelbd-dev
```

The `DB_VOLUME` / `DB_VOLUME_EXTERNAL=true` settings make the new stack reuse the
existing `orghub-source_postgres-cfelbd-dev-data` volume, so **your data carries
over** instead of starting blank.

---

## Troubleshooting (things that actually bit us)

| Symptom | Cause | Fix |
|---------|-------|-----|
| `not a git repository` in the checkout | It was under `/tmp` and got wiped | Re-clone to `/opt/orghub-source` (disaster-recovery section) |
| API logs show DB auth failure | `DB_PASSWORD` in the env ≠ the password the DB volume was created with | Read the real one with the `docker inspect ... POSTGRES_PASSWORD` command above and match it |
| `container name already in use` | Old container still exists under the same name | `docker rm -f <name>` (containers only — named volumes are safe) |
| Site shows an **empty** database after deploy | New stack created a fresh volume instead of reusing the old one | Ensure `DB_VOLUME` + `DB_VOLUME_EXTERNAL=true` are set in the env file |
| `WARN volume ... already exists but was created for project ...` | Adopting a volume from the old compose project | Harmless. `DB_VOLUME_EXTERNAL=true` silences it and protects the volume |
| nginx `ssl_stapling` / `protocol options redefined` warnings | Pre-existing config from the other sites on this box | Ignore — not from this instance |

**Golden rule:** removing a *container* is always safe; your data is in the
*volume*, not the container. Never run `docker compose down -v` on this instance —
that would try to delete volumes (the `external: true` guard blocks the data one,
but don't rely on it).

---

## Related docs

- `deploy/README.md` — the deploy system across all instances
- `.github/workflows/README.md` — deploying from the GitHub Actions button
- `CFL_DEPLOYMENT_INFO.md` — all instances, ports, and the secrets-rotation note
