import React, {useState, useEffect} from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
} from 'react-native';
import {useNavigation} from '@react-navigation/native';
import {GoogleSignin} from '@react-native-google-signin/google-signin';
import {AccessToken} from 'react-native-fbsdk-next';
import Icon from 'react-native-vector-icons/FontAwesome';
import Toast from 'react-native-toast-message';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {login, socialLogin, clearError} from '../../store/authStore';
import {useTheme} from '../../theme/ThemeContext';
import Button from '../../components/Button';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {AuthStackParamList} from '../../navigation/AuthNavigator';

type LoginNavProp = NativeStackNavigationProp<AuthStackParamList, 'Login'>;

export default function LoginScreen() {
  const navigation = useNavigation<LoginNavProp>();
  const dispatch = useAppDispatch();
  const isLoading = useAppSelector(s => s.auth.isLoading);
  const error = useAppSelector(s => s.auth.error);
  const emailVerificationRequired = useAppSelector(s => s.auth.emailVerificationRequired);
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const isValid = email.length > 0 && password.length > 0;

  useEffect(() => {
    //TODO-UPDATED: 2026-03-01 GoogleSignin configured with webClientId
    // GoogleSignin.configure({
    //   webClientId: '',
    // });
  }, []);

  const handleLogin = () => {
    dispatch(login({email, password}));
  };

  const handleGoogleLogin = async () => {
    await GoogleSignin.hasPlayServices();
    await GoogleSignin.signIn();
    const tokens = await GoogleSignin.getTokens();
    dispatch(socialLogin({provider: 'google', token: tokens.idToken}));
  };

  const handleFacebookLogin = async () => {
    const data = await AccessToken.getCurrentAccessToken();
    if (data) {
      dispatch(socialLogin({provider: 'facebook', token: data.accessToken}));
    }
  };

  useEffect(() => {
    if (emailVerificationRequired) {
      navigation.navigate('Verification');
    }
  }, [emailVerificationRequired, navigation]);

  useEffect(() => {
    if (error) {
      Toast.show({
        type: 'error',
        text1: 'Login Failed',
        text2: error,
      });
      dispatch(clearError());
    }
  }, [error, dispatch]);

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
    btnSocial: {
      backgroundColor: '#FFFFFF',
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      borderRadius: radius.md,
      padding: spacing.md,
      alignItems: 'center' as const,
      justifyContent: 'center' as const,
      marginBottom: spacing.sm,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.1,
      shadowRadius: 4,
      elevation: 2,
      flexDirection: 'row' as const,
      gap: spacing.sm,
    },
    btnFacebook: {
      backgroundColor: '#1877F2',
      borderWidth: 0,
    },
    btnTextSocial: {color: '#5F6368', fontSize: fontSizes.md, fontWeight: fontWeights.medium},
    btnTextFacebook: {color: '#FFFFFF', fontSize: fontSizes.md, fontWeight: fontWeights.medium},
    link: {color: colors.buttonPrimary, textAlign: 'center' as const, marginTop: spacing.sm, fontSize: fontSizes.sm, fontWeight: fontWeights.medium},
    divider: {
      flexDirection: 'row' as const,
      alignItems: 'center' as const,
      marginVertical: spacing.lg,
    },
    dividerLine: {flex: 1, height: 1, backgroundColor: colors.borderSubtle},
    dividerText: {
      paddingHorizontal: spacing.md,
      color: colors.textSecondary,
      fontSize: fontSizes.sm,
    },
  });

  return (
    <KeyboardAvoidingView style={s.container} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView contentContainerStyle={s.scroll} keyboardShouldPersistTaps="handled">
        <Text style={s.title}>Welcome back</Text>
        <Text style={s.subtitle}>Sign in to continue to Spendly</Text>


        <Text style={s.inputLabel}>Email</Text>
        <TextInput
          style={s.input}
          placeholder="Enter your email"
          placeholderTextColor={colors.inputPlaceholder}
          autoCapitalize="none"
          keyboardType="email-address"
          value={email}
          onChangeText={setEmail}
        />

        <Text style={s.inputLabel}>Password</Text>
        <TextInput
          style={s.input}
          placeholder="Enter your password"
          placeholderTextColor={colors.inputPlaceholder}
          secureTextEntry
          value={password}
          onChangeText={setPassword}
        />

        
        <Button
          text="Sign in"
          onPress={handleLogin}
          variant="primary"
          isLoading={isLoading}
          disabled={!isValid}
          style={{marginBottom: spacing.sm}}
        />

        <TouchableOpacity onPress={() => navigation.navigate('ForgotPassword')}>
          <Text style={s.link}>Forgot your password?</Text>
        </TouchableOpacity>

        <View style={s.divider}>
          <View style={s.dividerLine} />
          <Text style={s.dividerText}>or continue with</Text>
          <View style={s.dividerLine} />
        </View>

        <TouchableOpacity style={s.btnSocial} onPress={handleGoogleLogin} disabled={isLoading}>
          <Icon name="google" size={20} color="#DB4437" />
          <Text style={s.btnTextSocial}>Continue with Google</Text>
        </TouchableOpacity>

        <TouchableOpacity style={[s.btnSocial, s.btnFacebook]} onPress={handleFacebookLogin} disabled={isLoading}>
          <Icon name="facebook" size={20} color="#FFFFFF" />
          <Text style={s.btnTextFacebook}>Continue with Facebook</Text>
        </TouchableOpacity>

        <TouchableOpacity onPress={() => navigation.navigate('Register')}>
          <Text style={s.link}>Don't have an account? Sign up</Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}
