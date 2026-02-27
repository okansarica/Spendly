# BACKGROUND JOBS

## Principles

-   API must remain non-blocking.
-   Background jobs must be idempotent.
-   Failures must be logged.

## Retry Policy

-   Implement retry with backoff.
-   Avoid infinite retry loops.

## Monitoring

-   Log job start, success, failure.
-   Expose metrics if applicable.
