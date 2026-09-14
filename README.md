# Planning SaaS

Multi-tenant Planning SaaS foundation built with .NET 10 and Clean Architecture.

## Solution structure

```
Planning/
├── Planning.Api/              # Controllers, middleware, JWT, Swagger
├── Planning.Application/      # DTOs, application services, validators, Result pattern
├── Planning.Domain/           # Entities, enums, repository interfaces
└── Planning.Infrastructure/   # EF Core, repositories, PostgreSQL
```

## Architecture flow

```
API → Application Services → Domain ← Infrastructure
```

- No CQRS, MediatR, or event sourcing
- Application services orchestrate use cases
- Repository interfaces live in the Domain layer
- EF Core implementations live in Infrastructure

## Getting started

### Prerequisites

- .NET 10 SDK
- PostgreSQL 17 (`docker compose -f docker-compose.dev.yml up -d` starts one on 127.0.0.1:5432)

### Secrets (local development)

The JWT signing key is **not** committed. Set it once via `dotnet user-secrets` from
`Planning.Api`:

```bash
dotnet user-secrets set "Jwt:Key" "<a long random string, 32+ chars>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=PlanningDb;Username=planning;Password=planning_dev"
```

This applies to both the `Development` and `Test` environments. In CI/production, set the
`Jwt__Key` environment variable instead (double underscore is the .NET config separator).
Never add `Jwt:Key` back to `appsettings*.json`.

### Database migration

```bash
dotnet ef database update --project Planning.Infrastructure --startup-project Planning.Api
```

### Run

```bash
dotnet run --project Planning.Api
```

Open Swagger at `/swagger`.

### Authentication

1. Register an account via `POST /api/auth/register` (anonymous)
2. Create an organization via `POST /api/organizations`
3. Sign in via `POST /api/auth/login` and authorize in Swagger with `Bearer {accessToken}`

`EXAMPLES.md` still documents the removed `POST /api/auth/token` development endpoint and
needs a rewrite.

## Domain model

| Entity          | Description                                      |
|-----------------|--------------------------------------------------|
| Organization    | Tenant root aggregate                            |
| User            | Organization member with role                    |
| Customer        | Organization customer                            |
| PlanningRecord  | Scheduled work item assigned to a user          |

All tenant entities include `OrganizationId`, `CreatedAtUtc`, and `UpdatedAtUtc`.

## API endpoints

| Resource       | Methods                                                        |
|----------------|----------------------------------------------------------------|
| Organizations  | POST, GET by id                                                |
| Users          | POST, GET by id, GET by organization                           |
| Customers      | POST, PUT, DELETE, GET by id, GET by organization              |
| Planning       | POST, PUT, DELETE, GET by id, GET week planning                |
