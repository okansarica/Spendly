// CHANGED_BY_AI: 2026-03-11 - Extract VersionUpdateScreen to separate component file
// CHANGED_BY_AI: 2026-03-11 - Add startup version check and update gate screen
// CHANGED_BY_AI: 2026-03-05 - Handle subscription payment push results in app root
import React, {useEffect, useMemo, useRef, useState} from 'react';
import {Provider} from 'react-redux';
import {NavigationContainer} from '@react-navigation/native';
import {SafeAreaProvider} from 'react-native-safe-area-context';
import Toast from 'react-native-toast-message';
import {Alert, AppState, Linking, Platform} from 'react-native';
import {ThemeProvider} from './src/theme/ThemeContext';
import RootNavigator from './src/navigation/RootNavigator';
import {store} from './src/store';
import {subscriptionService, SubscriptionPaymentResultStatus} from './src/services/subscriptionService';
import {firebaseService} from './src/services/firebaseService';
import {userService} from './src/services/userService';
import InAppBrowser from 'react-native-inappbrowser-reborn';
import {versionService} from './src/services/versionService';
import APP_CONFIG from './src/config/appConfig';
import {getCurrentLanguage, translate} from './src/utils/translations';
import VersionUpdateScreen from './src/components/VersionUpdateScreen';

type VersionGateState = {
  visible: boolean;
  forceUpdate: boolean;
  storeUrl?: string;
  message?: string;
};

export default function App() {
  const appState = useRef(AppState.currentState);
  const [versionGate, setVersionGate] = useState<VersionGateState>({visible: false, forceUpdate: false});

  const selectedStoreUrl = useMemo(
    () => versionGate.storeUrl,
    [versionGate.storeUrl],
  );

  useEffect(() => {
    let unsubscribe: (() => void) | undefined;

    const initializeApp = async () => {
      try {
        const versionResult = await versionService.checkVersion(APP_CONFIG.version);
        if (versionResult.isSuccess && versionResult.data) {
          const shouldShowUpdate =
            versionResult.data.forceUpdate || versionResult.data.isThereNewVersion;
          const currentLanguage = getCurrentLanguage().toLowerCase();
          const localizedMessage = versionResult.data.localizedMessages?.find(
            item => item.languageCode?.toLowerCase() === currentLanguage,
          )?.message;
          const fallbackMessage = versionResult.data.localizedMessages?.find(
            item => item.languageCode?.toLowerCase() === 'en',
          )?.message;
          const platformUrl =
            Platform.OS === 'ios'
              ? versionResult.data.appleStoreUrl
              : versionResult.data.playStoreUrl;

          setVersionGate({
            visible: shouldShowUpdate,
            forceUpdate: versionResult.data.forceUpdate,
            storeUrl: platformUrl,
            message: localizedMessage?.trim() ? localizedMessage : fallbackMessage,
          });
        }

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

  const handleUpdatePress = async () => {
    if (!selectedStoreUrl) {
      Alert.alert(translate('Error'), translate('StoreLinkNotAvailable'));
      return;
    }

    const canOpen = await Linking.canOpenURL(selectedStoreUrl);
    if (!canOpen) {
      Alert.alert(translate('Error'), translate('StoreLinkNotAvailable'));
      return;
    }

    await Linking.openURL(selectedStoreUrl);
  };

  const handleSkipUpdate = () => {
    setVersionGate(current => ({...current, visible: false}));
  };

  return (
    <Provider store={store}>
      <SafeAreaProvider>
        <ThemeProvider>
          {versionGate.visible ? (
            <VersionUpdateScreen
              forceUpdate={versionGate.forceUpdate}
              message={versionGate.message}
              onUpdate={handleUpdatePress}
              onSkip={handleSkipUpdate}
            />
          ) : (
            <NavigationContainer>
              <RootNavigator />
            </NavigationContainer>
          )}
          <Toast />
        </ThemeProvider>
      </SafeAreaProvider>
    </Provider>
  );
}
