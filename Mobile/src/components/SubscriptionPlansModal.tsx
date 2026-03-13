// CHANGED_BY_AI: 2026-03-12 - Ensure registration modal always shows all 3 plan options
// CHANGED_BY_AI: 2026-03-12 - Add reusable trial selection mode for registration flow
// CHANGED_BY_AI: 2026-03-12 - Harmonize plan selection colors in dark mode
// CHANGED_BY_AI: 2026-03-12 - Improve diamond icon background visibility in dark mode
// CHANGED_BY_AI: 2026-03-12 - Improve modal close button visibility in light mode
// CHANGED_BY_AI: 2026-03-12 - Use different subscription modal copy for dismissible vs non-dismissible states
import React, {useEffect, useState} from 'react';
import {StyleSheet, Text, View, Modal, TouchableOpacity, ActivityIndicator} from 'react-native';
import {useTheme} from '../theme/ThemeContext';
import {translate} from '../utils/translations';
import InAppBrowser from 'react-native-inappbrowser-reborn';
import {useAppDispatch, useAppSelector} from '../store/hooks';
import {fetchSubscriptionPlans, createPaymentUrl} from '../store/subscriptionStore';
import Toast from 'react-native-toast-message';

type PlanType = 'Trial' | 'Monthly' | 'Yearly';
type PaidPlan = {planType: 'Monthly' | 'Yearly'; price: number};

type SubscriptionPlansModalProps = {
  visible: boolean;
  dismissible?: boolean;
  onClose?: () => void;
  includeTrialOption?: boolean;
  onPlanSelected?: (planType: PlanType) => Promise<void> | void;
};

export default function SubscriptionPlansModal({
  visible,
  dismissible = true,
  onClose,
  includeTrialOption = false,
  onPlanSelected,
}: SubscriptionPlansModalProps) {
  const {colors, spacing, fontSizes, fontWeights, mode} = useTheme();
  const dispatch = useAppDispatch();
  const {plans, isLoading, isProcessing, error} = useAppSelector(state => state.subscription);
  const publicFallbackPlans: PaidPlan[] = [
    {planType: 'Monthly', price: 6.99},
    {planType: 'Yearly', price: 69.99},
  ];
  const displayedPlans: PaidPlan[] = plans.length > 0 ? plans : includeTrialOption ? publicFallbackPlans : [];
  const [selectedPlan, setSelectedPlan] = useState<PlanType>('Yearly');
  const [isSubmittingSelection, setIsSubmittingSelection] = useState(false);

  useEffect(() => {
    if (visible) {
      if (!includeTrialOption) {
        dispatch(fetchSubscriptionPlans());
      }
      setSelectedPlan('Yearly');
    }
  }, [visible, dispatch, includeTrialOption]);

  useEffect(() => {
    if (error && !includeTrialOption) {
      Toast.show({
        type: 'error',
        text1: translate('Error'),
        text2: error,
      });
    }
  }, [error, includeTrialOption]);

  const closeModal = () => {
    if (!dismissible) {
      return;
    }
    onClose?.();
  };

  const titleKey = dismissible ? 'SubscriptionUpgradeTitle' : 'SubscriptionExpiredTitle';
  const messageKey = dismissible ? 'SubscriptionUpgradeMessage' : 'SubscriptionExpiredMessage';
  const defaultButtonKey = dismissible ? 'SubscriptionUpgradeAction' : 'RenewSubscription';
  const buttonKey = includeTrialOption && selectedPlan === 'Trial' ? 'Continue' : defaultButtonKey;
  const isBusy = isProcessing || isSubmittingSelection;
  const isActionDisabled = selectedPlan === 'Trial' ? isSubmittingSelection : isBusy || (!includeTrialOption && isLoading);

  const handleRenewPress = async () => {
    if (onPlanSelected) {
      try {
        setIsSubmittingSelection(true);
        await onPlanSelected(selectedPlan);
      } finally {
        setIsSubmittingSelection(false);
      }
      return;
    }

    if (selectedPlan === 'Trial') {
      return;
    }

    const result = await dispatch(createPaymentUrl(selectedPlan));
    if (createPaymentUrl.fulfilled.match(result)) {
      const paymentUrl = result.payload;
      if (InAppBrowser && (await InAppBrowser.isAvailable())) {        
        
        try {
          await InAppBrowser.open(paymentUrl, {
            dismissButtonStyle: 'close',
            preferredBarTintColor: colors.backgroundPrimary,
            preferredControlTintColor: colors.textPrimary,
            readerMode: false,
            animated: true,
            modalPresentationStyle: 'pageSheet',
            modalTransitionStyle: 'coverVertical',
            modalEnabled: true,
            enableBarCollapsing: false,
          });
        } catch {}
      }
    } else if (createPaymentUrl.rejected.match(result)) {
      Toast.show({
        type: 'error',
        text1: translate('Error'),
        text2: result.payload as string,
      });
    }
  };

  const s = StyleSheet.create({
    overlay: {
      flex: 1,
      backgroundColor: 'rgba(0, 0, 0, 0.85)',
      justifyContent: 'center',
      alignItems: 'center',
      padding: spacing.lg,
    },
    container: {
      backgroundColor: colors.cardBackground,
      borderRadius: 24,
      padding: spacing.xl,
      width: '100%',
      maxWidth: 400,
      shadowColor: '#000',
      shadowOffset: {width: 0, height: 12},
      shadowOpacity: 0.6,
      shadowRadius: 24,
      elevation: 20,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      position: 'relative',
    },
    closeButton: {
      position: 'absolute',
      top: spacing.md,
      right: spacing.md,
      zIndex: 1,
      width: 28,
      height: 28,
      borderRadius: 14,
      alignItems: 'center',
      justifyContent: 'center',
      backgroundColor: colors.backgroundSecondary,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 1},
      shadowOpacity: 0.18,
      shadowRadius: 2,
      elevation: 2,
    },
    closeText: {
      color: colors.textSecondary,
      fontSize: fontSizes.lg,
      fontWeight: fontWeights.bold,
      lineHeight: fontSizes.lg,
    },
    headerIcon: {
      width: 64,
      height: 64,
      borderRadius: 32,
      backgroundColor: mode === 'dark' ? colors.buttonPrimary + '33' : colors.buttonPrimary + '15',
      alignItems: 'center',
      justifyContent: 'center',
      alignSelf: 'center',
      marginBottom: spacing.md,
    },
    iconText: {
      fontSize: 32,
      color: colors.buttonPrimary,
    },
    title: {
      fontSize: fontSizes.xxl + 2,
      fontWeight: fontWeights.bold,
      color: colors.textPrimary,
      marginBottom: spacing.sm,
      textAlign: 'center',
      letterSpacing: -0.5,
    },
    subtitle: {
      fontSize: fontSizes.md,
      color: colors.textSecondary,
      marginBottom: spacing.xl,
      textAlign: 'center',
      lineHeight: fontSizes.md * 1.6,
    },
    plansContainer: {
      gap: spacing.md,
      marginBottom: spacing.xl,
    },
    planCard: {
      borderWidth: 2.5,
      borderColor: colors.borderSubtle,
      borderRadius: 16,
      padding: spacing.lg,
      backgroundColor: mode === 'dark' ? colors.backgroundSecondary : colors.backgroundPrimary,
      position: 'relative',
      overflow: 'visible',
    },
    planCardSelected: {
      borderColor: colors.buttonPrimary,
      backgroundColor: mode === 'dark' ? colors.buttonPrimary + '0D' : colors.buttonPrimary + '08',
      shadowColor: colors.buttonPrimary,
      shadowOffset: {width: 0, height: 4},
      shadowOpacity: mode === 'dark' ? 0.22 : 0.3,
      shadowRadius: 12,
      elevation: 8,
    },
    planHeader: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'flex-start',
      marginBottom: spacing.xs,
    },
    planLeft: {
      flex: 1,
    },
    planName: {
      fontSize: fontSizes.lg + 1,
      fontWeight: fontWeights.bold,
      color: colors.textPrimary,
      marginBottom: spacing.xs,
    },
    planPrice: {
      fontSize: fontSizes.xl,
      fontWeight: fontWeights.bold,
      color: mode === 'dark' ? colors.textPrimary : colors.buttonPrimary,
      marginBottom: 2,
    },
    planPeriod: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
    },
    badge: {
      backgroundColor: colors.success,
      paddingHorizontal: spacing.md,
      paddingVertical: 6,
      borderRadius: 20,
      position: 'absolute',
      top: -10,
      right: spacing.md,
      shadowColor: colors.success,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.4,
      shadowRadius: 4,
      elevation: 4,
    },
    badgeText: {
      color: '#fff',
      fontSize: fontSizes.xs,
      fontWeight: fontWeights.bold,
      letterSpacing: 0.5,
      textTransform: 'uppercase',
    },
    savingsText: {
      fontSize: fontSizes.sm,
      color: colors.success,
      fontWeight: fontWeights.semiBold,
      marginTop: spacing.xs,
    },
    trialDescription: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
      marginTop: spacing.xs,
      lineHeight: fontSizes.sm * 1.5,
    },
    checkIcon: {
      position: 'absolute',
      top: spacing.md,
      right: spacing.md,
      width: 24,
      height: 24,
      borderRadius: 12,
      backgroundColor: colors.buttonPrimary,
      borderWidth: mode === 'dark' ? 1 : 0,
      borderColor: mode === 'dark' ? colors.borderSubtle : colors.buttonPrimary,
      alignItems: 'center',
      justifyContent: 'center',
    },
    checkIconText: {
      color: '#fff',
      fontSize: 16,
      fontWeight: fontWeights.bold,
    },
    button: {
      backgroundColor: colors.buttonPrimary,
      paddingVertical: spacing.md + 2,
      paddingHorizontal: spacing.xl,
      borderRadius: 14,
      alignItems: 'center',
      shadowColor: colors.buttonPrimary,
      shadowOffset: {width: 0, height: 6},
      shadowOpacity: 0.4,
      shadowRadius: 12,
      elevation: 8,
    },
    buttonDisabled: {
      opacity: 0.6,
    },
    buttonText: {
      color: colors.buttonPrimaryText,
      fontSize: fontSizes.lg,
      fontWeight: fontWeights.bold,
      letterSpacing: 0.3,
    },
    loader: {
      marginVertical: spacing.xl * 1.5,
    },
  });

  return (
    <Modal
      visible={visible}
      transparent
      animationType="fade"
      statusBarTranslucent
      onRequestClose={closeModal}>
      <TouchableOpacity
        activeOpacity={1}
        style={s.overlay}
        onPress={closeModal}
        disabled={!dismissible || isBusy}>
        <TouchableOpacity activeOpacity={1} style={s.container} onPress={() => undefined}>
          {dismissible && (
            <TouchableOpacity style={s.closeButton} onPress={closeModal} disabled={isBusy}>
              <Text style={s.closeText}>×</Text>
            </TouchableOpacity>
          )}

          <View style={s.headerIcon}>
            <Text style={s.iconText}>💎</Text>
          </View>
          <Text style={s.title}>{translate(titleKey)}</Text>
          <Text style={s.subtitle}>{translate(messageKey)}</Text>

          <View style={s.plansContainer}>
            {includeTrialOption && (
              <TouchableOpacity
                style={[s.planCard, selectedPlan === 'Trial' && s.planCardSelected]}
                onPress={() => setSelectedPlan('Trial')}
                activeOpacity={0.7}>
                {selectedPlan === 'Trial' && (
                  <View style={s.checkIcon}>
                    <Text style={s.checkIconText}>✓</Text>
                  </View>
                )}
                <View style={s.planHeader}>
                  <View style={s.planLeft}>
                    <Text style={s.planName}>{translate('TrialPlanName')}</Text>
                    <Text style={s.planPrice}>£0.00</Text>
                    <Text style={s.trialDescription}>{translate('TrialPlanDescription')}</Text>
                  </View>
                </View>
              </TouchableOpacity>
            )}

            {!includeTrialOption && isLoading ? (
              <ActivityIndicator size="large" color={colors.buttonPrimary} style={s.loader} />
            ) : (
              displayedPlans.map(plan => {
                const isYearly = plan.planType === 'Yearly';
                const isSelected = selectedPlan === plan.planType;
                const monthlyCost = isYearly ? (plan.price / 12).toFixed(2) : plan.price.toFixed(2);
                const savings = isYearly ? ((3.99 * 12 - plan.price) / (3.99 * 12) * 100).toFixed(0) : null;

                return (
                  <TouchableOpacity
                    key={plan.planType}
                    style={[s.planCard, isSelected && s.planCardSelected]}
                    onPress={() => setSelectedPlan(plan.planType)}
                    activeOpacity={0.7}>
                    {isYearly && (
                      <View style={s.badge}>
                        <Text style={s.badgeText}>{translate('BestValue')}</Text>
                      </View>
                    )}
                    {isSelected && (
                      <View style={s.checkIcon}>
                        <Text style={s.checkIconText}>✓</Text>
                      </View>
                    )}
                    <View style={s.planHeader}>
                      <View style={s.planLeft}>
                        <Text style={s.planName}>{translate('PlanLabel').replace('{planType}', plan.planType)}</Text>
                        <Text style={s.planPrice}>£{plan.price.toFixed(2)}</Text>
                        <Text style={s.planPeriod}>£{monthlyCost}/month</Text>
                        {savings && <Text style={s.savingsText}>{translate('SavePercentage').replace('{percentage}', savings)}</Text>}
                      </View>
                    </View>
                  </TouchableOpacity>
                );
              })
            )}
          </View>

          <TouchableOpacity
            style={[s.button, isActionDisabled && s.buttonDisabled]}
            onPress={handleRenewPress}
            disabled={isActionDisabled}>
            {isBusy ? (
              <ActivityIndicator size="small" color={colors.buttonPrimaryText} />
            ) : (
              <Text style={s.buttonText}>{translate(buttonKey)}</Text>
            )}
          </TouchableOpacity>
        </TouchableOpacity>
      </TouchableOpacity>
    </Modal>
  );
}
