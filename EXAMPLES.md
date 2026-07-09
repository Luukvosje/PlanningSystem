# Planning SaaS API - Example Requests

Base URL (development): `https://localhost:7xxx` or `http://localhost:5xxx`

## 1. Obtain JWT (development)

```http
POST /api/auth/token
Content-Type: application/json

{
  "subject": "dev-user",
  "email": "owner@acme.com",
  "organizationId": "00000000-0000-0000-0000-000000000001"
}
```

**Response `200 OK`**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Use the token for all secured endpoints:

```http
Authorization: Bearer {accessToken}
```

---

## 2. Organizations

### Create organization (anonymous)

```http
POST /api/organizations
Content-Type: application/json

{
  "name": "Acme Planning BV",
  "email": "info@acme.com"
}
```

**Response `201 Created`**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Acme Planning BV",
  "email": "info@acme.com",
  "createdAtUtc": "2026-06-19T10:00:00Z",
  "updatedAtUtc": "2026-06-19T10:00:00Z"
}
```

### Get organization by id

```http
GET /api/organizations/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer {accessToken}
```

---

## 3. Users

### Create user

```http
POST /api/users
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "organizationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "Luuk",
  "lastName": "Planner",
  "email": "luuk@acme.com",
  "role": "Planner"
}
```

**Response `201 Created`**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "organizationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "Luuk",
  "lastName": "Planner",
  "email": "luuk@acme.com",
  "role": "Planner",
  "isActive": true,
  "createdAtUtc": "2026-06-19T10:05:00Z",
  "updatedAtUtc": "2026-06-19T10:05:00Z"
}
```

### Get user by id

```http
GET /api/users/7c9e6679-7425-40de-944b-e07fc1f90ae7
Authorization: Bearer {accessToken}
```

### Get users by organization

```http
GET /api/users/organization/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer {accessToken}
```

---

## 4. Customers

### Create customer

```http
POST /api/customers
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "organizationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Customer One",
  "email": "customer@example.com",
  "phoneNumber": "+31612345678",
  "notes": "Preferred morning appointments"
}
```

### Update customer

```http
PUT /api/customers/{customerId}
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "name": "Customer One Updated",
  "email": "customer@example.com",
  "phoneNumber": "+31612345678",
  "notes": "Updated notes"
}
```

### Delete customer

```http
DELETE /api/customers/{customerId}
Authorization: Bearer {accessToken}
```

**Response `204 No Content`**

### Get customer by id

```http
GET /api/customers/{customerId}
Authorization: Bearer {accessToken}
```

### Get customers by organization

```http
GET /api/customers/organization/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer {accessToken}
```

---

## 5. Planning

### Create planning record

```http
POST /api/planning
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "organizationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "{customerId}",
  "assignedUserId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "title": "Site visit",
  "description": "Initial inspection",
  "startUtc": "2026-06-23T08:00:00Z",
  "endUtc": "2026-06-23T10:00:00Z"
}
```

**Response `201 Created`**

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "organizationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "{customerId}",
  "assignedUserId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "title": "Site visit",
  "description": "Initial inspection",
  "startUtc": "2026-06-23T08:00:00Z",
  "endUtc": "2026-06-23T10:00:00Z",
  "status": "Planned",
  "createdAtUtc": "2026-06-19T10:15:00Z",
  "updatedAtUtc": "2026-06-19T10:15:00Z"
}
```

### Update planning record

```http
PUT /api/planning/{planningId}
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "customerId": "{customerId}",
  "assignedUserId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "title": "Site visit - confirmed",
  "description": "Initial inspection",
  "startUtc": "2026-06-23T08:00:00Z",
  "endUtc": "2026-06-23T10:00:00Z",
  "status": "Confirmed"
}
```

### Delete planning record

```http
DELETE /api/planning/{planningId}
Authorization: Bearer {accessToken}
```

### Get planning record by id

```http
GET /api/planning/{planningId}
Authorization: Bearer {accessToken}
```

### Get week planning

Returns all planning records for an organization that overlap the week starting at `weekStartUtc`.

```http
GET /api/planning/week?organizationId=3fa85f64-5717-4562-b3fc-2c963f66afa6&weekStartUtc=2026-06-23T00:00:00Z
Authorization: Bearer {accessToken}
```

---

## Error responses

**Validation error `400 Bad Request`**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["'Email' is not a valid email address."]
  }
}
```

**Not found `404 Not Found`**

```json
{
  "error": "Customer not found.",
  "errorCode": "NOT_FOUND"
}
```

**Unhandled exception `500 Internal Server Error`**

```json
{
  "error": "An unexpected error occurred.",
  "errorCode": "INTERNAL_ERROR",
  "traceId": "00-...",
  "detail": null
}
```

---

## Database setup

```bash
dotnet ef database update --project Planning.Infrastructure --startup-project Planning.Api
```

Connection string is configured in `Planning.Api/appsettings.json` under `ConnectionStrings:DefaultConnection`.

## Run the API

```bash
dotnet run --project Planning.Api
```

Swagger UI: `/swagger`
