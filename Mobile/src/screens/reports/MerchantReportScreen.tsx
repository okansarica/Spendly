// CHANGED_BY_AI: 2026-03-13 - Add merchant report screen
// CHANGED_BY_AI: 2026-03-13 - Add sort by name/amount toggle
import React, {useEffect, useMemo, useState} from 'react';
import {View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator, Dimensions, TextInput} from 'react-native';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {loadMerchantsOverview} from '../../store/reportsStore';
import {useTheme} from '../../theme/ThemeContext';
import {translate} from '../../utils/translations';
import {formatCurrency} from '../../utils/formatCurrency';
import {useNavigation} from '@react-navigation/native';
import {NativeStackNavigationProp} from '@react-navigation/native-stack';
import {ReportsStackParamList} from '../../navigation/ReportsNavigator';
import Header from '../../components/Header';
import ErrorDisplay from '../../components/ErrorDisplay';
import PieChartCard from '../../components/PieChartCard';
import Icon from 'react-native-vector-icons/MaterialIcons';

const screenWidth = Dimensions.get('window').width;

type MerchantsNavProp = NativeStackNavigationProp<ReportsStackParamList, 'MerchantsOverview'>;

export default function MerchantReportScreen() {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const navigation = useNavigation<MerchantsNavProp>();
  const overview = useAppSelector(s => s.reports.merchantsOverview);
  const isLoading = useAppSelector(s => s.reports.isLoadingMerchantsOverview);
  const error = useAppSelector(s => s.reports.error);
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState<'name' | 'amount'>('name');

  useEffect(() => {
    if (!overview && !isLoading && !error) {
      dispatch(loadMerchantsOverview(undefined));
    }
  }, [overview, isLoading, error]);

  const chartWidth = useMemo(() => screenWidth - spacing.lg * 4, [spacing.lg]);

  const summary = overview?.summary;
  const merchants = overview?.merchants ?? [];
  const distribution = overview?.merchantDistribution ?? [];

  const filteredMerchants = useMemo(() => {
    let result = merchants;
    
    // Filter by search
    if (search.trim()) {
      const lowerSearch = search.toLowerCase();
      result = result.filter(m => m.merchantName.toLowerCase().includes(lowerSearch));
    }
    
    // Sort by selected criteria
    if (sortBy === 'name') {
      result = [...result].sort((a, b) => a.merchantName.localeCompare(b.merchantName));
    } else {
      result = [...result].sort((a, b) => b.currentMonthToDateTotal - a.currentMonthToDateTotal);
    }
    
    return result;
  }, [merchants, search, sortBy]);

  const pieChartData = distribution.map(item => ({
    label: item.merchantName,
    amount: item.currentMonthToDateTotal,
    percentage: item.percentageOfTotal,
  }));

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
    listRow: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start'},
    listTitle: {fontSize: fontSizes.md, color: colors.textPrimary, fontWeight: fontWeights.medium},
    listSub: {fontSize: fontSizes.sm, color: colors.textSecondary},
    categoryText: {
      fontSize: fontSizes.xs,
      color: colors.textSecondary,
      marginTop: spacing.xs / 2,
      fontStyle: 'italic',
    },
    searchRow: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: spacing.sm,
      marginBottom: spacing.md,
    },
    searchInput: {
      flex: 1,
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      paddingHorizontal: spacing.md,
      paddingVertical: spacing.sm,
      color: colors.textPrimary,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
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

  if (isLoading && !overview) {
    return (
      <View style={[s.container, {justifyContent: 'center', alignItems: 'center'}]}>
        <Header title={translate('MerchantReport')} />
        <ActivityIndicator color={colors.spinner} />
      </View>
    );
  }

  if (error && !overview) {
    return (
      <View style={s.container}>
        <Header title={translate('MerchantReport')} />
        <ErrorDisplay message={error} />
      </View>
    );
  }

  return (
    <View style={s.container}>
      <Header title={translate('MerchantReport')} />
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
          <Text style={[s.title, s.sectionTitle]}>{translate('TopMerchants')}</Text>
          {pieChartData.length > 0 ? (
            <View style={[s.card, s.chartCard]}>
              <PieChartCard data={pieChartData} chartHeight={220} />
            </View>
          ) : (
            <Text style={s.subtitle}>{translate('NoMerchantData')}</Text>
          )}
        </View>

        <View style={s.section}>
          <Text style={[s.title, s.sectionTitle]}>{translate('AllMerchants')}</Text>
          <View style={s.searchRow}>
            <TextInput
              style={s.searchInput}
              placeholder={translate('SearchMerchants')}
              placeholderTextColor={colors.textSecondary}
              value={search}
              onChangeText={setSearch}
            />
            <TouchableOpacity 
              style={s.iconButton} 
              onPress={() => setSortBy(sortBy === 'name' ? 'amount' : 'name')}>
              <Icon 
                name={sortBy === 'name' ? 'sort-by-alpha' : 'attach-money'} 
                size={fontSizes.lg} 
                color={colors.textSecondary} 
              />
            </TouchableOpacity>
          </View>
          {filteredMerchants.length === 0 ? (
            <Text style={s.subtitle}>{translate('NoMerchantData')}</Text>
          ) : (
            filteredMerchants.map(item => (
              <TouchableOpacity
                key={item.merchantId}
                style={s.listItem}
                onPress={() =>
                  navigation.navigate('MerchantDetail', {
                    merchantId: item.merchantId,
                    merchantName: item.merchantName,
                  })
                }>
                <View style={s.listRow}>
                  <View style={{flex: 1}}>
                    <Text style={s.listTitle}>{item.merchantName}</Text>
                      <Text style={s.categoryText}>{item.categoryName}</Text>
                  </View>
                  <Text style={s.value}>{formatCurrency(item.currentMonthToDateTotal)}</Text>
                </View>
                <View style={s.listRow}>
                  <Text style={s.listSub}>{translate('PreviousMonthSamePeriod')}</Text>
                  <Text style={s.listSub}>{formatCurrency(item.previousMonthSamePeriodTotal)}</Text>
                </View>
                <View style={s.listRow}>
                  <Text style={s.listSub}>{translate('PercentageChange')}</Text>
                  <Text
                    style={[
                      s.listSub,
                      {
                        color:
                          item.differenceAmount > 0
                            ? colors.danger
                            : item.differenceAmount < 0
                              ? colors.success
                              : colors.textSecondary,
                      },
                    ]}>
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
