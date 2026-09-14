# Modules

Eén codebase, per organisatie in te schakelen onderdelen. Dit is het mechanisme waarmee een
klantwens een *configuratie* kan worden in plaats van een klantspecifieke tak in de code — zie
[my-company.md](../../my-company.md), "Bewaken".

## De drie modules

| Module | Inhoud | Per organisatie aan/uit? |
|---|---|---|
| `Planning` | planning-records, tijdlijn, beschikbaarheid, aanvragen | ja |
| `Klant` | klantbeheer | ja |
| `Beheer` | organisatie, gebruikers, uitnodigingen, rollen, moduletoewijzing | **nee, altijd aan** |

`AppModule` staat in `Planning.Domain/Modules/AppModule.cs`. De enum wordt op naam
geserialiseerd, dus hernoemen is een breaking change voor zowel de API als de frontend.

## Twee niveaus, en hoe ze samenkomen

Een module wordt op twee plekken aangezet:

- **`OrganizationModule`** — heeft deze organisatie de module afgenomen?
- **`UserModule`** — mag deze medewerker erbij?

De regels staan op één plek, `Planning.Application/Modules/ModulePermissions.cs`, en het zijn
er maar drie:

```csharp
IsAdminRole(role)                       => role is Owner or Admin
GetOrganizationEffectiveEnabled(m, org) => m is Beheer || org
HasEffectiveAccess(role, org, user)     => org && (IsAdminRole(role) || user)
```

Daaruit volgt:

- **Beheer staat altijd aan op organisatieniveau.** Anders zou een organisatie zichzelf kunnen
  uitsluiten van het scherm waarmee je modules aanzet.
- **De organisatie is een harde poort.** Staat de module daar uit, dan heeft niemand toegang —
  ook een Owner niet.
- **Owner en Admin hebben binnen die poort altijd toegang**, ongeacht hun `UserModule`. Dat is
  waarom de moduleschakelaars op de gebruikerspagina bij een admin vastgezet worden getoond in
  plaats van verborgen: zichtbaar maar `disabled`, met een tooltip.

## Waar het wordt afgedwongen

**Backend — `ModuleAuthorizationHandler`.** De policies `RequirePlanningModule`,
`RequireKlantModule` en `RequireBeheerModule` (gedeclareerd in `Program.cs`) dragen elk een
`ModuleRequirement`. De handler roept `IModuleService.HasEffectiveModuleAsync` aan, die
organisatie- en gebruikersmodules ophaalt en door `ModulePermissions` haalt.

Elke controller declareert zijn module-policy op klasseniveau. Meerdere `[Authorize]`-attributen
stapelen als EN — `UsersController` eist bij een rolwijziging zowel `RequireOwnerOrAdmin` als
`RequireBeheerModule`.

**Frontend — `ROUTE_MODULE_MAP`.** In `Planning.Web/app/utils/modules.ts` staat een tabel van
routeprefix naar module:

| Prefix | Module |
|---|---|
| `/planning`, `/timeline`, `/beschikbaarheid` | Planning |
| `/customers` | Klant |
| `/users`, `/organization` | Beheer |

`middleware/module.global.ts` blokkeert een route die daar niet doorheen komt. Daarnaast staat
er `PLANNER_ONLY_ROUTE_PREFIXES = ['/timeline']` voor routes die bovenop de module ook een
plannerrol eisen — zie [besluit 0010](../decisions/0010-tijdlijn-is-plannertool.md).

**Een nieuwe pagina onder een afgeschermd gebied moet in `ROUTE_MODULE_MAP`.** Het menu-item
weghalen is beveiliging via het menu; de URL blijft open. Dat is precies hoe `/timeline` ooit
bereikbaar was zonder de Planning-module.

## De twee helften moeten kloppen

De backend beslist wie erbij mag; de frontend beslist wat je te zien krijgt. Ze delen geen
code. Een nieuwe module aanzetten betekent dus altijd twee kanten aanpassen:

1. `AppModule` uitbreiden, plus een migratie voor bestaande organisaties
2. een `Require<X>Module`-policy in `Program.cs`
3. die policy op de betrokken controllers
4. `ALL_MODULES` en `ROUTE_MODULE_MAP` in `utils/modules.ts`
5. labels in `i18n/locales/nl.json` én `en.json`
6. `pnpm generate:api`, want de enum verandert

Vergeet je stap 4, dan is de pagina bereikbaar terwijl de API 403 geeft — zichtbaar kapot.
Vergeet je stap 3, dan is de API open terwijl het menu netjes oogt — onzichtbaar kapot. Dat
laatste is de gevaarlijke.
