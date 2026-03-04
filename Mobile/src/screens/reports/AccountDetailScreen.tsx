// CHANGED_BY_AI: 2026-03-02 - Align account detail layout and sorting with category screens
// CHANGED_BY_AI: 2026-03-02 - Align account detail UI with category detail
// CHANGED_BY_AI: 2026-03-02 - Add date picker filter popup
// CHANGED_BY_AI: 2026-03-02 - Add account detail screen
// CHANGED_BY_AI: 2026-03-02 - Guard percentageChange formatting
import React, {useEffect, useMemo, useState} from 'react';
import {View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator} from 'react-native';
import {useTheme} from '../../theme/ThemeContext';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {loadAccountDetail} from '../../store/reportsStore';
import {formatCurrency} from '../../utils/formatCurrency';
import {translate} from '../../utils/translations';
import type {NativeStackScreenProps} from '@react-navigation/native-stack';
import type {ReportsStackParamList} from '../../navigation/ReportsNavigator';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import {useNavigation} from '@react-navigation/native';
import Header from '../../components/Header';
import ErrorDisplay from '../../components/ErrorDisplay';
import Icon from 'react-native-vector-icons/MaterialIcons';

const isCurrentMonthRange = (start: string, end: string): boolean => {
  if (!start || !end) {
    return false;
  }
  const now = new Date();
  const startDate = new Date(start);
  const endDate = new Date(end);
  return startDate.getFullYear() === now.getFullYear() && startDate.getMonth() === now.getMonth() && startDate.getDate() === 1 && endDate.toDateString() === now.toDateString();
};

type Props = NativeStackScreenProps<ReportsStackParamList, 'AccountDetail'>;

type NavProp = NativeStackNavigationProp<ReportsStackParamList, 'AccountDetail'>;

export default function AccountDetailScreen({route}: Props) {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const navigation = useNavigation<NavProp>();
  const detail = useAppSelector(s => s.reports.accountDetail);
  const isLoading = useAppSelector(s => s.reports.isLoadingAccountDetail);
  const error = useAppSelector(s => s.reports.error);

  const [startDate, setStartDate] = useState(route.params.startDate ?? '');
  const [endDate, setEndDate] = useState(route.params.endDate ?? '');
  const [sortBy, setSortBy] = useState<'name' | 'amount'>('amount');

  useEffect(() => {
    if (!isLoading && !error) {
      dispatch(
        loadAccountDetail({
          accountId: route.params.accountId,
          params: {
            startDate: startDate || undefined,
            endDate: endDate || undefined,
          },
        })
      );
    }
  }, [route.params.accountId, startDate, endDate]);

  const s = StyleSheet.create({
    container: {flex: 1, backgroundColor: colors.backgroundSecondary},
    section: {paddingHorizontal: spacing.lg, paddingTop: spacing.lg},
    sectionTitle: {marginBottom: spacing.sm},
    title: {fontSize: fontSizes.lg, fontWeight: fontWeights.semiBold, color: colors.textPrimary},
    subtitle: {fontSize: fontSizes.sm, color: colors.textSecondary},
    card: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      padding: spacing.lg,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.1,
      shadowRadius: 6,
      elevation: 3,
    },
    summaryRow: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center'},
    summaryValue: {fontSize: fontSizes.lg, fontWeight: fontWeights.semiBold, color: colors.textPrimary},
    listItem: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      padding: spacing.md,
      marginBottom: spacing.sm,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
    row: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: spacing.sm},
    listRow: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center'},
    listTitle: {fontSize: fontSizes.md, color: colors.textPrimary, fontWeight: fontWeights.medium},
    listSub: {fontSize: fontSizes.sm, color: colors.textSecondary},
    value: {fontSize: fontSizes.md, color: colors.textPrimary, fontWeight: fontWeights.medium},
    headerRow: {flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between'},
    iconButton: {
      width: spacing.xl + spacing.sm,
      height: spacing.xl + spacing.sm,
      borderRadius: (spacing.xl + spacing.sm) / 2,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      backgroundColor: colors.cardBackground,
      alignItems: 'center',
      justifyContent: 'center',
    },
  });

  const comparisonVisible = detail?.accountSummary && isCurrentMonthRange(startDate, endDate);
  const sortedCategories = useMemo(() => {
    const items = [...(detail?.categories ?? [])];
    if (sortBy === 'name') {
      items.sort((a, b) => a.categoryName.localeCompare(b.categoryName));
    } else {
      items.sort((a, b) => b.totalAmount - a.totalAmount);
    }
    return items;
  }, [detail?.categories, sortBy]);

  if (error && !detail) {
    return (
      <View style={s.container}>
        <Header title={route.params.accountName} />
        <ErrorDisplay message={error} />
      </View>
    );
  }

  return (
    <View style={s.container}>
      <Header title={route.params.accountName} />
      <ScrollView contentContainerStyle={{paddingBottom: spacing.xl}}>
        <View style={s.section}>
          <View style={s.card}>
            <View style={s.summaryRow}>
              <Text style={s.subtitle}>{translate('TotalSpending')}</Text>
              <Text style={s.summaryValue}>{formatCurrency(detail?.accountSummary.totalAmount)}</Text>
            </View>
            {comparisonVisible && detail?.accountSummary.comparison ? (
              <View>
                <View style={s.row}>
                  <Text style={s.subtitle}>{translate('PreviousMonthSamePeriod')}</Text>
                  <Text style={s.subtitle}>{formatCurrency(detail.accountSummary.comparison.previousMonthSamePeriodTotal)}</Text>
                </View>
                <View style={s.row}>
                  <Text style={s.subtitle}>{translate('PercentageChange')}</Text>
                  <Text style={s.subtitle}>{(detail.accountSummary.comparison.percentageChange ?? 0).toFixed(1)}%</Text>
                </View>
              </View>
            ) : null}
          </View>
        </View>

        <View style={s.section}>
          <View style={s.headerRow}>
            <Text style={[s.title, s.sectionTitle]}>{translate('Categories')}</Text>
            <TouchableOpacity style={s.iconButton} onPress={() => setSortBy(sortBy === 'name' ? 'amount' : 'name')}>
              <Icon name={sortBy === 'name' ? 'sort-by-alpha' : 'attach-money'} size={fontSizes.lg} color={colors.textSecondary} />
            </TouchableOpacity>
          </View>
          {isLoading && !detail ? (
            <ActivityIndicator color={colors.spinner} />
          ) : sortedCategories.length ? (
            sortedCategories.map(item => (
              <TouchableOpacity
                key={item.categoryId}
                style={s.listItem}
                onPress={() =>
                  navigation.navigate('CategoryDetail', {
                    categoryId: item.categoryId,
                    categoryName: item.categoryName,
                    startDate: startDate || undefined,
                    endDate: endDate || undefined,
                    accountIds: [route.params.accountId],
                  })
                }>
                <View style={s.listRow}>
                  <Text style={s.listTitle}>{item.categoryName}</Text>
                  <Text style={s.value}>{formatCurrency(item.totalAmount)}</Text>
                </View>
              </TouchableOpacity>
            ))
          ) : (
            <Text style={s.subtitle}>{translate('NoCategoryData')}</Text>
          )}
        </View>
      </ScrollView>
    </View>
  );
}
