# API CONTRACTS

## General Rules

-   All endpoints must be versioned.
-   Use JSON only.
-   Standard response format:

{ "success": true, "data": {}, "error": null }

## Error Format

{ "success": false, "data": null, "error": { "code": "ERROR_CODE",
"message": "Human readable message" } }

## Status Codes

-   200 OK
-   201 Created
-   400 Bad Request
-   401 Unauthorized
-   403 Forbidden
-   404 Not Found
-   500 Internal Server Error

Any modification requires spec update.
