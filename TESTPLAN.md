# Handmatige testlijst — na de opruimronde

Niets hiervan is in een browser gedraaid. Deze lijst dekt wat er in fase 1 t/m 5 is gewijzigd,
gesorteerd op risico: **A** eerst, dat is waar een fout het meest kost.

Voorbereiding:

```bash
dotnet ef database update --project Planning.Infrastructure --startup-project Planning.Api
```

```bash
cd Planning.Web && npm run dev
```

Je hebt minimaal drie accounts nodig in één organisatie: een **Owner/Admin**, een **Planner** en
een **Employee**. Voor punt 2 en 12 heb je een tweede organisatie nodig.

---

## A. Hoogste risico — hier kan het stil fout gaan

### 1. Tijden staan op de juiste plek ⚠️ belangrijkste test

De backend stuurde tijdstempels zonder `Z`, waarna JavaScript ze als lokale tijd las. Dat is
opgelost in de database-laag, en de frontend-workaround die het maskeerde is verwijderd. Zit hier
een fout, dan schuift **alles** met je UTC-offset (nu 2 uur).

- [ ] Maak een boeking van **10:00 tot 11:00**. Sluit de sidebar. Staat het blok op 10:00?
- [ ] Herlaad de pagina (F5). Staat het er nog steeds op 10:00, niet op 12:00 of 08:00?
- [ ] Open de boeking opnieuw — staat er 10:00–11:00 in het formulier?
- [ ] Check de rode "nu"-lijn: staat die op de werkelijke huidige tijd?
- [ ] Maak een boeking rond **middernacht** (23:30–00:30) en kijk of die over de dagrand loopt
      zoals verwacht.
- [ ] Kijk in de netwerktab naar de respons van `GET /api/planning`. Eindigen `startUtc` en
      `endUtc` op een **`Z`**? Zo niet, stop en meld het.

### 2. Tenant-isolatie

- [ ] Log in bij organisatie A, kopieer een planning-id uit de URL of netwerktab.
- [ ] Wissel naar organisatie B en roep `GET /api/planning/{dat-id}` aan.
      Verwacht: **404**, met de tekst "Planningsregel niet gevonden" — niet 403, en zeker geen data.
- [ ] Idem voor een klant-id en een beschikbaarheidsregel-id.

### 3. Beschikbaarheid lekt niet meer

Dit was een echt lek: elke medewerker kon de vakantie- en ziekteredenen van alle collega's zien.

- [ ] Log in als **Employee**. Open de planning. Zie je in de netwerktab bij
      `GET /api/availability/rules/for-planning` alleen je **eigen** periodes?
- [ ] Als Employee: roep dat endpoint aan met `?EmployeeIds=<id-van-een-collega>`.
      Verwacht: **403**.
- [ ] Als Employee: met je **eigen** id → wel toegestaan.
- [ ] Log in als **Planner**. Zonder filter: zie je het hele team?

### 4. Modules aan/uit zetten blijft werken

Hier ging ik zelf bijna de fout in: een `AsNoTracking()` op de verkeerde query zou wijzigingen
zonder foutmelding hebben laten vallen.

- [ ] Als Owner/Admin: zet een module **uit** voor de organisatie. Bevestig. **Herlaad de pagina.**
      Staat hij nog uit?
- [ ] Zet hem weer aan. Herlaad. Staat hij aan?
- [ ] Idem per gebruiker, als je die UI kunt bereiken (die is grotendeels uitgeschakeld).

---

## B. Gewijzigd gedrag — dit hóórt anders te zijn dan voorheen

### 5. Statusovergangen worden nu bewaakt

- [ ] Zet een boeking op **Geannuleerd**. Probeer hem daarna op **Bevestigd** te zetten.
      Verwacht: geweigerd, met de Nederlandse melding
      *"Een afgeronde of geannuleerde boeking kan niet meer van status wijzigen."*
- [ ] Idem vanuit **Voltooid**.
- [ ] Gepland → Bevestigd: **moet gewoon werken**.
- [ ] Bevestigd → Voltooid: moet werken. Bevestigd → Geannuleerd: moet werken.
- [ ] **Let hier op:** wijzig van een boeking alleen de *titel* en sla op, zonder de status aan te
      raken. Dat moet slagen. Een status die gelijk blijft is geen overgang.
- [ ] Zet de app op Engels en herhaal de eerste stap — komt de melding dan in het Engels?

### 6. Openingstijden-venster is veranderd

Er stonden zes verschillende defaults in de code. Nu is er één: **de hele dag**. Voor een
organisatie **zonder** ingestelde openingstijden zag je eerst 07:00–19:00.

- [ ] Organisatie **zonder** openingstijden: de tijdlijn toont nu de volledige dag. Verwacht.
- [ ] Organisatie **mét** openingstijden (bijv. 08:00–18:00): venster volgt die instelling.
- [ ] Vind je de hele dag te breed als default? Zeg het — dan draaien we het terug.

### 7. Grijze arcering ligt op de blokken (A4)

De arcering "buiten openingstijden" werd met andere wiskunde getekend dan de blokken, dus die
liep bij de standaardinstellingen niet gelijk.

- [ ] Zet openingstijden op bijv. **09:00–17:00**. Standaardinstellingen laten staan
      (rij-layout *ruim*, "hele dag" **uit**).
- [ ] Begint de grijze band precies waar 09:00 op de tijdlijn ligt, en eindigt hij op 17:00?
- [ ] Maak een boeking van 08:00–10:00: valt de linkerhelft binnen de grijze zone en de
      rechterhelft erbuiten, netjes op de grens?
- [ ] Wissel de rij-layout naar **compact** en terug. Blijft de arcering kloppen?
- [ ] Zet "hele dag" **aan**: arcering verdwijnt of dekt de dag, en blokken blijven kloppen.

### 8. Overlap-markering

- [ ] Twee overlappende boekingen bij dezelfde medewerker → beide krijgen de rode rand.
- [ ] Twee boekingen die precies op elkaar aansluiten (10:00–11:00 en 11:00–12:00) → **geen** rand.
- [ ] Overlappende boekingen bij **verschillende** medewerkers → geen rand.
- [ ] **Nieuw gedrag:** annuleer een van twee overlappende boekingen. De rode rand op de
      andere moet **verdwijnen**.

### 9. Beschikbaarheidsregels: wie mag wat

- [ ] Als **Employee**: maak je eigen vakantie/ziekmelding aan → moet werken.
- [ ] Als **Employee**: probeer je eigen regel te **wijzigen** → geweigerd.
- [ ] Als **Employee**: probeer je eigen regel te **verwijderen** → geweigerd.
- [ ] Als **Planner**: wijzigen en verwijderen → moet werken.

*(Dit was jouw keuze "zelf aanmaken, planner verwijdert". Wijzigen valt daar bewust onder
verwijderen: anders kort je je eigen regel in tot een minuut en heb je hem effectief weggehaald.)*

### 10. Foutieve URL geeft 400, geen 500

- [ ] `GET /api/planning?StartUtc=2026-08-01T00:00:00Z&EndUtc=2026-08-31T00:00:00Z&UserIds=abc`
      → **400** met een veldfout op `UserIds`. Voorheen een 500.
- [ ] Idem met `&Statuses=Vergeten` → 400.

---

## C. Werkt alles nog — de hele planning-flow

De twee handgeschreven API-clients zijn verwijderd en vervangen door de gegenereerde client.
Elke actie hieronder loopt nu over een andere codepad dan voorheen.

### 11. Planning CRUD

- [ ] Boeking **aanmaken** via de sidebar
- [ ] Boeking **openen** en **wijzigen** (titel, omschrijving, notities, klant, medewerker, kleur)
- [ ] Boeking **verplaatsen** door te slepen naar een andere tijd
- [ ] Boeking **verplaatsen** naar een andere rij (andere medewerker)
- [ ] Boeking **verlengen/inkorten** aan de rand slepen
- [ ] Boeking **bevestigen**
- [ ] Boeking **dupliceren**
- [ ] Boeking **verwijderen**
- [ ] Na elke actie: verschijnt de wijziging direct, en blijft die staan na F5?

### 12. Rest van de app

- [ ] In-/uitloggen; laat een sessie lang genoeg open staan dat de token verlengd wordt
      (de refresh-call is bewust níet gemigreerd — controleer dat je niet uitgegooid wordt)
- [ ] Meerdere organisaties: de keuzelijst toont de **juiste namen** (die worden nu in één
      query opgehaald in plaats van één per lidmaatschap)
- [ ] Gebruikerslijst laadt en toont per gebruiker de juiste modules (idem, batch-query)
- [ ] Klanten: lijst, aanmaken, wijzigen, verwijderen, zoeken
- [ ] Gebruikers: lijst, rol wijzigen, zoeken
- [ ] Uitnodiging aanmaken en accepteren
- [ ] **Logo uploaden** bij organisatie-instellingen (nieuw codepad) — en verschijnt het daarna?
- [ ] Planning-instellingen opslaan (openingstijden + belangrijke werktijden)
- [ ] "Mijn planning"-weergave
- [ ] Dashboard: "volgende dienst"

### 13. Formulieren

`components/form/Control.vue` is herschreven — **de productie-build was hierdoor al kapot**
(`npm run build` faalde vóór deze ronde). Alle veldtypes moeten opnieuw langs.

- [ ] Tekstveld, e-mailveld, **wachtwoordveld**, tekstvlak, keuzelijst — typen en opslaan
- [ ] Validatiefouten verschijnen onder het juiste veld
- [ ] Aanmaak-modal en wijzig-modal openen, opslaan en sluiten (de interne registratie van
      formulieren is samengevoegd)

### 14. Vertalingen

13 hardgecodeerde Nederlandse teksten zijn naar i18n verplaatst.

- [ ] Beschikbaarheidsregel opslaan/wijzigen/verwijderen → toast in het **Nederlands**
- [ ] Zet de app op **Engels** → dezelfde toasts in het Engels
- [ ] Organisatie bijwerken, planning-instellingen opslaan, logo opslaan, profiel bijwerken → beide talen
- [ ] Navigeer naar een module waar je geen toegang tot hebt → "Geen toegang" / "No access"

---

## D. Wat weg is — hier hoort niets te missen

- [ ] Er was een "mixed"-tijdlijnweergave in de code. Die verwees naar niet-bestaande
      store-velden en werd nergens gerenderd. **Zie je nergens iets missen?**
- [ ] `GET /api/planning/week` is verwijderd; niets gebruikte het.
- [ ] Kleurkiezer bij een boeking: staan alle 10 kleuren er nog?

---

## Bekend en bewust niet opgelost

Geen bugs om te melden — dit weten we al:

| Wat | Waarom |
|---|---|
| **Er is geen uitlog-endpoint.** Refresh-tokens worden bij uitloggen nooit ingetrokken. | Buiten deze ronde gehouden. |
| Overlap wordt per query berekend, dus overlap over een **maandgrens** wordt niet gezien. | Echt oplossen vraagt een aparte query. |
| Tijdzone in **beschikbaarheid**: backend rekent in UTC-achtige tijd, frontend in lokale tijd. | Dit is A7 — die doen we samen. |
| Een dienst **over middernacht** mist de regels van de volgende dag. | Hoort bij A7. |
| Modulebeheer-UI is uitgeschakeld (uitgecommentarieerd). | Jouw keuze: onafgemaakt, niet dood. |
| `AvailabilityRuleStatus.Preferred` / `.Available` zijn onbereikbaar. | Laten staan, jouw keuze. |
| 10 typecheck-fouten in `npx nuxi typecheck`. | Allemaal van vóór deze ronde. |
| Veel `Missing semicolon`-lintfouten. | Bestaande stijlsplitsing in de codebase. |

## Nog te bespreken

- **A7** (tijdzone in beschikbaarheid) — samen
- `useCreate`/`useEdit` en de twee modals samenvoegen — bewust uitgesteld tot na het UI-gesprek
- `customers/index.vue` en `users/index.vue` zijn dezelfde pagina — idem
- Drie losse implementaties van hetzelfde opslaan/annuleren-knoppenpaar — idem
