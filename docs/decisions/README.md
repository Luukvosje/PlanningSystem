# Decisions

Choices already made, so a new session does not re-run the same discussion. **Do not deviate
from these without explicitly revising the decision** — and revising means editing the file,
not working around it.

This index is loaded at every session start (`SessionStart` hook in `.claude/settings.json`).
Keep it short; the reasoning lives in the individual files.

| # | Decision | Status |
|---|---|---|
| [0001](0001-product-scope-and-non-goals.md) | The MVP does no payroll, leave, invoicing, HR or AI planning. Test every feature against: planning faster, fewer mistakes, better overview, less WhatsApp | final |
| [0002](0002-open-shifts-nullable-assigneduserid.md) | An open shift is `AssignedUserId` being null. No separate status or table | final, built |
| [0003](0003-sharing-a-planning-frontend-only.md) | Sharing a planning is frontend only: text format + clipboard + `wa.me`. No API change, no PDF for now | provisional, **not built** |
| [0004](0004-shift-notes-no-build-needed.md) | Notes per shift already exist — test the UX, build nothing | final |
| [0005](0005-marketing-channel-meta-ads.md) | First channel is narrowly targeted Meta Ads, €10–20/day, video | first direction |
| [0006](0006-pricing-model.md) | Hybrid: planner seats as the core price, employees free up to a limit, packages alongside. No credits | provisional, amounts open |
| [0007](0007-open-shifts-visual-design.md) | An open shift is a dashed border plus an i18n label. The API sends `assignedUserName: null` | final, built |
| [0008](0008-hosting-vps-docker-compose.md) | A VPS with Docker Compose and Caddy, not Azure. The database is PostgreSQL | final, built |
| [0009](0009-no-ai-companion.md) | No LLM in the planning flow. Conflict detection stays deterministic rules | final |
| [0010](0010-timeline-is-a-planner-tool.md) | `/timeline` requires a planner role; an employee sees only their own week | final, built |
| [0012](0012-members-exist-before-accounts.md) | A team member can exist before their account: `User.AccountId` is nullable, `POST /api/users` adds one by name, an invite can target them and links the login on acceptance | final, built |

## Still open — no decision here yet

- Exact pricing amounts, the employee limit (10 or 15), and whether the package rates stay as
  they are now that a seat model sits beside them. See [0006](0006-pricing-model.md).
- Text format for "sharing a planning": grouped per employee or per day?
- Brand name, domain name, tone of voice, and a competitor comparison for marketing copy.

## Adding a decision

A new file `NNNN-short-title.md`, numbered upwards, with this header:

```markdown
# NNNN — Title

- **Date:** YYYY-MM-DD
- **Status:** final | provisional
- **Touches:** which files or parts

## Decision
## Why
## Alternative considered
```

Then one row in the table above. A decision the code has overtaken gets a
`## Where this stands (date)` section rather than a rewritten history — what you decided then
stays, what has happened since goes underneath.

A question that is not yet decided belongs **not** here but in the matching plan under
[../plans/](../plans/), until there is a choice.
