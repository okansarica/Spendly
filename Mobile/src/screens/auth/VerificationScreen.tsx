// CHANGED_BY_AI: 2026-03-12 - Open payment browser after verifying paid registration flow
// CHANGED_BY_AI: 2026-03-12 - Keep header fixed at top regardless of short content height
// CHANGED_BY_AI: 2026-03-12 - Fix auth store error handling and show toast on thunk rejection
// CHANGED_BY_AI: 2026-03-02 - Add shared header usage
import React, {useState, useRef} from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
} from 'react-native';
import Toast from 'react-native-toast-message';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {verifyEmail, resendCode, activatePendingAuth} from '../../store/authStore';
import {useTheme} from '../../theme/ThemeContext';
import Header from '../../components/Header';
import {translate} from '../../utils/translations';
import InAppBrowser from 'react-native-inappbrowser-reborn';

export default function VerificationScreen() {
  const dispatch = useAppDispatch();
  const userId = useAppSelector(s => s.auth.userId);
  const isLoading = useAppSelector(s => s.auth.isLoading);
  const {colors, spacing, radius, fontSizes, fontWeights, linkColor} = useTheme();
  const [digits, setDigits] = useState(['', '', '', '']);
  const [isOpeningBrowser, setIsOpeningBrowser] = useState(false);
  const refs = [
    useRef<TextInput>(null),
    useRef<TextInput>(null),
    useRef<TextInput>(null),
    useRef<TextInput>(null),
  ];

  const isValid = digits.every(d => d.length === 1) && !!userId;

  const handleDigit = (value: string, index: number) => {
    const next = [...digits];
    next[index] = value.replace(/[^0-9]/g, '').slice(-1);
    setDigits(next);
    if (value && index < 3) {
      refs[index + 1].current?.focus();
    }
  };

  const handleKeyPress = (key: string, index: number) => {
    if (key === 'Backspace' && !digits[index] && index > 0) {
      refs[index - 1].current?.focus();
    }
  };

  const handleVerify = async () => {
    if (!userId || isOpeningBrowser) return;
    const result = await dispatch(verifyEmail({userId, code: digits.join('')}));
    if (verifyEmail.rejected.match(result)) {
      Toast.show({
        type: 'error',
        text1: translate('VerificationFailed'),
        text2: (result.payload as string) || translate('An unexpected error occurred'),
      });
      return;
    }

    const paymentUrl = result.payload.paymentUrl;
    if (paymentUrl && InAppBrowser && (await InAppBrowser.isAvailable())) {
      setIsOpeningBrowser(true);
      try {
        // Close any existing browser instance first
        await InAppBrowser.close();
      } catch (error) {
        // Ignore error if no browser was open
      }
      
      // Delay to allow navigation and cleanup
      setTimeout(async () => {
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
          
          // Browser closed (payment completed or user cancelled)
          // Activate pending tokens and login
          await dispatch(activatePendingAuth());
        } catch (error){
          console.log('Failed to open payment URL in browser', error);
          // Even on error, try to activate pending tokens
          await dispatch(activatePendingAuth());
        } finally {
          setIsOpeningBrowser(false);
        }
      }, 500);
    }
  };

  const handleResend = async () => {
    if (!userId) return;
    const result = await dispatch(resendCode(userId));
    if (resendCode.rejected.match(result)) {
      Toast.show({
        type: 'error',
        text1: translate('Error'),
        text2: (result.payload as string) || translate('An unexpected error occurred'),
      });
    }
  };

  const s = StyleSheet.create({
    container: {flex: 1, backgroundColor: colors.backgroundPrimary},
    content: {flex: 1, justifyContent: 'center', padding: spacing.lg},
    title: {
      fontSize: fontSizes.xl,
      fontWeight: fontWeights.semiBold,
      color: colors.textPrimary,
      textAlign: 'center',
      marginBottom: spacing.sm,
    },
    subtitle: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
      textAlign: 'center',
      marginBottom: spacing.lg,
    },
    codeRow: {
      flexDirection: 'row',
      justifyContent: 'center',
      gap: spacing.sm,
      marginBottom: spacing.lg,
    },
    codeInput: {
      width: 52,
      height: 60,
      borderWidth: 1,
      borderColor: colors.inputBorder,
      borderRadius: radius.md,
      textAlign: 'center',
      fontSize: fontSizes.xl,
      fontWeight: fontWeights.semiBold,
      color: colors.inputText,
      backgroundColor: colors.inputBackground,
    },
    btn: {
      borderRadius: radius.md,
      padding: spacing.md,
      alignItems: 'center',
      marginBottom: spacing.sm,
    },
    btnPrimary: {backgroundColor: isValid && !isLoading && !isOpeningBrowser ? colors.buttonPrimary : colors.buttonPrimaryDisabled},
    btnText: {color: colors.buttonPrimaryText, fontSize: fontSizes.md, fontWeight: fontWeights.semiBold},
    link: {
      color: linkColor,
      textAlign: 'center',
      marginTop: spacing.sm,
      fontSize: fontSizes.sm,
      fontWeight: fontWeights.medium,
    },
  });

  return (
    <View style={s.container}>
      <Header title={translate('VerificationTitle')} />
      <View style={s.content}>
        <Text style={s.title}>{translate('VerifyYourEmail')}</Text>
        <Text style={s.subtitle}>{translate('EnterVerificationCode')}</Text>


        <View style={s.codeRow}>
          {digits.map((d, i) => (
            <TextInput
              key={i}
              ref={refs[i]}
              style={s.codeInput}
              value={d}
              onChangeText={v => handleDigit(v, i)}
              onKeyPress={({nativeEvent}) => handleKeyPress(nativeEvent.key, i)}
              keyboardType="number-pad"
              maxLength={1}
              selectTextOnFocus
            />
          ))}
        </View>

        <TouchableOpacity style={[s.btn, s.btnPrimary]} onPress={handleVerify} disabled={!isValid || isLoading || isOpeningBrowser}>
          {isLoading ? <ActivityIndicator color={colors.buttonPrimaryText} /> : <Text style={s.btnText}>{translate('Verify')}</Text>}
        </TouchableOpacity>

        <TouchableOpacity onPress={handleResend} disabled={isLoading}>
          <Text style={s.link}>{translate('ResendCode')}</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}
