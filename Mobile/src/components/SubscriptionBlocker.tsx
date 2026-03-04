import React from 'react';
import {StyleSheet, Text, View, Modal, TouchableOpacity, Linking} from 'react-native';
import {useTheme} from '../theme/ThemeContext';
import {translate} from '../utils/translations';

type SubscriptionBlockerProps = {
  visible: boolean;
};

export default function SubscriptionBlocker({visible}: SubscriptionBlockerProps) {
  const {colors, spacing, fontSizes, fontWeights} = useTheme();

  const handleRenewPress = () => {
    Linking.openURL('https://spendly.com/subscribe');
  };

  const s = StyleSheet.create({
    overlay: {
      flex: 1,
      backgroundColor: 'rgba(0, 0, 0, 0.9)',
      justifyContent: 'center',
      alignItems: 'center',
      padding: spacing.lg,
    },
    container: {
      backgroundColor: colors.surface,
      borderRadius: spacing.sm,
      padding: spacing.xl,
      width: '100%',
      maxWidth: 400,
      alignItems: 'center',
    },
    title: {
      fontSize: fontSizes.xxl,
      fontWeight: fontWeights.bold,
      color: colors.error,
      marginBottom: spacing.md,
      textAlign: 'center',
    },
    message: {
      fontSize: fontSizes.md,
      color: colors.text,
      marginBottom: spacing.xl,
      textAlign: 'center',
      lineHeight: fontSizes.md * 1.5,
    },
    button: {
      backgroundColor: colors.buttonPrimary,
      paddingVertical: spacing.md,
      paddingHorizontal: spacing.xl,
      borderRadius: spacing.sm,
      width: '100%',
    },
    buttonText: {
      color: colors.buttonPrimaryText,
      fontSize: fontSizes.md,
      fontWeight: fontWeights.semiBold,
      textAlign: 'center',
    },
  });

  return (
    <Modal visible={visible} transparent animationType="fade" statusBarTranslucent>
      <View style={s.overlay}>
        <View style={s.container}>
          <Text style={s.title}>{translate('SubscriptionExpiredTitle')}</Text>
          <Text style={s.message}>{translate('SubscriptionExpiredMessage')}</Text>
          <TouchableOpacity style={s.button} onPress={handleRenewPress}>
            <Text style={s.buttonText}>{translate('RenewSubscription')}</Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  );
}

