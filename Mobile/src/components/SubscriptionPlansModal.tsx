// CHANGED_BY_AI: 2026-03-22 - Use DurationType and SubscriptionType enums throughout, remove hardcoded string literals
import React, { useEffect, useState } from 'react';
import { ActivityIndicator, Modal, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { useTheme } from '../theme/ThemeContext';
import { translate } from '../utils/translations';
import InAppBrowser from 'react-native-inappbrowser-reborn';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { createPaymentUrl, SubscriptionPlan } from '../store/subscriptionStore';
import Toast from 'react-native-toast-message';
import { DurationType, SubscriptionType } from "../services/authService.ts";

type SubscriptionPlansModalProps = {
  visible: boolean;
  dismissible?: boolean;
  onClose?: () => void;
  onPlanSelected?: (subscriptionType:SubscriptionType, duration?: DurationType) => Promise<void> | void;
};

export default function SubscriptionPlansModal({
  visible,
  dismissible = true,
  onClose,
  onPlanSelected,
}: SubscriptionPlansModalProps) {
  const {colors, spacing, fontSizes, fontWeights, mode} = useTheme();
  const dispatch = useAppDispatch();
  const {isProcessing} = useAppSelector(state => state.subscription);
  
  const displayedPlans: SubscriptionPlan[] = [
    {subscriptionType: SubscriptionType.Free, price: 0},
    {subscriptionType: SubscriptionType.Plus, duration: DurationType.Monthly, price: 3.99},
    {subscriptionType: SubscriptionType.Plus, duration: DurationType.Yearly, price: 39.99},
  ];
  
  const [selectedPlan, setSelectedPlan] = useState<SubscriptionPlan>({subscriptionType: SubscriptionType.Plus, duration: DurationType.Yearly, price: 39.99});
  const [isSubmittingSelection, setIsSubmittingSelection] = useState(false);

  useEffect(() => {
    if (visible) {
      setSelectedPlan({subscriptionType: SubscriptionType.Plus, duration: DurationType.Yearly, price: 39.99});
    }
  }, [visible]);

  const closeModal = () => {
    if (!dismissible) {
      return;
    }
    onClose?.();
  };

  const titleKey = dismissible ? 'SubscriptionUpgradeTitle' : 'SubscriptionExpiredTitle';
  const messageKey = dismissible ? 'SubscriptionUpgradeMessage' : 'SubscriptionExpiredMessage';
  const defaultButtonKey = dismissible ? 'SubscriptionUpgradeAction' : 'RenewSubscription';
  const buttonKey = selectedPlan.subscriptionType === SubscriptionType.Free ? 'Continue' : defaultButtonKey;
  const isBusy = isProcessing || isSubmittingSelection;
  const isActionDisabled = isBusy;

  const handleRenewPress = async () => {
    if (onPlanSelected) {
      try {
        setIsSubmittingSelection(true);
        await onPlanSelected(selectedPlan.subscriptionType, selectedPlan.duration ?? undefined);
      } finally {
        setIsSubmittingSelection(false);
      }
      return;
    }

    if (selectedPlan.subscriptionType === SubscriptionType.Free) {
      return;
    }

    const result = await dispatch(createPaymentUrl({
      subscriptionType: selectedPlan.subscriptionType,
      duration: selectedPlan.duration ?? undefined
    }));
    if (createPaymentUrl.fulfilled.match(result)) {
      const paymentUrl = result.payload;
      if (InAppBrowser && (await InAppBrowser.isAvailable())) {        
        
        try {
            await InAppBrowser.openAuth(paymentUrl, paymentUrl,{
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
            
        } catch (error) {
            console.log('Error navigating to Payment Url', error);
        }
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
      shadowColor: colors.cardShadow,
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
      fontSize: fontSizes.xxxl,
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
            {displayedPlans.map(plan => {
              const isFree = plan.subscriptionType === SubscriptionType.Free;
              const isYearly = plan.duration === DurationType.Yearly;
              const isSelected = selectedPlan.subscriptionType === plan.subscriptionType && selectedPlan.duration === plan.duration;
              const planKey = plan.duration ? `${plan.subscriptionType}${plan.duration}` : plan.subscriptionType;

              if (isFree) {
                return (
                  <TouchableOpacity
                    key={planKey}
                    style={[s.planCard, isSelected && s.planCardSelected]}
                    onPress={() => setSelectedPlan({subscriptionType: plan.subscriptionType, price: plan.price})}
                    activeOpacity={0.7}>
                    {isSelected && (
                      <View style={s.checkIcon}>
                        <Text style={s.checkIconText}>✓</Text>
                      </View>
                    )}
                    <View style={s.planHeader}>
                      <View style={s.planLeft}>
                        <Text style={s.planName}>{translate('FreePlanName')}</Text>
                        <Text style={s.planPrice}>£0.00</Text>
                        <Text style={s.trialDescription}>{translate('FreePlanDescription')}</Text>
                      </View>
                    </View>
                  </TouchableOpacity>
                );
              }

              const monthlyPrice = 3.99;
              const savings = isYearly ? ((monthlyPrice * 12 - plan.price) / (monthlyPrice * 12) * 100).toFixed(0) : null;
              const durationText = isYearly ? translate('year') : translate('month');

              return (
                <TouchableOpacity
                  key={planKey}
                  style={[s.planCard, isSelected && s.planCardSelected]}
                  onPress={() => setSelectedPlan({subscriptionType: plan.subscriptionType, duration: plan.duration, price: plan.price})}
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
                      <Text style={s.planName}>
                        {translate(plan.duration!.toString())}
                      </Text>
                      <Text style={s.planPrice}>£{plan.price.toFixed(2)}/{durationText}</Text>
                      {savings && <Text style={s.savingsText}>{translate('SavePercentage').replace('{percentage}', savings)}</Text>}
                    </View>
                  </View>
                </TouchableOpacity>
              );
            })}
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
