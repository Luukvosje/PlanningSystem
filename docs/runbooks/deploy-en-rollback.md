# Deployen en terugrollen

Voor de dagelijkse gang van zaken. De éérste keer opzetten staat in
[first-deploy.md](first-deploy.md).

## Hoe een deploy loopt

**`main` is productie. Niets anders deployt.**

Een push naar `main` (of `workflow_dispatch`) start `.github/workflows/deploy.yml`, in vier
fasen:

1. **verify** — `dotnet restore/build/test` op `Planning.slnx`, daarna
   `pnpm install --frozen-lockfile && lint && test && build` in `Planning.Web`. Precies
   dezelfde checks als `ci.yml` op een PR.
2. **build** — twee images parallel (`planningsystem-api`, `planningsystem-web`) naar
   ghcr.io, getagd op **commit-SHA** én `latest`. De SHA is de echte tag; dat is wat
   terugrollen tot één regel reduceert.
3. **deploy** — ssh naar de VPS, `IMAGE_TAG` in `/opt/planning/.env` bijwerken (de vorige
   waarde wordt weggeschreven naar `.previous_tag`), `docker compose pull api web` en
   `up -d --remove-orphans`.
4. **ready-check** — dertig pogingen, vijf seconden uit elkaar, op
   `http://127.0.0.1:8080/health/ready` binnen de api-container. Dat endpoint bevat de
   database, dus het slaagt pas als de migraties gedraaid zijn en de API echt kan serveren.
   Faalt het, dan print de workflow de laatste 60 regels API-log en de rollback-instructie.

Twee deploys kunnen elkaar niet raken (`concurrency: deploy-production`) en worden nooit
halverwege afgebroken (`cancel-in-progress: false`) — een onderbroken deploy laat de stack
achter op gemengde image-versies.

## Migraties gaan vanzelf mee

De API past openstaande migraties toe bij het opstarten. Er is dus geen aparte stap, maar ook
geen rem: **een destructieve migratie op `main` wordt bij de volgende deploy uitgevoerd
zonder dat iemand een commando typt.** Zie de skill `db-schema-change`.

Dit werkt alleen omdat er precies één API-instantie draait. Bij een tweede racen ze op
dezelfde migratie en moet dit een aparte one-shot job worden vóór de app start.

## Terugrollen

Op de VPS, in `/opt/planning`:

```bash
cat .previous_tag                       # de SHA van vóór de laatste deploy
```

```bash
sed -i "s|^IMAGE_TAG=.*|IMAGE_TAG=<sha>|" .env && docker compose pull api web && docker compose up -d
```

Daarna controleren:

```bash
docker compose ps && docker compose exec -T api curl -fsS http://127.0.0.1:8080/health/ready
```

**Wat terugrollen níet doet: de database terugdraaien.** Images gaan terug, het schema niet.
Bevatte de kapotte deploy een migratie die kolommen wegneemt of hertypeert, dan draait de
oude code tegen het nieuwe schema — en dat gaat meestal mis. In dat geval is de volgorde:
eerst de database herstellen uit de back-up (zie [first-deploy.md](first-deploy.md) §9),
daarna pas de image terugzetten.

Dat is de reden dat een destructieve migratie een gesprek waard is en niet alleen een review.

## Snel kijken wat er aan de hand is

```bash
docker compose ps
```

```bash
docker compose logs --tail 100 api
```

```bash
docker compose exec -T api curl -fsS http://127.0.0.1:8080/health/ready; echo " <- exit $?"
```

Twee health-endpoints, met opzet gescheiden:

- **`/health/live`** — draait dit proces? Geen database. Dit pollen de container-healthcheck
  en de proxy, zodat een databasehapering geen gezonde API herstart.
- **`/health/ready`** — inclusief database. Dit wacht de deploy af.

Geen van beide is publiek bereikbaar; Caddy routeert `/health/*` bewust niet. Alles wat
bereikbaar is, moet je ook veilig houden.

## De containers

| Container | Poorten | Data die blijft |
|---|---|---|
| `caddy` | 80, 443, 443/udp — **de enige die publiceert** | `caddy-data` (certificaten) |
| `web` | intern 3000 | — |
| `api` | intern 8080 | `logo-data` (organisatielogo's) |
| `db` | intern 5432 | `db-data` |

`caddy-data` kwijtraken betekent nieuwe certificaten aanvragen en tegen de rate limits van
Let's Encrypt aanlopen. `logo-data` is de enige gebruikersdata buiten de database — de
back-up moet beide meenemen.

## Als de ready-check blijft falen

Op volgorde van waarschijnlijkheid:

1. **Migratie faalt.** `docker compose logs api` toont de EF-fout. De API start niet af zonder
   een geslaagde migratie.
2. **Database niet gezond.** `docker compose ps` toont `db` als unhealthy; `pg_isready` in de
   healthcheck faalt. Kijk naar schijfruimte (`df -h`).
3. **Ontbrekende environment variable.** `Jwt__Key`, `ConnectionStrings__DefaultConnection` en
   een lege `Cors__AllowedOrigins` zijn alle drie een fatale startfout, geen waarschuwing.
   Zie [references/configuration.md](../references/configuration.md).
4. **Verkeerde image-tag.** `grep IMAGE_TAG .env` en vergelijk met de SHA in de workflow.
