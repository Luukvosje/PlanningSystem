---
name: backend-conventions
description: Load the .NET conventions for this repo before writing C#. Use when working anywhere under Planning.Api, Planning.Application, Planning.Domain, Planning.Infrastructure or Planning.Tests — adding or changing an endpoint, service, entity, repository, validator, EF configuration or test. Covers the layering, the Result pattern, error codes, FluentValidation, authorization policies, EF Core, migrations and multi-tenancy.
---

# Backend conventions

Read [docs/guidelines/api.md](../../../docs/guidelines/api.md) **in full** before writing or
changing C# in this repository. It is the authority on:

- the seven-file shape of a feature slice and where each file goes
- what Domain / Application / Infrastructure / Api may and may not do
- multi-tenancy: how the organization is resolved and where the ownership check goes
- `Result` / `Result<T>`, error codes and how they map to HTTP status codes
- FluentValidation, DTO records, mappers
- repositories, entity configurations, migrations and the migrate-on-boot behaviour
- authorization policies and middleware order
- test conventions in `Planning.Tests`

Then check `CLAUDE.md` for the non-negotiables and `docs/decisions.md` for anything that
looks arbitrary — it probably is not.

If the code disagrees with the document, the code that most recently touched that area wins.
Say so, and fix whichever is wrong in the same change.
