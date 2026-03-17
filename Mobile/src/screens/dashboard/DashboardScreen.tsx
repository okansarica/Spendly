// CHANGED_BY_AI: 2026-03-02 - Refactor homepage dashboard to spec
import React, { useEffect, useMemo, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  ActivityIndicator,
  RefreshControl,
  Dimensions,
} from 'react-native';
import Icon from 'react-native-vector-icons/MaterialIcons';
import {useTheme} from '../../theme/ThemeContext';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {loadHomepage, refreshHomepage} from '../../store/homepageStore';
import {formatCurrency} from '../../utils/formatCurrency';
import {translate} from '../../utils/translations';
import {HomepageConstants} from '../../constants/homepageConstants';
import Header from '../../components/Header';
import ErrorDisplay from '../../components/ErrorDisplay';
import PieChartCard from '../../components/PieChartCard';
import PaginationDots from '../../components/PaginationDots';
import BarChartCard from '../../components/BarChartCard';

const screenWidth = Dimensions.get('window').width;

const formatPercentage = (percentage: number | undefined): string => {
  return (percentage ?? 0).toFixed(1);
};

const formatNumber = (value: number | undefined, decimals: number = 0): string => {
  return (value ?? 0).toFixed(decimals);
};

export default function DashboardScreen() {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const data = useAppSelector(s => s.homepage.data);
  const isLoading = useAppSelector(s => s.homepage.isLoading);
  const isRefreshing = useAppSelector(s => s.homepage.isRefreshing);
  const error = useAppSelector(s => s.homepage.error);
  const categories = useAppSelector(s => s.categories.items);
  
  const [accountChartPage, setAccountChartPage] = useState(0);
  const [categoryChartPage, setCategoryChartPage] = useState(0);
  
  const chartWidth = useMemo(() => screenWidth - spacing.lg * 4, [spacing.lg]);
  
  const categoryColorMap = useMemo(() => {
    const map: Record<string, string> = {};
    categories.forEach(cat => {
      if (cat.color) {
        map[cat.name] = cat.color;
      }
    });
    return map;
  }, [categories]);
  

  useEffect(() => {
    if (!data && !isLoading && !error) {
      dispatch(loadHomepage());
    }
  }, [data, isLoading, error]);

  const onRefresh = () => {
    dispatch(refreshHomepage());
  };

  const hasAnyData = !!data && (
    data.currentMonthTotalSpending > 0 ||
    data.previousMonthTotalSpending > 0 ||
    data.latestExpenses.length > 0 ||
    data.spendingByAccountCurrentMonth.length > 0 ||
    data.spendingByCategoryCurrentMonth.length > 0 ||
    data.sixMonthTrend.length > 0
  );


  const s = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.backgroundSecondary,
    },
    scrollView: {
      flex: 1,
      backgroundColor: colors.backgroundSecondary,
    },
    content: {
      padding: spacing.lg,
    },
    header: {
      marginBottom: spacing.lg,
    },
    title: {
      fontSize: fontSizes.xxl,
      fontWeight: fontWeights.bold,
      color: colors.textPrimary,
      marginBottom: spacing.xs,
    },
    subtitle: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
    },
    heroCard: {
      backgroundColor: colors.buttonPrimary,
      borderRadius: radius.lg,
      padding: spacing.xl,
      marginBottom: spacing.lg,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 4},
      shadowOpacity: 0.15,
      shadowRadius: 8,
      elevation: 5,
    },
    heroLabel: {
      fontSize: fontSizes.sm,
      color: colors.buttonPrimaryText,
      opacity: 0.9,
      marginBottom: spacing.xs,
      textTransform: 'uppercase',
      letterSpacing: 0.5,
    },
    heroValue: {
      fontSize: 42,
      fontWeight: fontWeights.bold,
      color: colors.buttonPrimaryText,
      marginBottom: spacing.md,
    },
    heroDivider: {
      height: 1,
      backgroundColor: colors.buttonPrimaryText,
      opacity: 0.2,
      marginBottom: spacing.md,
    },
    heroSecondary: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'center',
    },
    heroSecondaryLabel: {
      fontSize: fontSizes.sm,
      color: colors.buttonPrimaryText,
      opacity: 0.8,
    },
    heroSecondaryValue: {
      fontSize: fontSizes.lg,
      fontWeight: fontWeights.semiBold,
      color: colors.buttonPrimaryText,
    },
    comparisonBadge: {
      flexDirection: 'row',
      alignItems: 'center',
      backgroundColor: 'rgba(255,255,255,0.15)',
      paddingHorizontal: spacing.sm,
      paddingVertical: spacing.xs / 2,
      borderRadius: radius.sm,
      marginTop: spacing.xs,
    },
    comparisonText: {
      fontSize: fontSizes.xs,
      color: colors.buttonPrimaryText,
      fontWeight: fontWeights.medium,
    },
    card: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      padding: spacing.lg,
      marginBottom: spacing.lg,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.1,
      shadowRadius: 6,
      elevation: 3,
    },
    cardHeader: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'center',
      marginBottom: spacing.lg,
    },
    cardTitle: {
      fontSize: fontSizes.lg,
      fontWeight: fontWeights.semiBold,
      color: colors.textPrimary,
    },
    chartContainer: {
      alignItems: 'center',
      marginVertical: spacing.md,
    },
    listItem: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'center',
      paddingVertical: spacing.md,
      borderBottomWidth: 1,
      borderBottomColor: colors.borderSubtle,
    },
    listItemLast: {
      borderBottomWidth: 0,
    },
    listItemLabel: {
      fontSize: fontSizes.md,
      fontWeight: fontWeights.semiBold,
      color: colors.textPrimary,
      marginBottom: spacing.xs / 2,
    },
    listItemValue: {
      fontSize: fontSizes.md,
      fontWeight: fontWeights.semiBold,
      color: colors.textPrimary,
    },
    expenseItem: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'flex-start',
      paddingVertical: spacing.md,
      borderBottomWidth: 1,
      borderBottomColor: colors.borderSubtle,
    },
    expenseItemLast: {
      borderBottomWidth: 0,
    },
    expenseLeft: {
      flex: 1,
      marginRight: spacing.md,
    },
    expenseMerchant: {
      fontSize: fontSizes.md,
      fontWeight: fontWeights.semiBold,
      color: colors.textPrimary,
      marginBottom: spacing.xs / 2,
    },
    expenseMerchantSecondary: {
      fontSize: fontSizes.sm,
      color: colors.textPrimary,
      marginBottom: spacing.xs / 2,
    },
    expenseDetails: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
      marginBottom: spacing.xs / 2,
    },
    expenseDate: {
      fontSize: fontSizes.xs,
      color: colors.textSecondary,
    },
    expenseAmount: {
      fontSize: fontSizes.lg,
      fontWeight: fontWeights.bold,
      color: colors.textPrimary,
    },
    emptyState: {
      flex: 1,
      alignItems: 'center',
      justifyContent: 'center',
      padding: spacing.xl,
    },
    emptyText: {
      fontSize: fontSizes.md,
      color: colors.textSecondary,
      textAlign: 'center',
      marginTop: spacing.sm,
    },
    infoCardsRow: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      marginBottom: spacing.lg,
      gap: spacing.md,
    },
    infoCard: {
      flex: 1,
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      padding: spacing.md,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.1,
      shadowRadius: 6,
      elevation: 3,
    },
    infoCardLabel: {
      fontSize: fontSizes.xs,
      color: colors.textSecondary,
      marginBottom: spacing.xs,
      textTransform: 'uppercase',
      letterSpacing: 0.5,
    },
    infoCardValue: {
      fontSize: fontSizes.xl,
      fontWeight: fontWeights.bold,
      color: colors.textPrimary,
      marginBottom: spacing.xs / 2,
    },
    infoCardSubtext: {
      fontSize: fontSizes.xs,
      color: colors.textSecondary,
    },
    infoCardChange: {
      fontSize: fontSizes.xs,
      fontWeight: fontWeights.medium,
      marginTop: spacing.xs / 2,
    },
    infoCardChangePositive: {
      color: colors.success,
    },
    infoCardChangeNegative: {
      color: colors.danger,
    },
    trendChangeRow: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      marginBottom: spacing.sm,
    },
    trendChangeItem: {
      flex: 1,
      textAlign: 'center',
      fontSize: fontSizes.xs,
      color: colors.textSecondary,
    },
  });

  if (isLoading && !data) {
    return (
      <View style={s.container}>
        <Header title={translate('DashboardTitle')} showBack={false} />
        <View style={s.emptyState}>
          <ActivityIndicator size="large" color={colors.spinner} />
        </View>
      </View>
    );
  }

  if (error && !data) {
    return (
      <View style={s.container}>
        <Header title={translate('DashboardTitle')} showBack={false} />
        <ScrollView
          style={s.scrollView}
          contentContainerStyle={{flex: 1}}
          refreshControl={
            <RefreshControl refreshing={isRefreshing} onRefresh={onRefresh} colors={[colors.spinner]} tintColor={colors.spinner} />
          }>
          <ErrorDisplay message={error} />
        </ScrollView>
      </View>
    );
  }

  const renderEmptyState = (messageKey: string) => (
    <View style={s.emptyState}>
      <Icon name="account-balance-wallet" size={HomepageConstants.EmptyIconSize} color={colors.textSecondary} />
      <Text style={s.emptyText}>{translate(messageKey)}</Text>
    </View>
  );

  return (
    <View style={s.container}>
      <Header title={translate('DashboardTitle')} showBack={false} />
      <ScrollView
        style={s.scrollView}
        contentContainerStyle={{paddingBottom: spacing.xl}}
        refreshControl={
          <RefreshControl refreshing={isRefreshing} onRefresh={onRefresh} colors={[colors.spinner]} tintColor={colors.spinner} />
        }>
        {!data || !hasAnyData ? (
          renderEmptyState('EmptyStateMessage')
        ) : (
          <View style={s.content}>
            <View style={s.heroCard}>
              <Text style={s.heroLabel}>{translate('CurrentMonthTotal')}</Text>
              <Text style={s.heroValue}>{formatCurrency(data.currentMonthTotalSpending)}</Text>

              <View style={s.heroDivider} />

              <View style={s.heroSecondary}>
                <Text style={s.heroSecondaryLabel}>{translate('PreviousMonthSamePeriod')}</Text>
                <Text style={s.heroSecondaryValue}>{formatCurrency(data.previousMonthSamePeriodTotalSpending)}</Text>
              </View>

              <View style={s.heroSecondary}>
                <Text style={s.heroSecondaryLabel}>{translate('PreviousMonth')}</Text>
                <Text style={s.heroSecondaryValue}>{formatCurrency(data.previousMonthTotalSpending)}</Text>
              </View>

              <View style={s.comparisonBadge}>
                <Text style={s.comparisonText}>
                  {data.midMonthComparison.isIncreased ? '^' : 'v'} {formatPercentage(data.midMonthComparison.percentageChange)}%
                </Text>
              </View>
            </View>

            <View style={s.infoCardsRow}>
              <View style={s.infoCard}>
                <Text style={s.infoCardLabel}>{translate('ThisWeek')}</Text>
                <Text style={s.infoCardValue}>{formatCurrency(data.weeklySnapshot.thisWeekTotal)}</Text>
                <Text style={s.infoCardSubtext}>
                  {formatCurrency(data.weeklySnapshot.previousWeekTotal)} {translate('VsLastWeek')}
                </Text>
                <Text style={[s.infoCardChange, data.weeklySnapshot.isIncreased ? s.infoCardChangeNegative : s.infoCardChangePositive]}>
                  {data.weeklySnapshot.isIncreased ? '^' : 'v'} {formatPercentage(data.weeklySnapshot.percentageChange)}%
                </Text>
              </View>
              <View style={s.infoCard}>
                <Text style={s.infoCardLabel}>{translate('DailyAverage')}</Text>
                <Text style={s.infoCardValue}>{formatCurrency(data.dailyAverage.currentMonthAverage)}</Text>
                <Text style={s.infoCardSubtext}>
                  {formatCurrency(data.dailyAverage.previousMonthAverage)} {translate('VsLastMonth')}
                </Text>
                <Text style={[s.infoCardChange, data.dailyAverage.isIncreased ? s.infoCardChangeNegative : s.infoCardChangePositive]}>
                  {data.dailyAverage.isIncreased ? '^' : 'v'} {formatPercentage(data.dailyAverage.percentageChange)}%
                </Text>
              </View>
            </View>

            <View style={s.card}>
              <View style={s.cardHeader}>
                <Text style={s.cardTitle}>{translate('Insights')}</Text>
              </View>

              {data.topSpendingCategory.categoryName ? (
                <View style={[s.listItem, !(data.highestSingleExpense.merchantName || data.mostUsedAccount.accountName) && s.listItemLast]}>
                  <View style={s.expenseLeft}>
                    <Text style={s.listItemLabel}>{translate('TopCategory')}</Text>
                    <Text style={s.expenseDetails}>{formatNumber(data.topSpendingCategory.percentageOfTotal)}% {translate('OfTotal')}</Text>
                  </View>
                  <View style={{alignItems: 'flex-end'}}>
                    <Text style={s.listItemValue}>{data.topSpendingCategory.categoryName}</Text>
                    <Text style={s.expenseDetails}>{formatCurrency(data.topSpendingCategory.amount)}</Text>
                  </View>
                </View>
              ) : null}

              {data.highestSingleExpense.transactionName ? (
                <View style={[s.listItem, !data.mostUsedAccount.accountName && s.listItemLast]}>
                  <View style={s.expenseLeft}>
                    <Text style={s.listItemLabel}>{translate('HighestExpense')}</Text>
                    <Text style={s.expenseDetails}>
                      {new Date(data.highestSingleExpense.date).toLocaleDateString('en-GB', {month: 'long', day: 'numeric'})}
                    </Text>
                  </View>
                  <View style={{alignItems: 'flex-end'}}>
                    <Text style={s.listItemValue}>{data.highestSingleExpense.transactionName}</Text>
                    <Text style={s.expenseDetails}>{formatCurrency(data.highestSingleExpense.amount)}</Text>
                  </View>
                </View>
              ) : null}

              {data.mostUsedAccount.accountName ? (
                <View style={[s.listItem, s.listItemLast]}>
                  <View style={s.expenseLeft}>
                    <Text style={s.listItemLabel}>{translate('MostUsedAccount')}</Text>
                    <Text style={s.expenseDetails}>{formatNumber(data.mostUsedAccount.percentageShare)}% {translate('OfTransactions')}</Text>
                  </View>
                  <Text style={s.listItemValue}>{data.mostUsedAccount.accountName}</Text>
                </View>
              ) : null}
            </View>

            <View style={s.card}>
              <View style={s.cardHeader}>
                <Text style={s.cardTitle}>{translate('SpendingByAccount')}</Text>
              </View>
              {data.spendingByAccountCurrentMonth.length > 0 ? (
                <>
                  <ScrollView
                    horizontal
                    pagingEnabled
                    snapToInterval={chartWidth}
                    decelerationRate="fast"
                    showsHorizontalScrollIndicator={false}
                    onScroll={(e) => {
                      const page = Math.round(e.nativeEvent.contentOffset.x / chartWidth);
                      setAccountChartPage(page);
                    }}
                    scrollEventThrottle={16}>
                    <View style={{width: chartWidth}}>
                      <PieChartCard
                          data={data.spendingByAccountCurrentMonth.map(item => ({
                            label: item.accountName,
                            amount: item.amount,
                            percentage: item.percentageOfTotal,
                          }))}
                          chartHeight={HomepageConstants.ChartHeight}
                          title={translate('CurrentMonth')}
                      />
                    </View>
                    <View style={{width: chartWidth}}>
                      <PieChartCard
                          data={data.spendingByAccountPreviousMonth.map(item => ({
                            label: item.accountName,
                            amount: item.amount,
                            percentage: item.percentageOfTotal,
                          }))}
                          chartHeight={HomepageConstants.ChartHeight}
                          title={translate('PreviousMonth')}
                      />
                    </View>
                  </ScrollView>
                  <PaginationDots totalPages={2} activePage={accountChartPage} />
                </>
              ) : (
                renderEmptyState('NoDataForPeriod')
              )}
            </View>

            <View style={s.card}>
              <View style={s.cardHeader}>
                <Text style={s.cardTitle}>{translate('SpendingByCategory')}</Text>
              </View>
              {data.spendingByCategoryCurrentMonth.length > 0 ? (
                <>
                  <ScrollView
                    horizontal
                    pagingEnabled
                    snapToInterval={chartWidth}
                    decelerationRate="fast"
                    showsHorizontalScrollIndicator={false}
                    onScroll={(e) => {
                      const page = Math.round(e.nativeEvent.contentOffset.x / chartWidth);
                      setCategoryChartPage(page);
                    }}
                    scrollEventThrottle={16}>
                    <View style={{width: chartWidth}}>
                      <PieChartCard
                        data={data.spendingByCategoryCurrentMonth.map(item => ({
                          label: item.categoryName,
                          amount: item.amount,
                          percentage: item.percentageOfTotal,
                          color: categoryColorMap[item.categoryName],
                        }))}
                        chartHeight={HomepageConstants.ChartHeight}
                        title={translate('CurrentMonth')}
                      />
                    </View>
                    <View style={{width: chartWidth}}>
                      <PieChartCard
                        data={data.spendingByCategoryPreviousMonth.map(item => ({
                          label: item.categoryName,
                          amount: item.amount,
                          percentage: item.percentageOfTotal,
                          color: categoryColorMap[item.categoryName],
                        }))}
                        chartHeight={HomepageConstants.ChartHeight}
                        title={translate('PreviousMonth')}
                      />
                    </View>
                  </ScrollView>
                  <PaginationDots totalPages={2} activePage={categoryChartPage} />
                </>
              ) : (
                renderEmptyState('NoDataForPeriod')
              )}
            </View>

            <View style={s.card}>
              <View style={s.cardHeader}>
                <Text style={s.cardTitle}>{translate('SixMonthTrend')}</Text>
              </View>
              {data.sixMonthTrend.length > 0 ? (
                <View>
                  <View style={s.trendChangeRow}>
                    {data.sixMonthTrend.map(item => (
                      <Text key={`${item.year}-${item.month}`} style={s.trendChangeItem}>
                        {formatPercentage(item.percentageChange)}%
                      </Text>
                    ))}
                  </View>
                  <View style={s.chartContainer}>
                    <BarChartCard
                      datasets={[
                        {
                          data: data.sixMonthTrend.map(item => item.amount),
                          color: () => colors.buttonPrimary,
                        },
                        {
                          data: data.sixMonthTrend.map(item => item.previousMonthAmount),
                          color: () => colors.buttonPrimaryDisabled,
                        },
                      ]}
                      labels={data.sixMonthTrend.map(item =>
                        new Date(item.year, item.month - 1).toLocaleDateString('en-GB', {month: 'short'})
                      )}
                      chartWidth={chartWidth}
                      chartHeight={HomepageConstants.ChartHeight}
                    />
                  </View>
                </View>
              ) : (
                renderEmptyState('NoTrendData')
              )}
            </View>

            <View style={s.card}>
              <View style={s.cardHeader}>
                <Text style={s.cardTitle}>{translate('LatestExpenses')}</Text>
              </View>
              {data.latestExpenses.length > 0 ? (
                data.latestExpenses.map((item, index) => (
                  <View key={item.transactionId} style={[s.expenseItem, index === data.latestExpenses.length - 1 && s.expenseItemLast]}>
                    <View style={s.expenseLeft}>
                      <Text style={s.expenseMerchant}>{item.transactionName}</Text>
                      <Text style={s.expenseMerchantSecondary}>{item.merchantName}</Text>
                      <Text style={s.expenseDetails}>
                        {item.categoryName} • {item.accountName}
                      </Text>
                      <Text style={s.expenseDate}>
                        {new Date(item.date).toLocaleDateString('en-GB', {
                          month: 'short',
                          day: 'numeric',
                          year: 'numeric',
                        })}
                      </Text>
                    </View>
                    <Text style={s.expenseAmount}>{formatCurrency(item.amount)}</Text>
                  </View>
                ))
              ) : (
                renderEmptyState('NoRecentExpenses')
              )}
            </View>
          </View>
        )}
      </ScrollView>

    </View>
  );
}

