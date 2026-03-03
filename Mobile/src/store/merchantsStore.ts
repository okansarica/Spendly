// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname editing
// CHANGED_BY_AI: 2026-03-02 - Add merchants store
import {createAsyncThunk, createSlice} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import {merchantsService, MerchantListItem, MerchantListRequest, MerchantDetail} from '../services/merchantsService';

type MerchantsState = {
  items: MerchantListItem[];
  detail?: MerchantDetail;
  isLoading: boolean;
  isSaving: boolean;
  error?: string;
};

const initialState: MerchantsState = {
  items: [],
  detail: undefined,
  isLoading: false,
  isSaving: false,
  error: undefined,
};

export const loadMerchants = createAsyncThunk('merchants/load', async (params: MerchantListRequest | undefined, {rejectWithValue}) => {
  const response = await apiCall(() => merchantsService.getList(params));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as MerchantListItem[];
});


export const updateMerchant = createAsyncThunk(
  'merchants/update',
  async (payload: {id: string; categoryId?: string; nickname?: string}, {rejectWithValue, getState}) => {
    const response = await apiCall(() => merchantsService.update(payload.id, {categoryId: payload.categoryId, nickname: payload.nickname}));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    const result = response.data as MerchantDetail;
    if (!result.categoryName && payload.categoryId) {
      // try to fill categoryName from categories slice
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const state = (getState as any)();
      const categories: {id: string; name: string}[] = state.categories?.items ?? [];
      const match = categories.find((c: {id: string}) => c.id === payload.categoryId);
      if (match) {
        result.categoryName = match.name;
      }
    }
    return result as MerchantDetail;
  }
);

export const deleteMerchant = createAsyncThunk('merchants/delete', async (id: string, {rejectWithValue}) => {
  const response = await apiCall(() => merchantsService.remove(id));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return id;
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
      })
            .addCase(updateMerchant.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(updateMerchant.fulfilled, (state, action) => {
        state.isSaving = false;
        state.detail = action.payload;
        state.items = state.items.map(item =>
          item.id === action.payload.id
            ? {
                ...item,
                nickname: action.payload.nickname,
                categoryId: action.payload.categoryId,
                categoryName: action.payload.categoryName ?? item.categoryName,
              }
            : item
        );
      })
      .addCase(updateMerchant.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(deleteMerchant.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(deleteMerchant.fulfilled, (state, action) => {
        state.isLoading = false;
        state.items = state.items.filter(item => item.id !== action.payload);
      })
      .addCase(deleteMerchant.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export default merchantsSlice.reducer;
