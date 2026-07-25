# Deployment

One script, one generic compose file, one env file per instance. To deploy any
instance:

```bash
sudo ./deploy/deploy.sh <instance>
```

That is the whole workflow. Everything that differs between instances lives in
**`deploy/instances/<instance>.env`** — the single source of truth for that
instance. You never edit the compose file or the script to add or change an
instance; you only touch its env file.

## Instances

| Instance | Domain | Web / API / DB ports |
|----------|--------|----------------------|
| `cfelbd-live` | https://cfelbd.hekzor.com | 5179 / 5178 / 5437 |
| `cfelbd-dev`  | https://cfelbd-dev.hekzor.com | 5181 / 5180 / 5438 |
| `uorghub`     | https://uorghub.hekzor.com | 3001 / 5177 / 5436 |

## First-time setup on the server (per instance)

The real env files are **not** in git (they hold secrets). Create each one from
its template and fill in the two secret lines:

```bash
cd <repo checkout>
cp deploy/instances/cfelbd-live.env.example deploy/instances/cfelbd-live.env
nano deploy/instances/cfelbd-live.env      # set DB_PASSWORD and JWT_SECRET
```

Everything else in the template (ports, container names, domain, JWT issuer)
is already correct — only `DB_PASSWORD` and `JWT_SECRET` say `CHANGE_ME`.

> The values these instances currently use are in `CFL_DEPLOYMENT_INFO.md`.
> After you move them into the `.env` files, **rotate them** (see that doc),
> because the old values are in git history.

## Deploying

```bash
sudo ./deploy/deploy.sh cfelbd-dev              # pull master, rebuild, restart, reload nginx
sudo ./deploy/deploy.sh cfelbd-live
sudo ./deploy/deploy.sh uorghub

sudo ./deploy/deploy.sh cfelbd-dev --no-pull    # deploy current checkout, no git pull
sudo ./deploy/deploy.sh cfelbd-dev --dry-run    # print the steps without running them
```

Run with no instance to list what's available:

```bash
./deploy/deploy.sh
```

### What a deploy does

1. `git fetch` + `reset --hard origin/$GIT_BRANCH` (skip with `--no-pull`)
2. `docker compose build` using this instance's env file
3. Rolling restart: `db` → `api` → wait → `web`
4. `nginx -t && nginx -s reload` (skip with `--no-nginx`)

## Why each instance has its own image tag

The web image bakes `VITE_API_URL` **at build time**. If every instance built
to the same tag (`orghub-web:latest`), a restart-without-rebuild could serve one
site the wrong API URL. Each env file sets a unique `API_IMAGE` / `WEB_IMAGE`
(e.g. `orghub-web:cfelbd-live`), so an instance can only ever run its own image.

## How instances stay isolated on one host

`COMPOSE_PROJECT_NAME` (set per env file) namespaces each instance's Docker
network and volumes, and every container name is unique. So all instances share
one host — and can even share one checkout — without colliding. Internally each
API reaches its database as `Host=db` and nginx reaches the API as `api`; those
names resolve only within that instance's own network.

## Adding a new instance

1. `cp deploy/instances/cfelbd-live.env.example deploy/instances/<new>.env.example`
2. Edit it: new `COMPOSE_PROJECT_NAME`, container names, **unique ports**,
   unique image tags, DB name, domain, origins, `VITE_API_URL`.
3. Commit the `.example`; on the server copy it to `<new>.env` and fill secrets.
4. Point an nginx site + SSL cert at the new `WEB_PORT` (see below).
5. `sudo ./deploy/deploy.sh <new>`

## One-time server bootstrap

`scripts/deploy.sh` provisions a fresh server (Docker, nginx, certbot, SSL) for
the main instance. New instances reuse the installed Docker/nginx; you only add
an nginx site pointing `https://<domain>` → `127.0.0.1:<WEB_PORT>` and issue a
cert with `certbot --nginx -d <domain>`.

## Superseded files

Once every instance deploys through `deploy.sh`, these are no longer used and
can be removed: `scripts/update.sh` and `docker-compose.prod.yml` (both replaced
for the `uorghub` instance by `uorghub.env` + the generic compose). The old
per-instance scripts and CFL compose files have already been removed.
