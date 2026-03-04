// CHANGED_BY_AI: 2026-03-02 - Guard percentageChange formatting in category report
// CHANGED_BY_AI: 2026-03-02 - Add reports overview screen
import React, {useEffect, useMemo} from 'react';
import {View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator, Dimensions} from 'react-native';
import {BarChart} from 'react-native-chart-kit';
import {useTheme} from '../../theme/ThemeContext';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {loadReportsOverview} from '../../store/reportsStore';
import {formatCurrency} from '../../utils/formatCurrency';
import {translate} from '../../utils/translations';
import Header from '../../components/Header';
import ErrorDisplay from '../../components/ErrorDisplay';
import PieChartCard from '../../components/PieChartCard';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import {useNavigation} from '@react-navigation/native';
import type {ReportsStackParamList} from '../../navigation/ReportsNavigator';

const screenWidth = Dimensions.get('window').width;

type ReportsNavProp = NativeStackNavigationProp<ReportsStackParamList, 'ReportsOverview'>;

export default function CategoryReportScreen() {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const navigation = useNavigation<ReportsNavProp>();
  const overview = useAppSelector(s => s.reports.overview);
  const isLoading = useAppSelector(s => s.reports.isLoadingOverview);
  const error = useAppSelector(s => s.reports.error);
  const categoriesFromStore = useAppSelector(s => s.categories.items);

  useEffect(() => {
    if (!overview && !isLoading && !error) {
      dispatch(loadReportsOverview(undefined));
    }
  }, [overview, isLoading, error]);

  const chartWidth = useMemo(() => screenWidth - spacing.lg * 4, [spacing.lg]);
  
  const categoryColorMap = useMemo(() => {
    const map: Record<string, string> = {};
    categoriesFromStore.forEach(cat => {
      if (cat.color) {
        map[cat.name] = cat.color;
      }
    });
    return map;
  }, [categoriesFromStore]);

  const summary = overview?.summary;
  const categories = overview?.categories ?? [];
  const topChanging = (overview?.topChangingCategories ?? []).slice(0, 4);
  const distribution = overview?.categoryDistribution ?? [];

  const barLabels = topChanging.map(c => c.categoryName);
  const barData = topChanging.map(c => Math.abs(c.differenceAmount));

  const pieChartData = distribution.map(item => {
    const total = distribution.reduce((sum, d) => sum + d.currentMonthToDateTotal, 0);
    return {
      label: item.categoryName,
      amount: item.currentMonthToDateTotal,
      percentage: total > 0 ? (item.currentMonthToDateTotal / total) * 100 : 0,
      color: categoryColorMap[item.categoryName],
    };
  });

  const trendColor = summary?.differenceAmount
    ? summary.differenceAmount > 0
      ? colors.danger
      : summary.differenceAmount < 0
        ? colors.success
        : colors.textSecondary
    : colors.textSecondary;

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
    chartCard: {},
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
    button: {marginTop: spacing.sm, alignSelf: 'flex-start'},
    buttonText: {color: colors.buttonPrimary, fontSize: fontSizes.sm, fontWeight: fontWeights.medium},
  });

  if (isLoading && !overview) {
    return (
      <View style={[s.container, {justifyContent: 'center', alignItems: 'center'}]}>
        <Header title={translate('ReportOverviewTitle')} />
        <ActivityIndicator color={colors.spinner} />
      </View>
    );
  }

  if (error && !overview) {
    return (
      <View style={s.container}>
        <Header title={translate('ReportOverviewTitle')} />
        <ErrorDisplay message={error} />
      </View>
    );
  }

  return (
    <View style={s.container}>
      <Header title={translate('ReportOverviewTitle')} />
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
          <Text style={[s.title, s.sectionTitle]}>{translate('TopChangingCategories')}</Text>
          {barData.length > 0 ? (
            <View style={[s.card, s.chartCard]}>
              <BarChart
                data={{labels: barLabels, datasets: [{data: barData}]}}
                width={chartWidth}
                height={220}
                fromZero
                yAxisLabel=""
                yAxisSuffix=""
                withHorizontalLabels={false}
                chartConfig={{
                  backgroundGradientFrom: colors.cardBackground,
                  backgroundGradientTo: colors.cardBackground,
                  color: () => colors.buttonPrimary,
                  labelColor: () => colors.textSecondary,
                }}
                style={{borderRadius: radius.md}}
              />
              <View style={s.chartSummary}>
                {topChanging.map(item => (
                  <View key={item.categoryId} style={s.chartSummaryRow}>
                    <Text style={s.chartSummaryLabel}>{item.categoryName}</Text>
                    <Text style={s.chartSummaryValue}>{formatCurrency(Math.abs(item.differenceAmount))}</Text>
                  </View>
                ))}
              </View>
            </View>
          ) : (
            <Text style={s.subtitle}>{translate('NoCategoryData')}</Text>
          )}
        </View>

        <View style={s.section}>
          <Text style={[s.title, s.sectionTitle]}>{translate('CategoryDistribution')}</Text>
          {pieChartData.length > 0 ? (
            <View style={[s.card, s.chartCard]}>
              <PieChartCard
                data={pieChartData}
                chartHeight={220}
              />
            </View>
          ) : (
            <Text style={s.subtitle}>{translate('NoCategoryData')}</Text>
          )}
        </View>

        <View style={s.section}>
          <Text style={[s.title, s.sectionTitle]}>{translate('AllCategories')}</Text>
          {categories.length === 0 ? (
            <Text style={s.subtitle}>{translate('NoCategoryData')}</Text>
          ) : (
            categories.map(item => (
              <TouchableOpacity
                key={item.categoryId}
                style={s.listItem}
                onPress={() =>
                  navigation.navigate('CategoryDetail', {
                    categoryId: item.categoryId,
                    categoryName: item.categoryName,
                  })
                }>
                <View style={s.listRow}>
                  <Text style={s.listTitle}>{item.categoryName}</Text>
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
