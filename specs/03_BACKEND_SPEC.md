# BACKEND SPECIFICATION

## Technology Stack (Required)

- **Framework:** ASP.NET Core (.NET 10)
- **Database:** MongoDB (via official MongoDB.Driver)
- **Logging:** Serilog (structured logging with request enrichment)
- **Validation:** FluentValidation + FluentValidation.AspNetCore
- **AOP:** AspectCore.Extensions.DependencyInjection (for method interception)
- **DI:** Microsoft.Extensions.DependencyInjection (built-in)
- **Caching:** Microsoft.Extensions.Caching.Memory (IMemoryCache)
- **Testing:** No unit tesst required
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
│   └── Services/                    # IUserService, IExpenseService, etc.
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
│   ├── IRepository.cs               # Interface definition
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
        services.AddScoped<IMyService, MyService>();
        services.AddTransient<MyInterceptor>();
    })
    .ConfigureCors((services, config) => { /* ... */ })
    .BuildWebApi();

app.Run();
```

**DI Conventions:**
- Register services in `ConfigureServices` callback
- Use `WithServiceScanning()` for auto-registration by convention
- Scoped: Database repositories, RequestContextViewModel
- Transient: Validators, Interceptors, stateless services
- Singleton: Configuration, LoggerFactory, Cache managers

## Controllers & Request Handling

**Rules:**
- Controllers contain **only** request/response mapping and validation
- No business logic in controllers
- All logic in business layer services
- Return `FunctionResponse<T>` or `FunctionResponse` (from Spendly.Shared.ViewModels)
- Use FluentValidation for input validation (auto via middleware)

**Example:**
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController(IUserService service, RequestContextViewModel context)
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

## Data Access

**Repository Pattern:**

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetAsync(string id);
    Task<T?> GetAsync(Expression<Func<T, bool>> filter);
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null);
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(string id);
}
```

**Implementation:**
- Generic `Repository<T>` in Spendly.Shared.DataLayer
- MongoDB via MongoDB.Driver NuGet package
- No migrations (schema-less), but enforce document structure in code
- All operations async (ConfigureAwait(false))

**Connection:**
- Username/password from appsettings via DbSettings class
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

## Validation

**Approach:** FluentValidation with automatic discovery

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}
```

**Registration:**
- Auto-discovered via `AddValidatorsFromAssemblyContaining<Program>()`
- Invalid requests return 400 with error details (via ControllerExtensions.InvalidModelStateResponse)

## Logging

**Framework:** Serilog structured logging

**Configuration (Program.cs):**
```csharp
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
    {
        diagCtx.Set("SessionId", RequestSession.Get(httpCtx));
        diagCtx.Set("UserId", httpCtx.Items["UserId"]);
    };
});
```

**Rules:**
- Log at `Information` or `Error` level
- Never log PII (passwords, email addresses, tokens)
- Structured logging: use named properties, not format strings
- Example: `logger.LogInformation("User {UserId} created expense {ExpenseId}", userId, expenseId)`
- SessionId injected automatically by SerilogContextEnricherMiddleware
- Failed validations logged by LoggingActionFilter

## Testing

No test required

## Middleware & Filters

**Middleware (Program.cs order):**
1. RequestLocalization - sets culture
2. CORS - AngularClient policy
3. RequestSessionMiddleware - generates SessionId
4. SerilogContextEnricherMiddleware - enriches logs
5. SerilogRequestLogging - logs all requests

**Filters (MvcOptions):**
- LoggingActionFilter - logs action entry/exit
- CacheControlHeaderFilter - sets cache headers
- RequestContextFilter - populates RequestContextViewModel from claims

## Configuration

**Source:** appsettings.json + appsettings.Development.json + environment variables

**Standard settings:**
```json
{
  "Logging": {
    "LogLevel": { "Default": "Information" }
  },
  "Db": {
    "DatabaseName": "spendly",
    "UserName": "admin",
    "Password": "${DB_PASSWORD}"
  }
}
```

**Environment variables:** Use for secrets (DB_PASSWORD, JWT_SECRET, etc.)
Use appsettings.development.json for shared settings and use shared.local.json for sensitive overrides (ignored in git)

## Build & Run

```bash
dotnet build Api/Spendly.sln              # Full build
dotnet run -p Api/Spendly.Mobile.Api      # Run API
dotnet test Api/Spendly.sln               # Run all tests
```

API listens on `https://localhost:5001` (or configured port)
