// CHANGED_BY_AI: 2026-03-02 - Add reports reducer
// CHANGED_BY_AI: 2026-03-02 - Add homepage reducer
// CHANGED_BY_AI: 2026-03-02 - Add finance reducers
// CHANGED_BY_AI: 2026-03-03 - Add user reducer
import {configureStore} from '@reduxjs/toolkit';
import authReducer from './authStore';
import homepageReducer from './homepageStore';
import reportsReducer from './reportsStore';
import categoriesReducer from './categoriesStore';
import merchantsReducer from './merchantsStore';
import banksReducer from './banksStore';
import userReducer from './userStore';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    homepage: homepageReducer,
    reports: reportsReducer,
    categories: categoriesReducer,
    merchants: merchantsReducer,
    banks: banksReducer,
    user: userReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
