import axios, {AxiosInstance, InternalAxiosRequestConfig, AxiosError} from 'axios';
import * as Keychain from 'react-native-keychain';
import AsyncStorage from '@react-native-async-storage/async-storage';
import sessionService from './sessionService';

class TokenInterceptor {
  private isRefreshing = false;
  private refreshSubscribers: Array<() => void> = [];
  private rawAxios: AxiosInstance;

  constructor(private axiosInstance: AxiosInstance, baseURL: string) {
    this.rawAxios = axios.create({
      baseURL: baseURL,
      timeout: 30000,
    });
    this.setupInterceptors();
  }

  private setupInterceptors() {
    this.axiosInstance.interceptors.request.use(
      async (config) => this.handleRequest(config),
      (error) => Promise.reject(error)
    );

    this.axiosInstance.interceptors.response.use(
      (response) => response,
      async (error) => this.handleResponseError(error)
    );
  }

  private async handleRequest(config: InternalAxiosRequestConfig): Promise<InternalAxiosRequestConfig> {
    const sessionId = await sessionService.getSessionId();
    config.headers['X-Session-Id'] = sessionId;

    if (config.url?.includes('assets/')) {
      return config;
    }

    if (config.url?.includes('/refresh-access-token')) {
      return config;
    }

    if (config.headers?.['X-Disable-Auth'] === 'true') {
      return config;
    }

    const accessTokenExpire = await AsyncStorage.getItem('accessTokenExpire');
    if (accessTokenExpire && new Date(accessTokenExpire) >= new Date()) {
      const creds = await Keychain.getGenericPassword({service: 'accessToken'});
      if (creds) {
        config.headers.Authorization = `Bearer ${creds.password}`;
      }
      return config;
    }

    const refreshTokenExpire = await AsyncStorage.getItem('refreshTokenExpire');
    if (!refreshTokenExpire || new Date(refreshTokenExpire) < new Date()) {
      await AsyncStorage.setItem('isAuthenticated', 'false');
      await Keychain.resetGenericPassword({service: 'accessToken'});
      await Keychain.resetGenericPassword({service: 'refreshToken'});
      return Promise.reject(new Error('Session expired'));
    }

    if (this.isRefreshing) {
      await this.waitForRefresh();
      const creds = await Keychain.getGenericPassword({service: 'accessToken'});
      if (creds) {
        config.headers.Authorization = `Bearer ${creds.password}`;
      }
      return config;
    }

    await this.refreshToken();
    const creds = await Keychain.getGenericPassword({service: 'accessToken'});
    if (creds) {
      config.headers.Authorization = `Bearer ${creds.password}`;
    }
    return config;
  }

  private async handleResponseError(error: AxiosError): Promise<never> {
    const originalRequest = error.config as InternalAxiosRequestConfig & {_retry?: boolean};

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      if (this.isRefreshing) {
        await this.waitForRefresh();
        const creds = await Keychain.getGenericPassword({service: 'accessToken'});
        if (creds) {
          originalRequest.headers.Authorization = `Bearer ${creds.password}`;
        }
        return this.axiosInstance(originalRequest);
      }

      try {
        await this.refreshToken();
        const creds = await Keychain.getGenericPassword({service: 'accessToken'});
        if (creds) {
          originalRequest.headers.Authorization = `Bearer ${creds.password}`;
        }
        return this.axiosInstance(originalRequest);
      } catch (refreshError) {
        await AsyncStorage.setItem('isAuthenticated', 'false');
        await Keychain.resetGenericPassword({service: 'accessToken'});
        await Keychain.resetGenericPassword({service: 'refreshToken'});
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }

  private async refreshToken(): Promise<void> {
    this.isRefreshing = true;

    try {
      const sessionId = await sessionService.getSessionId();
      const response = await this.rawAxios.get<{
        accessToken: {token: string; expireDateTime: string};
        refreshToken: {token: string; expireDateTime: string};
      }>('/auth/refresh-access-token', {
        headers: {
          'X-Disable-Auth': 'true',
          'X-Session-Id': sessionId
        },
      });

      const {accessToken, refreshToken} = response.data;

      await Keychain.setGenericPassword('accessToken', accessToken.token, {service: 'accessToken'});
      await Keychain.setGenericPassword('refreshToken', refreshToken.token, {service: 'refreshToken'});
      await AsyncStorage.setItem('accessTokenExpire', accessToken.expireDateTime);
      await AsyncStorage.setItem('refreshTokenExpire', refreshToken.expireDateTime);

      this.onRefreshComplete();
    } catch (error) {
      this.isRefreshing = false;
      throw error;
    } finally {
      this.isRefreshing = false;
    }
  }

  private waitForRefresh(): Promise<void> {
    return new Promise((resolve) => {
      this.refreshSubscribers.push(resolve);
    });
  }

  private onRefreshComplete() {
    this.refreshSubscribers.forEach((callback) => callback());
    this.refreshSubscribers = [];
  }
}

export default TokenInterceptor;



