import React from 'react';
import {Provider} from 'react-redux';
import {NavigationContainer} from '@react-navigation/native';
import {SafeAreaProvider} from 'react-native-safe-area-context';
import Toast from 'react-native-toast-message';
import {ThemeProvider} from './src/theme/ThemeContext';
import RootNavigator from './src/navigation/RootNavigator';
import {store} from './src/store';

export default function App() {
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
