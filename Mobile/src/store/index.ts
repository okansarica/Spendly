// CHANGED_BY_AI: 2026-03-02 - Add homepage reducer
import {configureStore} from '@reduxjs/toolkit';
import authReducer from './authStore';
import homepageReducer from './homepageStore';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    homepage: homepageReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
