// CHANGED_BY_AI: 2026-03-02 - Add categories service
import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

export type CategoryListItem = {
  id: string;
  name: string;
  parentId?: string;
  color?: string;
  icon?: string;
  merchantCount: number;
};

export type CategoryMerchantItem = {
  id: string;
  name: string;
  transactionCount: number;
  lastTransactionDate?: string;
};

export type CategoryListRequest = {
  search?: string;
  sortBy?: string;
  sortDirection?: string;
};

export type CategoryUpsertRequest = {
  name?: string;
  parentId?: string;
  color?: string;
  icon?: string;
  merchantIds?: string[];
};

export type CategoryMerchantsRequest = {
  merchantIds?: string[];
};

export const categoriesService = {
  getList: (params?: CategoryListRequest) => apiClient.get<CategoryListItem[]>(ApiEndpoints.Categories.Base, {params}),
  create: (payload: CategoryUpsertRequest) => apiClient.post<CategoryListItem>(ApiEndpoints.Categories.Base, payload),
  update: (id: string, payload: CategoryUpsertRequest) => apiClient.put<CategoryListItem>(ApiEndpoints.Categories.ById(id), payload),
  remove: (id: string) => apiClient.delete<void>(ApiEndpoints.Categories.ById(id)),
  getMerchants: (id: string) => apiClient.get<CategoryMerchantItem[]>(ApiEndpoints.Categories.Merchants(id)),
};

