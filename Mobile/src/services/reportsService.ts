// CHANGED_BY_AI: 2026-03-02 - Add reports service
import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

export type ReportSummary = {
  currentMonthToDateTotal: number;
  previousMonthSamePeriodTotal: number;
  differenceAmount: number;
  percentageChange: number;
  trend: string;
  isNewSpending: boolean;
};

export type ReportCategoryChange = {
  categoryId: string;
  categoryName: string;
  currentMonthToDateTotal: number;
  previousMonthSamePeriodTotal: number;
  differenceAmount: number;
  percentageChange: number;
};

export type ReportCategoryDistribution = {
  categoryId: string;
  categoryName: string;
  currentMonthToDateTotal: number;
  percentageOfTotal: number;
};

export type ReportsOverviewResponse = {
  summary: ReportSummary;
  topChangingCategories: ReportCategoryChange[];
  categoryDistribution: ReportCategoryDistribution[];
  categories: ReportCategoryChange[];
};

export type ReportCategoryDetailResponse = {
  categorySummary: {
    categoryId: string;
    categoryName: string;
    totalAmount: number;
  };
  transactions: {
    items: {
      transactionId: string;
      transactionName: string;
      date: string;
      merchantName: string;
      accountName: string;
      amount: number;
      
    }[];
    total: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  };
};

export type AccountsOverviewResponse = {
  summary: ReportSummary;
  accountDistribution: {
    accountId: string;
    accountName: string;
    currentMonthToDateTotal: number;
    percentageOfTotal: number;
  }[];
  accounts: {
    accountId: string;
    accountName: string;
    currentMonthToDateTotal: number;
    previousMonthSamePeriodTotal: number;
    differenceAmount: number;
    percentageChange: number;
  }[];
};

export type AccountDetailResponse = {
  accountSummary: {
    accountId: string;
    accountName: string;
    startDate: string;
    endDate: string;
    totalAmount: number;
    comparison: {
      previousMonthSamePeriodTotal: number;
      differenceAmount: number;
      percentageChange: number;
      trend: string;
      isNewSpending: boolean;
    };
  };
  categories: {
    categoryId: string;
    categoryName: string;
    totalAmount: number;
  }[];
};

export type ReportsOverviewParams = {
  startDate?: string;
  endDate?: string;
};

export type ReportsCategoryParams = {
  startDate?: string;
  endDate?: string;
  accountId?: string;
  page?: number;
  pageSize?: number;
};

export type AccountsOverviewParams = {
  startDate?: string;
  endDate?: string;
};

export type AccountDetailParams = {
  startDate?: string;
  endDate?: string;
};

export const reportsService = {
  getOverview: (params?: ReportsOverviewParams) => apiClient.get<ReportsOverviewResponse>(ApiEndpoints.Reports.Overview, {params}),
  getCategoryDetail: (categoryId: string, params?: ReportsCategoryParams) =>
    apiClient.get<ReportCategoryDetailResponse>(ApiEndpoints.Reports.CategoryDetail(categoryId), {params}),
  getAccountsOverview: (params?: AccountsOverviewParams) =>
    apiClient.get<AccountsOverviewResponse>(ApiEndpoints.Reports.AccountsOverview, {params}),
  getAccountDetail: (accountId: string, params?: AccountDetailParams) =>
    apiClient.get<AccountDetailResponse>(ApiEndpoints.Reports.AccountDetail(accountId), {params}),
};
