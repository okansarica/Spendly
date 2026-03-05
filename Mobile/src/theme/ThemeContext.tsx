// CHANGED_BY_AI: 2026-03-05 - Persist manual theme mode on device with system fallback
import React, {createContext, useContext, useEffect, useState} from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import {useColorScheme} from 'react-native';
import {lightTheme, darkTheme, AppTheme} from './theme';

type ThemeMode = 'light' | 'dark';

type ThemeContextValue = AppTheme & {
  mode: ThemeMode;
  setLightMode: () => void;
  setDarkMode: () => void;
};

const ThemeContext = createContext<ThemeContextValue>({
  ...lightTheme,
  mode: 'light',
  setLightMode: () => {},
  setDarkMode: () => {},
});

const THEME_MODE_STORAGE_KEY = 'theme_mode';

export function ThemeProvider({children}: {children: React.ReactNode}) {
  const scheme = useColorScheme();
  const [mode, setMode] = useState<ThemeMode>(scheme === 'dark' ? 'dark' : 'light');

  useEffect(() => {
    AsyncStorage.getItem(THEME_MODE_STORAGE_KEY).then(storedMode => {
      if (storedMode === 'light' || storedMode === 'dark') {
        setMode(storedMode);
      }
    });
  }, []);

  const setLightMode = () => {
    setMode('light');
    AsyncStorage.setItem(THEME_MODE_STORAGE_KEY, 'light');
  };

  const setDarkMode = () => {
    setMode('dark');
    AsyncStorage.setItem(THEME_MODE_STORAGE_KEY, 'dark');
  };

  const theme = mode === 'dark' ? darkTheme : lightTheme;

  return (
    <ThemeContext.Provider
      value={{
        ...theme,
        mode,
        setLightMode,
        setDarkMode,
      }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme(): ThemeContextValue {
  return useContext(ThemeContext);
}
