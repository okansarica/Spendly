// CHANGED_BY_AI: 2026-03-03 - Add user stack navigator
import React from 'react';
import {createNativeStackNavigator} from '@react-navigation/native-stack';
import {useTheme} from '../theme/ThemeContext';
import UserScreen from '../screens/user/UserScreen';
import ProfileScreen from '../screens/user/ProfileScreen';
import ChangePasswordScreen from '../screens/user/ChangePasswordScreen';

export type UserStackParamList = {
  UserMenu: undefined;
  Profile: undefined;
  ChangePassword: undefined;
};

const Stack = createNativeStackNavigator<UserStackParamList>();

export default function UserNavigator() {
  const {colors, fontWeights} = useTheme();

  return (
    <Stack.Navigator
      initialRouteName="UserMenu"
      screenOptions={{
        headerShown: false,
        headerStyle: {backgroundColor: colors.backgroundPrimary},
        headerTitleStyle: {color: colors.textPrimary, fontWeight: fontWeights.semiBold},
      }}>
      <Stack.Screen name="UserMenu" component={UserScreen} />
      <Stack.Screen name="Profile" component={ProfileScreen} />
      <Stack.Screen name="ChangePassword" component={ChangePasswordScreen} />
    </Stack.Navigator>
  );
}

