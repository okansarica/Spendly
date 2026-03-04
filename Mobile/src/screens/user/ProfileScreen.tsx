// CHANGED_BY_AI: 2026-03-04 - Rebuild profile screen with stable delete challenge

import React, {useEffect, useState} from 'react';
import {Alert, Modal, StyleSheet, Switch, Text, TextInput, View} from 'react-native';
import Header from '../../components/Header';
import ErrorDisplay from '../../components/ErrorDisplay';
import {translate} from '../../utils/translations';
import {useTheme} from '../../theme/ThemeContext';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {deleteUserAccount, loadUserProfile, updateUserProfile} from '../../store/userStore';
import {logoutLocal} from '../../store/authStore';
import Button from '../../components/Button';

type DeleteQuestion = {
  id: number;
  text: string;
  answer: number;
};

export default function ProfileScreen() {
  const dispatch = useAppDispatch();
  const profile = useAppSelector(s => s.user.profile);
  const isSaving = useAppSelector(s => s.user.isSaving);
  const isLoading = useAppSelector(s => s.user.isLoading);
  const error = useAppSelector(s => s.user.error);
  const {colors, spacing, radius, fontSizes} = useTheme();

  const [name, setName] = useState('');
  const [surname, setSurname] = useState('');
  const [newsletter, setNewsletter] = useState(false);

  const [nameError, setNameError] = useState('');
  const [surnameError, setSurnameError] = useState('');

  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [question, setQuestion] = useState<DeleteQuestion | null>(null);
  const [answer, setAnswer] = useState('');
  const [deleteError, setDeleteError] = useState('');

  useEffect(() => {
    if (!profile && !isLoading && !error) {
      dispatch(loadUserProfile());
    }
  }, [profile, isLoading, error]);

  useEffect(() => {
    if (profile) {
      setName(profile.name ?? '');
      setSurname(profile.surname ?? '');
      setNewsletter(profile.isNewsletterSubscribed ?? false);
    }
  }, [profile]);

  const randomInt = (min: number, max: number) =>
      Math.floor(Math.random() * (max - min + 1)) + min;

  // ✅ GUARANTEED 4 DIFFERENT OPERATIONS
  const generateQuestion = (): DeleteQuestion => {
    const operator = ['+', '-', '*', '/'][randomInt(0, 3)];

    if (operator === '+') {
      const a = randomInt(10, 99);
      const b = randomInt(10, 99);
      return {id: 0, text: `${a} + ${b} = ?`, answer: a + b};
    }

    if (operator === '-') {
      const a = randomInt(50, 150);
      const b = randomInt(10, a);
      return {id: 0, text: `${a} - ${b} = ?`, answer: a - b};
    }

    if (operator === '*') {
      const a = randomInt(2, 12);
      const b = randomInt(2, 12);
      return {id: 0, text: `${a} * ${b} = ?`, answer: a * b};
    }

    const divisor = randomInt(2, 12);
    const quotient = randomInt(2, 12);
    const dividend = divisor * quotient;
    return {id: 0, text: `${dividend} / ${divisor} = ?`, answer: quotient};
  };

  const openDeleteModal = () => {
    setQuestion(generateQuestion());
    setAnswer('');
    setDeleteError('');
    setIsDeleteModalOpen(true);
  };

  const closeDeleteModal = () => {
    setIsDeleteModalOpen(false);
    setDeleteError('');
  };

  const handleAnswerChange = (value: string) => {
    setAnswer(value);
    if (deleteError) setDeleteError('');
  };

  const confirmDelete = async () => {
    if (!answer.trim() || !question) {
      setDeleteError(translate('DeleteAccountChallengeError'));
      return;
    }

    const parsed = Number(answer);
    const isCorrect = Number.isFinite(parsed) && parsed === question.answer;

    if (!isCorrect) {
      setDeleteError(translate('DeleteAccountChallengeError'));
      return;
    }

    try {
      await dispatch(deleteUserAccount()).unwrap();
      await dispatch(logoutLocal()).unwrap();
    } catch {
      setDeleteError(translate('DeleteAccountChallengeError'));
    }
  };

  const onSave = async () => {
    const trimmedName = name.trim();
    const trimmedSurname = surname.trim();

    const nError = !trimmedName ? translate('NameRequired') : '';
    const sError = !trimmedSurname ? translate('SurnameRequired') : '';

    setNameError(nError);
    setSurnameError(sError);

    if (nError || sError) return;

    await dispatch(
        updateUserProfile({
          name: trimmedName,
          surname: trimmedSurname,
          isNewsletterSubscribed: newsletter,
        }),
    );

    Alert.alert(translate('SuccessTitle'), translate('ProfileUpdatedMessage'));
  };

  const onDeletePress = () => {
    openDeleteModal();
  };

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
      padding: spacing.md,
      marginTop: spacing.sm,
    },
    error: {color: colors.danger, fontSize: fontSizes.xs},
    row: {flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center'},
    modalBackdrop: {
      flex: 1,
      backgroundColor: colors.overlay,
      justifyContent: 'center',
      padding: spacing.lg,
    },
    modalCard: {
      backgroundColor: colors.cardBackground,
      padding: spacing.lg,
      borderRadius: radius.md,
      gap: spacing.md,
    },
    modalActionRow: {
      flexDirection: 'row',
    },
    modalActionButtonLeft: {
      flex: 1,
      marginRight: spacing.sm,
    },
    modalActionButtonRight: {
      flex: 1,
    },
  });

  return (
      <View style={s.container}>
        <Header title={translate('ProfileTitle')} />

        {error && !profile ? (
          <ErrorDisplay message={error} />
        ) : (
          <View style={s.content}>
          <Text style={s.label}>{translate('EmailTitle')}</Text>
          <Text>{profile?.email}</Text>

          <Text style={s.label}>{translate('FirstNameTitle')}</Text>
          <TextInput value={name} onChangeText={setName} style={s.input} />
          {nameError ? <Text style={s.error}>{nameError}</Text> : null}

          <Text style={s.label}>{translate('LastNameTitle')}</Text>
          <TextInput value={surname} onChangeText={setSurname} style={s.input} />
          {surnameError ? <Text style={s.error}>{surnameError}</Text> : null}

          <View style={s.row}>
            <Text>{translate('NewsletterTitle')}</Text>
            <Switch value={newsletter} onValueChange={setNewsletter} />
          </View>

          <Button text={translate('Save')} onPress={onSave} isLoading={isSaving} />

          <Button
              text={translate('DeleteAccountTitle')}
              onPress={onDeletePress}
              variant="danger"
              disabled={isSaving}
          />
        </View>
        )}

        <Modal visible={isDeleteModalOpen} transparent animationType="fade">
          <View style={s.modalBackdrop}>
            <View style={s.modalCard}>
              <Text>{translate('DeleteAccountChallengeTitle')}</Text>
              <Text>{translate('DeleteAccountChallengeMessage')}</Text>

              {question ? (
                <View>
                  <Text>{question.text}</Text>
                  <TextInput
                      keyboardType="number-pad"
                      value={answer}
                      onChangeText={handleAnswerChange}
                      style={s.input}
                  />
                </View>
              ) : null}

              {deleteError ? <Text style={s.error}>{deleteError}</Text> : null}

              <View style={s.modalActionRow}>
                <Button text={translate('Cancel')} onPress={closeDeleteModal} variant="secondary" style={s.modalActionButtonLeft} />
                <Button text={translate('Delete')} onPress={confirmDelete} variant="danger" isLoading={isSaving} style={s.modalActionButtonRight} />
              </View>
            </View>
          </View>
        </Modal>
      </View>
  );
}
