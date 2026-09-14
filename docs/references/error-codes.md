# Foutcodes

Elke verwachte fout reist als `Result.Failure(message, errorCode)` door de application-laag.
`Planning.Api/Extensions/ResultExtensions.cs` vertaalt die code naar een HTTP-status. Een code
die dáár niet in de `switch` staat valt door naar **400**, wat vrijwel nooit is wat je bedoelde.

Schrijfwijze: `SCREAMING_SNAKE_CASE`.

## De mapping

| Code | HTTP | Wanneer |
|---|---|---|
| `NOT_FOUND` | 404 | bestaat niet, óf hoort bij een andere organisatie — die twee geven bewust hetzelfde antwoord |
| `VALIDATION_ERROR` | 400 | een domeininvariant is geschonden (`ArgumentException` opgevangen door `TranslateDomainErrorsAsync`) |
| `NO_ORGANIZATION` | 400 | geen organisatiecontext; het account heeft er meerdere en koos er geen, of geen enkele actieve |
| `UNAUTHORIZED` | 401 | inloggegevens kloppen niet |
| `FORBIDDEN` | 403 | mag wel bestaan, mag jij niet doen |
| `CONFLICT` | 409 | botst met bestaande staat (dubbel e-mailadres, al beslist verzoek) |
| `EXPIRED` | 410 | uitnodiging of resetlink is verlopen |
| `MODULE_DISABLED` | 403 | staat in de mapping, wordt nergens geproduceerd — zie hieronder |
| `INTERNAL_ERROR` | 500 | door `GlobalExceptionMiddleware`, met `traceId`; het exception-bericht alleen in Development |

## `NOT_FOUND` versus `FORBIDDEN`

De belangrijkste regel. Vraagt iemand een record op dat van een andere tenant is, dan is het
antwoord `NOT_FOUND`. Met `FORBIDDEN` bevestig je dat het id bestaat, en dat is al een lek.
`FORBIDDEN` is voor het geval waarin de caller de rij wél mag zien maar de handeling niet mag
uitvoeren. Zie [architecture/multi-tenancy.md](../architecture/multi-tenancy.md).

## Constanten

De vier meest gebruikte codes staan als constante op `Failures`
(`Planning.Application/Common/TenantServiceBase.cs`) met bijbehorende helpers:

```csharp
Failures.NoOrganizationContext<T>()
Failures.NotFoundFor<T>("Customer")
Failures.ForbiddenFor<T>("...")
```

Gebruik die in plaats van de string. `CONFLICT`, `EXPIRED` en `UNAUTHORIZED` hebben nog geen
constante en worden als literal geschreven.

## Berichten zijn Engels en statisch

De backend heeft geen i18n. Elk `Result.Failure`-bericht en elk `ArgumentException`-bericht is
een vaste Engelse string; de frontend vertaalt ze op exacte match in
`Planning.Web/app/utils/backendMessages.ts`.

Gevolgen:

- **Een bericht met geïnterpoleerde data is niet te vertalen.** Bijvoorbeeld
  `Module '{module}' is disabled at organization level.` — die valt terug op Engels.
- **Een bestaand bericht wijzigen breekt stilzwijgend de vertaling.** Pas
  `backendMessages.ts` in dezelfde commit aan.
- **Een nieuw bericht zonder entry is niet kapot**, alleen nog niet vertaald.

Uitzondering: de *standaard*-meldingen van FluentValidation (`'{Field}' must not be empty.`)
worden wél door de backend gelokaliseerd, via `UseRequestLocalization` in `Program.cs`,
gestuurd door de `Accept-Language`-header die `apiClient.ts` meestuurt. Eigen
`.WithMessage(...)`-teksten vallen daar níet onder.

## Validatiefouten van FluentValidation

Die gaan niet via `Result` maar via `ApiControllerBase.ValidateAndExecuteAsync`, die een
`ValidationProblemDetails` teruggeeft (400) met de fouten gegroepeerd per veldnaam. De
frontend leest ze uit `useApiError(err).validationErrors` en zet ze op de bijbehorende
formuliervelden.

## Openstaand

- **`MODULE_DISABLED` wordt nergens geproduceerd.** De branch in `ResultExtensions` is dood;
  moduletoegang wordt afgewezen door `ModuleAuthorizationHandler`, wat een kale 403 van
  ASP.NET oplevert zonder body. Ofwel de branch weg, ofwel de handler die code laten
  teruggeven.
- **De mapping matcht op stringliteralen**, niet op `Failures.NotFound`. Een hernoemde
  constante breekt de build dus niet.
- **De foutbody is een anoniem object** (`new { error, errorCode }`) terwijl
  `Planning.Api/Models/ApiErrorResponse.cs` bestaat en in `[ProducesResponseType]` wordt
  geadverteerd. Ze serialiseren toevallig hetzelfde.
