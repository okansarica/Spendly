// CHANGED_BY_AI: 2026-03-11 - Extract startup version update gate screen from App.tsx
import React from 'react';
import {StyleSheet, Text, View} from 'react-native';
import {useTheme} from '../theme/ThemeContext';
import {translate} from '../utils/translations';
import Button from './Button';

type VersionUpdateScreenProps = {
  forceUpdate: boolean;
  message?: string;
  onUpdate: () => void;
  onSkip: () => void;
};

export default function VersionUpdateScreen({
  forceUpdate,
  message,
  onUpdate,
  onSkip,
}: VersionUpdateScreenProps) {
  const {colors, spacing, fontSizes, fontWeights, radius} = useTheme();

  const title = forceUpdate ? translate('ForceUpdateTitle') : translate('OptionalUpdateTitle');
  const defaultMessage = forceUpdate ? translate('ForceUpdateMessage') : translate('OptionalUpdateMessage');
  const description = message?.trim() ? message : defaultMessage;

  const s = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.backgroundPrimary,
      justifyContent: 'center',
      paddingHorizontal: spacing.lg,
    },
    card: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      padding: spacing.lg,
      borderColor: colors.borderSubtle,
      borderWidth: 1,
      gap: spacing.md,
    },
    title: {
      fontSize: fontSizes.xl,
      fontWeight: fontWeights.bold,
      color: colors.textPrimary,
      textAlign: 'center',
    },
    message: {
      fontSize: fontSizes.md,
      color: colors.textSecondary,
      textAlign: 'center',
    },
    buttonRow: {
      marginTop: spacing.sm,
      flexDirection: forceUpdate ? 'column' : 'row',
      gap: spacing.sm,
    },
    button: {
      flex: 1,
    },
  });

  return (
    <View style={s.container}>
      <View style={s.card}>
        <Text style={s.title}>{title}</Text>
        <Text style={s.message}>{description}</Text>
        <View style={s.buttonRow}>
          <Button text={translate('UpdateNow')} onPress={onUpdate} style={s.button} />
          {!forceUpdate ? (
            <Button
              text={translate('SkipForNow')}
              onPress={onSkip}
              variant="secondary"
              style={s.button}
            />
          ) : null}
        </View>
      </View>
    </View>
  );
}
