import axios, {AxiosResponse} from 'axios';
import {Platform} from 'react-native';
import TokenInterceptor from './tokenInterceptor';

const BASE_URL = Platform.OS === 'ios' ? 'http://localhost:5001' : 'http://10.0.2.2:5001';

const apiClient = axios.create({
  baseURL: BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
});

new TokenInterceptor(apiClient, BASE_URL);

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
    console.error(error);
    if (error.response?.status === 400) {
      const msg =
        error.response.data?.message ??
        error.response.data?.errors?.[0]?.message ??
        'an error occurred'; //TODO hem backendden gelen mesaj hem de genel hata mesaji localize olmali
      return {isSuccess: false, data: undefined, errorMessage: msg};
    }
    return {isSuccess: false, data: undefined, errorMessage: 'An unexpected error occurred'}; //TODO hata mesaji localize olmali
  }
}

export default apiClient;
