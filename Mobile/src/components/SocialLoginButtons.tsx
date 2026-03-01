import React from 'react';
import {View, Text, TouchableOpacity, StyleSheet} from 'react-native';
import {GoogleSignin} from '@react-native-google-signin/google-signin';
import {LoginManager, AccessToken} from 'react-native-fbsdk-next';
import Icon from 'react-native-vector-icons/FontAwesome';
import {useTheme} from '../theme/ThemeContext';

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

  const s = StyleSheet.create({
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
      backgroundColor: '#FFFFFF',
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
    btnTextSocial: {color: '#5F6368', fontSize: fontSizes.md, fontWeight: fontWeights.medium},
  });

  return (
    <View>
      <View style={s.divider}>
        <View style={s.dividerLine} />
        <Text style={s.dividerText}>or continue with</Text>
        <View style={s.dividerLine} />
      </View>

      <TouchableOpacity style={s.btnSocial} onPress={handleGoogleLogin} disabled={disabled}>
        <Icon name="google" size={20} color="#DB4437" />
        <Text style={s.btnTextSocial}>Continue with Google</Text>
      </TouchableOpacity>

      <TouchableOpacity style={s.btnSocial} onPress={handleFacebookLogin} disabled={disabled}>
        <Icon name="facebook" size={20} color="#1877F2" />
        <Text style={s.btnTextSocial}>Continue with Facebook</Text>
      </TouchableOpacity>
    </View>
  );
}

