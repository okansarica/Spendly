// CHANGED_BY_AI: 2026-03-02 - Add date picker filter popup
// CHANGED_BY_AI: 2026-03-02 - Add account detail screen
import React, {useEffect, useState} from 'react';
import {View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator, Modal} from 'react-native';
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
import Button from '../../components/Button';
import Icon from 'react-native-vector-icons/MaterialIcons';
import DateTimePicker from '@react-native-community/datetimepicker';

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
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [isStartPickerOpen, setIsStartPickerOpen] = useState(false);
  const [isEndPickerOpen, setIsEndPickerOpen] = useState(false);

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
    modalBackdrop: {flex: 1, backgroundColor: 'rgba(0,0,0,0.4)', justifyContent: 'center', padding: spacing.lg},
    modalCard: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      padding: spacing.lg,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
    modalTitle: {fontSize: fontSizes.lg, fontWeight: fontWeights.semiBold, color: colors.textPrimary, marginBottom: spacing.sm},
    modalActions: {flexDirection: 'row', justifyContent: 'space-between', marginTop: spacing.md, gap: spacing.sm},
  });

  const comparisonVisible = detail?.accountSummary && isCurrentMonthRange(startDate, endDate);
  const formatDateValue = (date: Date): string => date.toISOString().split('T')[0];

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
          <View style={s.headerRow}>
            <Text style={s.title}>{translate('Filters')}</Text>
            <TouchableOpacity style={s.iconButton} onPress={() => setIsFilterOpen(true)}>
              <Icon name="filter-list" size={fontSizes.lg} color={colors.textSecondary} />
            </TouchableOpacity>
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
      <Modal visible={isFilterOpen} transparent animationType="fade" onRequestClose={() => setIsFilterOpen(false)}>
        <View style={s.modalBackdrop}>
          <View style={s.modalCard}>
            <Text style={s.modalTitle}>{translate('Filters')}</Text>
            <TouchableOpacity style={s.input} onPress={() => setIsStartPickerOpen(true)}>
              <Text style={{color: startDate ? colors.textPrimary : colors.textSecondary}}>
                {startDate || translate('StartDatePlaceholder')}
              </Text>
            </TouchableOpacity>
            <TouchableOpacity style={s.input} onPress={() => setIsEndPickerOpen(true)}>
              <Text style={{color: endDate ? colors.textPrimary : colors.textSecondary}}>
                {endDate || translate('EndDatePlaceholder')}
              </Text>
            </TouchableOpacity>
            {isStartPickerOpen ? (
              <DateTimePicker
                value={startDate ? new Date(startDate) : new Date()}
                mode="date"
                display="default"
                onChange={(_, selectedDate) => {
                  setIsStartPickerOpen(false);
                  if (selectedDate) {
                    setStartDate(formatDateValue(selectedDate));
                  }
                }}
              />
            ) : null}
            {isEndPickerOpen ? (
              <DateTimePicker
                value={endDate ? new Date(endDate) : new Date()}
                mode="date"
                display="default"
                onChange={(_, selectedDate) => {
                  setIsEndPickerOpen(false);
                  if (selectedDate) {
                    setEndDate(formatDateValue(selectedDate));
                  }
                }}
              />
            ) : null}
            <View style={s.modalActions}>
              <Button text={translate('Cancel')} onPress={() => setIsFilterOpen(false)} variant="secondary" style={{flex: 1}} />
              <Button text={translate('ApplyFilters')} onPress={() => setIsFilterOpen(false)} variant="primary" style={{flex: 1}} />
            </View>
          </View>
        </View>
      </Modal>
    </View>
  );
}
