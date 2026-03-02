// CHANGED_BY_AI: 2026-03-02 - Add reports store
import {createSlice, createAsyncThunk} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import {
  reportsService,
  ReportsOverviewResponse,
  ReportCategoryDetailResponse,
  AccountsOverviewResponse,
  AccountDetailResponse,
  ReportsOverviewParams,
  ReportsCategoryParams,
  AccountsOverviewParams,
  AccountDetailParams,
} from '../services/reportsService';

type ReportsState = {
  overview: ReportsOverviewResponse | undefined;
  categoryDetail: ReportCategoryDetailResponse | undefined;
  accountsOverview: AccountsOverviewResponse | undefined;
  accountDetail: AccountDetailResponse | undefined;
  isLoadingOverview: boolean;
  isLoadingCategoryDetail: boolean;
  isLoadingAccountsOverview: boolean;
  isLoadingAccountDetail: boolean;
  error: string | undefined;
};

const initialState: ReportsState = {
  overview: undefined,
  categoryDetail: undefined,
  accountsOverview: undefined,
  accountDetail: undefined,
  isLoadingOverview: false,
  isLoadingCategoryDetail: false,
  isLoadingAccountsOverview: false,
  isLoadingAccountDetail: false,
  error: undefined,
};

export const loadReportsOverview = createAsyncThunk('reports/loadOverview', async (params: ReportsOverviewParams | undefined, {rejectWithValue}) => {
  const response = await apiCall(() => reportsService.getOverview(params));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as ReportsOverviewResponse;
});

export const loadCategoryDetail = createAsyncThunk(
  'reports/loadCategoryDetail',
  async (
    payload: {categoryId: string; params?: ReportsCategoryParams},
    {rejectWithValue}
  ) => {
    const response = await apiCall(() => reportsService.getCategoryDetail(payload.categoryId, payload.params));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data as ReportCategoryDetailResponse;
  }
);

export const loadAccountsOverview = createAsyncThunk(
  'reports/loadAccountsOverview',
  async (params: AccountsOverviewParams | undefined, {rejectWithValue}) => {
    const response = await apiCall(() => reportsService.getAccountsOverview(params));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data as AccountsOverviewResponse;
  }
);

export const loadAccountDetail = createAsyncThunk(
  'reports/loadAccountDetail',
  async (
    payload: {accountId: string; params?: AccountDetailParams},
    {rejectWithValue}
  ) => {
    const response = await apiCall(() => reportsService.getAccountDetail(payload.accountId, payload.params));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data as AccountDetailResponse;
  }
);

const reportsSlice = createSlice({
  name: 'reports',
  initialState,
  reducers: {},
  extraReducers: builder => {
    builder
      .addCase(loadReportsOverview.pending, state => {
        state.isLoadingOverview = true;
        state.error = undefined;
      })
      .addCase(loadReportsOverview.fulfilled, (state, action) => {
        state.isLoadingOverview = false;
        state.overview = action.payload;
      })
      .addCase(loadReportsOverview.rejected, (state, action) => {
        state.isLoadingOverview = false;
        state.error = action.payload as string;
      })
      .addCase(loadCategoryDetail.pending, state => {
        state.isLoadingCategoryDetail = true;
        state.error = undefined;
      })
      .addCase(loadCategoryDetail.fulfilled, (state, action) => {
        state.isLoadingCategoryDetail = false;
        state.categoryDetail = action.payload;
      })
      .addCase(loadCategoryDetail.rejected, (state, action) => {
        state.isLoadingCategoryDetail = false;
        state.error = action.payload as string;
      })
      .addCase(loadAccountsOverview.pending, state => {
        state.isLoadingAccountsOverview = true;
        state.error = undefined;
      })
      .addCase(loadAccountsOverview.fulfilled, (state, action) => {
        state.isLoadingAccountsOverview = false;
        state.accountsOverview = action.payload;
      })
      .addCase(loadAccountsOverview.rejected, (state, action) => {
        state.isLoadingAccountsOverview = false;
        state.error = action.payload as string;
      })
      .addCase(loadAccountDetail.pending, state => {
        state.isLoadingAccountDetail = true;
        state.error = undefined;
      })
      .addCase(loadAccountDetail.fulfilled, (state, action) => {
        state.isLoadingAccountDetail = false;
        state.accountDetail = action.payload;
      })
      .addCase(loadAccountDetail.rejected, (state, action) => {
        state.isLoadingAccountDetail = false;
        state.error = action.payload as string;
      });
  },
});

export default reportsSlice.reducer;

