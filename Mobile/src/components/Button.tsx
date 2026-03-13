import React from 'react';
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

  const getPadding = () => {
    switch (size) {
      case 'small':
        return spacing.sm;
      case 'large':
        return spacing.lg;
      default:
        return spacing.md;
    }
  };

  const getFontSize = () => {
    switch (size) {
      case 'small':
        return fontSizes.sm;
      case 'large':
        return fontSizes.lg;
      default:
        return fontSizes.md;
    }
  };

  const getMinHeight = () => {
    switch (size) {
      case 'small':
        return 40;
      case 'large':
        return 56;
      default:
        return 48;
    }
  };

  const s = StyleSheet.create({
    btn: {
      borderRadius: size === 'small' ? radius.sm : radius.md,
      padding: getPadding(),
      alignItems: 'center' as const,
      justifyContent: 'center' as const,
      backgroundColor: getBackgroundColor(),
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.1,
      shadowRadius: 4,
      elevation: 2,
      minWidth: size === 'small' ? 88 : 120,
      minHeight: getMinHeight(),
    },
    btnContent: {
      position: 'relative' as const,
      alignItems: 'center' as const,
      justifyContent: 'center' as const,
    },
    btnText: {
      color: getTextColor(),
      fontSize: getFontSize(),
      fontWeight: fontWeights.semiBold,
      opacity: isLoading ? 0 : 1,
    },
    loadingIndicator: {
      position: 'absolute' as const,
    },
  });

  return (
    <TouchableOpacity style={[s.btn, style]} onPress={onPress} disabled={disabled || isLoading}>
      <View style={s.btnContent}>
        <Text style={[s.btnText, textStyle]}>{text}</Text>
        {isLoading && <ActivityIndicator style={s.loadingIndicator} color={getTextColor()} />}
      </View>
    </TouchableOpacity>
  );
}
