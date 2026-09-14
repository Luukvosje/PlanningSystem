# Referenties

Exacte namen en waarden om op te zoeken, niet om door te lezen.

| Document | Bevat |
|---|---|
| [configuration.md](configuration.md) | elke instelling, waar hij vandaan komt, wat er gebeurt als hij ontbreekt |
| [authorization.md](authorization.md) | rollen, policies, en per endpoint wat het eist |
| [error-codes.md](error-codes.md) | foutcodes en hun HTTP-status |

Enums (`UserRole`, `PlanningStatus`, `AppModule`, …) staan in `Planning.Domain/Enums/` en
worden op naam geserialiseerd — de code is daar de referentie, en hernoemen is een breaking
change voor de API én de frontend.
