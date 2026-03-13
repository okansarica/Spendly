import * as Keychain from 'react-native-keychain';
import AsyncStorage from '@react-native-async-storage/async-storage';

export const tokenService = {
  async saveTokens(
    accessToken: string,
    refreshToken: string,
    accessTokenExpire?: string,
    refreshTokenExpire?: string
  ): Promise<void> {
    console.log('💾 Saving tokens:', {
      accessTokenLength: accessToken?.length,
      refreshTokenLength: refreshToken?.length,
      hasExpire: !!accessTokenExpire,
    });
    
    await Keychain.setGenericPassword('token', accessToken, {service: 'accessToken'});
    await Keychain.setGenericPassword('token', refreshToken, {service: 'refreshToken'});
    
    if (accessTokenExpire) {
      await AsyncStorage.setItem('accessTokenExpire', accessTokenExpire);
    }
    if (refreshTokenExpire) {
      await AsyncStorage.setItem('refreshTokenExpire', refreshTokenExpire);
    }
    
    console.log('✅ Tokens saved successfully');
  },

  async getAccessToken(): Promise<string | undefined> {
    console.log('🔑 Getting access token from Keychain...');
    const creds = await Keychain.getGenericPassword({service: 'accessToken'});
    console.log('🔑 Keychain result:', {
      hasCreds: !!creds,
      username: creds ? creds.username : 'none',
      passwordLength: creds ? creds.password?.length : 0,
    });
    const result = creds ? creds.password : undefined;
    console.log('🔑 Returning token:', result ? `${result.substring(0, 20)}...` : 'undefined');
    return result;
  },

  async getRefreshToken(): Promise<string | null> {
    const creds = await Keychain.getGenericPassword({service: 'refreshToken'});
    return creds ? creds.password : null;
  },

  async getAccessTokenExpire(): Promise<string | null> {
    return await AsyncStorage.getItem('accessTokenExpire');
  },

  async getRefreshTokenExpire(): Promise<string | null> {
    return await AsyncStorage.getItem('refreshTokenExpire');
  },

  async clearTokens(): Promise<void> {
    await Keychain.resetGenericPassword({service: 'accessToken'});
    await Keychain.resetGenericPassword({service: 'refreshToken'});
    await AsyncStorage.removeItem('accessTokenExpire');
    await AsyncStorage.removeItem('refreshTokenExpire');
  },

  async hasAccessToken(): Promise<boolean> {
    const creds = await Keychain.getGenericPassword({service: 'accessToken'});
    return !!creds;
  },

  // Pending tokens for paid subscriptions waiting for payment completion
  async savePendingTokens(
    accessToken: string,
    refreshToken: string,
    accessTokenExpire?: string,
    refreshTokenExpire?: string
  ): Promise<void> {
    await Keychain.setGenericPassword('token', accessToken, {service: 'pendingAccessToken'});
    await Keychain.setGenericPassword('token', refreshToken, {service: 'pendingRefreshToken'});
    
    if (accessTokenExpire) {
      await AsyncStorage.setItem('pendingAccessTokenExpire', accessTokenExpire);
    }
    if (refreshTokenExpire) {
      await AsyncStorage.setItem('pendingRefreshTokenExpire', refreshTokenExpire);
    }
  },

  async getPendingAccessToken(): Promise<string | undefined> {
    const creds = await Keychain.getGenericPassword({service: 'pendingAccessToken'});
    return creds ? creds.password : undefined;
  },

  async getPendingRefreshToken(): Promise<string | null> {
    const creds = await Keychain.getGenericPassword({service: 'pendingRefreshToken'});
    return creds ? creds.password : null;
  },

  async activatePendingTokens(): Promise<void> {
    const accessToken = await this.getPendingAccessToken();
    const refreshToken = await this.getPendingRefreshToken();
    const accessTokenExpire = await AsyncStorage.getItem('pendingAccessTokenExpire');
    const refreshTokenExpire = await AsyncStorage.getItem('pendingRefreshTokenExpire');

    if (accessToken && refreshToken) {
      await this.saveTokens(accessToken, refreshToken, accessTokenExpire || undefined, refreshTokenExpire || undefined);
      await this.clearPendingTokens();
    }
  },

  async clearPendingTokens(): Promise<void> {
    await Keychain.resetGenericPassword({service: 'pendingAccessToken'});
    await Keychain.resetGenericPassword({service: 'pendingRefreshToken'});
    await AsyncStorage.removeItem('pendingAccessTokenExpire');
    await AsyncStorage.removeItem('pendingRefreshTokenExpire');
  },

  async hasPendingTokens(): Promise<boolean> {
    const creds = await Keychain.getGenericPassword({service: 'pendingAccessToken'});
    return !!creds;
  },
};

