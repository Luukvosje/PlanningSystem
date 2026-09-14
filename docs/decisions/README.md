# Besluiten

Genomen keuzes, zodat een nieuwe sessie niet opnieuw dezelfde discussie voert. **Wijk hier
niet vanaf zonder het besluit expliciet te herzien** — en herzien betekent: het bestand
aanpassen, niet er omheen werken.

Deze index wordt bij elke sessiestart ingeladen (`SessionStart`-hook in
`.claude/settings.json`). Houd hem kort; de motivatie staat in de losse bestanden.

| # | Besluit | Status |
|---|---|---|
| [0001](0001-product-scope-en-niet-doelen.md) | MVP doet géén salaris, verlof, facturatie, HR, AI-planning, urenregistratie. Toets elke feature aan: sneller plannen, minder fouten, beter overzicht, minder WhatsApp | definitief |
| [0002](0002-open-diensten-nullable-assigneduserid.md) | Open dienst = `AssignedUserId` is null. Geen aparte status of tabel | definitief, gebouwd |
| [0003](0003-planning-delen-frontend-wame.md) | Planning delen is puur frontend: tekstformat + clipboard + `wa.me`. Geen API-wijziging, geen PDF nu | voorlopig, **niet gebouwd** |
| [0004](0004-notities-per-dienst-geen-bouwwerk.md) | Notities per dienst bestaan al — alleen UX testen, niets bouwen | definitief |
| [0005](0005-marketingkanaal-meta-ads.md) | Eerste kanaal is smal getargete Meta Ads, €10–20/dag, video | eerste richting |
| [0006](0006-pricing-model.md) | Hybride: planner-seats als kernprijs, medewerkers gratis tot een grens, plus pakketten. Geen credits | voorlopig, bedragen open |
| [0007](0007-open-diensten-vormgeving.md) | Open dienst = gestreepte rand plus i18n-label. API stuurt `assignedUserName: null` | definitief, gebouwd |
| [0008](0008-hosting-vps-docker-compose.md) | VPS met Docker Compose en Caddy, niet Azure. Database is PostgreSQL | definitief, gebouwd |
| [0009](0009-geen-ai-companion.md) | Geen LLM in de planningsflow. Conflictsignalering blijft deterministische regels | definitief |
| [0010](0010-tijdlijn-is-plannertool.md) | `/timeline` vereist een plannerrol; medewerker ziet alleen zijn eigen week | definitief, gebouwd |

## Nog open — hier is nog géén besluit over

- Exacte pricing-bedragen, de medewerkersgrens (10 of 15), en of de pakkettarieven blijven
  staan nu er een seat-model naast komt. Zie [0006](0006-pricing-model.md).
- Tekstformat voor "Planning delen": per medewerker of per dag gegroepeerd?
- Merknaam, domeinnaam, tone-of-voice en een concurrentievergelijking voor marketing-copy.

## Een besluit toevoegen

Nieuw bestand `NNNN-korte-titel.md`, oplopend genummerd, met deze kop:

```markdown
# NNNN — Titel

- **Datum:** YYYY-MM-DD
- **Status:** definitief | voorlopig
- **Raakt:** welke bestanden of onderdelen

## Besluit
## Waarom
## Alternatief overwogen
```

Daarna één regel in de tabel hierboven. Een besluit dat de code inhaalt krijgt er een
`## Stand van zaken (datum)` bij in plaats van een herschreven geschiedenis — wat je toen
besloot blijft staan, wat er sindsdien gebeurd is komt eronder.

Een vraag die nog niet beslist is hoort **niet** hier maar in het bijbehorende plan onder
[../plans/](../plans/), tot er een keuze is.
