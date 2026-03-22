// CHANGED_BY_AI: 2026-03-06 - Add banks and accounts listing screen
// CHANGED_BY_AI: 2026-03-10 - Trigger Plaid update-mode flow from connected bank AddAccount
import React, { useEffect, useMemo, useState } from 'react';
import { View, Text, StyleSheet, TextInput, TouchableOpacity, FlatList, Modal, Alert } from 'react-native';
import { useTheme } from '../../../theme/ThemeContext';
import { translate } from '../../../utils/translations';
import Header from '../../../components/Header';
import ErrorDisplay from '../../../components/ErrorDisplay';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import { deleteBank, deleteBankAccount, loadBanks } from '../../../store/banksStore';
import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import type { FinanceStackParamList } from '../../../navigation/FinanceNavigator';
import Icon from 'react-native-vector-icons/MaterialIcons';
import { useBottomTabBarHeight } from '@react-navigation/bottom-tabs';
import Toast from 'react-native-toast-message';
import { BankAccountItem, BankListItem } from '../../../services/banksService';

import {
    create,
    open,
    dismissLink,
    LinkSuccess,
    LinkExit,
    LinkOpenProps,
    LinkTokenConfiguration
} from 'react-native-plaid-link-sdk';
import Button from '../../../components/Button';
import { CompleteIntegrationRequest, plaidService, PlaidFlowMode } from "../../../services/plaidService.ts";

type FinanceNavProp = NativeStackNavigationProp<FinanceStackParamList, 'BanksAccountsList'>;

type BankRow = {
    type: 'bank';
    bank: BankListItem;
};

type AccountRow = {
    type: 'account';
    bank: BankListItem;
    account: BankAccountItem;
};

type BankMenuState = {
    type: 'bank';
    bank: BankListItem;
};

type AccountMenuState = {
    type: 'account';
    bank: BankListItem;
    account: BankAccountItem;
};

type MenuState = BankMenuState | AccountMenuState;

export default function BanksAccountsListScreen() {
    const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
    const dispatch = useAppDispatch();
    const navigation = useNavigation<FinanceNavProp>();
    const tabBarHeight = useBottomTabBarHeight();
    const items = useAppSelector(state => state.banks.items);
    const isLoading = useAppSelector(state => state.banks.isLoading);
    const isSaving = useAppSelector(state => state.banks.isSaving);
    const error = useAppSelector(state => state.banks.error);
    const [search, setSearch] = useState('');
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const [menuState, setMenuState] = useState<MenuState | undefined>(undefined);
    const [menuPosition, setMenuPosition] = useState<{ x: number; y: number } | undefined>(undefined);
    const [isAddBankOptionsOpen, setIsAddBankOptionsOpen] = useState(false);
    const [addBankMode, setAddBankMode] = useState<'openBanking' | 'manual' | undefined>('openBanking');
    const [isPlaidLoading, setIsPlaidLoading] = useState(false);

    useEffect(() => {
        dispatch(loadBanks());
    }, [dispatch]);

    const data = useMemo(() => {
        console.log("use memo - ITEMS", items.length);
        const rows: Array<BankRow | AccountRow> = [];
        const needle = search.trim().toLowerCase();

        items.forEach(bank => {
            const matchedAccounts = bank.accounts.filter(account => account.name.toLowerCase().includes(needle));
            const bankMatch = bank.name.toLowerCase().includes(needle);
            if (!needle || bankMatch || matchedAccounts.length > 0) {
                rows.push({type: 'bank', bank});
                (needle ? matchedAccounts : bank.accounts).forEach(account => rows.push({
                    type: 'account',
                    bank,
                    account
                }));
            }
        });

        return rows;
    }, [items, search]);

    const s = StyleSheet.create({
        container: {
            flex: 1,
            backgroundColor: colors.backgroundSecondary,
        },
        content: {
            padding: spacing.lg,
        },
        listContent: {
            paddingBottom: tabBarHeight + spacing.xl + 50,
        },
        searchRow: {
            flexDirection: 'row',
            alignItems: 'center',
            marginBottom: spacing.md,
            gap: spacing.sm,
        },
        searchInput: {
            flex: 1,
            backgroundColor: colors.cardBackground,
            borderRadius: radius.md,
            paddingHorizontal: spacing.md,
            paddingVertical: spacing.sm,
            color: colors.textPrimary,
            borderWidth: 1,
            borderColor: colors.borderSubtle,
        },
        addBankButton: {
            paddingHorizontal: spacing.md,
            paddingVertical: spacing.sm,
            borderRadius: radius.md,
            backgroundColor: colors.buttonPrimary,
        },
        addBankText: {
            color: colors.buttonPrimaryText,
            fontWeight: fontWeights.semiBold,
        },
        card: {
            backgroundColor: colors.cardBackground,
            borderRadius: radius.md,
            padding: spacing.md,
            marginBottom: spacing.sm,
            borderWidth: 1,
            borderColor: colors.borderSubtle,
            borderLeftWidth: 6,
        },
        bankCard: {
            borderLeftColor: colors.buttonPrimary,
        },
        accountCard: {
            marginLeft: spacing.md,
            borderLeftColor: colors.buttonSecondaryText,
        },
        cardRow: {
            flexDirection: 'row',
            alignItems: 'center',
            justifyContent: 'space-between',
        },
        left: {
            flexDirection: 'row',
            alignItems: 'center',
            flex: 1,
            marginRight: spacing.sm,
        },
        iconWrap: {
            width: spacing.xl + spacing.xs,
            height: spacing.xl + spacing.xs,
            borderRadius: radius.md,
            alignItems: 'center',
            justifyContent: 'center',
            marginRight: spacing.md,
            backgroundColor: colors.buttonSecondary,
            position: 'relative',
        },
        connectedIndicator: {
            position: 'absolute',
            top: -2,
            right: -2,
            width: fontSizes.sm + 2,
            height: fontSizes.sm + 2,
            borderRadius: (fontSizes.sm + 2) / 2,
            backgroundColor: colors.success,
            alignItems: 'center',
            justifyContent: 'center',
            borderWidth: 2,
            borderColor: colors.cardBackground,
        },
        title: {
            fontSize: fontSizes.md,
            fontWeight: fontWeights.semiBold,
            color: colors.textPrimary,
            flexShrink: 1,
        },
        subtitle: {
            fontSize: fontSizes.sm,
            color: colors.textSecondary,
            marginTop: spacing.xs,
        },
        menuButton: {
            padding: spacing.xs,
            marginLeft: spacing.sm,
        },
        menuIcon: {
            color: colors.textSecondary,
        },
        modalBackdrop: {
            flex: 1,
            backgroundColor: colors.overlay,
            justifyContent: 'center',
            padding: spacing.lg,
        },
        menuCard: {
            position: 'absolute',
            right: spacing.lg,
            top: menuPosition?.y ?? spacing.lg,
            backgroundColor: colors.cardBackground,
            borderRadius: radius.md,
            paddingVertical: spacing.sm,
            borderWidth: 1,
            borderColor: colors.borderSubtle,
            minWidth: 180,
        },
        menuItem: {
            paddingHorizontal: spacing.lg,
            paddingVertical: spacing.sm,
        },
        menuItemText: {
            fontSize: fontSizes.md,
            color: colors.textPrimary,
            fontWeight: fontWeights.medium,
        },
        menuItemDanger: {
            color: colors.danger,
        },
        menuItemDisabled: {
            color: colors.textSecondary,
            opacity: 0.55,
        },
        empty: {
            textAlign: 'center',
            color: colors.textSecondary,
            marginTop: spacing.xl,
        },
        chooserCard: {
            backgroundColor: colors.cardBackground,
            borderRadius: radius.lg,
            borderWidth: 1,
            borderColor: colors.borderSubtle,
            padding: spacing.lg,
            width: '100%',
            maxWidth: 360,
            alignSelf: 'center',
            shadowColor: colors.cardShadow,
            shadowOffset: {width: 0, height: 6},
            shadowOpacity: 0.2,
            shadowRadius: 12,
            elevation: 8,
        },
        chooserHeaderIcon: {
            width: 44,
            height: 44,
            borderRadius: 22,
            backgroundColor: colors.buttonPrimary + '18',
            alignItems: 'center',
            justifyContent: 'center',
            alignSelf: 'center',
            marginBottom: spacing.sm,
        },
        chooserHeaderIconText: {
            color: colors.buttonPrimary,
            fontSize: fontSizes.lg,
            fontWeight: fontWeights.bold,
        },
        chooserTitle: {
            fontSize: fontSizes.lg,
            color: colors.textPrimary,
            fontWeight: fontWeights.semiBold,
            marginBottom: spacing.xs,
            textAlign: 'center',
        },
        chooserDescription: {
            color: colors.textSecondary,
            fontSize: fontSizes.sm,
            marginBottom: spacing.md,
            textAlign: 'center',
        },
        optionItem: {
            borderWidth: 1,
            borderColor: colors.borderSubtle,
            borderRadius: radius.md,
            paddingHorizontal: spacing.md,
            paddingVertical: spacing.sm,
            marginBottom: spacing.sm,
            backgroundColor: colors.backgroundPrimary,
        },
        optionItemSelected: {
            borderColor: colors.buttonPrimary,
            backgroundColor: colors.buttonPrimary + '10',
        },
        optionRow: {
            flexDirection: 'row',
            alignItems: 'center',
            justifyContent: 'space-between',
        },
        optionLabel: {
            color: colors.textPrimary,
            fontSize: fontSizes.md,
            fontWeight: fontWeights.medium,
        },
        optionDescription: {
            color: colors.textSecondary,
            fontSize: fontSizes.sm,
            marginTop: spacing.xs,
        },
        optionCheck: {
            width: 20,
            height: 20,
            borderRadius: 10,
            borderWidth: 1,
            borderColor: colors.buttonPrimary,
            alignItems: 'center',
            justifyContent: 'center',
            backgroundColor: colors.buttonPrimary,
        },
        optionCheckText: {
            color: colors.buttonPrimaryText,
            fontSize: fontSizes.xs,
            fontWeight: fontWeights.bold,
        },
        actionRow: {
            flexDirection: 'row',
            justifyContent: 'flex-end',
            gap: spacing.sm,
            marginTop: spacing.sm,
        },
        actionButton: {
            paddingHorizontal: spacing.md,
            paddingVertical: spacing.xs + 2,
            borderRadius: radius.md,
            borderWidth: 1,
            borderColor: colors.borderSubtle,
            backgroundColor: colors.cardBackground,
            minWidth: 88,
            alignItems: 'center',
        },
        actionButtonPrimary: {
            backgroundColor: colors.buttonPrimary,
            borderColor: colors.buttonPrimary,
        },
        actionButtonDisabled: {
            opacity: 0.5,
        },
        actionButtonText: {
            color: colors.textPrimary,
            fontSize: fontSizes.sm,
            fontWeight: fontWeights.semiBold,
        },
        actionButtonPrimaryText: {
            color: colors.buttonPrimaryText,
        },
        footerWrap: {
            marginTop: spacing.md,
            paddingHorizontal: spacing.lg,
            paddingBottom: spacing.lg,
            alignItems: 'center',
        },
        bottomAddButton: {
            width: '100%',
            paddingVertical: spacing.md,
            borderRadius: radius.lg,
            alignItems: 'center',
        },
    });

    const closeMenu = () => {
        setIsMenuOpen(false);
        setMenuState(undefined);
        setMenuPosition(undefined);
    };

    const openBankMenu = (bank: BankListItem, position: { x: number; y: number }) => {
        setMenuState({type: 'bank', bank});
        setMenuPosition(position);
        setIsMenuOpen(true);
    };

    const openAccountMenu = (bank: BankListItem, account: BankAccountItem, position: { x: number; y: number }) => {
        setMenuState({type: 'account', bank, account});
        setMenuPosition(position);
        setIsMenuOpen(true);
    };

    const onBankDelete = (bank: BankListItem) => {
        closeMenu();
        if (!bank.isConnected && bank.accounts.length > 0) {
            Toast.show({type: 'error', text1: translate('Error'), text2: translate('DeleteAccountsFirstMessage')});
            return;
        }
        Alert.alert(translate('DeleteBankTitle'), translate('DeleteBankMessage'), [
            {text: translate('Cancel'), style: 'cancel'},
            {
                text: translate('Delete'),
                style: 'destructive',
                onPress: async () => {
                    const result = await dispatch(deleteBank(bank.id));
                    if (result.meta.requestStatus !== 'fulfilled') {
                        Toast.show({type: 'error', text1: translate('Error'), text2: result.payload as string});
                    }
                },
            },
        ]);
    };

    const onAccountDelete = (bank: BankListItem, account: BankAccountItem) => {
        closeMenu();
        Alert.alert(translate('DeleteBankAccountTitle'), translate('DeleteBankAccountMessage'), [
            {text: translate('Cancel'), style: 'cancel'},
            {
                text: translate('Delete'),
                style: 'destructive',
                onPress: async () => {
                    const result = await dispatch(deleteBankAccount({bankId: bank.id, id: account.id}));
                    if (result.meta.requestStatus !== 'fulfilled') {
                        Toast.show({type: 'error', text1: translate('Error'), text2: result.payload as string});
                    }
                },
            },
        ]);
    };

    const showTodo = () => {
        closeMenu();
        Toast.show({
            type: 'info',
            text1: translate('Info'),
            text2: translate('FeatureComingSoon'),
        });
    };

    const getErrorMessage = (err: any) => err?.response?.data?.message ?? err?.message ?? translate('An unexpected error occurred');

    const showPlaidError = (err: any) => {
        Toast.show({
            type: 'error',
            text1: translate('Error'),
            text2: getErrorMessage(err)
        });
    };

    const openPlaidFlow = async (linkToken: string, mode: PlaidFlowMode, bankId?: string) => {
        const tokenConfiguration: LinkTokenConfiguration = {
            token: linkToken,
        };

        const openProps: LinkOpenProps = {
            onSuccess: async (linkSuccess: LinkSuccess) => {
                if (!linkSuccess.metadata?.accounts?.length || !linkSuccess.metadata?.institution) {
                    showPlaidError(new Error('No accounts were linked. Please link at least one account to continue.'));
                    dismissLink();
                    return;
                }

                const request: CompleteIntegrationRequest = {
                    publicToken: linkSuccess.publicToken,
                    mode,
                    bankId,
                    accounts: linkSuccess.metadata.accounts.map(a => ({
                        verificationStatus: a.verificationStatus?.toString(),
                        type: a.type,
                        mask: a.mask,
                        name: a.name,
                        subtype: a.subtype?.toString(),
                        id: a.id,
                    })),
                    institution: {
                        name: linkSuccess.metadata.institution.name,
                        id: linkSuccess.metadata.institution.id,
                    },
                    linkSessionId: linkSuccess.metadata.linkSessionId,
                };

                try {
                    await plaidService.completeIntegration(request);
                    await dispatch(loadBanks()).unwrap();
                    setIsAddBankOptionsOpen(false);
                    setAddBankMode(undefined);
                    
                    // Show success message
                    Toast.show({
                        type: 'success',
                        text1: translate('SuccessTitle'),
                        text2: translate('BankDataProcessingMessage'),
                        visibilityTime: 5000,
                    });
                } catch (err: any) {
                    console.log('Error when completing transaction', err);
                    showPlaidError(err);
                } finally {
                    setIsPlaidLoading(false);
                }
            },
            onExit: (exit: LinkExit) => {
                if (exit?.error) {
                    showPlaidError(exit.error);
                }
                dismissLink();
                setIsPlaidLoading(false);
            },
        };

        create(tokenConfiguration);
        open(openProps);
    };

    const startConnectedBankUpdateFlow = async (bank: BankListItem) => {
        closeMenu();
        try {
            setIsPlaidLoading(true);
            const linkTokenResponse = await plaidService.createPlaidLinkToken({mode: 'update', bankId: bank.id});
            await openPlaidFlow(linkTokenResponse.data.linkToken, 'update', bank.id);
        } catch (err: any) {
            console.log(err);
            showPlaidError(err);
            setIsPlaidLoading(false);
        }
    };

    const onContinueAddBank = async () => {
        if (!addBankMode) return;

        if (addBankMode === 'openBanking') {
            try {
                setIsPlaidLoading(true);
                const linkTokenResponse = await plaidService.createPlaidLinkToken({mode: 'create'});

                //TODO linkTokenResponse success donmeyebilir kontrol et

                await openPlaidFlow(linkTokenResponse.data.linkToken, 'create');
            } catch (err: any) {
                console.log(err);
                showPlaidError(err);
                setIsPlaidLoading(false);
            }
            return;
        }

        setIsAddBankOptionsOpen(false);
        setAddBankMode(undefined);
        navigation.navigate('BankEdit', {mode: 'create'});
    };

    const renderItem = ({item}: { item: BankRow | AccountRow }) => {
        if (item.type === 'bank') {
            return (
                <TouchableOpacity
                    style={[s.card, s.bankCard]}
                    onPress={() => navigation.navigate('BankEdit', {mode: 'edit', bank: item.bank})}>
                    <View style={s.cardRow}>
                        <View style={s.left}>
                            <View style={s.iconWrap}>
                                <Icon name="account-balance" size={fontSizes.lg} color={colors.textSecondary}/>
                                {item.bank.isConnected && (
                                    <View style={s.connectedIndicator}>
                                        <Icon name="link" size={fontSizes.xs} color={colors.success}/>
                                    </View>
                                )}
                            </View>
                            <View style={{flex: 1}}>
                                <Text style={s.title}>{item.bank.name}</Text>
                                <Text style={s.subtitle}>{item.bank.accounts.length} {translate('AccountsTitle')}</Text>
                            </View>
                        </View>
                        <TouchableOpacity
                            style={s.menuButton}
                            onPress={event => openBankMenu(item.bank, {
                                x: event.nativeEvent.pageX,
                                y: event.nativeEvent.pageY
                            })}>
                            <Icon name="more-vert" size={fontSizes.lg} style={s.menuIcon}/>
                        </TouchableOpacity>
                    </View>
                </TouchableOpacity>
            );
        }

        return (
            <TouchableOpacity
                style={[s.card, s.accountCard]}
                onPress={() => {
                    navigation.navigate('AccountEdit', {mode: 'edit', bankId: item.bank.id, account: item.account});
                }}>
                <View style={s.cardRow}>
                    <View style={s.left}>
                        <View style={s.iconWrap}>
                            <Icon name="credit-card" size={fontSizes.lg} color={colors.textSecondary}/>
                        </View>
                        <View style={{flex: 1}}>
                            <Text style={s.title}>{item.account.name}</Text>
                            <Text style={s.subtitle}>
                                {item.account.mask ? `**** ${item.account.mask}` : ''}
                            </Text>
                        </View>
                    </View>
                    <TouchableOpacity
                        style={s.menuButton}
                        onPress={event => openAccountMenu(item.bank, item.account, {
                            x: event.nativeEvent.pageX,
                            y: event.nativeEvent.pageY
                        })}>
                        <Icon name="more-vert" size={fontSizes.lg} style={s.menuIcon}/>
                    </TouchableOpacity>
                </View>
            </TouchableOpacity>
        );
    };

    return (
        <View style={s.container}>
            <Header title={translate('BanksAccountsTitle')}/>
            {error && items.length === 0 ? (
                <ErrorDisplay message={error}/>
            ) : (
                <>
                    <View style={s.content}>
                        <View style={s.searchRow}>
                            <TextInput
                                value={search}
                                onChangeText={setSearch}
                                placeholder={translate('SearchBanksAccounts')}
                                placeholderTextColor={colors.textSecondary}
                                style={s.searchInput}
                            />
                        </View>
                        <FlatList
                            data={data}
                            renderItem={renderItem}
                            keyExtractor={item => (item.type === 'bank' ? `bank-${item.bank.id}` : `account-${item.bank.id}-${item.account.id}`)}
                            contentContainerStyle={s.listContent}
                            scrollIndicatorInsets={{right: -spacing.sm, bottom: tabBarHeight + spacing.xl}}
                            ListEmptyComponent={!isLoading ?
                                <Text style={s.empty}>{translate('NoBanks')}</Text> : undefined}
                            ListFooterComponent={() => (
                                <View style={s.footerWrap}>
                                    <Button
                                        text={translate('AddBank')}
                                        onPress={() => {
                                            setAddBankMode('openBanking');
                                            setIsAddBankOptionsOpen(true);
                                        }}
                                        variant="primary"
                                        size="small"
                                        style={s.bottomAddButton}
                                        textStyle={s.addBankText}
                                    />
                                </View>
                            )}
                        />
                    </View>

                </>
            )}
            <Modal visible={isMenuOpen} transparent animationType="fade" onRequestClose={closeMenu}>
                <TouchableOpacity style={s.modalBackdrop} activeOpacity={1} onPress={closeMenu}>
                    <View style={s.menuCard}>
                        {menuState?.type === 'bank' ? (
                            <>
                                <TouchableOpacity
                                    style={s.menuItem}
                                    onPress={() => {
                                        const bank = menuState.bank;
                                        if (bank.isConnected) {
                                            startConnectedBankUpdateFlow(bank);
                                        }
                                        else {
                                            closeMenu();
                                            navigation.navigate('AccountEdit', {mode: 'create', bankId: bank.id});
                                        }
                                    }}>
                                    {menuState.bank.isConnected ?
                                        <Text style={s.menuItemText}>{translate('ReselectAccounts')}</Text>
                                        :
                                        <Text style={s.menuItemText}>{translate('AddAccount')}</Text>
                                    }

                                </TouchableOpacity>

                                <TouchableOpacity
                                    style={s.menuItem}
                                    onPress={() => {
                                        const bank = menuState.bank;
                                        closeMenu();
                                        navigation.navigate('BankEdit', {mode: 'edit', bank});
                                    }}>
                                    <Text style={s.menuItemText}>{translate('Update')}</Text>
                                </TouchableOpacity>
                                <TouchableOpacity style={s.menuItem} onPress={() => onBankDelete(menuState.bank)}>
                                    <Text style={[s.menuItemText, s.menuItemDanger]}>{translate('Delete')}</Text>
                                </TouchableOpacity>
                            </>
                        ) : null}
                        {menuState?.type === 'account' ? (
                            <>
                                <TouchableOpacity
                                    style={s.menuItem}
                                    onPress={() => {
                                        const bankId = menuState.bank.id;
                                        const account = menuState.account;
                                        closeMenu();
                                        navigation.navigate('AccountEdit', {mode: 'edit', bankId, account});
                                    }}>
                                    <Text style={s.menuItemText}>{translate('Update')}</Text>
                                </TouchableOpacity>

                                <TouchableOpacity
                                    style={s.menuItem}
                                    onPress={() => onAccountDelete(menuState.bank, menuState.account)}>
                                    <Text
                                        style={[s.menuItemText, s.menuItemDanger]}>
                                        {translate('Delete')}
                                    </Text>
                                </TouchableOpacity>
                            </>
                        ) : null}
                    </View>
                </TouchableOpacity>
            </Modal>
            <Modal
                visible={isAddBankOptionsOpen}
                transparent
                animationType="fade"
                onRequestClose={() => {
                    // Intentionally no-op: modal should only close via Cancel/Continue
                }}>
                <View style={s.modalBackdrop}>
                    <View style={s.chooserCard}>
                        <View style={s.chooserHeaderIcon}>
                            <Text style={s.chooserHeaderIconText}>$</Text>
                        </View>
                        <Text style={s.chooserTitle}>{translate('AddBank')}</Text>
                        <Text style={s.chooserDescription}>{translate('SelectAddBankMethod')}</Text>

                        <TouchableOpacity
                            style={[s.optionItem, addBankMode === 'openBanking' ? s.optionItemSelected : undefined]}
                            onPress={() => setAddBankMode('openBanking')}>
                            <View style={s.optionRow}>
                                <Text style={s.optionLabel}>{translate('OpenBanking')}</Text>
                                {addBankMode === 'openBanking' ? (
                                    <View style={s.optionCheck}>
                                        <Text style={s.optionCheckText}>✓</Text>
                                    </View>
                                ) : null}
                            </View>
                            <Text style={s.optionDescription}>{translate('OpenBankingDescription')}</Text>
                            {addBankMode === 'openBanking' ? (
                                <Text
                                    style={[s.optionDescription, {marginTop: 8}]}>{translate('OpenBankingUsageExplanation')}</Text>
                            ) : null}
                        </TouchableOpacity>

                        <TouchableOpacity
                            style={[s.optionItem, addBankMode === 'manual' ? s.optionItemSelected : undefined]}
                            onPress={() => setAddBankMode('manual')}>
                            <View style={s.optionRow}>
                                <Text style={s.optionLabel}>{translate('ManualSetup')}</Text>
                                {addBankMode === 'manual' ? (
                                    <View style={s.optionCheck}>
                                        <Text style={s.optionCheckText}>✓</Text>
                                    </View>
                                ) : null}
                            </View>
                            <Text style={s.optionDescription}>{translate('ManualSetupDescription')}</Text>
                            {addBankMode === 'manual' ? (
                                <Text
                                    style={[s.optionDescription, {marginTop: 8}]}>{translate('ManualSetupUsageExplanation')}</Text>
                            ) : null}
                        </TouchableOpacity>

                        <View style={s.actionRow}>
                            <Button
                                text={translate('Cancel')}
                                onPress={() => {
                                    setIsAddBankOptionsOpen(false);
                                    setAddBankMode(undefined);
                                }}
                                variant="secondary"
                                size="small"
                                style={[s.actionButton, isPlaidLoading ? s.actionButtonDisabled : undefined]}
                                textStyle={s.actionButtonText}
                                disabled={isPlaidLoading}
                            />

                            <Button
                                text={translate('Continue')}
                                onPress={onContinueAddBank}
                                variant="primary"
                                size="small"
                                style={[s.actionButton, s.actionButtonPrimary, !addBankMode ? s.actionButtonDisabled : undefined]}
                                textStyle={[s.actionButtonText, s.actionButtonPrimaryText]}
                                disabled={!addBankMode}
                                isLoading={isPlaidLoading}
                            />
                        </View>
                    </View>
                </View>
            </Modal>
            {isSaving ? <View/> : null}
        </View>
    
);
}

