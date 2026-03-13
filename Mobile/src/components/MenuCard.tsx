// CHANGED_BY_AI: 2026-03-13 - Create reusable menu card component for consistent design
import React from 'react';
import {View, Text, StyleSheet, TouchableOpacity} from 'react-native';
import {useTheme} from '../theme/ThemeContext';
import Icon from 'react-native-vector-icons/MaterialIcons';

type MenuCardProps = {
  title: string;
  description: string;
  icon: string;
  onPress: () => void;
  leftBorderColor?: string;
  iconBackgroundColor?: string;
  showArrow?: boolean;
  minHeight?: number;
};

export default function MenuCard({
  title,
  description,
  icon,
  onPress,
  leftBorderColor,
  iconBackgroundColor,
  showArrow = true,
  minHeight = 140,
}: MenuCardProps) {
  const {colors, spacing, fontSizes, fontWeights, radius} = useTheme();

  const borderColor = leftBorderColor || colors.buttonPrimary;
  const iconBgColor = iconBackgroundColor || colors.buttonPrimary;

  const s = StyleSheet.create({
    card: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      marginBottom: spacing.lg,
      overflow: 'hidden',
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      borderLeftWidth: 6,
      borderLeftColor: borderColor,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.08,
      shadowRadius: 8,
      elevation: 3,
    },
    cardContent: {
      padding: spacing.lg,
      minHeight: minHeight,
    },
    cardHeader: {
      flexDirection: 'row',
      alignItems: 'center',
      marginBottom: spacing.md,
    },
    iconContainer: {
      width: 56,
      height: 56,
      borderRadius: 28,
      backgroundColor: iconBgColor,
      alignItems: 'center',
      justifyContent: 'center',
      marginRight: spacing.md,
    },
    textContent: {
      flex: 1,
    },
    title: {
      color: colors.textPrimary,
      fontSize: fontSizes.xl,
      fontWeight: fontWeights.bold,
      marginBottom: spacing.xs,
    },
    description: {
      color: colors.textSecondary,
      fontSize: fontSizes.sm,
      lineHeight: 20,
    },
    arrowContainer: {
      width: 32,
      height: 32,
      borderRadius: 16,
      backgroundColor: colors.buttonPrimary + '15',
      alignItems: 'center',
      justifyContent: 'center',
    },
  });

  return (
    <TouchableOpacity style={s.card} onPress={onPress} activeOpacity={0.7}>
      <View style={s.cardContent}>
        <View style={s.cardHeader}>
          <View style={s.iconContainer}>
            <Icon name={icon} size={28} color={colors.buttonPrimaryText} />
          </View>
          <View style={s.textContent}>
            <Text style={s.title}>{title}</Text>
          </View>
          {showArrow && (
            <View style={s.arrowContainer}>
              <Icon name="arrow-forward" size={20} color={colors.buttonPrimary} />
            </View>
          )}
        </View>
        <Text style={s.description}>{description}</Text>
      </View>
    </TouchableOpacity>
  );
}
