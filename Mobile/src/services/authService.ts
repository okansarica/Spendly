import apiClient from './apiClient';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SocialLoginRequest {
  provider: 'google' | 'facebook';
  token: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface AuthResponse {
  id: string;
  email: string;
  accessToken?: string;
  refreshToken?: string;
  emailVerificationRequired: boolean;
}

export const authService = {
  login: (data: LoginRequest) =>
    apiClient.post<AuthResponse>('/auth/login', data),

  socialLogin: (data: SocialLoginRequest) =>
    apiClient.post<AuthResponse>('/auth/social-login', data),

  forgotPassword: (data: ForgotPasswordRequest) =>
    apiClient.post('/auth/forgot-password', data),
};

