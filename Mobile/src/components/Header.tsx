// CHANGED_BY_AI: 2026-03-05 - Add header right light/dark mode buttons
import React, {useCallback, useEffect, useState} from 'react';
import {SafeAreaView, StyleSheet, Text, TouchableOpacity, View} from 'react-native';
import Icon from 'react-native-vector-icons/MaterialIcons';
import {useNavigation} from '@react-navigation/native';
import {useTheme} from '../theme/ThemeContext';
import {HeaderConstants} from '../constants/headerConstants';
import {subscriptionService} from '../services/subscriptionService';
import {translate} from '../utils/translations';
import SubscriptionPlansModal from './SubscriptionPlansModal';

type HeaderProps = {
  title?: string;
  showBack?: boolean;
};

export default function Header({title, showBack}: HeaderProps) {
  const {colors, spacing, fontSizes, fontWeights, mode, setLightMode, setDarkMode} = useTheme();
  const toggleTheme = mode === 'light' ? setDarkMode : setLightMode;
  const toggleIconName = mode === 'light' ? HeaderConstants.DarkModeIconName : HeaderConstants.LightModeIconName;
  const navigation = useNavigation();
  const canGoBack = navigation.canGoBack();
  const shouldShowBack = showBack ?? canGoBack;
  const [showWarning, setShowWarning] = useState(false);
  const [timeUntilExpiration, setTimeUntilExpiration] = useState<{days: number; hours: number} | null>(null);
  const [showSubscriptionModal, setShowSubscriptionModal] = useState(false);

  const checkSubscription = useCallback(async () => {
    const isExpiring = await subscriptionService.isSubscriptionExpiring();
    const time = await subscriptionService.getTimeUntilExpiration();
    setShowWarning(isExpiring);
    setTimeUntilExpiration(time);
  }, []);

  useEffect(() => {
    checkSubscription();
  }, [checkSubscription]);

  useEffect(() => {
    return subscriptionService.subscribeToSubscriptionState(() => {
      checkSubscription();
    });
  }, [checkSubscription]);

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
    sideContainer: {
      width: spacing.xl * 2,
      height: spacing.xl + spacing.sm,
      justifyContent: 'center',
    },
    backButton: {
      width: spacing.xl + spacing.sm,
      height: spacing.xl + spacing.sm,
      alignItems: 'center',
      justifyContent: 'center',
    },
    rightActions: {
      width: spacing.xl * 2,
      height: spacing.xl + spacing.sm,
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'flex-end',
    },
    modeButton: {
      width: spacing.xl,
      height: spacing.xl,
      alignItems: 'center',
      justifyContent: 'center',
      borderRadius: spacing.sm,
      opacity: 0.75,
    },
    activeModeButton: {
      opacity: 1,
      backgroundColor: 'rgba(255,255,255,0.2)',
    },
    title: {
      flex: 1,
      textAlign: 'center',
      color: colors.buttonPrimaryText,
      fontSize: fontSizes.lg,
      fontWeight: fontWeights.semiBold,
    },
    warningBanner: {
      backgroundColor: '#FFA500',
      paddingVertical: spacing.sm,
      paddingHorizontal: spacing.md,
      width: '100%',
    },
    warningText: {
      color: '#FFFFFF',
      fontSize: fontSizes.sm,
      textAlign: 'center',
      fontWeight: fontWeights.semiBold,
    },
  });

  return (
    <>
      <SafeAreaView style={s.safeArea}>
        <View style={s.container}>
          {shouldShowBack ? (
            <View style={s.sideContainer}>
              <TouchableOpacity style={s.backButton} onPress={() => navigation.goBack()}>
                <Icon name={HeaderConstants.BackIconName} size={fontSizes.xxl} color={colors.buttonPrimaryText} />
              </TouchableOpacity>
            </View>
          ) : (
            <View style={s.sideContainer} />
          )}
          <Text style={s.title} numberOfLines={1}>
            {title ?? ''}
          </Text>
          <View style={s.rightActions}>
            <TouchableOpacity
              style={s.modeButton}
              onPress={toggleTheme}>
              <Icon name={toggleIconName} size={fontSizes.xl} color={colors.buttonPrimaryText} />
            </TouchableOpacity>
          </View>
        </View>
      </SafeAreaView>
      {showWarning && timeUntilExpiration !== null && (
        <TouchableOpacity style={s.warningBanner} onPress={() => setShowSubscriptionModal(true)}>
          <Text style={s.warningText}>
            {timeUntilExpiration.days === 0
              ? translate('SubscriptionExpiringWarningLastDay').replace('{hours}', timeUntilExpiration.hours.toString())
              : translate('SubscriptionExpiringWarning').replace('{days}', (timeUntilExpiration.hours>12?(timeUntilExpiration.days+1):timeUntilExpiration.days).toString())}
          </Text>
        </TouchableOpacity>
      )}
      <SubscriptionPlansModal
        visible={showSubscriptionModal}
        dismissible
        onClose={() => setShowSubscriptionModal(false)}
      />
    </>
  );
}
