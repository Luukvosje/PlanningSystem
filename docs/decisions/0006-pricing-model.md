# 0006 — Pricing: hybride seats plus pakketten

- **Datum:** 2026-08-16
- **Status:** voorlopig — het model staat, de bedragen niet
- **Raakt:** verdienmodel, [prd.md](../prd.md), [collaboration.md](../collaboration.md),
  [my-company.md](../../my-company.md)

## Besluit

Een hybride model. **Planner-seats zijn de kernprijs**; medewerker-seats zijn gratis tot een
grens en daarboven een klein bedrag per stuk. Daarnaast blijft het totaalpakket bestaan als
alles-in-één-optie voor wie niet met seats wil puzzelen.

| Onderdeel | Richting |
|---|---|
| Planner-seat — maakt en wijzigt roosters | hoofdprijs, hier zit de waarde |
| Medewerker-seat — ziet eigen rooster, geeft beschikbaarheid door | gratis tot een grens (10–15), daarna klein bedrag per stuk |
| Totaalpakket Starter / Groei / Premium | blijft bestaan als alles-in-één-optie |

De pakkettarieven zoals ze vandaag in de [PRD](../prd.md) staan:

| Pakket | Voor | Prijs |
|---|---|---|
| Starter | ±15 medewerkers | €19–29 per maand |
| Groei | meer medewerkers, meerdere planners | €49–69 per maand |
| Premium | meerdere vestigingen, uitgebreide functies | €99+ per maand |

## Waarom

De waarde zit bij de planner, niet bij de medewerker die alleen zijn eigen rooster bekijkt.
Een model dat per medewerker afrekent straft precies de groei die je wilt: een klant die er
vijf mensen bij neemt, gebruikt de tool niet vijf keer intensiever.

## Alternatief overwogen: credits

Credits per dienst of per actie. Verworpen.

Credits passen bij sporadisch gebruik — AI-beeldgeneratie bijvoorbeeld. Bij een planningstool
wil je juíst frequent gebruik; credits per actie zetten een prikkel neer om de tool *minder*
te gebruiken, precies tegen het doel in (minder WhatsApp, meer overzicht).

Voor latere, optionele extra's zijn credits wél denkbaar — niet voor het plannen zelf.

## Nog open

Geen van deze vier is vastgesteld:

- Exacte bedragen per planner-seat en per medewerker-seat.
- Vanaf welk aantal medewerkers de toeslag begint: 10 of 15?
- Blijven de pakkettarieven hierboven staan nu er een seat-model naast komt, of gaan ze mee
  omhoog?
- Toets bij een paar potentiële klanten of €19–29 leest als "te goedkoop om serieus te nemen"
  of als "eindelijk betaalbaar". Bij B2B-tools is ondergeprijsd vaker het probleem dan
  overgeprijsd.

Te beslissen vóór het eerste prijsgesprek met een klant — niet eerder, wel op tijd.

## Stand van zaken (2026-09-14)

Dit bestand is nu de enige plek waar het verdienmodel wordt uitgelegd. `collaboration.md`,
`my-company.md` en de `prd.md` verwijzen ernaar in plaats van het opnieuw te beschrijven; ze
zeiden alle drie iets anders, tot en met "nog niet bepaald" in `my-company.md`.

De PRD houdt de pakkettarieven, want dat is waar ze vandaan komen — maar noemt er nu bij dat
ze de pakket-helft van dit model zijn en niet het hele verhaal.
