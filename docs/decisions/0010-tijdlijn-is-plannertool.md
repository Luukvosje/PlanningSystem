# 0010 — Tijdlijn is een plannertool; de medewerker ziet alleen zijn eigen week

- **Datum:** 2026-08-22
- **Status:** definitief voor de MVP
- **Raakt:** `Planning.Web/app/utils/modules.ts`, `middleware/module.global.ts`

## Besluit

`/timeline` — de organisatiebrede planning — vereist een plannerrol. Weg uit het menu voor
medewerkers én geblokkeerd in `module.global.ts`, met een redirect naar `/planning`. De
medewerker houdt zijn eigen weekoverzicht.

Open diensten kunnen dus alleen door een planner gevuld worden; de "medewerker claimt open
dienst"-flow blijft uitgesteld.

## Waarom

Een medewerker heeft geen reden om te zien wanneer collega's werken. Alleen het menu-item
weghalen is beveiliging via het menu — de URL bleef open. `/timeline` stond bovendien
helemaal niet in `ROUTE_MODULE_MAP`, dus zelfs zónder de Planning-module was de pagina
bereikbaar. Dat is meteen rechtgezet.

## Alternatief overwogen

Open diensten wél tonen in het persoonlijke weekoverzicht, met "meld je bij je planner".
Bewaard voor later; tot die tijd loopt die communicatie via de gedeelde weekplanning
(WhatsApp).

## Waarom de claim-flow wacht

Die kost een nieuw endpoint, het eerste mutatierecht voor medewerkers, een race-regel én
notificaties (SMTP hangt aan hosting). Pas zinvol als een betalende klant erom vraagt.

## Stand van zaken (2026-09-14)

Gebouwd. `PLANNER_ONLY_ROUTE_PREFIXES = ['/timeline']` en `/timeline` staat in
`ROUTE_MODULE_MAP` onder de Planning-module.
