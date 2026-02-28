import React, {useEffect} from 'react';
import {View, Text, ActivityIndicator, StyleSheet} from 'react-native';
import {useTheme} from '../../theme/ThemeContext';
import {useAuthStore} from '../../store/authStore';

export default function SplashScreen() {
  const {colors, fontSizes, fontWeights} = useTheme();
  const checkAuth = useAuthStore(s => s.checkAuth);

  useEffect(() => {
    checkAuth();
  }, [checkAuth]);

  return (
    <View style={[styles.container, {backgroundColor: colors.backgroundPrimary}]}>
      <Text style={[styles.logo, {color: colors.buttonPrimary, fontSize: fontSizes.xxl, fontWeight: fontWeights.bold}]}>
        Spendly
      </Text>
      <ActivityIndicator color={colors.spinner} style={styles.spinner} size="large" />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {flex: 1, alignItems: 'center', justifyContent: 'center'},
  logo: {},
  spinner: {marginTop: 24},
});

