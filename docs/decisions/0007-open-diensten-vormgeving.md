# 0007 — Open diensten: vormgeving en unassign-flow

- **Datum:** 2026-08-22
- **Status:** definitief, gebouwd
- **Raakt:** `Planning.Web/app/components/planning/timeline/Block.vue`, de planning-API

## Besluit

Een open dienst krijgt geen eigen kleur maar de omtrek-stijl met een **gestreepte rand** (de
kleur van de dienst blijft de rand), plus het label "Open dienst" in het blok.

In medewerker-modus staat bovenaan een rij "Open diensten" zodra er minstens één is. Een
dienst naar die rij slepen haalt de medewerker eraf; eruit slepen wijst iemand toe. In het
formulier is "Open dienst" de eerste optie in de medewerker-keuzelijst.

De API stuurt `assignedUserName: null`, niet de tekst "Open dienst" — afwijking van
[plans/mvp-gaps.md](../plans/mvp-gaps.md) §1. Het label komt uit i18n, zodat de backend geen
UI-tekst bevat.

## Waarom

De gestreepte rand leest als "nog niet ingevuld" zonder een kleur te claimen die ook een
echte dienstkleur kan zijn. De rij spiegelt de bestaande "Zonder klant"-rij, dus er is geen
nieuw patroon om uit te leggen.

## Alternatief overwogen

Een grijs vlak. Verworpen: grijs is al in gebruik voor concept-diensten
(`CONCEPT_BLOCK_STYLE`).

## Stand van zaken (2026-09-14)

Op `main`. `Block.vue` zet `border-dashed` op `isOpenShift` en toont `t('planning.openShift')`
zodra het blok breder is dan 80px.
