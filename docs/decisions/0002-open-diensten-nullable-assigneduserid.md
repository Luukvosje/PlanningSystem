# 0002 — Open diensten: `AssignedUserId` nullable, geen apart statusveld

- **Datum:** 2026-08-16
- **Status:** definitief voor het datamodel
- **Raakt:** `Planning.Domain/Planning/PlanningRecord.cs`, de planning-API, de tijdlijn

## Besluit

`PlanningRecord.AssignedUserId` is `Guid?`. Een dienst zónder medewerker *ís* de open
dienst — geen aparte `OpenShift`-status en geen aparte tabel. `IsOpenShift` is een
gemaksproperty op basis van de null-check.

## Waarom

Het simpelste model dat het probleem oplost, en het past bij [0001](0001-product-scope-en-niet-doelen.md):
geen functie bouwen die niet bijdraagt aan sneller plannen.

## Alternatief overwogen

Een apart statuslabel voor dashboard-tellingen. Niet nodig — die telling loopt ook via de
null-check.

## Bewust niet

De "medewerker accepteert open dienst"-flow. Dat is een aparte actie en een apart endpoint;
zie [0010](0010-tijdlijn-is-plannertool.md) voor waarom die uitgesteld blijft.

## Stand van zaken (2026-09-14)

Gebouwd en op `main`. `IsOpenShift` staat in het domein, `OPEN_SHIFT_ROW_ID` en
`OPEN_SHIFT_SELECT_VALUE` in `Planning.Web/app/utils/planning/constants.ts`. De open vraag
over de vormgeving is beantwoord in [0007](0007-open-diensten-vormgeving.md).
