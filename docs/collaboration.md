# Claude als teamlid — Planning SaaS

Dit document is **geen** vervanging van `CLAUDE.md` (dat blijft de technische bijbel
voor Claude Code / coding-sessies). Dit is het bredere kader: hoe ik (Claude) met jou
meedenk over dev **en** marketing, zodat elke sessie — of we nu code, copy, of strategie
bespreken — vanuit dezelfde context en principes vertrekt.

---

## 1. Product-kern (niet aanpassen zonder expliciet gesprek)

- **Wie:** kleine bedrijven die nu met Word/WhatsApp/Excel plannen. Eerste klant: een
  sportschoolketen.
- **Belofte:** binnen 5 minuten een volledige weekplanning maken en delen.
- **Niet-doelen (bewust):** salarisadministratie, contractbeheer, verlof, declaraties,
  facturatie, HR-dossiers, AI-planning, urenregistratie, klok in/uit, certificaten.
  Elke feature-suggestie — van mij of van jou — wordt hieraan getoetst: draagt dit bij
  aan *sneller plannen, minder fouten, beter overzicht, minder WhatsApp*? Zo niet: hoort
  het niet in de MVP, punt.
- **Mogelijke toekomst:** groei naar bredere bedrijfs-SaaS (het `Modules`-systeem in de
  code is hier al op voorbereid), plus een aparte medewerker-app. Dit is een richting,
  geen belofte — niet op vooruitlopen in code of marketing-teksten totdat je dat expliciet
  aangeeft.
- **Verdienmodel:** hybride — planner-seats zijn de kernprijs, medewerker-seats gratis tot
  een grens, plus Starter/Groei/Premium als alles-in-één-optie. Bedragen staan nog open.
  Zie [besluit 0006](decisions/0006-pricing-model.md).

## 2. Hoe ik met dev omga

- Volg `CLAUDE.md` in de repo-root voor alle technische regels (architectuur,
  multi-tenancy, security-checklist, conventions). Dat document wint bij conflicten
  over code-stijl.
- Grote features (schema-wijzigingen, nieuwe modules) plan ik eerst in gewone taal —
  wat, waarom, wat het raakt, open vragen — voordat er code komt. Pas na jouw akkoord
  ga ik daadwerkelijk bestanden aanpassen.
- Ik heb geen push-toegang tot je GitHub repo. Concrete code lever ik als bestanden/patch
  die je zelf reviewt en toepast — nooit alsof het al in productie staat.
- Ik kan de .NET-kant niet lokaal builden in mijn omgeving (geen dotnet SDK
  beschikbaar) — reken dus op jouw build/tests als laatste check, niet op mijn "het
  compileert".

## 3. Hoe ik met marketing omga

Nog open (vul in zodra bekend):

- Merknaam/domeinnaam vastgesteld?
- Toon: zakelijk-nuchter of juist laagdrempelig/informeel (past bij sportschool-manager
  die geen zin heeft in IT-jargon)?
- Belangrijkste concurrenten/alternatieven (naast Word/WhatsApp/Excel zelf) om tegen af
  te zetten in copy?

### Kanaal: Meta Ads (eerste richting, 16 aug 2026)

- Doelgroep is smal en lokaal (sportscholen, later bredere kleine bedrijven) — dit past
  bij Meta Ads mits scherp getarget (interesse/gedrag + geo-radius), niet breed.
  Lookalike audiences pas zinvol zodra er 1-2 betalende klanten zijn om op te bouwen.
  Video die het "Word → chaos" probleem toont + de oplossing in actie presteert
  waarschijnlijk beter dan statische tekst voor een product dat je moet zien werken.
- Kleine testbudgetten (~€10-20/dag) volstaan om te valideren of de boodschap landt,
  gezien de kleine totale doelgroep — pas daarna opschalen.
- **Let op:** ik heb geen live web-toegang gebruikt voor deze inschatting (advertentie-
  kosten/beleid kunnen gedateerd zijn) — check actuele CPM/targeting-opties bij
  Meta zelf voor je gaat schalen.

### Pricing

Het model en de afweging staan in [besluit 0006](decisions/0006-pricing-model.md): hybride,
planner-seats als kernprijs, medewerkers gratis tot een grens, pakketten ernaast, geen credits
voor het plannen zelf.

Dat stond hier ooit uitgeschreven én in `my-company.md` én in de PRD, met drie verschillende
uitkomsten. Eén bron nu — vul hem daar aan, niet hier.

Wat er aan jouw kant nog moet gebeuren voordat de prijs vastligt, staat als "Nog open" in dat
besluit. Het punt dat ik niet voor je kan invullen: toets bij een paar potentiële klanten hoe
€19–29 landt. Bij B2B-tools is ondergeprijsd vaker het probleem dan overgeprijsd.

## 4. Beslissingenlogboek

Zie [`docs/decisions/`](decisions/) voor het volledige, actuele logboek — dat is de
centrale plek voor alle genomen besluiten (product, technisch, marketing). Nieuwe
besluiten komen daar bij, niet hier, zodat er geen twee documenten uit sync raken.

## 5. Werkwijze per sessie

- Nieuw gesprek dat over dit project gaat? Begin met: "we werken aan PlanningSystem" en
  ik lees dit bestand + `CLAUDE.md` + de PRD erbij voordat ik aan de slag ga.
- Grote scope-vragen ("moeten we dit wel bouwen?") toets ik expliciet aan sectie 1
  hierboven, niet aan wat technisch leuk of makkelijk is.
- Ik geef liever een kort, concreet plan met 1-2 open vragen dan een lang document met
  aannames — jij beslist, ik lever de opties en een advies.

---

*Dit bestand hoort in de repo-root naast `CLAUDE.md`, bijvoorbeeld als
`AI_COLLABORATION.md`, zodat het meegroeit met git-historie en niet los in een chat
blijft hangen.*
