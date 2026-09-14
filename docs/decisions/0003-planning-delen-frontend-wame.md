# 0003 — Planning delen: puur frontend, `wa.me`-link

- **Datum:** 2026-08-16
- **Status:** voorlopig — nog niet gebouwd
- **Raakt:** `Planning.Web/app/utils/planning/`, `app/components/planning/header/`

## Besluit

Geen backend-wijziging. Een tekstformat-functie zet de al geladen weekplanning om naar
leesbare tekst, een "kopiëren"-knop gebruikt de clipboard-API, en delen gaat via
`https://wa.me/?text=<url-encoded tekst>`. PDF-export komt expliciet later.

## Waarom

De weekplanning-data is al beschikbaar via de bestaande query-composables — geen reden om de
API uit te breiden voor een puur presentatieprobleem. `wa.me` werkt op desktop (WhatsApp Web)
én mobiel zonder extra dependency.

## Alternatief overwogen

PDF-export nu al meenemen. Verworpen: staat expliciet als *later* in de [PRD](../prd.md).

## Nog open

Tekst per medewerker gegroepeerd, of per dag met alle medewerkers onder elkaar? Zie
[plans/mvp-gaps.md](../plans/mvp-gaps.md) §2.

## Stand van zaken (2026-09-14)

Nog niets van gebouwd. Er is geen `formatWeekPlanningText.ts` en geen clipboard- of
`wa.me`-code in `Planning.Web`.
