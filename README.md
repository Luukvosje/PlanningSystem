# Planning SaaS

Multi-tenant Planning SaaS foundation built with .NET 10 and Clean Architecture.

## Solution structure

```
Planning/
├── Planning.Api/              # Controllers, middleware, JWT, Swagger
├── Planning.Application/      # DTOs, application services, validators, Result pattern
├── Planning.Domain/           # Entities, enums, repository interfaces
└── Planning.Infrastructure/   # EF Core, repositories, SQL Server
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
- SQL Server or LocalDB

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

1. Create an organization via `POST /api/organizations` (anonymous)
2. Request a development JWT via `POST /api/auth/token`
3. Authorize in Swagger with `Bearer {token}`

See [EXAMPLES.md](./EXAMPLES.md) for full request/response samples.

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
