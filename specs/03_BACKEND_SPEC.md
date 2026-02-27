# BACKEND SPECIFICATION

## Technology Stack

-   .NET (latest LTS)
-   REST API
-   Relational Database
-   Dependency Injection

## Project Structure

-   Controllers
-   Application Services
-   Domain Models
-   Infrastructure
-   Tests

## Rules

-   No business logic in controllers.
-   Use DTOs for request/response.
-   Validate all inputs.
-   Return standardized error responses.
-   Follow API_CONTRACTS strictly.

## Logging

-   Structured logging only.
-   No sensitive data in logs.
