import React from 'react';
import {View, Text, TouchableOpacity, StyleSheet} from 'react-native';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {logout} from '../../store/authStore';
import {useTheme} from '../../theme/ThemeContext';

export default function UserScreen() {
  const dispatch = useAppDispatch();
  const email = useAppSelector(s => s.auth.email);
  const {colors, fontSizes, fontWeights, spacing, radius} = useTheme();

  return (
    <View style={[styles.container, {backgroundColor: colors.backgroundSecondary}]}>
      <Text style={[styles.email, {color: colors.textPrimary, fontSize: fontSizes.md}]}>{email}</Text>
      <TouchableOpacity
        style={[styles.btn, {backgroundColor: colors.buttonPrimary, borderRadius: radius.md, padding: spacing.md}]}
        onPress={() => dispatch(logout())}>
        <Text style={{color: colors.buttonPrimaryText, fontSize: fontSizes.md, fontWeight: fontWeights.semiBold}}>
          Logout
        </Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {flex: 1, alignItems: 'center', justifyContent: 'center', gap: 16},
  email: {},
  btn: {alignItems: 'center', minWidth: 120},
});
