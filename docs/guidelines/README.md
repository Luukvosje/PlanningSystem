# Guidelines

Hoe je in dit project code schrijft. Wat het systeem ís staat in
[../architecture/](../architecture/).

| Document | Voor | Skill |
|---|---|---|
| [api.md](api.md) | de vijf .NET-projecten | `backend-conventions` |
| [frontend.md](frontend.md) | `Planning.Web` — architectuur en code | `frontend-conventions` |
| [design.md](design.md) | `Planning.Web` — ontwerptaal, pagina-anatomie, gedeelde componenten | `frontend-conventions` |

Deze documenten staan bewust **niet** in `CLAUDE.md`: samen zijn ze te groot om elke sessie mee
te laden, en een frontend-sessie heeft niets aan de backend-helft. `CLAUDE.md` houdt alleen de
non-negotiables.

`api.md` en `frontend.md` sluiten elk af met **Open questions** — plekken waar de codebase
zichzelf tegenspreekt. Kies daar een bestaande variant, verzin geen derde, en beslecht het in
[../decisions/](../decisions/) in plaats van in dat lijstje.

Een guideline die de code tegenspreekt is erger dan geen guideline. Wijk je bewust af, pas dan
het document aan in dezelfde commit.
