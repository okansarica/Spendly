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
};

