// CHANGED_BY_AI: 2026-03-17 - Replace hardcoded color with theme token; add a11y labels; useMemo for styles
import React, {useMemo} from 'react';
import {View, Text, TouchableOpacity, StyleSheet} from 'react-native';
import {GoogleSignin} from '@react-native-google-signin/google-signin';
import {LoginManager, AccessToken} from 'react-native-fbsdk-next';
import Icon from 'react-native-vector-icons/FontAwesome';
import {useTheme} from '../theme/ThemeContext';
import {translate} from '../utils/translations';

type SocialLoginButtonsProps = {
  onGoogleLogin: (idToken: string) => void;
  onFacebookLogin: (accessToken: string) => void;
  disabled?: boolean;
};

export default function SocialLoginButtons({
  onGoogleLogin,
  onFacebookLogin,
  disabled = false,
}: SocialLoginButtonsProps) {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();

  const handleGoogleLogin = async () => {
    await GoogleSignin.hasPlayServices();
    await GoogleSignin.signIn();
    const tokens = await GoogleSignin.getTokens();
    onGoogleLogin(tokens.idToken);
  };

  const handleFacebookLogin = async () => {
    const result = await LoginManager.logInWithPermissions(['public_profile', 'email']);
    if (!result.isCancelled) {
      const data = await AccessToken.getCurrentAccessToken();
      if (data) {
        onFacebookLogin(data.accessToken);
      }
    }
  };

  const s = useMemo(
    () =>
      StyleSheet.create({
        divider: {
          flexDirection: 'row',
          alignItems: 'center',
          marginVertical: spacing.lg,
        },
        dividerLine: {flex: 1, height: 1, backgroundColor: colors.borderSubtle},
        dividerText: {
          paddingHorizontal: spacing.md,
          color: colors.textSecondary,
          fontSize: fontSizes.sm,
        },
        btnSocial: {
          backgroundColor: colors.inputBackground,
          borderWidth: 1,
          borderColor: colors.borderSubtle,
          borderRadius: radius.md,
          padding: spacing.md,
          alignItems: 'center',
          justifyContent: 'center',
          marginBottom: spacing.sm,
          shadowColor: colors.cardShadow,
          shadowOffset: {width: 0, height: 2},
          shadowOpacity: 0.1,
          shadowRadius: 4,
          elevation: 2,
          flexDirection: 'row',
          gap: spacing.sm,
        },
        btnTextSocial: {
          color: colors.textSecondary,
          fontSize: fontSizes.md,
          fontWeight: fontWeights.medium,
        },
      }),
    [colors, spacing, radius, fontSizes, fontWeights],
  );

  return (
    <View>
      <View style={s.divider}>
        <View style={s.dividerLine} />
        <Text style={s.dividerText}>or continue with</Text>
        <View style={s.dividerLine} />
      </View>

      <TouchableOpacity
        style={s.btnSocial}
        onPress={handleGoogleLogin}
        disabled={disabled}
        accessibilityRole="button"
        accessibilityLabel={translate('ContinueWithGoogle')}>
        <Icon name="google" size={20} color="#DB4437" accessibilityElementsHidden />
        <Text style={s.btnTextSocial}>{translate('ContinueWithGoogle')}</Text>
      </TouchableOpacity>

      <TouchableOpacity
        style={s.btnSocial}
        onPress={handleFacebookLogin}
        disabled={disabled}
        accessibilityRole="button"
        accessibilityLabel={translate('ContinueWithFacebook')}>
        <Icon name="facebook" size={20} color="#1877F2" accessibilityElementsHidden />
        <Text style={s.btnTextSocial}>{translate('ContinueWithFacebook')}</Text>
      </TouchableOpacity>
    </View>
  );
}
