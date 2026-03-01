# API CONTRACTS

## General Rules

- **Versioning:** Path-based (e.g., `/api/v1/users`, `/api/v2/expenses`)
- **Format:** JSON only (Content-Type: application/json)
- **Character Encoding:** UTF-8
- **ID Format:** MongoDB ObjectId as string (24 hex characters)
- **Timestamps:** ISO 8601 with UTC timezone (e.g., 2026-02-27T14:30:00Z)
- **Authentication:** Bearer token in Authorization header (JWT)

## Standard Response Format

**Success Response (200, 201):**
Returns the data payload directly (no wrapper):
```json
{
  "id": "507f1f77bcf86cd799439011",
  "email": "user@example.com",
  "createdAt": "2026-02-27T10:00:00Z"
}
```

**Error Response (400, 401, 403, 404, 500):**
```json
{
  "message": "EmailIsRequired", // This code will come from MessageCodes
}
```

**List Response (no paging required):**
```json

  [
    { "id": "507f1f77bcf86cd799439011", "email": "user1@example.com" },
    { "id": "507f1f77bcf86cd799439012", "email": "user2@example.com" }
    ]

```

## Mobile TODO

- [ ] Define auth token storage strategy (react-native-keychain)
- [ ] Define base API call function pattern (see UI_ARCHITECTURE.md)
- [ ] Define error display strategy (toast vs modal)

## Authentication

**Header:** `Authorization: Bearer <jwt_token>`

**JWT Claims (example):**
```json
{
  "sub": "507f1f77bcf86cd799439011",
  "name": "user@example.com",
  "iat": 1645000000,
  "exp": 1645010000,
  "iss": "https://api.spendly.example.com"
}
```

**Token Expiry:** 1 hour (configurable)

**Refresh Token:** (TBD - implement if long-lived sessions needed)

## HTTP Status Codes

| Code | Meaning | Example |
|------|---------|---------|
| 200 | OK - Request succeeded | GET /users/{id} |
| 201 | Created - Resource created | POST /users |
| 204 | No Content - Success but no body | DELETE /expenses/{id} |
| 400 | Bad Request - Validation failed | Invalid email format |
| 401 | Unauthorized - Missing/invalid token | Missing Authorization header |
| 403 | Forbidden - Authenticated but no access | User accessing another user's data |
| 404 | Not Found - Resource doesn't exist | GET /users/nonexistent |
| 409 | Conflict - Resource already exists | Email already registered |
| 500 | Server Error - Unhandled exception | Database connection failed |

## Error Codes Catalog

**Authentication & Authorization:**
- `UNAUTHORIZED` - Missing or invalid token
- `TOKEN_EXPIRED` - JWT token expired
- `FORBIDDEN` - Insufficient permissions
- `USER_NOT_FOUND` - User ID doesn't exist

**Validation:**
- `VALIDATION_ERROR` - Generic validation failure (details in message)
- `EMAIL_REQUIRED` - Email field is required
- `EMAIL_INVALID` - Email format invalid
- `PASSWORD_WEAK` - Password doesn't meet requirements
- `PASSWORD_REQUIRED` - Password field is required
- `DUPLICATE_EMAIL` - Email already registered

**Business Logic:**
- `USER_NOT_ACTIVE` - Account disabled
- `EXPENSE_NOT_FOUND` - Expense ID doesn't exist
- `CATEGORY_NOT_FOUND` - Category ID doesn't exist
- `INSUFFICIENT_BALANCE` - Not enough balance for transaction
- `INVALID_DATE_RANGE` - Start date after end date

**System:**
- `INTERNAL_SERVER_ERROR` - Unhandled exception
- `SERVICE_UNAVAILABLE` - Temporary outage
- `RATE_LIMITED` - Too many requests from IP

## Pagination

**Query Parameters:**
- `pageNumber` - 1-indexed (default: 1)
- `pageSize` - Items per page (default: 10, max: 100)

**Example:**
```
GET /api/v1/expenses?pageNumber=2&pageSize=20
```

**Response includes:**
```json
{
  "data": {
    "items": [ ... ],
    "total": 150,
    "pageNumber": 2,
    "pageSize": 20,
    "totalPages": 8
  }
}
```

## Filtering & Sorting

**Filters:** Query parameters per endpoint (TBD per feature)

**Example:**
```
GET /api/v1/expenses?category=food&startDate=2026-01-01&endDate=2026-02-01
```

**Sorting:** Query parameter `sortBy` (TBD per endpoint)

**Example:**
```
GET /api/v1/expenses?sortBy=date.desc,amount.asc
```

## Common Request Headers

```
Authorization: Bearer <token>
Content-Type: application/json
Accept: application/json
Accept-Language: en   (for localization)
User-Agent: Spendly-Mobile/1.0
```

## Common Response Headers

```
Content-Type: application/json
X-Request-Id: <uuid>  (for tracing)
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1645000000
Cache-Control: no-cache, no-store, must-revalidate
```

## Endpoint Summary (TBD - add per feature)

### Authentication Endpoints (v1)

**POST /api/v1/auth/register**
- Request: `{ email, password }`
- Response: `{ id, email, token }`
- Errors: 400 (validation), 409 (duplicate email)

**POST /api/v1/auth/login**
- Request: `{ email, password }`
- Response: `{ id, email, token }`
- Errors: 401 (invalid credentials)

**POST /api/v1/auth/logout**
- Auth: Required (Bearer token)
- Response: `{ success: true }`

**POST /api/v1/auth/refresh** (TBD)
- Request: `{ refreshToken }`
- Response: `{ token, refreshToken }`

**POST /api/v1/auth/oauth/google** (TBD)
- Request: `{ idToken }`
- Response: `{ id, email, token }`

### User Endpoints (v1)

**GET /api/v1/users/{id}**
- Auth: Required
- Response: `{ id, email, createdAt, updatedAt }`
- Errors: 404, 403 (not owner)

**PUT /api/v1/users/{id}**
- Auth: Required
- Request: `{ email?, password? }`
- Response: Updated user object
- Errors: 404, 409 (email taken)

### Expense Endpoints (v1)

**GET /api/v1/expenses**
- Auth: Required
- Query: `pageNumber, pageSize, category, startDate, endDate`
- Response: Paginated list of expenses
- Filters own expenses only

**POST /api/v1/expenses**
- Auth: Required
- Request: `{ amount, description, categoryId, expenseDate, accountId? }`
- Response: Created expense
- Errors: 404 (category not found)

**GET /api/v1/expenses/{id}**
- Auth: Required
- Response: Single expense
- Errors: 404, 403 (not owner)

**PUT /api/v1/expenses/{id}**
- Auth: Required
- Request: `{ amount?, description?, categoryId?, expenseDate? }`
- Response: Updated expense

**DELETE /api/v1/expenses/{id}**
- Auth: Required
- Response: 204 No Content
- Errors: 404, 403 (not owner)

### Category Endpoints (v1)

**GET /api/v1/categories**
- Auth: Required
- Response: User's custom categories + default categories
- Errors: None

**POST /api/v1/categories**
- Auth: Required
- Request: `{ name, color?, icon? }`
- Response: Created category

### Report Endpoints (v1)

**GET /api/v1/reports/summary**
- Auth: Required
- Query: `startDate, endDate`
- Response: `{ totalExpense, byCategory: { ... }, byMerchant: { ... } }`

## Rate Limiting (TBD)

**If implemented:**
- Limit: 1000 requests per hour per user
- Limit: 100 requests per minute per IP (login/register)
- Headers: X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset
- Response: 429 Too Many Requests

## Breaking Changes & Versioning

**Versioning Strategy:**
- Major changes: new API version (v2, v3, etc.)
- Minor additions: backward compatible (v1.1)
- Deprecation: announce 6 months before removal
- Old versions: support for 12 months minimum

**Example:**
```
v1: Original API
v1.1: Add optional fields (backward compatible)
v2: Breaking change (old endpoint still works for 12 months)
```

## Documentation

- OpenAPI/Swagger spec generated from code (TBD if enabled)
- Human-readable docs at `/api/docs`
- Interactive API explorer at `/api/swagger-ui`
