# BACKEND SPECIFICATION

## Technology Stack (Required)

- **Framework:** ASP.NET Core (.NET 10)
- **Database:** MongoDB (via official MongoDB.Driver)
- **Logging:** Serilog (structured logging with request enrichment)
- **Validation:** FluentValidation + FluentValidation.AspNetCore
- **AOP:** AspectCore.Extensions.DependencyInjection (for method interception)
- **DI:** Microsoft.Extensions.DependencyInjection (built-in)
- **Caching:** Microsoft.Extensions.Caching.Memory (IMemoryCache)
- **Testing:** No unit tests required
- **Localization:** Custom TranslationService (Spendly.Shared.Localization)

## Project Structure

**Backend Projects:**

```
Api/
├── Spendly.Mobile.Api/              # Main API - Controllers only
│   ├── Controllers/                 # Request handlers (no business logic)
│   ├── Infrastructure/              # Middleware, Filters, Interceptors
│   │   ├── Middleware/              # RequestSessionMiddleware, SerilogContextEnricher, etc.
│   │   └── Filters/                 # ActionFilters (LoggingActionFilter, CacheControlHeaderFilter, RequestContextFilter)
│   └── Program.cs                   # Bootstrap via AppBootstrapper
│
├── Spendly.Mobile.BusinessLayer/    # Application services & business logic
│   └── Services/                    # UserService, ExpenseService, etc. (no interfaces)
│
├── Spendly.Mobile.ViewModels/       # API DTOs for this API
│   └── [Feature]ViewModel.cs
│
├── Spendly.Shared.Core/             # Cross-cutting concerns
│   ├── Bootstrap/                   # AppBootstrapper.cs (DI setup)
│   ├── Logging/                     # Logger initialization
│   ├── Interception/                # Method interceptors (CacheableMethodInterceptor)
│   ├── MongoSerialization/          # BSON custom serializers
│   ├── PasswordHelper.cs            # PBKDF2 hashing utility
│   └── StringExtensions.cs
│
├── Spendly.Shared.DataLayer/        # Data access layer
│   ├── Repository.cs                # Generic Repository<T> implementation
│   ├── IRepository.cs               # IRepository<T> interface - use this, never modify
│   └── PagingParameter.cs           # Pagination helper
│
├── Spendly.Shared.Entities/         # Domain models
│   ├── Core/                        # BaseEntity, IEntity interfaces
│   └── [Domain]/                    # User, Expense, Category, etc.
│
├── Spendly.Shared.Enums/            # Shared enumerations
│   └── Enums.cs
│
├── Spendly.Shared.Localization/     # Multi-language support
│   ├── MessageCodes.cs              # All validation/error message codes as constants
│   ├── TranslationService.cs
│   └── Translations/                # JSON translation files
│
└── Spendly.Shared.ViewModels/       # Shared response models
    ├── FunctionResponse.cs          # Generic success/failure response
    ├── RequestContextViewModel.cs   # Current user context
    └── Settings/                    # Configuration view models
```

## Dependency Injection Pattern

Use **AppBootstrapper** (in Spendly.Shared.Core/Bootstrap):

```csharp
var app = AppBootstrapper
    .Create("AppName", args)
    .WithServiceScanning(assemblies)
    .ConfigureServices((services, config) =>
    {
        services.AddScoped<UserService>();
        services.AddTransient<MyInterceptor>();
    })
    .ConfigureCors((services, config) => { /* ... */ })
    .BuildWebApi();

app.Run();
```

**DI Conventions:**
- Register concrete service classes directly - do NOT create service interfaces
- Services must NOT derive from interfaces (e.g., no IUserService)
- Use `WithServiceScanning()` for auto-registration by convention
- Scoped: Database repositories, RequestContextViewModel, Services
- Transient: Validators, Interceptors
- Singleton: Configuration, LoggerFactory, Cache managers

## Controllers & Request Handling

**Rules:**
- Controllers contain **only** request/response mapping
- No business logic in controllers
- Controllers never access repositories directly
- All logic in business layer services
- On success: return `Ok(response.Data)`
- On failure: return `this.BadRequestFrom(response)`
- Use FluentValidation for input validation (auto via middleware)

### Controller

Controllers must not perform validations such as user id checks; `RequestContextViewModel` is populated automatically (via `RequestContextFilter`) and user-related validation should be performed in the business layer.

All validations must be implemented as FluentValidation validators. For request-taking operations (e.g., PUT, POST), required fields must be validated using these validators.

**Example:**
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController(CategoryService categoryService)
{
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
}
```

## Business Layer

If a service requires a userId, add `RequestContextViewModel` as a constructor parameter of the service and obtain the userId from it; do not pass userId or `RequestContextViewModel` from controllers.

If an unexpected error occurs (for example: an id arrives as a string in the request but cannot be parsed to an `ObjectId`), do not return a `Function Failure`; instead throw an exception that includes the affected Id in the message. `Function Failure` should be used only for business-rule failures that result in a 400 response to the UI.

Some of the DB entities has IsDeleted property and DeletedAt property. This is handled in the repository, so dont include them in query, dont try to update them, just ignore them. The repository will automatically filter out deleted records and set these properties when deleting.

All entities has CreatedAt and UpdatedAt properties. The repository will automatically set these when inserting or updating records, so do not set them manually in the service layer.

## Data Access

**Repository Pattern:**

```csharp
public interface IRepository<T> where T : BaseEntity
{
	public string CollectionName { get; set; }
	Task<T?> GetAsync(Expression<Func<T, bool>> filter);
	Task<T?> GetAsync(string id);
	Task<T?> GetAsync(ObjectId id);
	Task<T?> GetAsync(Expression<Func<T, bool>> filter, ProjectionDefinition<T> projectionDefinition);
	Task<T?> GetAsync(FilterDefinition<T> filterDefinition, ProjectionDefinition<T> projectionDefinition);
	Task<T?> GetAsync(ObjectId id, ProjectionDefinition<T> projectionDefinition);
	Task<T?> GetAsync(string id, ProjectionDefinition<T> projectionDefinition);
	Task<T> GetRequiredAsync(string id);
	Task<T> GetRequiredAsync(ObjectId id);
	Task<T> GetRequiredAsync(FilterDefinition<T> filterDefinition);
	Task<T> GetRequiredAsync(Expression<Func<T, bool>> filter);
	Task InsertAsync(T model);
	Task UpdateAsync(T model);
	Task<UpdateResult> UpdateWithIdAsync(ObjectId id, UpdateDefinition<T> updateDef);
	Task<UpdateResult> UpdateAsync(FilterDefinition<T> filterDefinition, UpdateDefinition<T> updateDefinition);
	Task DeleteAsync(string id);
	Task DeleteAsync(ObjectId id);
	//Task<List<T>> ListAsync(FilterDefinition<T> filterDefinition);
	Task<List<T>> ListAsync(Expression<Func<T, bool>> filter, SortDefinition<T> sortDefinition, int limit);
	Task<List<T>> ListPagingAsync(Expression<Func<T, bool>>? filter = null, PagingParameter? paging = null, SortDefinition<T>? sortDefinition = null);
	Task<List<T>> ListAsync(FilterDefinition<T> filter, ProjectionDefinition<T>? projection = null);
	Task<List<T>> ListAsync(Expression<Func<T, bool>>? filter, ProjectionDefinition<T>? projection = null);
	Task<Dictionary<ObjectId, T>> ListAsync(IEnumerable<ObjectId> ids);
	Task<ListPagingAsync(FilterDefinition<T> filterDefinition, ProjectionDefinition<T> projectionDefinition, PagingParameter paging);
	Task<long> CountAsync(FilterDefinition<T> filterDefinition);
	Task<Dictionary<ObjectId, T>> ListAsync(IEnumerable<ObjectId> ids,
		Expression<Func<T, ObjectId?>> propertySelector);

}

```

**Rules:**
- IRepository<T> is defined in Spendly.Shared.DataLayer - use it as-is
- Never modify the repository pattern or IRepository interface
- Generic `Repository<T>` in Spendly.Shared.DataLayer implements IRepository<T>
- MongoDB via MongoDB.Driver NuGet package
- No migrations (schema-less), but enforce document structure in code
- All operations async (ConfigureAwait(false))
- Data ownership checks always performed in the service layer, never in controllers
- When querying a data with Id if the record must be in the db according to domein rules, use GetRequiredAsync to throw if not found. Use GetAsync when record may not exist.
- When converting id strings to MongoDB `ObjectId`, use the `ToObjectId()` string extension method where appropriate.

**Connection:**
- Username/password from shared.local.json via DbSettings class
- Collation: "en" with secondary strength (case-insensitive)
- Database: Name from DbSettings

## DTOs & Entity Mapping

**Strategy:** Manual mapping (no AutoMapper currently)

**Conventions:**
- Request DTOs: `[Feature]Request`, `[Feature]CreateRequest`, `[Feature]UpdateRequest`
- Response DTOs: `[Feature]Response`, `[Feature]ViewModel`
- Entities live in Spendly.Shared.Entities/ 
- DTOs live in `*ViewModels` projects
- Mapping in business layer services (not in controllers)
- All enums should be placed in Enums project Enums.cs file

## Validation

**Approach:** FluentValidation with automatic discovery

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage(MessageCodes.Validation.EmailRequired);
        RuleFor(x => x.Email).EmailAddress().WithMessage(MessageCodes.Validation.EmailInvalid);
    }
}
```

**Rules:**
- All validation messages must come from `MessageCodes` constants (Spendly.Shared.Localization)
- Do NOT perform length validation (no MaximumLength, MinimumLength checks)
- Auto-discovered via `AddValidatorsFromAssemblyContaining<Program>()`
- Invalid requests return 400 with error details (via ControllerExtensions.InvalidModelStateResponse)

## Coding Conventions

**Braces:** Always use `{}` for all scopes, even single-line blocks:
```csharp
if (condition)
{
    DoSomething();
}
```

**Variable Declaration:** Always use `var` for object creation:
```csharp
var user = new User();
var response = await userService.GetUser(id);
```

**Constants:** All constants grouped under a `Constants` class, organized by function. Never define constants directly in other classes:
```csharp
public static class Constants
{
    public static class Jwt
    {
        public const string Issuer = "spendly";
    }
    
    public static class Cache
    {
        public const int DefaultExpiryMinutes = 60;
    }
}
```

**Sensitive Configuration:** All sensitive values (JWT secrets, DB passwords, connection strings) must be stored in `shared.local.json` (git-ignored). Never in appsettings.json or code.

## Logging

**Framework:** Serilog structured logging

**Rules:**
- Log at `Information` or `Error` level
- Never log PII (passwords, email addresses, tokens)
- Structured logging: use named properties, not format strings
- SessionId injected automatically by SerilogContextEnricherMiddleware
- Failed validations logged by LoggingActionFilter

## Testing

No test required

## Middleware & Filters

**Middleware (Program.cs order):**
1. RequestLocalization - sets culture
2. CORS - MobileClient policy
3. RequestSessionMiddleware - generates SessionId
4. SerilogContextEnricherMiddleware - enriches logs
5. SerilogRequestLogging - logs all requests

**Filters (MvcOptions):**
- LoggingActionFilter - logs action entry/exit
- CacheControlHeaderFilter - sets cache headers
- RequestContextFilter - populates RequestContextViewModel from claims

## Configuration

**Source:** appsettings.json + appsettings.Development.json + shared.local.json

**shared.local.json (git-ignored, sensitive overrides):**
```json
{
  "Db": {
    "Password": "your-db-password"
  },
  "JwtSettings": {
    "Secret": "your-jwt-secret"
  }
}
```

**Rules:** All sensitive information (JWT secrets, DB passwords, API keys) must be in `shared.local.json`. Never commit secrets to source control.

## Build & Run

```bash
dotnet build Api/Spendly.sln              # Full build
dotnet run -p Api/Spendly.Mobile.Api      # Run API
```

API listens on `https://localhost:5001` (or configured port)
