import React from 'react';
import {createNativeStackNavigator} from '@react-navigation/native-stack';
import {useAuthStore} from '../store/authStore';
import SplashScreen from '../screens/splash/SplashScreen';
import AuthNavigator from './AuthNavigator';
import MainNavigator from './MainNavigator';

export type RootStackParamList = {
  Splash: undefined;
  Auth: undefined;
  Main: undefined;
};

const Stack = createNativeStackNavigator<RootStackParamList>();

export default function RootNavigator() {
  const {isAuthenticated, isInitializing} = useAuthStore();

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

