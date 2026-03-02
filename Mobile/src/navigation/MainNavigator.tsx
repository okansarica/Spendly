// CHANGED_BY_AI: 2026-03-02 - Use finance navigator in main tabs
import React from 'react';
import {createBottomTabNavigator} from '@react-navigation/bottom-tabs';
import Icon from 'react-native-vector-icons/MaterialIcons';
import ReportsNavigator from './ReportsNavigator';
import {useTheme} from '../theme/ThemeContext';
import DashboardScreen from "../screens/dashboard/DashboardScreen.tsx";
import UserScreen from "../screens/user/UserScreen.tsx";
import FinanceNavigator from "./FinanceNavigator";

export type MainTabParamList = {
  Dashboard: undefined;
  Reports: undefined;
  Finance: undefined;
  User: undefined;
};

const Tab = createBottomTabNavigator();

function TabIcon({label, focused, color}: {label: string; focused: boolean; color: string}) {
  const icons: Record<string, string> = {
    Dashboard: 'dashboard',
    Reports: 'bar-chart',
    Finance: 'account-balance-wallet',
    User: 'person',
  };
  const size = focused ? 26 : 24;
  return <Icon name={icons[label]} size={size} color={color} />;
}

export default function MainNavigator() {
  const {colors, fontSizes, fontWeights} = useTheme();

  return (
    <Tab.Navigator
      screenOptions={({route}: {route: {name: string}}) => ({
        headerShown: false,
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
      <Tab.Screen name="Reports" component={ReportsNavigator} />
      <Tab.Screen name="Finance" component={FinanceNavigator} />
      <Tab.Screen name="User" component={UserScreen} />
    </Tab.Navigator>
  );
}
