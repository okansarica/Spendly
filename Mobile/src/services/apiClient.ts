import axios from 'axios';
import * as Keychain from 'react-native-keychain';

const BASE_URL = 'http://10.0.2.2:5000/api/v1';

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

export default apiClient;

