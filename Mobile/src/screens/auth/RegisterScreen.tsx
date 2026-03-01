import React, {useState} from 'react';
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
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {register, socialLogin, clearError} from '../../store/authStore';
import {useTheme} from '../../theme/ThemeContext';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {AuthStackParamList} from '../../navigation/AuthNavigator';

type RegisterNavProp = NativeStackNavigationProp<AuthStackParamList, 'Register'>;

export default function RegisterScreen() {
  const navigation = useNavigation<RegisterNavProp>();
  const dispatch = useAppDispatch();
  const isLoading = useAppSelector(s => s.auth.isLoading);
  const error = useAppSelector(s => s.auth.error);
  const emailVerificationRequired = useAppSelector(s => s.auth.emailVerificationRequired);
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const [name, setName] = useState('');
  const [surname, setSurname] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const isValid = name.length > 0 && surname.length > 0 && email.length > 0 && password.length > 0;

  React.useEffect(() => {
    if (emailVerificationRequired) {
      navigation.navigate('Verification');
    }
  }, [emailVerificationRequired, navigation]);

  const handleRegister = () => {
    dispatch(register({name, surname, email, password}));
  };

  const handleGoogleRegister = async () => {
    await GoogleSignin.hasPlayServices();
    await GoogleSignin.signIn();
    const tokens = await GoogleSignin.getTokens();
    dispatch(socialLogin({provider: 'google', token: tokens.idToken}));
  };

  const handleFacebookRegister = async () => {
    const data = await AccessToken.getCurrentAccessToken();
    if (data) {
      dispatch(socialLogin({provider: 'facebook', token: data.accessToken}));
    }
  };

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
        {error ? <Text style={s.error} onPress={() => dispatch(clearError())}>{error}</Text> : null}

        <TextInput
          style={s.input}
          placeholder="Name"
          placeholderTextColor={colors.inputPlaceholder}
          value={name}
          onChangeText={setName}
        />

        <TextInput
          style={s.input}
          placeholder="Surname"
          placeholderTextColor={colors.inputPlaceholder}
          value={surname}
          onChangeText={setSurname}
        />

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

        <TouchableOpacity style={[s.btn, s.btnPrimary]} onPress={handleRegister} disabled={!isValid || isLoading}>
          {isLoading ? <ActivityIndicator color={colors.buttonPrimaryText} /> : <Text style={s.btnText}>Register</Text>}
        </TouchableOpacity>

        <View style={s.divider} />

        <TouchableOpacity style={[s.btn, s.btnGoogle]} onPress={handleGoogleRegister} disabled={isLoading}>
          <Text style={s.btnText}>Continue with Google</Text>
        </TouchableOpacity>

        <LoginButton
          style={s.facebookBtn}
          onLoginFinished={(_err, result) => {
            if (!_err && !result.isCancelled) {
              handleFacebookRegister();
            }
          }}
          onLogoutFinished={() => {}}
        />

        <TouchableOpacity onPress={() => navigation.navigate('Login')}>
          <Text style={s.link}>Already have an account? Login</Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}
