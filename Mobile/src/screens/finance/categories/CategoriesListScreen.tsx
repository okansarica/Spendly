// CHANGED_BY_AI: 2026-03-02 - Add compact list layout and item menu
import React, {useMemo, useState, useEffect} from 'react';
import {View, Text, StyleSheet, TextInput, TouchableOpacity, FlatList, Modal, Alert} from 'react-native';
import {useTheme} from '../../../theme/ThemeContext';
import {translate} from '../../../utils/translations';
import Header from '../../../components/Header';
import {useAppDispatch, useAppSelector} from '../../../store/hooks';
import {loadCategories, deleteCategory} from '../../../store/categoriesStore';
import {useNavigation} from '@react-navigation/native';
import {useBottomTabBarHeight} from '@react-navigation/bottom-tabs';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {FinanceStackParamList} from '../../../navigation/FinanceNavigator';
import Icon from 'react-native-vector-icons/MaterialIcons';
import {CategoryListItem} from '../../../services/categoriesService';

const sortBy = 'name';
const sortDirection = 'asc';

type FinanceNavProp = NativeStackNavigationProp<FinanceStackParamList, 'CategoriesList'>;

type CategoryRow = {
  item: CategoryListItem;
  isChild: boolean;
};

export default function CategoriesListScreen() {
  const {colors, spacing, radius, fontSizes, fontWeights} = useTheme();
  const dispatch = useAppDispatch();
  const navigation = useNavigation<FinanceNavProp>();
  const tabBarHeight = useBottomTabBarHeight();
  const items = useAppSelector(state => state.categories.items);
  const isLoading = useAppSelector(state => state.categories.isLoading);
  const [search, setSearch] = useState('');
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [menuCategory, setMenuCategory] = useState<CategoryListItem | undefined>(undefined);
  const [menuPosition, setMenuPosition] = useState<{x: number; y: number} | undefined>(undefined);

  useEffect(() => {
    dispatch(loadCategories({search, sortBy, sortDirection}));
  }, [dispatch, search]);

  const data = useMemo(() => {
    const parents = items.filter(item => !item.parentId);
    const children = items.filter(item => item.parentId);
    const rows: CategoryRow[] = [];
    parents.forEach(parent => {
      rows.push({item: parent, isChild: false});
      children
        .filter(child => child.parentId === parent.id)
        .forEach(child => rows.push({item: child, isChild: true}));
    });
    return rows;
  }, [items]);

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
    newButton: {
      backgroundColor: colors.buttonPrimary,
      paddingHorizontal: spacing.md,
      paddingVertical: spacing.sm,
      borderRadius: radius.md,
    },
    newButtonText: {
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
    },
    cardChild: {
      marginLeft: spacing.md,
      borderStyle: 'dashed',
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
    iconWrap: {
      width: spacing.xl + spacing.xs,
      height: spacing.xl + spacing.xs,
      borderRadius: radius.md,
      alignItems: 'center',
      justifyContent: 'center',
      marginRight: spacing.md,
    },
    arrowIcon: {
      color: colors.textPrimary,
    },
    title: {
      fontSize: fontSizes.md,
      fontWeight: fontWeights.semiBold,
      color: colors.textPrimary,
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
    empty: {
      textAlign: 'center',
      color: colors.textSecondary,
      marginTop: spacing.xl,
    },
  });

  const closeMenu = () => {
    setIsMenuOpen(false);
    setMenuCategory(undefined);
    setMenuPosition(undefined);
  };

  const openMenu = (category: CategoryListItem, position: {x: number; y: number}) => {
    setMenuCategory(category);
    setMenuPosition(position);
    setIsMenuOpen(true);
  };

  const onMenuEdit = () => {
    if (!menuCategory) {
      return;
    }
    const target = menuCategory;
    closeMenu();
    navigation.navigate('CategoryEdit', {mode: 'edit', category: target});
  };

  const onMenuAddChild = () => {
    if (!menuCategory) {
      return;
    }
    const target = menuCategory;
    closeMenu();
    navigation.navigate('CategoryEdit', {mode: 'create', parentCategory: target});
  };

  const onMenuDelete = () => {
    if (!menuCategory?.id) {
      return;
    }
    const target = menuCategory;
    closeMenu();
    Alert.alert(
      translate('DeleteCategoryTitle'),
      translate('DeleteCategoryMessage'),
      [
        {text: translate('Cancel'), style: 'cancel'},
        {
          text: translate('Delete'),
          style: 'destructive',
          onPress: () => {
            dispatch(deleteCategory(target.id));
          },
        },
      ]
    );
  };

  const renderItem = ({item}: {item: CategoryRow}) => {
    const backgroundColor = item.item.color || colors.buttonSecondary;
    const iconName = item.item.icon || 'label';
    return (
      <TouchableOpacity
        style={[s.card, item.isChild ? s.cardChild : undefined]}
        onPress={() => navigation.navigate('CategoryEdit', {mode: 'edit', category: item.item})}>
        <View style={s.cardRow}>
          <View style={s.left}>
            <View style={[s.iconWrap, {backgroundColor}]}>
              <Icon name={iconName} size={fontSizes.md} color={colors.buttonPrimaryText} />
            </View>
            <View style={{flex: 1}}>
              <Text style={s.title}>{item.item.name}</Text>
              <Text style={s.subtitle}>
                {translate('MerchantCount')}: {item.item.merchantCount}
              </Text>
            </View>
          </View>
          <TouchableOpacity style={s.menuButton} onPress={event => openMenu(item.item, {x: event.nativeEvent.pageX, y: event.nativeEvent.pageY})}>
            <Icon name="more-vert" size={fontSizes.lg} style={s.menuIcon} />
          </TouchableOpacity>
        </View>
      </TouchableOpacity>
    );
  };

  return (
    <View style={s.container}>
      <Header title={translate('CategoriesTitle')} />
      <View style={s.content}>
        <View style={s.searchRow}>
          <TextInput
            value={search}
            onChangeText={setSearch}
            placeholder={translate('SearchCategories')}
            placeholderTextColor={colors.textSecondary}
            style={s.searchInput}
          />
          <TouchableOpacity
            style={s.newButton}
            onPress={() => navigation.navigate('CategoryEdit', {mode: 'create'})}>
            <Text style={s.newButtonText}>{translate('NewCategory')}</Text>
          </TouchableOpacity>
        </View>
        <FlatList
          data={data}
          renderItem={renderItem}
          keyExtractor={item => item.item.id}
          contentContainerStyle={s.listContent}
          scrollIndicatorInsets={{right: -spacing.sm, bottom: tabBarHeight + spacing.xl}}
          ListEmptyComponent={!isLoading ? <Text style={s.empty}>{translate('NoCategories')}</Text> : undefined}
        />
      </View>
      <Modal visible={isMenuOpen} transparent animationType="fade" onRequestClose={closeMenu}>
        <TouchableOpacity style={s.modalBackdrop} activeOpacity={1} onPress={closeMenu}>
          <View style={s.menuCard}>
            <TouchableOpacity style={s.menuItem} onPress={onMenuEdit}>
              <Text style={s.menuItemText}>{translate('EditCategory')}</Text>
            </TouchableOpacity>
            {!menuCategory?.parentId ? (
              <TouchableOpacity style={s.menuItem} onPress={onMenuAddChild}>
                <Text style={s.menuItemText}>{translate('AddChildCategory')}</Text>
              </TouchableOpacity>
            ) : undefined}
            <TouchableOpacity style={s.menuItem} onPress={onMenuDelete}>
              <Text style={[s.menuItemText, s.menuItemDanger]}>{translate('Delete')}</Text>
            </TouchableOpacity>
          </View>
        </TouchableOpacity>
      </Modal>
    </View>
  );
}
