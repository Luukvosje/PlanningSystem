---
name: db-schema-change
description: Change the database schema — add or modify an entity, a property, a column, an index or a relationship — and produce the EF Core migration. Use whenever a domain entity gains or loses a field, a new table is needed, or a column's type, length or nullability changes. Covers the migrate-on-boot behaviour that makes a destructive migration ship silently.
---

# Changing the schema

## Before you start: this ships itself

`Planning.Api/Program.cs` applies pending migrations **on boot**, in every environment except
`Test`. So a migration merged to `main` is applied by the next deploy with nobody running a
command.

That makes three things true:

- A destructive migration — dropping a column, retyping one, narrowing a length, making a
  nullable column required — **destroys production data silently**. Ask before writing one.
- It is safe only because exactly one API instance runs. A second instance races this.
- Running `dotnet ef database update` against anything but the local dev database needs to be
  asked about too.

If the change is destructive, propose the two-step alternative: ship the additive migration
and the code that writes both shapes first, backfill, drop the old column in a later deploy.

## 1. Domain entity

`Planning.Domain/<Feature>/<Entity>.cs`.

- Private setter, set from the private constructor and from the mutator methods.
- A new required field is a new parameter on `Create(...)` and on `Update(...)`, plus a
  `Validate*` that throws `ArgumentException`.
- No EF Core attributes on the entity — the mapping lives in Infrastructure.
- Derived state is a computed property, not a column.

## 2. Entity configuration

`Planning.Infrastructure/<Feature>/<Entity>Configuration.cs`.

- `IsRequired()` / `HasMaxLength(n)` matching the FluentValidation rule on the request DTO.
  If they disagree, the database wins at 500 instead of the validator winning at 400.
- Index anything you filter on. Tenant entities always have an index on `OrganizationId`.
- Relationship + `OnDelete(...)`. `Cascade` to `Organization`, `Restrict` for references that
  should block a delete.
- A new entity needs a `DbSet<T>` on `ApplicationDbContext`; configurations themselves are
  discovered automatically.
- A value object stored as JSON goes through `JsonColumnConverter`; dates go through
  `UtcDateTimeConverter` — everything is UTC in the backend, the frontend converts for display.

## 3. Migration

```bash
docker compose -f docker-compose.dev.yml up -d
dotnet ef migrations add <DescriptiveName> --project Planning.Infrastructure --startup-project Planning.Api
```

**Read the generated `Up` and `Down` before doing anything else.** Look for:

- `DropColumn`, `AlterColumn` narrowing a type or length, `nullable: false` on an existing
  column with no default → destructive. Stop and ask.
- A rename that EF modelled as drop + add → that is data loss; write `RenameColumn` by hand.
- Anything touching a table you did not mean to change → the snapshot was out of date.

Then apply it locally and check the app still starts:

```bash
dotnet ef database update --project Planning.Infrastructure --startup-project Planning.Api
dotnet build Planning.slnx
```

The provider is Npgsql with `EnableRetryOnFailure`, so an operation can be retried — do not
rely on a transaction spanning several `SaveChangesAsync` calls without an explicit execution
strategy.

## 4. Everything downstream

A new field is not done until it exists all the way out:

- [ ] request/response records in `Planning.Application/<Feature>/<Feature>Dtos.cs`
- [ ] `<Feature>Mapper.ToResponse`
- [ ] the FluentValidation rule, matching the column's constraints
- [ ] the service method that passes it into `Create`/`Update`
- [ ] `pnpm generate:api` in `Planning.Web` against a running API
- [ ] the zod schema in `app/schemas/`, with limits matching the validator
- [ ] the `controls` array in the relevant `composables/edit/` or `composables/create/`
- [ ] `i18n/locales/nl.json` **and** `en.json` for the field label and any validation message

## 5. Record the decision

If the schema change reflects a product decision — a field becoming optional, an entity
gaining a concept — add a record under `docs/decisions/` in the format its README describes, and one row in
that README's table.
