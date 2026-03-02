// CHANGED_BY_AI: 2026-03-02 - Add shared header component
import React from 'react';
import {SafeAreaView, StyleSheet, Text, TouchableOpacity, View} from 'react-native';
import Icon from 'react-native-vector-icons/MaterialIcons';
import {useNavigation} from '@react-navigation/native';
import {useTheme} from '../theme/ThemeContext';
import {HeaderConstants} from '../constants/headerConstants';

type HeaderProps = {
  title?: string;
  showBack?: boolean;
};

export default function Header({title, showBack}: HeaderProps) {
  const {colors, spacing, fontSizes, fontWeights} = useTheme();
  const navigation = useNavigation();
  const canGoBack = navigation.canGoBack();
  const shouldShowBack = showBack ?? canGoBack;

  const s = StyleSheet.create({
    safeArea: {backgroundColor: colors.buttonPrimary, width: '100%'},
    container: {
      flexDirection: 'row',
      alignItems: 'center',
      paddingHorizontal: spacing.md,
      paddingBottom: spacing.sm,
      height: spacing.xl + spacing.md,
      backgroundColor: colors.buttonPrimary,
      width: '100%',
    },
    backButton: {
      width: spacing.xl + spacing.sm,
      height: spacing.xl + spacing.sm,
      alignItems: 'center',
      justifyContent: 'center',
    },
    backPlaceholder: {
      width: spacing.xl + spacing.sm,
      height: spacing.xl + spacing.sm,
    },
    title: {
      flex: 1,
      textAlign: 'center',
      color: colors.buttonPrimaryText,
      fontSize: fontSizes.lg,
      fontWeight: fontWeights.semiBold,
    },
  });

  return (
    <SafeAreaView style={s.safeArea}>
      <View style={s.container}>
        {shouldShowBack ? (
          <TouchableOpacity style={s.backButton} onPress={() => navigation.goBack()}>
            <Icon name={HeaderConstants.BackIconName} size={fontSizes.xxl} color={colors.buttonPrimaryText} />
          </TouchableOpacity>
        ) : (
          <View style={s.backPlaceholder} />
        )}
        <Text style={s.title} numberOfLines={1}>
          {title ?? ''}
        </Text>
        <View style={s.backPlaceholder} />
      </View>
    </SafeAreaView>
  );
}
