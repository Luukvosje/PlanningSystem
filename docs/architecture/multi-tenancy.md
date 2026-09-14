# Multi-tenancy

De belangrijkste pagina in deze map. Eén fout hier is geen bug maar een datalek: de ene klant
ziet de planning van de andere.

## Het model

Een **Account** is een persoon met een e-mailadres en een wachtwoord. Een **User** is het
lidmaatschap van dat account in één organisatie, met een rol. Eén account kan meerdere
lidmaatschappen hebben — dat is hoe iemand voor twee bedrijven kan plannen.

```
Account ──1:n──► User ──n:1──► Organization
 (inloggen)   (rol, actief)     (de tenant)
```

Alles wat aan een organisatie toebehoort erft van `TenantEntity`
(`Planning.Domain/Common/TenantEntity.cs`): `OrganizationId`, `CreatedAtUtc`, `UpdatedAtUtc`.

## De keten: van token naar organisatie

Dit is het stuk dat je moet begrijpen voordat je een endpoint schrijft.

**1. Het JWT draagt `accountId`, niet de organisatie.**
`JwtTokenService` zet `sub`, `accountId` en `email` in het token. Bewust géén organisatie:
een token dat een organisatie vastlegt, moet opnieuw uitgegeven worden als je wisselt, en
blijft geldig als je lidmaatschap wordt ingetrokken.

**2. `OrganizationContextMiddleware` bepaalt per request wélke organisatie.**
Hij leest `accountId` uit het token en zoekt het lidmaatschap:

- Staat er een `X-Organization-Id`-header? Dan wordt die alléén gehonoreerd als er een
  lidmaatschap voor dat account in die organisatie bestaat **en** dat lidmaatschap actief is.
  De header is dus een *verzoek*, geen bewering.
- Geen bruikbare header? Dan het enige actieve lidmaatschap.
- Meerdere actieve lidmaatschappen en geen header? Dan **geen** organisatiecontext. Er wordt
  bewust niet gegokt: één van de twee kiezen zou stilzwijgend bepalen welke tenant de caller
  leest en schrijft. Die keuze blijft bij de client, via `GET /api/organizations/mine`.

Slaagt het, dan worden `userId`, `organizationId` en de rol als claims aan de identity
toegevoegd — pas dáár, in deze request, niet in het token.

**3. `ICurrentUserContext` leest die claims.**
`CurrentUserContext` (in `Planning.Api/Services/`) is de enige plek waar de application-laag
aan de identiteit komt. `HasOrganization` is waar zodra er zowel een `organizationId` als een
`userId` is.

**4. De middleware staat tussen authenticatie en autorisatie.**
Dat is geen detail: de `Require*Module`-policies lezen de claims die deze middleware toevoegt.
Verplaats hem en elke module-policy faalt — zonder duidelijke foutmelding.

## Waar de grens bewaakt wordt

**Repositories filteren niet op organisatie.** `GetByIdAsync(id)` geeft de entiteit terug,
van wie hij ook is. Dat is opzettelijk: als de repository stilletjes zou filteren, zou een
ontbrekende controle in de service *werken* in plaats van opvallen, en zou niemand ooit leren
dat de controle nodig is.

**De application service is het enige controlepunt.** Het patroon, uit `PlanningService`:

```csharp
var record = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

if (record is null || !Owns(record))
{
    return Failures.NotFoundFor<PlanningResponse>("Planning record");
}
```

Twee dingen dragen hier het gewicht:

1. **`null` en "van een andere tenant" geven hetzelfde antwoord.** `NOT_FOUND`, nooit
   `FORBIDDEN`. Met `FORBIDDEN` bevestig je dat het id bestaat, en dat is al een lek.
2. **Het geldt ook voor reads.** Een `GET` die de controle overslaat is precies zo erg als een
   `DELETE` die dat doet.

`TenantServiceBase` levert het gereedschap: `TryGetOrganizationId` (organisatie of
`NO_ORGANIZATION`), `Owns(entity)` en `TranslateDomainErrorsAsync`.

## Vreemde verwijzingen tellen ook mee

Een planning-record verwijst naar een klant en naar een medewerker. Beide id's komen uit het
request en zijn dus door de aanvaller te kiezen. `PlanningService.ValidateReferencesAsync`
laadt ze en controleert ze tegen dezelfde organisatie vóór het opslaan. Zonder die stap kan
iemand zijn eigen dienst aan de klant van een andere tenant hangen.

Hetzelfde geldt voor elke nieuwe verwijzing die je toevoegt.

## Lijstquery's

Die filteren wél in de repository — `GetByOrganizationIdAsync(organizationId, ...)` — met een
id dat uit `TryGetOrganizationId` komt, nooit uit het request.

## Hoe je het test

`Planning.Tests/Application/PlanningServiceTests.cs` is het model. Vaste GUID's voor
`OwnOrganizationId` en `OtherOrganizationId`, zodat een gefaalde test meteen zegt wélke tenant
het betrof. Drie tests per tenant-entiteit zijn genoeg:

- lezen van een record van een andere organisatie geeft `NOT_FOUND`
- verwijderen ervan geeft `NOT_FOUND`
- muteren ervan geeft `NOT_FOUND`

Dit is de plek waar wij wél tests schrijven, ondanks het "standaard geen tests"-uitgangspunt.
Ze zijn goedkoop en ze vangen de duurste soort fout.

## De checklist bij elk nieuw endpoint

- [ ] organisatie komt uit `ICurrentUserContext`, nooit uit body of query
- [ ] na elke load-by-id een `Owns`-controle
- [ ] `NOT_FOUND`, niet `FORBIDDEN`
- [ ] elke vreemde verwijzing gecontroleerd tegen dezelfde organisatie
- [ ] lijstquery's filteren op `organizationId`
- [ ] module-policy op de controller, actie-policy op elke mutatie

Zie de skill `tenant-endpoint` voor de volledige procedure.
