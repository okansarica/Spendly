import React from 'react';
import {Text} from 'react-native';
import {createBottomTabNavigator} from '@react-navigation/bottom-tabs';
import DashboardScreen from '../screens/main/DashboardScreen';
import FinanceScreen from '../screens/main/FinanceScreen';
import UserScreen from '../screens/main/UserScreen';
import {useTheme} from '../theme/ThemeContext';

export type MainTabParamList = {
  Dashboard: undefined;
  Finance: undefined;
  User: undefined;
};

const Tab = createBottomTabNavigator();

function TabIcon({label, focused, color}: {label: string; focused: boolean; color: string}) {
  const icons: Record<string, string> = {
    Dashboard: '⊞',
    Finance: '₤',
    User: '◉',
  };
  return <Text style={{fontSize: focused ? 22 : 20, color}}>{icons[label]}</Text>;
}

export default function MainNavigator() {
  const {colors, fontSizes, fontWeights} = useTheme();

  return (
    <Tab.Navigator
      screenOptions={({route}: {route: {name: string}}) => ({
        headerShown: true,
        tabBarActiveTintColor: colors.tabBarActive,
        tabBarInactiveTintColor: colors.tabBarInactive,
        tabBarStyle: {
          backgroundColor: colors.tabBarBackground,
          borderTopColor: colors.tabBarBorder,
          borderTopWidth: 1,
        },
        tabBarLabelStyle: {
          fontSize: fontSizes.xs,
          fontWeight: fontWeights.medium,
          marginBottom: 2,
        },
        headerStyle: {backgroundColor: colors.backgroundPrimary},
        headerTitleStyle: {color: colors.textPrimary, fontWeight: fontWeights.semiBold},
        tabBarIcon: ({focused, color}: {focused: boolean; color: string}) => (
          <TabIcon label={route.name} focused={focused} color={color} />
        ),
      })}>
      <Tab.Screen name="Dashboard" component={DashboardScreen} />
      <Tab.Screen name="Finance" component={FinanceScreen} />
      <Tab.Screen name="User" component={UserScreen} />
    </Tab.Navigator>
  );
}
