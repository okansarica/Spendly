import React, {useState, useEffect} from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
} from 'react-native';
import {useNavigation} from '@react-navigation/native';
import {GoogleSignin} from '@react-native-google-signin/google-signin';
import {LoginButton, AccessToken} from 'react-native-fbsdk-next';
import {useAuthStore} from '../../store/authStore';
import {useTheme} from '../../theme/ThemeContext';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {AuthStackParamList} from '../../navigation/AuthNavigator';

type LoginNavProp = NativeStackNavigationProp<AuthStackParamList, 'Login'>;

export default function LoginScreen() {
  const navigation = useNavigation<LoginNavProp>();
  const {login, socialLogin, isLoading, error, emailVerificationRequired, clearError} = useAuthStore();
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const isValid = email.length > 0 && password.length > 0;

  useEffect(() => {
    GoogleSignin.configure();
  }, []);

  const handleLogin = async () => {
    await login({email, password});
  };

  const handleGoogleLogin = async () => {
    await GoogleSignin.hasPlayServices();
    await GoogleSignin.signIn();
    const tokens = await GoogleSignin.getTokens();
    await socialLogin({provider: 'google', token: tokens.idToken});
  };

  const handleFacebookLogin = async () => {
    const data = await AccessToken.getCurrentAccessToken();
    if (data) {
      await socialLogin({provider: 'facebook', token: data.accessToken});
    }
  };

  useEffect(() => {
    if (emailVerificationRequired) {
      navigation.navigate('Verification');
    }
  }, [emailVerificationRequired, navigation]);

  const s = StyleSheet.create({
    container: {flex: 1, backgroundColor: colors.backgroundPrimary},
    scroll: {flexGrow: 1, justifyContent: 'center', padding: spacing.lg},
    error: {
      backgroundColor: colors.errorBackground,
      color: colors.errorText,
      borderRadius: radius.sm,
      padding: spacing.sm,
      marginBottom: spacing.md,
      fontSize: fontSizes.sm,
      textAlign: 'center',
    },
    input: {
      borderWidth: 1,
      borderColor: colors.inputBorder,
      borderRadius: radius.md,
      padding: spacing.md,
      marginBottom: spacing.sm,
      fontSize: fontSizes.md,
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
    btnGoogle: {backgroundColor: '#DB4437'},
    btnText: {color: colors.buttonPrimaryText, fontSize: fontSizes.md, fontWeight: fontWeights.semiBold},
    facebookBtn: {height: 44, width: '100%', marginBottom: spacing.sm},
    link: {color: colors.buttonPrimary, textAlign: 'center', marginTop: spacing.sm, fontSize: fontSizes.sm},
    divider: {height: 1, backgroundColor: colors.borderSubtle, marginVertical: spacing.md},
  });

  return (
    <KeyboardAvoidingView style={s.container} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView contentContainerStyle={s.scroll} keyboardShouldPersistTaps="handled">
        {error ? <Text style={s.error} onPress={clearError}>{error}</Text> : null}

        <TextInput
          style={s.input}
          placeholder="Email"
          placeholderTextColor={colors.inputPlaceholder}
          autoCapitalize="none"
          keyboardType="email-address"
          value={email}
          onChangeText={setEmail}
        />

        <TextInput
          style={s.input}
          placeholder="Password"
          placeholderTextColor={colors.inputPlaceholder}
          secureTextEntry
          value={password}
          onChangeText={setPassword}
        />

        <TouchableOpacity style={[s.btn, s.btnPrimary]} onPress={handleLogin} disabled={!isValid || isLoading}>
          {isLoading ? <ActivityIndicator color={colors.buttonPrimaryText} /> : <Text style={s.btnText}>Login</Text>}
        </TouchableOpacity>

        <View style={s.divider} />

        <TouchableOpacity style={[s.btn, s.btnGoogle]} onPress={handleGoogleLogin} disabled={isLoading}>
          <Text style={s.btnText}>Login with Google</Text>
        </TouchableOpacity>

        <LoginButton
          style={s.facebookBtn}
          onLoginFinished={(_err, result) => {
            if (!_err && !result.isCancelled) {
              handleFacebookLogin();
            }
          }}
          onLogoutFinished={() => {}}
        />

        <TouchableOpacity onPress={() => navigation.navigate('ForgotPassword')}>
          <Text style={s.link}>Forgot Password?</Text>
        </TouchableOpacity>

        <TouchableOpacity onPress={() => navigation.navigate('Register')}>
          <Text style={s.link}>Don't have an account? Register</Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}
