# Van niets naar productie

Draaiboek voor de eerste deploy. Werk het van boven naar beneden af; elke stap is te kopiëren.
Reken op ongeveer twee uur, waarvan een half uur wachten op DNS.

Bij elk `<...>` vul je je eigen waarde in.

---

## 1. Server kiezen

Aanbeveling: **Hetzner CX22** — 2 vCPU, 4 GB RAM, 40 GB SSD, ongeveer €4,50 per maand,
datacenter Falkenstein of Neurenberg. EU-hosting, dus geen doorgifte buiten de EU om aan je
klanten uit te leggen.

Wat er straks draait en wat het kost aan geheugen:

| Container | Geheugen |
|-----------|----------|
| PostgreSQL | ~150 MB |
| API (.NET) | ~150 MB |
| Frontend (Nuxt) | ~150 MB |
| Caddy | ~20 MB |

Ruim binnen 4 GB. Kies Ubuntu 24.04 LTS als besturingssysteem en voeg bij het aanmaken meteen
je SSH-publieke sleutel toe, dan is wachtwoord-inloggen nooit aan geweest.

Heb je nog geen sleutel, maak er dan één op je eigen machine:

```bash
ssh-keygen -t ed25519 -C "planning-vps"
```

---

## 2. Domein en DNS

Koop een domein en zet twee records klaar. `app` is de hele applicatie — frontend en API delen
één domein, wat schuift langs een hele klasse CORS- en cookieproblemen heen.

| Type | Naam | Waarde |
|------|------|--------|
| A | `app` | het IPv4-adres van de VPS |
| AAAA | `app` | het IPv6-adres van de VPS |

Controleer voordat je verdergaat — Let's Encrypt heeft een limiet op mislukte pogingen, dus
te vroeg beginnen kost je een uur wachten:

```bash
dig +short app.<jouwdomein.nl>
```

---

## 3. Server dichtzetten

Inloggen als root:

```bash
ssh root@<server-ip>
```

Alles bijwerken en beveiligingsupdates automatisch maken:

```bash
apt update && apt upgrade -y
apt install -y unattended-upgrades ufw fail2ban restic
dpkg-reconfigure -plow unattended-upgrades
```

Een gebruiker zonder rootrechten voor de deploys:

```bash
adduser --disabled-password --gecos "" deploy
usermod -aG sudo deploy
mkdir -p /home/deploy/.ssh
cp /root/.ssh/authorized_keys /home/deploy/.ssh/
chown -R deploy:deploy /home/deploy/.ssh
chmod 700 /home/deploy/.ssh
chmod 600 /home/deploy/.ssh/authorized_keys
```

Firewall: alleen SSH en web naar buiten. De database en de containers zitten op een intern
Docker-netwerk en horen van buiten onbereikbaar te zijn.

```bash
ufw default deny incoming
ufw default allow outgoing
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable
```

Wachtwoord- en root-login uitzetten:

```bash
sed -i 's/^#*PermitRootLogin.*/PermitRootLogin no/' /etc/ssh/sshd_config
sed -i 's/^#*PasswordAuthentication.*/PasswordAuthentication no/' /etc/ssh/sshd_config
systemctl restart ssh
```

> Open **nu** een tweede terminal en controleer `ssh deploy@<server-ip>` vóór je deze sessie
> sluit. Klopt er iets niet aan de sleutels, dan sluit je jezelf anders buiten.

---

## 4. Docker installeren

Als `deploy`:

```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker deploy
```

Uitloggen en opnieuw inloggen, zodat je in de `docker`-groep zit. Controle:

```bash
docker run --rm hello-world
```

---

## 5. De stack neerzetten

```bash
sudo mkdir -p /opt/planning/backups
sudo chown -R deploy:deploy /opt/planning
cd /opt/planning
```

Kopieer vanaf je eigen machine drie bestanden uit de repository:

```bash
scp deploy/docker-compose.yml deploy/Caddyfile deploy/backup.sh deploy/.env.example deploy@<server-ip>:/opt/planning/
```

Op de server:

```bash
cd /opt/planning
cp .env.example .env
chmod 600 .env
chmod +x backup.sh
```

Genereer de geheimen en zet ze in `.env`. Voer ze één voor één uit en plak de uitvoer:

```bash
openssl rand -base64 32   # POSTGRES_PASSWORD
openssl rand -base64 64   # JWT_KEY
openssl rand -base64 32   # RESTIC_PASSWORD
```

> Bewaar `RESTIC_PASSWORD` ook érgens anders — in je wachtwoordmanager. Zonder die zin zijn je
> back-ups wiskundig onherstelbaar, ook door jou.

Vul verder in: `APP_DOMAIN`, `ACME_EMAIL`, `GITHUB_REPOSITORY_OWNER`. De `EMAIL_*`-regels
komen in stap 8.

---

## 6. GitHub instellen

**Secrets** — Settings → Secrets and variables → Actions → New repository secret:

| Secret | Waarde |
|--------|--------|
| `VPS_HOST` | het IP-adres of `app.<jouwdomein.nl>` |
| `VPS_USER` | `deploy` |
| `VPS_SSH_KEY` | de **private** sleutel, volledig, inclusief begin- en eindregel |
| `VPS_SSH_HOST_KEY` | uitvoer van het commando hieronder |

De host key haal je zo op, op je eigen machine:

```bash
ssh-keyscan -t ed25519 <server-ip>
```

Die pinning zorgt dat een deploy faalt in plaats van stilletjes doorgaat wanneer er ineens een
andere server op dat adres antwoordt.

**Environment** — Settings → Environments → New environment → `production`. Zet daar
"Required reviewers" op jezelf als je elke productie-deploy met één klik wilt goedkeuren.

**Test-branch** aanmaken:

```bash
git checkout main
git checkout -b test
git push -u origin test
```

**Branch protection** op `main` — Settings → Rules → New branch ruleset:
- Require a pull request before merging
- Require status checks to pass → `Backend` en `Frontend`

Daarmee is de route vast: werken op een feature branch → PR → CI groen → merge naar `main` →
productie.

---

## 7. Eerste deploy

De workflow schrijft `IMAGE_TAG` zelf, maar de allereerste keer moet er iets te pullen zijn.
Push naar `main` (of Actions → Deploy to production → Run workflow) en volg het live mee.

Op de server controleren:

```bash
cd /opt/planning
docker compose ps
docker compose logs -f api
```

Caddy vraagt bij de eerste start automatisch een certificaat aan. Open daarna
`https://app.<jouwdomein.nl>` en maak via **Registreren** het eerste account aan — dat wordt de
eigenaar van de eerste organisatie.

---

## 8. E-mail aanzetten

Kies een transactionele provider. Voor beginnen zonder kosten:

| Provider | Gratis | Let op |
|----------|--------|--------|
| **Resend** | 3.000/maand | EU-regio beschikbaar, prettige interface |
| **SMTP2GO** | 1.000/maand | EU-servers |
| **Postmark** | geen gratis laag | beste bezorging, ~$15/maand |

Verifieer je domein bij de provider. Die geeft je drie DNS-records — zet ze **allemaal**:

| Type | Doel |
|------|------|
| TXT (SPF) | zegt welke servers namens jouw domein mogen verzenden |
| CNAME/TXT (DKIM) | ondertekent je mail |
| TXT (DMARC) | vertelt ontvangers wat te doen bij een mislukte controle |

Begin DMARC soepel en scherp later aan:

```
_dmarc.<jouwdomein.nl>  TXT  "v=DMARC1; p=none; rua=mailto:dmarc@<jouwdomein.nl>"
```

Zonder SPF en DKIM accepteert je provider de mail en gooien Gmail en Outlook hem weg zonder
melding. Dat is precies het probleem dat je met een eigen mailserver óók zou hebben, alleen dan
in je eentje.

Vul daarna in `.env` op de server de `EMAIL_*`-waarden in en herstart:

```bash
docker compose up -d api
```

Controleer met **Wachtwoord vergeten** op de inlogpagina. Zolang `EMAIL_SMTP_HOST` leeg is,
verstuurt de app niets en schrijft hij de mail naar het log — handig, maar geen bewijs dat
bezorging werkt.

---

## 9. Back-ups

Initialiseer de off-site opslag (bijvoorbeeld een Hetzner Storage Box) en zet de dagelijkse taak:

```bash
cd /opt/planning
set -a && source .env && set +a
restic init
crontab -e
```

Toevoegen:

```
0 3 * * * /opt/planning/backup.sh >> /var/log/planning-backup.log 2>&1
```

Draai hem één keer met de hand en kijk of er iets uitkomt:

```bash
/opt/planning/backup.sh
ls -lh /opt/planning/backups
```

### De hersteloefening

**Doe dit één keer vóór je eerste klant.** Een back-up die je nooit hebt teruggezet is een
aanname, geen zekerheid.

```bash
# Zet de laatste dump terug in een wegwerp-database.
gunzip -c /opt/planning/backups/planning-<tijdstempel>.sql.gz \
  | docker compose exec -T db psql -U "$POSTGRES_USER" -d postgres \
    -c "CREATE DATABASE restoretest;" -d restoretest

# Kijk of de tabellen er zijn.
docker compose exec -T db psql -U "$POSTGRES_USER" -d restoretest -c '\dt'

# Opruimen.
docker compose exec -T db psql -U "$POSTGRES_USER" -d postgres -c "DROP DATABASE restoretest;"
```

---

## 10. Terugrollen

Elke image draagt de commit-SHA als tag, dus terug is één regel:

```bash
cd /opt/planning
cat .previous_tag              # de vorige versie
nano .env                      # zet IMAGE_TAG op die SHA
docker compose pull api web && docker compose up -d
```

Let op: een migratie die al gedraaid heeft, draait niet vanzelf terug. Heeft de mislukte versie
het schema veranderd, zet dan eerst de database terug uit de back-up van die nacht.

---

## Dagelijks gebruik

```bash
docker compose ps                     # wat draait er
docker compose logs -f api            # meekijken met de API
docker compose logs --tail 100 caddy  # certificaatproblemen
docker system prune -af --volumes=false   # oude images opruimen (volumes blijven)
docker compose exec db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"   # de database in
```

## Als er iets misgaat

| Symptoom | Waar je begint |
|----------|----------------|
| Geen certificaat | `docker compose logs caddy` — bijna altijd DNS dat nog niet klopt |
| API blijft herstarten | `docker compose logs api` — meestal `.env` of de database |
| Deploy hangt op "waiting for ready" | migratie faalt; zie de API-logs |
| 502 van Caddy | `docker compose ps` — draait `web` of `api` wel |
