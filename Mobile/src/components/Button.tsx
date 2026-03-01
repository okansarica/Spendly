import React from 'react';
import {TouchableOpacity, Text, StyleSheet, ActivityIndicator, ViewStyle, TextStyle} from 'react-native';
import {useTheme} from '../theme/ThemeContext';

type ButtonVariant = 'primary' | 'secondary' | 'danger';

type ButtonProps = {
  text: string;
  onPress: () => void;
  variant?: ButtonVariant;
  isLoading?: boolean;
  disabled?: boolean;
  style?: ViewStyle;
  textStyle?: TextStyle;
};

export default function Button({
  text,
  onPress,
  variant = 'primary',
  isLoading = false,
  disabled = false,
  style,
  textStyle,
}: ButtonProps) {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();

  const getBackgroundColor = () => {
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
  };

  const getTextColor = () => {
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
  };

  const s = StyleSheet.create({
    btn: {
      borderRadius: radius.md,
      padding: spacing.md,
      alignItems: 'center' as const,
      backgroundColor: getBackgroundColor(),
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.1,
      shadowRadius: 4,
      elevation: 2,
    },
    btnText: {
      color: getTextColor(),
      fontSize: fontSizes.md,
      fontWeight: fontWeights.semiBold,
    },
  });

  return (
    <TouchableOpacity style={[s.btn, style]} onPress={onPress} disabled={disabled || isLoading}>
      {isLoading ? <ActivityIndicator color={getTextColor()} /> : <Text style={[s.btnText, textStyle]}>{text}</Text>}
    </TouchableOpacity>
  );
}

