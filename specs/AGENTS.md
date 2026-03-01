# AGENTS.md - AI Coding Guide for Spendly

## Critical First Step

Every AI prompt MUST begin with:
> Follow /specs/00_AI_RULES.md strictly.

All architectural decisions documented in `/specs/02_ARCHITECTURE.md` through `/specs/07_BACKGROUND_JOBS.md` are authoritative. Do not make assumptions; if unclear, stop and ask.

## Architecture Overview

**Spendly** is a mobile expense tracking application with ASP.NET Core backend (.NET 10) and MongoDB persistence. Mobile frontend: React Native (TypeScript). Stack: MongoDB (not relational), Serilog, FluentValidation, AspectCore, AppBootstrapper.

Backend: Layered architecture (Controllers → Business Layer → Data Layer → MongoDB). All service responses via `FunctionResponse<T>`. Controllers return `Ok(response.Data)` on success or `BadRequestFrom(response)` on failure. Request context via scoped DI (`RequestContextViewModel`). Middleware pipeline critical - order matters.

## Must-Read Specs (In Order)

1. **00_AI_RULES.md** - Mandatory rules (preserve comments, track changes, protect APIs)
2. **02_ARCHITECTURE.md** - System design (MongoDB, layered pattern, middleware pipeline)
3. **03_BACKEND_SPEC.md** - Backend stack (tech exact, project structure, DI via AppBootstrapper)
4. **05_API_CONTRACTS.md** - API design (versioning, response format, status codes, error codes)
5. **06_SECURITY_SPEC.md** - Security (JWT, PBKDF2 hashing, CORS, OAuth, no secrets in code)
6. **04_MOBILE_SPEC.md** - Mobile design (React Native, Redux, Axios, existing interceptor)
7. **UI_ARCHITECTURE.md** - UI architecture decisions (no null, no try/catch, response pattern)
8. **07_BACKGROUND_JOBS.md** - Async tasks (non-blocking, idempotent, mechanism TBD)

## Backend Project Structure

```
Api/
├── Spendly.Mobile.Api/            # Controllers (request handlers only, no logic)
├── Spendly.Mobile.BusinessLayer/  # Services (business logic, no interfaces)
├── Spendly.Mobile.ViewModels/     # DTOs for this API
├── Spendly.Shared.Core/           # Bootstrap, Logging, Interception, Crypto
├── Spendly.Shared.DataLayer/      # IRepository<T> + Repository<T> + MongoDB access
├── Spendly.Shared.Entities/       # Domain models (BaseEntity)
├── Spendly.Shared.Enums/          # Shared enumerations
├── Spendly.Shared.Localization/   # TranslationService, MessageCodes, i18n files
└── Spendly.Shared.ViewModels/     # FunctionResponse<T>, RequestContextViewModel
```

**Data Flow:** Controller receives request → calls service → service calls repository → repository queries MongoDB → service returns FunctionResponse<T> → controller returns Ok(data) or BadRequest.

## Key Technical Patterns

### Controller Response Pattern (All Endpoints)

Controllers return data directly - NOT the FunctionResponse wrapper:

```csharp
[HttpPut]
public async Task<IActionResult> Update([FromBody] CategoryRequestViewModel vm)
{
    var response = await categoryService.Update(vm);
    if (!response.IsSuccess)
    {
        return this.BadRequestFrom(response);
    }
    return Ok(response.Data);
}
```

Success responses contain only the data payload. Error responses (400) contain error code and message.

### Services

- Services do NOT implement interfaces (no IUserService, IExpenseService, etc.)
- Register concrete services directly: `services.AddScoped<UserService>()`
- Services return `FunctionResponse<T>` internally
- Data ownership validation always in service layer, never in controllers
- Controllers never access repositories directly

### Request Context

`RequestContextViewModel` (scoped DI) holds current user context. Populated by `RequestContextFilter` from JWT claim `ClaimTypes.Name`. For testing, use header `X-Test-SellerId`. Always call `TryToGetUserId()` before property access (which throws if null).

### Validation

FluentValidation auto-discovered from API assembly. Invalid requests return 400 with validation details via `ControllerExtensions.InvalidModelStateResponse`. Create validators in API project, inherit `AbstractValidator<TRequest>`. All validation messages from `MessageCodes` constants. No length validation rules.

### Constants

All constants in a `Constants` class organized by function. Never define constants directly in other classes.

### Sensitive Configuration

All sensitive values (JWT, DB password, etc.) in `shared.local.json` (git-ignored). Never in appsettings.json or code.

### Authentication

JWT tokens. Issued on login, validated server-side. Token format: `Authorization: Bearer <jwt>`. Claims: `sub` (user ID), `name` (email), `exp`, `iat`. Validate in `RequestContextFilter`.

### Database

MongoDB (not SQL). IRepository<T> defined in Spendly.Shared.DataLayer - use as-is, never modify. Generic `Repository<T>` implements IRepository<T>. Entities inherit `BaseEntity` (has `Id` property). Connection credentials from `shared.local.json` via DbSettings. No migrations (schema-less). Async/await with `ConfigureAwait(false)`.

### Logging

Serilog structured only. SessionId injected via `SerilogContextEnricherMiddleware`. Never log PII. Example: `logger.LogInformation("Expense created: {ExpenseId} by {UserId}", expenseId, userId)`.

### Coding Style

- Always use `{}` braces for all scopes (even single-line)
- Always use `var` for object creation
- Constants in `Constants` class only

## Middleware Pipeline (Critical Order)

1. RequestLocalization (culture support)
2. CORS (MobileClient policy)
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
        services.AddScoped<UserService>();
    })
    .ConfigureCors((services, config) => { /* ... */ })
    .BuildWebApi();
app.Run();
```

**Scopes:** Scoped = per-request (repositories, RequestContextViewModel, services), Transient = new each time (validators, interceptors).

## API Versioning & Contracts

**Path-based:** `/api/v1/users`, `/api/v1/expenses`. New contract = new major version. Protected from changes via 00_AI_RULES.md section 6.

**Error Codes:** Catalog in 05_API_CONTRACTS.md (VALIDATION_ERROR, UNAUTHORIZED, USER_NOT_FOUND, etc.). Adding new codes requires spec update.

**Status Codes:** 200 (OK), 201 (Created), 400 (validation/business error), 401 (auth), 403 (forbidden), 404 (not found), 500 (error).

## Security Requirements

- **Secrets:** `shared.local.json` only (DB password, JWT secret, etc.), never in code
- **Passwords:** PBKDF2 with 600k iterations (PasswordHelper.cs)
- **CORS:** MobileClient policy (React Native - AllowAnyOrigin)
- **Input Validation:** Server-side mandatory (FluentValidation, MessageCodes, no length checks)
- **PII:** Never log emails, passwords, tokens
- **HTTPS:** Required in production
- **Data Ownership:** Always checked in service layer, never in controllers

## Mobile Stack

- Framework: React Native (TypeScript)
- State: Redux (no Saga)
- HTTP: Axios via existing `src/services/apiClient.ts` interceptor
- Tokens: react-native-keychain
- See UI_ARCHITECTURE.md for UI patterns (no null, no try/catch, response class)

## TBD (Pending Decisions)

- **Background Jobs:** Hangfire vs SQS vs Hosted Service (see 07_BACKGROUND_JOBS.md)
- **Refresh Tokens:** Implement or not (JWT alone sufficient?)
- **Rate Limiting:** Enable or defer (see 05_API_CONTRACTS.md)
- **Encryption at Rest:** Needed for sensitive fields?

## When Uncertain

1. Check relevant spec file (see Must-Read list above)
2. If not detailed, refer to "TBD (Pending Decisions)"
3. If architectural, check 02_ARCHITECTURE.md
4. If API, check 05_API_CONTRACTS.md
5. If security, check 06_SECURITY_SPEC.md
6. If unclear, **STOP and ask** (rule 1.2 from 00_AI_RULES.md)

Never assume database, framework, or architecture details. Refer to specs as source of truth.

