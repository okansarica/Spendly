import {lightColors, darkColors} from './colors';

export const spacing = {
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
} as const;

export const radius = {
  sm: 6,
  md: 10,
  lg: 16,
} as const;

export const fontSizes = {
  xs: 11,
  sm: 13,
  md: 15,
  lg: 17,
  xl: 20,
  xxl: 26,
} as const;

export const fontWeights = {
  regular: '400' as const,
  medium: '500' as const,
  semiBold: '600' as const,
  bold: '700' as const,
};

export const lineHeights = {
  tight: 18,
  normal: 22,
  relaxed: 26,
} as const;

export const lightTheme = {
  colors: lightColors,
  spacing,
  radius,
  fontSizes,
  fontWeights,
  lineHeights,
};

export const darkTheme = {
  ...lightTheme,
  colors: darkColors,
};

export type AppTheme = typeof lightTheme;

