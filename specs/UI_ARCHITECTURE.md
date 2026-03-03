// CHANGED_BY_AI: 2026-03-03 - add redux insert/update and screen handling example
# UI ARCHITECTURE

## Null Policy

- Never use `null` on the client side
- Values are either populated or `undefined`
- Do not assign `null` to variables, props, or state
- TypeScript types use `T | undefined`, never `T | null`

```text
type TempClass = {
  id: string;
  email: string;
  name?: string;
};
```

## API Call Pattern

- Never use `try/catch` in screens, components, or services for API calls
- All API calls go through the `apiCall` base function in `src/services/apiClient.ts`
- This function handles all HTTP errors centrally
- All API communication must use async/await.
- Never swallow errors, always show error messages coming from ApiClient using a Toast

```text
type ApiResponse<T> = {
  isSuccess: boolean;
  data?: T ;
  errorMessage?: string;
};

async function apiCall<T>(request: () => Promise<AxiosResponse<T>>): Promise<ApiResponse<T>> {
  const response = await request();
  return { isSuccess: true, data: response.data, errorMessage: undefined };
}
```

On 400 response: return `{ isSuccess: false, errorMessage: response.data.message }`
On 500 response: return `{ isSuccess: false, errorMessage: 'An unexpected error occurred' }`



## Response Handling Pattern

All callers check `isSuccess` before proceeding; see `src/services/apiClient.ts` for the canonical example:

```text
const response = await apiCall(() => authService.login(credentials));

if (!response.isSuccess) {
  showError(response.errorMessage);
  return;
}

dispatch(setUser(response.data));
```

Never assume success - always check `isSuccess` first.

## ApiResponse Type

Client-side response class mirrors the backend pattern and is documented in `src/services/apiClient.ts`.

Services return `ApiResponse<T>`. Screens/actions check `isSuccess` and act accordingly.

## State Management Rules

- Redux for global state (no Saga)
- No `null` in Redux state - use `undefined` for missing values
- Async actions (thunks) use `apiCall` and dispatch result
- Screens read from store selectors, never call API directly

## Redux Insert/Update Pattern

Use `rejectWithValue` to return API error messages and handle them in the screen.

```text
export const updateMerchant = createAsyncThunk(
  'merchants/update',
  async (
    payload: { id: string; ... },
    { rejectWithValue },
  ) => {
    const response = await apiCall(() =>
      merchantsService.update(payload.id, {
        ...        
      }),
    );

    if (!response.isSuccess) {
      return rejectWithValue(response.errorMessage);
    }

    return response.data as MerchantDetail;
  },
);
```

## Screen Handling Example

```text
const onSave = async () => {
  const trimmedNickname = nickname.trim();
  const action = await dispatch(
    updateMerchant({
      id: params.merchantId,
      ...
    }),
  );

  if (action.meta.requestStatus !== 'fulfilled') {
    Toast.show({
      type: 'error',
      text1: t('errors.title'),
      text2: action.payload as string,
    });
    return;
  }

  //Progress to next step
};
```

## Constants

All constants defined in dedicated constant files, never inline in components:

```text
export const ApiEndpoints = {
  Auth: {
    Login: '/api/v1/auth/login',
    Register: '/api/v1/auth/register',
  },
  Expenses: {
    Base: '/api/v1/expenses',
  },
};
```

## Error Display

- 400 errors: show the `errorMessage` from `ApiResponse` to the user
- 500 errors: show generic message ('Something went wrong, please try again')
- Display errors in-screen (inline message or toast) - do not use console.error in production

## Localization

- All user-facing strings must be localized
