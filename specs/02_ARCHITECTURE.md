# SYSTEM ARCHITECTURE

## High-Level Architecture

```
Mobile Client
    ↓
  HTTPS
    ↓
API Gateway (ASP.NET Core)
    ↓
Business Layer Services
    ↓
Data Layer (Repository Pattern)
    ↓
MongoDB Database
```

## Backend Architecture

**Technology Stack:**
- Framework: ASP.NET Core (.NET 10)
- Database: MongoDB (document-oriented, no migrations)
- Logging: Serilog (structured)
- Validation: FluentValidation
- AOP: AspectCore
- DI: Microsoft.Extensions.DependencyInjection + AppBootstrapper

**Core Pattern: Layered Architecture**

```
Controllers (Spendly.Mobile.Api)
    ↓
Business Layer (Spendly.Mobile.BusinessLayer)
    ↓
Shared ViewModels (Spendly.Shared.ViewModels) - DTOs & FunctionResponse<T>
    ↓
Data Layer (Spendly.Shared.DataLayer) - Repository<T> pattern
    ↓
Entities (Spendly.Shared.Entities) - Domain models with BaseEntity
    ↓
MongoDB
```

**Request Context Flow:**
- RequestContextViewModel (scoped DI) holds current user context
- Populated by RequestContextFilter from ClaimTypes.Name claim
- Available to all services in the same request scope
- For testing: use X-Test-SellerId header

**Middleware Pipeline Order (Critical):**
1. RequestLocalization (culture support)
2. CORS (AngularClient policy)
3. RequestSessionMiddleware (generates SessionId)
4. SerilogContextEnricherMiddleware (enriches logs)
5. SerilogRequestLogging
6. Exception handling (GlobalExceptionLoggingMiddleware)
7. Routing & Endpoints

**Filters (MvcOptions):**
- LoggingActionFilter - logs all controller actions
- CacheControlHeaderFilter - sets cache headers
- RequestContextFilter - extracts UserId from JWT claims

## Mobile Architecture

**Pattern:** Clean Architecture with separation of concerns
- UI Layer React Native
- Domain Layer (entities, use cases)
- Data Layer (repositories, API client)

**Rules:**
- No direct API calls from UI (use service abstraction)
- Secure token storage (platform keychain/keystore)
- Graceful error & offline state handling
- Follow API_CONTRACTS strictly

## Data Layer Details

**Database:** MongoDB (not relational)
- Connection: localhost:27017 (configurable)
- Authentication: username/password via DbSettings
- Collections: Named by entity type (e.g., User, Expense)
- Collation: "en" with secondary strength (case-insensitive)
- Serialization: BSON with custom serializers (Spendly.Shared.Core/MongoSerialization/)

**Repository Pattern:**
- Generic Repository<T> for CRUD operations
- T must inherit from BaseEntity (has Id property)
- Async/await throughout (ConfigureAwait(false))
- Expression-based filtering: `FindAsync(x => x.Status == "Active")`
- No migrations - schema-less, but document structure must be maintained

## API Response Format (Mandatory)

All endpoints return:
```json
// Success
{ "success": true, "data": { /* payload */ }, "error": null }

// Error
{ "success": false, "data": null, "error": { "code": "ERROR_CODE", "message": "Human readable" } }
```

Implemented via `FunctionResponse<T>` class in Spendly.Shared.ViewModels.

## Architecture Rules

- All architectural changes require ADR (Architectural Decision Record)
- No direct coupling between mobile and database
- No business logic in controllers
- Request context must flow via DI, not headers or cookies
- All data access through Repository<T> pattern
- Centralized exception handling via middleware
- Request SessionId must be tracked for observability
