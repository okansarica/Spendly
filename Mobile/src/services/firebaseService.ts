// CHANGED_BY_AI: 2026-03-05 - Register remote messages before token fetch
import messaging from '@react-native-firebase/messaging';
import AsyncStorage from '@react-native-async-storage/async-storage';

const FIREBASE_TOKEN_KEY = 'firebase_token';

export const firebaseService = {
  async requestPermission(): Promise<boolean> {
    const authStatus = await messaging().requestPermission();
    return (
      authStatus === messaging.AuthorizationStatus.AUTHORIZED ||
      authStatus === messaging.AuthorizationStatus.PROVISIONAL
    );
  },

  async getToken(): Promise<string | null> {
      const hasPermission = await this.requestPermission();
      if (!hasPermission) {
        return null;
      }

      if (!messaging().isDeviceRegisteredForRemoteMessages) {
        await messaging().registerDeviceForRemoteMessages();
      }

      const token = await messaging().getToken();
      await AsyncStorage.setItem(FIREBASE_TOKEN_KEY, token);
      return token;
    
  },

  async getCachedToken(): Promise<string | null> {
    try {
      return await AsyncStorage.getItem(FIREBASE_TOKEN_KEY);
    } catch (error) {
      return null;
    }
  },

  setupNotificationListeners(
    onNotification: (message: any) => Promise<void>,
  ) {
    const unsubscribe = messaging().onMessage(async message => {
      await onNotification(message);
    });

    const unsubscribeTokenRefresh = messaging().onTokenRefresh(async token => {
      await AsyncStorage.setItem(FIREBASE_TOKEN_KEY, token);
    });

    return () => {
      unsubscribe();
      unsubscribeTokenRefresh();
    };
  },
};
