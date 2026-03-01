import axios, {AxiosResponse} from 'axios';
import * as Keychain from 'react-native-keychain';

const BASE_URL = 'http://10.0.2.2:5000';

const apiClient = axios.create({
  baseURL: BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
});

apiClient.interceptors.request.use(async config => {
  const creds = await Keychain.getGenericPassword({service: 'accessToken'});
  if (creds) {
    config.headers.Authorization = `Bearer ${creds.password}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  res => res,
  async error => {
    if (error.response?.status === 401) {
      await Keychain.resetGenericPassword({service: 'accessToken'});
      await Keychain.resetGenericPassword({service: 'refreshToken'});
    }
    return Promise.reject(error);
  },
);

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
