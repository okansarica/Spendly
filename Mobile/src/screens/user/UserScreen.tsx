// CHANGED_BY_AI: 2026-03-03 - Implement user menu screen integration
import React, {useEffect, useState} from 'react';
import {View, Text, TouchableOpacity, StyleSheet, Alert} from 'react-native';
import Toast from 'react-native-toast-message';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {logout} from '../../store/authStore';
import {loadUserProfile, setUserLanguage} from '../../store/userStore';
import {useTheme} from '../../theme/ThemeContext';
import Header from '../../components/Header';
import {translate, setLanguage} from '../../utils/translations';
import {useNavigation} from '@react-navigation/native';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {UserStackParamList} from '../../navigation/UserNavigator';
import {Dropdown} from 'react-native-element-dropdown';

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
    content: {padding: spacing.lg, gap: spacing.md},
    email: {color: colors.textPrimary, fontSize: fontSizes.md},
    sectionTitle: {color: colors.textSecondary, fontSize: fontSizes.sm, fontWeight: fontWeights.medium},
    pickerContainer: {
      backgroundColor: colors.cardBackground,
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
    navBtn: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      backgroundColor: colors.cardBackground,
      borderRadius: radius.md,
      borderWidth: 1,
      borderColor: colors.borderSubtle,
      paddingHorizontal: spacing.lg,
      paddingVertical: spacing.md,
    },
    navText: {
      flex: 1,
      color: colors.textPrimary,
      fontSize: fontSizes.md,
      fontWeight: fontWeights.medium,
    },
    navArrow: {
      color: colors.textSecondary,
      fontSize: fontSizes.lg,
    },
    logoutBtn: {
      backgroundColor: colors.buttonPrimary,
      borderRadius: radius.md,
      height: 44,
      justifyContent: 'center',
      paddingHorizontal: spacing.md,
    },
    logoutText: {
      color: colors.buttonPrimaryText,
      fontSize: fontSizes.md,
      fontWeight: fontWeights.medium,
      textAlign: 'center',
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
      <View style={s.content}>        
        <Text style={s.sectionTitle}>{translate('LanguageTitle')}</Text>

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

        <TouchableOpacity style={s.navBtn} onPress={() => navigation.navigate('Profile')}>
          <Text style={s.navText}>{translate('ProfileTitle')}</Text>
          <Text style={s.navArrow}>›</Text>
        </TouchableOpacity>

        <TouchableOpacity style={s.navBtn} onPress={() => navigation.navigate('ChangePassword')}>
          <Text style={s.navText}>{translate('ChangePasswordTitle')}</Text>
          <Text style={s.navArrow}>›</Text>
        </TouchableOpacity>

        <TouchableOpacity style={s.logoutBtn} onPress={confirmLogout}>
          <Text style={s.logoutText}>{translate('LogoutTitle')}</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}
