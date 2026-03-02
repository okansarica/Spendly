// CHANGED_BY_AI: 2026-03-02 - Add category edit screen
import React, {useCallback, useEffect, useMemo, useState} from 'react';
import {View, Text, StyleSheet, TextInput, TouchableOpacity, ScrollView, Alert} from 'react-native';
import {useTheme} from '../../../theme/ThemeContext';
import {translate} from '../../../utils/translations';
import Header from '../../../components/Header';
import {useAppDispatch, useAppSelector} from '../../../store/hooks';
import {
  addCategoryMerchants,
  clearDraftMerchantIds,
  createCategory,
  deleteCategory,
  loadCategoryMerchants,
  removeCategoryMerchant,
  setDraftMerchantIds,
  updateCategory,
} from '../../../store/categoriesStore';
import {useFocusEffect, useNavigation, useRoute} from '@react-navigation/native';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {FinanceStackParamList} from '../../../navigation/FinanceNavigator';
import {CategoryColors, CategoryIcons} from '../../../constants/categoryConstants';
import Icon from 'react-native-vector-icons/MaterialIcons';
import {formatCurrency} from '../../../utils/formatCurrency';

type FinanceNavProp = NativeStackNavigationProp<FinanceStackParamList, 'CategoryEdit'>;

export default function CategoryEditScreen() {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const navigation = useNavigation<FinanceNavProp>();
  const route = useRoute();
  const params = route.params as FinanceStackParamList['CategoryEdit'];
  const mode = params.mode;
  const category = params.category;
  const parentCategory = params.parentCategory;
  const merchantsByCategory = useAppSelector(state => state.categories.merchantsByCategory);
  const draftMerchantIds = useAppSelector(state => state.categories.draftMerchantIds);
  const merchantItems = useAppSelector(state => state.merchants.items);
  const categories = useAppSelector(state => state.categories.items);

  const usedColors = useMemo(() => {
    const list = categories.map(item => item.color).filter(color => color !== undefined) as string[];
    return new Set(list);
  }, [categories]);

  const availableColors = useMemo(() => {
    const pool = CategoryColors.filter(colorItem => !usedColors.has(colorItem));
    return pool.length > 0 ? pool : CategoryColors;
  }, [usedColors]);

  const pickRandomColor = (pool: string[]) => {
    return pool[Math.floor(Math.random() * pool.length)];
  };

  const [name, setName] = useState(category?.name ?? '');
  const [color, setColor] = useState(() => (mode === 'create' ? pickRandomColor(availableColors) : category?.color ?? CategoryColors[0]));
  const [colorTouched, setColorTouched] = useState(false);
  const [icon, setIcon] = useState(() => (mode === 'create' ? CategoryIcons[0] : category?.icon ?? CategoryIcons[0]));

  useEffect(() => {
    if (mode !== 'create' || colorTouched) {
      return;
    }
    if (!availableColors.includes(color)) {
      setColor(pickRandomColor(availableColors));
    }
  }, [mode, colorTouched, availableColors, color]);

  useFocusEffect(
    useCallback(() => {
      if (mode === 'edit' && category?.id) {
        dispatch(loadCategoryMerchants(category.id));
      }
    }, [dispatch, mode, category?.id])
  );

  const linkedMerchants = useMemo(() => {
    if (mode === 'edit' && category?.id) {
      return merchantsByCategory[category.id] ?? [];
    }
    return merchantItems.filter(item => draftMerchantIds.includes(item.id));
  }, [mode, category?.id, merchantsByCategory, merchantItems, draftMerchantIds]);

  const onSave = () => {
    if (mode === 'create') {
      dispatch(
        createCategory({
          name,
          parentId: parentCategory?.id,
          color,
          icon,
          merchantIds: draftMerchantIds,
        })
      ).then(result => {
        if (result.meta.requestStatus === 'fulfilled') {
          dispatch(clearDraftMerchantIds());
          navigation.goBack();
        }
      });
      return;
    }

    if (category?.id) {
      dispatch(updateCategory({id: category.id, data: {name, parentId: parentCategory?.id, color, icon}})).then(result => {
        if (result.meta.requestStatus === 'fulfilled') {
          navigation.goBack();
        }
      });
    }
  };

  const onAddMerchants = () => {
    if (mode === 'edit' && category?.id) {
      navigation.navigate('CategoryMerchantsSelect', {categoryId: category.id, mode});
      return;
    }
    navigation.navigate('CategoryMerchantsSelect', {mode});
  };

  const onRemoveMerchant = (merchantId: string) => {
    if (mode === 'edit' && category?.id) {
      dispatch(removeCategoryMerchant({categoryId: category.id, merchantId}));
      return;
    }
    dispatch(setDraftMerchantIds(draftMerchantIds.filter(id => id !== merchantId)));
  };

  const onDelete = () => {
    if (!category?.id) {
      return;
    }
    Alert.alert(
      translate('DeleteCategoryTitle'),
      translate('DeleteCategoryMessage'),
      [
        {text: translate('Cancel'), style: 'cancel'},
        {
          text: translate('Delete'),
          style: 'destructive',
          onPress: () => {
            dispatch(deleteCategory(category.id)).then(result => {
              if (result.meta.requestStatus === 'fulfilled') {
                navigation.goBack();
              }
            });
          },
        },
      ]
    );
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
    row: {
      flexDirection: 'row',
      flexWrap: 'wrap',
      gap: spacing.sm,
      marginBottom: spacing.lg,
    },
    colorSwatch: {
      width: spacing.xl,
      height: spacing.xl,
      borderRadius: radius.lg,
      borderWidth: 2,
      borderColor: colors.borderSubtle,
    },
    iconButton: {
      padding: spacing.sm,
      borderRadius: radius.md,
      backgroundColor: colors.cardBackground,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
    iconSelected: {
      borderColor: colors.buttonPrimary,
    },
    actionRow: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'center',
      marginBottom: spacing.md,
    },
    actionButton: {
      backgroundColor: colors.buttonSecondary,
      paddingHorizontal: spacing.md,
      paddingVertical: spacing.sm,
      borderRadius: radius.md,
    },
    actionButtonText: {
      color: colors.buttonSecondaryText,
      fontWeight: fontWeights.semiBold,
    },
    merchantCard: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      padding: spacing.md,
      marginBottom: spacing.sm,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
    merchantRow: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'center',
    },
    merchantName: {
      fontSize: fontSizes.md,
      fontWeight: fontWeights.semiBold,
      color: colors.textPrimary,
    },
    merchantMeta: {
      fontSize: fontSizes.xs,
      color: colors.textSecondary,
      marginTop: spacing.xs,
    },
    removeButton: {
      paddingHorizontal: spacing.sm,
      paddingVertical: spacing.xs,
      borderRadius: radius.sm,
      backgroundColor: colors.danger,
    },
    removeButtonText: {
      color: colors.buttonPrimaryText,
      fontSize: fontSizes.xs,
    },
    saveButton: {
      backgroundColor: colors.buttonPrimary,
      paddingVertical: spacing.md,
      borderRadius: radius.md,
      alignItems: 'center',
      marginTop: spacing.lg,
    },
    saveButtonText: {
      color: colors.buttonPrimaryText,
      fontWeight: fontWeights.semiBold,
    },
    deleteButton: {
      backgroundColor: colors.danger,
      paddingVertical: spacing.md,
      borderRadius: radius.md,
      alignItems: 'center',
      marginTop: spacing.md,
    },
    deleteButtonText: {
      color: colors.buttonPrimaryText,
      fontWeight: fontWeights.semiBold,
    },
    parentBadge: {
      marginBottom: spacing.md,
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      padding: spacing.sm,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
    },
    parentText: {
      color: colors.textSecondary,
      fontSize: fontSizes.sm,
    },
  });

  const title = mode === 'edit' ? translate('EditCategory') : translate('CreateCategory');

  return (
    <View style={s.container}>
      <Header title={title} />
      <ScrollView contentContainerStyle={s.content}>
        {parentCategory ? (
          <View style={s.parentBadge}>
            <Text style={s.parentText}>{parentCategory.name}</Text>
          </View>
        ) : undefined}
        <Text style={s.label}>{translate('CategoryName')}</Text>
        <TextInput value={name} onChangeText={setName} style={s.input} />
        <Text style={s.label}>{translate('Color')}</Text>
        <View style={s.row}>
          {CategoryColors.map(swatch => (
            <TouchableOpacity
              key={swatch}
              style={[s.colorSwatch, {backgroundColor: swatch, borderColor: color === swatch ? colors.buttonPrimary : colors.borderSubtle}]}
              onPress={() => {
                setColorTouched(true);
                setColor(swatch);
              }}
            />
          ))}
        </View>
        <Text style={s.label}>{translate('Icon')}</Text>
        <View style={s.row}>
          {CategoryIcons.map(iconName => (
            <TouchableOpacity
              key={iconName}
              style={[s.iconButton, icon === iconName ? s.iconSelected : undefined]}
              onPress={() => setIcon(iconName)}>
              <Icon name={iconName} size={fontSizes.lg} color={colors.textPrimary} />
            </TouchableOpacity>
          ))}
        </View>
        <View style={s.actionRow}>
          <Text style={s.label}>{translate('MerchantsTitle')}</Text>
          <TouchableOpacity style={s.actionButton} onPress={onAddMerchants}>
            <Text style={s.actionButtonText}>{translate('AddMerchant')}</Text>
          </TouchableOpacity>
        </View>
        {linkedMerchants.map(merchant => (
          <View key={merchant.id} style={s.merchantCard}>
            <View style={s.merchantRow}>
              <View>
                <Text style={s.merchantName}>{merchant.name}</Text>
                {'transactionCount' in merchant ? (
                  <Text style={s.merchantMeta}>
                    {translate('TransactionCount')}: {merchant.transactionCount}
                  </Text>
                ) : undefined}
                {'lastTransactionDate' in merchant && merchant.lastTransactionDate ? (
                  <Text style={s.merchantMeta}>
                    {translate('LastTransaction')}: {merchant.lastTransactionDate}
                  </Text>
                ) : undefined}
                {'totalAmount' in merchant ? (
                  <Text style={s.merchantMeta}>{formatCurrency(merchant.totalAmount as number)}</Text>
                ) : undefined}
              </View>
              <TouchableOpacity style={s.removeButton} onPress={() => onRemoveMerchant(merchant.id)}>
                <Text style={s.removeButtonText}>×</Text>
              </TouchableOpacity>
            </View>
          </View>
        ))}
        <TouchableOpacity style={s.saveButton} onPress={onSave}>
          <Text style={s.saveButtonText}>{translate('Save')}</Text>
        </TouchableOpacity>
        {mode === 'edit' ? (
          <TouchableOpacity style={s.deleteButton} onPress={onDelete}>
            <Text style={s.deleteButtonText}>{translate('Delete')}</Text>
          </TouchableOpacity>
        ) : undefined}
      </ScrollView>
    </View>
  );
}
