# Documentatie

Ingedeeld naar het soort vraag dat een document beantwoordt. Eén onderwerp heeft één plek —
staat het ergens anders ook, dan is een van de twee fout.

| Map | Beantwoordt |
|---|---|
| [decisions/](decisions/) | "Waarom is dit zo?" — genomen keuzes, niet opnieuw ter discussie |
| [architecture/](architecture/) | "Hoe zit het in elkaar?" — projecten, tenancy, modules, productie |
| [guidelines/](guidelines/) | "Hoe schrijf ik hier code?" — conventies per laag |
| [references/](references/) | "Hoe heet dat ook alweer?" — configuratie, policies, foutcodes |
| [runbooks/](runbooks/) | "Hoe zet ik dit live, en wat doe ik als het stukgaat?" |
| [help/](help/) | "Hoe gebruik ik dit?" — voor de klant, in het Nederlands |
| [plans/](plans/) | "Wat gaan we bouwen en wat raakt het?" — werk dat nog niet bestaat |

Losse bestanden:

| Bestand | Beantwoordt |
|---|---|
| [prd.md](prd.md) | Wat is het product? De MVP-specificatie |
| [collaboration.md](collaboration.md) | Hoe werken Luuk en Claude samen, buiten code om? |
| [testplan.md](testplan.md) | Wat moet ik met de hand nalopen? |
| [../my-company.md](../my-company.md) | Waar werken we deze week aan? Bedrijfskern en weekfocus |
| [../marketing.md](../marketing.md) | Positionering, kanalen, budget |

## Waar begin je

- **Iets aan de API doen** → [architecture/multi-tenancy.md](architecture/multi-tenancy.md),
  daarna [guidelines/api.md](guidelines/api.md)
- **Iets aan de frontend doen** → [guidelines/frontend.md](guidelines/frontend.md), en
  [guidelines/design.md](guidelines/design.md) als het zichtbaar is
- **Iets wat willekeurig lijkt veranderen** → eerst [decisions/](decisions/)
- **Productie** → [runbooks/](runbooks/)

## Regels

- **Een besluit hoort in [decisions/](decisions/).** Sta je op het punt ergens anders "we
  besloten om…" te schrijven, schrijf het daar. Die index wordt bij elke sessiestart
  ingeladen, dus hij moet kort blijven.
- **Een open vraag hoort in het bijbehorende plan** onder [plans/](plans/), niet in
  `decisions/`. Zodra er een keuze is, verhuist hij.
- **Een document dat de code tegenspreekt is erger dan geen document.** Wijk je bewust af,
  pas het dan aan in dezelfde commit. Haalt de code een besluit in, zet er dan een
  `## Stand van zaken (datum)` onder in plaats van de geschiedenis te herschrijven.
- **Nederlands waar het over het product of het bedrijf gaat, Engels waar het over de code
  gaat.** `help/` is altijd Nederlands. Code, commits en identifiers altijd Engels.
