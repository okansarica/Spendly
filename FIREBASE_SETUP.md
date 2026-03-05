# Firebase Notifications Setup

Firebase Cloud Messaging (FCM) has been integrated into the Spendly mobile app.

## Setup Required

### 1. Firebase Console Setup
- Go to Firebase Console (https://console.firebase.google.com/)
- Create a new project or select existing project
- Add iOS and Android apps to your Firebase project

### 2. Android Configuration
- Download `google-services.json` from Firebase Console
- Place it in: `android/app/google-services.json`
- The project is already configured to use it

### 3. iOS Configuration
- Download `GoogleService-Info.plist` from Firebase Console
- Open Xcode project: `ios/Spendly.xcworkspace`
- Drag `GoogleService-Info.plist` into the project (make sure "Copy items if needed" is checked)
- Enable Push Notifications capability in Xcode
- Upload APNs certificate to Firebase Console

### 4. Testing
Run the app and check logs for Firebase token generation.

## Implementation Details

### Mobile App (React Native)
- Firebase token is generated on app start
- Token is sent to backend via `/api/v1/users/firebase-token` endpoint
- Token is included in login, register, and social login requests
- Notifications are displayed using Toast when app is in foreground
- Background notifications are handled automatically

### Backend (C#)
- `FirebaseToken` field added to `LoginRequestViewModel`, `RegisterRequestViewModel`, and `SocialLoginRequestViewModel`
- Firebase tokens are saved to database with user association
- Backend can send push notifications to users via Firebase Admin SDK (implementation pending)

## Files Modified/Created

### Mobile
- `src/services/firebaseService.ts` - Firebase service
- `src/services/userService.ts` - Added sendFirebaseToken method
- `src/services/authService.ts` - Added firebaseToken to request types
- `src/store/authStore.ts` - Include Firebase token in auth flows
- `src/constants/apiEndpoints.ts` - Added FirebaseToken endpoint
- `App.tsx` - Initialize Firebase on app start
- `index.js` - Background message handler
- `android/app/src/main/AndroidManifest.xml` - Notification permissions
- `android/app/src/main/res/values/colors.xml` - Notification color
- `firebase.json` - Firebase config

### Backend
- `Api/Spendly.Mobile.BusinessLayer/Services/Auth/AuthService.cs` - Save Firebase tokens on auth

