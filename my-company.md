# My company — kern

De kern van het bedrijf achter dit product. Statisch bovenin, wekelijkse focus onderin.
Werk de sectie **Focus deze week** elke maandag bij; verplaats de vorige week naar het log.

> Bedrijfsnaam: _nog invullen_

## Wat we zijn

Een **multi-tenant planning-SaaS voor het MKB**, branche-onafhankelijk: elk bedrijf dat
mensen inplant bij klanten. Gebouwd en gerund door één persoon (Luuk), **naast een baan**,
op ~5–10 uur per week.

## Waarom wij

1. **Simpeler dan de rest.** Bestaande planningspakketten zijn te log en te duur voor kleine
   bedrijven. Elke feature moet die belofte overeind houden — complexiteit is hier geen
   neutraal iets, het is verlies van de belangrijkste reden om voor ons te kiezen.
2. **Persoonlijk contact en snel schakelen.** Ik ken de klant en bouw wensen snel. Grote
   leveranciers doen dat niet.

Die twee bijten elkaar: snel klantwensen bouwen is precies hoe een simpel product log wordt.
Zie *Bewaken* hieronder.

## Product

Eén codebase, per organisatie in te schakelen modules:

| Module   | Inhoud |
|----------|--------|
| Planning | Planning-records aanmaken, verplaatsen, bevestigen, weekoverzicht |
| Klant    | Klantbeheer |
| Beheer   | Organisatie, gebruikers, uitnodigingen, rollen, module-toewijzing |

Stack: .NET 10 (Clean Architecture) + Nuxt/Vue 3. Zie `CLAUDE.md` voor de regels.

## Fase en doel

- **Fase:** bouwen, nog geen klanten.
- **Doel komende 3 maanden:** het product werkend genoeg krijgen voor de eerste klant.

"Werkend genoeg" = alle vier af:
- [ ] Planning maken en inzien (aanmaken, verplaatsen, bevestigen, weekoverzicht)
- [ ] Klanten en medewerkers beheren (CRUD, uitnodigen, rollen, modules)
- [ ] Beschikbaarheid van medewerkers, inclusief conflictsignalering bij inplannen
- [ ] Live te zetten met echte data (hosting, login, backups)

Alles wat hier niet in staat is **niet nu**.

## Verdienmodel

Hybride: **planner-seats zijn de kernprijs**, medewerker-seats gratis tot een grens en
daarboven een klein bedrag per stuk, met Starter/Groei/Premium ernaast als alles-in-één-optie.
Geen credits voor het plannen zelf — die zetten een prikkel om de tool minder te gebruiken.

De bedragen en de medewerkersgrens (10 of 15) staan nog open. Te beslissen vóór het eerste
prijsgesprek met een klant — niet eerder, wel op tijd.

Volledige afweging: [besluit 0006](docs/decisions/0006-pricing-model.md).

## Bewaken

- **Simpel blijven.** Bij elke feature: maakt dit het product moeilijker uit te leggen?
- **Klantwensen generiek oplossen.** Eerst per-organisatie configuratie of een module,
  pas daarna maatwerk. Hardcoded klantnamen of `if (organizationId == ...)` zijn een red flag.
- **Scope.** 5–10 uur per week. Elke week die niet aan de vier MVP-punten hierboven werkt,
  is een week uitstel van de eerste klant.
- **Tenant-isolatie en secrets.** Zie de security-checklist in `CLAUDE.md`.

## Marketing

Positionering, kanalen, budget, ads en resultaten staan in `marketing.md`. Eén bestand,
maandelijks bijwerken.

---

# Focus deze week

**Week van: 2026-08-17**

**Doel deze week:**
- Beschikbaarheid-refactor afmaken: conflictsignalering werkend via `availabilityMath.ts`
  en `usePlanningAvailabilityPeriods` (de oude `AvailabilityOverlapChecker` is al verwijderd).
- Frontend lint schoon: `npm run lint:fix`, daarna de resterende echte errors handmatig
  (o.a. één `no-explicit-any`).
- De 170 ongecommitte bestanden opsplitsen in logische commits op een feature branch.

**Blokkades:**
- Geen.

**Bewust niet deze week:**
- Hosting en deployment (MVP-punt 4, staat op 0%) — schuift naar volgende week.
- Pricing-bedragen bepalen — het model staat (besluit 0006), alleen de bedragen nog niet.

**Stand van de code bij aanvang (woensdag 2026-08-19):**
- Backend bouwt schoon (0 warnings, 0 errors).
- Laatste commit 2026-08-16; 170 gewijzigde bestanden in de working tree.
- Datamodel compleet voor alle vier MVP-punten; laatste migratie 2026-07-18.
- Geen Dockerfile, geen CI, `.github` leeg.

---

# Weeklog

_(afgeronde weken komen hier, nieuwste bovenaan)_
