// CHANGED_BY_AI: 2026-03-02 - Use date picker in filter modal
// CHANGED_BY_AI: 2026-03-02 - Add category detail screen
import React, {useEffect, useState} from 'react';
import {View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator, Modal, Platform} from 'react-native';
import {useTheme} from '../../theme/ThemeContext';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {loadCategoryDetail} from '../../store/reportsStore';
import {formatCurrency} from '../../utils/formatCurrency';
import {translate} from '../../utils/translations';
import type {NativeStackScreenProps} from '@react-navigation/native-stack';
import Header from '../../components/Header';
import ErrorDisplay from '../../components/ErrorDisplay';
import Button from '../../components/Button';
import Icon from 'react-native-vector-icons/MaterialIcons';
import DateTimePicker from '@react-native-community/datetimepicker';
import type {ReportsStackParamList} from '../../navigation/ReportsNavigator';

type Props = NativeStackScreenProps<ReportsStackParamList, 'CategoryDetail'>;

export default function CategoryDetailScreen({route}: Props) {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const detail = useAppSelector(s => s.reports.categoryDetail);
  const isLoading = useAppSelector(s => s.reports.isLoadingCategoryDetail);
  const error = useAppSelector(s => s.reports.error);
  const today = new Date().toISOString().split('T')[0];
  const firstDayOfMonth = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0];
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [draftStartDate, setDraftStartDate] = useState(route.params.startDate ?? firstDayOfMonth);
  const [draftEndDate, setDraftEndDate] = useState(route.params.endDate ?? today);
  const [activePicker, setActivePicker] = useState<'start' | 'end' | null>(null);

  const [startDate, setStartDate] = useState(route.params.startDate ?? firstDayOfMonth);
  const [endDate, setEndDate] = useState(route.params.endDate ?? today);
  const [sortBy, setSortBy] = useState<'date' | 'amount'>('date');
  const [page, setPage] = useState(1);

  useEffect(() => {
    if (!isLoading && !error) {
      dispatch(
        loadCategoryDetail({
          categoryId: route.params.categoryId,
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
  }, [route.params.categoryId, startDate, endDate, page, route.params.accountId]);

  useEffect(() => {
    if (isFilterOpen) {
      setDraftStartDate(startDate);
      setDraftEndDate(endDate);
      setActivePicker(null);
    }
  }, [isFilterOpen, startDate, endDate]);

  const sortedTransactions = React.useMemo(() => {
    const items = [...(detail?.transactions.items ?? [])];
    if (sortBy === 'amount') {
      return items.sort((a, b) => b.amount - a.amount);
    }
    return items.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
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
  const onPickerChange = (_: unknown, selectedDate?: Date) => {
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
        <Header title={route.params.categoryName} />
        <ErrorDisplay message={error} />
      </View>
    );
  }

  return (
    <View style={s.container}>
      <Header title={route.params.categoryName} />
      <ScrollView contentContainerStyle={{paddingBottom: spacing.xl}}>
        <View style={s.section}>
          <View style={s.headerRow}>
            <Text style={s.title}>{route.params.categoryName}</Text>
            <View />
          </View>
          <View style={s.card}>
            <View style={s.summaryRow}>
              <Text style={s.subtitle}>{translate('CategoryTotal')}</Text>
              <Text style={s.summaryValue}>{formatCurrency(detail?.categorySummary.totalAmount)}</Text>
            </View>
          </View>
        </View>

        <View style={s.section}>
          <View style={s.headerRow}>
            <Text style={[s.title, s.sectionTitle]}>{translate('Transactions')}</Text>
            <View style={s.headerActions}>
              <TouchableOpacity style={s.iconButton} onPress={() => setIsFilterOpen(true)}>
                <Icon name="filter-list" size={fontSizes.lg} color={colors.textSecondary} />
              </TouchableOpacity>
              <TouchableOpacity style={s.iconButton} onPress={() => setSortBy(sortBy === 'date' ? 'amount' : 'date')}>
                <Icon name={sortBy === 'date' ? 'event' : 'attach-money'} size={fontSizes.lg} color={colors.textSecondary} />
              </TouchableOpacity>
            </View>
          </View>
          <View style={s.card}>
            {isLoading && !detail ? (
              <ActivityIndicator color={colors.spinner} />
            ) : sortedTransactions.length === 0 ? (
              <Text style={s.subtitle}>{translate('NoTransactions')}</Text>
            ) : (
              sortedTransactions.map((item, index) => (
                <View key={item.transactionId} style={[s.expenseItem, index === sortedTransactions.length - 1 && s.expenseItemLast]}>
                  <View style={s.expenseLeft}>
                    <Text style={s.expenseMerchant}>{(item as any).transactionName}</Text>
                    <Text style={s.expenseDetails}>{item.merchantName}</Text>
                    <Text style={s.expenseDetails}>{item.accountName}</Text>
                    <Text style={s.expenseDate}>{new Date(item.date).toLocaleDateString()}</Text>
                  </View>
                  <Text style={s.expenseAmount}>{formatCurrency(item.amount)}</Text>
                </View>
              ))
            )}
            <View style={s.row}>
              <Button text={translate('PreviousPage')} onPress={() => setPage(Math.max(page - 1, 1))} variant="secondary" />
              <Text style={s.subtitle}>{page} / {totalPages}</Text>
              <Button text={translate('NextPage')} onPress={() => setPage(Math.min(page + 1, totalPages))} variant="secondary" />
            </View>
          </View>
        </View>
      </ScrollView>
      <Modal visible={isFilterOpen} transparent animationType="fade" onRequestClose={() => setIsFilterOpen(false)}>
        <View style={s.modalBackdrop}>
          <View style={s.modalCard}>
            <Text style={s.modalTitle}>{translate('Filters')}</Text>
            <TouchableOpacity style={s.input} onPress={() => openPicker('start')}>
              <Text style={{color: draftStartDate ? colors.textPrimary : colors.textSecondary}}>
                {draftStartDate || translate('StartDatePlaceholder')}
              </Text>
            </TouchableOpacity>
            <TouchableOpacity style={s.input} onPress={() => openPicker('end')}>
              <Text style={{color: draftEndDate ? colors.textPrimary : colors.textSecondary}}>
                {draftEndDate || translate('EndDatePlaceholder')}
              </Text>
            </TouchableOpacity>
            <View style={s.modalActions}>
              <Button
                text={translate('Cancel')}
                onPress={() => {
                  setIsFilterOpen(false);
                  setActivePicker(null);
                }}
                variant="secondary"
                style={{flex: 1}}
              />
              <Button
                text={translate('ApplyFilters')}
                onPress={() => {
                  setStartDate(draftStartDate);
                  setEndDate(draftEndDate);
                  setIsFilterOpen(false);
                  setActivePicker(null);
                }}
                variant="primary"
                style={{flex: 1}}
              />
            </View>
          </View>
          {Platform.OS === 'ios' && activePicker ? (
            <View style={s.pickerSheet}>
              <View style={s.pickerSheetCard}>
                <DateTimePicker
                  value={
                    activePicker === 'start'
                      ? (draftStartDate ? new Date(draftStartDate) : new Date())
                      : (draftEndDate ? new Date(draftEndDate) : new Date())
                  }
                  mode="date"
                  display="spinner"
                  onChange={onPickerChange}
                />
              </View>
            </View>
          ) : null}
          {Platform.OS === 'android' && activePicker ? (
            <DateTimePicker
              value={
                activePicker === 'start'
                  ? (draftStartDate ? new Date(draftStartDate) : new Date())
                  : (draftEndDate ? new Date(draftEndDate) : new Date())
              }
              mode="date"
              display="default"
              onChange={onPickerChange}
            />
          ) : null}
        </View>
      </Modal>
    </View>
  );
}
