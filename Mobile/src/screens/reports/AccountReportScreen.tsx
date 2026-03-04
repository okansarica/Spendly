// CHANGED_BY_AI: 2026-03-02 - Match account report summary and chart layout to category report
// CHANGED_BY_AI: 2026-03-02 - Align account report UI with category report
// CHANGED_BY_AI: 2026-03-02 - Add accounts overview screen
// CHANGED_BY_AI: 2026-03-02 - Guard percentageChange formatting in account report
import React, {useEffect, useMemo} from 'react';
import {View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator, Dimensions} from 'react-native';
import {PieChart} from 'react-native-chart-kit';
import {useTheme} from '../../theme/ThemeContext';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {loadAccountsOverview} from '../../store/reportsStore';
import {formatCurrency} from '../../utils/formatCurrency';
import {translate} from '../../utils/translations';
import {ReportConstants} from '../../constants/reportConstants';
import Header from '../../components/Header';
import ErrorDisplay from '../../components/ErrorDisplay';
import Icon from 'react-native-vector-icons/MaterialIcons';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import {useNavigation} from '@react-navigation/native';
import type {ReportsStackParamList} from '../../navigation/ReportsNavigator';

const screenWidth = Dimensions.get('window').width;

type AccountsNavProp = NativeStackNavigationProp<ReportsStackParamList, 'AccountsOverview'>;

export default function AccountReportScreen() {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const navigation = useNavigation<AccountsNavProp>();
  const overview = useAppSelector(s => s.reports.accountsOverview);
  const isLoading = useAppSelector(s => s.reports.isLoadingAccountsOverview);
  const error = useAppSelector(s => s.reports.error);

  useEffect(() => {
    if (!overview && !isLoading && !error) {
      dispatch(loadAccountsOverview(undefined));
    }
  }, [overview, isLoading, error]);

  const chartWidth = useMemo(() => screenWidth - spacing.lg * 4, [spacing.lg]);

  const summary = overview?.summary;
  const accounts = overview?.accounts ?? [];
  const distribution = overview?.accountDistribution ?? [];

  const pieData = distribution.map((item, index) => ({
    name: item.accountName,
    population: item.currentMonthToDateTotal,
    color: ReportConstants.ChartColors[index % ReportConstants.ChartColors.length],
    legendFontColor: colors.textSecondary,
    legendFontSize: fontSizes.xs,
  }));

  const trendColor = summary?.differenceAmount ? (summary.differenceAmount > 0 ? colors.danger : summary.differenceAmount < 0 ? colors.success : colors.textSecondary) : colors.textSecondary;

  const s = StyleSheet.create({
    container: {flex: 1, backgroundColor: colors.backgroundSecondary},
    section: {paddingHorizontal: spacing.lg, paddingTop: spacing.lg},
    sectionTitle: {marginBottom: spacing.sm},
    card: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      padding: spacing.lg,
      marginBottom: spacing.lg,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.1,
      shadowRadius: 6,
      elevation: 3,
    },
    chartCard: {alignItems: 'center'},
    chartSummary: {marginTop: spacing.md, width: '100%'},
    chartSummaryRow: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', paddingVertical: spacing.xs},
    chartSummaryLabel: {fontSize: fontSizes.sm, color: colors.textPrimary},
    chartSummaryValue: {fontSize: fontSizes.sm, color: colors.textSecondary, fontWeight: fontWeights.medium},
    title: {fontSize: fontSizes.lg, fontWeight: fontWeights.semiBold, color: colors.textPrimary},
    subtitle: {fontSize: fontSizes.sm, color: colors.textSecondary},
    row: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: spacing.sm},
    label: {fontSize: fontSizes.sm, color: colors.textSecondary},
    value: {fontSize: fontSizes.md, color: colors.textPrimary, fontWeight: fontWeights.medium},
    summaryValueLarge: {fontSize: fontSizes.xxl, color: colors.textPrimary, fontWeight: fontWeights.bold},
    summaryValueSmall: {fontSize: fontSizes.sm, color: colors.textSecondary, fontWeight: fontWeights.medium},
    listItem: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      padding: spacing.md,
      marginBottom: spacing.sm,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
    listRow: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center'},
    listTitle: {fontSize: fontSizes.md, color: colors.textPrimary, fontWeight: fontWeights.medium},
    listSub: {fontSize: fontSizes.sm, color: colors.textSecondary},
  });

  if (isLoading && !overview) {
    return (
      <View style={[s.container, {justifyContent: 'center', alignItems: 'center'}]}>
        <Header title={translate('AccountReportsTitle')} />
        <ActivityIndicator color={colors.spinner} />
      </View>
    );
  }

  if (error && !overview) {
    return (
      <View style={s.container}>
        <Header title={translate('AccountReportsTitle')} />
        <ErrorDisplay message={error} />
      </View>
    );
  }

  return (
    <View style={s.container}>
      <Header title={translate('AccountReportsTitle')} />
      <ScrollView contentContainerStyle={{paddingBottom: spacing.xl}}>
        <View style={s.section}>
          <View style={s.card}>
            <View style={s.row}>
              <Text style={s.label}>{translate('CurrentMonthTotal')}</Text>
              <Text style={s.summaryValueLarge}>{formatCurrency(summary?.currentMonthToDateTotal)}</Text>
            </View>
            <View style={s.row}>
              <Text style={s.label}>{translate('PreviousMonthSamePeriod')}</Text>
              <Text style={s.summaryValueSmall}>{formatCurrency(summary?.previousMonthSamePeriodTotal)}</Text>
            </View>
            <View style={s.row}>
              <Text style={s.label}>{translate('PercentageChange')}</Text>
              <Text style={[s.summaryValueSmall, {color: trendColor}]}>{(summary?.percentageChange ?? 0).toFixed(1)}%</Text>
            </View>
          </View>
        </View>

        <View style={s.section}>
          <Text style={[s.title, s.sectionTitle]}>{translate('AccountDistribution')}</Text>
          {pieData.length > 0 ? (
            <View style={[s.card, s.chartCard]}>
              <PieChart
                data={pieData}
                width={chartWidth}
                height={220}
                chartConfig={{
                  color: () => colors.buttonPrimary,
                  labelColor: () => colors.textSecondary,
                  backgroundGradientFrom: colors.cardBackground,
                  backgroundGradientTo: colors.cardBackground,
                }}
                accessor="population"
                backgroundColor={colors.cardBackground}
                paddingLeft={`${spacing.lg}`}
              />
              <View style={s.chartSummary}>
                {distribution.map(item => (
                  <View key={item.accountId} style={s.chartSummaryRow}>
                    <Text style={s.chartSummaryLabel}>{item.accountName}</Text>
                    <Text style={s.chartSummaryValue}>{formatCurrency(item.currentMonthToDateTotal)}</Text>
                  </View>
                ))}
              </View>
            </View>
          ) : (
            <Text style={s.subtitle}>{translate('NoAccountData')}</Text>
          )}
        </View>

        <View style={s.section}>
          <Text style={[s.title, s.sectionTitle]}>{translate('AccountList')}</Text>
          {accounts.length === 0 ? (
            <Text style={s.subtitle}>{translate('NoAccountData')}</Text>
          ) : (
            accounts.map(item => (
              <TouchableOpacity
                key={item.accountId}
                style={s.listItem}
                onPress={() =>
                  navigation.navigate('AccountDetail', {
                    accountId: item.accountId,
                    accountName: item.accountName,
                  })
                }>
                <View style={s.listRow}>
                  <Text style={s.listTitle}>{item.accountName}</Text>
                  <Text style={s.value}>{formatCurrency(item.currentMonthToDateTotal)}</Text>
                </View>
                <View style={s.listRow}>
                  <Text style={s.listSub}>{translate('PreviousMonthSamePeriod')}</Text>
                  <Text style={s.listSub}>{formatCurrency(item.previousMonthSamePeriodTotal)}</Text>
                </View>
                <View style={s.listRow}>
                  <Text style={s.listSub}>{translate('PercentageChange')}</Text>
                  <Text style={[s.listSub, {color: item.differenceAmount > 0 ? colors.danger : item.differenceAmount < 0 ? colors.success : colors.textSecondary}]}>
                    {(item.percentageChange ?? 0).toFixed(1)}%
                  </Text>
                </View>
              </TouchableOpacity>
            ))
          )}
        </View>
      </ScrollView>
    </View>
  );
}
