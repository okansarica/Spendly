// CHANGED_BY_AI: 2026-03-05 - Register remote messages before token fetch
import messaging from '@react-native-firebase/messaging';
import AsyncStorage from '@react-native-async-storage/async-storage';
import {SubscriptionPaymentResultStatus} from './subscriptionService';

const FIREBASE_TOKEN_KEY = 'firebase_token';

type NotificationMessage = {
  data?: {
    type?: string;
    status?: string;
  };
};

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
    onSubscriptionPaymentResult: (status: SubscriptionPaymentResultStatus) => Promise<void>,
  ) {
    const handleMessage = async (message: NotificationMessage) => {
      if (message.data?.type === 'subscription_payment_result') {
        const status = message.data?.status;
        if (status === 'success' || status === 'fail') {
          await onSubscriptionPaymentResult(status);
          return;
        }
      }
      await onNotification(message);
    };

    const unsubscribe = messaging().onMessage(async message => {
      await handleMessage(message);
    });

    const unsubscribeOpened = messaging().onNotificationOpenedApp(async message => {
      await handleMessage(message);
    });

    const unsubscribeTokenRefresh = messaging().onTokenRefresh(async token => {
      await AsyncStorage.setItem(FIREBASE_TOKEN_KEY, token);
    });

    return () => {
      unsubscribe();
      unsubscribeOpened();
      unsubscribeTokenRefresh();
    };
  },

  async handleInitialNotification(
    onSubscriptionPaymentResult: (status: SubscriptionPaymentResultStatus) => Promise<void>,
  ): Promise<void> {
    const message = await messaging().getInitialNotification();
    if (message?.data?.type !== 'subscription_payment_result') {
      return;
    }

    const status = message.data.status;
    if (status === 'success' || status === 'fail') {
      await onSubscriptionPaymentResult(status);
    }
  },
};
