# Error codes

Every expected failure travels through the application layer as
`Result.Failure(message, errorCode)`. `Planning.Api/Extensions/ResultExtensions.cs` translates
that code into an HTTP status. A code that is *not* in that `switch` falls through to **400**,
which is almost never what you meant.

Spelling: `SCREAMING_SNAKE_CASE`.

## The mapping

| Code | HTTP | When |
|---|---|---|
| `NOT_FOUND` | 404 | does not exist, *or* belongs to another organization — those two deliberately give the same answer |
| `VALIDATION_ERROR` | 400 | a domain invariant was violated (`ArgumentException` caught by `TranslateDomainErrorsAsync`) |
| `NO_ORGANIZATION` | 400 | no organization context; the account has several and chose none, or has no active one |
| `UNAUTHORIZED` | 401 | credentials do not match |
| `FORBIDDEN` | 403 | may exist, you may not do this to it |
| `CONFLICT` | 409 | clashes with existing state (duplicate email, request already decided) |
| `EXPIRED` | 410 | invite or reset link has expired |
| `MODULE_DISABLED` | 403 | present in the mapping, produced nowhere — see below |
| `INTERNAL_ERROR` | 500 | from `GlobalExceptionMiddleware`, with a `traceId`; the exception message only in Development |

## `NOT_FOUND` versus `FORBIDDEN`

The most important rule. If somebody requests a record belonging to another tenant, the answer
is `NOT_FOUND`. With `FORBIDDEN` you confirm the id exists, and that is already a leak.
`FORBIDDEN` is for the case where the caller *may* see the row but may not perform the action.
See [architecture/multi-tenancy.md](../architecture/multi-tenancy.md).

## Constants

The four most-used codes are constants on `Failures`
(`Planning.Application/Common/TenantServiceBase.cs`) with matching helpers:

```csharp
Failures.NoOrganizationContext<T>()
Failures.NotFoundFor<T>("Customer")
Failures.ForbiddenFor<T>("...")
```

Use those rather than the string. `CONFLICT`, `EXPIRED` and `UNAUTHORIZED` have no constant yet
and are written as literals.

## Messages are English and static

The backend has no i18n. Every `Result.Failure` message and every `ArgumentException` message
is a fixed English string; the frontend translates them by exact match in
`Planning.Web/app/utils/backendMessages.ts`.

Consequences:

- **A message with interpolated data cannot be translated.** For example
  `Module '{module}' is disabled at organization level.` — that one falls back to English.
- **Changing an existing message silently breaks its translation.** Update
  `backendMessages.ts` in the same commit.
- **A new message without an entry is not broken**, just not translated yet.

Exception: FluentValidation's *default* messages (`'{Field}' must not be empty.`) *are*
localized by the backend, through `UseRequestLocalization` in `Program.cs`, driven by the
`Accept-Language` header `apiClient.ts` sends. Custom `.WithMessage(...)` text is not covered.

## FluentValidation failures

Those do not travel as a `Result` but through
`ApiControllerBase.ValidateAndExecuteAsync`, which returns a `ValidationProblemDetails` (400)
with the errors grouped per field name. The frontend reads them from
`useApiError(err).validationErrors` and puts them on the matching form fields.

## Open

- **`MODULE_DISABLED` is produced nowhere.** The branch in `ResultExtensions` is dead; module
  access is refused by `ModuleAuthorizationHandler`, which yields a bare ASP.NET 403 with no
  body. Either drop the branch, or have the handler return that code.
- **The mapping matches on string literals**, not on `Failures.NotFound`. A renamed constant
  therefore does not break the build.
- **The error body is an anonymous object** (`new { error, errorCode }`) while
  `Planning.Api/Models/ApiErrorResponse.cs` exists and is what `[ProducesResponseType]`
  advertises. They happen to serialize identically.
