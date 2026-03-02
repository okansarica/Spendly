import {createSlice, createAsyncThunk} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import {authService, LoginRequest, SocialLoginRequest, RegisterRequest, VerifyEmailRequest} from '../services/authService';
import {tokenService} from '../services/tokenService';

type AuthState = {
  userId: string | undefined;
  email: string | undefined;
  isAuthenticated: boolean;
  isInitializing: boolean;
  isLoading: boolean;
  error: string | undefined;
  emailVerificationRequired: boolean;
};

const initialState: AuthState = {
  userId: undefined,
  email: undefined,
  isAuthenticated: false,
  isInitializing: true,
  isLoading: false,
  error: undefined,
  emailVerificationRequired: false,
};

export const checkAuth = createAsyncThunk('auth/checkAuth', async () => {
  return await tokenService.hasAccessToken();
});

export const login = createAsyncThunk(
  'auth/login',
  async (data: LoginRequest, {rejectWithValue}) => {
    const response = await apiCall(() => authService.login(data));
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
    return auth;
  },
);

export const socialLogin = createAsyncThunk(
  'auth/socialLogin',
  async (data: SocialLoginRequest, {rejectWithValue}) => {
    const response = await apiCall(() => authService.socialLogin(data));
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

export const logout = createAsyncThunk('auth/logout', async () => {
  await tokenService.clearTokens();
});

export const register = createAsyncThunk(
  'auth/register',
  async (data: RegisterRequest, {rejectWithValue}) => {
    const response = await apiCall(() => authService.register(data));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data!;
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
    if (auth.accessToken && auth.refreshToken) {
      await tokenService.saveTokens(
        auth.accessToken,
        auth.refreshToken,
        auth.accessTokenExpire,
        auth.refreshTokenExpire
      );
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

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    clearError: state => {
      state.error = undefined;
    },
  },
  extraReducers: builder => {
    builder
      .addCase(checkAuth.fulfilled, (state, action) => {
        state.isAuthenticated = action.payload;
        state.isInitializing = false;
      })
      .addCase(login.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.isLoading = false;
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
      .addCase(login.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(socialLogin.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(socialLogin.fulfilled, (state, action) => {
        state.isLoading = false;
        state.userId = action.payload.id;
        state.email = action.payload.email;
        state.isAuthenticated = true;
      })
      .addCase(socialLogin.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(forgotPassword.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(forgotPassword.fulfilled, state => {
        state.isLoading = false;
      })
      .addCase(forgotPassword.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(logout.fulfilled, state => {
        state.userId = undefined;
        state.email = undefined;
        state.isAuthenticated = false;
      })
      .addCase(register.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(register.fulfilled, (state, action) => {
        state.isLoading = false;
        state.emailVerificationRequired = true;
        state.userId = action.payload.id;
        state.email = action.payload.email;
      })
      .addCase(register.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(verifyEmail.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(verifyEmail.fulfilled, (state, action) => {
        state.isLoading = false;
        state.userId = action.payload.id;
        state.email = action.payload.email;
        state.isAuthenticated = true;
        state.emailVerificationRequired = false;
      })
      .addCase(verifyEmail.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(resendCode.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(resendCode.fulfilled, state => {
        state.isLoading = false;
      })
      .addCase(resendCode.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const {clearError} = authSlice.actions;
export default authSlice.reducer;
