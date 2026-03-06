// CHANGED_BY_AI: 2026-03-06 - Add banks and accounts redux store
import {createAsyncThunk, createSlice} from '@reduxjs/toolkit';
import {apiCall} from '../services/apiClient';
import {
  banksService,
  BankListItem,
  BankListRequest,
  BankUpsertRequest,
  BankAccountUpsertRequest,
  BankAccountItem,
  BankDefinitionItem,
} from '../services/banksService';

type BanksState = {
  items: BankListItem[];
  definitions: BankDefinitionItem[];
  isLoading: boolean;
  isSaving: boolean;
  error?: string;
};

const initialState: BanksState = {
  items: [],
  definitions: [],
  isLoading: false,
  isSaving: false,
  error: undefined,
};

export const loadBanks = createAsyncThunk('banks/load', async (params: BankListRequest | undefined, {rejectWithValue}) => {
  const response = await apiCall(() => banksService.getList(params));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as BankListItem[];
});

export const loadBankDefinitions = createAsyncThunk('banks/loadDefinitions', async (_, {rejectWithValue}) => {
  const response = await apiCall(() => banksService.getBankDefinitions());
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as BankDefinitionItem[];
});

export const createBank = createAsyncThunk('banks/create', async (payload: BankUpsertRequest, {rejectWithValue}) => {
  const response = await apiCall(() => banksService.create(payload));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as BankListItem;
});

export const updateBank = createAsyncThunk('banks/update', async (payload: {id: string; data: BankUpsertRequest}, {rejectWithValue}) => {
  const response = await apiCall(() => banksService.update(payload.id, payload.data));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return response.data as BankListItem;
});

export const deleteBank = createAsyncThunk('banks/delete', async (id: string, {rejectWithValue}) => {
  const response = await apiCall(() => banksService.remove(id));
  if (!response.isSuccess) {
    return rejectWithValue(response.errorMessage);
  }
  return id;
});

export const createBankAccount = createAsyncThunk(
  'banks/createAccount',
  async (payload: {bankId: string; data: BankAccountUpsertRequest}, {rejectWithValue}) => {
    const response = await apiCall(() => banksService.createAccount(payload.bankId, payload.data));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return {bankId: payload.bankId, account: response.data as BankAccountItem};
  }
);

export const updateBankAccount = createAsyncThunk(
  'banks/updateAccount',
  async (payload: {bankId: string; id: string; data: BankAccountUpsertRequest}, {rejectWithValue}) => {
    const response = await apiCall(() => banksService.updateAccount(payload.bankId, payload.id, payload.data));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return {bankId: payload.bankId, account: response.data as BankAccountItem};
  }
);

export const deleteBankAccount = createAsyncThunk(
  'banks/deleteAccount',
  async (payload: {bankId: string; id: string}, {rejectWithValue}) => {
    const response = await apiCall(() => banksService.removeAccount(payload.bankId, payload.id));
    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }
    return payload;
  }
);

const sortBanks = (banks: BankListItem[]) => [...banks].sort((a, b) => a.name.localeCompare(b.name));
const sortAccounts = (accounts: BankAccountItem[]) => [...accounts].sort((a, b) => a.name.localeCompare(b.name));

const banksSlice = createSlice({
  name: 'banks',
  initialState,
  reducers: {},
  extraReducers: builder => {
    builder
      .addCase(loadBanks.pending, state => {
        state.isLoading = true;
        state.error = undefined;
      })
      .addCase(loadBanks.fulfilled, (state, action) => {
        state.isLoading = false;
        state.items = action.payload.map(item => ({...item, accounts: sortAccounts(item.accounts)}));
      })
      .addCase(loadBanks.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(loadBankDefinitions.fulfilled, (state, action) => {
        state.definitions = action.payload;
      })
      .addCase(createBank.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(createBank.fulfilled, (state, action) => {
        state.isSaving = false;
        state.items = sortBanks([...state.items, {...action.payload, accounts: sortAccounts(action.payload.accounts ?? [])}]);
      })
      .addCase(createBank.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(updateBank.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(updateBank.fulfilled, (state, action) => {
        state.isSaving = false;
        state.items = sortBanks(
          state.items.map(item =>
            item.id === action.payload.id
              ? {...action.payload, accounts: sortAccounts(action.payload.accounts ?? item.accounts)}
              : item
          )
        );
      })
      .addCase(updateBank.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(deleteBank.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(deleteBank.fulfilled, (state, action) => {
        state.isSaving = false;
        state.items = state.items.filter(item => item.id !== action.payload);
      })
      .addCase(deleteBank.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(createBankAccount.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(createBankAccount.fulfilled, (state, action) => {
        state.isSaving = false;
        state.items = state.items.map(item =>
          item.id === action.payload.bankId
            ? {...item, accounts: sortAccounts([...item.accounts, action.payload.account])}
            : item
        );
      })
      .addCase(createBankAccount.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(updateBankAccount.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(updateBankAccount.fulfilled, (state, action) => {
        state.isSaving = false;
        state.items = state.items.map(item =>
          item.id === action.payload.bankId
            ? {
                ...item,
                accounts: sortAccounts(
                  item.accounts.map(account =>
                    account.id === action.payload.account.id ? action.payload.account : account
                  )
                ),
              }
            : item
        );
      })
      .addCase(updateBankAccount.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      })
      .addCase(deleteBankAccount.pending, state => {
        state.isSaving = true;
        state.error = undefined;
      })
      .addCase(deleteBankAccount.fulfilled, (state, action) => {
        state.isSaving = false;
        state.items = state.items.map(item =>
          item.id === action.payload.bankId
            ? {...item, accounts: item.accounts.filter(account => account.id !== action.payload.id)}
            : item
        );
      })
      .addCase(deleteBankAccount.rejected, (state, action) => {
        state.isSaving = false;
        state.error = action.payload as string;
      });
  },
});

export default banksSlice.reducer;

