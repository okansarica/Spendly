// CHANGED_BY_AI: 2026-03-02 - Add merchant edit screen route
// CHANGED_BY_AI: 2026-03-02 - Add finance navigator
import React from 'react';
import {createNativeStackNavigator} from '@react-navigation/native-stack';
import {useTheme} from '../theme/ThemeContext';
import FinanceMenuScreen from '../screens/finance/FinanceMenuScreen';
import CategoriesListScreen from '../screens/finance/categories/CategoriesListScreen';
import CategoryEditScreen from '../screens/finance/categories/CategoryEditScreen';
import CategoryMerchantsSelectScreen from '../screens/finance/categories/CategoryMerchantsSelectScreen';
import MerchantsListScreen from '../screens/finance/merchants/MerchantsListScreen';
import MerchantEditScreen from '../screens/finance/merchants/MerchantEditScreen';
import BanksAccountsListScreen from '../screens/finance/banks/BanksAccountsListScreen';
import BankEditScreen from '../screens/finance/banks/BankEditScreen';
import AccountEditScreen from '../screens/finance/banks/AccountEditScreen';
import {CategoryListItem} from '../services/categoriesService';
import {BankAccountItem, BankListItem} from '../services/banksService';

export type FinanceStackParamList = {
  FinanceMenu: undefined;
  BanksAccountsList: undefined;
  CategoriesList: undefined;
  CategoryEdit: {mode: 'create' | 'edit'; category?: CategoryListItem; parentCategory?: CategoryListItem};
  CategoryMerchantsSelect: {categoryId?: string; mode: 'create' | 'edit'};
  MerchantsList: undefined;
  MerchantEdit: {merchantId: string};
  BankEdit: {mode: 'create'} | {mode: 'edit'; bank: BankListItem};
  AccountEdit: {mode: 'create'; bankId: string} | {mode: 'edit'; bankId: string; account: BankAccountItem};
};

const Stack = createNativeStackNavigator<FinanceStackParamList>();

export default function FinanceNavigator() {
  const {colors, fontWeights} = useTheme();

  return (
    <Stack.Navigator
      initialRouteName="FinanceMenu"
      screenOptions={{
        headerShown: false,
        headerStyle: {backgroundColor: colors.backgroundPrimary},
        headerTitleStyle: {color: colors.textPrimary, fontWeight: fontWeights.semiBold},
      }}>
      <Stack.Screen name="FinanceMenu" component={FinanceMenuScreen} />
      <Stack.Screen name="BanksAccountsList" component={BanksAccountsListScreen} />
      <Stack.Screen name="CategoriesList" component={CategoriesListScreen} />
      <Stack.Screen name="CategoryEdit" component={CategoryEditScreen} />
      <Stack.Screen name="CategoryMerchantsSelect" component={CategoryMerchantsSelectScreen} />
      <Stack.Screen name="MerchantsList" component={MerchantsListScreen} />
      <Stack.Screen name="MerchantEdit" component={MerchantEditScreen} />
      <Stack.Screen name="BankEdit" component={BankEditScreen} />
      <Stack.Screen name="AccountEdit" component={AccountEditScreen} />
    </Stack.Navigator>
  );
}
