// index.js veya index.ts
import { AppRegistry, Platform } from 'react-native';
import App from './App';
import { name as appName } from './app.json';

async function setupFirebaseBackgroundHandler() {
  try {
    // Native modül hazır olana kadar bekle (dinamik import)
    const messaging = (await import('@react-native-firebase/messaging')).default;

    // Sadece iOS ve Android için background handler
    if (Platform.OS === 'ios' || Platform.OS === 'android') {
      messaging().setBackgroundMessageHandler(async remoteMessage => {
        console.log('📩 Background message received:', remoteMessage);
      });
    }
  } catch (e) {
    console.warn('⚠️ Firebase Messaging background handler kurulamadı:', e);
  }
}

// Hemen çağır
setupFirebaseBackgroundHandler();

// Uygulamayı kaydet
AppRegistry.registerComponent(appName, () => App);
