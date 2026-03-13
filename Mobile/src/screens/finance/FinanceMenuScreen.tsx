// CHANGED_BY_AI: 2026-03-02 - Align finance menu styling and navigation typing
// CHANGED_BY_AI: 2026-03-13 - Redesign with modern card layout matching Reports screen
import React from 'react';
import {View, Text, StyleSheet, TouchableOpacity, ScrollView} from 'react-native';
import {useTheme} from '../../theme/ThemeContext';
import {translate} from '../../utils/translations';
import {useNavigation} from '@react-navigation/native';
import {useBottomTabBarHeight} from '@react-navigation/bottom-tabs';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {FinanceStackParamList} from '../../navigation/FinanceNavigator';
import Icon from 'react-native-vector-icons/MaterialIcons';
import Header from '../../components/Header';

type FinanceNavProp = NativeStackNavigationProp<FinanceStackParamList, 'FinanceMenu'>;

type FinanceItem = {
  id: 'banksAccounts' | 'categories' | 'merchants';
  titleKey: string;
  descriptionKey: string;
  icon: string;
  route: 'BanksAccountsList' | 'CategoriesList' | 'MerchantsList';
};

export default function FinanceMenuScreen() {
  const {colors, spacing, fontSizes, fontWeights, radius} = useTheme();
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
    card: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      marginBottom: spacing.lg,
      overflow: 'hidden',
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      borderLeftWidth: 6,
      borderLeftColor: colors.buttonPrimary,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.08,
      shadowRadius: 8,
      elevation: 3,
    },
    cardContent: {
      padding: spacing.lg,
      minHeight: 140,
    },
    cardHeader: {
      flexDirection: 'row',
      alignItems: 'center',
      marginBottom: spacing.md,
    },
    iconContainer: {
      width: 56,
      height: 56,
      borderRadius: 28,
      backgroundColor: colors.buttonPrimary,
      alignItems: 'center',
      justifyContent: 'center',
      marginRight: spacing.md,
    },
    textContent: {
      flex: 1,
    },
    title: {
      color: colors.textPrimary,
      fontSize: fontSizes.xl,
      fontWeight: fontWeights.bold,
      marginBottom: spacing.xs,
    },
    description: {
      color: colors.textSecondary,
      fontSize: fontSizes.sm,
      lineHeight: 20,
    },
    arrowContainer: {
      width: 32,
      height: 32,
      borderRadius: 16,
      backgroundColor: colors.buttonPrimary + '15',
      alignItems: 'center',
      justifyContent: 'center',
    },
  });

  const renderCard = (item: FinanceItem) => {
    return (
      <TouchableOpacity
        key={item.id}
        style={s.card}
        onPress={() => navigation.navigate(item.route)}
        activeOpacity={0.7}>
        <View style={s.cardContent}>
          <View style={s.cardHeader}>
            <View style={s.iconContainer}>
              <Icon name={item.icon} size={28} color={colors.buttonPrimaryText} />
            </View>
            <View style={s.textContent}>
              <Text style={s.title}>{translate(item.titleKey)}</Text>
            </View>
            <View style={s.arrowContainer}>
              <Icon name="arrow-forward" size={20} color={colors.buttonPrimary} />
            </View>
          </View>
          <Text style={s.description}>{translate(item.descriptionKey)}</Text>
        </View>
      </TouchableOpacity>
    );
  };

  return (
    <View style={s.container}>
      <Header title={translate('FinanceTitle')} showBack={false} />
      <ScrollView
        style={{flex: 1}}
        contentContainerStyle={s.scrollContent}
        showsVerticalScrollIndicator={false}>
        {items.map(item => renderCard(item))}
      </ScrollView>
    </View>
  );
}
