# References

Things to look up, not to read through.

| Document | Contains |
|---|---|
| [configuration.md](configuration.md) | every setting, where it comes from, what happens when it is missing |
| [authorization.md](authorization.md) | roles, policies, and what each endpoint requires |
| [error-codes.md](error-codes.md) | error codes and their HTTP status |

Enums (`UserRole`, `PlanningStatus`, `AppModule`, …) live in `Planning.Domain/Enums/` and are
serialized by name — the code is the reference there, and renaming a member is a breaking
change for the API and the frontend alike.
