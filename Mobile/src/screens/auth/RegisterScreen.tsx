// CHANGED_BY_AI: 2026-03-17 - Fix DEV_AUTO_FILL; add confirm password; inline validation; password toggle; use Button component; a11y; useMemo
// CHANGED_BY_AI: 2026-03-12 - Add registration subscription selection modal flow
// CHANGED_BY_AI: 2026-03-12 - Use theme linkColor token for auth link readability
// CHANGED_BY_AI: 2026-03-02 - Add shared header usage
import React, { useMemo, useState } from 'react';
import {
    Text,
    TextInput,
    TouchableOpacity,
    StyleSheet,
    KeyboardAvoidingView,
    Platform,
    ScrollView,
    View,
} from 'react-native';
import { useNavigation } from '@react-navigation/native';
import Toast from 'react-native-toast-message';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { register, socialLogin } from '../../store/authStore';
import { useTheme } from '../../theme/ThemeContext';
import SocialLoginButtons from '../../components/SocialLoginButtons';
import Header from '../../components/Header';
import Button from '../../components/Button';
import SubscriptionPlansModal from '../../components/SubscriptionPlansModal';
import { translate } from '../../utils/translations';
import Icon from 'react-native-vector-icons/MaterialIcons';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import type { AuthStackParamList } from '../../navigation/AuthNavigator';
import { DurationType, RegisterRequest, SubscriptionType } from "../../services/authService.ts";


type RegisterNavProp = NativeStackNavigationProp<AuthStackParamList, 'Register'>;

// DEV: Set inner flag to true to auto-fill registration form for testing (never active in production)
const DEV_AUTO_FILL = __DEV__ && true;

const getDevEmail = () => {
    const now = new Date();
    const datetime = now.toISOString().replace(/[-:]/g, '').replace(/\..+/, '').replace('T', '');
    return `okansarica+${datetime}@gmail.com`;
};

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

type FieldErrors = {
    name: string;
    surname: string;
    email: string;
    password: string;
    confirmPassword: string;
};

const emptyErrors: FieldErrors = {
    name: '',
    surname: '',
    email: '',
    password: '',
    confirmPassword: '',
};

export default function RegisterScreen() {
    const navigation = useNavigation<RegisterNavProp>();
    const dispatch = useAppDispatch();
    const isLoading = useAppSelector(s => s.auth.isLoading);
    const {colors, spacing, radius, fontSizes, fontWeights, linkColor} = useTheme();

    const [name, setName] = useState(DEV_AUTO_FILL ? 'Test' : '');
    const [surname, setSurname] = useState(DEV_AUTO_FILL ? 'User' : '');
    const [email, setEmail] = useState(DEV_AUTO_FILL ? getDevEmail() : '');
    const [password, setPassword] = useState(DEV_AUTO_FILL ? '123456' : '');
    const [confirmPassword, setConfirmPassword] = useState(DEV_AUTO_FILL ? '123456' : '');
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const [errors, setErrors] = useState<FieldErrors>(emptyErrors);
    const [showSubscriptionModal, setShowSubscriptionModal] = useState(false);

    const validate = (): FieldErrors => {
        const next: FieldErrors = {...emptyErrors};
        if (!name.trim()) next.name = translate('NameRequired');
        if (!surname.trim()) next.surname = translate('SurnameRequired');
        if (!email.trim()) {
            next.email = translate('EmailRequired');
        }
        else if (!EMAIL_REGEX.test(email.trim())) {
            next.email = translate('EmailInvalid');
        }
        if (!password) {
            next.password = translate('PasswordRequired');
        }
        else if (password.length < 6) {
            next.password = translate('PasswordTooShort');
        }
        if (!confirmPassword) {
            next.confirmPassword = translate('ConfirmPasswordRequired');
        }
        else if (password !== confirmPassword) {
            next.confirmPassword = translate('PasswordsMustMatch');
        }
        return next;
    };

    const hasErrors = (e: FieldErrors) => Object.values(e).some(v => v.length > 0);

    const handleRegister = () => {
        if (isLoading) return;
        const nextErrors = validate();
        setErrors(nextErrors);
        if (hasErrors(nextErrors)) return;
        setShowSubscriptionModal(true);
    };

    const handlePlanSelection = async (subscriptionType: SubscriptionType, duration?: DurationType) => {
        const request: RegisterRequest = {
            name: name.trim(),
            surname: surname.trim(),
            email: email.trim(),
            password,
            duration,
            subscriptionType,
        };
        const result = await dispatch(register(request));
        if (register.rejected.match(result)) {
            Toast.show({
                type: 'error',
                text1: translate('RegistrationFailed'),
                text2: result.payload as string,
            });
            return;
        }

        setShowSubscriptionModal(false);

        // Navigate to verification screen if email verification is required
        if (register.fulfilled.match(result) && result.payload.emailVerificationRequired) {
            navigation.navigate('Verification');
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

    const s = useMemo(
        () =>
            StyleSheet.create({
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
                fieldWrapper: {
                    marginBottom: spacing.sm,
                },
                input: {
                    borderWidth: 1,
                    borderColor: colors.inputBorder,
                    borderRadius: radius.md,
                    padding: spacing.md,
                    fontSize: fontSizes.md,
                    color: colors.inputText,
                    backgroundColor: colors.inputBackground,
                },
                inputError: {
                    borderColor: colors.danger,
                },
                errorText: {
                    fontSize: fontSizes.xs,
                    color: colors.danger,
                    marginTop: spacing.xs / 2,
                    marginLeft: spacing.xs,
                },
                passwordRow: {
                    flexDirection: 'row',
                    alignItems: 'center',
                    borderWidth: 1,
                    borderColor: colors.inputBorder,
                    borderRadius: radius.md,
                    backgroundColor: colors.inputBackground,
                },
                passwordRowError: {
                    borderColor: colors.danger,
                },
                passwordInput: {
                    flex: 1,
                    padding: spacing.md,
                    fontSize: fontSizes.md,
                    color: colors.inputText,
                },
                eyeButton: {
                    padding: spacing.md,
                    justifyContent: 'center',
                    alignItems: 'center',
                },
                link: {
                    color: linkColor,
                    textAlign: 'center',
                    marginTop: spacing.sm,
                    fontSize: fontSizes.sm,
                },
                submitBtn: {
                    marginBottom: spacing.sm,
                },
            }),
        [colors, spacing, radius, fontSizes, fontWeights, linkColor],
    );

    return (
        <KeyboardAvoidingView style={s.container} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
            <Header title={translate('RegisterTitle')}/>
            <ScrollView contentContainerStyle={s.scroll} keyboardShouldPersistTaps="handled">
                <Text style={s.title}>{translate('CreateYourAccount')}</Text>
                <Text style={s.subtitle}>{translate('SignUpToStartManaging')}</Text>

                {/* Name */}
                <View style={s.fieldWrapper}>
                    <TextInput
                        style={[s.input, errors.name ? s.inputError : null]}
                        placeholder={translate('Name')}
                        placeholderTextColor={colors.inputPlaceholder}
                        value={name}
                        onChangeText={v => {
                            setName(v);
                            if (errors.name) setErrors(prev => ({...prev, name: ''}));
                        }}
                        accessibilityLabel={translate('Name')}
                    />
                    {!!errors.name && <Text style={s.errorText}>{errors.name}</Text>}
                </View>

                {/* Surname */}
                <View style={s.fieldWrapper}>
                    <TextInput
                        style={[s.input, errors.surname ? s.inputError : null]}
                        placeholder={translate('Surname')}
                        placeholderTextColor={colors.inputPlaceholder}
                        value={surname}
                        onChangeText={v => {
                            setSurname(v);
                            if (errors.surname) setErrors(prev => ({...prev, surname: ''}));
                        }}
                        accessibilityLabel={translate('Surname')}
                    />
                    {!!errors.surname && <Text style={s.errorText}>{errors.surname}</Text>}
                </View>

                {/* Email */}
                <View style={s.fieldWrapper}>
                    <TextInput
                        style={[s.input, errors.email ? s.inputError : null]}
                        placeholder={translate('Email')}
                        placeholderTextColor={colors.inputPlaceholder}
                        autoCapitalize="none"
                        keyboardType="email-address"
                        value={email}
                        onChangeText={v => {
                            setEmail(v);
                            if (errors.email) setErrors(prev => ({...prev, email: ''}));
                        }}
                        accessibilityLabel={translate('Email')}
                    />
                    {!!errors.email && <Text style={s.errorText}>{errors.email}</Text>}
                </View>

                {/* Password */}
                <View style={s.fieldWrapper}>
                    <View style={[s.passwordRow, errors.password ? s.passwordRowError : null]}>
                        <TextInput
                            style={s.passwordInput}
                            placeholder={translate('Password')}
                            placeholderTextColor={colors.inputPlaceholder}
                            secureTextEntry={!showPassword}
                            value={password}
                            onChangeText={v => {
                                setPassword(v);
                                if (errors.password) setErrors(prev => ({...prev, password: ''}));
                            }}
                            accessibilityLabel={translate('Password')}
                        />
                        <TouchableOpacity
                            style={s.eyeButton}
                            onPress={() => setShowPassword(v => !v)}
                            accessibilityRole="button"
                            accessibilityLabel={showPassword ? translate('HidePassword') : translate('ShowPassword')}>
                            <Icon
                                name={showPassword ? 'visibility-off' : 'visibility'}
                                size={fontSizes.xl}
                                color={colors.textSecondary}
                            />
                        </TouchableOpacity>
                    </View>
                    {!!errors.password && <Text style={s.errorText}>{errors.password}</Text>}
                </View>

                {/* Confirm Password */}
                <View style={s.fieldWrapper}>
                    <View style={[s.passwordRow, errors.confirmPassword ? s.passwordRowError : null]}>
                        <TextInput
                            style={s.passwordInput}
                            placeholder={translate('ConfirmPassword')}
                            placeholderTextColor={colors.inputPlaceholder}
                            secureTextEntry={!showConfirmPassword}
                            value={confirmPassword}
                            onChangeText={v => {
                                setConfirmPassword(v);
                                if (errors.confirmPassword) setErrors(prev => ({...prev, confirmPassword: ''}));
                            }}
                            accessibilityLabel={translate('ConfirmPassword')}
                        />
                        <TouchableOpacity
                            style={s.eyeButton}
                            onPress={() => setShowConfirmPassword(v => !v)}
                            accessibilityRole="button"
                            accessibilityLabel={showConfirmPassword ? translate('HidePassword') : translate('ShowPassword')}>
                            <Icon
                                name={showConfirmPassword ? 'visibility-off' : 'visibility'}
                                size={fontSizes.xl}
                                color={colors.textSecondary}
                            />
                        </TouchableOpacity>
                    </View>
                    {!!errors.confirmPassword && <Text style={s.errorText}>{errors.confirmPassword}</Text>}
                </View>

                <Button
                    text={translate('Register')}
                    onPress={handleRegister}
                    isLoading={isLoading}
                    style={s.submitBtn}
                />

                <SocialLoginButtons
                    onGoogleLogin={token => handleSocialLogin('google', token)}
                    onFacebookLogin={token => handleSocialLogin('facebook', token)}
                    disabled={isLoading}
                />

                <TouchableOpacity
                    onPress={() => navigation.navigate('Login')}
                    accessibilityRole="link"
                    accessibilityLabel={translate('AlreadyHaveAccount')}>
                    <Text style={s.link}>{translate('AlreadyHaveAccount')}</Text>
                </TouchableOpacity>
            </ScrollView>
            <SubscriptionPlansModal
                visible={showSubscriptionModal}
                dismissible
                includeTrialOption
                onClose={() => setShowSubscriptionModal(false)}
                onPlanSelected={handlePlanSelection}
            />
        </KeyboardAvoidingView>
    );
}
