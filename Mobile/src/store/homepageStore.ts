// CHANGED_BY_AI: 2026-03-02 - Add homepage Redux store
import {createSlice, createAsyncThunk} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import {homepageService, HomepageResponse} from '../services/homepageService';

type HomepageState = {
  data: HomepageResponse | undefined;
  isLoading: boolean;
  isRefreshing: boolean;
  error: string | undefined;
};

const initialState: HomepageState = {
  data: undefined,
  isLoading: false,
  isRefreshing: false,
  error: undefined,
};

export const loadHomepage = createAsyncThunk('homepage/load', async (_, {rejectWithValue}) => {
  const response = await apiCall(() => homepageService.getHomepage());
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as HomepageResponse;
});

export const refreshHomepage = createAsyncThunk('homepage/refresh', async (_, {rejectWithValue}) => {
  const response = await apiCall(() => homepageService.getHomepage());
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as HomepageResponse;
});

const homepageSlice = createSlice({
  name: 'homepage',
  initialState,
  reducers: {},
  extraReducers: builder => {
    builder
      .addCase(loadHomepage.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(loadHomepage.fulfilled, (state, action) => {
        state.isLoading = false;
        state.data = action.payload;
      })
      .addCase(loadHomepage.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(refreshHomepage.pending, state => {
        state.isRefreshing = true;
        state.error = undefined;
      })
      .addCase(refreshHomepage.fulfilled, (state, action) => {
        state.isRefreshing = false;
        state.data = action.payload;
      })
      .addCase(refreshHomepage.rejected, (state, action) => {
        state.isRefreshing = false;
        state.error = action.payload as string;
      });
  },
});

export default homepageSlice.reducer;

