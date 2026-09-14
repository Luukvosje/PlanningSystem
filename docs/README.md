# Documentation

Organised by the kind of question a document answers. One subject has one home — if it is
written down in two places, one of them is wrong.

| Folder | Answers |
|---|---|
| [decisions/](decisions/) | "Why is it like this?" — choices already made, not up for re-litigation |
| [architecture/](architecture/) | "How does it fit together?" — projects, tenancy, modules, production |
| [guidelines/](guidelines/) | "How do I write code here?" — conventions per layer |
| [references/](references/) | "What was that called again?" — configuration, policies, error codes |
| [runbooks/](runbooks/) | "How do I ship this, and what do I do when it breaks?" |
| [help/](help/) | "How do I use this?" — for the customer, in Dutch |
| [plans/](plans/) | "What are we building and what does it touch?" — work that does not exist yet |

Loose files:

| File | Answers |
|---|---|
| [prd.md](prd.md) | What is the product? The MVP specification |
| [collaboration.md](collaboration.md) | How do Luuk and Claude work together, outside of code? |
| [testplan.md](testplan.md) | What do I have to check by hand? |
| [../my-company.md](../my-company.md) | What are we working on this week? Company core and weekly focus |
| [../marketing.md](../marketing.md) | Positioning, channels, budget |

## Where to start

- **Touching the API** → [architecture/multi-tenancy.md](architecture/multi-tenancy.md), then
  [guidelines/api.md](guidelines/api.md)
- **Touching the frontend** → [guidelines/frontend.md](guidelines/frontend.md), plus
  [guidelines/design.md](guidelines/design.md) if it is visible
- **Changing something that looks arbitrary** → [decisions/](decisions/) first
- **Production** → [runbooks/](runbooks/)

## Rules

- **A decision belongs in [decisions/](decisions/).** If you are about to write "we decided
  to…" anywhere else, write it there instead. That index is loaded at every session start, so
  it has to stay short.
- **An undecided question belongs in its plan** under [plans/](plans/), not in `decisions/`.
  Once there is a choice, it moves.
- **A document that contradicts the code is worse than no document.** If you deviate on
  purpose, update it in the same commit. When the code overtakes a decision, add a
  `## Where this stands (date)` section instead of rewriting history.
- **Everything is written in English, except `docs/help/`**, which is customer-facing and
  therefore Dutch — including its README. Code, commits and identifiers are always English.
