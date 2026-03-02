// CHANGED_BY_AI: 2026-03-02 - Add categories store
import {createAsyncThunk, createSlice} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import {
  categoriesService,
  CategoryListItem,
  CategoryListRequest,
  CategoryUpsertRequest,
  CategoryMerchantsRequest,
  CategoryMerchantItem,
} from '../services/categoriesService';

type CategoriesState = {
  items: CategoryListItem[];
  merchantsByCategory: Record<string, CategoryMerchantItem[] | undefined>;
  draftMerchantIds: string[];
  isLoading: boolean;
  isSaving: boolean;
  error?: string;
};

const initialState: CategoriesState = {
  items: [],
  merchantsByCategory: {},
  draftMerchantIds: [],
  isLoading: false,
  isSaving: false,
  error: undefined,
};

export const loadCategories = createAsyncThunk('categories/load', async (params: CategoryListRequest | undefined, {rejectWithValue}) => {
  const response = await apiCall(() => categoriesService.getList(params));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as CategoryListItem[];
});

export const createCategory = createAsyncThunk('categories/create', async (payload: CategoryUpsertRequest, {rejectWithValue}) => {
  const response = await apiCall(() => categoriesService.create(payload));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as CategoryListItem;
});

export const updateCategory = createAsyncThunk(
  'categories/update',
  async (payload: {id: string; data: CategoryUpsertRequest}, {rejectWithValue}) => {
    const response = await apiCall(() => categoriesService.update(payload.id, payload.data));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data as CategoryListItem;
  }
);

export const deleteCategory = createAsyncThunk('categories/delete', async (id: string, {rejectWithValue}) => {
  const response = await apiCall(() => categoriesService.remove(id));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return id;
});

export const loadCategoryMerchants = createAsyncThunk(
  'categories/loadMerchants',
  async (categoryId: string, {rejectWithValue}) => {
    const response = await apiCall(() => categoriesService.getMerchants(categoryId));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return {categoryId, items: response.data as CategoryMerchantItem[]};
  }
);

export const addCategoryMerchants = createAsyncThunk(
  'categories/addMerchants',
  async (payload: {categoryId: string; data: CategoryMerchantsRequest}, {rejectWithValue}) => {
    const response = await apiCall(() => categoriesService.addMerchants(payload.categoryId, payload.data));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return response.data as CategoryListItem;
  }
);

export const removeCategoryMerchant = createAsyncThunk(
  'categories/removeMerchant',
  async (payload: {categoryId: string; merchantId: string}, {rejectWithValue}) => {
    const response = await apiCall(() => categoriesService.removeMerchant(payload.categoryId, payload.merchantId));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return payload;
  }
);

const categoriesSlice = createSlice({
  name: 'categories',
  initialState,
  reducers: {
    setDraftMerchantIds: (state, action) => {
      state.draftMerchantIds = action.payload as string[];
    },
    clearDraftMerchantIds: state => {
      state.draftMerchantIds = [];
    },
  },
  extraReducers: builder => {
    builder
      .addCase(loadCategories.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(loadCategories.fulfilled, (state, action) => {
        state.isLoading = false;
        state.items = action.payload;
      })
      .addCase(loadCategories.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(createCategory.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(createCategory.fulfilled, (state, action) => {
        state.isSaving = false;
        state.items = [...state.items, action.payload].sort((a, b) => a.name.localeCompare(b.name));
        state.draftMerchantIds = [];
      })
      .addCase(createCategory.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(updateCategory.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(updateCategory.fulfilled, (state, action) => {
        state.isSaving = false;
        state.items = state.items.map(item => (item.id === action.payload.id ? action.payload : item));
      })
      .addCase(updateCategory.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(deleteCategory.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(deleteCategory.fulfilled, (state, action) => {
        state.isSaving = false;
        state.items = state.items.filter(item => item.id !== action.payload);
      })
      .addCase(deleteCategory.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(loadCategoryMerchants.fulfilled, (state, action) => {
        state.merchantsByCategory[action.payload.categoryId] = action.payload.items;
      })
      .addCase(removeCategoryMerchant.fulfilled, (state, action) => {
        const current = state.merchantsByCategory[action.payload.categoryId] ?? [];
        state.merchantsByCategory[action.payload.categoryId] = current.filter(item => item.id !== action.payload.merchantId);
      });
  },
});

export const {setDraftMerchantIds, clearDraftMerchantIds} = categoriesSlice.actions;

export default categoriesSlice.reducer;

