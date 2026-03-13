// CHANGED_BY_AI: 2026-03-13 - Add merchant reports store
// CHANGED_BY_AI: 2026-03-02 - Add reports store
import {createSlice, createAsyncThunk} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import {
  reportsService,
  ReportsOverviewResponse,
  ReportCategoryDetailResponse,
  AccountsOverviewResponse,
  AccountDetailResponse,
  MerchantsReportOverviewResponse,
  MerchantDetailResponse,
  ReportsOverviewParams,
  ReportsCategoryParams,
  AccountsOverviewParams,
  AccountDetailParams,
  MerchantsReportOverviewParams,
  MerchantDetailParams,
} from '../services/reportsService';

type ReportsState = {
  overview: ReportsOverviewResponse | undefined;
  categoryDetail: ReportCategoryDetailResponse | undefined;
  accountsOverview: AccountsOverviewResponse | undefined;
  accountDetail: AccountDetailResponse | undefined;
  merchantsOverview: MerchantsReportOverviewResponse | undefined;
  merchantDetail: MerchantDetailResponse | undefined;
  isLoadingOverview: boolean;
  isLoadingCategoryDetail: boolean;
  isLoadingAccountsOverview: boolean;
  isLoadingAccountDetail: boolean;
  isLoadingMerchantsOverview: boolean;
  isLoadingMerchantDetail: boolean;
  error: string | undefined;
};

const initialState: ReportsState = {
  overview: undefined,
  categoryDetail: undefined,
  accountsOverview: undefined,
  accountDetail: undefined,
  merchantsOverview: undefined,
  merchantDetail: undefined,
  isLoadingOverview: false,
  isLoadingCategoryDetail: false,
  isLoadingAccountsOverview: false,
  isLoadingAccountDetail: false,
  isLoadingMerchantsOverview: false,
  isLoadingMerchantDetail: false,
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

export const loadMerchantsOverview = createAsyncThunk(
  'reports/loadMerchantsOverview',
  async (params: MerchantsReportOverviewParams | undefined, {rejectWithValue}) => {
    const response = await apiCall(() => reportsService.getMerchantsOverview(params));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data as MerchantsReportOverviewResponse;
  }
);

export const loadMerchantDetail = createAsyncThunk(
  'reports/loadMerchantDetail',
  async (
    payload: {merchantId: string; params?: MerchantDetailParams},
    {rejectWithValue}
  ) => {
    const response = await apiCall(() => reportsService.getMerchantDetail(payload.merchantId, payload.params));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data as MerchantDetailResponse;
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
      })
      .addCase(loadMerchantsOverview.pending, state => {
        state.isLoadingMerchantsOverview = true;
        state.error = undefined;
      })
      .addCase(loadMerchantsOverview.fulfilled, (state, action) => {
        state.isLoadingMerchantsOverview = false;
        state.merchantsOverview = action.payload;
      })
      .addCase(loadMerchantsOverview.rejected, (state, action) => {
        state.isLoadingMerchantsOverview = false;
        state.error = action.payload as string;
      })
      .addCase(loadMerchantDetail.pending, state => {
        state.isLoadingMerchantDetail = true;
        state.error = undefined;
      })
      .addCase(loadMerchantDetail.fulfilled, (state, action) => {
        state.isLoadingMerchantDetail = false;
        state.merchantDetail = action.payload;
      })
      .addCase(loadMerchantDetail.rejected, (state, action) => {
        state.isLoadingMerchantDetail = false;
        state.error = action.payload as string;
      });
  },
});

export default reportsSlice.reducer;

