import React from 'react';
import {View, Text, StyleSheet} from 'react-native';
import Icon from 'react-native-vector-icons/MaterialIcons';
import {useTheme} from '../theme/ThemeContext';

type ErrorDisplayProps = {
  message: string;
};

export default function ErrorDisplay({message}: ErrorDisplayProps) {
  const {colors, spacing} = useTheme();

  const s = StyleSheet.create({
    container: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      padding: spacing.lg,
    },
    text: {
      color: colors.danger,
      marginTop: spacing.md,
      textAlign: 'center',
    },
  });

  return (
    <View style={s.container}>
      <Icon name="error-outline" size={48} color={colors.danger} />
      <Text style={s.text}>{message}</Text>
    </View>
  );
}

