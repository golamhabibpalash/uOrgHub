# CI/CD

Two workflows:

| Workflow | When | What it does |
|----------|------|--------------|
| **CI** (`ci.yml`) | every PR to `master`, every push to a working branch | Compiles backend + frontend (the gate). Runs tests and lint too, reported but non-blocking. |
| **Deploy** (`deploy.yml`) | manual — **Actions → Deploy → Run workflow** | SSHes into the VPS and runs `./deploy/deploy.sh <instance>`, the same command you'd run by hand. |

Deploy is **manual, one instance at a time**: you pick `cfelbd-dev`, `cfelbd-live`,
or `uorghub` from a dropdown. Nothing deploys on its own.

---

## One-time setup

### 1. A stable checkout on the VPS

The deploy needs a persistent git checkout (not `/tmp`, which gets wiped):

```bash
# on the VPS, as root
cd /opt
git clone https://github.com/golamhabibpalash/uOrgHub.git orghub-source
cd /opt/orghub-source
git checkout master        # or the branch each instance's env pins
```

Create each instance's env file here once (secrets are never in git):

```bash
cp deploy/instances/cfelbd-dev.env.example deploy/instances/cfelbd-dev.env
nano deploy/instances/cfelbd-dev.env       # fill DB_PASSWORD, JWT_SECRET
```

### 2. A dedicated SSH deploy key

Generate a key **just for CI** (don't reuse your personal key). On your local machine:

```bash
ssh-keygen -t ed25519 -C "github-actions-deploy" -f ~/.ssh/orghub_deploy -N ""
```

Install the **public** key on the VPS so the runner can log in:

```bash
ssh-copy-id -i ~/.ssh/orghub_deploy.pub root@62.84.181.246
# or append ~/.ssh/orghub_deploy.pub to /root/.ssh/authorized_keys by hand
```

### 3. GitHub repository secrets

Repo → **Settings → Secrets and variables → Actions → New repository secret**:

| Secret | Value |
|--------|-------|
| `VPS_HOST` | `62.84.181.246` |
| `VPS_USER` | `root` |
| `VPS_CHECKOUT` | `/opt/orghub-source` |
| `VPS_SSH_KEY` | the **private** key — contents of `~/.ssh/orghub_deploy` (whole file, including the BEGIN/END lines) |

### 4. (Recommended) Approval gates for live

Repo → **Settings → Environments** → open `cfelbd-live` and `uorghub` →
enable **Required reviewers** and add yourself. Now a live deploy pauses and
waits for your click, even though anyone with access can start one. (`cfelbd-dev`
can stay ungated for a fast dev loop.)

---

## Deploying

1. GitHub → **Actions → Deploy → Run workflow**
2. Pick the instance → **Run workflow**
3. Watch the log. It runs `deploy.sh`, which pulls the instance's branch, rebuilds
   its images, rolling-restarts the containers, and reloads nginx.

What each run deploys is whatever `GIT_BRANCH` the instance's env file points at
(`deploy/instances/<instance>.env` on the server) — so to change what `cfelbd-live`
runs, change its `GIT_BRANCH`, not this workflow.

### Deploying by hand still works

CI/CD is a convenience wrapper, not a replacement. On the VPS you can always:

```bash
cd /opt/orghub-source
sudo ./deploy/deploy.sh cfelbd-dev
```

---

## Hardening (optional, later)

The deploy secret is a **root** SSH key, so it grants full server access. Two ways
to tighten it when you're ready:

- **Dedicated deploy user** with the `docker` group and a narrow `NOPASSWD` sudo
  rule for `nginx`, instead of root.
- **Forced-command key**: restrict the key in `authorized_keys` to a wrapper that
  only accepts `deploy.sh <known-instance>`, so a leaked key can't run arbitrary
  commands.
