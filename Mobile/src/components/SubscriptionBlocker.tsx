import React, {useEffect, useState} from 'react';
import {StyleSheet, Text, View, Modal, TouchableOpacity, ActivityIndicator, Alert} from 'react-native';
import {useTheme} from '../theme/ThemeContext';
import {translate} from '../utils/translations';
import {subscriptionService} from '../services/subscriptionService';
import InAppBrowser from 'react-native-inappbrowser-reborn';

type SubscriptionBlockerProps = {
  visible: boolean;
};

type PlanType = 'Monthly' | 'Yearly';

export default function SubscriptionBlocker({visible}: SubscriptionBlockerProps) {
  const {colors, spacing, fontSizes, fontWeights} = useTheme();
  const [plans, setPlans] = useState<{planType: PlanType; price: number}[]>([]);
  const [selectedPlan, setSelectedPlan] = useState<PlanType>('Yearly');
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);

  useEffect(() => {
    if (visible) {
      loadPlans();
    }
  }, [visible]);

  const loadPlans = async () => {
    try {
      setLoading(true);
      const fetchedPlans = await subscriptionService.fetchSubscriptionPlans();
      setPlans(fetchedPlans);
      setLoading(false);
    } catch (error) {
      setLoading(false);
      Alert.alert('Error', 'Failed to load subscription plans');
    }
  };

  const handleRenewPress = async () => {
    try {
      setProcessing(true);
      const paymentUrl = await subscriptionService.createPaymentUrl(selectedPlan);
      
      if (InAppBrowser && await InAppBrowser.isAvailable()) {
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
      }
      
      setProcessing(false);
    } catch (error) {
      console.log(error);
      setProcessing(false);
      Alert.alert('Error', 'Failed to open payment page');
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
    },
    headerIcon: {
      width: 64,
      height: 64,
      borderRadius: 32,
      backgroundColor: colors.buttonPrimary + '15',
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
      backgroundColor: colors.backgroundPrimary,
      position: 'relative',
      overflow: 'visible',
    },
    planCardSelected: {
      borderColor: colors.buttonPrimary,
      backgroundColor: colors.buttonPrimary + '08',
      shadowColor: colors.buttonPrimary,
      shadowOffset: {width: 0, height: 4},
      shadowOpacity: 0.3,
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
      color: colors.buttonPrimary,
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
    checkIcon: {
      position: 'absolute',
      top: spacing.md,
      right: spacing.md,
      width: 24,
      height: 24,
      borderRadius: 12,
      backgroundColor: colors.buttonPrimary,
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
    features: {
      marginTop: spacing.md,
      gap: spacing.xs,
    },
    feature: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: spacing.sm,
    },
    featureIcon: {
      fontSize: 14,
      color: colors.success,
    },
    featureText: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
    },
  });

  return (
    <Modal visible={visible} transparent animationType="fade" statusBarTranslucent>
      <View style={s.overlay}>
        <View style={s.container}>
          <View style={s.headerIcon}>
            <Text style={s.iconText}>💎</Text>
          </View>
          <Text style={s.title}>{translate('SubscriptionExpiredTitle')}</Text>
          <Text style={s.subtitle}>{translate('SubscriptionExpiredMessage')}</Text>

          {loading ? (
            <ActivityIndicator size="large" color={colors.buttonPrimary} style={s.loader} />
          ) : (
            <>
              <View style={s.plansContainer}>
                {plans.map(plan => {
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
                          <Text style={s.badgeText}>Best Value</Text>
                        </View>
                      )}
                      {isSelected && (
                        <View style={s.checkIcon}>
                          <Text style={s.checkIconText}>✓</Text>
                        </View>
                      )}
                      <View style={s.planHeader}>
                        <View style={s.planLeft}>
                          <Text style={s.planName}>{plan.planType} Plan</Text>
                          <Text style={s.planPrice}>£{plan.price.toFixed(2)}</Text>
                          <Text style={s.planPeriod}>£{monthlyCost}/month</Text>
                          {savings && (
                            <Text style={s.savingsText}>Save {savings}%</Text>
                          )}
                        </View>
                      </View>
                    </TouchableOpacity>
                  );
                })}
              </View>

              <TouchableOpacity
                style={[s.button, (processing || loading) && s.buttonDisabled]}
                onPress={handleRenewPress}
                disabled={processing || loading}>
                {processing ? (
                  <ActivityIndicator size="small" color={colors.buttonPrimaryText} />
                ) : (
                  <Text style={s.buttonText}>{translate('RenewSubscription')}</Text>
                )}
              </TouchableOpacity>
            </>
          )}
        </View>
      </View>
    </Modal>
  );
}

