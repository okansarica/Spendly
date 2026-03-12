// CHANGED_BY_AI: 2026-03-02 - Add shared header usage
import React, {useState, useEffect} from 'react';
import {
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
import Toast from 'react-native-toast-message';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {register, socialLogin} from '../../store/authStore';
import {useTheme} from '../../theme/ThemeContext';
import SocialLoginButtons from '../../components/SocialLoginButtons';
import Header from '../../components/Header';
import {translate} from '../../utils/translations';

import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {AuthStackParamList} from '../../navigation/AuthNavigator';

type RegisterNavProp = NativeStackNavigationProp<AuthStackParamList, 'Register'>;

export default function RegisterScreen() {
  const navigation = useNavigation<RegisterNavProp>();
  const dispatch = useAppDispatch();
  const isLoading = useAppSelector(s => s.auth.isLoading);
  const emailVerificationRequired = useAppSelector(s => s.auth.emailVerificationRequired);
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const [name, setName] = useState('');
  const [surname, setSurname] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const isValid = name.length > 0 && surname.length > 0 && email.length > 0 && password.length > 0;

  useEffect(() => {
    if (emailVerificationRequired) {
      navigation.navigate('Verification');
    }
  }, [emailVerificationRequired, navigation]);


  const handleRegister = async () => {
    const result = await dispatch(register({name, surname, email, password}));
    if (register.rejected.match(result)) {
      Toast.show({
        type: 'error',
        text1: translate('RegistrationFailed'),
        text2: result.payload as string,
      });
    }
  };

  const handleSocialLogin = async (provider: 'google' | 'facebook', token: string) => {
    const result = await dispatch(socialLogin({provider, token}));
    if (socialLogin.rejected.match(result)) {
      Toast.show({
        type: 'error',
        text1: translate('RegistrationFailed'),
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
      textAlign: 'center',
    },
    subtitle: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
      marginBottom: spacing.xl,
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
    btnText: {color: colors.buttonPrimaryText, fontSize: fontSizes.md, fontWeight: fontWeights.semiBold},
    link: {color: colors.buttonPrimary, textAlign: 'center', marginTop: spacing.sm, fontSize: fontSizes.sm},
  });

  return (
    <KeyboardAvoidingView style={s.container} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <Header title={translate('RegisterTitle')} />
      <ScrollView contentContainerStyle={s.scroll} keyboardShouldPersistTaps="handled">
        <Text style={s.title}>{translate('CreateYourAccount')}</Text>
        <Text style={s.subtitle}>{translate('SignUpToStartManaging')}</Text>

        <TextInput
          style={s.input}
          placeholder={translate('Name')}
          placeholderTextColor={colors.inputPlaceholder}
          value={name}
          onChangeText={setName}
        />

        <TextInput
          style={s.input}
          placeholder={translate('Surname')}
          placeholderTextColor={colors.inputPlaceholder}
          value={surname}
          onChangeText={setSurname}
        />

        <TextInput
          style={s.input}
          placeholder={translate('Email')}
          placeholderTextColor={colors.inputPlaceholder}
          autoCapitalize="none"
          keyboardType="email-address"
          value={email}
          onChangeText={setEmail}
        />

        <TextInput
          style={s.input}
          placeholder={translate('Password')}
          placeholderTextColor={colors.inputPlaceholder}
          secureTextEntry
          value={password}
          onChangeText={setPassword}
        />

        <TouchableOpacity style={[s.btn, s.btnPrimary]} onPress={handleRegister} disabled={!isValid || isLoading}>
          {isLoading ? <ActivityIndicator color={colors.buttonPrimaryText} /> : <Text style={s.btnText}>{translate('Register')}</Text>}
        </TouchableOpacity>

        <SocialLoginButtons
          onGoogleLogin={token => handleSocialLogin('google', token)}
          onFacebookLogin={token => handleSocialLogin('facebook', token)}
          disabled={isLoading}
        />

        <TouchableOpacity onPress={() => navigation.navigate('Login')}>
          <Text style={s.link}>{translate('AlreadyHaveAccount')}</Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}
