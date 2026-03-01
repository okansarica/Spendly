import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

export type LoginRequest = {
  email: string;
  password: string;
};

export type SocialLoginRequest = {
  provider: 'google' | 'facebook'; //TODO enum yapilmali. enumlar ayri bir dosyada tutulmali
  token: string;
};

export type ForgotPasswordRequest = {
  email: string;
};

export type RegisterRequest = {
  name: string;
  surname: string;
  email: string;
  password: string;
};

export type VerifyEmailRequest = {
  userId: string;
  code: string;
};

export type AuthResponse = {
  id: string;
  email: string;
  accessToken?: string;
  accessTokenExpire?: string;
  refreshToken?: string;
  refreshTokenExpire?: string;
  emailVerificationRequired: boolean;
};

export const authService = {
  login: (data: LoginRequest) =>
    apiClient.post<AuthResponse>(ApiEndpoints.Auth.Login, data, {
      headers: {'X-Disable-Auth': 'true'},
    }),

  socialLogin: (data: SocialLoginRequest) =>
    apiClient.post<AuthResponse>(ApiEndpoints.Auth.SocialLogin, data, {
      headers: {'X-Disable-Auth': 'true'},
    }),

  forgotPassword: (data: ForgotPasswordRequest) =>
    apiClient.post(ApiEndpoints.Auth.ForgotPassword, data, {
      headers: {'X-Disable-Auth': 'true'},
    }),

  register: (data: RegisterRequest) =>
    apiClient.post<AuthResponse>(ApiEndpoints.Auth.Register, data, {
      headers: {'X-Disable-Auth': 'true'},
    }),

  verifyEmail: (data: VerifyEmailRequest) =>
    apiClient.post<AuthResponse>(ApiEndpoints.Auth.VerifyEmail, data, {
      headers: {'X-Disable-Auth': 'true'},
    }),

  resendCode: (userId: string) =>
    apiClient.post(ApiEndpoints.Auth.ResendCode, {userId}, {
      headers: {'X-Disable-Auth': 'true'},
    }),
};
