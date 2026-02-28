# MOBILE SPECIFICATION

## Technology Stack (PENDING APPROVAL)

**Framework:** TBD - Choose one:
- Option 1: Flutter (Dart) - recommended for iOS/Android from single codebase
- Option 2: React Native (JavaScript/TypeScript) - if web also needed
- Option 3: Native (Swift + Kotlin) - if iOS/Android separate teams

**UI Framework:** (depends on framework choice above) - TBD

**HTTP Client:** TBD (Dio for Flutter, Fetch/Axios for React Native, URLSession/OkHttp for native)

**State Management:** TBD (BLoC for Flutter, Redux/Zustand for React Native, etc.)

**Local Storage:** TBD (must be platform-secure keychain/keystore)

**Navigation:** TBD (depends on framework)

## Architecture

**Pattern:** Clean Architecture with strict layer separation

```
Presentation Layer (UI)
    ↓
Domain Layer (entities, use cases)
    ↓
Data Layer (repositories, API client)
    ↓
API (Spendly Backend)
```

**Layer Responsibilities:**

**Presentation Layer:**
- UI screens and widgets
- State management (BLoC, Redux, ViewModel, etc.)
- User interactions
- Navigation
- NO direct API calls
- NO business logic

**Domain Layer:**
- Core entities (User, Expense, Category, etc.)
- Use cases / business logic
- Repository interfaces (no implementation)
- Platform-agnostic

**Data Layer:**
- Concrete repository implementations
- API client (centralized HTTP wrapper)
- Local database (if offline support needed)
- Token/credential storage
- Network error handling and retries

## API Client (Centralized)

**Rules:**
- Single HTTP client instance (singleton)
- All API calls go through this wrapper
- Handles:
  - Base URL configuration
  - Authentication headers (Bearer token)
  - Request/response serialization
  - Error mapping
  - Retry logic with exponential backoff
  - Request timeout

**Example structure:**
```
services/
├── api_client.dart          # (or .ts, .swift, .kt)
├── repositories/
│   ├── user_repository.dart
│   ├── expense_repository.dart
│   └── category_repository.dart
└── models/
    ├── user.dart
    ├── expense.dart
    └── api_response.dart
```

## Authentication & Token Management

**Flow:**
1. User logs in with email/password or OAuth (Google/Facebook)
2. Backend returns JWT token
3. Token stored securely in platform keychain/keystore (NOT SharedPreferences/UserDefaults)
4. Token attached to all subsequent API requests as `Authorization: Bearer <token>`
5. On 401 response: attempt refresh (if refresh token available) or redirect to login
6. On logout: clear token from secure storage and all app state

**Token Storage:**
- iOS: Keychain
- Android: KeyStore
- Flutter: flutter_secure_storage
- React Native: react-native-keychain or similar
- **Never store tokens in SharedPreferences, UserDefaults, or AsyncStorage**

## Offline Support (TBD)

**If required:**
- Local SQLite database (or platform equivalent)
- Queue pending changes (create/update/delete operations)
- Sync when connection restored
- Optimistic UI updates with rollback on failure
- Service workers for caching (if web app)

**Currently:** TBD if offline mode is required. If yes, add to spec.

## Error Handling

**API Error Response Format** (from 05_API_CONTRACTS.md):
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Email is required"
  }
}
```

**Mobile Handling:**
- Map API error codes to user-friendly messages
- Show toast notifications or error modals
- Log errors for analytics
- Retry transient failures (5xx, network timeouts)
- Don't retry on 400, 401, 403, 404
- Timeout: 30 seconds (configurable)

## Offline-First Features (If Offline Support Enabled)

- **Pending Changes Queue:** SQLite table for pending operations
- **Background Sync:** Periodic attempt to sync when connection available
- **Optimistic Updates:** Update UI immediately, sync in background
- **Conflict Resolution:** Server wins (simpler), or merge logic (complex)

## Project Structure (Framework-Agnostic Template)

```
mobile/
├── lib/ (or src/)
│   ├── main.dart                      # Entry point
│   ├── config/
│   │   ├── routes.dart                # Navigation routes
│   │   ├── theme.dart                 # UI theme
│   │   └── constants.dart             # App constants
│   │
│   ├── domain/
│   │   ├── entities/
│   │   │   ├── user.dart
│   │   │   ├── expense.dart
│   │   │   └── category.dart
│   │   └── repositories/
│   │       ├── user_repository.dart
│   │       ├── expense_repository.dart
│   │       └── category_repository.dart
│   │
│   ├── data/
│   │   ├── datasources/
│   │   │   ├── remote/
│   │   │   │   └── api_client.dart
│   │   │   └── local/
│   │   │       └── secure_storage.dart (tokens, credentials)
│   │   └── repositories/
│   │       ├── user_repository_impl.dart
│   │       ├── expense_repository_impl.dart
│   │       └── category_repository_impl.dart
│   │
│   ├── presentation/
│   │   ├── screens/
│   │   │   ├── login_screen.dart
│   │   │   ├── home_screen.dart
│   │   │   ├── expense_list_screen.dart
│   │   │   └── add_expense_screen.dart
│   │   ├── widgets/
│   │   │   ├── expense_card.dart
│   │   │   └── category_selector.dart
│   │   └── state_management/
│   │       ├── bloc/ (or redux/, riverpod/)
│   │       │   ├── user_bloc.dart
│   │       │   ├── expense_bloc.dart
│   │       │   └── auth_bloc.dart
│   │       └── events/
│   │           ├── user_event.dart
│   │           └── expense_event.dart
│   │
│   └── utils/
│       ├── extensions.dart
│       ├── validators.dart
│       └── formatters.dart
│
└── test/
    ├── unit/
    ├── widget/
    └── integration/
```

## State Management

**Pattern:** (TBD) One of:
- BLoC (Business Logic Component) - Flutter
- Redux - any framework
- Riverpod - Flutter
- Zustand - React Native
- Vuex - if web

**Rules:**
- Business logic in state management, not UI
- UI listens to state changes and rebuilds
- State changes via events/actions only
- Immutable state objects
- No side effects in state reducers

## Error Handling Strategy

```
API Error Response
    ↓
Map to Domain Error
    ↓
Update State (show error)
    ↓
UI displays error to user
    ↓
User can retry
```

## Testing

**Required:**
- Unit tests for repositories and domain logic
- Widget/integration tests for UI (smoke tests)
- Mock API client for unit tests
- Mock local storage for tests

**Framework:** (TBD - depends on framework choice)
- Flutter: flutter_test + mockito
- React Native: Jest + react-native-testing-library
- Native: XCTest (iOS) + JUnit (Android)

## Build & Run

**Development:**
```bash
flutter run                # Flutter
npm run android            # React Native
xcodebuild -scheme App     # iOS
```

**Release:**
- iOS: TestFlight → App Store
- Android: Internal Testing → Google Play
- Config management: Build variants/flavors for dev/staging/prod

## Configuration

**Environment-specific settings:**
```
dev:
  api_base_url: https://api-dev.example.com
  timeout: 30s

staging:
  api_base_url: https://api-staging.example.com
  timeout: 30s

production:
  api_base_url: https://api.example.com
  timeout: 30s
```

## Security

- SSL pinning (optional but recommended)
- Token refresh mechanism
- Secure token storage (Keychain/KeyStore only)
- Input validation (email format, password strength)
- No sensitive data in logs
- No hardcoded credentials or API keys
