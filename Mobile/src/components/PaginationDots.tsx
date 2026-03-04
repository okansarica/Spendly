import React from 'react';
import {View, StyleSheet} from 'react-native';
import {useTheme} from '../theme/ThemeContext';

interface PaginationDotsProps {
  totalPages: number;
  activePage: number;
}

export default function PaginationDots({totalPages, activePage}: PaginationDotsProps) {
  const {colors, spacing} = useTheme();

  const s = StyleSheet.create({
    container: {
      flexDirection: 'row',
      justifyContent: 'center',
      alignItems: 'center',
      marginTop: spacing.md,
      gap: spacing.xs,
    },
    dot: {
      width: 8,
      height: 8,
      borderRadius: 4,
      backgroundColor: colors.borderSubtle,
    },
    dotActive: {
      backgroundColor: colors.buttonPrimary,
    },
  });

  return (
    <View style={s.container}>
      {Array.from({length: totalPages}).map((_, index) => (
        <View key={index} style={[s.dot, activePage === index && s.dotActive]} />
      ))}
    </View>
  );
}

