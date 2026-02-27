# MOBILE SPECIFICATION

## Architecture

-   Clean architecture
-   Separation of UI, Domain, and Data layers

## Rules

-   No direct API calls from UI.
-   Use service/repository abstraction.
-   Secure token storage required.
-   Handle offline and error states gracefully.

## Networking

-   Follow API_CONTRACTS strictly.
-   Centralized API client.
