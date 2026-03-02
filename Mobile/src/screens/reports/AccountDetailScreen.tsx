// CHANGED_BY_AI: 2026-03-02 - Add account detail screen
import React, {useEffect, useState} from 'react';
import {View, Text, StyleSheet, ScrollView, TextInput, TouchableOpacity, ActivityIndicator} from 'react-native';
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

  const [startDate, setStartDate] = useState(route.params.startDate ?? '');
  const [endDate, setEndDate] = useState(route.params.endDate ?? '');

  useEffect(() => {
    dispatch(
      loadAccountDetail({
        accountId: route.params.accountId,
        params: {
          startDate: startDate || undefined,
          endDate: endDate || undefined,
        },
      })
    );
  }, [dispatch, route.params.accountId, startDate, endDate]);

  const s = StyleSheet.create({
    container: {flex: 1, backgroundColor: colors.backgroundSecondary},
    section: {paddingHorizontal: spacing.lg, paddingTop: spacing.lg},
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
    input: {
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      borderRadius: radius.sm,
      paddingHorizontal: spacing.md,
      paddingVertical: spacing.sm,
      color: colors.textPrimary,
      marginTop: spacing.sm,
    },
    listItem: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      padding: spacing.md,
      marginBottom: spacing.sm,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
    row: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: spacing.sm},
  });

  const comparisonVisible = detail?.accountSummary && isCurrentMonthRange(startDate, endDate);

  return (
    <View style={s.container}>
      <Header title={route.params.accountName} />
      <ScrollView contentContainerStyle={{paddingBottom: spacing.xl}}>
        <View style={s.section}>
          <Text style={s.title}>{route.params.accountName}</Text>
          <View style={s.card}>
            <Text style={s.subtitle}>{translate('TotalSpending')}</Text>
            <Text style={s.title}>{formatCurrency(detail?.accountSummary.totalAmount)}</Text>
            {comparisonVisible && detail?.accountSummary.comparison ? (
              <View>
                <View style={s.row}>
                  <Text style={s.subtitle}>{translate('PreviousMonthSamePeriod')}</Text>
                  <Text style={s.subtitle}>{formatCurrency(detail.accountSummary.comparison.previousMonthSamePeriodTotal)}</Text>
                </View>
                <View style={s.row}>
                  <Text style={s.subtitle}>{translate('PercentageChange')}</Text>
                  <Text style={s.subtitle}>{detail.accountSummary.comparison.percentageChange.toFixed(1)}%</Text>
                </View>
              </View>
            ) : null}
          </View>
        </View>

        <View style={s.section}>
          <Text style={s.title}>{translate('Filters')}</Text>
          <View style={s.card}>
            <TextInput
              style={s.input}
              placeholder={translate('StartDatePlaceholder')}
              placeholderTextColor={colors.textSecondary}
              value={startDate}
              onChangeText={setStartDate}
            />
            <TextInput
              style={s.input}
              placeholder={translate('EndDatePlaceholder')}
              placeholderTextColor={colors.textSecondary}
              value={endDate}
              onChangeText={setEndDate}
            />
          </View>
        </View>

        <View style={s.section}>
          <Text style={s.title}>{translate('Categories')}</Text>
          <View style={s.card}>
            {isLoading && !detail ? (
              <ActivityIndicator color={colors.spinner} />
            ) : detail?.categories.length ? (
              detail.categories.map(item => (
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
                  <View style={s.row}>
                    <Text style={s.subtitle}>{item.categoryName}</Text>
                    <Text style={s.subtitle}>{formatCurrency(item.totalAmount)}</Text>
                  </View>
                </TouchableOpacity>
              ))
            ) : (
              <Text style={s.subtitle}>{translate('NoCategoryData')}</Text>
            )}
          </View>
        </View>
      </ScrollView>
    </View>
  );
}
