# SECURITY SPECIFICATION

## Secrets

-   Must be stored in environment variables.
-   Never commit secrets.

## Authentication

-   Token-based authentication.
-   Tokens must be validated server-side.

## Input Validation

-   Validate all inputs.
-   Reject malformed requests.

## Data Protection

-   Do not log PII.
-   Encrypt sensitive data at rest if required.

## Security Reviews

-   Major changes require review.
