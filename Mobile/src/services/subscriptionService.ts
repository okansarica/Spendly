import AsyncStorage from '@react-native-async-storage/async-storage';

export const subscriptionService = {
  async saveSubscriptionEndDate(endDateTime: string): Promise<void> {
    await AsyncStorage.setItem('subscriptionEndDateTime', endDateTime);
  },

  async getSubscriptionEndDate(): Promise<string | null> {
    return await AsyncStorage.getItem('subscriptionEndDateTime');
  },

  async clearSubscriptionEndDate(): Promise<void> {
    await AsyncStorage.removeItem('subscriptionEndDateTime');
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

