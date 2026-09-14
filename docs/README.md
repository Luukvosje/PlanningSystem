# Documentatie

Ingedeeld naar het soort vraag dat een document beantwoordt. Eén onderwerp heeft één plek —
staat het ergens anders ook, dan is een van de twee fout.

| Map / bestand | Beantwoordt |
|---|---|
| [decisions.md](decisions.md) | "Waarom is dit zo?" — het beslissingenlogboek, nieuwste bovenaan |
| [guidelines/](guidelines/) | "Hoe schrijf ik hier code?" — conventies per laag |
| [plans/](plans/) | "Wat gaan we bouwen en wat raakt het?" — werk dat nog niet bestaat |
| [prd.md](prd.md) | "Wat is het product?" — de MVP-specificatie |
| [collaboration.md](collaboration.md) | "Hoe werken Luuk en Claude samen?" — buiten code om |
| [testplan.md](testplan.md) | "Wat moet ik met de hand nalopen?" |
| `../deploy/VPS-SETUP.md` | "Hoe zet ik dit live?" — het draaiboek voor de eerste deploy |
| `../my-company.md` | "Waar werken we deze week aan?" — bedrijfskern en weekfocus |
| `../marketing.md` | Positionering, kanalen, budget |

## Guidelines

| Document | Waarvoor | Skill |
|---|---|---|
| [guidelines/api.md](guidelines/api.md) | De vijf .NET-projecten | `backend-conventions` |
| [guidelines/frontend.md](guidelines/frontend.md) | `Planning.Web` — architectuur en code | `frontend-conventions` |
| [guidelines/design.md](guidelines/design.md) | `Planning.Web` — ontwerptaal, pagina-anatomie | `frontend-conventions` |

Deze documenten staan bewust **niet** in `CLAUDE.md`: samen zijn ze te groot om elke sessie
mee te laden, en een frontend-sessie heeft niets aan de backend-helft. `CLAUDE.md` houdt
alleen de non-negotiables.

## Regels

- **Een besluit hoort in `decisions.md`.** Sta je op het punt ergens anders "we besloten
  om…" te schrijven, schrijf het daar. Lees het voordat je iets verandert dat willekeurig
  lijkt — dat is het waarschijnlijk niet.
- **Een open vraag hoort in het bijbehorende plan** onder `plans/`, niet in `decisions.md`.
  Zodra er een keuze is, verhuist hij.
- **Een guideline die de code tegenspreekt is erger dan geen guideline.** Wijk je bewust af,
  pas dan het document aan in dezelfde commit.
- Productdocumentatie is Nederlands, codedocumentatie Engels. Code, commits en identifiers
  altijd Engels.
