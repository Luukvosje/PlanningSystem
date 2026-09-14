# From nothing to production

The runbook for the first deploy. Work top to bottom; every step is meant to be copied. Budget
roughly two hours, half an hour of which is waiting for DNS.

Fill in your own value wherever you see `<...>`.

---

## 1. Choosing a server

Recommendation: **Hetzner CX22** — 2 vCPU, 4 GB RAM, 40 GB SSD, about €4.50 a month, datacenter
Falkenstein or Nuremberg. EU hosting, so there is no transfer outside the EU to explain to your
customers.

What will run there, and what it costs in memory:

| Container | Memory |
|-----------|--------|
| PostgreSQL | ~150 MB |
| API (.NET) | ~150 MB |
| Frontend (Nuxt) | ~150 MB |
| Caddy | ~20 MB |

Comfortably within 4 GB. Pick Ubuntu 24.04 LTS as the operating system and add your SSH public
key while creating the machine, so password login was never enabled in the first place.

If you do not have a key yet, create one on your own machine:

```bash
ssh-keygen -t ed25519 -C "planning-vps"
```

---

## 2. Domain and DNS

Buy a domain and prepare two records. `app` is the whole application — frontend and API share
one domain, which sidesteps an entire class of CORS and cookie problems.

| Type | Name | Value |
|------|------|-------|
| A | `app` | the VPS's IPv4 address |
| AAAA | `app` | the VPS's IPv6 address |

Check before continuing — Let's Encrypt limits failed attempts, so starting too early costs you
an hour of waiting:

```bash
dig +short app.<yourdomain.com>
```

---

## 3. Locking the server down

Sign in as root:

```bash
ssh root@<server-ip>
```

Update everything and make security updates automatic:

```bash
apt update && apt upgrade -y
apt install -y unattended-upgrades ufw fail2ban restic
dpkg-reconfigure -plow unattended-upgrades
```

A non-root user for the deploys:

```bash
adduser --disabled-password --gecos "" deploy
usermod -aG sudo deploy
mkdir -p /home/deploy/.ssh
cp /root/.ssh/authorized_keys /home/deploy/.ssh/
chown -R deploy:deploy /home/deploy/.ssh
chmod 700 /home/deploy/.ssh
chmod 600 /home/deploy/.ssh/authorized_keys
```

Firewall: only SSH and web from outside. The database and the containers sit on an internal
Docker network and are meant to be unreachable externally.

```bash
ufw default deny incoming
ufw default allow outgoing
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable
```

Disable password and root login:

```bash
sed -i 's/^#*PermitRootLogin.*/PermitRootLogin no/' /etc/ssh/sshd_config
sed -i 's/^#*PasswordAuthentication.*/PasswordAuthentication no/' /etc/ssh/sshd_config
systemctl restart ssh
```

> Open a second terminal **now** and verify `ssh deploy@<server-ip>` before closing this
> session. If anything is wrong with the keys, this is where you would lock yourself out.

---

## 4. Installing Docker

As `deploy`:

```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker deploy
```

Sign out and back in so that you are in the `docker` group. Check:

```bash
docker run --rm hello-world
```

---

## 5. Putting the stack in place

```bash
sudo mkdir -p /opt/planning/backups
sudo chown -R deploy:deploy /opt/planning
cd /opt/planning
```

From your own machine, copy four files out of the repository:

```bash
scp deploy/docker-compose.yml deploy/Caddyfile deploy/backup.sh deploy/.env.example deploy@<server-ip>:/opt/planning/
```

On the server:

```bash
cd /opt/planning
cp .env.example .env
chmod 600 .env
chmod +x backup.sh
```

Generate the secrets and put them in `.env`. Run them one at a time and paste the output:

```bash
openssl rand -base64 32   # POSTGRES_PASSWORD
openssl rand -base64 64   # JWT_KEY
openssl rand -base64 32   # RESTIC_PASSWORD
```

> Keep `RESTIC_PASSWORD` somewhere else as well — in your password manager. Without that
> passphrase your backups are mathematically unrecoverable, including by you.

Also fill in `APP_DOMAIN`, `ACME_EMAIL` and `GITHUB_REPOSITORY_OWNER`. The `EMAIL_*` lines come
in step 8.

---

## 6. Setting up GitHub

**Secrets** — Settings → Secrets and variables → Actions → New repository secret:

| Secret | Value |
|--------|-------|
| `VPS_HOST` | the IP address or `app.<yourdomain.com>` |
| `VPS_USER` | `deploy` |
| `VPS_SSH_KEY` | the **private** key, in full, including its first and last line |
| `VPS_SSH_HOST_KEY` | the output of the command below |

Fetch the host key on your own machine:

```bash
ssh-keyscan -t ed25519 <server-ip>
```

That pinning makes a deploy fail rather than quietly continue when a different server suddenly
answers on that address.

**Environment** — Settings → Environments → New environment → `production`. Set "Required
reviewers" to yourself there if you want to approve every production deploy with one click.

Create a **test branch**:

```bash
git checkout main
git checkout -b test
git push -u origin test
```

**Branch protection** on `main` — Settings → Rules → New branch ruleset:
- Require a pull request before merging
- Require status checks to pass → `Backend` and `Frontend`

That fixes the route: work on a feature branch → PR → CI green → merge to `main` → production.

---

## 7. The first deploy

The workflow writes `IMAGE_TAG` itself, but the very first time there has to be something to
pull. Push to `main` (or Actions → Deploy to production → Run workflow) and follow it live.

Check on the server:

```bash
cd /opt/planning
docker compose ps
docker compose logs -f api
```

Caddy requests a certificate automatically on first start. Then open
`https://app.<yourdomain.com>` and create the first account through **Register** — that becomes
the owner of the first organization.

---

## 8. Turning email on

Pick a transactional provider. To start without cost:

| Provider | Free tier | Note |
|----------|-----------|------|
| **Resend** | 3,000/month | EU region available, pleasant interface |
| **SMTP2GO** | 1,000/month | EU servers |
| **Postmark** | no free tier | best deliverability, about $15/month |

Verify your domain with the provider. They give you three DNS records — set **all** of them:

| Type | Purpose |
|------|---------|
| TXT (SPF) | says which servers may send on behalf of your domain |
| CNAME/TXT (DKIM) | signs your mail |
| TXT (DMARC) | tells recipients what to do when a check fails |

Start DMARC leniently and tighten later:

```
_dmarc.<yourdomain.com>  TXT  "v=DMARC1; p=none; rua=mailto:dmarc@<yourdomain.com>"
```

Without SPF and DKIM your provider accepts the mail and Gmail and Outlook throw it away without
telling anyone. That is exactly the problem you would have running your own mail server, only
then you would be facing it alone.

Then fill in the `EMAIL_*` values in `.env` on the server and restart:

```bash
docker compose up -d api
```

Verify with **Forgot password** on the sign-in page. As long as `EMAIL_SMTP_HOST` is empty the
app sends nothing and writes the mail to the log — useful, but not proof that delivery works.

---

## 9. Backups

Initialise the off-site storage (a Hetzner Storage Box, for example) and set the daily job:

```bash
cd /opt/planning
set -a && source .env && set +a
restic init
crontab -e
```

Add:

```
0 3 * * * /opt/planning/backup.sh >> /var/log/planning-backup.log 2>&1
```

Run it once by hand and see whether anything comes out:

```bash
/opt/planning/backup.sh
ls -lh /opt/planning/backups
```

### The restore drill

**Do this once before your first customer.** A backup you have never restored is an assumption,
not a certainty.

```bash
# Restore the latest dump into a throwaway database.
gunzip -c /opt/planning/backups/planning-<timestamp>.sql.gz \
  | docker compose exec -T db psql -U "$POSTGRES_USER" -d postgres \
    -c "CREATE DATABASE restoretest;" -d restoretest

# Check the tables are there.
docker compose exec -T db psql -U "$POSTGRES_USER" -d restoretest -c '\dt'

# Clean up.
docker compose exec -T db psql -U "$POSTGRES_USER" -d postgres -c "DROP DATABASE restoretest;"
```

---

## 10. Rolling back

Every image carries the commit SHA as a tag, so going back is one line:

```bash
cd /opt/planning
cat .previous_tag              # the previous version
nano .env                      # set IMAGE_TAG to that SHA
docker compose pull api web && docker compose up -d
```

Note: a migration that has already run does not roll itself back. If the failed version changed
the schema, restore the database from that night's backup first. See
[deploy-and-rollback.md](deploy-and-rollback.md).

---

## Day-to-day use

```bash
docker compose ps                     # what is running
docker compose logs -f api            # follow the API
docker compose logs --tail 100 caddy  # certificate problems
docker system prune -af --volumes=false   # clear out old images (volumes stay)
docker compose exec db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"   # into the database
```

## When something goes wrong

| Symptom | Where to start |
|---------|----------------|
| No certificate | `docker compose logs caddy` — almost always DNS that is not right yet |
| The API keeps restarting | `docker compose logs api` — usually `.env` or the database |
| The deploy hangs on "waiting for ready" | a migration is failing; see the API logs |
| 502 from Caddy | `docker compose ps` — is `web` or `api` actually running |
