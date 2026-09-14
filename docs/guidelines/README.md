# Guidelines

How to write code in this project. What the system *is* lives in
[../architecture/](../architecture/).

| Document | For | Skill |
|---|---|---|
| [api.md](api.md) | the five .NET projects | `backend-conventions` |
| [frontend.md](frontend.md) | `Planning.Web` — architecture and code | `frontend-conventions` |
| [design.md](design.md) | `Planning.Web` — visual language, page anatomy, shared components | `frontend-conventions` |

These documents are deliberately **not** in `CLAUDE.md`: together they are too large to load
every session, and a frontend session has no use for the backend half. `CLAUDE.md` carries
only the non-negotiables.

`api.md` and `frontend.md` each close with **Open questions** — places where the codebase
contradicts itself. Pick an existing variant there, do not invent a third, and settle it in
[../decisions/](../decisions/) rather than in that list.

A guideline that contradicts the code is worse than no guideline. If you deviate on purpose,
update the document in the same commit.
