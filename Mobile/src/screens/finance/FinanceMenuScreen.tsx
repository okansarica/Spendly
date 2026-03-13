// CHANGED_BY_AI: 2026-03-02 - Align finance menu styling and navigation typing
// CHANGED_BY_AI: 2026-03-13 - Redesign with modern card layout matching Reports screen
// CHANGED_BY_AI: 2026-03-13 - Refactor to use MenuCard component
import React from 'react';
import {View, StyleSheet, ScrollView} from 'react-native';
import {useTheme} from '../../theme/ThemeContext';
import {translate} from '../../utils/translations';
import {useNavigation} from '@react-navigation/native';
import {useBottomTabBarHeight} from '@react-navigation/bottom-tabs';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {FinanceStackParamList} from '../../navigation/FinanceNavigator';
import Header from '../../components/Header';
import MenuCard from '../../components/MenuCard';

type FinanceNavProp = NativeStackNavigationProp<FinanceStackParamList, 'FinanceMenu'>;

type FinanceItem = {
  id: 'banksAccounts' | 'categories' | 'merchants';
  titleKey: string;
  descriptionKey: string;
  icon: string;
  route: 'BanksAccountsList' | 'CategoriesList' | 'MerchantsList';
};

export default function FinanceMenuScreen() {
  const {colors, spacing} = useTheme();
  const navigation = useNavigation<FinanceNavProp>();
  const tabBarHeight = useBottomTabBarHeight();

  const items: FinanceItem[] = [
    {
      id: 'banksAccounts',
      titleKey: 'BanksAccountsTitle',
      descriptionKey: 'BanksAccountsDescription',
      icon: 'account-balance',
      route: 'BanksAccountsList',
    },
    {
      id: 'categories',
      titleKey: 'CategoriesTitle',
      descriptionKey: 'CategoriesDescription',
      icon: 'donut-large',
      route: 'CategoriesList',
    },
    {
      id: 'merchants',
      titleKey: 'MerchantsTitle',
      descriptionKey: 'MerchantsDescription',
      icon: 'store',
      route: 'MerchantsList',
    },
  ];

  const s = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.backgroundSecondary,
    },
    scrollContent: {
      padding: spacing.lg,
      paddingBottom: tabBarHeight + spacing.xl,
    },
  });

  return (
    <View style={s.container}>
      <Header title={translate('FinanceTitle')} showBack={false} />
      <ScrollView
        style={{flex: 1}}
        contentContainerStyle={s.scrollContent}
        showsVerticalScrollIndicator={false}>
        {items.map(item => (
          <MenuCard
            key={item.id}
            title={translate(item.titleKey)}
            description={translate(item.descriptionKey)}
            icon={item.icon}
            onPress={() => navigation.navigate(item.route)}
          />
        ))}
      </ScrollView>
    </View>
  );
}
