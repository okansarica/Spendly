// CHANGED_BY_AI: 2026-03-02 - Add merchants store
import {createAsyncThunk, createSlice} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import {merchantsService, MerchantListItem, MerchantListRequest} from '../services/merchantsService';

type MerchantsState = {
  items: MerchantListItem[];
  isLoading: boolean;
  error?: string;
};

const initialState: MerchantsState = {
  items: [],
  isLoading: false,
  error: undefined,
};

export const loadMerchants = createAsyncThunk('merchants/load', async (params: MerchantListRequest | undefined, {rejectWithValue}) => {
  const response = await apiCall(() => merchantsService.getList(params));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as MerchantListItem[];
});

const merchantsSlice = createSlice({
  name: 'merchants',
  initialState,
  reducers: {},
  extraReducers: builder => {
    builder
      .addCase(loadMerchants.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(loadMerchants.fulfilled, (state, action) => {
        state.isLoading = false;
        state.items = action.payload;
      })
      .addCase(loadMerchants.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export default merchantsSlice.reducer;

