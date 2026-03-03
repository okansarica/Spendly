// CHANGED_BY_AI: 2026-03-03 - Fix update merchant error toast payload
// CHANGED_BY_AI: 2026-03-02 - Add merchant category selection
import React, { useEffect, useMemo, useState } from 'react';
import { View, Text, StyleSheet, TextInput, TouchableOpacity, Modal } from 'react-native';
import { useTheme } from '../../../theme/ThemeContext';
import Header from '../../../components/Header';
import { translate } from '../../../utils/translations';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import { updateMerchant } from '../../../store/merchantsStore';
import { loadCategories } from '../../../store/categoriesStore';
import { CategoryListItem } from '../../../services/categoriesService';
import { useNavigation, useRoute } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import type { FinanceStackParamList } from '../../../navigation/FinanceNavigator';
import Button from '../../../components/Button';
import Toast from "react-native-toast-message";

type FinanceNavProp = NativeStackNavigationProp<FinanceStackParamList, 'MerchantEdit'>;

export default function MerchantEditScreen() {
    const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
    const dispatch = useAppDispatch();
    const navigation = useNavigation<FinanceNavProp>();
    const route = useRoute();
    const params = route.params as FinanceStackParamList['MerchantEdit'];
    const detail = useAppSelector(state => state.merchants.detail);
    const merchants = useAppSelector(state => state.merchants.items);
    const isSaving = useAppSelector(state => state.merchants.isSaving);
    const categories = useAppSelector(state => state.categories.items);
    const [nickname, setNickname] = useState('');
    const [selectedCategoryId, setSelectedCategoryId] = useState<string | undefined>(undefined);
    const [isCategoryOpen, setIsCategoryOpen] = useState(false);

    useEffect(() => {
        dispatch(loadCategories({search: '', sortBy: 'name', sortDirection: 'asc'}));
    }, [dispatch, params.merchantId, merchants]);

    useEffect(() => {
        const fromList = merchants.find(m => m.id === params.merchantId);
        if (fromList) {
            setNickname(fromList.nickname ?? '');
            setSelectedCategoryId(fromList.categoryId);
            return;
        }
        if (detail?.id === params.merchantId) {
            setNickname(detail.nickname ?? '');
            setSelectedCategoryId(detail.categoryId);
        }
    }, [merchants, detail, params.merchantId]);

    const title = useMemo(() => translate('EditMerchant'), []);

    const selectedCategoryName = useMemo(() => {
        if (!selectedCategoryId) {
            return translate('Uncategorized');
        }
        const match = categories.find(item => item.id === selectedCategoryId);
        return match?.name || translate('Uncategorized');
    }, [categories, selectedCategoryId]);

    const onSave = async () => {
        const trimmedNickname = nickname.trim();

        const result = await
            dispatch(
                updateMerchant({
                    id: params.merchantId,
                    nickname: trimmedNickname.length > 0 ? trimmedNickname : undefined,
                    categoryId: selectedCategoryId,
                }),
            );

        if (result.meta.requestStatus !== 'fulfilled') {
            Toast.show({
                type: 'error',
                text1: translate('Error'),
                text2: result.payload as string,
            });
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
        value: {
            fontSize: fontSizes.md,
            color: colors.textPrimary,
            marginBottom: spacing.lg,
            fontWeight: fontWeights.semiBold,
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
        modalItemSecondary: {
            color: colors.textSecondary,
        },
        saveButton: {
            backgroundColor: colors.buttonPrimary,
            paddingVertical: spacing.md,
            borderRadius: radius.md,
            alignItems: 'center',
        },
        saveButtonText: {
            color: colors.buttonPrimaryText,
            fontWeight: fontWeights.semiBold,
        },
    });

    const merchant = merchants.find(m => m.id === params.merchantId) ?? (detail?.id === params.merchantId ? detail : undefined);

    return (
        <View style={s.container}>
            <Header title={title}/>
            <View style={s.content}>
                <Text style={s.label}>{translate('MerchantName')}</Text>
                <Text style={s.value}>{merchant?.name || ''}</Text>
                <Text style={s.label}>{translate('MerchantNickname')}</Text>
                <TextInput value={nickname} onChangeText={setNickname} style={s.input}/>
                <Text style={s.label}>{translate('MerchantCategory')}</Text>
                <TouchableOpacity style={s.pickerButton} onPress={() => setIsCategoryOpen(true)}>
                    <Text style={s.pickerValue}>{selectedCategoryName}</Text>
                </TouchableOpacity>
                <Button
                    text={translate('Save')}
                    onPress={onSave}
                    style={s.saveButton}
                    textStyle={s.saveButtonText}
                    isLoading={isSaving}
                    disabled={isSaving}
                />
            </View>
            <Modal visible={isCategoryOpen} transparent animationType="fade"
                   onRequestClose={() => setIsCategoryOpen(false)}>
                <TouchableOpacity style={s.modalBackdrop} activeOpacity={1} onPress={() => setIsCategoryOpen(false)}>
                    <View style={s.modalCard}>
                        <TouchableOpacity
                            style={s.modalItem}
                            onPress={() => {
                                setSelectedCategoryId(undefined);
                                setIsCategoryOpen(false);
                            }}>
                            <Text style={[s.modalItemText, s.modalItemSecondary]}>{translate('Uncategorized')}</Text>
                        </TouchableOpacity>
                        {categories.map((item: CategoryListItem) => (
                            <TouchableOpacity
                                key={item.id}
                                style={s.modalItem}
                                onPress={() => {
                                    setSelectedCategoryId(item.id);
                                    setIsCategoryOpen(false);
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
