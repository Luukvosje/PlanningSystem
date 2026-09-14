# MVP-gaten — Plan (geen code, alleen besluiten)

Status van de 3 punten uit `../prd.md`, gecheckt tegen de huidige codebase (16 aug 2026):

| Feature | Status |
|---|---|
| Notities per dienst | ✅ Al gebouwd (domain, DTOs, frontend tooltip/sidebar) — alleen nog verifiëren of UX goed genoeg is |
| Open diensten | ✅ Gebouwd 22 aug 2026 (branch `feature/open-shifts`) |
| Planning delen (tekst + WhatsApp) | ❌ Nog niet gebouwd |

---

## 1. Open diensten

**Besluit:** `AssignedUserId` wordt nullable. Geen apart status-veld — een dienst zonder
medewerker ís de open dienst. Simpelste optie, past bij "geen functie bouwen die niet
bijdraagt aan sneller plannen."

**Wat dit raakt (voor als we gaan coderen):**

- **Domain** — `PlanningRecord`: `AssignedUserId` → `Guid?`, plus een `IsOpenShift`
  gemaksproperty. `Create`/`Update`/`Move` accepteren `Guid?`.
- **Infrastructure** — EF-configuratie: kolom nullable maken → nieuwe migratie nodig.
  Foreign key naar `User` blijft `Restrict`, werkt vanzelf met nullable FK.
- **Application** — DTOs (`CreatePlanningRequest`, `UpdatePlanningRequest`,
  `MovePlanningRequest`, `PlanningResponse`) → `AssignedUserId` nullable,
  `AssignedUserName` → `"Open dienst"` als er niemand is toegewezen. Validators: geen
  `NotEmpty`-check meer op `AssignedUserId`. Service: user-referentie alleen valideren
  als er een waarde is; overlap-detectie (`BuildOverlapLookup`) moet open diensten
  overslaan (die kunnen niet met zichzelf overlappen op een persoon die niet bestaat).
- **Frontend** — orval-modellen hergenereren na backend-wijziging; timeline-blok toont
  "Open dienst" state (bv. gestreepte rand, geen medewerkerkleur); form voor nieuwe
  dienst: medewerker-veld mag leeg; dashboard-teller "X open diensten" (staat al in de
  PRD-dashboard-mockup) kan hier bovenop.

**Bewust niet nu:** de "medewerker accepteert open dienst"-flow (staat in PRD als
*later*). Dat is een aparte actie/endpoint en hoort bij een volgende fase, niet bij het
mogelijk maken van open diensten zelf.

**Beslist (22 aug 2026):** gestreepte rand in de omtrek-stijl, label "Open dienst" in het
blok, en een rij "Open diensten" bovenaan de medewerker-modus waar je naartoe kunt slepen om
iemand van een dienst af te halen. Zie `../decisions/`.

---

## 2. Planning delen (tekst + WhatsApp)

**Besluit:** puur frontend, geen backend-wijziging nodig — de weekplanning-data is al
beschikbaar via de bestaande query composables.

**Aanpak:**

1. **Tekstformat-functie** — zet de al-geladen weekplanning (per dag, per medewerker,
   sorted op tijd) om naar het PRD-format:
   ```
   Planning week 31

   Maandag
   Kevin
   08:00 - 16:00

   Lisa
   16:00 - 22:00
   ```
   Open diensten expliciet vermelden als "Open dienst" (sluit aan op punt 1).
2. **Kopiëren als tekst** — clipboard API, knop in de week-header naast de bestaande
   acties.
3. **WhatsApp-share** — `https://wa.me/?text=<url-encoded tekst>` in een nieuw tabblad/
   deep link. Werkt zowel desktop (WhatsApp Web) als mobiel (native app) zonder
   extra dependency.
4. **PDF-export** — expliciet *later* volgens PRD, niet nu meenemen.

**Waar in de code (voor later):** een nieuwe util in `app/utils/planning/`
(bijv. `formatWeekPlanningText.ts`) + een component/knop in
`app/components/planning/header/`.

**Open vraag voor jou:** moet de gedeelde tekst per medewerker gegroepeerd zijn (zoals
PRD-voorbeeld) of per dag met alle medewerkers onder elkaar (zoals het andere
PRD-voorbeeld)? Beide voorbeelden staan er in, dus even kiezen.

---

## 3. Notities — verificatie, geen bouwwerk

Backend en frontend zijn er al. Enige actiepunt: samen kort testen of het UX-genoeg is
(waar kun je een notitie toevoegen/zien tijdens het plannen — is dat duidelijk genoeg
zonder uitleg, gezien de PRD-eis "planner snapt het binnen 5 minuten"). Geen
codewijziging verwacht, tenzij de test iets anders uitwijst.

---

## Volgorde-voorstel

1. Open diensten (backend + frontend) — grootste wijziging, ontgrendelt ook het
   dashboard-cijfer.
2. Planning delen — kleine, losstaande frontend-feature, kan gelijktijdig of erna.
3. Notities-UX-check — 15 minuten samen testen, geen aparte sprint nodig.

Zodra je akkoord bent op dit plan (en de twee open vragen), kunnen we per feature een
losse coding-sessie doen.
