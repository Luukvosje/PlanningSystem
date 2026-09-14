# .claude

Claude Code configuration for this repository. Everything here is tooling — the documentation
itself lives in [`docs/`](../docs/README.md) and the rules in [`CLAUDE.md`](../CLAUDE.md).

| File | What it does |
|---|---|
| `settings.json` | shared project settings: allowed and denied commands, MCP servers, and the `SessionStart` hook. Checked in. |
| `settings.local.json` | your personal overrides. **Not** checked in, not to be shared — it holds session-specific tokens. |
| `launch.json` | dev server for the Browser pane (`npm run dev` in `Planning.Web`, port 3000). There is a second one in `Planning.Web/.claude/` on port 3100. |
| `skills/` | procedures Claude can load instead of deriving them again. |

## Skills

| Skill | When |
|---|---|
| `backend-conventions` | before writing C# — loads `docs/guidelines/api.md` |
| `frontend-conventions` | before writing Vue or TypeScript — loads `docs/guidelines/frontend.md` and `design.md` |
| `tenant-endpoint` | a new or changed endpoint on tenant data, from domain method to orval |
| `db-schema-change` | an entity or column change plus its EF migration |
| `form-card` | an editable entity card: schema → `use*Edit` → `FormEditableSection` |
| `query-slice` | pulling an endpoint into the frontend: `use*Api` → `queryKeys` → `useQuery` → invalidation |

Invoke with `/backend-conventions`, `/tenant-endpoint`, and so on. Claude also picks them up on
its own when the description matches the task.

A skill is one folder with a `SKILL.md` carrying frontmatter (`name`, `description`). The
`description` decides when Claude reaches for it — write it in terms of the task, not the
contents.

## The SessionStart hook

`settings.json` carries one hook. At every session start `docs/decisions/README.md` is loaded
into context, so decisions already made are present without anyone having to remember them:

```
node -e "try{process.stdout.write(require('fs').readFileSync('docs/decisions/README.md','utf8'))}catch(e){}"
```

It is `node -e` rather than `cat` because `cat` does not exist under `cmd.exe`, and there is no
guarantee which shell runs a hook on Windows — this command was tested in Git Bash, PowerShell
and cmd. The `try/catch` means a missing file degrades to silence instead of tripping the
session.

That is also why the index has to stay **short**: it costs context at every start. The
reasoning belongs in the individual decision files, not in the table.

## What does not belong here

Plans, decisions and specifications. Those live in `docs/`; they are not for Claude alone and
should not be hidden in a tooling folder.
