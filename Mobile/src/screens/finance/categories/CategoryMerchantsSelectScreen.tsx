// CHANGED_BY_AI: 2026-03-02 - Add category merchants selection screen
import React, { useCallback, useState, useEffect, useMemo } from 'react';
import { View, Text, StyleSheet, TextInput, TouchableOpacity, FlatList } from 'react-native';
import { useTheme } from '../../../theme/ThemeContext';
import { translate } from '../../../utils/translations';
import Header from '../../../components/Header';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import { loadMerchants } from '../../../store/merchantsStore';
import { setDraftMerchantIds } from '../../../store/categoriesStore';
import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import type { FinanceStackParamList } from '../../../navigation/FinanceNavigator';
import Icon from 'react-native-vector-icons/MaterialIcons';
import { useBottomTabBarHeight } from "@react-navigation/bottom-tabs";

const sortByName = 'name';

export default function CategoryMerchantsSelectScreen() {
    const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
    const dispatch = useAppDispatch();
    const navigation = useNavigation<NativeStackNavigationProp<FinanceStackParamList, 'CategoryMerchantsSelect'>>();
    const merchants = useAppSelector(state => state.merchants.items);
    const draftMerchantIds = useAppSelector(state => state.categories.draftMerchantIds);
    const [search, setSearch] = useState('');
    const [selectedIds, setSelectedIds] = useState<string[]>([]);
    const tabBarHeight = useBottomTabBarHeight();

    const loadData = useCallback(() => {
        dispatch(
            loadMerchants({
                isUncategorized: true,
                sortBy: sortByName,
            })
        );
    }, [dispatch]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const toggleSelect = (id: string) => {
        setSelectedIds(prev => (prev.includes(id) ? prev.filter(item => item !== id) : [...prev, id]));
    };

    const onAssign = () => {
        dispatch(setDraftMerchantIds(Array.from(new Set([...draftMerchantIds, ...selectedIds]))));
        navigation.goBack();
    };

    const filteredMerchants = useMemo(() => {
        const q = search.trim().toLowerCase();
        if (!q) {
            return merchants;
        }
        return merchants.filter(item => item.name.toLowerCase().includes(q));
    }, [merchants, search]);

    const s = StyleSheet.create({
        container: {
            flex: 1,
            backgroundColor: colors.backgroundSecondary,
        },
        content: {
            padding: spacing.lg,
            paddingBottom: spacing.xl * 2 + tabBarHeight  +100, // Add extra padding to account for the bottom bar
        },
        searchInput: {
            backgroundColor: colors.cardBackground,
            borderRadius: radius.md,
            paddingHorizontal: spacing.md,
            paddingVertical: spacing.sm,
            color: colors.textPrimary,
            borderWidth: 1,
            borderColor: colors.borderSubtle,
            marginBottom: spacing.md,
        },
        row: {
            backgroundColor: colors.cardBackground,
            borderRadius: radius.md,
            padding: spacing.md,
            borderWidth: 1,
            borderColor: colors.borderSubtle,
            marginBottom: spacing.sm,
            flexDirection: 'row',
            alignItems: 'center',
            justifyContent: 'space-between',
        },
        rowLeft: {
            flexDirection: 'row',
            alignItems: 'center',
            flex: 1,
        },
        checkbox: {
            width: spacing.lg,
            height: spacing.lg,
            borderRadius: radius.sm,
            borderWidth: 1,
            borderColor: colors.borderSubtle,
            alignItems: 'center',
            justifyContent: 'center',
            marginRight: spacing.sm,
            backgroundColor: colors.backgroundPrimary,
        },
        name: {
            fontSize: fontSizes.md,
            fontWeight: fontWeights.semiBold,
            color: colors.textPrimary,
        },
        meta: {
            fontSize: fontSizes.xs,
            color: colors.textSecondary,
        },
        bottomBar: {
            position: 'absolute',
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: colors.backgroundPrimary,
            borderTopWidth: 1,
            borderTopColor: colors.borderSubtle,
            padding: spacing.md,
            flexDirection: 'row',
            alignItems: 'center',
            justifyContent: 'space-between',
        },
        bottomText: {
            fontSize: fontSizes.sm,
            color: colors.textPrimary,
            fontWeight: fontWeights.semiBold,
        },
        assignButton: {
            backgroundColor: colors.buttonPrimary,
            paddingHorizontal: spacing.lg,
            paddingVertical: spacing.sm,
            borderRadius: radius.md,
        },
        assignText: {
            color: colors.buttonPrimaryText,
            fontWeight: fontWeights.semiBold,
        },
    });

    return (
        <View style={s.container}>
            <Header title={translate('AddMerchant')}/>
            <View style={s.content}>
                <TextInput
                    value={search}
                    onChangeText={setSearch}
                    placeholder={translate('SearchMerchants')}
                    placeholderTextColor={colors.textSecondary}
                    style={s.searchInput}
                />
                <FlatList
                    data={filteredMerchants}
                    keyExtractor={item => item.id}
                    renderItem={({item}) => (
                        <TouchableOpacity style={s.row} onPress={() => toggleSelect(item.id)}>
                            <View style={s.rowLeft}>
                                <View style={s.checkbox}>
                                    {selectedIds.includes(item.id) ? <Icon name="check" size={fontSizes.md}
                                                                           color={colors.buttonPrimary}/> : undefined}
                                </View>
                                <View>
                                    <Text style={s.name}>{item.name}</Text>
                                </View>
                            </View>
                        </TouchableOpacity>
                    )}
                />
            </View>
            {selectedIds.length > 0 ? (
                <View style={s.bottomBar}>
                    <Text style={s.bottomText}>{selectedIds.length} {translate('SelectedMerchants')}</Text>
                    <TouchableOpacity style={s.assignButton} onPress={onAssign}>
                        <Text style={s.assignText}>{translate('AssignMerchants')}</Text>
                    </TouchableOpacity>
                </View>
            ) : undefined}
        </View>
    );
}
