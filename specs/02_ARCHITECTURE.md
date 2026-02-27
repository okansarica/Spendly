# SYSTEM ARCHITECTURE

## High-Level Architecture

Mobile App → API Gateway → Backend Services → Database

## Backend

-   RESTful API
-   Stateless services
-   Environment-based configuration
-   Structured logging

## Mobile

-   Clean architecture
-   Service layer abstraction
-   Secure token storage

## Data Layer

-   Relational database
-   Migrations required for schema updates
-   No direct DB access from mobile

## Architecture Rules

-   All architectural changes require ADR.
-   No direct coupling between mobile and database.
