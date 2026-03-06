// CHANGED_BY_AI: 2026-03-06 - Add create/update account screen
import React, {useState} from 'react';
import {View, Text, StyleSheet, TextInput} from 'react-native';
import {useTheme} from '../../../theme/ThemeContext';
import Header from '../../../components/Header';
import {translate} from '../../../utils/translations';
import {useAppDispatch, useAppSelector} from '../../../store/hooks';
import {createBankAccount, updateBankAccount} from '../../../store/banksStore';
import {useNavigation, useRoute} from '@react-navigation/native';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {FinanceStackParamList} from '../../../navigation/FinanceNavigator';
import Button from '../../../components/Button';
import Toast from 'react-native-toast-message';

type FinanceNavProp = NativeStackNavigationProp<FinanceStackParamList, 'AccountEdit'>;

export default function AccountEditScreen() {
  const {colors, spacing, radius, fontSizes} = useTheme();
  const dispatch = useAppDispatch();
  const navigation = useNavigation<FinanceNavProp>();
  const route = useRoute();
  const params = route.params as FinanceStackParamList['AccountEdit'];
  const isSaving = useAppSelector(state => state.banks.isSaving);
  const [name, setName] = useState(params.mode === 'edit' ? params.account.name : '');

  const onSave = async () => {
    if (params.mode === 'create') {
      const result = await dispatch(createBankAccount({bankId: params.bankId, data: {name}}));
      if (result.meta.requestStatus !== 'fulfilled') {
        Toast.show({type: 'error', text1: translate('Error'), text2: result.payload as string});
        return;
      }
      navigation.goBack();
      return;
    }

    const result = await dispatch(updateBankAccount({bankId: params.bankId, id: params.account.id, data: {name}}));
    if (result.meta.requestStatus !== 'fulfilled') {
      Toast.show({type: 'error', text1: translate('Error'), text2: result.payload as string});
      return;
    }
    navigation.goBack();
  };

  const s = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.backgroundSecondary,
    },
    content: {
      padding: spacing.lg,
    },
    label: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
      marginBottom: spacing.xs,
    },
    input: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      paddingHorizontal: spacing.md,
      paddingVertical: spacing.sm,
      color: colors.textPrimary,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      marginBottom: spacing.lg,
    },
  });

  const title = params.mode === 'edit' ? translate('UpdateAccount') : translate('CreateAccount');

  return (
    <View style={s.container}>
      <Header title={title} />
      <View style={s.content}>
        <Text style={s.label}>{translate('AccountName')}</Text>
        <TextInput value={name} onChangeText={setName} style={s.input} />
        <Button text={translate('Save')} onPress={onSave} isLoading={isSaving} disabled={isSaving} />
      </View>
    </View>
  );
}

