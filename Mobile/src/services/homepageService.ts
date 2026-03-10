// CHANGED_BY_AI: 2026-03-02 - Align homepage response types
import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

export type SpendingByAccount = {
  accountId: string;
  accountName: string;
  amount: number;
  percentageOfTotal: number;
};

export type SpendingByCategory = {
  categoryId: string;
  categoryName: string;
  amount: number;
  percentageOfTotal: number;
};

export type MonthlySpendingTrend = {
  year: number;
  month: number;
  amount: number;
  previousMonthAmount: number;
  percentageChange: number;
};

export type LatestExpense = {
  transactionId: string;
  date: string;
  amount: number;
  categoryName: string;
  merchantName: string;
  accountName: string;
  transactionName: string;
};

export type HomepageResponse = {
  currentMonthTotalSpending: number;
  previousMonthTotalSpending: number;
  midMonthComparison: {
    isIncreased: boolean;
    percentageChange: number;
  };
  spendingByAccountCurrentMonth: SpendingByAccount[];
  spendingByAccountPreviousMonth: SpendingByAccount[];
  spendingByCategoryCurrentMonth: SpendingByCategory[];
  spendingByCategoryPreviousMonth: SpendingByCategory[];
  sixMonthTrend: MonthlySpendingTrend[];
  latestExpenses: LatestExpense[];
  weeklySnapshot: {
    thisWeekTotal: number;
    previousWeekTotal: number;
    percentageChange: number;
    isIncreased: boolean;
  };
  topSpendingCategory: {
    categoryName: string;
    amount: number;
    percentageOfTotal: number;
  };
  highestSingleExpense: {
    merchantName: string;
    amount: number;
    date: string;
  };
  mostUsedAccount: {
    accountName: string;
    percentageShare: number;
  };
  dailyAverage: {
    currentMonthAverage: number;
    previousMonthAverage: number;
    percentageChange: number;
    isIncreased: boolean;
  };
};

export const homepageService = {
  getHomepage: () => apiClient.get<HomepageResponse>(ApiEndpoints.Homepage.Get),
};
