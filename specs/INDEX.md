# 📑 Complete File Index

**All specification and guidance files for Spendly project**  
**Total: 12 files, 3,242 lines, 89 KB**

---

## 🚀 START HERE (Read First)

1. **START_HERE.md** ← YOU ARE HERE
   - 5-minute high-level overview
   - Quick reference table
   - Next steps guide

2. **FINAL_DELIVERY_SUMMARY.md** ← Executive Summary
   - What was delivered
   - Key achievements
   - File size summary
   - Questions & answers reference table

---

## 🎯 For Decision-Making (Next Step)

3. **CRITICAL_DECISIONS.md** ← FILL THIS OUT
   - Section 1: Mobile Framework (Flutter/React Native/Native)
   - Section 2: Background Jobs (Hangfire/SQS/Hosted Service/RabbitMQ)
   - Section 3: Refresh Tokens (Implement/JWT-only)
   - Section 4: Rate Limiting (optional)
   - Section 5: Encryption at Rest (optional)
   - Time required: 65 minutes for blocking items + 30 min to update specs

---

## 📖 For Code Generation (Read When Ready)

4. **AGENTS.md** ← AI Coding Guide
   - How to prompt the AI
   - Specification hierarchy
   - When uncertain (decision flowchart)
   - Critical first step reminder
   - Backend project structure
   - Key technical patterns
   - TBD decisions list

5. **PRE_GENERATION_CHECKLIST.md** ← Verification Checklist
   - Blocking items (must complete)
   - High priority items (should complete)
   - Nice-to-have items (optional)
   - Verification checklist
   - Common issues & resolutions

6. **VERIFICATION_CHECKLIST.md** ← Final Check
   - File locations reference
   - Success metrics (before/after)
   - How to use specs
   - Next conversation format

---

## 📊 For Reference (Background Reading)

7. **SPEC_UPDATES_SUMMARY.md** ← Changelog
   - Detailed changes per file
   - Problems fixed
   - Growth statistics (368 → 2,161 lines)
   - Impact analysis per file
   - Decisions now locked in
   - Still TBD list
   - Next steps to unlock code generation

---

## 📋 Specification Files (The Authority)

**Core Rules & Overview:**

8. **specs/00_AI_RULES.md** ← MANDATORY RULES
   - 15 sections of AI implementation rules
   - Code preservation rules
   - Refactoring rules
   - Dependency policy
   - Security rules
   - API contract protection
   - Background jobs
   - Logging rules
   - Testing rules
   - Conflict resolution
   - Audit checklist

9. **specs/01_PROJECT_OVERVIEW.md** ← Context (Optional)
   - Project vision
   - Core goals
   - Authentication methods
   - User profile
   - Environments

**Architecture & Backend:**

10. **specs/02_ARCHITECTURE.md** ← System Design (READ)
    - High-level architecture diagram
    - Backend architecture (MongoDB, layered, middleware)
    - Mobile architecture
    - Data layer details (MongoDB specifics, Repository pattern)
    - API response format (FunctionResponse<T>)
    - Architecture rules
    - **KEY FIX: MongoDB confirmed (not "relational")**

11. **specs/03_BACKEND_SPEC.md** ← Backend Tech & Patterns (READ)
    - Technology stack (exact: .NET 10, MongoDB Driver, Serilog, FluentValidation, AspectCore, xUnit)
    - Project structure (8 projects mapped)
    - Dependency injection (AppBootstrapper pattern)
    - Controllers & request handling
    - Data access (Repository pattern)
    - DTOs & entity mapping
    - Validation (FluentValidation)
    - Logging (Serilog)
    - Testing (xUnit)
    - Middleware & filters
    - Configuration
    - Build & run commands

**API & Security:**

12. **specs/05_API_CONTRACTS.md** ← API Design (READ)
    - General rules & versioning (path-based: /api/v1/...)
    - Standard response format (success/error/list)
    - Authentication (Bearer JWT)
    - HTTP status codes (table)
    - Error codes catalog (20+ codes with meanings)
    - Pagination (pageNumber/pageSize)
    - Filtering & sorting
    - Common headers (Authorization, Content-Type, etc.)
    - Endpoint summary (Auth, User, Expense, Category, Report)
    - Rate limiting (TBD)
    - Breaking changes policy

13. **specs/06_SECURITY_SPEC.md** ← Security Requirements (READ)
    - Secrets management (list: DB_PASSWORD, JWT_SECRET, OAUTH_*, etc.)
    - Authentication (JWT: HS256, Bearer format, 1-hour expiry)
    - Authorization (RBAC with examples)
    - CORS policy (origins: dev/staging/prod)
    - Input validation (FluentValidation examples)
    - Password hashing (PBKDF2 with 600k iterations - OWASP 2023)
    - Data protection (PII rules, safe logging examples)
    - HTTPS enforcement
    - OAuth integration (Google, Facebook)
    - Rate limiting (TBD)
    - Security review checklist
    - Incident response
    - Audit logging

**Mobile & Async (TBD):**

14. **specs/04_MOBILE_SPEC.md** ← Mobile Design (TBD Framework Choice)
    - Technology stack (PENDING: framework, HTTP client, state management)
    - Clean Architecture layers (Presentation, Domain, Data)
    - API Client pattern (centralized, single instance)
    - Authentication & token management
    - Token storage (Keychain/KeyStore, not SharedPreferences)
    - Offline support (if needed)
    - Error handling
    - Project structure template
    - State management patterns
    - Testing strategies
    - Build & run commands
    - Configuration (dev/staging/prod)
    - Security (SSL pinning, token refresh, no hardcodes)

15. **specs/07_BACKGROUND_JOBS.md** ← Async Tasks (TBD Mechanism)
    - Principles (non-blocking, idempotent, logged, reliable)
    - Mechanism options (Hangfire recommended / AWS SQS / Hosted Service / RabbitMQ)
    - Job types (Email, Reports, Data Sync, Audit Logging, Expense AI)
    - Idempotency (implementation pattern)
    - Retry policy (exponential backoff, max 5 retries, DLQ)
    - Error handling (retryable vs non-retryable)
    - Monitoring (metrics, logging, alerts, dashboard)
    - Job configuration table (timeout, retries per type)
    - Testing strategies
    - Security
    - Scaling (single server / multi-server / serverless)

---

## 📊 File Reading Paths

### For AI Developers (15 min)
1. START_HERE.md (5 min)
2. AGENTS.md (10 min)
→ Ready to prompt AI (with decisions already made)

### For Decision-Makers (1.5 hours)
1. START_HERE.md (5 min)
2. CRITICAL_DECISIONS.md (review options, 30 min)
3. Team discussion & decision (65 min)
4. Update specs with decisions (30 min)

### For Backend Developers (30 min)
1. specs/02_ARCHITECTURE.md (10 min)
2. specs/03_BACKEND_SPEC.md (15 min)
3. specs/05_API_CONTRACTS.md + specs/06_SECURITY_SPEC.md (5 min skimming)

### For Full System Understanding (2 hours)
1. START_HERE.md (5 min)
2. FINAL_DELIVERY_SUMMARY.md (5 min)
3. SPEC_UPDATES_SUMMARY.md (10 min)
4. CRITICAL_DECISIONS.md (review, 10 min)
5. specs/02_ARCHITECTURE.md (15 min)
6. specs/03_BACKEND_SPEC.md (20 min)
7. specs/05_API_CONTRACTS.md (15 min)
8. specs/06_SECURITY_SPEC.md (15 min)
9. AGENTS.md (10 min)

### For Code Review Checklist (5 min reference)
- API changes? → Check specs/05_API_CONTRACTS.md
- Security? → Check specs/06_SECURITY_SPEC.md
- Database? → Check specs/02_ARCHITECTURE.md
- Async tasks? → Check specs/07_BACKGROUND_JOBS.md
- Rules? → Check specs/00_AI_RULES.md

---

## 🔑 Key Decisions (TBD)

| # | Decision | Options | Impact | Decision File |
|---|----------|---------|--------|--------------|
| 1 | Mobile Framework | Flutter / React Native / Native | Entire mobile codebase | CRITICAL_DECISIONS.md § 1 |
| 2 | Background Jobs | Hangfire / SQS / Hosted Service / RabbitMQ | Async task implementation | CRITICAL_DECISIONS.md § 2 |
| 3 | Refresh Tokens | Implement / JWT-only | Auth flow, token storage | CRITICAL_DECISIONS.md § 3 |
| 4 | Rate Limiting | Enable now / Phase 2 | API security | CRITICAL_DECISIONS.md § 4 |
| 5 | Encryption at Rest | Encrypt / Skip MVP | Data security | CRITICAL_DECISIONS.md § 5 |

---

## 💡 Quick Navigation

**Need to...**
- Understand the system → START_HERE.md
- Make architectural decisions → CRITICAL_DECISIONS.md
- Prompt the AI → AGENTS.md
- Review before code generation → PRE_GENERATION_CHECKLIST.md
- Check backend patterns → specs/03_BACKEND_SPEC.md
- Design API endpoint → specs/05_API_CONTRACTS.md
- Implement security feature → specs/06_SECURITY_SPEC.md
- Understand architecture → specs/02_ARCHITECTURE.md
- Enforce AI rules → specs/00_AI_RULES.md
- See what changed → SPEC_UPDATES_SUMMARY.md

---

## ✅ Status

- [x] All specifications updated & expanded
- [x] All documentation in English
- [x] Zero critical assumptions (except framework choice)
- [x] Decision templates created
- [x] AI coding guide prepared
- [ ] 5 decisions made (NEXT STEP)
- [ ] Code generation started (AFTER DECISIONS)

---

**Next action:** Open CRITICAL_DECISIONS.md and start with Section 1

