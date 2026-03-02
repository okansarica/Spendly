// CHANGED_BY_AI: 2026-03-02 - Add category merchants selection screen
import React, {useCallback, useMemo, useState, useEffect} from 'react';
import {View, Text, StyleSheet, TextInput, TouchableOpacity, FlatList} from 'react-native';
import {useTheme} from '../../../theme/ThemeContext';
import {translate} from '../../../utils/translations';
import Header from '../../../components/Header';
import {useAppDispatch, useAppSelector} from '../../../store/hooks';
import {loadMerchants} from '../../../store/merchantsStore';
import {addCategoryMerchants, setDraftMerchantIds} from '../../../store/categoriesStore';
import {useNavigation, useRoute} from '@react-navigation/native';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {FinanceStackParamList} from '../../../navigation/FinanceNavigator';
import Icon from 'react-native-vector-icons/MaterialIcons';

const sortByName = 'name';
const sortDesc = 'desc';
const sortAsc = 'asc';

export default function CategoryMerchantsSelectScreen() {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const navigation = useNavigation<NativeStackNavigationProp<FinanceStackParamList, 'CategoryMerchantsSelect'>>();
  const route = useRoute();
  const params = route.params as FinanceStackParamList['CategoryMerchantsSelect'];
  const merchants = useAppSelector(state => state.merchants.items);
  const draftMerchantIds = useAppSelector(state => state.categories.draftMerchantIds);
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<'recent' | 'spend' | 'count' | undefined>(undefined);
  const [selectedIds, setSelectedIds] = useState<string[]>(draftMerchantIds);

  const loadData = useCallback(() => {
    const now = new Date();
    const start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
    const startDate = filter === 'recent' ? start.toISOString() : undefined;
    const endDate = filter === 'recent' ? now.toISOString() : undefined;
    const sortBy = filter === 'spend' ? 'amount' : filter === 'count' ? 'transactions' : sortByName;
    const sortDirection = filter === 'spend' || filter === 'count' ? sortDesc : sortAsc;

    dispatch(
      loadMerchants({
        search,
        isUncategorized: true,
        sortBy,
        sortDirection,
        startDate,
        endDate,
      })
    );
  }, [dispatch, filter, search]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const toggleSelect = (id: string) => {
    setSelectedIds(prev => (prev.includes(id) ? prev.filter(item => item !== id) : [...prev, id]));
  };

  const onAssign = () => {
    if (params.mode === 'edit' && params.categoryId) {
      dispatch(addCategoryMerchants({categoryId: params.categoryId, data: {merchantIds: selectedIds}})).then(result => {
        if (result.meta.requestStatus === 'fulfilled') {
          navigation.goBack();
        }
      });
      return;
    }
    dispatch(setDraftMerchantIds(selectedIds));
    navigation.goBack();
  };

  const s = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.backgroundSecondary,
    },
    content: {
      padding: spacing.lg,
      paddingBottom: spacing.xl * 2,
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
    filtersRow: {
      flexDirection: 'row',
      gap: spacing.sm,
      marginBottom: spacing.md,
    },
    filterButton: {
      paddingHorizontal: spacing.sm,
      paddingVertical: spacing.xs,
      borderRadius: radius.md,
      backgroundColor: colors.buttonSecondary,
    },
    filterButtonActive: {
      backgroundColor: colors.buttonPrimary,
    },
    filterText: {
      color: colors.buttonSecondaryText,
      fontSize: fontSizes.xs,
      fontWeight: fontWeights.medium,
    },
    filterTextActive: {
      color: colors.buttonPrimaryText,
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
    amount: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
      textAlign: 'right',
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

  const data = useMemo(() => merchants, [merchants]);

  return (
    <View style={s.container}>
      <Header title={translate('AddMerchant')} />
      <View style={s.content}>
        <TextInput
          value={search}
          onChangeText={setSearch}
          placeholder={translate('SearchMerchants')}
          placeholderTextColor={colors.textSecondary}
          style={s.searchInput}
        />
        <View style={s.filtersRow}>
          <TouchableOpacity
            style={[s.filterButton, filter === 'recent' ? s.filterButtonActive : undefined]}
            onPress={() => setFilter(filter === 'recent' ? undefined : 'recent')}>
            <Text style={[s.filterText, filter === 'recent' ? s.filterTextActive : undefined]}>{translate('Last30Days')}</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[s.filterButton, filter === 'spend' ? s.filterButtonActive : undefined]}
            onPress={() => setFilter(filter === 'spend' ? undefined : 'spend')}>
            <Text style={[s.filterText, filter === 'spend' ? s.filterTextActive : undefined]}>{translate('HighestSpending')}</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[s.filterButton, filter === 'count' ? s.filterButtonActive : undefined]}
            onPress={() => setFilter(filter === 'count' ? undefined : 'count')}>
            <Text style={[s.filterText, filter === 'count' ? s.filterTextActive : undefined]}>{translate('HighestTransactions')}</Text>
          </TouchableOpacity>
        </View>
        <FlatList
          data={data}
          keyExtractor={item => item.id}
          renderItem={({item}) => (
            <TouchableOpacity style={s.row} onPress={() => toggleSelect(item.id)}>
              <View style={s.rowLeft}>
                <View style={s.checkbox}>
                  {selectedIds.includes(item.id) ? <Icon name="check" size={fontSizes.md} color={colors.buttonPrimary} /> : undefined}
                </View>
                <View>
                  <Text style={s.name}>{item.name}</Text>
                  <Text style={s.meta}>{translate('TransactionCount')}: {item.transactionCount}</Text>
                </View>
              </View>
              <Text style={s.amount}>{item.totalAmount.toFixed(2)}</Text>
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
