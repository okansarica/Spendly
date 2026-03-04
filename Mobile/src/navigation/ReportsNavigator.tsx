// CHANGED_BY_AI: 2026-03-02 - Use shared header component
import React from 'react';
import {createNativeStackNavigator} from '@react-navigation/native-stack';

import CategoryDetailScreen from '../screens/reports/CategoryDetailScreen';
import AccountDetailScreen from '../screens/reports/AccountDetailScreen';
import ReportsMenuScreen from '../screens/reports/ReportsMenuScreen';
import {useTheme} from '../theme/ThemeContext';
import {translate} from '../utils/translations';
import CategoryReportScreen from "../screens/reports/CategoryReportScreen.tsx";
import AccountReportScreen from "../screens/reports/AccountReportScreen.tsx";

export type ReportsStackParamList = {
  ReportsMenu: undefined;
  ReportsOverview: undefined;
  CategoryDetail: {categoryId: string; categoryName: string; startDate?: string; endDate?: string; accountId?: string};
  AccountsOverview: undefined;
  AccountDetail: {accountId: string; accountName: string; startDate?: string; endDate?: string};
};

const Stack = createNativeStackNavigator<ReportsStackParamList>();

export default function ReportsNavigator() {
  const {colors, fontWeights} = useTheme();

  return (
    <Stack.Navigator
      initialRouteName="ReportsMenu"
      screenOptions={{
        headerShown: false,
        headerStyle: {backgroundColor: colors.backgroundPrimary},
        headerTitleStyle: {color: colors.textPrimary, fontWeight: fontWeights.semiBold},
      }}>
      <Stack.Screen name="ReportsMenu" component={ReportsMenuScreen} options={{title: translate('ReportsTitle')}} />
      <Stack.Screen name="ReportsOverview" component={CategoryReportScreen} options={{title: translate('ReportsTitle')}} />
      <Stack.Screen name="CategoryDetail" component={CategoryDetailScreen} options={{title: translate('CategoryTitle')}} />
      <Stack.Screen name="AccountsOverview" component={AccountReportScreen} options={{title: translate('AccountsTitle')}} />
      <Stack.Screen name="AccountDetail" component={AccountDetailScreen} options={{title: translate('AccountTitle')}} />
    </Stack.Navigator>
  );
}
