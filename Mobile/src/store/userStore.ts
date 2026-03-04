// CHANGED_BY_AI: 2026-03-03 - Add user profile state and async actions
import {createAsyncThunk, createSlice} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import {
  userService,
  UserProfile,
  UpdateUserProfileRequest,
  ChangePasswordRequest,
} from '../services/userService';
import {setLanguage} from '../utils/translations';

type UserState = {
  profile?: UserProfile;
  languageCode: string;
  isLoading: boolean;
  isSaving: boolean;
  error?: string;
};

const initialState: UserState = {
  profile: undefined,
  languageCode: 'en',
  isLoading: false,
  isSaving: false,
  error: undefined,
};

export const loadUserProfile = createAsyncThunk('user/loadProfile', async (_, {rejectWithValue}) => {
  const response = await apiCall(() => userService.getProfile());
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  const profile = response.data as UserProfile;
  setLanguage(profile.languageCode || 'en');
  return profile;
});

export const updateUserProfile = createAsyncThunk(
  'user/updateProfile',
  async (payload: UpdateUserProfileRequest, {rejectWithValue}) => {
    const response = await apiCall(() => userService.updateProfile(payload));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data as UserProfile;
  }
);

export const changeUserPassword = createAsyncThunk(
  'user/changePassword',
  async (payload: ChangePasswordRequest, {rejectWithValue}) => {
    const response = await apiCall(() => userService.changePassword(payload));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
  }
);

export const setUserLanguage = createAsyncThunk(
  'user/setLanguage',
  async (languageCode: string, {rejectWithValue}) => {
    const response = await apiCall(() => userService.setLanguagePreference({languageCode}));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    const value = response.data?.languageCode || languageCode;
    setLanguage(value);
    return value;
  }
);

export const deleteUserAccount = createAsyncThunk('user/deleteAccount', async (_, {rejectWithValue}) => {
  const response = await apiCall(() => userService.deleteAccount());
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
});

const userSlice = createSlice({
  name: 'user',
  initialState,
  reducers: {
    clearUserState: state => {
      state.profile = undefined;
      state.error = undefined;
      state.languageCode = 'en';
    },
  },
  extraReducers: builder => {
    builder
      .addCase(loadUserProfile.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(loadUserProfile.fulfilled, (state, action) => {
        state.isLoading = false;
        state.profile = action.payload;
        state.languageCode = action.payload.languageCode || 'en';
      })
      .addCase(loadUserProfile.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(updateUserProfile.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(updateUserProfile.fulfilled, (state, action) => {
        state.isSaving = false;
        state.profile = action.payload;
      })
      .addCase(updateUserProfile.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(changeUserPassword.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(changeUserPassword.fulfilled, state => {
        state.isSaving = false;
      })
      .addCase(changeUserPassword.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(setUserLanguage.pending, state => {
        state.error = undefined;
      })
      .addCase(setUserLanguage.fulfilled, (state, action) => {
        state.languageCode = action.payload;
        if (state.profile) {
          state.profile.languageCode = action.payload;
        }
      })
      .addCase(setUserLanguage.rejected, (state, action) => {
        state.error = action.payload as string;
      })
      .addCase(deleteUserAccount.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(deleteUserAccount.fulfilled, state => {
        state.isSaving = false;
      })
      .addCase(deleteUserAccount.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      });
  },
});

export const {clearUserState} = userSlice.actions;
export default userSlice.reducer;

