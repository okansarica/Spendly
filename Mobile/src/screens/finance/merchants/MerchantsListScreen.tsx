// CHANGED_BY_AI: 2026-03-02 - Add merchants list screen
import React, {useState, useEffect} from 'react';
import {View, Text, StyleSheet, TextInput, TouchableOpacity, FlatList} from 'react-native';
import {useTheme} from '../../../theme/ThemeContext';
import {translate} from '../../../utils/translations';
import Header from '../../../components/Header';
import {useAppDispatch, useAppSelector} from '../../../store/hooks';
import {loadMerchants} from '../../../store/merchantsStore';
import Icon from 'react-native-vector-icons/MaterialIcons';

const sortBy = 'name';
const sortDirection = 'asc';

export default function MerchantsListScreen() {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const items = useAppSelector(state => state.merchants.items);
  const isLoading = useAppSelector(state => state.merchants.isLoading);
  const [search, setSearch] = useState('');

  useEffect(() => {
    dispatch(loadMerchants({search, sortBy, sortDirection}));
  }, [dispatch, search]);

  const s = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.backgroundSecondary,
    },
    content: {
      padding: spacing.lg,
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
    card: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      padding: spacing.lg,
      marginBottom: spacing.md,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.1,
      shadowRadius: 6,
      elevation: 3,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
    row: {
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
    subtitle: {
      fontSize: fontSizes.xs,
      color: colors.textSecondary,
      marginTop: spacing.xs,
    },
    badge: {
      paddingHorizontal: spacing.sm,
      paddingVertical: spacing.xs,
      borderRadius: radius.sm,
      backgroundColor: colors.buttonSecondary,
      marginTop: spacing.xs,
      alignSelf: 'flex-start',
    },
    badgeText: {
      fontSize: fontSizes.xs,
      color: colors.buttonSecondaryText,
    },
    arrow: {
      color: colors.textSecondary,
    },
    empty: {
      textAlign: 'center',
      color: colors.textSecondary,
      marginTop: spacing.xl,
    },
  });

  return (
    <View style={s.container}>
      <Header title={translate('MerchantsTitle')} />
      <View style={s.content}>
        <TextInput
          value={search}
          onChangeText={setSearch}
          placeholder={translate('SearchMerchants')}
          placeholderTextColor={colors.textSecondary}
          style={s.searchInput}
        />
        <FlatList
          data={items}
          keyExtractor={item => item.id}
          ListEmptyComponent={!isLoading ? <Text style={s.empty}>{translate('NoMerchants')}</Text> : undefined}
          renderItem={({item}) => (
            <TouchableOpacity style={s.card}>
              <View style={s.row}>
                <View style={s.left}>
                  <Icon name="store" size={fontSizes.lg} color={colors.textSecondary} style={{marginRight: spacing.md}} />
                  <View>
                    <Text style={s.title}>{item.name}</Text>
                    <Text style={s.subtitle}>{translate('TransactionCount')}: {item.transactionCount}</Text>
                    <View style={s.badge}>
                      <Text style={s.badgeText}>{item.categoryName || translate('Uncategorized')}</Text>
                    </View>
                  </View>
                </View>
                <Text style={s.arrow}>›</Text>
              </View>
            </TouchableOpacity>
          )}
        />
      </View>
    </View>
  );
}
