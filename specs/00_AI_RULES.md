# AI IMPLEMENTATION RULES

**Purpose:**\
This document defines mandatory rules for any AI system (Copilot,
ChatGPT, etc.) generating or modifying code in this repository.

**Usage:**\
Every AI prompt MUST begin with:

> Follow /spec/00_AI_RULES.md strictly.

If a rule conflicts with a requested task, AI must STOP and ask for
clarification.

------------------------------------------------------------------------

## 1. General Principles

1.  Do NOT make assumptions.
2.  If any requirement is unclear, STOP and ask before implementing.
3.  Do NOT change architecture defined in `/spec`.
4.  Implement ONLY the explicitly requested module.
5.  Do NOT introduce new technologies without approval.
6.  If confidence is below 90%, ask clarification questions.

------------------------------------------------------------------------

## 2. Code Preservation Rules

1.  NEVER remove existing comments except AI generated header comment
2.  NEVER remove `// TODO:` comments.
3.  NEVER remove `// FIXME:` comments.
4.  Do NOT modify commented-out code.
5.  Do NOT remove logging statements.
6.  Do NOT delete file headers (license, attribution, etc.).

If a TODO must be updated:

// TODO-UPDATED: 2026-02-22 Reason for update

------------------------------------------------------------------------

## 3. Refactoring & Modification Rules

1.  Do NOT refactor working code unless explicitly requested.
2.  Do NOT rename public methods, API endpoints, or database fields.
3.  Apply minimal necessary changes only.
4.  Do NOT modify unrelated files.
5.  If AI modifies a file, it MUST add at the top:

// CHANGED_BY_AI: `<date>`{=html} - `<short reason>`{=html}

------------------------------------------------------------------------

## 4. Dependency & Library Policy

1.  Do NOT introduce new libraries without explicit approval.
2.  Only use technologies defined in:
    -   `/spec/03_BACKEND_SPEC.md`
    -   `/spec/04_MOBILE_SPEC.md`
3.  No CDN or remote script usage unless approved via ADR.

------------------------------------------------------------------------

## 5. Security Rules

1.  NEVER hardcode secrets (API keys, tokens, passwords).
2.  Use environment variables for secrets.
3.  All inputs MUST be validated server-side.
4.  Do NOT log sensitive user data (PII).
5.  Follow `/spec/06_SECURITY_SPEC.md`.

------------------------------------------------------------------------

## 6. API Contract Protection

1.  API must strictly follow `/spec/05_API_CONTRACTS.md`.
2.  Do NOT change request/response schemas.
3.  Do NOT introduce new endpoints without updating API_CONTRACTS and
    getting approval.
4.  Status codes must match specification.

If conflict occurs, AI must respond:

Conflict with /spec/05_API_CONTRACTS.md: `<short explanation>`{=html}.
Proceed? (yes/no)

------------------------------------------------------------------------

## 7. Background Jobs & Async Processing

1.  API must NOT block waiting for background jobs.
2.  Asynchronous tasks must follow `/spec/07_BACKGROUND_JOBS.md`.
3.  Do NOT introduce external queue systems unless specified in ADR.

------------------------------------------------------------------------

## 8. Logging & Observability

1.  Do NOT remove existing log statements.
2.  Do NOT downgrade log levels.
3.  Avoid logging sensitive data.
4.  Any new logs must follow existing logging conventions.

------------------------------------------------------------------------

## 9. Testing Rules

1.  No test required

------------------------------------------------------------------------

## 10. Formatting & Code Style

1.  Follow existing formatting rules (.editorconfig, eslint, dotnet
    format).
2.  Do NOT reformat entire files.
3.  Only format modified sections.
4.  Preserve existing indentation and structure.

------------------------------------------------------------------------

## 11. Scope Control Rules

1.  Do NOT implement future features.
2.  Do NOT add improvements not requested.
3.  If a feature is not defined in API_CONTRACTS, do NOT create it.
4.  Stay within the exact scope of the prompt.

------------------------------------------------------------------------

## 12. Conflict & Exception Protocol

If any rule prevents implementation:

AI must STOP and ask:

Conflict with /spec/`<file>`{=html}.md: `<short explanation>`{=html}.
Options: 1. Follow spec strictly 2. Modify spec (requires approval)
Which option?

If an exception is granted, mark it in code:

/\* AI-ALLOW: `<rule-id>`{=html} - `<reason>`{=html} \*/

------------------------------------------------------------------------

## 13. Protected File Header Template

Critical files may contain:

/* DO NOT MODIFY: Protected by /spec/00_AI_RULES.md Any modification
requires CHANGED_BY_AI header and PR documentation. */

AI must not remove this block.

------------------------------------------------------------------------

## 14. AI Audit Checklist

Before completing any task, AI must verify:

-   Followed 00_AI_RULES.md
-   No comments removed
-   No TODO/FIXME removed
-   No new dependencies added
-   API contracts respected
-   No Unit tests added
-   CHANGED_BY_AI header added (if modified)

------------------------------------------------------------------------

## 15. Living Document Policy

This file is a living document.

-   New AI issues → new rule.
-   Any architectural decision must be recorded in `/spec/ADR`.
-   Spec changes require approval before implementation.
