# 0009 — Geen AI-companion in de planning

- **Datum:** 2026-08-22
- **Status:** definitief voor de MVP
- **Raakt:** de planningsflow, conflictsignalering

## Besluit

Het niet-doel "AI-planning" uit [0001](0001-product-scope-en-niet-doelen.md) blijft staan.
Geen chat-companion, geen LLM in de planningsflow. Conflictsignalering en "wie kan deze open
dienst doen" blijven deterministische regels over `AvailabilityRule` plus overlap-detectie.

## Waarom

Die vragen zijn regels, geen taalmodellen — deterministisch, gratis en uitlegbaar aan de
planner. Een LLM zou personeelsdata (namen, roosters, afwezigheid) naar een derde partij
sturen, wat per klant een verwerkersovereenkomst kost, en kost geld per request terwijl er
nog geen verdienmodel is. Een chatvenster erbij is bovendien precies wat de logge concurrenten
doen.

## Alternatief overwogen

Smalle natural-language invoer ("Kevin ma 8-16" → dienst). Niet verworpen, wel uitgesteld tot
ná de eerste betalende klant. Dat is één endpoint, geen laag.
