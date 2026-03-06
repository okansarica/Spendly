// CHANGED_BY_AI: 2026-03-06 - Add banks and accounts listing screen
import React, {useEffect, useMemo, useState} from 'react';
import {View, Text, StyleSheet, TextInput, TouchableOpacity, FlatList, Modal, Alert} from 'react-native';
import {useTheme} from '../../../theme/ThemeContext';
import {translate} from '../../../utils/translations';
import Header from '../../../components/Header';
import ErrorDisplay from '../../../components/ErrorDisplay';
import {useAppDispatch, useAppSelector} from '../../../store/hooks';
import {deleteBank, deleteBankAccount, loadBanks} from '../../../store/banksStore';
import {useNavigation} from '@react-navigation/native';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {FinanceStackParamList} from '../../../navigation/FinanceNavigator';
import Icon from 'react-native-vector-icons/MaterialIcons';
import {useBottomTabBarHeight} from '@react-navigation/bottom-tabs';
import Button from '../../../components/Button';
import Toast from 'react-native-toast-message';
import {BankAccountItem, BankListItem} from '../../../services/banksService';

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
  const [menuPosition, setMenuPosition] = useState<{x: number; y: number} | undefined>(undefined);
  const [isAddBankOptionsOpen, setIsAddBankOptionsOpen] = useState(false);

  useEffect(() => {
    dispatch(loadBanks(undefined));
  }, [dispatch]);

  const data = useMemo(() => {
    const rows: Array<BankRow | AccountRow> = [];
    const needle = search.trim().toLowerCase();

    items.forEach(bank => {
      const matchedAccounts = bank.accounts.filter(account => account.name.toLowerCase().includes(needle));
      const bankMatch = bank.name.toLowerCase().includes(needle);
      if (!needle || bankMatch || matchedAccounts.length > 0) {
        rows.push({type: 'bank', bank});
        (needle ? matchedAccounts : bank.accounts).forEach(account => rows.push({type: 'account', bank, account}));
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
      borderRadius: radius.md,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      padding: spacing.lg,
    },
    chooserTitle: {
      fontSize: fontSizes.lg,
      color: colors.textPrimary,
      fontWeight: fontWeights.semiBold,
      marginBottom: spacing.sm,
    },
    chooserDescription: {
      color: colors.textSecondary,
      fontSize: fontSizes.sm,
      marginBottom: spacing.lg,
    },
    chooserButtonPrimary: {
      marginBottom: spacing.sm,
    },
  });

  const closeMenu = () => {
    setIsMenuOpen(false);
    setMenuState(undefined);
    setMenuPosition(undefined);
  };

  const openBankMenu = (bank: BankListItem, position: {x: number; y: number}) => {
    setMenuState({type: 'bank', bank});
    setMenuPosition(position);
    setIsMenuOpen(true);
  };

  const openAccountMenu = (bank: BankListItem, account: BankAccountItem, position: {x: number; y: number}) => {
    setMenuState({type: 'account', bank, account});
    setMenuPosition(position);
    setIsMenuOpen(true);
  };

  const onBankDelete = (bank: BankListItem) => {
    closeMenu();
    Alert.alert(translate('DeleteBankTitle'), translate('DeleteBankMessage'), [
      {text: translate('Cancel'), style: 'cancel'},
      {
        text: translate('Delete'),
        style: 'destructive',
        onPress: () => {
          dispatch(deleteBank(bank.id));
        },
      },
    ]);
  };

  const onAccountDelete = (bank: BankListItem, account: BankAccountItem) => {
    closeMenu();
    if (account.isConnected) {
      return;
    }
    Alert.alert(translate('DeleteBankAccountTitle'), translate('DeleteBankAccountMessage'), [
      {text: translate('Cancel'), style: 'cancel'},
      {
        text: translate('Delete'),
        style: 'destructive',
        onPress: () => {
          dispatch(deleteBankAccount({bankId: bank.id, id: account.id}));
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

  const renderItem = ({item}: {item: BankRow | AccountRow}) => {
    if (item.type === 'bank') {
      return (
        <TouchableOpacity
          style={[s.card, s.bankCard]}
          onPress={() => navigation.navigate('BankEdit', {mode: 'edit', bank: item.bank})}>
          <View style={s.cardRow}>
            <View style={s.left}>
              <View style={s.iconWrap}>
                <Icon name="account-balance" size={fontSizes.lg} color={colors.textSecondary} />
              </View>
              <View style={{flex: 1}}>
                <Text style={s.title}>{item.bank.name}</Text>
                <Text style={s.subtitle}>{item.bank.accounts.length} {translate('AccountsTitle')}</Text>
              </View>
            </View>
            <TouchableOpacity
              style={s.menuButton}
              onPress={event => openBankMenu(item.bank, {x: event.nativeEvent.pageX, y: event.nativeEvent.pageY})}>
              <Icon name="more-vert" size={fontSizes.lg} style={s.menuIcon} />
            </TouchableOpacity>
          </View>
        </TouchableOpacity>
      );
    }

    return (
      <TouchableOpacity
        style={[s.card, s.accountCard]}
        onPress={() => {
          if (!item.bank.isConnected) {
            navigation.navigate('AccountEdit', {mode: 'edit', bankId: item.bank.id, account: item.account});
          }
        }}>
        <View style={s.cardRow}>
          <View style={s.left}>
            <View style={s.iconWrap}>
              <Icon name="credit-card" size={fontSizes.lg} color={colors.textSecondary} />
            </View>
            <View style={{flex: 1}}>
              <Text style={s.title}>{item.account.name}</Text>
              <Text style={s.subtitle}>{item.account.isConnected ? translate('Connected') : translate('ManualSetup')}</Text>
            </View>
          </View>
          <TouchableOpacity
            style={s.menuButton}
            onPress={event => openAccountMenu(item.bank, item.account, {x: event.nativeEvent.pageX, y: event.nativeEvent.pageY})}>
            <Icon name="more-vert" size={fontSizes.lg} style={s.menuIcon} />
          </TouchableOpacity>
        </View>
      </TouchableOpacity>
    );
  };

  return (
    <View style={s.container}>
      <Header title={translate('BanksAccountsTitle')} />
      {error && items.length === 0 ? (
        <ErrorDisplay message={error} />
      ) : (
        <View style={s.content}>
          <View style={s.searchRow}>
            <TextInput
              value={search}
              onChangeText={setSearch}
              placeholder={translate('SearchBanksAccounts')}
              placeholderTextColor={colors.textSecondary}
              style={s.searchInput}
            />
            <TouchableOpacity style={s.addBankButton} onPress={() => setIsAddBankOptionsOpen(true)}>
              <Text style={s.addBankText}>{translate('AddBank')}</Text>
            </TouchableOpacity>
          </View>
          <FlatList
            data={data}
            renderItem={renderItem}
            keyExtractor={item => (item.type === 'bank' ? `bank-${item.bank.id}` : `account-${item.account.id}`)}
            contentContainerStyle={s.listContent}
            scrollIndicatorInsets={{right: -spacing.sm, bottom: tabBarHeight + spacing.xl}}
            ListEmptyComponent={!isLoading ? <Text style={s.empty}>{translate('NoBanks')}</Text> : undefined}
          />
        </View>
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
                    closeMenu();
                    navigation.navigate('BankEdit', {mode: 'edit', bank});
                  }}>
                  <Text style={s.menuItemText}>{translate('UpdateBank')}</Text>
                </TouchableOpacity>
                {menuState.bank.isConnected ? (
                  <TouchableOpacity style={s.menuItem} onPress={showTodo}>
                    <Text style={s.menuItemText}>{translate('Disconnect')}</Text>
                  </TouchableOpacity>
                ) : (
                  <TouchableOpacity style={s.menuItem} onPress={showTodo}>
                    <Text style={s.menuItemText}>{translate('Connect')}</Text>
                  </TouchableOpacity>
                )}
                <TouchableOpacity
                  style={s.menuItem}
                  onPress={() => {
                    const bank = menuState.bank;
                    closeMenu();
                    navigation.navigate('AccountEdit', {mode: 'create', bankId: bank.id});
                  }}>
                  <Text style={s.menuItemText}>{translate('AddAccount')}</Text>
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
                  disabled={menuState.account.isConnected}
                  onPress={() => onAccountDelete(menuState.bank, menuState.account)}>
                  <Text style={[s.menuItemText, s.menuItemDanger, menuState.account.isConnected ? s.menuItemDisabled : undefined]}>
                    {translate('Delete')}
                  </Text>
                </TouchableOpacity>
                <TouchableOpacity style={s.menuItem} disabled={menuState.account.isConnected} onPress={showTodo}>
                  <Text style={[s.menuItemText, menuState.account.isConnected ? s.menuItemDisabled : undefined]}>{translate('Upload')}</Text>
                </TouchableOpacity>
                {menuState.bank.isConnected && !menuState.account.isConnected ? (
                  <TouchableOpacity style={s.menuItem} onPress={showTodo}>
                    <Text style={s.menuItemText}>{translate('Connect')}</Text>
                  </TouchableOpacity>
                ) : null}
                {menuState.bank.isConnected && menuState.account.isConnected ? (
                  <TouchableOpacity style={s.menuItem} onPress={showTodo}>
                    <Text style={s.menuItemText}>{translate('Disconnect')}</Text>
                  </TouchableOpacity>
                ) : null}
                {!menuState.bank.isConnected ? (
                  <TouchableOpacity
                    style={s.menuItem}
                    onPress={() => {
                      const bankId = menuState.bank.id;
                      const account = menuState.account;
                      closeMenu();
                      navigation.navigate('AccountEdit', {mode: 'edit', bankId, account});
                    }}>
                    <Text style={s.menuItemText}>{translate('UpdateAccount')}</Text>
                  </TouchableOpacity>
                ) : null}
              </>
            ) : null}
          </View>
        </TouchableOpacity>
      </Modal>
      <Modal visible={isAddBankOptionsOpen} transparent animationType="fade" onRequestClose={() => setIsAddBankOptionsOpen(false)}>
        <TouchableOpacity style={s.modalBackdrop} activeOpacity={1} onPress={() => setIsAddBankOptionsOpen(false)}>
          <View style={s.chooserCard}>
            <Text style={s.chooserTitle}>{translate('AddBank')}</Text>
            <Text style={s.chooserDescription}>{translate('OpenBankingDescription')}</Text>
            <Button
              text={translate('OpenBanking')}
              onPress={() => {
                setIsAddBankOptionsOpen(false);
                showTodo();
              }}
              style={s.chooserButtonPrimary}
            />
            <Button
              text={translate('ManualSetup')}
              variant="secondary"
              onPress={() => {
                setIsAddBankOptionsOpen(false);
                navigation.navigate('BankEdit', {mode: 'create'});
              }}
            />
          </View>
        </TouchableOpacity>
      </Modal>
      {isSaving ? <View /> : null}
    </View>
  );
}

