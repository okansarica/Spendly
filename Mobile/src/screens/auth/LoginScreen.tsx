// CHANGED_BY_AI: 2026-03-13 - Add trial expired subscription payment flow to login
// CHANGED_BY_AI: 2026-03-12 - Use theme linkColor token for auth link readability
// CHANGED_BY_AI: 2026-03-02 - Add shared header usage
import React, {useState, useEffect} from 'react';
import {
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
} from 'react-native';
import {useNavigation} from '@react-navigation/native';
import Toast from 'react-native-toast-message';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {login, socialLogin} from '../../store/authStore';
import {useTheme} from '../../theme/ThemeContext';
import Button from '../../components/Button';
import SocialLoginButtons from '../../components/SocialLoginButtons';
import Header from '../../components/Header';
import {translate} from '../../utils/translations';

import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {AuthStackParamList} from '../../navigation/AuthNavigator';

type LoginNavProp = NativeStackNavigationProp<AuthStackParamList, 'Login'>;

export default function LoginScreen() {
  const navigation = useNavigation<LoginNavProp>();
  const dispatch = useAppDispatch();
  const isLoading = useAppSelector(s => s.auth.isLoading);
  const emailVerificationRequired = useAppSelector(s => s.auth.emailVerificationRequired);
  const {colors, spacing, radius, fontSizes, fontWeights, linkColor} = useTheme();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  

  const isValid = email.length > 0 && password.length > 0;

  useEffect(() => {
    //TODO-UPDATED: 2026-03-01 GoogleSignin configured with webClientId
    // GoogleSignin.configure({
    //   webClientId: '',
    // });
  }, []);

  useEffect(() => {
    if (emailVerificationRequired) {
      navigation.navigate('Verification');
    }
  }, [emailVerificationRequired, navigation]);

  const handleLogin = async () => {
    const result = await dispatch(login({email, password}));
    if (login.rejected.match(result)) {
      Toast.show({
        type: 'error',
        text1: translate('LoginFailed'),
        text2: result.payload as string,
      });
      return;
    }
  };


  const handleSocialLogin = async (provider: 'google' | 'facebook', token: string) => {
    const result = await dispatch(socialLogin({provider, token}));
    if (socialLogin.rejected.match(result)) {
      Toast.show({
        type: 'error',
        text1: translate('LoginFailed'),
        text2: result.payload as string,
      });
    }
  };

  const s = StyleSheet.create({
    container: {flex: 1, backgroundColor: colors.backgroundPrimary},
    scroll: {flexGrow: 1, justifyContent: 'center', padding: spacing.lg},
    title: {
      fontSize: fontSizes.xxl,
      fontWeight: fontWeights.bold,
      color: colors.textPrimary,
      marginBottom: spacing.xs,
      textAlign: 'center' as const,
    },
    subtitle: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
      marginBottom: spacing.xl,
      textAlign: 'center' as const,
    },
    inputLabel: {
      fontSize: fontSizes.sm,
      fontWeight: fontWeights.medium,
      color: colors.textPrimary,
      marginBottom: spacing.xs,
    },
    input: {
      borderWidth: 1,
      borderColor: colors.inputBorder,
      borderRadius: radius.md,
      padding: spacing.md,
      marginBottom: spacing.md,
      fontSize: fontSizes.md,
      color: colors.inputText,
      backgroundColor: colors.inputBackground,
    },
    link: {
      color: linkColor,
      textAlign: 'center' as const,
      marginTop: spacing.sm,
      fontSize: fontSizes.sm,
      fontWeight: fontWeights.medium,
    },
  });

  return (
    <KeyboardAvoidingView style={s.container} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <Header title={translate('LoginTitle')} showBack={false} />
      <ScrollView contentContainerStyle={s.scroll} keyboardShouldPersistTaps="handled">
        <Text style={s.title}>{translate('WelcomeBack')}</Text>
        <Text style={s.subtitle}>{translate('SignInToContinue')}</Text>


        <Text style={s.inputLabel}>{translate('Email')}</Text>
        <TextInput
          style={s.input}
          placeholder={translate('EnterYourEmail')}
          placeholderTextColor={colors.inputPlaceholder}
          autoCapitalize="none"
          keyboardType="email-address"
          value={email}
          onChangeText={setEmail}
        />

        <Text style={s.inputLabel}>{translate('Password')}</Text>
        <TextInput
          style={s.input}
          placeholder={translate('EnterYourPassword')}
          placeholderTextColor={colors.inputPlaceholder}
          secureTextEntry
          value={password}
          onChangeText={setPassword}
        />

        
        <Button
          text={translate('SignIn')}
          onPress={handleLogin}
          variant="primary"
          isLoading={isLoading}
          disabled={!isValid}
          style={{marginBottom: spacing.sm}}
        />

        <TouchableOpacity onPress={() => navigation.navigate('ForgotPassword')}>
          <Text style={s.link}>{translate('ForgotYourPassword')}</Text>
        </TouchableOpacity>

        <SocialLoginButtons
          onGoogleLogin={token => handleSocialLogin('google', token)}
          onFacebookLogin={token => handleSocialLogin('facebook', token)}
          disabled={isLoading}
        />

        <TouchableOpacity onPress={() => navigation.navigate('Register')}>
          <Text style={s.link}>{translate('DontHaveAccount')}</Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}
