// CHANGED_BY_AI: 2026-03-03 - Add profile screen with update and delete account flows
import React, {useEffect, useState} from 'react';
import {Alert, StyleSheet, Switch, Text, TextInput, TouchableOpacity, View} from 'react-native';
import Header from '../../components/Header';
import {translate} from '../../utils/translations';
import {useTheme} from '../../theme/ThemeContext';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {deleteUserAccount, loadUserProfile, updateUserProfile} from '../../store/userStore';
import {logout} from '../../store/authStore';
import Button from '../../components/Button';

export default function ProfileScreen() {
  const dispatch = useAppDispatch();
  const profile = useAppSelector(s => s.user.profile);
  const isSaving = useAppSelector(s => s.user.isSaving);
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();

  const [name, setName] = useState('');
  const [surname, setSurname] = useState('');
  const [newsletter, setNewsletter] = useState(false);
  const [nameError, setNameError] = useState('');
  const [surnameError, setSurnameError] = useState('');

  useEffect(() => {
    dispatch(loadUserProfile());
  }, [dispatch]);

  useEffect(() => {
    if (profile) {
      setName(profile.name || '');
      setSurname(profile.surname || '');
      setNewsletter(profile.isNewsletterSubscribed || false);
    }
  }, [profile]);

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
    errorText: {
      color: colors.danger,
      fontSize: fontSizes.xs,
      marginTop: spacing.xs,
    },
    emailText: {color: colors.textPrimary, fontSize: fontSizes.md},
    row: {flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between'},
    saveBtn: {
      marginTop: spacing.sm,
      backgroundColor: colors.buttonPrimary,
      borderRadius: radius.md,
      padding: spacing.md,
      alignItems: 'center',
    },
    saveText: {color: colors.buttonPrimaryText, fontWeight: fontWeights.semiBold, fontSize: fontSizes.md},
    deleteBtn: {
      marginTop: spacing.lg,
      borderRadius: radius.md,
      padding: spacing.md,
      alignItems: 'center',
      borderWidth: 1,
      borderColor: colors.danger,
      backgroundColor: colors.cardBackground,
    },
    deleteText: {color: colors.danger, fontWeight: fontWeights.semiBold, fontSize: fontSizes.md},
  });

  const onSave = async () => {
    const trimmedName = name.trim();
    const trimmedSurname = surname.trim();
    const nextNameError = !trimmedName ? translate('NameRequired') : '';
    const nextSurnameError = !trimmedSurname ? translate('SurnameRequired') : '';

    setNameError(nextNameError);
    setSurnameError(nextSurnameError);

    if (nextNameError || nextSurnameError) {
      return;
    }

    await dispatch(updateUserProfile({name: trimmedName, surname: trimmedSurname, isNewsletterSubscribed: newsletter}));
    Alert.alert(translate('SuccessTitle'), translate('ProfileUpdatedMessage'));
  };

  const onDeleteAccount = () => {
    Alert.alert(translate('DeleteAccountTitle'), translate('DeleteAccountMessage'), [
      {text: translate('Cancel'), style: 'cancel'},
      {
        text: translate('Delete'),
        style: 'destructive',
        onPress: async () => {
          await dispatch(deleteUserAccount());
          await dispatch(logout());
        },
      },
    ]);
  };

  return (
    <View style={s.container}>
      <Header title={translate('ProfileTitle')} />
      <View style={s.content}>
        <Text style={s.label}>{translate('EmailTitle')}</Text>
        <Text style={s.emailText}>{profile?.email}</Text>
        
        <Text style={s.label}>{translate('FirstNameTitle')}</Text>
        <TextInput
          value={name}
          onChangeText={value => {
            setName(value);
            if (nameError) {
              setNameError('');
            }
          }}
          style={s.input}
        />
        {nameError ? <Text style={s.errorText}>{nameError}</Text> : null}

        <Text style={s.label}>{translate('LastNameTitle')}</Text>
        <TextInput
          value={surname}
          onChangeText={value => {
            setSurname(value);
            if (surnameError) {
              setSurnameError('');
            }
          }}
          style={s.input}
        />
        {surnameError ? <Text style={s.errorText}>{surnameError}</Text> : null}        

        <View style={s.row}>
          <Text style={s.label}>{translate('NewsletterTitle')}</Text>
          <Switch value={newsletter} onValueChange={setNewsletter} />
        </View>

        <Button text={translate('Save')} onPress={onSave} isLoading={isSaving} style={s.saveBtn} />

        <TouchableOpacity style={s.deleteBtn} onPress={onDeleteAccount} disabled={isSaving}>
          <Text style={s.deleteText}>{translate('DeleteAccountTitle')}</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}
