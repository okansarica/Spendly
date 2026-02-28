# Pre-Generation Checklist

Complete this checklist BEFORE asking AI to generate code.

## Must Complete (Blocking)

### Mobile Framework Decision ❌ REQUIRED
- [ ] Choose: Flutter (Dart) / React Native (JS) / Native (Swift+Kotlin)
- [ ] If chosen, update `/specs/04_MOBILE_SPEC.md` "Technology Stack" section with:
  - Framework and version
  - State management library
  - HTTP client library
  - Navigation library
  - Local storage library for tokens
  - Testing framework
  - Build toolchain

**Impact:** AI cannot generate mobile code without this.

---

### Background Jobs Mechanism ❌ REQUIRED
- [ ] Choose: Hangfire / AWS SQS / Hosted Service / RabbitMQ
- [ ] If chosen, update `/specs/07_BACKGROUND_JOBS.md` "Mechanism (TBD)" with:
  - Selected option
  - NuGet packages (or dependencies)
  - Configuration in Program.cs
  - Example job implementation
  - Remove other options (clean up)

**Impact:** AI cannot implement async tasks without this.

---

### Refresh Token Strategy ❌ REQUIRED
- [ ] Decide: Implement refresh tokens / JWT-only (short-lived)
- [ ] If yes, update `/specs/06_SECURITY_SPEC.md` "Token Refresh" with:
  - Refresh token lifetime (e.g., 7 days)
  - Refresh endpoint shape (POST /api/v1/auth/refresh)
  - Storage on mobile (secure, rotating)
  - Revocation strategy (logout = invalidate)
- [ ] If no, add comment: "Decided: JWT-only, refresh not implemented. Mobile must re-login on token expiry."

**Impact:** Affects auth flow design.

---

## Should Complete (High Priority)

### Error Codes Catalog ⚠️ RECOMMENDED
- [ ] Review `/specs/05_API_CONTRACTS.md` error codes catalog
- [ ] Add domain-specific codes if needed:
  - Bank sync errors? (e.g., SYNC_FAILED, BANK_CREDENTIALS_INVALID)
  - Transaction import errors? (e.g., DUPLICATE_TRANSACTION, INVALID_CSV)
  - Report generation errors? (e.g., NO_DATA_FOR_PERIOD)
- [ ] **All new error codes must be added to spec BEFORE feature implementation**

**Impact:** Ensures consistency across all endpoints.

---

### Rate Limiting Decision ⚠️ RECOMMENDED
- [ ] Decide: Enable rate limiting / defer to later
- [ ] If yes, update `/specs/05_API_CONTRACTS.md` "Rate Limiting":
  - Per-user limit (e.g., 1000 req/hour)
  - Per-IP limit for auth endpoints (e.g., 100 req/minute)
  - Per-IP limit for general endpoints (e.g., 10 req/minute)
  - Response behavior (429 status, retry-after header)
- [ ] Implement middleware or attribute

**Impact:** Affects API performance and security.

---

### Encryption at Rest Decision ⚠️ RECOMMENDED
- [ ] Decide: Encrypt sensitive fields / store plaintext (risky)
- [ ] If yes, update `/specs/06_SECURITY_SPEC.md` "Data Protection" with:
  - Which fields encrypted (password hashes exempt; consider: email, phone, SSN?)
  - Encryption algorithm (e.g., AES-256)
  - Key storage (environment variable, key management service?)
  - Encryption/decryption in Repository layer?

**Impact:** Security posture of stored data.

---

## Nice to Have (Can Do Anytime)

- [ ] Add ADR (Architectural Decision Record) folder:
  - Create `specs/ADR/` folder
  - Document major decisions (mobile framework, background jobs, etc.)
  - Example: `ADR-001-MOBILE-FRAMEWORK.md`

- [ ] Add API endpoint examples to `05_API_CONTRACTS.md`:
  - Full cURL examples per endpoint
  - Response examples (success and error)
  - Validation error examples

- [ ] Add database schema documentation:
  - Expected MongoDB collections and document structure
  - Field validation rules
  - Indexes recommended

- [ ] Add deployment checklist:
  - Environment variables required
  - Database setup steps
  - Secret rotation procedure
  - Health check endpoints

---

## How to Mark Decisions in Specs

Once decided, use this format:

### Example: Decided on Flutter

```markdown
## Technology Stack (DECIDED: Flutter)

**Framework:** Flutter (Dart)
- Version: 3.16.0+
- Package management: pub
- ...rest of stack...

**Historical Note:** Decided 2026-02-27. Other options considered: React Native (not chosen due to web complexity), Native (not chosen due to team expertise).
```

### Example: Decided on Hangfire

```markdown
### Selected Mechanism: Hangfire ✅

- **Package:** `Hangfire.Core` + `Hangfire.MongoDB`
- **Configuration:** See Program.cs example below
- **Storage:** MongoDB (same database as app)
- **Limitations:** Not distributed by default (use Hangfire.Pro for HA)

**Historical Note:** Decided 2026-02-27. AWS SQS rejected (cost), Hosted Service rejected (no persistence on restart).
```

---

## Verification Checklist

Before marking "ready for AI code generation", verify:

- [ ] **02_ARCHITECTURE.md:** MongoDB confirmed (not SQL)
- [ ] **03_BACKEND_SPEC.md:** .NET 10, project structure clear, AppBootstrapper usage clear
- [ ] **04_MOBILE_SPEC.md:** Framework decided and documented (not TBD anymore)
- [ ] **05_API_CONTRACTS.md:** Error codes catalog complete, versioning strategy clear
- [ ] **06_SECURITY_SPEC.md:** Secrets list complete, auth flow clear, encryption decision made
- [ ] **07_BACKGROUND_JOBS.md:** Mechanism chosen and documented (not TBD anymore)
- [ ] **00_AI_RULES.md:** Reviewed and understood (especially sections 1, 3, 4, 6)
- [ ] **AGENTS.md:** Updated with latest spec references

---

## Final Sign-Off

Once all blocking items + most recommended items complete:

```bash
# Run this to verify no TBD conflicts
grep -r "TBD\|todo\|TODO\|FIXME" specs/ | grep -i "critical\|blocking\|must"

# Review any remaining TBDs
grep -r "TBD" specs/ | wc -l

# Commit changes
git add specs/
git commit -m "chore: finalize specs - all blocking decisions made, ready for AI code generation"
git log --oneline -5  # Verify commit
```

**Message to AI:** `All specs finalized. Follow AGENTS.md and /specs/00_AI_RULES.md. Ready to generate code for [feature name].`

---

## Common Issues & Resolutions

### Issue: "AI made wrong assumption about database"
**Prevention:** Ensure 02_ARCHITECTURE.md explicitly says "MongoDB (not relational)"
**Resolution:** Update spec and re-prompt with spec reference

### Issue: "AI added unauthorized library"
**Prevention:** Keep 03_BACKEND_SPEC.md tech stack list exhaustive and definitive
**Resolution:** Reject code, update spec, re-prompt

### Issue: "API response format inconsistent"
**Prevention:** Ensure 05_API_CONTRACTS.md shows examples for success/error/list cases
**Resolution:** Update spec, rollback code, re-prompt

### Issue: "Passwords not hashing correctly"
**Prevention:** Ensure 06_SECURITY_SPEC.md specifies PBKDF2 600k iterations exactly
**Resolution:** Reference spec in code review

### Issue: "Mobile app stores token insecurely"
**Prevention:** Ensure 04_MOBILE_SPEC.md explicitly says "Keychain/KeyStore, NOT SharedPreferences"
**Resolution:** Update spec, reject code, re-prompt

---

**All set?** Message: ✅ Specs finalized, ready for code generation.

