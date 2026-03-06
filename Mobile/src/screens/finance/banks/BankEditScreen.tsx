// CHANGED_BY_AI: 2026-03-06 - Add create/update bank screen
import React, {useEffect, useMemo, useState} from 'react';
import {View, Text, StyleSheet, TextInput, TouchableOpacity, Modal} from 'react-native';
import {useTheme} from '../../../theme/ThemeContext';
import Header from '../../../components/Header';
import {translate} from '../../../utils/translations';
import {useAppDispatch, useAppSelector} from '../../../store/hooks';
import {createBank, loadBankDefinitions, updateBank} from '../../../store/banksStore';
import {useNavigation, useRoute} from '@react-navigation/native';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {FinanceStackParamList} from '../../../navigation/FinanceNavigator';
import Button from '../../../components/Button';
import Toast from 'react-native-toast-message';

type FinanceNavProp = NativeStackNavigationProp<FinanceStackParamList, 'BankEdit'>;

export default function BankEditScreen() {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const navigation = useNavigation<FinanceNavProp>();
  const route = useRoute();
  const params = route.params as FinanceStackParamList['BankEdit'];
  const mode = params.mode;
  const bank = params.mode === 'edit' ? params.bank : undefined;
  const definitions = useAppSelector(state => state.banks.definitions);
  const isSaving = useAppSelector(state => state.banks.isSaving);
  const [isDefinitionMode, setIsDefinitionMode] = useState(mode === 'create');
  const [selectedDefinitionId, setSelectedDefinitionId] = useState<string | undefined>(bank?.bankDefinitionId);
  const [name, setName] = useState(bank?.name ?? '');
  const [description, setDescription] = useState(bank?.description ?? '');
  const [isDefinitionsOpen, setIsDefinitionsOpen] = useState(false);
  const [validationError, setValidationError] = useState<string | undefined>(undefined);

  useEffect(() => {
    if (mode === 'create') {
      dispatch(loadBankDefinitions());
    }
  }, [dispatch, mode]);

  useEffect(() => {
    if (!selectedDefinitionId) {
      return;
    }
    const selected = definitions.find(item => item.id === selectedDefinitionId);
    if (selected) {
      setName(selected.name);
    }
  }, [definitions, selectedDefinitionId]);

  const isNameEditable = useMemo(() => {
    if (mode === 'edit' && bank?.bankDefinitionId) {
      return false;
    }
    if (mode === 'create' && isDefinitionMode) {
      return false;
    }
    return true;
  }, [mode, bank?.bankDefinitionId, isDefinitionMode]);

  const selectedDefinitionName = useMemo(() => {
    const selected = definitions.find(item => item.id === selectedDefinitionId);
    return selected?.name ?? translate('SelectBankName');
  }, [definitions, selectedDefinitionId]);

  const onSave = async () => {
    const trimmedName = name.trim();

    if (mode === 'create' && isDefinitionMode && !selectedDefinitionId) {
      setValidationError(translate('BankSelectionRequired'));
      return;
    }

    if ((!isDefinitionMode || mode === 'edit') && !trimmedName) {
      setValidationError(translate('BankNameRequired'));
      return;
    }

    setValidationError(undefined);

    const payload = isDefinitionMode && mode === 'create'
      ? {bankDefinitionId: selectedDefinitionId, description}
      : {name: trimmedName, description};

    if (mode === 'create') {
      const result = await dispatch(createBank(payload));
      if (result.meta.requestStatus !== 'fulfilled') {
        Toast.show({type: 'error', text1: translate('Error'), text2: result.payload as string});
        return;
      }
      navigation.goBack();
      return;
    }

    if (!bank?.id) {
      return;
    }

    const result = await dispatch(updateBank({id: bank.id, data: payload}));
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
    radioRow: {
      flexDirection: 'row',
      alignItems: 'center',
      marginBottom: spacing.sm,
    },
    radioButton: {
      width: spacing.lg,
      height: spacing.lg,
      borderRadius: spacing.lg,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      alignItems: 'center',
      justifyContent: 'center',
      marginRight: spacing.sm,
      backgroundColor: colors.cardBackground,
    },
    radioDot: {
      width: spacing.sm,
      height: spacing.sm,
      borderRadius: spacing.sm,
      backgroundColor: colors.buttonPrimary,
    },
    radioLabel: {
      fontSize: fontSizes.md,
      color: colors.textPrimary,
      fontWeight: fontWeights.medium,
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
    inputDisabled: {
      opacity: 0.6,
    },
    inputError: {
      borderColor: colors.danger,
      marginBottom: spacing.sm,
    },
    errorText: {
      color: colors.danger,
      marginBottom: spacing.md,
      fontSize: fontSizes.sm,
    },
    pickerButton: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      paddingHorizontal: spacing.md,
      paddingVertical: spacing.sm,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      marginBottom: spacing.lg,
    },
    pickerValue: {
      color: colors.textPrimary,
      fontSize: fontSizes.md,
    },
    modalBackdrop: {
      flex: 1,
      backgroundColor: colors.overlay,
      justifyContent: 'center',
      padding: spacing.lg,
    },
    modalCard: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      paddingVertical: spacing.sm,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
    modalItem: {
      paddingHorizontal: spacing.lg,
      paddingVertical: spacing.sm,
    },
    modalItemText: {
      fontSize: fontSizes.md,
      color: colors.textPrimary,
      fontWeight: fontWeights.medium,
    },
    saveButton: {
      marginTop: spacing.md,
    },
  });

  const title = mode === 'edit' ? translate('UpdateBank') : translate('CreateBank');

  return (
    <View style={s.container}>
      <Header title={title} />
      <View style={s.content}>
        {mode === 'create' ? (
          <>
            <TouchableOpacity style={s.radioRow} onPress={() => setIsDefinitionMode(true)}>
              <View style={s.radioButton}>{isDefinitionMode ? <View style={s.radioDot} /> : null}</View>
              <Text style={s.radioLabel}>{translate('SelectFromBankList')}</Text>
            </TouchableOpacity>
            <TouchableOpacity style={s.radioRow} onPress={() => setIsDefinitionMode(false)}>
              <View style={s.radioButton}>{!isDefinitionMode ? <View style={s.radioDot} /> : null}</View>
              <Text style={s.radioLabel}>{translate('EnterBankNameManually')}</Text>
            </TouchableOpacity>
            <Text style={s.label}>{translate('BankName')}</Text>
            <TouchableOpacity
              style={[s.pickerButton, !isDefinitionMode ? s.inputDisabled : undefined, validationError ? s.inputError : undefined]}
              disabled={!isDefinitionMode}
              onPress={() => setIsDefinitionsOpen(true)}>
              <Text style={s.pickerValue}>{selectedDefinitionName}</Text>
            </TouchableOpacity>
          </>
        ) : null}

        <Text style={s.label}>{translate('BankName')}</Text>
        <TextInput
          value={name}
          onChangeText={value => {
            setName(value);
            if (validationError) {
              setValidationError(undefined);
            }
          }}
          style={[s.input, !isNameEditable ? s.inputDisabled : undefined, validationError ? s.inputError : undefined]}
          editable={isNameEditable}
        />

        {validationError ? <Text style={s.errorText}>{validationError}</Text> : null}

        <Text style={s.label}>{translate('Description')}</Text>
        <TextInput value={description} onChangeText={setDescription} style={s.input} />

        <Button text={translate('Save')} onPress={onSave} style={s.saveButton} isLoading={isSaving} disabled={isSaving} />
      </View>
      <Modal visible={isDefinitionsOpen} transparent animationType="fade" onRequestClose={() => setIsDefinitionsOpen(false)}>
        <TouchableOpacity style={s.modalBackdrop} activeOpacity={1} onPress={() => setIsDefinitionsOpen(false)}>
          <View style={s.modalCard}>
            {definitions.map(item => (
              <TouchableOpacity
                key={item.id}
                style={s.modalItem}
                onPress={() => {
                  setSelectedDefinitionId(item.id);
                  setIsDefinitionsOpen(false);
                  if (validationError) {
                    setValidationError(undefined);
                  }
                }}>
                <Text style={s.modalItemText}>{item.name}</Text>
              </TouchableOpacity>
            ))}
          </View>
        </TouchableOpacity>
      </Modal>
    </View>
  );
}

