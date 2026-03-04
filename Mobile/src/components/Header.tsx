// CHANGED_BY_AI: 2026-03-02 - Add shared header component
import React, {useEffect, useState} from 'react';
import {SafeAreaView, StyleSheet, Text, TouchableOpacity, View} from 'react-native';
import Icon from 'react-native-vector-icons/MaterialIcons';
import {useNavigation} from '@react-navigation/native';
import {useTheme} from '../theme/ThemeContext';
import {HeaderConstants} from '../constants/headerConstants';
import {subscriptionService} from '../services/subscriptionService';
import {translate} from '../utils/translations';

type HeaderProps = {
  title?: string;
  showBack?: boolean;
};

export default function Header({title, showBack}: HeaderProps) {
  const {colors, spacing, fontSizes, fontWeights} = useTheme();
  const navigation = useNavigation();
  const canGoBack = navigation.canGoBack();
  const shouldShowBack = showBack ?? canGoBack;
  const [showWarning, setShowWarning] = useState(false);
  const [timeUntilExpiration, setTimeUntilExpiration] = useState<{days: number; hours: number} | null>(null);

  useEffect(() => {
    const checkSubscription = async () => {
      const isExpiring = await subscriptionService.isSubscriptionExpiring();
      const time = await subscriptionService.getTimeUntilExpiration();
      setShowWarning(isExpiring);
      setTimeUntilExpiration(time);
    };
    checkSubscription();
  }, []);

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
      {showWarning && timeUntilExpiration !== null && (
        <View style={s.warningBanner}>
          <Text style={s.warningText}>
            {timeUntilExpiration.days === 0
              ? translate('SubscriptionExpiringWarningLastDay').replace('{hours}', timeUntilExpiration.hours.toString())
              : translate('SubscriptionExpiringWarning').replace('{days}', (timeUntilExpiration.hours>12?(timeUntilExpiration.days+1):timeUntilExpiration.days).toString())}
          </Text>
        </View>
      )}
    </>
  );
}
