# CRITICAL DECISIONS - Complete These First

Use this document to record decisions. Copy relevant section, fill in, and commit to git.

---

## 1️⃣ MOBILE FRAMEWORK (BLOCKING)

**Status:** ❌ UNDECIDED

**Question:** What framework will we use for the mobile app?

**Options:**
```
A) Flutter (Dart)
   - Pros: Single codebase iOS/Android, fast, good ecosystem
   - Cons: Dart learning curve, smaller community than React Native
   - Effort: Medium (3-4 weeks setup + learning)

B) React Native (JavaScript/TypeScript)
   - Pros: JS community, can share code with web if needed
   - Cons: Performance concerns, bridge overhead, community fragmented
   - Effort: Medium (similar to Flutter)

C) Native (Swift iOS + Kotlin Android)
   - Pros: Best performance, native feel, full platform capabilities
   - Cons: Requires 2 separate teams, 2x development effort, 2x maintenance
   - Effort: High (6-8 weeks per platform)

D) Other: Xamarin, Ionic, NativeScript, etc.
   - ⚠️ Must justify decision with team expertise
```

**Decision:**
```
Chosen: [ ] A (Flutter)  [ ] B (React Native)  [ ] C (Native)  [ ] D (Other: _______)
Date: _______________
Owner: _______________
Justification: _________________________________________
```

**Once Decided:** Update `/specs/04_MOBILE_SPEC.md` section "Technology Stack" with exact:
- Framework name and version
- UI framework (Material / Cupertino / custom)
- State management (BLoC / Redux / Riverpod / Provider / etc.)
- HTTP client (Dio / http / etc.)
- Navigation (GoRouter / GetX / etc.)
- Local storage (flutter_secure_storage / react-native-keychain / etc.)
- Testing framework (flutter_test / Jest / etc.)

---

## 2️⃣ BACKGROUND JOBS MECHANISM (BLOCKING)

**Status:** ❌ UNDECIDED

**Question:** How will we handle async background jobs (emails, reports, syncs)?

**Options:**
```
A) Hangfire (Recommended for quick start)
   - Pros: Simple API, dashboard, persistent storage (MongoDB)
   - Cons: Requires storage layer, not distributed by default
   - Setup: 1-2 days
   - NuGet: Hangfire.Core, Hangfire.MongoDB

B) AWS SQS + Lambda
   - Pros: Serverless, scalable, managed
   - Cons: Vendor lock-in, cost at scale, operational complexity
   - Setup: 2-3 days (need AWS setup)
   - NuGet: AWSSDK.SQS, AWSSDK.Lambda

C) ASP.NET Core Hosted Service (Minimal approach)
   - Pros: No external dependencies, simple
   - Cons: Jobs lost on restart, not distributed, limited scaling
   - Setup: 1 day (minimum viable)
   - NuGet: None (built-in)

D) RabbitMQ + Separate Worker Service
   - Pros: Robust, scalable, open-source
   - Cons: Requires RabbitMQ server, more complex setup
   - Setup: 3-4 days
   - NuGet: RabbitMQ.Client, MassTransit

E) Other: Quartz.NET, Coravel, Mass Transit, etc.
   - ⚠️ Must justify addition to approved stack
```

**Decision:**
```
Chosen: [ ] A (Hangfire)  [ ] B (AWS SQS)  [ ] C (Hosted Service)  [ ] D (RabbitMQ)  [ ] E (Other: _______)
Date: _______________
Owner: _______________
Justification: _________________________________________
```

**Once Decided:** Update `/specs/07_BACKGROUND_JOBS.md` section "Mechanism (TBD)" with:
- Selected option name
- NuGet packages required
- Configuration code in Program.cs
- Example job implementation
- Remove other options section

**Timeline for first jobs:** Emails, Daily Reports, Bank Sync

---

## 3️⃣ REFRESH TOKEN STRATEGY (BLOCKING)

**Status:** ❌ UNDECIDED

**Question:** Should we implement refresh tokens or use short-lived JWT only?

**Options:**
```
A) Implement Refresh Tokens (More secure, better UX)
   - Access token lifetime: 1 hour
   - Refresh token lifetime: 7-30 days (configurable)
   - Endpoint: POST /api/v1/auth/refresh (refresh_token → new access token)
   - Mobile: Store both in secure storage (Keychain/KeyStore)
   - On logout: Invalidate refresh token (revocation list)
   - Pros: Better security (can revoke without password reset), better UX (no re-login)
   - Cons: More complex, requires token rotation, needs revocation DB
   - Setup: 1-2 days

B) JWT-Only (Simpler, less secure)
   - Access token lifetime: 8-24 hours (longer due to no refresh)
   - Refresh: Not available (must use password login again)
   - Mobile: Just store JWT
   - On logout: Clear storage (no server-side action)
   - Pros: Simpler, no revocation needed, stateless
   - Cons: Longer token = larger attack window, worse UX (re-login required)
   - Setup: Already implemented
```

**Decision:**
```
Chosen: [ ] A (Implement Refresh Tokens)  [ ] B (JWT-Only, short-lived)
Date: _______________
Owner: _______________
Access token lifetime: ______ hours
Refresh token lifetime: ______ days (if A chosen)
Justification: _________________________________________
```

**Once Decided:** Update `/specs/06_SECURITY_SPEC.md` section "Token Refresh" with decision and implementation details.

---

## 4️⃣ RATE LIMITING (HIGH PRIORITY)

**Status:** ⚠️ OPTIONAL (Can implement now or defer)

**Question:** Should we implement rate limiting on API endpoints?

**Options:**
```
A) Implement Rate Limiting (Recommended for production)
   - Per-user: 1000 requests/hour
   - Per-IP login/register: 100 requests/minute
   - Per-IP general: 10 requests/minute
   - Response: 429 Too Many Requests
   - Tracking: Redis or in-memory cache
   - Setup: 2-3 days

B) Defer Until Later
   - Implement security baseline first
   - Add rate limiting in Phase 2
   - Decision point: Launch vs Post-launch
```

**Decision:**
```
Chosen: [ ] A (Implement now)  [ ] B (Defer to Phase 2)
Date: _______________
Owner: _______________
Justification: _________________________________________
```

**Once Decided:** If A, update `/specs/05_API_CONTRACTS.md` "Rate Limiting" section with configuration.

---

## 5️⃣ ENCRYPTION AT REST (MEDIUM PRIORITY)

**Status:** ⚠️ OPTIONAL

**Question:** Should we encrypt sensitive data in MongoDB?

**Options:**
```
A) Encrypt Sensitive Fields (Recommended for compliance)
   - Algorithm: AES-256
   - Fields to encrypt: (TBD - at least: passwords [already hashed], email, phone?)
   - Key storage: Environment variable (ENCRYPTION_KEY)
   - Implementation: Repository layer (transparent to business logic)
   - Pros: GDPR compliance, defense-in-depth
   - Cons: Performance overhead, key rotation complexity
   - Setup: 2-3 days

B) No Encryption (Acceptable for MVP)
   - Rely on database access controls
   - Risk: If DB compromised, PII exposed
   - Decision point: Launch -> Post-launch if needed
```

**Decision:**
```
Chosen: [ ] A (Encrypt now)  [ ] B (Skip for MVP)
Date: _______________
Owner: _______________
Justification: _________________________________________
```

**Once Decided:** If A, update `/specs/06_SECURITY_SPEC.md` "Data Protection" section with field list and implementation.

---

## HOW TO FILL THIS OUT

1. **Review each section** with team
2. **Make decision** for each BLOCKING item (1-3)
3. **Fill in blanks:**
   - Check the option box: `[X]`
   - Write date: `2026-02-27`
   - Write owner/team: `Your Name`
   - Write justification: Short reason why
4. **Update corresponding spec file** (link provided)
5. **Commit to git:**
   ```bash
   git add CRITICAL_DECISIONS.md specs/
   git commit -m "feat: finalize critical decisions - mobile framework, background jobs, refresh tokens"
   ```

---

## EXAMPLE: Completed Decision

```markdown
## 1️⃣ MOBILE FRAMEWORK (BLOCKING)

**Status:** ✅ DECIDED

**Decision:**
```
Chosen: [X] A (Flutter)
Date: 2026-02-27
Owner: Tech Lead
Justification: Team has Flutter expertise, single codebase for iOS/Android reduces 
maintenance burden, good for 6-month MVP timeline. React Native considered but team 
prefers Dart. Native rejected due to 2x effort.
```

**Spec Updated:** `/specs/04_MOBILE_SPEC.md` - Framework set to Flutter 3.16.0, BLoC 
for state management, Dio for HTTP, flutter_secure_storage for tokens.

---

## STATUS AFTER ALL DECISIONS

Once all BLOCKING decisions completed and specs updated:

```bash
# Verify no critical TBDs remain
grep -E "BLOCKING|CRITICAL" CRITICAL_DECISIONS.md | grep "❌"
# If output is empty: ✅ Ready
# If output has results: ⚠️ Still pending

# Then start code generation:
```

**Message to AI:**
```
All critical decisions made and specs finalized.
Follow /specs/00_AI_RULES.md strictly.
Generate [feature name] code with no assumptions.
```

---

**Status:** 🟠 Awaiting 3 blocking decisions (mobile framework, background jobs, refresh tokens)

**Next Step:** Complete this document with decisions, then message: "Ready for code generation"

