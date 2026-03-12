// CHANGED_BY_AI: 2026-03-05 - Add subscription state listeners for payment push updates
// CHANGED_BY_AI: 2026-03-12 - Move fetchSubscriptionPlans and createPaymentUrl to subscriptionStore
import AsyncStorage from '@react-native-async-storage/async-storage';
import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

type SubscriptionStateListener = () => void;

export type SubscriptionPaymentResultStatus = 'success' | 'fail';

const subscriptionStateListeners = new Set<SubscriptionStateListener>();

const notifySubscriptionStateListeners = () => {
  subscriptionStateListeners.forEach(listener => listener());
};

export const subscriptionService = {
  subscribeToSubscriptionState(listener: SubscriptionStateListener): () => void {
    subscriptionStateListeners.add(listener);
    return () => {
      subscriptionStateListeners.delete(listener);
    };
  },

  async fetchSubscriptionEndDate(): Promise<void> {
    const response = await apiClient.get<{subscriptionEndDateTime: string | null}>(
      ApiEndpoints.Users.SubscriptionEnd,
    );
    if (response.data.subscriptionEndDateTime) {
      await this.saveSubscriptionEndDate(response.data.subscriptionEndDateTime);
      return;
    }
    await this.clearSubscriptionEndDate();
  },

  async saveSubscriptionEndDate(endDateTime: string): Promise<void> {
    await AsyncStorage.setItem('subscriptionEndDateTime', endDateTime);
    notifySubscriptionStateListeners();
  },

  async getSubscriptionEndDate(): Promise<string | null> {
    return await AsyncStorage.getItem('subscriptionEndDateTime');
  },

  async clearSubscriptionEndDate(): Promise<void> {
    await AsyncStorage.removeItem('subscriptionEndDateTime');
    notifySubscriptionStateListeners();
  },

  async isSubscriptionExpired(): Promise<boolean> {
    const endDateTime = await this.getSubscriptionEndDate();
    if (!endDateTime) {
      return false;
    }
    return new Date(endDateTime) < new Date();
  },

  async isSubscriptionExpiring(): Promise<boolean> {
    const endDateTime = await this.getSubscriptionEndDate();
    if (!endDateTime) {
      return false;
    }
    const endDate = new Date(endDateTime);
    const now = new Date();
    const diffDays = Math.round((endDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
    return diffDays > 0 && diffDays <= 7;
  },

  async getDaysUntilExpiration(): Promise<number | null> {
    const endDateTime = await this.getSubscriptionEndDate();
    if (!endDateTime) {
      return null;
    }
    const endDate = new Date(endDateTime);
    const now = new Date();
    return Math.ceil((endDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
  },

  async getTimeUntilExpiration(): Promise<{days: number; hours: number} | null> {
    const endDateTime = await this.getSubscriptionEndDate();
    if (!endDateTime) {
      return null;
    }
    const endDate = new Date(endDateTime);
    const now = new Date();
    const diffMs = endDate.getTime() - now.getTime();
    const days = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    return {days, hours};
  },
};
