# Postman Collection Setup

Dit project bevat een complete Postman collection voor de Planning System API.

## Bestanden

- `PlanningSystem.postman_collection.json` - De Postman collection met alle API endpoints
- `PlanningSystem.postman_environment.json` - Environment variabelen voor development

## Installatie

1. Open Postman
2. Klik op **Import** (linksboven)
3. Sleep beide JSON bestanden naar Postman, of klik op **Upload Files** en selecteer ze
4. Selecteer de environment "Planning System - Development" in de environment dropdown (rechtsboven)

## Gebruik

### 1. Eerst inloggen

1. Ga naar de **User** folder
2. Voer eerst **Login** uit met je credentials:
   ```json
   {
     "email": "user@example.com",
     "password": "Password123!"
   }
   ```
3. De JWT token wordt automatisch opgeslagen in de environment variabele `jwtToken`

### 2. API Endpoints gebruiken

Alle endpoints zijn georganiseerd in folders:
- **User** - Gebruikersbeheer en authenticatie
- **Organization** - Organisatiebeheer
- **Shift** - Shift/rooster beheer

### 3. Environment Variabelen

- `baseUrl` - Standaard: `http://localhost:5157` (pas aan naar je eigen API URL)
- `jwtToken` - Wordt automatisch ingesteld na login
- `shiftId` - Handmatig instellen voor shift-specifieke requests

## Endpoints Overzicht

### User Endpoints
- `POST /user/create` - Maak nieuwe gebruiker
- `POST /user/login` - Login en krijg JWT token

### Organization Endpoints
- `GET /organization` - Haal alle organisaties op voor huidige gebruiker
- `GET /organization/{id}` - Haal organisatie op bij ID
- `POST /organization/create` - Maak nieuwe organisatie
- `PUT /organization/{id}` - Update organisatie (Admin/Manager)
- `DELETE /organization/{id}` - Verwijder organisatie (Admin)
- `GET /organization/{id}/users` - Haal gebruikers op in organisatie
- `POST /organization/{id}/users` - Voeg gebruiker toe aan organisatie (Admin/Manager)
- `DELETE /organization/{id}/users/{userId}` - Verwijder gebruiker uit organisatie (Admin/Manager)

### Shift Endpoints
- `GET /shift` - Haal shifts op met optionele filters
- `GET /shift/{id}` - Haal shift op bij ID
- `POST /shift/create` - Maak nieuwe shift
- `PUT /shift/{id}` - Update shift (Admin/Manager/Worker)
- `DELETE /shift/{id}` - Verwijder shift (Admin/Manager/Worker)
- `PUT /shift/{id}/status` - Update shift status

## Authenticatie

De meeste endpoints vereisen JWT authenticatie. De token wordt automatisch toegevoegd aan de Authorization header voor alle requests in de Organization en Shift folders.

## Tips

1. **Test eerst de Login** - Zonder een geldige JWT token werken de meeste endpoints niet
2. **Pas baseUrl aan** - Als je API op een andere poort draait, pas dan de `baseUrl` variabele aan
3. **Gebruik Environment variabelen** - Voor verschillende omgevingen (dev, staging, prod) kun je meerdere environments aanmaken
4. **Shift ID opslaan** - Na het aanmaken van een shift, kopieer de ID en zet deze in de `shiftId` variabele voor verdere tests

## API Basis URL

Standaard staat de API op:
- HTTP: `http://localhost:5157`
- HTTPS: `https://localhost:7269`

Pas de `baseUrl` variabele aan als je API op een andere URL draait.
