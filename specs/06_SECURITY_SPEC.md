# SECURITY SPECIFICATION

## Secrets Management

**Rule:** All sensitive values must be stored in `shared.local.json` (git-ignored), never committed to code.

**Required Secrets:**
- `Db.Password` - MongoDB database password
- `JwtSettings.Secret` - Private key for JWT signing (minimum 256 bits / 32 bytes)
- `GOOGLE_OAUTH_CLIENT_ID` - Google OAuth app ID
- `GOOGLE_OAUTH_CLIENT_SECRET` - Google OAuth secret
- `FACEBOOK_OAUTH_APP_ID` - Facebook OAuth app ID
- `FACEBOOK_OAUTH_APP_SECRET` - Facebook OAuth secret

**Configuration:**
- Development: `shared.local.json` at project root (git-ignored)
- Never log secret values
- Never put secrets in appsettings.json or code

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

**Resource-level authorization:**
- Controllers NEVER access repositories directly
- Data ownership checks ALWAYS performed in the service layer
- Services verify that the requesting user owns the resource before any operation

```csharp
public async Task<FunctionResponse<ExpenseViewModel>> GetExpense(string id)
{
    var expense = await repository.GetAsync(id);
    if (expense.UserId != requestContext.UserId)
    {
        return FunctionResponse<ExpenseViewModel>.Failure(MessageCodes.Auth.Forbidden);
    }
    return FunctionResponse<ExpenseViewModel>.Success(expense.ToViewModel());
}
```

## CORS Policy

**Client:** Mobile app (React Native) - no browser CORS required for native HTTP calls.

**Configuration (Program.cs):**
```csharp
services.AddCors(options =>
{
    options.AddPolicy("MobileClient", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

**Note:** React Native apps do not send browser CORS preflight requests. The policy name `MobileClient` is used in middleware pipeline.

## Input Validation

**Server-Side Validation (Mandatory):**
- All inputs must be validated server-side
- Client-side validation is UX only, not security

**Strategy:** FluentValidation in ASP.NET Core

**Rules:**
- All validation messages must use `MessageCodes` constants (never hardcoded strings)
- Do NOT perform length validation (no MaximumLength, MinimumLength)

**Example:**
```csharp
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(MessageCodes.Validation.EmailRequired)
            .EmailAddress().WithMessage(MessageCodes.Validation.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(MessageCodes.Validation.PasswordRequired);
    }
}
```

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
