# AGENTS.md - AI Coding Guide for Spendly

## Critical First Step

Every AI prompt MUST begin with:
> Follow /specs/00_AI_RULES.md strictly.

All architectural decisions documented in `/specs/02_ARCHITECTURE.md` through `/specs/07_BACKGROUND_JOBS.md` are authoritative. Do not make assumptions; if unclear, stop and ask.

## Architecture Overview

**Spendly** is a mobile expense tracking application with ASP.NET Core backend (.NET 10) and MongoDB persistence. Mobile frontend (Flutter/React Native - TBD). Stack: MongoDB (not relational), Serilog, FluentValidation, AspectCore, AppBootstrapper.

Backend: Layered architecture (Controllers → Business Layer → Data Layer → MongoDB). All responses via `FunctionResponse<T>`. Request context via scoped DI (`RequestContextViewModel`). Middleware pipeline critical - order matters.

## Must-Read Specs (In Order)

1. **00_AI_RULES.md** - Mandatory rules (preserve comments, track changes, protect APIs)
2. **02_ARCHITECTURE.md** - System design (MongoDB, layered pattern, middleware pipeline)
3. **03_BACKEND_SPEC.md** - Backend stack (tech exact, project structure, DI via AppBootstrapper)
4. **05_API_CONTRACTS.md** - API design (versioning, response format, status codes, error codes)
5. **06_SECURITY_SPEC.md** - Security (JWT, PBKDF2 hashing, CORS, OAuth, no secrets in code)
6. **04_MOBILE_SPEC.md** - Mobile design (TBD: framework choice, project structure)
7. **07_BACKGROUND_JOBS.md** - Async tasks (non-blocking, idempotent, mechanism TBD)

## Backend Project Structure

```
Api/
├── Spendly.Mobile.Api/            # Controllers (request handlers only, no logic)
├── Spendly.Mobile.BusinessLayer/  # Services (business logic)
├── Spendly.Mobile.ViewModels/     # DTOs for this API
├── Spendly.Shared.Core/           # Bootstrap, Logging, Interception, Crypto
├── Spendly.Shared.DataLayer/      # Repository<T> + MongoDB access
├── Spendly.Shared.Entities/       # Domain models (BaseEntity)
├── Spendly.Shared.Enums/          # Shared enumerations
├── Spendly.Shared.Localization/   # TranslationService, i18n files
└── Spendly.Shared.ViewModels/     # FunctionResponse<T>, RequestContextViewModel
```

**Data Flow:** Controller receives request → calls service → service calls repository → repository queries MongoDB → response wrapped in FunctionResponse<T>.

## Key Technical Patterns

### Response Format (All Endpoints)

```json
// Success (200/201)
{ "success": true, "data": { /* payload */ }, "error": null }

// Error (400+)
{ "success": false, "data": null, "error": { "code": "ERROR_CODE", "message": "..." } }
```

Implement with `FunctionResponse<T>.Success(data)` or `FunctionResponse<T>.Failure("ERROR_CODE")`.

### Request Context

`RequestContextViewModel` (scoped DI) holds current user context. Populated by `RequestContextFilter` from JWT claim `ClaimTypes.Name`. For testing, use header `X-Test-SellerId`. Always call `TryToGetUserId()` before property access (which throws if null).

### Validation

FluentValidation auto-discovered from API assembly. Invalid requests return 400 with validation details via `ControllerExtensions.InvalidModelStateResponse`. Create validators in API project, inherit `AbstractValidator<TRequest>`.

### Authentication

JWT tokens. Issued on login, validated server-side. Token format: `Authorization: Bearer <jwt>`. Claims: `sub` (user ID), `name` (email), `exp`, `iat`. Validate in `RequestContextFilter`.

### Database

MongoDB (not SQL). Generic `Repository<T>` for CRUD. Entities inherit `BaseEntity` (has `Id` property). Connection: localhost:27017, credentials from `DbSettings`. No migrations (schema-less). Async/await with `ConfigureAwait(false)`.

### Logging

Serilog structured only. SessionId injected via `SerilogContextEnricherMiddleware`. Never log PII. Example: `logger.LogInformation("Expense created: {ExpenseId} by {UserId}", expenseId, userId)`.

## Middleware Pipeline (Critical Order)

1. RequestLocalization (culture support)
2. CORS (AngularClient policy)
3. RequestSessionMiddleware (generates SessionId)
4. SerilogContextEnricherMiddleware (enriches logs)
5. SerilogRequestLogging
6. Exception handling → GlobalExceptionLoggingMiddleware
7. Routing & Endpoints

Register via Program.cs in `AppBootstrapper.BuildWebApi()`.

## Action Filters

Applied via MvcOptions in Program.cs:
- `LoggingActionFilter` - logs action entry/exit
- `CacheControlHeaderFilter` - sets cache headers
- `RequestContextFilter` - extracts UserId from claims

## DI & Bootstrap Pattern

```csharp
var app = AppBootstrapper
    .Create("AppName", args)
    .WithServiceScanning([typeof(SomeClass).Assembly])
    .ConfigureServices((services, config) =>
    {
        services.AddScoped<IService, ServiceImpl>();
    })
    .ConfigureCors((services, config) => { /* ... */ })
    .BuildWebApi();
app.Run();
```

**Scopes:** Scoped = per-request (repositories, RequestContextViewModel), Transient = new each time (validators, interceptors).

## API Versioning & Contracts

**Path-based:** `/api/v1/users`, `/api/v1/expenses`. New contract = new major version. Protected from changes via 00_AI_RULES.md section 6.

**Error Codes:** Catalog in 05_API_CONTRACTS.md (VALIDATION_ERROR, UNAUTHORIZED, USER_NOT_FOUND, etc.). Adding new codes requires spec update.

**Status Codes:** 200 (OK), 201 (Created), 400 (validation), 401 (auth), 403 (forbidden), 404 (not found), 500 (error).

## Security Requirements

- **Secrets:** Environment variables only (DB_PASSWORD, JWT_SECRET, etc.), never in code
- **Passwords:** PBKDF2 with 600k iterations (PasswordHelper.cs)
- **CORS:** AngularClient policy (localhost + staging/prod origins - configurable)
- **Input Validation:** Server-side mandatory (FluentValidation)
- **PII:** Never log emails, passwords, tokens
- **HTTPS:** Required in production

## Building & Testing

**Commands:**
```bash
dotnet build Api/Spendly.sln
dotnet run -p Api/Spendly.Mobile.Api
dotnet test Api/Spendly.sln
```

**Testing:** xUnit required. Tests in separate projects (*.Tests). Mock repositories. Test validation, business logic, API contracts.

## TBD (Pending Decisions)

- **Mobile Framework:** Flutter vs React Native vs Native (see 04_MOBILE_SPEC.md)
- **Background Jobs:** Hangfire vs SQS vs Hosted Service (see 07_BACKGROUND_JOBS.md)
- **Refresh Tokens:** Implement or not (JWT alone sufficient?)
- **Rate Limiting:** Enable or defer (see 05_API_CONTRACTS.md)
- **Encryption at Rest:** Needed for sensitive fields?
- **Offline Sync:** Required for mobile?

## When Uncertain

1. Check relevant spec file (see Must-Read list above)
2. If not detailed, refer to "TBD (Pending Decisions)"
3. If architectural, check 02_ARCHITECTURE.md
4. If API, check 05_API_CONTRACTS.md
5. If security, check 06_SECURITY_SPEC.md
6. If unclear, **STOP and ask** (rule 1.2 from 00_AI_RULES.md)

Never assume database, framework, or architecture details. Refer to specs as source of truth.

