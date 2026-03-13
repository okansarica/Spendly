// CHANGED_BY_AI: 2026-03-12 - Respect backend register auth response for direct login flow
// CHANGED_BY_AI: 2026-03-03 - Integrate backend logout in auth store
// CHANGED_BY_AI: 2026-03-05 - Add Firebase token to auth flows
import {createSlice, createAsyncThunk} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import {authService, LoginRequest, SocialLoginRequest, RegisterRequest, VerifyEmailRequest} from '../services/authService';
import {tokenService} from '../services/tokenService';
import sessionService from '../services/sessionService';
import {subscriptionService} from '../services/subscriptionService';
import {clearUserState} from './userStore';
import {setLanguage} from '../utils/translations';
import {firebaseService} from '../services/firebaseService';

type AuthState = {
  userId: string | undefined;
  email: string | undefined;
  isAuthenticated: boolean;
  isInitializing: boolean;
  isLoading: boolean;
  emailVerificationRequired: boolean;
};

const initialState: AuthState = {
  userId: undefined,
  email: undefined,
  isAuthenticated: false,
  isInitializing: true,
  isLoading: false,
  emailVerificationRequired: false,
};

export const checkAuth = createAsyncThunk('auth/checkAuth', async () => {
  return await tokenService.hasAccessToken();
});

export const login = createAsyncThunk(
  'auth/login',
  async (data: LoginRequest, {rejectWithValue}) => {
    const firebaseToken = await firebaseService.getCachedToken();
    const requestData = firebaseToken ? {...data, firebaseToken} : data;
    
    const response = await apiCall(() => authService.login(requestData));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    const auth = response.data!;
    if (!auth.emailVerificationRequired && auth.accessToken && auth.refreshToken) {
      await tokenService.saveTokens(
        auth.accessToken,
        auth.refreshToken,
        auth.accessTokenExpire,
        auth.refreshTokenExpire
      );
    }
    if (auth.subscriptionEndDateTime) {
      await subscriptionService.saveSubscriptionEndDate(auth.subscriptionEndDateTime);
    }
    return auth;
  },
);

export const socialLogin = createAsyncThunk(
  'auth/socialLogin',
  async (data: SocialLoginRequest, {rejectWithValue}) => {
    const firebaseToken = await firebaseService.getCachedToken();
    const requestData = firebaseToken ? {...data, firebaseToken} : data;
    
    const response = await apiCall(() => authService.socialLogin(requestData));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    const auth = response.data!;
    if (auth.accessToken && auth.refreshToken) {
      await tokenService.saveTokens(
        auth.accessToken,
        auth.refreshToken,
        auth.accessTokenExpire,
        auth.refreshTokenExpire
      );
    }
    if (auth.subscriptionEndDateTime) {
      await subscriptionService.saveSubscriptionEndDate(auth.subscriptionEndDateTime);
    }
    return auth;
  },
);

export const forgotPassword = createAsyncThunk(
  'auth/forgotPassword',
  async (email: string, {rejectWithValue}) => {
    const response = await apiCall(() => authService.forgotPassword({email}));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
  },
);

export const logout = createAsyncThunk('auth/logout', async (_, {dispatch}) => {
  try {
    await apiCall(() => authService.logout());
  } catch (error) {
  } finally {
    await Promise.all([
      tokenService.clearTokens().catch(() => undefined),
      sessionService.clearSessionId().catch(() => undefined),
      subscriptionService.clearSubscriptionEndDate().catch(() => undefined),
    ]);
    dispatch(clearUserState());
  }
});

export const logoutLocal = createAsyncThunk('auth/logoutLocal', async (_, {dispatch}) => {
  await Promise.all([
    tokenService.clearTokens().catch(() => undefined),
    sessionService.clearSessionId().catch(() => undefined),
    subscriptionService.clearSubscriptionEndDate().catch(() => undefined),
  ]);
  dispatch(clearUserState());
});

export const register = createAsyncThunk(
  'auth/register',
  async (data: RegisterRequest, {rejectWithValue}) => {
    const firebaseToken = await firebaseService.getCachedToken();
    const requestData = firebaseToken ? {...data, firebaseToken} : data;
    
    const response = await apiCall(() => authService.register(requestData));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    const auth = response.data!;
    if (!auth.emailVerificationRequired && auth.accessToken && auth.refreshToken) {
      await tokenService.saveTokens(
        auth.accessToken,
        auth.refreshToken,
        auth.accessTokenExpire,
        auth.refreshTokenExpire,
      );
    }
    if (auth.subscriptionEndDateTime) {
      await subscriptionService.saveSubscriptionEndDate(auth.subscriptionEndDateTime);
    }
    return auth;
  },
);

export const verifyEmail = createAsyncThunk(
  'auth/verifyEmail',
  async (data: VerifyEmailRequest, {rejectWithValue}) => {
    const response = await apiCall(() => authService.verifyEmail(data));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    const auth = response.data!;
    
    // If paymentUrl exists, save tokens as pending (waiting for payment)
    if (auth.paymentUrl && auth.accessToken && auth.refreshToken) {
      await tokenService.savePendingTokens(
        auth.accessToken,
        auth.refreshToken,
        auth.accessTokenExpire,
        auth.refreshTokenExpire
      );
    } else if (auth.accessToken && auth.refreshToken) {
      // No payment needed (Trial), save tokens normally
      await tokenService.saveTokens(
        auth.accessToken,
        auth.refreshToken,
        auth.accessTokenExpire,
        auth.refreshTokenExpire
      );
    }
    
    if (auth.subscriptionEndDateTime) {
      await subscriptionService.saveSubscriptionEndDate(auth.subscriptionEndDateTime);
    }
    return auth;
  },
);

export const resendCode = createAsyncThunk(
  'auth/resendCode',
  async (userId: string, {rejectWithValue}) => {
    const response = await apiCall(() => authService.resendCode(userId));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
  },
);

export const activatePendingAuth = createAsyncThunk(
  'auth/activatePendingAuth',
  async () => {
    await tokenService.activatePendingTokens();
    const hasToken = await tokenService.hasAccessToken();
    return hasToken;
  },
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {},
  extraReducers: builder => {
    builder
      .addCase(checkAuth.fulfilled, (state, action) => {
        state.isAuthenticated = action.payload;
        state.isInitializing = false;
      })
      .addCase(login.pending, state => {
        state.isLoading = true;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.isLoading = false;
        if (action.payload.languageCode) {
          setLanguage(action.payload.languageCode);
        }
        if (action.payload.emailVerificationRequired) {
          state.emailVerificationRequired = true;
          state.userId = action.payload.id;
          state.email = action.payload.email;
        } else {
          state.userId = action.payload.id;
          state.email = action.payload.email;
          state.isAuthenticated = true;
          state.emailVerificationRequired = false;
        }
      })
      .addCase(login.rejected, state => {
        state.isLoading = false;
      })
      .addCase(socialLogin.pending, state => {
        state.isLoading = true;
      })
      .addCase(socialLogin.fulfilled, (state, action) => {
        state.isLoading = false;
        if (action.payload.languageCode) {
          setLanguage(action.payload.languageCode);
        }
        state.userId = action.payload.id;
        state.email = action.payload.email;
        state.isAuthenticated = true;
      })
      .addCase(socialLogin.rejected, state => {
        state.isLoading = false;
      })
      .addCase(forgotPassword.pending, state => {
        state.isLoading = true;
      })
      .addCase(forgotPassword.fulfilled, state => {
        state.isLoading = false;
      })
      .addCase(forgotPassword.rejected, state => {
        state.isLoading = false;
      })
      .addCase(logout.pending, state => {
        state.isLoading = true;
      })
      .addCase(logout.fulfilled, state => {
        state.isLoading = false;
        state.userId = undefined;
        state.email = undefined;
        state.isAuthenticated = false;
        state.emailVerificationRequired = false;
      })
      .addCase(logout.rejected, state => {
        state.isLoading = false;
        state.userId = undefined;
        state.email = undefined;
        state.isAuthenticated = false;
        state.emailVerificationRequired = false;
      })
      .addCase(logoutLocal.fulfilled, state => {
        state.userId = undefined;
        state.email = undefined;
        state.isAuthenticated = false;
        state.emailVerificationRequired = false;
      })
      .addCase(register.pending, state => {
        state.isLoading = true;
      })
      .addCase(register.fulfilled, (state, action) => {
        state.isLoading = false;
        if (action.payload.languageCode) {
          setLanguage(action.payload.languageCode);
        }
        if (action.payload.emailVerificationRequired) {
          state.emailVerificationRequired = true;
          state.userId = action.payload.id;
          state.email = action.payload.email;
          state.isAuthenticated = false;
        } else {
          state.userId = action.payload.id;
          state.email = action.payload.email;
          state.isAuthenticated = true;
          state.emailVerificationRequired = false;
        }
      })
      .addCase(register.rejected, state => {
        state.isLoading = false;
      })
      .addCase(verifyEmail.pending, state => {
        state.isLoading = true;
      })
      .addCase(verifyEmail.fulfilled, (state, action) => {
        state.isLoading = false;
        if (action.payload.languageCode) {
          setLanguage(action.payload.languageCode);
        }
        state.userId = action.payload.id;
        state.email = action.payload.email;
        state.emailVerificationRequired = false;
        
        // If paymentUrl exists, don't authenticate yet (waiting for payment)
        if (action.payload.paymentUrl) {
          state.isAuthenticated = false;
        } else {
          // No payment needed (Trial), authenticate immediately
          state.isAuthenticated = true;
        }
      })
      .addCase(verifyEmail.rejected, state => {
        state.isLoading = false;
      })
      .addCase(resendCode.pending, state => {
        state.isLoading = true;
      })
      .addCase(resendCode.fulfilled, state => {
        state.isLoading = false;
      })
      .addCase(resendCode.rejected, state => {
        state.isLoading = false;
      })
      .addCase(activatePendingAuth.fulfilled, (state, action) => {
        if (action.payload) {
          state.isAuthenticated = true;
        }
      });
  },
});

export default authSlice.reducer;
