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

export default function App() {
  const appState = useRef(AppState.currentState);

  useEffect(() => {
    subscriptionService.fetchSubscriptionEndDate();

    const subscription = AppState.addEventListener('change', nextAppState => {
      if (appState.current.match(/inactive|background/) && nextAppState === 'active') {
        subscriptionService.fetchSubscriptionEndDate();
      }
      appState.current = nextAppState;
    });

    return () => {
      subscription.remove();
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
