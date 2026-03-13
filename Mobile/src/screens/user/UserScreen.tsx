// CHANGED_BY_AI: 2026-03-03 - Implement user menu screen integration
// CHANGED_BY_AI: 2026-03-13 - Redesign with modern card layout and improved logout placement
// CHANGED_BY_AI: 2026-03-13 - Refactor to use MenuCard component
import React, {useEffect, useState} from 'react';
import {View, Text, TouchableOpacity, StyleSheet, Alert, ScrollView} from 'react-native';
import Toast from 'react-native-toast-message';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {logout} from '../../store/authStore';
import {loadUserProfile, setUserLanguage} from '../../store/userStore';
import {useTheme} from '../../theme/ThemeContext';
import {useBottomTabBarHeight} from '@react-navigation/bottom-tabs';
import Header from '../../components/Header';
import MenuCard from '../../components/MenuCard';
import {translate, setLanguage} from '../../utils/translations';
import {useNavigation} from '@react-navigation/native';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {UserStackParamList} from '../../navigation/UserNavigator';
import {Dropdown} from 'react-native-element-dropdown';
import Icon from 'react-native-vector-icons/MaterialIcons';
import APP_CONFIG from '../../config/appConfig';

type UserNavProp = NativeStackNavigationProp<UserStackParamList, 'UserMenu'>;

const LANGUAGE_OPTIONS = [
  {value: "en", label: "English"},
  {value: "fr", label: "Français"},
  {value: "de", label: "Deutsch"},
  {value: "es", label: "Español"},
  {value: "it", label: "Italiano"},
  {value: "pt", label: "Português"},
  {value: "sv", label: "Svenska"},
  {value: "fi", label: "Suomi"},
  {value: "da", label: "Dansk"},
  {value: "nl", label: "Nederlands"},
  {value: "no", label: "Norsk"},
  {value: "pl", label: "Polski"}
];

export default function UserScreen() {
  const dispatch = useAppDispatch();
  const navigation = useNavigation<UserNavProp>();  
  const profile = useAppSelector(s => s.user.profile);
  const currentLanguage = useAppSelector(s => s.user.languageCode);
  const {colors, fontSizes, fontWeights, spacing, radius} = useTheme();
  const tabBarHeight = useBottomTabBarHeight();
  const [selectedLanguage, setSelectedLanguage] = useState(currentLanguage || 'en');

  useEffect(() => {
    dispatch(loadUserProfile());
  }, [dispatch]);

  useEffect(() => {
    if (currentLanguage) {
      setSelectedLanguage(currentLanguage);
    }
  }, [currentLanguage]);

  useEffect(() => {
    if (profile?.languageCode) {
      setSelectedLanguage(profile.languageCode);
    }
  }, [profile?.languageCode]);

  const s = StyleSheet.create({
    container: {flex: 1, backgroundColor: colors.backgroundSecondary},
    scrollContent: {
      padding: spacing.lg,
      paddingBottom: tabBarHeight + spacing.xl,
    },
    languageCard: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.lg,
      marginBottom: spacing.lg,
      overflow: 'hidden',
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      borderLeftWidth: 6,
      borderLeftColor: colors.buttonPrimary,
      shadowColor: colors.cardShadow,
      shadowOffset: {width: 0, height: 2},
      shadowOpacity: 0.08,
      shadowRadius: 8,
      elevation: 3,
      padding: spacing.lg,
    },
    languageHeader: {
      flexDirection: 'row',
      alignItems: 'center',
      marginBottom: spacing.md,
    },
    iconContainer: {
      width: 56,
      height: 56,
      borderRadius: 28,
      backgroundColor: colors.buttonPrimary,
      alignItems: 'center',
      justifyContent: 'center',
      marginRight: spacing.md,
    },
    textContent: {
      flex: 1,
    },
    languageTitle: {
      color: colors.textPrimary,
      fontSize: fontSizes.xl,
      fontWeight: fontWeights.bold,
      marginBottom: spacing.xs,
    },
    languageDescription: {
      color: colors.textSecondary,
      fontSize: fontSizes.sm,
      lineHeight: 20,
      marginBottom: spacing.md,
    },
    pickerContainer: {
      backgroundColor: colors.backgroundPrimary,
      borderRadius: radius.md,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      overflow: 'hidden',
    },
    dropdown: {
      height: 44,
      paddingHorizontal: spacing.md,
    },
    dropdownSelectedText: {
      color: colors.textPrimary,
      fontSize: fontSizes.md,
    },
    dropdownItemText: {
      color: colors.textPrimary,
      fontSize: fontSizes.md,
    },
    dropdownContainer: {
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      borderColor: colors.borderSubtle,
    },
    versionText: {
      color: colors.textSecondary,
      fontSize: fontSizes.sm,
      textAlign: 'left',
      opacity: 0.4,
      marginTop: spacing.md,
    },
  });

  const onLanguageSelect = async (languageCode: string) => {
    setSelectedLanguage(languageCode);
    setLanguage(languageCode);
    const result = await dispatch(setUserLanguage(languageCode));
    if (result.meta.requestStatus !== 'fulfilled') {
      Toast.show({type: 'error', text1: translate('Error'), text2: result.payload as string});
    }
  };

  const confirmLogout = () => {
    Alert.alert(
      translate('LogoutTitle'),
      translate('LogoutConfirmMessage') || translate('AreYouSure') || 'Are you sure you want to logout?',
      [
        { text: translate('Cancel') || 'Cancel', style: 'cancel' },
        { text: translate('LogoutTitle'), style: 'destructive', onPress: () => dispatch(logout()) }
      ],
      { cancelable: true }
    );
  };

  return (
    <View style={s.container}>
      <Header title={translate('UserTitle')} showBack={false} />
      <ScrollView
        style={{flex: 1}}
        contentContainerStyle={s.scrollContent}
        showsVerticalScrollIndicator={false}>
        
        {/* Language Selection Card */}
        <View style={s.languageCard}>
          <View style={s.languageHeader}>
            <View style={s.iconContainer}>
              <Icon name="language" size={28} color={colors.buttonPrimaryText} />
            </View>
            <View style={s.textContent}>
              <Text style={s.languageTitle}>{translate('LanguageTitle')}</Text>
            </View>
          </View>
          <Text style={s.languageDescription}>{translate('LanguageDescription')}</Text>
          <View style={s.pickerContainer}>
            <Dropdown
              data={LANGUAGE_OPTIONS}
              labelField="label"
              valueField="value"
              value={selectedLanguage}
              style={s.dropdown}
              selectedTextStyle={s.dropdownSelectedText}
              itemTextStyle={s.dropdownItemText}
              containerStyle={s.dropdownContainer}
              onChange={item => onLanguageSelect(item.value)}
            />
          </View>
        </View>

        {/* Profile Card */}
        <MenuCard
          title={translate('ProfileTitle')}
          description={translate('ProfileDescription')}
          icon="person"
          onPress={() => navigation.navigate('Profile')}
        />

        {/* Change Password Card */}
        <MenuCard
          title={translate('ChangePasswordTitle')}
          description={translate('ChangePasswordDescription')}
          icon="lock"
          onPress={() => navigation.navigate('ChangePassword')}
        />

        {/* Logout Card */}
        <MenuCard
          title={translate('LogoutTitle')}
          description={translate('LogoutDescription')}
          icon="logout"
          onPress={confirmLogout}
          leftBorderColor={colors.danger}
          iconBackgroundColor={colors.danger}
          showArrow={false}
          minHeight={100}
        />

        <Text style={s.versionText}>v{APP_CONFIG.version}</Text>
      </ScrollView>
    </View>
  );
}
