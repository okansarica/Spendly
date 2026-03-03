// CHANGED_BY_AI: 2026-03-02 - Add merchant item menu
// CHANGED_BY_AI: 2026-03-02 - Compact merchant rows with inline category
import React, {useState, useEffect, useMemo} from 'react';
import {View, Text, StyleSheet, TextInput, TouchableOpacity, FlatList, Modal, Alert} from 'react-native';
import {useTheme} from '../../../theme/ThemeContext';
import {translate} from '../../../utils/translations';
import Header from '../../../components/Header';
import {useAppDispatch, useAppSelector} from '../../../store/hooks';
import {loadMerchants, deleteMerchant} from '../../../store/merchantsStore';
import Icon from 'react-native-vector-icons/MaterialIcons';
import {useBottomTabBarHeight} from '@react-navigation/bottom-tabs';
import {MerchantDefaults} from '../../../constants/merchantConstants';
import {useNavigation} from '@react-navigation/native';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {FinanceStackParamList} from '../../../navigation/FinanceNavigator';

export default function MerchantsListScreen() {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const navigation = useNavigation<NativeStackNavigationProp<FinanceStackParamList, 'MerchantsList'>>();
  const tabBarHeight = useBottomTabBarHeight();
  const items = useAppSelector(state => state.merchants.items);
  const isLoading = useAppSelector(state => state.merchants.isLoading);
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [menuMerchantId, setMenuMerchantId] = useState<string | undefined>(undefined);
  const [menuPosition, setMenuPosition] = useState<{x: number; y: number} | undefined>(undefined);

  useEffect(() => {
    const id = setTimeout(() => {
      setDebouncedSearch(search.trim());
    }, MerchantDefaults.SearchDebounceMs);
    return () => clearTimeout(id);
  }, [search]);

  useEffect(() => {
    dispatch(loadMerchants({sortBy: 'name', sortDirection: 'asc'}));
  }, [dispatch]);

  const filteredItems = useMemo(() => {
    const needle = debouncedSearch.toLowerCase();
    const list = items.filter(item => (needle ? item.name.toLowerCase().includes(needle) : true));
    return [...list].sort((a, b) => a.name.localeCompare(b.name));
  }, [items, debouncedSearch]);

  const hasMerchants = items.length > 0;
  const hasFiltered = filteredItems.length > 0;

  const s = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.backgroundSecondary,
    },
    content: {
      padding: spacing.lg,
    },
    listContent: {
      paddingBottom: tabBarHeight + spacing.xl,
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
    card: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      padding: spacing.md,
      marginBottom: spacing.sm,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
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
    },
    title: {
      fontSize: fontSizes.md,
      fontWeight: fontWeights.semiBold,
      color: colors.textPrimary,
    },
    categoryText: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
    },
    titleRow: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: spacing.xs,
    },
    menuButton: {
      padding: spacing.xs,
      marginLeft: spacing.sm,
    },
    menuIcon: {
      color: colors.textSecondary,
    },
    emptyTitle: {
      textAlign: 'center',
      color: colors.textPrimary,
      fontSize: fontSizes.md,
      fontWeight: fontWeights.semiBold,
      marginTop: spacing.xl,
    },
    emptyDescription: {
      textAlign: 'center',
      color: colors.textSecondary,
      marginTop: spacing.xs,
    },
    emptyButton: {
      alignSelf: 'center',
      marginTop: spacing.md,
      paddingHorizontal: spacing.md,
      paddingVertical: spacing.sm,
      borderRadius: radius.md,
      backgroundColor: colors.buttonSecondary,
    },
    emptyButtonText: {
      color: colors.buttonSecondaryText,
      fontWeight: fontWeights.semiBold,
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
  });

  const openMenu = (merchantId: string, position: {x: number; y: number}) => {
    setMenuMerchantId(merchantId);
    setMenuPosition(position);
    setIsMenuOpen(true);
  };

  const closeMenu = () => {
    setIsMenuOpen(false);
    setMenuMerchantId(undefined);
    setMenuPosition(undefined);
  };

  const onMenuEdit = () => {
    if (!menuMerchantId) {
      return;
    }
    const id = menuMerchantId;
    closeMenu();
    navigation.navigate('MerchantEdit', {merchantId: id});
  };

  const onMenuDelete = () => {
    if (!menuMerchantId) {
      return;
    }
    const id = menuMerchantId;
    closeMenu();
    Alert.alert(
      translate('DeleteMerchantTitle'),
      translate('DeleteMerchantMessage'),
      [
        {text: translate('Cancel'), style: 'cancel'},
        {
          text: translate('Delete'),
          style: 'destructive',
          onPress: () => {
            dispatch(deleteMerchant(id));
          },
        },
      ]
    );
  };

  return (
    <View style={s.container}>
      <Header title={translate('MerchantsTitle')} />
      <View style={s.content}>
        <View style={s.searchRow}>
          <TextInput
            value={search}
            onChangeText={setSearch}
            placeholder={translate('SearchMerchants')}
            placeholderTextColor={colors.textSecondary}
            style={s.searchInput}
          />
        </View>
        {hasMerchants && !hasFiltered && !isLoading ? (
          <View>
            <Text style={s.emptyTitle}>{translate('NoMerchantsFilteredTitle')}</Text>
            <Text style={s.emptyDescription}>{translate('NoMerchantsFilteredDescription')}</Text>
            <TouchableOpacity
              style={s.emptyButton}
              onPress={() => {
                setSearch('');
                setDebouncedSearch('');
              }}>
              <Text style={s.emptyButtonText}>{translate('ClearFilters')}</Text>
            </TouchableOpacity>
          </View>
        ) : !hasMerchants && !isLoading ? (
          <View>
            <Text style={s.emptyTitle}>{translate('NoMerchantsTitle')}</Text>
            <Text style={s.emptyDescription}>{translate('NoMerchantsDescription')}</Text>
          </View>
        ) : (
          <>
            <FlatList
              data={filteredItems}
              keyExtractor={item => item.id}
              contentContainerStyle={s.listContent}
              renderItem={({item}) => (
                <TouchableOpacity style={s.card} onPress={() => navigation.navigate('MerchantEdit', {merchantId: item.id})}>
                  <View style={s.cardRow}>
                    <View style={s.left}>
                      <Icon name="store" size={fontSizes.lg} color={colors.textSecondary} style={{marginRight: spacing.md}} />
                      <View style={s.titleRow}>
                        <Text style={s.title}>{item.nickname ? `${item.nickname} (${item.name})` : item.name}</Text>
                        <Text style={s.categoryText}>• {item.categoryName || translate('Uncategorized')}</Text>
                      </View>
                    </View>
                    <TouchableOpacity style={s.menuButton} onPress={event => openMenu(item.id, {x: event.nativeEvent.pageX, y: event.nativeEvent.pageY})}>
                      <Icon name="more-vert" size={fontSizes.lg} style={s.menuIcon} />
                    </TouchableOpacity>
                  </View>
                </TouchableOpacity>
              )}
            />
          </>
        )}
      </View>
      <Modal visible={isMenuOpen} transparent animationType="fade" onRequestClose={closeMenu}>
        <TouchableOpacity style={s.modalBackdrop} activeOpacity={1} onPress={closeMenu}>
          <View style={s.menuCard}>
            <TouchableOpacity style={s.menuItem} onPress={onMenuEdit}>
              <Text style={s.menuItemText}>{translate('EditMerchant')}</Text>
            </TouchableOpacity>
            <TouchableOpacity style={s.menuItem} onPress={onMenuDelete}>
              <Text style={[s.menuItemText, s.menuItemDanger]}>{translate('Delete')}</Text>
            </TouchableOpacity>
          </View>
        </TouchableOpacity>
      </Modal>
    </View>
  );
}
