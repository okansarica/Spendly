// CHANGED_BY_AI: 2026-03-13 - Add merchant detail screen
import React, {useEffect, useState, useMemo} from 'react';
import {View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator, Modal, Button, Platform} from 'react-native';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {loadMerchantDetail} from '../../store/reportsStore';
import {useTheme} from '../../theme/ThemeContext';
import {translate} from '../../utils/translations';
import {formatCurrency} from '../../utils/formatCurrency';
import {NativeStackScreenProps} from '@react-navigation/native-stack';
import {ReportsStackParamList} from '../../navigation/ReportsNavigator';
import Header from '../../components/Header';
import ErrorDisplay from '../../components/ErrorDisplay';
import Icon from 'react-native-vector-icons/MaterialIcons';
import DateTimePicker, {DateTimePickerEvent} from '@react-native-community/datetimepicker';

type Props = NativeStackScreenProps<ReportsStackParamList, 'MerchantDetail'>;

export default function MerchantDetailScreen({route}: Props) {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const detail = useAppSelector(s => s.reports.merchantDetail);
  const isLoading = useAppSelector(s => s.reports.isLoadingMerchantDetail);
  const error = useAppSelector(s => s.reports.error);
  const today = new Date().toISOString().split('T')[0];
  const firstDayOfMonth = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0];
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [draftStartDate, setDraftStartDate] = useState(route.params.startDate ?? firstDayOfMonth);
  const [draftEndDate, setDraftEndDate] = useState(route.params.endDate ?? today);
  const [activePicker, setActivePicker] = useState<'start' | 'end' | null>(null);

  const [startDate, setStartDate] = useState(route.params.startDate ?? firstDayOfMonth);
  const [endDate, setEndDate] = useState(route.params.endDate ?? today);
  const [sortBy, setSortBy] = useState<'name' | 'amount' | 'date'>('name');
  const [page, setPage] = useState(1);

  useEffect(() => {
    if (!isLoading && !error) {
      dispatch(
        loadMerchantDetail({
          merchantId: route.params.merchantId,
          params: {
            startDate: startDate || undefined,
            endDate: endDate || undefined,
            page,
            pageSize: detail?.transactions.pageSize,
            accountId: route.params.accountId,
          },
        })
      );
    }
  }, [route.params.merchantId, startDate, endDate, page, route.params.accountId]);

  useEffect(() => {
    if (isFilterOpen) {
      setDraftStartDate(startDate);
      setDraftEndDate(endDate);
      setActivePicker(null);
    }
  }, [isFilterOpen, startDate, endDate]);

  const sortedTransactions = useMemo(() => {
    const items = [...(detail?.transactions.items ?? [])];
    if (sortBy === 'amount') {
      return items.sort((a, b) => b.amount - a.amount);
    }
    if (sortBy === 'date') {
      return items.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
    }
    return items.sort((a, b) => a.transactionName.localeCompare(b.transactionName));
  }, [detail?.transactions.items, sortBy]);

  const totalPages = detail?.transactions.totalPages ?? 1;

  const s = StyleSheet.create({
    container: {flex: 1, backgroundColor: colors.backgroundSecondary},
    section: {paddingHorizontal: spacing.lg, paddingTop: spacing.lg},
    sectionTitle: {marginBottom: spacing.sm},
    headerRow: {flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between'},
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
    headerActions: {flexDirection: 'row', alignItems: 'center', gap: spacing.sm},
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
    summaryRow: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center'},
    summaryValue: {fontSize: fontSizes.lg, fontWeight: fontWeights.semiBold, color: colors.textPrimary},
    input: {
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      borderRadius: radius.sm,
      paddingHorizontal: spacing.md,
      paddingVertical: spacing.sm,
      color: colors.textPrimary,
      marginTop: spacing.sm,
    },
    row: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: spacing.sm},
    buttonRow: {flexDirection: 'row', gap: spacing.sm, marginTop: spacing.sm},
    modalActions: {flexDirection: 'row', justifyContent: 'space-between', marginTop: spacing.md, gap: spacing.sm},
    expenseItem: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'flex-start',
      paddingVertical: spacing.md,
      borderBottomWidth: 1,
      borderBottomColor: colors.borderSubtle,
    },
    expenseItemLast: {borderBottomWidth: 0},
    expenseLeft: {flex: 1, marginRight: spacing.md},
    expenseMerchant: {fontSize: fontSizes.md, fontWeight: fontWeights.semiBold, color: colors.textPrimary, marginBottom: spacing.xs / 2},
    expenseDetails: {fontSize: fontSizes.sm, color: colors.textSecondary, marginBottom: spacing.xs / 2},
    expenseDate: {fontSize: fontSizes.xs, color: colors.textSecondary},
    expenseAmount: {fontSize: fontSizes.lg, fontWeight: fontWeights.bold, color: colors.textPrimary},
    modalBackdrop: {flex: 1, backgroundColor: 'rgba(0,0,0,0.4)', justifyContent: 'center', padding: spacing.lg},
    modalCard: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      padding: spacing.lg,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
    modalTitle: {fontSize: fontSizes.lg, fontWeight: fontWeights.semiBold, color: colors.textPrimary, marginBottom: spacing.sm},
    pickerSheet: {position: 'absolute', left: 0, right: 0, bottom: 0, padding: spacing.lg},
    pickerSheetCard: {
      backgroundColor: colors.cardBackground,
      borderTopLeftRadius: radius.lg,
      borderTopRightRadius: radius.lg,
      padding: spacing.md,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
  });

  const formatDateValue = (date: Date): string => date.toISOString().split('T')[0];
  const openPicker = (target: 'start' | 'end') => {
    setActivePicker(target);
  };
  const onPickerChange = (_: DateTimePickerEvent, selectedDate?: Date) => {
    if (!selectedDate || !activePicker) {
      if (Platform.OS === 'android') {
        setActivePicker(null);
      }
      return;
    }
    const value = formatDateValue(selectedDate);
    if (activePicker === 'start') {
      setDraftStartDate(value);
    }
    if (activePicker === 'end') {
      setDraftEndDate(value);
    }
    if (Platform.OS === 'android') {
      setActivePicker(null);
    }
  };

  if (error && !detail) {
    return (
      <View style={s.container}>
        <Header title={route.params.merchantName} />
        <ErrorDisplay message={error} />
      </View>
    );
  }

  return (
    <View style={s.container}>
      <Header title={route.params.merchantName} />
      <ScrollView contentContainerStyle={{paddingBottom: spacing.xl}}>
        <View style={s.section}>
          <View style={s.headerRow}>
            <Text style={s.title}>{translate('Summary')}</Text>
            <View style={s.headerActions}>
              <TouchableOpacity style={s.iconButton} onPress={() => setIsFilterOpen(true)}>
                <Icon name="filter-list" size={fontSizes.lg} color={colors.textSecondary} />
              </TouchableOpacity>
              <TouchableOpacity
                style={s.iconButton}
                onPress={() => setSortBy(sortBy === 'name' ? 'amount' : sortBy === 'amount' ? 'date' : 'name')}>
                <Icon
                  name={sortBy === 'name' ? 'sort-by-alpha' : sortBy === 'amount' ? 'attach-money' : 'calendar-today'}
                  size={fontSizes.lg}
                  color={colors.textSecondary}
                />
              </TouchableOpacity>
            </View>
          </View>
          <View style={s.card}>
            <View style={s.summaryRow}>
              <Text style={s.subtitle}>{translate('TotalSpending')}</Text>
              <Text style={s.summaryValue}>{formatCurrency(detail?.merchantSummary.totalAmount)}</Text>
            </View>
            {detail?.merchantSummary.comparison ? (
              <View>
                <View style={s.row}>
                  <Text style={s.subtitle}>{translate('PreviousMonthSamePeriod')}</Text>
                  <Text style={s.subtitle}>{formatCurrency(detail.merchantSummary.comparison.previousMonthSamePeriodTotal)}</Text>
                </View>
                <View style={s.row}>
                  <Text style={s.subtitle}>{translate('PercentageChange')}</Text>
                  <Text style={s.subtitle}>{(detail.merchantSummary.comparison.percentageChange ?? 0).toFixed(1)}%</Text>
                </View>
              </View>
            ) : null}
          </View>
        </View>

        <View style={s.section}>
          <Text style={[s.title, s.sectionTitle]}>{translate('Transactions')}</Text>
          {isLoading && !detail ? (
            <ActivityIndicator color={colors.spinner} />
          ) : sortedTransactions.length ? (
            <View style={s.card}>
              {sortedTransactions.map((item, index) => (
                <View key={item.transactionId} style={[s.expenseItem, index === sortedTransactions.length - 1 ? s.expenseItemLast : undefined]}>
                  <View style={s.expenseLeft}>
                    <Text style={s.expenseMerchant}>{item.transactionName}</Text>
                    <Text style={s.expenseDetails}>{item.accountName}</Text>
                    <Text style={s.expenseDate}>{new Date(item.date).toLocaleDateString()}</Text>
                  </View>
                  <Text style={s.expenseAmount}>{formatCurrency(item.amount)}</Text>
                </View>
              ))}
              {totalPages > 1 && (
                <View style={s.buttonRow}>
                  <Button title={translate('Previous')} onPress={() => setPage(Math.max(1, page - 1))} disabled={page === 1} />
                  <Text style={s.subtitle}>
                    {translate('Page')} {page} {translate('of')} {totalPages}
                  </Text>
                  <Button title={translate('Next')} onPress={() => setPage(Math.min(totalPages, page + 1))} disabled={page === totalPages} />
                </View>
              )}
            </View>
          ) : (
            <Text style={s.subtitle}>{translate('NoTransactions')}</Text>
          )}
        </View>
      </ScrollView>
      <Modal visible={isFilterOpen} transparent animationType="fade" onRequestClose={() => setIsFilterOpen(false)}>
        <TouchableOpacity style={s.modalBackdrop} activeOpacity={1} onPress={() => setIsFilterOpen(false)}>
          <TouchableOpacity activeOpacity={1} style={s.modalCard}>
            <Text style={s.modalTitle}>{translate('FilterTransactions')}</Text>
            <Text style={s.subtitle}>{translate('StartDate')}</Text>
            <TouchableOpacity onPress={() => openPicker('start')}>
              <Text style={s.input}>{draftStartDate}</Text>
            </TouchableOpacity>
            <Text style={s.subtitle}>{translate('EndDate')}</Text>
            <TouchableOpacity onPress={() => openPicker('end')}>
              <Text style={s.input}>{draftEndDate}</Text>
            </TouchableOpacity>
            <View style={s.modalActions}>
              <Button title={translate('Cancel')} onPress={() => setIsFilterOpen(false)} />
              <Button
                title={translate('Apply')}
                onPress={() => {
                  setStartDate(draftStartDate);
                  setEndDate(draftEndDate);
                  setIsFilterOpen(false);
                }}
              />
            </View>
          </TouchableOpacity>
        </TouchableOpacity>
        {activePicker && (
          <View style={s.pickerSheet}>
            <View style={s.pickerSheetCard}>
              <DateTimePicker
                value={new Date(activePicker === 'start' ? draftStartDate : draftEndDate)}
                mode="date"
                display={Platform.OS === 'ios' ? 'inline' : 'default'}
                onChange={onPickerChange}
              />
              {Platform.OS === 'ios' && (
                <Button title={translate('Done')} onPress={() => setActivePicker(null)} />
              )}
            </View>
          </View>
        )}
      </Modal>
    </View>
  );
}
