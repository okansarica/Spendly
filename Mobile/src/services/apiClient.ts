// CHANGED_BY_AI: 2026-03-02 - Add timezone header to api client
import axios, {AxiosResponse} from 'axios';
import TokenInterceptor from './tokenInterceptor';
import {translate} from '../utils/translations';
import {APP_CONFIG} from '../config/appConfig';

const TIMEZONE = Intl.DateTimeFormat().resolvedOptions().timeZone;

const apiClient = axios.create({
  baseURL: APP_CONFIG.apiBaseUrl,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
    'X-Timezone': TIMEZONE,
  },
});

new TokenInterceptor(apiClient, APP_CONFIG.apiBaseUrl);

export type ApiResponse<T = undefined> = {
  isSuccess: boolean;
  data: T | undefined;
  errorMessage: string | undefined;
};

export async function apiCall<T>(request: () => Promise<AxiosResponse<T>>): Promise<ApiResponse<T>> {
  try {
    const response = await request();
    return {isSuccess: true, data: response.data, errorMessage: undefined};
  } catch (error: any) {
    console.log(error);
    if (error.response?.status === 400) { //TODO translation yapilacak
      const key =
        error.response.data?.message ??
        error.response.data?.errors?.[0]?.message ??
        'an error occurred';
      return {isSuccess: false, data: undefined, errorMessage: translate(key)};
    }
    return {isSuccess: false, data: undefined, errorMessage: translate('An unexpected error occurred')}; //TODO translation yapilacak
  }
}

export default apiClient;
