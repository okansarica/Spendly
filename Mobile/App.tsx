// CHANGED_BY_AI: 2026-03-05 - Handle subscription payment push results in app root
import React, {useEffect, useRef} from 'react';
import {Provider} from 'react-redux';
import {NavigationContainer} from '@react-navigation/native';
import {SafeAreaProvider} from 'react-native-safe-area-context';
import Toast from 'react-native-toast-message';
import {AppState} from 'react-native';
import {ThemeProvider} from './src/theme/ThemeContext';
import RootNavigator from './src/navigation/RootNavigator';
import {store} from './src/store';
import {subscriptionService, SubscriptionPaymentResultStatus} from './src/services/subscriptionService';
import {firebaseService} from './src/services/firebaseService';
import {userService} from './src/services/userService';
import InAppBrowser from 'react-native-inappbrowser-reborn';

export default function App() {
  const appState = useRef(AppState.currentState);

  useEffect(() => {
    let unsubscribe: (() => void) | undefined;

    const initializeApp = async () => {
      try {
        const isAuthenticated = store.getState().auth.isAuthenticated;
        if (isAuthenticated) {
          await subscriptionService.fetchSubscriptionEndDate();
        }

        const handleSubscriptionPaymentResult = async (status: SubscriptionPaymentResultStatus) => {
          if (await InAppBrowser.isAvailable()) {
            InAppBrowser.close();
          }

          if (status === 'success') {
            await subscriptionService.fetchSubscriptionEndDate();
            Toast.show({
              type: 'success',
              text1: 'Payment completed',
            });
            return;
          }

          Toast.show({
            type: 'error',
            text1: 'Payment failed',
          });
        };

        await firebaseService.handleInitialNotification(handleSubscriptionPaymentResult);

        unsubscribe = firebaseService.setupNotificationListeners(
          async message => {
            Toast.show({
              type: 'info',
              text1: message.notification?.title || 'Notification',
              text2: message.notification?.body || '',
            });
          },
          handleSubscriptionPaymentResult,
        );

        const token = await firebaseService.getToken();
        if (token && isAuthenticated) {
          await userService.sendFirebaseToken(token);
        }
      } catch (error) {
        console.error('App initialization error:', error);
      }
    };

    setTimeout(() => {
      initializeApp();
    }, 100);

    const subscription = AppState.addEventListener('change', nextAppState => {
      if (appState.current.match(/inactive|background/) && nextAppState === 'active') {
        if (store.getState().auth.isAuthenticated) {
          subscriptionService.fetchSubscriptionEndDate();
        }
      }
      appState.current = nextAppState;
    });

    return () => {
      subscription.remove();
      if (unsubscribe) {
        unsubscribe();
      }
    };
  }, []);

  return (
    <Provider store={store}>
      <SafeAreaProvider>
        <ThemeProvider>
          <NavigationContainer>
            <RootNavigator />
          </NavigationContainer>
          <Toast />
        </ThemeProvider>
      </SafeAreaProvider>
    </Provider>
  );
}
