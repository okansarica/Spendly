// CHANGED_BY_AI: 2026-03-02 - Add merchants service
import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

export type MerchantListItem = {
  id: string;
  name: string;
  categoryId?: string;
  categoryName: string;
  transactionCount: number;
  totalAmount: number;
  lastTransactionDate?: string;
};

export type MerchantDetail = {
  id: string;
  name: string;
  categoryId?: string;
  categoryName: string;
  transactionCount: number;
  totalAmount: number;
};

export type MerchantListRequest = {
  search?: string;
  isUncategorized?: boolean;
  sortBy?: string;
  sortDirection?: string;
  startDate?: string;
  endDate?: string;
};

export type MerchantCategoryUpdateRequest = {
  categoryId?: string;
};

export const merchantsService = {
  getList: (params?: MerchantListRequest) => apiClient.get<MerchantListItem[]>(ApiEndpoints.Merchants.Base, {params}),
  getDetail: (id: string, params?: MerchantListRequest) => apiClient.get<MerchantDetail>(ApiEndpoints.Merchants.ById(id), {params}),
  updateCategory: (id: string, payload: MerchantCategoryUpdateRequest) => apiClient.put<MerchantDetail>(ApiEndpoints.Merchants.Category(id), payload),
};

