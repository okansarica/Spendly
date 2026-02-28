# SECURITY SPECIFICATION

## Secrets Management

**Rule:** All secrets must be stored in environment variables, never committed to code.

**Required Secrets:**
- `DB_PASSWORD` - MongoDB database password
- `JWT_SECRET` - Private key for JWT signing (minimum 256 bits / 32 bytes)
- `GOOGLE_OAUTH_CLIENT_ID` - Google OAuth app ID
- `GOOGLE_OAUTH_CLIENT_SECRET` - Google OAuth secret
- `FACEBOOK_OAUTH_APP_ID` - Facebook OAuth app ID
- `FACEBOOK_OAUTH_APP_SECRET` - Facebook OAuth secret
- `EMAIL_SMTP_PASSWORD` - (if transactional emails needed)
- `ENCRYPTION_KEY` - (if data encryption needed)

**Configuration:**
- Development: Use `.env` file (git-ignored)
- Production: Same as dev
- Never log secret values

## Authentication

**Method:** JWT (JSON Web Tokens)

**JWT Implementation:**

**Token Structure:**
```
Header.Payload.Signature
```

**Header:**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload (Claims):**
```json
{
  "sub": "507f1f77bcf86cd799439011",     // User ID
  "name": "user@example.com",             // Username/Email
  "iat": 1645000000,                      // Issued at (Unix timestamp)
  "exp": 1645003600,                      // Expiration (1 hour later)
  "iss": "https://api.spendly.io",        // Issuer
  "aud": "spendly-mobile",                // Audience
  "roles": ["user"]                       // Roles/Permissions
}
```

**Signing:**
- Algorithm: HMAC-SHA256
- Secret: Store in JWT_SECRET env var (256+ bits)
- Never share secret

**Token Lifetime:**
- Access token: 1 hour
- Refresh token: 7 days (TBD if implemented)

**Usage:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Token Validation (Server):**
1. Extract token from Authorization header
2. Verify signature using JWT_SECRET
3. Check token expiration
4. Extract claims (sub, roles, etc.)
5. Verify user exists and is active
6. Set RequestContextViewModel.UserId from "name" claim

**Token Refresh :**
Already implement in custom http interceptor dont change

## Authorization

**Pattern:** Role-based access control (RBAC)

**Roles:**
No role required

**Implementation:**
```csharp
[HttpPost("api/v1/expenses")]
public async Task<IActionResult> CreateExpense(CreateExpenseRequest request)
{
    // Only users with "user" role can access
}
```

**Resource-level authorization:**
Users can only access their own data:
```csharp
[HttpGet("api/v1/expenses/{id}")]
public async Task<IActionResult> GetExpense(string id)
{
    var expense = await _repository.GetAsync(id);
    if (expense.UserId != RequestContext.UserId)
        throw new Exception($"User not authorized to access this expense. UserId: {RequestContext.UserId} ExpenseUserId: {expense.UserId}");
    return Ok(expense);
}
```

## CORS Policy

**Frontend Origins (Allowed):**
- Development: `http://localhost:4200`, `https://localhost:4200`, `http://localhost:4300`, `https://localhost:4300`
- Staging: `https://staging.spendly.io`
- Production: `https://app.spendly.io`

**Configuration (Program.cs):**
```csharp
services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
```

**Allowed Methods:** GET, POST, PUT, DELETE, OPTIONS

**Allowed Headers:** Content-Type, Authorization, Accept, Accept-Language

**Credentials:** Allowed (for cookies if used)

**Preflight:** Automatically handled

## Input Validation

**Server-Side Validation (Mandatory):**
- All inputs must be validated server-side
- Client-side validation is UX only, not security

**Strategy:** FluentValidation in ASP.NET Core

**Example:**
```csharp
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain uppercase")
            .Matches("[a-z]").WithMessage("Password must contain lowercase")
            .Matches("[0-9]").WithMessage("Password must contain digit");
    }
}
```

**Validation Rules:**
- Email: Valid format, max 255 chars, unique in database
- Password: Min 8 chars, 1 uppercase, 1 lowercase, 1 digit
- Amount: Positive number, max 2 decimals
- Date: Valid ISO 8601 format
- Description: Max 500 chars, no HTML/script injection

## Password Hashing

**Algorithm:** PBKDF2 with HMAC-SHA256

**Implementation (Spendly.Shared.Core/PasswordHelper.cs):**
```csharp
public static string HashPassword(string password)
{
    byte[] salt = RandomNumberGenerator.GetBytes(16);
    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: password,
        salt: salt,
        prf: KeyDerivationPrf.HMACSHA256,
        iterationCount: 600000,  // OWASP 2023 recommendation
        numBytesRequested: 32));
    return $"{Convert.ToBase64String(salt)}.{hashed}";
}
```

**Requirements:**
- Iteration count: 600,000 (OWASP minimum for 2023)
- Salt: 16 bytes random
- Hash length: 32 bytes
- Never store plain-text passwords
- Never send passwords in logs or responses

**Password Reset Flow:**
1. User requests password reset
2. Generate time-limited reset token (valid 1 hour)
3. Send reset link via email (not API response)
4. User clicks link, creates new password
5. Server validates token and hashes new password
6. Invalidate all existing sessions (force re-login)

## Data Protection

**Personally Identifiable Information (PII):**
- Email addresses
- Passwords
- Phone numbers (if added)
- User preferences

**Protection Rules:**
1. Never log PII
2. Never send in error responses
3. Use encryption at rest for sensitive fields (TBD if needed)
4. Use HTTPS for all communication (enforced)
5. Delete on user request (GDPR compliance)

**Example - Safe Logging:**
```csharp
// ❌ WRONG - logs PII
logger.LogInformation("User registered: {Email}", user.Email);

// ✅ CORRECT - logs ID only
logger.LogInformation("User registered with ID {UserId}", user.Id);
```

**Example - Safe Responses:**
```csharp
// ❌ WRONG - exposes password hash
return new { id = user.Id, email = user.Email, passwordHash = user.PasswordHash };

// ✅ CORRECT - only public data
return new { id = user.Id, email = user.Email, createdAt = user.CreatedAt };
```

## HTTPS Enforcement

**Rules:**
- All API endpoints served over HTTPS only
- HTTP traffic redirected to HTTPS
- HSTS header enabled (Strict-Transport-Security)
- Certificate: Valid, non-expired, matches domain
- TLS 1.2 minimum (preferably 1.3)

**Configuration:**
```csharp
app.UseHsts();
app.UseHttpsRedirection();
```

## OAuth Integration

**Google OAuth:**
- Redirect to: `https://accounts.google.com/o/oauth2/v2/auth`
- Scopes: `openid profile email`
- Client ID/Secret: From Google Cloud Console
- Callback: `https://api.spendly.io/api/v1/auth/oauth/google/callback`
- Handle: ID token validation, user creation/lookup, token issue

**Facebook OAuth:**
- Similar flow to Google
- Scopes: `public_profile email`
- Client ID/Secret: From Facebook App Dashboard
- Handle same as Google

**Flow (Mobile):**
1. Mobile app redirects to OAuth provider
2. User authorizes in provider UI
3. Provider redirects back to app with authorization code
4. App sends code to backend
5. Backend exchanges code for ID token (server-to-server)
6. Backend validates ID token signature
7. Create or lookup user by email
8. Issue JWT token to mobile app
9. App stores JWT in secure storage

## Rate Limiting (TBD)

**If implemented:**
- Per-user: 1000 requests/hour
- Per-IP: 100 requests/minute (login, register, password reset)
- Per-IP: 10 requests/minute (same endpoint)
- Response: 429 Too Many Requests + Retry-After header

## Security Review Checklist

Before deployment to production:
- [ ] All secrets in environment variables
- [ ] HTTPS enabled
- [ ] JWT secret is strong (256+ bits)
- [ ] Password hashing with 600k iterations
- [ ] Input validation on all endpoints
- [ ] No PII in logs
- [ ] CORS properly configured
- [ ] SQL injection impossible (using ORM/parameterized queries)
- [ ] XSS protection (Content-Type: application/json)
- [ ] CSRF tokens (if form submissions used)
- [ ] Rate limiting configured
- [ ] Security headers present (HSTS, X-Content-Type-Options, X-Frame-Options)
- [ ] Dependencies scanned for vulnerabilities
- [ ] OAuth tokens stored securely on mobile

## Incident Response

**If security incident detected:**
1. Immediately revoke JWT_SECRET and issue new one (all users must re-login)
2. Invalidate all active sessions
3. Review logs for compromise
4. Notify affected users
5. Document in security log
6. Implement fix and deploy
7. Conduct post-mortem

## Audit Logging

**Events to log (never sensitive data):**
- User login (success/failure)
- User registration
- Password changed
- Permission changed
- Data deleted (by whom, what, when)
- Failed authentication attempts (IP, count)

**Never log:**
- Passwords
- Tokens
- Credit card data
- Detailed error messages (user sees generic error, server logs details)
