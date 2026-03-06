// CHANGED_BY_AI: 2026-03-02 - Align finance menu styling and navigation typing
import React from 'react';
import {View, Text, StyleSheet, TouchableOpacity} from 'react-native';
import {useTheme} from '../../theme/ThemeContext';
import {translate} from '../../utils/translations';
import {useNavigation} from '@react-navigation/native';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {FinanceStackParamList} from '../../navigation/FinanceNavigator';
import Icon from 'react-native-vector-icons/MaterialIcons';
import Header from '../../components/Header';

type FinanceNavProp = NativeStackNavigationProp<FinanceStackParamList, 'FinanceMenu'>;

type FinanceItem = {
  id: 'banksAccounts' | 'categories' | 'merchants';
  titleKey: string;
  icon: string;
  route: 'BanksAccountsList' | 'CategoriesList' | 'MerchantsList';
};

export default function FinanceMenuScreen() {
  const {colors, spacing, fontSizes, fontWeights, radius} = useTheme();
  const navigation = useNavigation<FinanceNavProp>();

  const items: FinanceItem[] = [
    {id: 'banksAccounts', titleKey: 'BanksAccountsTitle', icon: 'account-balance', route: 'BanksAccountsList'},
    {id: 'categories', titleKey: 'CategoriesTitle', icon: 'donut-large', route: 'CategoriesList'},
    {id: 'merchants', titleKey: 'MerchantsTitle', icon: 'store', route: 'MerchantsList'},
  ];

  const s = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.backgroundSecondary,
    },
    listContainer: {
      paddingTop: spacing.lg,
    },
    item: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      backgroundColor: colors.cardBackground,
      paddingHorizontal: spacing.lg,
      paddingVertical: spacing.md,
      borderRadius: radius.md,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      marginHorizontal: spacing.md,
    },
    content: {
      flexDirection: 'row',
      alignItems: 'center',
      flex: 1,
    },
    icon: {
      marginRight: spacing.md,
    },
    title: {
      flex: 1,
      color: colors.textPrimary,
      fontSize: fontSizes.md,
      fontWeight: fontWeights.medium,
    },
    arrow: {
      color: colors.textSecondary,
      fontSize: fontSizes.lg,
    },
    divider: {
      height: spacing.sm,
    },
  });

  return (
    <View style={s.container}>
      <Header title={translate('FinanceTitle')} showBack={false} />
      <View style={s.listContainer}>
        {items.map((item, index) => (
          <React.Fragment key={item.id}>
            <TouchableOpacity style={s.item} onPress={() => navigation.navigate(item.route)}>
              <View style={s.content}>
                <Icon name={item.icon} size={fontSizes.xl} color={colors.textSecondary} style={s.icon} />
                <Text style={s.title}>{translate(item.titleKey)}</Text>
              </View>
              <Text style={s.arrow}>›</Text>
            </TouchableOpacity>
            {index < items.length - 1 ? <View style={s.divider} /> : undefined}
          </React.Fragment>
        ))}
      </View>
    </View>
  );
}
