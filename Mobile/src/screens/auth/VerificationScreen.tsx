// CHANGED_BY_AI: 2026-03-02 - Add shared header usage
import React, {useState, useRef, useEffect} from 'react';
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
import {verifyEmail, resendCode, clearError} from '../../store/authStore';
import {useTheme} from '../../theme/ThemeContext';
import Header from '../../components/Header';
import {translate} from '../../utils/translations';

export default function VerificationScreen() {
  const dispatch = useAppDispatch();
  const userId = useAppSelector(s => s.auth.userId);
  const isLoading = useAppSelector(s => s.auth.isLoading);
  const error = useAppSelector(s => s.auth.error);
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const [digits, setDigits] = useState(['', '', '', '']);
  const refs = [
    useRef<TextInput>(null),
    useRef<TextInput>(null),
    useRef<TextInput>(null),
    useRef<TextInput>(null),
  ];

  const isValid = digits.every(d => d.length === 1) && !!userId;

  useEffect(() => {
    if (error) {
      Toast.show({
        type: 'error',
        text1: 'Verification Failed',
        text2: error,
      });
      dispatch(clearError());
    }
  }, [error, dispatch]);

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

  const handleVerify = () => {
    if (!userId) return;
    dispatch(verifyEmail({userId, code: digits.join('')}));
  };

  const handleResend = () => {
    if (!userId) return;
    dispatch(resendCode(userId));
  };

  const s = StyleSheet.create({
    container: {flex: 1, justifyContent: 'center', padding: spacing.lg, backgroundColor: colors.backgroundPrimary},
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
    btnPrimary: {backgroundColor: isValid && !isLoading ? colors.buttonPrimary : colors.buttonPrimaryDisabled},
    btnText: {color: colors.buttonPrimaryText, fontSize: fontSizes.md, fontWeight: fontWeights.semiBold},
    link: {color: colors.buttonPrimary, textAlign: 'center', marginTop: spacing.sm, fontSize: fontSizes.sm},
  });

  return (
    <View style={s.container}>
      <Header title={translate('VerificationTitle')} />
      <Text style={s.title}>Verify your email</Text>
      <Text style={s.subtitle}>Enter the 4-digit code sent to your email address.</Text>


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

      <TouchableOpacity style={[s.btn, s.btnPrimary]} onPress={handleVerify} disabled={!isValid || isLoading}>
        {isLoading ? <ActivityIndicator color={colors.buttonPrimaryText} /> : <Text style={s.btnText}>Verify</Text>}
      </TouchableOpacity>

      <TouchableOpacity onPress={handleResend} disabled={isLoading}>
        <Text style={s.link}>Resend code</Text>
      </TouchableOpacity>
    </View>
  );
}
