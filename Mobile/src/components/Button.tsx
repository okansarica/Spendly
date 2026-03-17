// CHANGED_BY_AI: 2026-03-17 - useMemo for styles to avoid recreation on every render
import React, {useMemo} from 'react';
import {TouchableOpacity, Text, StyleSheet, ActivityIndicator, ViewStyle, TextStyle, StyleProp, View} from 'react-native';
import {useTheme} from '../theme/ThemeContext';

type ButtonVariant = 'primary' | 'secondary' | 'danger';

type ButtonProps = {
  text: string;
  onPress: () => void;
  variant?: ButtonVariant;
  isLoading?: boolean;
  disabled?: boolean;
  style?: StyleProp<ViewStyle>;
  textStyle?: StyleProp<TextStyle>;
  size?: 'small' | 'medium' | 'large';
};

export default function Button({
  text,
  onPress,
  variant = 'primary',
  isLoading = false,
  disabled = false,
  style,
  textStyle,
  size = 'medium',
}: ButtonProps) {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();

  const backgroundColor = useMemo(() => {
    if (disabled) return colors.buttonPrimaryDisabled;
    switch (variant) {
      case 'primary':
        return colors.buttonPrimary;
      case 'secondary':
        return colors.buttonSecondary;
      case 'danger':
        return colors.danger;
      default:
        return colors.buttonPrimary;
    }
  }, [disabled, variant, colors]);

  const textColor = useMemo(() => {
    switch (variant) {
      case 'primary':
        return colors.buttonPrimaryText;
      case 'secondary':
        return colors.buttonSecondaryText;
      case 'danger':
        return colors.dangerText;
      default:
        return colors.buttonPrimaryText;
    }
  }, [variant, colors]);

  const padding = size === 'small' ? spacing.sm : size === 'large' ? spacing.lg : spacing.md;
  const fontSize = size === 'small' ? fontSizes.sm : size === 'large' ? fontSizes.lg : fontSizes.md;
  const minHeight = size === 'small' ? 40 : size === 'large' ? 56 : 48;

  const s = useMemo(
    () =>
      StyleSheet.create({
        btn: {
          borderRadius: size === 'small' ? radius.sm : radius.md,
          padding,
          alignItems: 'center' as const,
          justifyContent: 'center' as const,
          backgroundColor,
          shadowColor: colors.cardShadow,
          shadowOffset: {width: 0, height: 2},
          shadowOpacity: 0.1,
          shadowRadius: 4,
          elevation: 2,
          minWidth: size === 'small' ? 88 : 120,
          minHeight,
        },
        btnContent: {
          position: 'relative' as const,
          alignItems: 'center' as const,
          justifyContent: 'center' as const,
        },
        btnText: {
          color: textColor,
          fontSize,
          fontWeight: fontWeights.semiBold,
          opacity: isLoading ? 0 : 1,
        },
        loadingIndicator: {
          position: 'absolute' as const,
        },
      }),
    [backgroundColor, textColor, colors.cardShadow, radius, padding, fontSize, fontWeights, minHeight, size, isLoading],
  );

  return (
    <TouchableOpacity
      style={[s.btn, style]}
      onPress={onPress}
      disabled={disabled || isLoading}
      accessibilityRole="button"
      accessibilityLabel={text}
      accessibilityState={{disabled: disabled || isLoading, busy: isLoading}}>
      <View style={s.btnContent}>
        <Text style={[s.btnText, textStyle]}>{text}</Text>
        {isLoading && <ActivityIndicator style={s.loadingIndicator} color={textColor} />}
      </View>
    </TouchableOpacity>
  );
}
