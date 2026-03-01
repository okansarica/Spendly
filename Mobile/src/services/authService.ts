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
  //TODO access token ve refresh token expire date timelari da donmeli apiden
  //TODO kullanici name surname de apiden donmeli ve session bilgileri ile kaydedilmeli. bunu ilerde kullanici ekranlarinda gosterecegiz
  refreshToken?: string;
  emailVerificationRequired: boolean;
};

export const authService = {
  login: (data: LoginRequest) =>
    apiClient.post<AuthResponse>(ApiEndpoints.Auth.Login, data),

  socialLogin: (data: SocialLoginRequest) =>
    apiClient.post<AuthResponse>(ApiEndpoints.Auth.SocialLogin, data),

  forgotPassword: (data: ForgotPasswordRequest) =>
    apiClient.post(ApiEndpoints.Auth.ForgotPassword, data),

  register: (data: RegisterRequest) =>
    apiClient.post<AuthResponse>(ApiEndpoints.Auth.Register, data),

  verifyEmail: (data: VerifyEmailRequest) =>
    apiClient.post<AuthResponse>(ApiEndpoints.Auth.VerifyEmail, data),

  resendCode: (userId: string) =>
    apiClient.post(ApiEndpoints.Auth.ResendCode, {userId}),
};
