import React, {useEffect, useRef} from 'react';
import {Provider} from 'react-redux';
import {NavigationContainer} from '@react-navigation/native';
import {SafeAreaProvider} from 'react-native-safe-area-context';
import Toast from 'react-native-toast-message';
import {AppState} from 'react-native';
import {ThemeProvider} from './src/theme/ThemeContext';
import RootNavigator from './src/navigation/RootNavigator';
import {store} from './src/store';
import {subscriptionService} from './src/services/subscriptionService';
import {firebaseService} from './src/services/firebaseService';
import {userService} from './src/services/userService';

export default function App() {
  const appState = useRef(AppState.currentState);

  useEffect(() => {
    let unsubscribe: (() => void) | undefined;

    const initializeApp = async () => {
      try {
        subscriptionService.fetchSubscriptionEndDate();

        const token = await firebaseService.getToken();
        if (token) {
            await userService.sendFirebaseToken(token);
        }

        unsubscribe = firebaseService.setupNotificationListeners(
          async message => {
            Toast.show({
              type: 'info',
              text1: message.notification?.title || 'Notification',
              text2: message.notification?.body || '',
            });
          },
        );
      } catch (error) {
        console.error('App initialization error:', error);
      }
    };

    setTimeout(() => {
      initializeApp();
    }, 100);

    const subscription = AppState.addEventListener('change', nextAppState => {
      if (appState.current.match(/inactive|background/) && nextAppState === 'active') {
        subscriptionService.fetchSubscriptionEndDate();
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
