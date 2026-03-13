// CHANGED_BY_AI: 2026-03-12 - Add registration plan selection and payment redirect response fields
// CHANGED_BY_AI: 2026-03-03 - Add logout API call
// CHANGED_BY_AI: 2026-03-05 - Add Firebase token to auth requests
import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

export type RegisterPlanType = 'Trial' | 'Monthly' | 'Yearly';

export type LoginRequest = {
  email: string;
  password: string;
  firebaseToken?: string;
};

export type SocialLoginRequest = {
  provider: 'google' | 'facebook';
  token: string;
  firebaseToken?: string;
};

export type ForgotPasswordRequest = {
  email: string;
};

export type RegisterRequest = {
  name: string;
  surname: string;
  email: string;
  password: string;
  selectedPlanType: RegisterPlanType;
  firebaseToken?: string;
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
  languageCode: string;
  subscriptionEndDateTime?: string;
  paymentUrl?: string;
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

  logout: () => apiClient.post(ApiEndpoints.Auth.Logout, {}),
};
