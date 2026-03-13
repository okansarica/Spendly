import {createAsyncThunk, createSlice} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import apiClient from '../services/apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';
import {tokenService} from '../services/tokenService';

type SubscriptionPlan = {
  planType: 'Monthly' | 'Yearly';
  price: number;
};

type SubscriptionState = {
  plans: SubscriptionPlan[];
  isLoading: boolean;
  isProcessing: boolean;
  error?: string;
};

const initialState: SubscriptionState = {
  plans: [],
  isLoading: false,
  isProcessing: false,
  error: undefined,
};

export const fetchSubscriptionPlans = createAsyncThunk(
  'subscription/fetchPlans',
  async (_, {rejectWithValue}) => {
    const response = await apiCall(() =>
      apiClient.get<SubscriptionPlan[]>(ApiEndpoints.Users.SubscriptionPlans,
        {
          headers: {'X-Disable-Auth': 'true'},
        }
      )
    );
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data as SubscriptionPlan[];
  }
);

export const createPaymentUrl = createAsyncThunk(
  'subscription/createPaymentUrl',
  async (planType: 'Monthly' | 'Yearly', {rejectWithValue}) => {
    const response = await apiCall(() =>
      apiClient.post<{paymentUrl: string}>(ApiEndpoints.Users.CreatePaymentUrl, {
        selectedPlanType: planType,
      })
    );
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data!.paymentUrl;
  }
);

export const createPaymentUrlWithToken = createAsyncThunk(
  'subscription/createPaymentUrlWithToken',
  async (planType: 'Monthly' | 'Yearly', {rejectWithValue}) => {
    const pendingToken = await tokenService.getPendingAccessToken();
    if (!pendingToken) {
      return rejectWithValue('No pending token found');
    }

    const response = await apiCall(() =>
      apiClient.post<{paymentUrl: string}>(
        ApiEndpoints.Users.CreatePaymentUrlWithToken,
        {
          accessToken: pendingToken,
          selectedPlanType: planType,
        },
        {
          headers: {'X-Disable-Auth': 'true'},
        }
      )
    );
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data!.paymentUrl;
  }
);

const subscriptionSlice = createSlice({
  name: 'subscription',
  initialState,
  reducers: {
    clearSubscriptionError: state => {
      state.error = undefined;
    },
  },
  extraReducers: builder => {
    builder
      .addCase(fetchSubscriptionPlans.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(fetchSubscriptionPlans.fulfilled, (state, action) => {
        state.isLoading = false;
        state.plans = action.payload;
      })
      .addCase(fetchSubscriptionPlans.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(createPaymentUrl.pending, state => {
        state.isProcessing = true;
        state.error = undefined;
      })
      .addCase(createPaymentUrl.fulfilled, state => {
        state.isProcessing = false;
      })
      .addCase(createPaymentUrl.rejected, (state, action) => {
        state.isProcessing = false;
        state.error = action.payload as string;
      })
      .addCase(createPaymentUrlWithToken.pending, state => {
        state.isProcessing = true;
        state.error = undefined;
      })
      .addCase(createPaymentUrlWithToken.fulfilled, state => {
        state.isProcessing = false;
      })
      .addCase(createPaymentUrlWithToken.rejected, (state, action) => {
        state.isProcessing = false;
        state.error = action.payload as string;
      });
  },
});

export const {clearSubscriptionError} = subscriptionSlice.actions;
export default subscriptionSlice.reducer;

