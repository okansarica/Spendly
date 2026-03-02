import axios, {AxiosInstance, InternalAxiosRequestConfig, AxiosError} from 'axios';
import sessionService from './sessionService';
import {tokenService} from './tokenService';

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

    console.log('🔄 Interceptor: Checking tokens...');
    const accessTokenExpire = await tokenService.getAccessTokenExpire();
    console.log('🔄 Interceptor: accessTokenExpire =', accessTokenExpire);
    
    if (accessTokenExpire && new Date(accessTokenExpire) >= new Date()) {
      console.log('🔄 Interceptor: Token not expired, getting access token...');
      const accessToken = await tokenService.getAccessToken();
      console.log('🔄 Interceptor: accessToken =', accessToken ? `${accessToken.substring(0, 20)}...` : 'undefined');
      if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`;
      }
      return config;
    }
    
    console.log('🔄 Interceptor: Token expired or not found, checking refresh token...');

    const refreshTokenExpire = await tokenService.getRefreshTokenExpire();
    if (!refreshTokenExpire || new Date(refreshTokenExpire) < new Date()) {
      await tokenService.clearTokens();
      return Promise.reject(new Error('Session expired'));
    }

    if (this.isRefreshing) {
      await this.waitForRefresh();
      const accessToken = await tokenService.getAccessToken();
      if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`;
      }
      return config;
    }

    await this.refreshToken();
    const accessToken = await tokenService.getAccessToken();
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  }

  private async handleResponseError(error: AxiosError): Promise<never> {
    const originalRequest = error.config as InternalAxiosRequestConfig & {_retry?: boolean};

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      if (this.isRefreshing) {
        await this.waitForRefresh();
        const accessToken = await tokenService.getAccessToken();
        if (accessToken) {
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        }
        return this.axiosInstance(originalRequest);
      }

      try {
        await this.refreshToken();
        const accessToken = await tokenService.getAccessToken();
        if (accessToken) {
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        }
        return this.axiosInstance(originalRequest);
      } catch (refreshError) {
        await tokenService.clearTokens();
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }

  private async refreshToken(): Promise<void> {
    this.isRefreshing = true;

    try {
      const sessionId = await sessionService.getSessionId();
      const refreshToken = await tokenService.getRefreshToken();
      
      if (!refreshToken) {
        throw new Error('No refresh token available');
      }

      const response = await this.rawAxios.post<{
        accessToken: string;
        accessTokenExpire: string;
        refreshToken: string;
        refreshTokenExpire: string;
      }>('/api/v1/auth/refresh-access-token', 
        {refreshToken},
        {
          headers: {
            'X-Disable-Auth': 'true',
            'X-Session-Id': sessionId
          },
        }
      );

      const {accessToken, accessTokenExpire, refreshToken: newRefreshToken, refreshTokenExpire} = response.data;

      await tokenService.saveTokens(accessToken, newRefreshToken, accessTokenExpire, refreshTokenExpire);

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



