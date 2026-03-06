// CHANGED_BY_AI: 2026-03-02 - Add shared header component rule
# MOBILE SPECIFICATION

## Technology Stack

- **Framework:** React Native (TypeScript)
- **State Management:** Redux (no Saga)
- **HTTP Client:** Axios
- **HTTP Interceptor:** Existing interceptor in `src/services/apiClient.ts` - do not replace
- **Navigation:** React Navigation
- **Local Storage:** react-native-keychain (tokens only)
- **Offline Support:** Not required

## Project Structure (Actual)

```
Mobile/
├── App.tsx                          # Root component
├── index.js                         # Entry point
├── src/
│   ├── navigation/
│   │   ├── RootNavigator.tsx        # Root navigator (auth vs main)
│   │   ├── AuthNavigator.tsx        # Auth flow screens
│   │   └── MainNavigator.tsx        # Main app screens
│   │
│   ├── screens/
│   │   ├── auth/
│   │   │   ├── LoginScreen.tsx
│   │   │   ├── RegisterScreen.tsx
│   │   │   ├── ForgotPasswordScreen.tsx
│   │   │   └── VerificationScreen.tsx
│   │   ├── main/
│   │   │   ├── DashboardScreen.tsx
│   │   │   ├── FinanceScreen.tsx
│   │   │   └── UserScreen.tsx
│   │   └── splash/
│   │       └── SplashScreen.tsx
│   │
│   ├── services/
│   │   ├── apiClient.ts             # Axios instance with interceptors (do not replace)
│   │   └── authService.ts           # Auth API calls
│   │
│   ├── store/
│   │   └── authStore.ts             # Redux auth state
│   │
│   └── theme/
│       ├── ThemeContext.tsx
│       ├── colors.ts
│       └── theme.ts
│
├── android/
└── ios/
```

## Architecture

**Pattern:** Layered with Redux

```
Screens (UI)
    ↓
Redux Store (state)
    ↓
Services (API calls)
    ↓
apiClient.ts (Axios + interceptors)
    ↓
Backend API
```

**Layer Responsibilities:**

**Screens:**
- Render UI only
- Dispatch Redux actions
- Read from Redux state
- No direct API calls
- No business logic

**Redux Store:**
- Holds application state
- No Saga - use Redux Thunk or plain async actions
- Immutable state updates

**Services:**
- All API calls go through service layer
- Use apiClient.ts (existing Axios instance with interceptors)
- Return typed response objects

## Translation
- Every string in the UI must be translatable
- Whole application languge can be translatable

## API Client

- Existing `src/services/apiClient.ts` is the single Axios instance
- Existing HTTP interceptor handles: Bearer token injection, 401 redirect, token refresh
- Do NOT replace or create a new HTTP client
- All service files import and use `apiClient`

## Authentication & Token Management

- JWT stored in react-native-keychain (NOT AsyncStorage)
- Token attached via existing interceptor
- On 401: existing interceptor attempts refresh or redirects to login
- On logout: clear keychain and Redux state

## Error Handling

- API errors (400): show error message from response body
- Server errors (500): show generic error message
- No try/catch for API calls in screens or services - use the base API call function
- See UI_ARCHITECTURE.md for client-side response handling pattern

## State Management

**Redux (no Saga):**
- Business logic in async action creators (thunks)
- UI reads from store selectors
- State changes via dispatched actions only
- Immutable state objects

## Build & Run

```bash
npm run android     # Android
npm run ios         # iOS
```

## Configuration

```
dev:
  api_base_url: https://localhost:5001
  timeout: 30s

production:
  api_base_url: https://api.spendly.io
  timeout: 30s
```

## Security

- Tokens in Keychain/KeyStore only (never AsyncStorage)
- No sensitive data in logs
- No hardcoded credentials or API keys
- All sensitive config in `shared.local.json` equivalent on mobile

## Shared Header Component

- Use `src/components/Header.tsx` on every screen
- Back button shows only when `navigation.canGoBack()` is true or `showBack` is true
- Header background color is the main app color (`colors.buttonPrimary`)
- Navigation headers must be disabled in navigators
