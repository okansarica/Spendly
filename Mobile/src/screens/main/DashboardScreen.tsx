import React, {useEffect, useState} from 'react';
import {View, Text, StyleSheet, ScrollView, ActivityIndicator} from 'react-native';
import {useTheme} from '../../theme/ThemeContext';
import {homepageService, HomepageResponse} from '../../services/homepageService';
import {apiCall} from '../../services/apiClient';

export default function DashboardScreen() {
  const {colors, fontSizes, fontWeights} = useTheme();
  const [loading, setLoading] = useState(true);
  const [data, setData] = useState<HomepageResponse | null>(null);
  const [showAccountView, setShowAccountView] = useState(true);

  useEffect(() => {
    loadHomepageData();
  }, []);

  const loadHomepageData = async () => {
    setLoading(true);
    const response = await apiCall(() => homepageService.getHomepage());
    if (response.isSuccess && response.data) {
      setData(response.data);
    }
    setLoading(false);
  };

  if (loading) {
    return (
      <View style={[styles.container, {backgroundColor: colors.backgroundSecondary}]}>
        <ActivityIndicator size="large" color={colors.spinner} />
      </View>
    );
  }

  if (!data) {
    return (
      <View style={[styles.container, {backgroundColor: colors.backgroundSecondary}]}>
        <Text style={{color: colors.textPrimary}}>No data available</Text>
      </View>
    );
  }

  return (
    <ScrollView style={[styles.scrollView, {backgroundColor: colors.backgroundSecondary}]}>
      <View style={styles.content}>
        <Text style={[styles.title, {color: colors.textPrimary, fontSize: fontSizes.xxl, fontWeight: fontWeights.bold}]}>
          Dashboard
        </Text>

        <View style={[styles.card, {backgroundColor: colors.backgroundPrimary}]}>
          <Text style={[styles.cardLabel, {color: colors.textSecondary, fontSize: fontSizes.sm}]}>
            Previous Month Total
          </Text>
          <Text style={[styles.cardValue, {color: colors.buttonPrimary, fontSize: 32, fontWeight: fontWeights.bold}]}>
            £{data.previousMonthTotalSpending.toFixed(2)}
          </Text>
        </View>

        <View style={[styles.card, {backgroundColor: colors.backgroundPrimary}]}>
          <View style={styles.cardHeader}>
            <Text style={[styles.cardTitle, {color: colors.textPrimary, fontSize: fontSizes.lg, fontWeight: fontWeights.semiBold}]}>
              Spending By {showAccountView ? 'Account' : 'Category'}
            </Text>
            <Text
              style={[styles.toggle, {color: colors.buttonPrimary, fontSize: fontSizes.sm}]}
              onPress={() => setShowAccountView(!showAccountView)}>
              Switch
            </Text>
          </View>
          {showAccountView
            ? data.spendingByAccount.map(item => (
                <View key={item.accountId} style={styles.listItem}>
                  <Text style={[styles.listItemLabel, {color: colors.textPrimary, fontSize: fontSizes.md}]}>
                    {item.accountName}
                  </Text>
                  <Text style={[styles.listItemValue, {color: colors.textPrimary, fontSize: fontSizes.md, fontWeight: fontWeights.semiBold}]}>
                    £{item.amount.toFixed(2)}
                  </Text>
                </View>
              ))
            : data.spendingByCategory.map(item => (
                <View key={item.categoryId} style={styles.listItem}>
                  <Text style={[styles.listItemLabel, {color: colors.textPrimary, fontSize: fontSizes.md}]}>
                    {item.categoryName}
                  </Text>
                  <Text style={[styles.listItemValue, {color: colors.textPrimary, fontSize: fontSizes.md, fontWeight: fontWeights.semiBold}]}>
                    £{item.amount.toFixed(2)}
                  </Text>
                </View>
              ))}
        </View>

        <View style={[styles.card, {backgroundColor: colors.backgroundPrimary}]}>
          <Text style={[styles.cardTitle, {color: colors.textPrimary, fontSize: fontSizes.lg, fontWeight: fontWeights.semiBold}]}>
            6-Month Spending Trend
          </Text>
          {data.sixMonthTrend.map((item, index) => (
            <View key={index} style={styles.listItem}>
              <Text style={[styles.listItemLabel, {color: colors.textPrimary, fontSize: fontSizes.md}]}>
                {new Date(item.year, item.month - 1).toLocaleDateString('en-US', {
                  month: 'short',
                  year: 'numeric',
                })}
              </Text>
              <Text style={[styles.listItemValue, {color: colors.textPrimary, fontSize: fontSizes.md, fontWeight: fontWeights.semiBold}]}>
                £{item.amount.toFixed(2)}
              </Text>
            </View>
          ))}
        </View>

        <View style={[styles.card, {backgroundColor: colors.backgroundPrimary}]}>
          <Text style={[styles.cardTitle, {color: colors.textPrimary, fontSize: fontSizes.lg, fontWeight: fontWeights.semiBold}]}>
            Latest 10 Expenses
          </Text>
          {data.latestExpenses.map(item => (
            <View key={item.transactionId} style={styles.expenseItem}>
              <View style={styles.expenseLeft}>
                <Text style={[styles.expenseMerchant, {color: colors.textPrimary, fontSize: fontSizes.md, fontWeight: fontWeights.medium}]}>
                  {item.merchantName}
                </Text>
                <Text style={[styles.expenseDetails, {color: colors.textSecondary, fontSize: fontSizes.sm}]}>
                  {item.categoryName} • {item.accountName}
                </Text>
                <Text style={[styles.expenseDate, {color: colors.textSecondary, fontSize: fontSizes.xs}]}>
                  {new Date(item.date).toLocaleDateString('en-US', {
                    month: 'short',
                    day: 'numeric',
                    year: 'numeric',
                  })}
                </Text>
              </View>
              <Text style={[styles.expenseAmount, {color: colors.textPrimary, fontSize: fontSizes.lg, fontWeight: fontWeights.semiBold}]}>
                £{item.amount.toFixed(2)}
              </Text>
            </View>
          ))}
        </View>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {flex: 1, alignItems: 'center', justifyContent: 'center'},
  scrollView: {flex: 1},
  content: {},
  title: {},
  card: {shadowColor: '#000', shadowOffset: {width: 0, height: 2}, shadowOpacity: 0.1, shadowRadius: 4, elevation: 3},
  cardLabel: {},
  cardValue: {},
  cardHeader: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center'},
  cardTitle: {},
  toggle: {},
  listItem: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', paddingVertical: 8, borderBottomWidth: 1},
  listItemLabel: {flex: 1},
  listItemValue: {},
  expenseItem: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', paddingVertical: 12, borderBottomWidth: 1},
  expenseLeft: {flex: 1, marginRight: 12},
  expenseMerchant: {},
  expenseDetails: {},
  expenseDate: {},
  expenseAmount: {},
});

