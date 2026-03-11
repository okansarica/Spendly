import { AppRegistry, Platform } from 'react-native';
import App from './App';
import { name as appName } from './app.json';

async function setupFirebaseBackgroundHandler() {
  try {
    const messaging = (await import('@react-native-firebase/messaging')).default;

    if (Platform.OS === 'ios' || Platform.OS === 'android') {
      messaging().setBackgroundMessageHandler(async remoteMessage => {
        const isSilent = remoteMessage.data?.silent === 'true';
        if (isSilent) {
          return;
        }
      });
    }
  } catch (e) {
  }
}

setupFirebaseBackgroundHandler();

AppRegistry.registerComponent(appName, () => App);
