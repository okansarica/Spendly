# React Native Upgrade: 0.73.6 → 0.76.7

## Changes Applied

### Package Updates
- React: 18.2.0 → 18.3.1
- React Native: 0.73.6 → 0.76.7
- All @react-native/* packages updated to 0.76.7
- TypeScript: 4.8.4 → 5.3.0
- Babel packages: 7.20.x → 7.25.x
- Node requirement: 18+ → 20.18.0+

### Configuration Changes

#### metro.config.js
- Enabled `unstable_enablePackageExports: true` for better module resolution

#### babel.config.js
- Added `unstable_transformProfile: 'hermes-stable'` for optimized Hermes bytecode

#### Android
- Gradle wrapper: 9.0.0 → 8.11.1
- Kotlin: 2.1.20 → 2.1.0
- Android Gradle Plugin compatible with 8.7.3

#### iOS
- Using `min_ios_version_supported` from RN (13.4+)
- CocoaPods clean install required

## Next Steps

### 1. Install iOS Pods
```bash
cd ios
rm -rf Pods Podfile.lock build
pod install
cd ..
```

### 2. Clean Android Build
```bash
cd android
./gradlew clean
cd ..
```

### 3. Test Builds
```bash
# iOS
npm run ios

# Android
npm run android
```

### 4. Test Critical Features
- Google Sign-In (@react-native-google-signin/google-signin)
- Facebook SDK (react-native-fbsdk-next)
- Keychain (react-native-keychain)
- Navigation (@react-navigation)
- Safe Area Context
- Screens navigation

### 5. Production Build Tests
```bash
# iOS Release
cd ios
xcodebuild -workspace Spendly.xcworkspace -scheme Spendly -configuration Release clean build

# Android Release Bundle
cd android
./gradlew bundleRelease
```

## Rollback Plan

### If Issues Occur
```bash
git checkout main
rm -rf node_modules ios/Pods android/.gradle
npm install
cd ios && pod install && cd ..
```

### Branch Info
- Current branch: `upgrade/rn-0.76`
- Original version preserved on: `main`

## Known Issues & Warnings

1. `metro-react-native-babel-preset` is deprecated - using @react-native/babel-preset instead
2. 6 low severity npm vulnerabilities detected (run `npm audit` for details)
3. Some Babel plugins deprecated but still functional

## Future Upgrades

To reach RN 0.83.0 + React 19:
1. Test 0.76.7 stability for 1-2 weeks
2. Upgrade to 0.80.x (intermediate step)
3. Upgrade to 0.83.0 with React 19
4. Update third-party libs (google-signin v17, fbsdk v14, keychain v11)
5. Test New Architecture compatibility (optional)

