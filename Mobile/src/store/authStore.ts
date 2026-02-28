import {create} from 'zustand';
import * as Keychain from 'react-native-keychain';
import {authService, LoginRequest, SocialLoginRequest} from '../services/authService';

interface AuthState {
  userId: string | null;
  email: string | null;
  isAuthenticated: boolean;
  isInitializing: boolean;
  isLoading: boolean;
  error: string | null;
  emailVerificationRequired: boolean;
  checkAuth: () => Promise<void>;
  login: (data: LoginRequest) => Promise<void>;
  socialLogin: (data: SocialLoginRequest) => Promise<void>;
  forgotPassword: (email: string) => Promise<void>;
  logout: () => Promise<void>;
  clearError: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  userId: null,
  email: null,
  isAuthenticated: false,
  isInitializing: true,
  isLoading: false,
  error: null,
  emailVerificationRequired: false,

  checkAuth: async () => {
    const creds = await Keychain.getGenericPassword({service: 'accessToken'});
    set({isAuthenticated: !!creds, isInitializing: false});
  },

  login: async (data) => {
    set({isLoading: true, error: null});
    try {
      const res = await authService.login(data);
      const auth = res.data;
      if (auth.emailVerificationRequired) {
        set({emailVerificationRequired: true, userId: auth.id, email: auth.email, isLoading: false});
        return;
      }
      await Keychain.setGenericPassword('accessToken', auth.accessToken!, {service: 'accessToken'});
      await Keychain.setGenericPassword('refreshToken', auth.refreshToken!, {service: 'refreshToken'});
      set({userId: auth.id, email: auth.email, isAuthenticated: true, isLoading: false, emailVerificationRequired: false});
    } catch (e: any) {
      const msg = e.response?.data?.errors?.[0]?.message ?? 'Login failed';
      set({error: msg, isLoading: false});
    }
  },

  socialLogin: async (data) => {
    set({isLoading: true, error: null});
    try {
      const res = await authService.socialLogin(data);
      const auth = res.data;
      await Keychain.setGenericPassword('accessToken', auth.accessToken!, {service: 'accessToken'});
      await Keychain.setGenericPassword('refreshToken', auth.refreshToken!, {service: 'refreshToken'});
      set({userId: auth.id, email: auth.email, isAuthenticated: true, isLoading: false});
    } catch (e: any) {
      const msg = e.response?.data?.errors?.[0]?.message ?? 'Social login failed';
      set({error: msg, isLoading: false});
    }
  },

  forgotPassword: async (email) => {
    set({isLoading: true, error: null});
    try {
      await authService.forgotPassword({email});
      set({isLoading: false});
    } catch (e: any) {
      const msg = e.response?.data?.errors?.[0]?.message ?? 'Request failed';
      set({error: msg, isLoading: false});
    }
  },

  logout: async () => {
    await Keychain.resetGenericPassword({service: 'accessToken'});
    await Keychain.resetGenericPassword({service: 'refreshToken'});
    set({userId: null, email: null, isAuthenticated: false});
  },

  clearError: () => set({error: null}),
}));

