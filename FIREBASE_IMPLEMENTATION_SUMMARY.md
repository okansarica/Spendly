# Firebase Notifications Implementation Summary

## What Was Implemented

### Mobile App (React Native)

1. **Firebase Service** (`src/services/firebaseService.ts`)
   - Request notification permissions
   - Get Firebase token and cache it in AsyncStorage
   - Setup foreground and background notification listeners
   - Handle token refresh

2. **User Service** (`src/services/userService.ts`)
   - Added `sendFirebaseToken` method to send token to backend
   - Endpoint: POST `/api/v1/users/firebase-token`

3. **Auth Service** (`src/services/authService.ts`)
   - Added optional `firebaseToken` field to `LoginRequest`, `RegisterRequest`, and `SocialLoginRequest`

4. **Auth Store** (`src/store/authStore.ts`)
   - Updated login, register, and socialLogin thunks to include cached Firebase token in requests

5. **App Component** (`App.tsx`)
   - Initialize Firebase on app start
   - Request permission and get token
   - Send token to backend
   - Setup notification listeners to show Toast notifications when app is foreground

6. **Background Handler** (`index.js`)
   - Setup background message handler for when app is closed/background

7. **Android Configuration**
   - Added notification permissions to AndroidManifest.xml
   - Added Firebase metadata for notification channel and color
   - Created colors.xml with notification color

8. **Firebase Config** (`firebase.json`)
   - Basic Firebase configuration for React Native

### Backend (C# .NET)

1. **Auth Service** (`Api/Spendly.Mobile.BusinessLayer/Services/Auth/AuthService.cs`)
   - Updated `LoginAsync`, `SocialLoginAsync`, and `RegisterAsync` to save Firebase token
   - Tokens are saved/updated in `FirebaseToken` collection with user association
   - If token exists, it's updated with the new userId
   - Handles empty/null tokens gracefully

2. **View Models**
   - `LoginRequestViewModel`, `SocialLoginRequestViewModel`, `RegisterRequestViewModel` already had `FirebaseToken` field

3. **Subscription Service**
   - `SaveFirebaseToken` method already existed to save tokens manually

## How It Works

1. **App Launch Flow**:
   - App requests notification permissions
   - Gets Firebase token from Firebase SDK
   - Sends token to backend via `/api/v1/users/firebase-token`
   - Token is cached in AsyncStorage

2. **Login/Register Flow**:
   - User logs in or registers
   - Cached Firebase token is included in the request
   - Backend saves token with user association
   - User can now receive push notifications

3. **Notification Display**:
   - **Foreground**: Shows Toast notification at top of screen
   - **Background/Closed**: System notification shows automatically
   - Notification has title and body from server

## Next Steps (Manual)

1. **Firebase Console Setup**:
   - Create Firebase project
   - Add Android app and download `google-services.json`
   - Add iOS app and download `GoogleService-Info.plist`
   - Configure APNs certificates for iOS

2. **Backend Firebase Admin SDK** (if needed):
   - Install Firebase Admin SDK in .NET project
   - Implement notification sending service
   - Use stored Firebase tokens to send targeted notifications

3. **Testing**:
   - Test on physical devices (push notifications don't work on simulators/emulators)
   - Test foreground, background, and closed app scenarios
   - Test token refresh handling

