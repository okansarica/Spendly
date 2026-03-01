import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

export type SpendingByAccount = {
  accountId: string;
  accountName: string;
  amount: number;
};

export type SpendingByCategory = {
  categoryId: string;
  categoryName: string;
  amount: number;
};

export type MonthlySpendingTrend = {
  year: number;
  month: number;
  amount: number;
};

export type LatestExpense = {
  transactionId: string;
  date: string;
  amount: number;
  categoryName: string;
  merchantName: string;
  accountName: string;
};

export type HomepageResponse = {
  previousMonthTotalSpending: number;
  spendingByAccount: SpendingByAccount[];
  spendingByCategory: SpendingByCategory[];
  sixMonthTrend: MonthlySpendingTrend[];
  latestExpenses: LatestExpense[];
};

export const homepageService = {
  getHomepage: () => apiClient.get<HomepageResponse>(ApiEndpoints.Homepage.Get),
};

