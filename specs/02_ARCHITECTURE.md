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
2. CORS (MobileClient policy)
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

**Framework:** React Native (TypeScript)

**Pattern:** Layered with Redux state management
- Screens (UI)
- Redux Store (state)
- Services (API calls via Axios)
- Navigation (React Navigation)

**Rules:**
- No direct API calls from screens (use service layer)
- Secure token storage (platform keychain/keystore)
- No Saga middleware - use Redux Thunk or plain Redux
- Follow API_CONTRACTS strictly

## Data Layer Details

**Database:** MongoDB (not relational)
- Connection: localhost:27017 (configurable)
- Authentication: username/password via DbSettings
- Collections: Named by entity type (e.g., User, Expense)
- Collation: "en" with secondary strength (case-insensitive)
- Serialization: BSON with custom serializers (Spendly.Shared.Core/MongoSerialization/)

**Repository Pattern:**
- IRepository<T> defined in Spendly.Shared.DataLayer
- Generic Repository<T> implements IRepository<T> for CRUD operations
- T must inherit from BaseEntity (has Id property)
- Async/await throughout (ConfigureAwait(false))
- Expression-based filtering: `FindAsync(x => x.Status == "Active")`
- No migrations - schema-less, but document structure must be maintained
- Never modify the repository pattern

## Controller Response Pattern (Mandatory)

All controller actions must follow this pattern:
- On success: return `Ok(response.Data)`
- On failure: return `this.BadRequestFrom(response)`
- Never return FunctionResponse wrapper directly to client

```csharp
[HttpPut]
public async Task<IActionResult> Update([FromBody] CategoryRequestViewModel categoryRequestViewModel)
{
    var response = await categoryService.Update(categoryRequestViewModel);
    if (!response.IsSuccess)
    {
        return this.BadRequestFrom(response);
    }
    return Ok(response.Data);
}
```

## API Response Format

**Success (200/201):** Returns data object directly
```json
{ "id": "507f1f77bcf86cd799439011", "email": "user@example.com" }
```

**Error (400):**
```json
{ "code": "ERROR_CODE", "message": "Human readable message" }
```

## Dependency Injection Rules

- Services are registered directly in DI without interfaces
- Do NOT create service interfaces (e.g., IUserService)
- Register concrete service classes directly:

```csharp
services.AddScoped<UserService>();
services.AddScoped<ExpenseService>();
```

## Architecture Rules

- All architectural changes require ADR (Architectural Decision Record)
- No direct coupling between mobile and database
- No business logic in controllers
- Controllers never access repositories directly
- Request context must flow via DI, not headers or cookies
- All data access through IRepository<T> (defined in Spendly.Shared.DataLayer)
- Centralized exception handling via middleware
- Request SessionId must be tracked for observability
- Never modify the repository pattern
