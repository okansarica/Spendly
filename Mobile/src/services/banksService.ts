// CHANGED_BY_AI: 2026-03-06 - Add banks and accounts API service
import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

export type BankAccountItem = {
  id: string;
  name: string;
  description?: string;
  isConnected: boolean;
};

export type BankListItem = {
  id: string;
  name: string;
  description?: string;
  bankDefinitionId?: string;
  isConnected: boolean;
  accounts: BankAccountItem[];
};

export type BankDefinitionItem = {
  id: string;
  name: string;
  logoName?: string;
};

export type BankUpsertRequest = {
  name?: string;
  bankDefinitionId?: string;
  description?: string;
};

export type BankAccountUpsertRequest = {
  name?: string;
};

export const banksService = {
  getList: () => apiClient.get<BankListItem[]>(ApiEndpoints.Banks.Base),
  getDefinitions: () => apiClient.get<BankDefinitionItem[]>(ApiEndpoints.Banks.BankDefinitions),
  getBankDefinitions: () => apiClient.get<BankDefinitionItem[]>(ApiEndpoints.Banks.BankDefinitions),
  create: (payload: BankUpsertRequest) => apiClient.post<BankListItem>(ApiEndpoints.Banks.Base, payload),
  update: (id: string, payload: BankUpsertRequest) => apiClient.put<BankListItem>(ApiEndpoints.Banks.ById(id), payload),
  remove: (id: string) => apiClient.delete(ApiEndpoints.Banks.ById(id)),
  createAccount: (bankId: string, payload: BankAccountUpsertRequest) =>
    apiClient.post<BankAccountItem>(ApiEndpoints.Banks.Accounts(bankId), payload),
  updateAccount: (bankId: string, id: string, payload: BankAccountUpsertRequest) =>
    apiClient.put<BankAccountItem>(ApiEndpoints.Banks.AccountById(bankId, id), payload),
  removeAccount: (bankId: string, id: string) => apiClient.delete(ApiEndpoints.Banks.AccountById(bankId, id)),
};
