# Database Migration Instructies

## Stap 1: Controleer of er wijzigingen zijn

Run dit commando in de terminal (vanuit de root van het project):

```powershell
cd PlanningSystem.DAL
dotnet ef migrations add CheckForChanges --dry-run
```

Als er geen output is of het zegt dat er geen wijzigingen zijn, dan hoef je geen nieuwe migration te maken.

## Stap 2: Maak een nieuwe migration (als nodig)

Als er wel wijzigingen zijn, maak dan een nieuwe migration:

```powershell
cd PlanningSystem.DAL
dotnet ef migrations add UpdateSchema
```

Dit maakt een nieuwe migration file aan in `PlanningSystem.DAL/Migrations/`

## Stap 3: Pas de migrations toe op de database

Om alle pending migrations toe te passen op je LocalDB database:

```powershell
cd PlanningSystem.DAL
dotnet ef database update
```

Dit zal:
- De database `PlanningSystem` aanmaken als die nog niet bestaat
- Alle migrations toepassen (inclusief de InitialCreate migration als die nog niet is toegepast)

## Alternatief: Direct database update (als je zeker weet dat de models kloppen)

Als je gewoon de database wilt updaten zonder eerst te checken:

```powershell
cd PlanningSystem.DAL
dotnet ef database update
```

## Troubleshooting

Als je een error krijgt dat de database niet bestaat:
- LocalDB wordt automatisch aangemaakt bij de eerste connectie
- Zorg dat SQL Server LocalDB is geïnstalleerd

Als je een error krijgt over connection string:
- Check `appsettings.json` in PlanningSystem.API
- De connection string moet naar LocalDB wijzen

## Belangrijk

De bestaande `InitialCreate` migration bevat al alle tabellen:
- Organizations
- Users  
- OrganizationUserMaps
- Shifts

Als je database al bestaat en deze migration al is toegepast, hoef je alleen maar `dotnet ef database update` te runnen om te checken of er nieuwe migrations zijn.
