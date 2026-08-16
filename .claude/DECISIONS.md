# Beslissingenlogboek — Planning SaaS

Eén centrale plek voor besluiten (product, technisch, marketing) die al genomen zijn,
zodat een nieuwe sessie niet opnieuw dezelfde discussie hoeft te voeren of dezelfde vraag
opnieuw hoeft te stellen. `CLAUDE.md` blijft leidend voor vaste architectuur-/codeconventies
(die veranderen niet per gesprek); dit bestand is voor de beslissingen die daar niet in
thuishoren omdat ze specifiek, tijdgebonden of nog in beweging zijn.

**Regel:** nieuwe besluiten komen onderaan bij "Log", nieuwste eerst. Elke entry:

```
### [datum] — [onderwerp]
Besluit: ...
Waarom: ...
Alternatief overwogen: ...
Status: definitief | voorlopig
```

Open vragen die nog niet beslist zijn horen NIET hier — die blijven in het bijbehorende
plan-document (bv. `MVP_GAPS_PLAN.md`) totdat er een keuze is, en verhuizen dan hierheen.

---

## Log

### 2026-08-16 — Pricing-model
Besluit: hybride model — planner-seats zijn de kernprijs, medewerker-seats gratis tot een
grens (daarna klein bedrag per stuk), plus een totaalpakket (Starter/Groei/Premium) als
eenvoudig alternatief voor wie niet wil puzzelen met seats.
Waarom: de waarde zit bij de planner, niet bij de medewerker die alleen zijn rooster
bekijkt; credits passen niet bij een tool die je juist vaker wil laten gebruiken (zie
onderstaand punt).
Alternatief overwogen: credits per dienst/actie — verworpen, want dat zet een prikkel neer
om de tool minder te gebruiken, wat tegen het doel ingaat (minder WhatsApp, meer overzicht).
Status: voorlopig — exacte bedragen en de precieze medewerkersgrens (10 vs 15) staan nog
open, zie `CLAUDE_COLLABORATION.md` §3.

### 2026-08-16 — Marketingkanaal: Meta Ads als eerste richting
Besluit: starten met smal getargete Meta Ads (interesse/gedrag + geo-radius), kleine
testbudgetten (~€10-20/dag), video die "Word → chaos → oplossing" toont.
Waarom: doelgroep (sportscholen, later kleine bedrijven) is smal en lokaal — breed
adverteren is verspilling; een product dat je moet zien werken presteert beter in video
dan statische tekst.
Alternatief overwogen: lookalike audiences — pas zinvol na 1-2 betalende klanten, nu nog
geen basis voor.
Status: definitief als eerste richting, niet geverifieerd met live marktdata (geen
actuele CPM/targeting gecheckt).

### 2026-08-16 — Notities per dienst: geen bouwwerk, alleen UX-verificatie
Besluit: backend, DTOs en frontend (tooltip/sidebar) bestaan al — geen nieuwe code, wel
samen 15 minuten testen of het duidelijk genoeg is zonder uitleg.
Waarom: voldoet al aan de PRD-eis; onnodig om iets te bouwen wat er al is.
Alternatief overwogen: n.v.t.
Status: definitief, actie is een test-sessie, geen sprint.

### 2026-08-16 — Planning delen: puur frontend, wa.me-link
Besluit: geen backend-wijziging. Tekstformat-functie zet de al-geladen weekplanning om
naar leesbare tekst, "kopiëren"-knop via clipboard API, WhatsApp-share via
`https://wa.me/?text=<url-encoded tekst>`. PDF-export expliciet later, niet nu.
Waarom: weekplanning-data is al beschikbaar via bestaande query composables — geen reden
om de API uit te breiden voor een puur presentatie-probleem. `wa.me` werkt op desktop
(WhatsApp Web) én mobiel zonder extra dependency.
Alternatief overwogen: losse PDF-export nu al meenemen — verworpen, staat expliciet als
*later* in de PRD.
Status: voorlopig — open vraag (nog niet beslist): tekst per medewerker gegroepeerd, of
per dag met alle medewerkers onder elkaar? Zie `MVP_GAPS_PLAN.md` §2. Locatie in code
zodra gebouwd: `app/utils/planning/formatWeekPlanningText.ts` +
`app/components/planning/header/`.

### 2026-08-16 — Open diensten: AssignedUserId nullable, geen apart statusveld
Besluit: `PlanningRecord.AssignedUserId` wordt `Guid?`. Een dienst zonder medewerker ís de
open dienst — geen aparte `OpenShift`-status of -tabel. `IsOpenShift` als
gemaksproperty op basis van de null-check.
Waarom: simpelste model dat het probleem oplost; past bij het principe "geen functie
bouwen die niet bijdraagt aan sneller plannen, minder fouten, beter overzicht, minder
WhatsApp".
Alternatief overwogen: apart statuslabel voor dashboard-tellingen — niet nodig, telt ook
via null-check.
Status: definitief voor het datamodel. Bewust NIET nu: de "medewerker accepteert open
dienst"-flow (aparte actie/endpoint, hoort bij een latere fase). Open vraag (nog niet
beslist): aparte kleur/stijl voor open diensten in de tijdlijn, of grijs/leeg? Zie
`MVP_GAPS_PLAN.md` §1 voor de volledige impact-analyse (Domain/Infrastructure/
Application/Frontend).

### 2026-08-16 — Product-scope / niet-doelen
Besluit: MVP doet geen salarisadministratie, contractbeheer, verlof, declaraties,
facturatie, HR-dossiers, AI-planning, urenregistratie, klok in/uit, certificaten. Elke
feature wordt getoetst aan: draagt dit bij aan sneller plannen, minder fouten, beter
overzicht, minder WhatsApp?
Waarom: focus op de kernbelofte (binnen 5 minuten een weekplanning maken en delen) voor
de eerste klant (sportschoolketen); scope-kruip is het grootste risico voor een MVP.
Alternatief overwogen: n.v.t. — dit is een grens, geen keuze tussen opties.
Status: definitief, niet aanpassen zonder expliciet gesprek (zie `CLAUDE_COLLABORATION.md`
§1). Wel een bewuste toekomstrichting (niet-belofte): groei naar bredere bedrijfs-SaaS via
het bestaande `Modules`-systeem, plus een aparte medewerker-app — niet op vooruitlopen in
code of marketing totdat expliciet aangegeven.

---

## Nog openstaand (bewust hier genoemd, nog geen besluit)

- Exacte pricing-bedragen per planner-/medewerker-seat en de precieze grens.
- Tekstformat voor "Planning delen": per medewerker of per dag gegroepeerd?
- Kleur/stijl van open diensten in de tijdlijn.
- Merknaam/domeinnaam, tone-of-voice, concurrentie-vergelijking voor marketing-copy.
