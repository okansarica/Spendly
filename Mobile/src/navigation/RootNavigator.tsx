// CHANGED_BY_AI: 2026-03-03 - Re-render root on language changes
import React from 'react';
import {createNativeStackNavigator} from '@react-navigation/native-stack';
import {useAppSelector} from '../store/hooks';
import SplashScreen from '../screens/splash/SplashScreen';
import AuthNavigator from './AuthNavigator';
import MainNavigator from './MainNavigator';

export type RootStackParamList = {
  Splash: undefined;
  Auth: undefined;
  Main: undefined;
};

const Stack = createNativeStackNavigator();

export default function RootNavigator() {
  const isAuthenticated = useAppSelector(s => s.auth.isAuthenticated);
  const isInitializing = useAppSelector(s => s.auth.isInitializing);
  useAppSelector(s => s.user.languageCode);

  return (
    <Stack.Navigator screenOptions={{headerShown: false, animation: 'fade'}}>
      {isInitializing ? (
        <Stack.Screen name="Splash" component={SplashScreen} />
      ) : isAuthenticated ? (
        <Stack.Screen name="Main" component={MainNavigator} />
      ) : (
        <Stack.Screen name="Auth" component={AuthNavigator} />
      )}
    </Stack.Navigator>
  );
}
