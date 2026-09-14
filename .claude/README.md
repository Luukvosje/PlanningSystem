# .claude

Claude Code configuration for this repository. Everything here is tooling — the actual
documentation lives in [`docs/`](../docs/README.md) and the rules in
[`CLAUDE.md`](../CLAUDE.md).

| Bestand | Wat het doet |
|---|---|
| `settings.json` | gedeelde projectinstellingen: toegestane en geweigerde commando's, MCP-servers, en de `SessionStart`-hook. Ingecheckt. |
| `settings.local.json` | jouw persoonlijke overrides. **Niet** ingecheckt, niet delen — er staan sessiespecifieke tokens in. |
| `launch.json` | dev-server voor de Browser-pane (`npm run dev` in `Planning.Web`, poort 3000). Er staat een tweede in `Planning.Web/.claude/` op poort 3100. |
| `skills/` | procedures die Claude kan laden in plaats van ze opnieuw af te leiden. |

## Skills

| Skill | Wanneer |
|---|---|
| `backend-conventions` | vóór het schrijven van C# — laadt `docs/guidelines/api.md` |
| `frontend-conventions` | vóór het schrijven van Vue/TS — laadt `docs/guidelines/frontend.md` en `design.md` |
| `tenant-endpoint` | nieuw of gewijzigd endpoint op tenant-data, van domain tot orval |
| `db-schema-change` | entity- of kolomwijziging plus de EF-migratie |
| `form-card` | bewerkbare entiteitskaart: schema → `use*Edit` → `FormEditableSection` |
| `query-slice` | endpoint naar de frontend trekken: `use*Api` → `queryKeys` → `useQuery` → invalidatie |

Aanroepen met `/backend-conventions`, `/tenant-endpoint`, enzovoort. Claude pakt ze ook zelf
op wanneer de beschrijving bij de taak past.

Een skill is één map met een `SKILL.md` met frontmatter (`name`, `description`). De
`description` bepaalt wanneer Claude de skill oppakt — schrijf hem in termen van de taak, niet
van de inhoud.

## De SessionStart-hook

`settings.json` bevat één hook. Bij elke sessiestart wordt `docs/decisions/README.md` in de
context geladen, zodat genomen besluiten er zijn zonder dat iemand eraan hoeft te denken:

```
node -e "try{process.stdout.write(require('fs').readFileSync('docs/decisions/README.md','utf8'))}catch(e){}"
```

Het is `node -e` en geen `cat`, omdat `cat` niet bestaat onder `cmd.exe` — dit commando is in
Git Bash, PowerShell en cmd getest. De `try/catch` zorgt dat een ontbrekend bestand de sessie
niet laat struikelen.

Daarom moet die index **kort** blijven: hij kost context bij elke start. De motivatie hoort in
de losse besluitbestanden, niet in de tabel.

## Wat hier niet hoort

Plannen, besluiten en specificaties. Die staan in `docs/`; ze zijn niet voor Claude alleen en
horen niet in een tooling-map te verstoppen.
