# Specification Updates Summary (2026-02-27)

All specification files have been expanded to eliminate AI assumptions and provide authoritative guidance for code generation.

## Files Updated

### 1. 02_ARCHITECTURE.md ✅
**Changes:**
- Clarified database: **MongoDB** (not relational - was incorrect before)
- Added data flow diagram
- Detailed layered architecture with project mappings
- Explained RequestContext flow via scoped DI
- Documented middleware pipeline order (critical)
- Added RequestContextViewModel pattern
- Documented FunctionResponse<T> response format
- Added filter details (LoggingActionFilter, CacheControlHeaderFilter, RequestContextFilter)

**Before:** 30 lines (generic)  
**After:** 180+ lines (concrete, with examples)

---

### 2. 03_BACKEND_SPEC.md ✅
**Changes:**
- Listed exact tech stack: .NET 10, MongoDB Driver, Serilog, FluentValidation, AspectCore, xUnit
- Documented full project structure (with folder hierarchy)
- Explained AppBootstrapper pattern (how to register services)
- DI conventions (scoped vs transient scopes)
- Controllers: no business logic rule with example
- Repository<T> pattern explained
- DTO mapping strategy: manual (no AutoMapper)
- Validation: FluentValidation with auto-discovery
- Logging: Serilog structured logging patterns
- Testing: xUnit required, examples provided
- Middleware order documented
- Build/run commands provided

**Before:** 30 lines (vague)  
**After:** 320+ lines (comprehensive)

---

### 3. 04_MOBILE_SPEC.md ✅
**Changes:**
- Marked TBD decisions: Framework (Flutter/React Native/Native), HTTP client, state management
- Clean Architecture layers detailed (Presentation, Domain, Data)
- API Client pattern (centralized, single HTTP instance)
- Authentication & token management flow
- Token storage security (Keychain/KeyStore, NOT SharedPreferences)
- Offline support strategy (if needed)
- Error handling mapping
- Project structure template (framework-agnostic)
- State management patterns (BLoC/Redux/etc.)
- Testing strategies per framework
- Build commands (flutter run, npm run android, xcodebuild)
- Configuration (dev/staging/prod)
- Security: SSL pinning, token refresh, no hardcoded credentials

**Before:** 19 lines (skeletal)  
**After:** 350+ lines (detailed)

**Critical:** Still TBD on framework choice - waiting on decision

---

### 4. 05_API_CONTRACTS.md ✅
**Changes:**
- Versioning strategy: path-based (`/api/v1/...`)
- Standard response format with examples (success, error, list)
- Authentication: Bearer token, JWT claims documented
- HTTP status codes: full table with meanings
- Error codes catalog: UNAUTHORIZED, VALIDATION_ERROR, DUPLICATE_EMAIL, etc.
- Pagination: query parameters (pageNumber, pageSize), response format
- Filtering & sorting conventions
- Common headers (Authorization, Content-Type, Accept-Language, User-Agent)
- Response headers (Cache-Control, X-Request-Id, X-RateLimit-*)
- Endpoint summary (Auth, User, Expense, Category, Report endpoints)
- OAuth endpoints for Google/Facebook
- Rate limiting (TBD if enabled): limits documented
- Breaking changes policy: 6-month deprecation, 12-month support window

**Before:** 27 lines (minimal)  
**After:** 300+ lines (complete)

**Critical Changes:**
- Endpoints listed with request/response shapes
- Error code catalog definitive (add new codes via spec update only)
- Versioning strategy locked in (no API changes without version bump)

---

### 5. 06_SECURITY_SPEC.md ✅
**Changes:**
- Secrets: Complete list (DB_PASSWORD, JWT_SECRET, GOOGLE_OAUTH_*, FACEBOOK_OAUTH_*)
- Environment variables vs cloud secrets manager
- JWT implementation: algorithm (HS256), claims structure, expiration (1 hour)
- Token validation steps (signature, expiry, claims extraction)
- Refresh token strategy (TBD if implemented)
- Authorization: RBAC pattern with example code
- CORS policy: origins whitelisted (dev, staging, prod)
- Input validation: FluentValidation examples (email, password strength)
- Password hashing: **PBKDF2 with 600k iterations** (OWASP 2023 standard)
- Password reset flow: time-limited tokens via email
- Data protection: PII rules, safe logging examples, safe response examples
- HTTPS enforcement: HSTS headers, TLS 1.2+
- OAuth flows: Google and Facebook integration steps
- Rate limiting (TBD): 1000 req/hr per user, 100 req/min per IP for auth endpoints
- Security review checklist (pre-production audit)
- Incident response: revoke secrets, invalidate sessions, notify users
- Audit logging: events to log (never log sensitive data)

**Before:** 26 lines (basic)  
**After:** 430+ lines (comprehensive)

**Critical Details:**
- Password hashing algorithm and iteration count specified (no guessing)
- CORS configuration documented (can't be changed without spec)
- JWT structure definitive
- Secrets list exhaustive

---

### 6. 07_BACKGROUND_JOBS.md ✅
**Changes:**
- Principles: non-blocking, idempotent, logged, reliable, monitored
- Mechanism options: Hangfire (recommended), AWS SQS, Hosted Service, RabbitMQ
- Job types planned: Email, Reports, Data Sync, Audit Logging, Expense AI
- Idempotency implementation: idempotency keys, check-before-execute pattern
- Retry policy: exponential backoff formula, max 5 retries, dead letter queue
- Error handling: non-retryable vs retryable errors with examples
- Monitoring: metrics (success rate, duration, queue depth), logging format
- Hangfire dashboard details
- Alerts: DLQ depth, failure rate, duration, queue growth
- Job configuration table (timeout, retries per job type)
- Timeout management with CancellationToken
- Testing: unit test examples, integration test strategy
- Security: no hardcoded creds, rate limiting external APIs
- Scaling: single server vs multi-server vs serverless
- Future enhancements: job prioritization, deduplication, grouping, chaining

**Before:** 18 lines (principles only)  
**After:** 380+ lines (complete)

**TBD:** Mechanism not yet chosen (Hangfire recommended but awaiting decision)

---

## AGENTS.md Updated ✅

**Changes:**
- Updated to reference all new detailed specs
- Added "Critical First Step" reminder (follow 00_AI_RULES.md)
- Prioritized spec reading order
- Removed outdated assumptions
- Added "TBD (Pending Decisions)" section
- Enhanced "When Uncertain" escalation path

---

## Key Decisions Now Locked In

### Backend
- ✅ Framework: .NET 10
- ✅ Database: **MongoDB** (not SQL)
- ✅ Logging: Serilog
- ✅ Validation: FluentValidation
- ✅ Testing: xUnit
- ✅ DI: AppBootstrapper
- ✅ Response: FunctionResponse<T>
- ✅ Auth: JWT (Bearer token)
- ✅ Password hashing: PBKDF2 600k iterations

### API
- ✅ Versioning: Path-based `/api/v1/...`
- ✅ Response format: Standard JSON (success, data, error)
- ✅ Status codes: Documented per scenario
- ✅ Error codes: Definitive catalog (changes require spec update)
- ✅ Pagination: pageNumber/pageSize with total/totalPages

### Security
- ✅ Secrets: Environment variables only (list: DB_PASSWORD, JWT_SECRET, *_OAUTH_*)
- ✅ Password: PBKDF2 600k iterations
- ✅ CORS: AngularClient policy (origins configurable)
- ✅ OAuth: Google + Facebook
- ✅ PII: Never logged

---

## Still TBD (Awaiting Decisions)

### Mobile ❓
- Framework: Flutter vs React Native vs Native?
- HTTP Client library?
- State management: BLoC vs Redux vs Riverpod?
- Offline sync: Required or not?

### Backend ❓
- Background jobs mechanism: Hangfire vs SQS vs Hosted Service?
- Refresh tokens: Implement or JWT-only?
- Rate limiting: Enable immediately or defer?
- Encryption at rest: Needed for sensitive fields?

---

## How to Use These Specs

### For AI Code Generation
1. Always start prompt with: "Follow /specs/00_AI_RULES.md strictly"
2. Reference specific spec sections if rules unclear
3. Never assume; if detail missing, check TBD list or ask
4. All architectural decisions in specs are authoritative

### For Code Reviews
- Check API changes against 05_API_CONTRACTS.md
- Check security changes against 06_SECURITY_SPEC.md
- Check async patterns against 07_BACKGROUND_JOBS.md
- Check project structure against 03_BACKEND_SPEC.md

### For Onboarding
- New team members: Read specs in order (00 → 01 → 02 → 03 → rest)
- New API features: Check 05_API_CONTRACTS.md for shape
- New security concerns: Check 06_SECURITY_SPEC.md
- New async tasks: Check 07_BACKGROUND_JOBS.md

---

## Next Steps to Unlock AI Code Generation

1. **Decide on mobile framework** (update 04_MOBILE_SPEC.md)
   - Option A: Flutter (Dart) - single codebase iOS/Android
   - Option B: React Native (JS/TS) - if web app also needed
   - Option C: Native (Swift + Kotlin) - if separate iOS/Android teams

2. **Decide on background jobs** (update 07_BACKGROUND_JOBS.md)
   - Option A: Hangfire (simple, recommended)
   - Option B: AWS SQS (serverless, more complex)
   - Option C: Hosted Service (minimal dependencies, less scaling)
   - Option D: RabbitMQ (robust, requires separate server)

3. **Decide on refresh tokens** (update 06_SECURITY_SPEC.md)
   - Option A: Implement (better UX, more secure)
   - Option B: JWT-only (simpler, less secure)

4. **Review TBD questions** in each spec and decide

Once these 3-4 decisions made, run:
```bash
git add specs/
git commit -m "specs: finalize decisions for mobile framework, background jobs, refresh tokens"
```

Then AI can generate complete, assumption-free code for any feature.

---

## Files Sizes (Before → After)

| File | Before | After | Growth |
|------|--------|-------|--------|
| 00_AI_RULES.md | 184 lines | 184 lines | (unchanged) |
| 01_PROJECT_OVERVIEW.md | 34 lines | (unchanged) | (unchanged) |
| 02_ARCHITECTURE.md | 30 lines | 183 lines | +610% |
| 03_BACKEND_SPEC.md | 30 lines | 323 lines | +977% |
| 04_MOBILE_SPEC.md | 19 lines | 351 lines | +1747% |
| 05_API_CONTRACTS.md | 27 lines | 305 lines | +1030% |
| 06_SECURITY_SPEC.md | 26 lines | 432 lines | +1562% |
| 07_BACKGROUND_JOBS.md | 18 lines | 383 lines | +2028% |
| **Total** | **368 lines** | **2161 lines** | **+487%** |

All specs now comprehensive, decision-locked, and ready for AI code generation.

