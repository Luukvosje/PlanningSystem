# Planning Web

Type-safe Nuxt 4 frontend voor de Planning API, gegenereerd via Orval.

## Vereisten

- Node.js 20+
- Planning API draaiend op `http://localhost:5264`

## Setup

```bash
cd planning-web
npm install
cp .env.example .env
```

## API client genereren

Zorg dat de backend draait, daarna:

```bash
npm run generate:api
```

Dit leest `http://localhost:5264/swagger/v1/swagger.json` en schrijft naar `src/generated/`.

**Regel:** wijzig nooit handmatig bestanden in `src/generated/`. Pas C# DTO's aan en regenereer.

## Development

```bash
# Terminal 1 — backend
dotnet run --project ../Planning.Api

# Terminal 2 — frontend
npm run dev
```

Open `http://localhost:3000`, log in via `/login` (dev JWT endpoint).

## Architectuur

| Laag | Map |
|------|-----|
| Gegenereerde API (infra) | `src/generated/` |
| Use cases | `src/composables/api/` |
| Query hooks | `src/composables/queries/` |
| Auth state | `src/stores/` |
| HTTP + errors | `src/utils/` |

## Scripts

| Script | Beschrijving |
|--------|-------------|
| `npm run dev` | Nuxt dev server |
| `npm run build` | Productie build |
| `npm run generate:api` | Orval codegen vanuit Swagger |
| `npm run generate:api:watch` | Orval watch mode |
