// CHANGED_BY_AI: 2026-03-02 - Add reports reducer
// CHANGED_BY_AI: 2026-03-02 - Add homepage reducer
import {configureStore} from '@reduxjs/toolkit';
import authReducer from './authStore';
import homepageReducer from './homepageStore';
import reportsReducer from './reportsStore';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    homepage: homepageReducer,
    reports: reportsReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
