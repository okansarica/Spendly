// CHANGED_BY_AI: 2026-03-02 - Add finance navigator
import React from 'react';
import {createNativeStackNavigator} from '@react-navigation/native-stack';
import {useTheme} from '../theme/ThemeContext';
import FinanceMenuScreen from '../screens/finance/FinanceMenuScreen';
import CategoriesListScreen from '../screens/finance/categories/CategoriesListScreen';
import CategoryEditScreen from '../screens/finance/categories/CategoryEditScreen';
import CategoryMerchantsSelectScreen from '../screens/finance/categories/CategoryMerchantsSelectScreen';
import MerchantsListScreen from '../screens/finance/merchants/MerchantsListScreen';
import {CategoryListItem} from '../services/categoriesService';

export type FinanceStackParamList = {
  FinanceMenu: undefined;
  CategoriesList: undefined;
  CategoryEdit: {mode: 'create' | 'edit'; category?: CategoryListItem; parentCategory?: CategoryListItem};
  CategoryMerchantsSelect: {categoryId?: string; mode: 'create' | 'edit'};
  MerchantsList: undefined;
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
      <Stack.Screen name="CategoriesList" component={CategoriesListScreen} />
      <Stack.Screen name="CategoryEdit" component={CategoryEditScreen} />
      <Stack.Screen name="CategoryMerchantsSelect" component={CategoryMerchantsSelectScreen} />
      <Stack.Screen name="MerchantsList" component={MerchantsListScreen} />
    </Stack.Navigator>
  );
}

