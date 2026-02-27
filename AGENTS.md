# AGENTS.md - AI Coding Guide for Spendly

## Architecture Overview

**Spendly** is a financial management system with ASP.NET Core backend and mobile frontend. Stack: .NET 10, MongoDB, Serilog, FluentValidation, AspectCore.

Backend uses layered architecture: API Controllers → Business Layer → Data Layer (Repository pattern with MongoDB). Request context flows through RequestContextViewModel (DI singleton). All responses follow standard format: `{success, data, error}`.

## Key Structural Patterns

**Response Format (Mandatory)**: All endpoints return FunctionResponse<T>. Standard error response:
```csharp
FunctionResponse<T>.Failure("ERROR_CODE")  // returns {success: false, data: null, error: {code, message}}
FunctionResponse<T>.Success(data)          // returns {success: true, data: {...}, error: null}
```

**Request Context**: RequestContextViewModel is scoped DI dependency providing current user context. Set via RequestContextFilter from ClaimTypes.Name claim. For testing, use header X-Test-SellerId. Always call TryToGetUserId() before direct UserId property (which throws if null).

**Validation**: All controllers use FluentValidation through middleware (configured in Program.cs). Validators discovered auto from API assembly. Response errors from InvalidModelState handled by ControllerExtensions.InvalidModelStateResponse.

**Localization**: Middleware loads from LocaleData (supported cultures: "en" default + codes from Spendly.Shared.Localization/Translations). UseRequestLocalization configured in middleware pipeline.

## Critical Infrastructure & Middleware

**Middleware Order** (Program.cs):
1. RequestLocalization
2. CORS (AngularClient policy only)
3. RequestSessionMiddleware (generates SessionId, used in logs)
4. SerilogContextEnricherMiddleware (adds SessionId/UserId to logs)
5. SerilogRequestLogging
6. Endpoint routing

**Filters** (applied via MvcOptions):
- LoggingActionFilter: logs all actions
- CacheControlHeaderFilter: sets cache headers
- RequestContextFilter: extracts UserId from claims

**Exception Handling**: GlobalExceptionLoggingMiddleware catches unhandled exceptions, logs with TraceId+SessionId, returns 500 with standard error format.

**Logging**: Serilog structured logging only. Enrich context with SessionId (via RequestSession.Get(httpContext)) and UserId. Never log PII.

## Development Patterns

**DI Service Registration**: Use AppBootstrapper.Create(...).WithServiceScanning([...]).ConfigureServices(...).ConfigureCors(...).BuildWebApi(). Services auto-scanned from assemblies. Memory cache available (AddMemoryCache). ReportRepositories configured via config. ICacheInvalidationService scoped.

**Database**: MongoDB via Repository<T> generic class. Credentials from DbSettings (username, password in config). Collation set to "en" with secondary strength. Connection to localhost:27017.

**Authentication**: Token-based (JWT). Claims extracted in RequestContextFilter. Approval/scope validation TBD in business layer.

**AOP/Interception**: AspectCore configured (CacheableMethodInterceptor registered). DynamicProxyServiceProviderFactory commented out but available.

## Critical Rules (From /specs/00_AI_RULES.md)

- **No assumptions**: Ask before implementing if unclear
- **Preserve code**: Never remove comments, TODOs, FIXMEs, logs, or headers
- **Minimal changes**: Modify only requested modules; don't refactor unrelated files
- **Track changes**: Add `// CHANGED_BY_AI: <date> - <reason>` at file top
- **Protect contracts**: Don't change request/response schemas or endpoint names (requires API_CONTRACTS update)
- **No new libraries** without approval; check /specs/03_BACKEND_SPEC.md for approved stack
- **Security**: Use env vars for secrets; validate inputs server-side; don't log PII
- **Background jobs**: API must not block; follow /specs/07_BACKGROUND_JOBS.md
- **Architecture**: All changes need ADR; reference /specs/02_ARCHITECTURE.md

## Project Structure

```
Api/
  Spendly.Mobile.Api/              # Main API (Controllers)
  Spendly.Mobile.BusinessLayer/    # Business logic
  Spendly.Mobile.ViewModels/       # API DTOs
  Spendly.Shared.Core/             # Bootstrap, Helpers, Logging
  Spendly.Shared.DataLayer/        # Repository<T>, MongoDB access
  Spendly.Shared.Entities/         # Domain models (Core/, LocaleManagement/)
  Spendly.Shared.Enums/            # Shared enums
  Spendly.Shared.Localization/     # TranslationService, Translations/
  Spendly.Shared.ViewModels/       # FunctionResponse<T>, RequestContextViewModel
specs/                             # Authority: Architecture, API contracts, Security, Background jobs
Mobile/                            # (Flutter/React-native, not detailed)
```

## Build & Run (Inference)

.NET 10 project. Build: `dotnet build Api/Spendly.sln`. Run: `dotnet run` from Spendly.Mobile.Api. Debug: Attach to localhost (check Spendly.Mobile.Api.http for endpoints). Test header: X-Test-SellerId for user context.

## When Uncertain

Reference specs in order: 00_AI_RULES (mandatory), 01_PROJECT_OVERVIEW, 02_ARCHITECTURE, then feature-specific (03_BACKEND_SPEC, 05_API_CONTRACTS, 06_SECURITY_SPEC, 07_BACKGROUND_JOBS). Always ask before deviating from established patterns.

