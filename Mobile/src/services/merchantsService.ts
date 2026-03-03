// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname updates
// CHANGED_BY_AI: 2026-03-02 - Remove transaction fields from merchant models
// CHANGED_BY_AI: 2026-03-02 - Add merchant delete API
// CHANGED_BY_AI: 2026-03-02 - Add merchant update API
import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

export type MerchantListItem = {
  id: string;
  name: string;
  nickname?: string;
  categoryId?: string;
  categoryName: string;
};

export type MerchantDetail = {
  id: string;
  name: string;
  nickname?: string;
  categoryId?: string;
  categoryName?: string;
};

export type MerchantListRequest = {
  search?: string;
  isUncategorized?: boolean;
  sortBy?: string;
  sortDirection?: string;
  startDate?: string;
  endDate?: string;
};

export type MerchantUpdateRequest = {
  categoryId?: string;
  nickname?: string;
};

export const merchantsService = {
  getList: (params?: MerchantListRequest) => apiClient.get<MerchantListItem[]>(ApiEndpoints.Merchants.Base, {params}),  
  update: (id: string, payload: MerchantUpdateRequest) => apiClient.put<MerchantDetail>(ApiEndpoints.Merchants.ById(id), payload),
  remove: (id: string) => apiClient.delete(ApiEndpoints.Merchants.ById(id)),
};
