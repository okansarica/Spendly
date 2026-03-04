// CHANGED_BY_AI: 2026-03-03 - Add change password screen with backend integration
import React, {useState} from 'react';
import {Alert, StyleSheet, Text, TextInput, TouchableOpacity, View} from 'react-native';
import Header from '../../components/Header';
import {translate} from '../../utils/translations';
import {useTheme} from '../../theme/ThemeContext';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {changeUserPassword} from '../../store/userStore';
import Toast from "react-native-toast-message";

export default function ChangePasswordScreen() {
  const dispatch = useAppDispatch();
  const isSaving = useAppSelector(s => s.user.isSaving);
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');

  const s = StyleSheet.create({
    container: {flex: 1, backgroundColor: colors.backgroundSecondary},
    content: {padding: spacing.lg, gap: spacing.md},
    label: {color: colors.textSecondary, fontSize: fontSizes.sm},
    input: {
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      color: colors.textPrimary,
      paddingHorizontal: spacing.md,
      paddingVertical: spacing.sm,
    },
    saveBtn: {
      marginTop: spacing.md,
      backgroundColor: colors.buttonPrimary,
      borderRadius: radius.md,
      padding: spacing.md,
      alignItems: 'center',
    },
    saveText: {color: colors.buttonPrimaryText, fontWeight: fontWeights.semiBold, fontSize: fontSizes.md},
  });

  const onSave = async () => {
    if (!currentPassword || !newPassword || !confirmNewPassword) {
      Alert.alert(translate('Warning'), translate('AllPasswordFieldsRequired'));
      return;
    }

    if (newPassword !== confirmNewPassword) {
      Alert.alert(translate('Warning'), translate('PasswordsDoNotMatch'));
      return;
    }

    const result = await dispatch(changeUserPassword({currentPassword, newPassword, confirmNewPassword}));if (result.meta.requestStatus !== 'fulfilled') {
      Toast.show({
        type: 'error',
        text1: translate('Error'),
        text2: result.payload as string,
      });
      return;
    }
    
    setCurrentPassword('');
    setNewPassword('');
    setConfirmNewPassword('');
    Alert.alert(translate('SuccessTitle'), translate('PasswordUpdatedMessage'));
  };

  return (
    <View style={s.container}>
      <Header title={translate('ChangePasswordTitle')} />
      <View style={s.content}>
        <Text style={s.label}>{translate('CurrentPasswordTitle')}</Text>
        <TextInput value={currentPassword} onChangeText={setCurrentPassword} secureTextEntry style={s.input} />

        <Text style={s.label}>{translate('NewPasswordTitle')}</Text>
        <TextInput value={newPassword} onChangeText={setNewPassword} secureTextEntry style={s.input} />

        <Text style={s.label}>{translate('ConfirmNewPasswordTitle')}</Text>
        <TextInput value={confirmNewPassword} onChangeText={setConfirmNewPassword} secureTextEntry style={s.input} />

        <TouchableOpacity style={s.saveBtn} onPress={onSave} disabled={isSaving}>
          <Text style={s.saveText}>{translate('Save')}</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}
