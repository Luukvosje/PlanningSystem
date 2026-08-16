# Beschikbaarheid — Herontwerp (plan, geen code)

Status: **concept, wacht op akkoord**. Geen bestanden aangepast.

Dit plan reageert op een brainstorm-idee (unified patroon+uitzondering-model, drie UI-plekken
i.p.v. één beschikbaarheid-pagina). Onderstaand: wat er al bestaat, waar het idee klopt, waar
het een echte architectuurwijziging is (niet alleen UI), en waar het afwijkt van de PRD/MVP-
scope.

---

## 1. Wat er nu al is (bevindingen, geen aannames)

Het is **geen** twee losse features op databaseniveau. Er is al één entity:
`AvailabilityRule` (`Planning.Domain/Availability/AvailabilityRule.cs`) met een discriminator
`Type` (`Weekly` / `OneTime`). Beide typen delen dezelfde tabel, dezelfde repository, dezelfde
service. Het "voelt" als twee features omdat de UI ze als twee losse lijsten op één pagina
rendert (`RulesEditor.vue` → weekly-sectie + one-time-sectie, elk met eigen "toevoegen"-knop),
niet omdat het model gesplitst is.

Belangrijker: de manier waarop een uitzondering vandaag met het patroon samenwerkt is **puur
optellen (union), niet overschrijven**:

- `AvailabilityRuleStatus` kent `Unavailable`, `Preferred`, `Available` — maar de entity
  blokkeert alles behalve `Unavailable` hard
  (`AvailabilityRule.cs:173-179`: *"Only unavailable rules are supported at this time."*),
  en de validator herhaalt die regel op applicatieniveau.
- Dat betekent: het model is vandaag een **blocklist**. Iedereen is standaard beschikbaar;
  regels (weekly of onetime) prikken alleen gaten van *on*beschikbaarheid.
- `AvailabilityPeriodExpander.ExpandForDate` (Application-laag) voegt voor een gegeven datum
  gewoon alle matchende Weekly- én OneTime-periodes samen tot één lijst
  (`AvailabilityPeriodExpander.cs:8-41`) — er is geen precedence-logica. Een OneTime-regel
  "overschrijft" een Weekly-regel niet; hij komt er gewoon bovenop.
- Er bestaat een `AvailabilityOverlapChecker` voor server-side overlap-detectie, maar die wordt
  nergens automatisch aangeroepen als een uitzondering wordt toegevoegd na een al ingeplande
  dienst — dat scenario geeft vandaag geen enkele waarschuwing.

Planner-zichtbaarheid tijdens roosteren **bestaat al**, deels zoals het brainstorm-idee
voorstelt: een achtergrondlaag in de planningskalender (`showAvailability`-toggle,
default **uit**, alleen visueel — geen click-to-edit, geen "namens iemand aanmaken" vanuit
de kalender). Punt 3 van het idee is dus grotendeels al gebouwd, niet nieuw.

De medewerker heeft **geen** gecombineerde kalender van rooster + eigen uitzonderingen. De
"Mijn planning"-view (`PlanningMyView.vue` → `PlanningMyDayStrip.vue` +
`PlanningMyDayDetail.vue`) toont alleen diensten; beschikbaarheid komt daar nergens in voor.
Punt 2 van het idee is dus wél nieuw werk.

Backend-autorisatie voor "namens iemand anders" bestaat al voor **beide** regeltypen:
`CanManageForEmployeeAsync` in `AvailabilityRuleService` staat een Planner/Admin/Owner al toe
om elke medewerker se regels te beheren, niet alleen de eigen. Dit is dus geen nieuwe
beslissing voor de backend — wel een UI-keuze (waar in de planner-flow bied je dat aan).

Ten slotte: `RuleSheet.vue` (het aanmaak/edit-formulier) is een volledig los, hand-geschreven
formulier — niet gebouwd op de generieke `Form`/`useCreate`/`useEdit`-stack die de rest van de
app net naar toe migreert (zie de vele gewijzigde form-bestanden in de huidige git-status). En
`useAvailabilityApi.ts` praat met een handgeschreven `availabilityClient.ts` die zelf
PascalCase/camelCase normaliseert, terwijl er al een gegenereerde Orval-client voor
`availability` bestaat die niet gebruikt wordt. Dit is exact het patroon dat `CLAUDE.md`
al benoemt als legacy-symptoom (vergelijkbaar met `planningClient.ts`) — "niet imiteren in
nieuwe code, migreer richting de generated client als je dit aanraakt."

---

## 2. Waar het brainstorm-idee klopt — en waar het een architectuurwijziging is, geen UI-wijziging

**Klopt, en is vooral een UI-herschikking (laag risico):**
- "Eén model" — is al zo op databaseniveau. Het echte werk is de UI zo laten *voelen*.
- Planner-achtergrondlaag in de bestaande planningskalender — bestaat al, kleine uitbreiding.
- Basispatroon verplaatsen naar instellingen/profiel — puur een kwestie van de weekly-sectie
  van `RulesEditor.vue` een andere plek geven; het model verandert niet.

**Klopt conceptueel, maar is een echte modelwijziging (hoger risico), niet "gewoon UI":**
- "Een uitzondering overschrijft het patroon altijd, in beide richtingen" — dit vereist:
  1. `Status.Available` uitpakken uit de huidige hard-block (`ValidateStatus` aanpassen), zodat
     een uitzondering kan zeggen "ik ben deze dag wél beschikbaar" ondanks een weekly-blok.
  2. `AvailabilityPeriodExpander` herschrijven van "optellen" naar "patroon toepassen, dan
     OneTime-regels met voorrang over de overlappende tijdvakken heen leggen" — een
     interval-vervang/aftrek-algoritme, geen simpele lijst meer.
  3. Dezelfde logica bestaat **ook client-side** (`availabilityMath.ts`, ter voorkoming van een
     extra round-trip voor conflict-checks tijdens het inplannen) — die moet dan in lockstep
     mee veranderen, anders raken server en client het oneens over wat "beschikbaar" betekent
     op een gegeven dag. Dat is precies het soort gedupliceerde-logica-risico dat er al is
     (client mirrort server), en dit voorstel maakt die logica complexer, niet simpeler.

Dit tweede stuk is de kern van waarom ik dit niet als "klein uitgangspunt" zou behandelen: het
raakt Domain (invariant loslaten), Application (expander-algoritme), én twee synchrone
implementaties (server + client) van dezelfde regel.

---

## 3. Toetsing aan MVP-principe / architectuur (CLAUDE_COLLABORATION.md, CLAUDE.md)

Twee dingen die ik expliciet wil benoemen, geen showstoppers maar wel relevant voor de
volgorde:

1. **Beschikbaarheid staat in de PRD als Fase 2**, en de Fase 1-gaten uit `MVP_GAPS_PLAN.md`
   (open diensten, planning delen) staan nog open. Beschikbaarheid *bestaat* al functioneel
   (zij het imperfect) — dit is dus een verbetering van iets dat al werkt, niet het dichten van
   een gat dat de eerste klant blokkeert. Geen bezwaar om er nu aan te werken als jij dat wilt,
   maar ik wil de prioriteits-afweging zichtbaar maken in plaats van hem te negeren.
2. **De "in beide richtingen"-uitbreiding staat niet in de PRD.** Het PRD-voorbeeld
   (`Vrijdag: Niet beschikbaar 19:00-22:00`) is één-richting: het patroon is de default-
   beschikbaarheid, een uitzondering blokkeert een stuk extra. Het scenario "ik ben normaal
   niet beschikbaar op maandagavond, maar deze ene maandag wél" is een uitbreiding die jij
   nu toevoegt vanuit de brainstorm, niet iets dat de sportschool-klant gevraagd heeft. Getoetst
   aan "sneller plannen, minder fouten, beter overzicht, minder WhatsApp": het voorkomt mogelijk
   een WhatsApp-appje ("ik kan toch wel maandag"), maar het is de duurste wijziging in dit hele
   plan qua risico/complexiteit voor een scenario dat nog niet bevestigd is als een echt
   probleem. Zie open vraag 1 hieronder.

Verder geen conflicten met de architectuur — het bestaande model (TenantEntity, Result-pattern,
FluentValidation, module-policy op de controller) volgt de conventions al correct; dit plan
stelt niet voor daar iets aan te veranderen.

---

## 4. Voorgestelde aanpak (mijn advies)

Ik zou dit in twee onafhankelijke stappen splitsen, zodat je na stap 1 al waarde hebt en stap 2
alleen doet als er echt een reden voor is:

### Stap 1 — UI-herschikking, model blijft "optellen" (blocklist), geen backend-risico
- Basispatroon-editor verhuist naar instellingen/profiel (weekly-sectie van `RulesEditor.vue`
  loskoppelen van de one-time-sectie, geen datamodelwijziging).
- Medewerker: `PlanningMyDayStrip.vue`/`PlanningMyDayDetail.vue` uitbreiden zodat eigen
  uitzonderingen (en evt. patroon-indicatie) samen met diensten getoond worden op dezelfde
  dag-as. Nieuw aanmaken van een uitzondering kan hier ook een plek krijgen (in plaats van op
  een losse pagina).
- Planner: bestaande `showAvailability`-overlay behouden, evt. default aanzetten en/of een
  mogelijkheid toevoegen om vanuit de kalender een uitzondering namens een medewerker aan te
  maken (backend-autorisatie hiervoor bestaat al).
- Terwijl we hier toch in zitten: `RuleSheet.vue` migreren naar de generieke `Form`/
  `useCreate`/`useEdit`-stack, en `useAvailabilityApi.ts` naar de gegenereerde Orval-client i.p.v.
  de handgeschreven normalizer — dit is opportunistisch meeliften op conventies die de rest van
  de app al volgt, geen apart project.

### Stap 2 — "overschrijft in beide richtingen" (alleen als je dat echt wilt), los in te plannen
- `Status.Available` uitpakken, expander herschrijven met precedence, client-side mirror-logica
  in lockstep meenemen. Dit doe ik pas als stap 1 er staat en jij bevestigt dat dit scenario
  (normaal niet beschikbaar → deze ene dag wél) echt voorkomt bij de sportschool-klant.

Dit scheidt "veilig en snel" (stap 1) van "risicovol en duur" (stap 2), in plaats van ze als
één brainstorm-pakket te behandelen.

---

## 5. Open vragen (jouw beslissing)

1. **Is de "in beide richtingen"-uitbreiding (stap 2) nu al nodig**, of is de bestaande
   blocklist-richting (patroon + extra blokkades) voorlopig genoeg? Dit bepaalt of we stap 2
   überhaupt inplannen.
2. **Dagdeel-precisie**: er is nooit een DayPart-enum geweest in de huidige code (die is
   bewust verwijderd in een eerdere iteratie, git-historie toont dit) — vandaag is alles vrije
   `TimeOnly`-start/eind met UI-presets (hele dag / tot / vanaf / aangepast). Wil je alsnog
   ochtend/middag/avond als vaste blokken terug, of blijft vrije tijd + presets de norm?
3. **Wie mag een uitzondering aanmaken?** Backend staat al "planner namens medewerker" toe voor
   beide regeltypen. Vraag is puur UX: bied je dat expliciet aan vanuit de planningskalender
   (stap 1), of blijft het alleen mogelijk via de medewerker-kalender/instellingen met
   `?userId=`-achtige omweg zoals nu?
4. **Conflict-afhandeling**: als een uitzondering wordt toegevoegd ná een al ingeplande dienst
   op die datum, wil je (a) niets — planner ontdekt het pas via de overlay, (b) een zachte
   waarschuwing (toast/dashboard-melding) op het moment van aanmaken, of (c) hard blokkeren tot
   het is opgelost? De bestaande `AvailabilityOverlapChecker` bestaat al server-side maar wordt
   nergens voor dit scenario aangeroepen — (b) is de kleinste toevoeging die aansluit bij
   "minder fouten" zonder een blokkerende flow te introduceren.
5. **Prioriteit**: dit oppakken nu, of eerst de Fase 1-gaten uit `MVP_GAPS_PLAN.md` (open
   diensten, planning delen) afronden? Geen technische blocker, puur een volgorde-vraag.

---

Zodra je hierop reageert (akkoord op de aanpak in §4, antwoorden op §5), lever ik pas concrete
code — per de afspraak in `CLAUDE_COLLABORATION.md` §2 en §5.
