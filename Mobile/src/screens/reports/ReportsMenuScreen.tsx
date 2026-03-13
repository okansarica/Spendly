// CHANGED_BY_AI: 2026-03-02 - Add reports menu screen
// CHANGED_BY_AI: 2026-03-13 - Redesign with modern card layout and add merchant report
import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, ScrollView } from 'react-native';
import { useTheme } from '../../theme/ThemeContext';
import { translate } from '../../utils/translations';
import { useNavigation } from '@react-navigation/native';
import { useBottomTabBarHeight } from '@react-navigation/bottom-tabs';
import Header from '../../components/Header';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import type { ReportsStackParamList } from '../../navigation/ReportsNavigator';
import Icon from 'react-native-vector-icons/MaterialIcons';

type ReportsNavProp = NativeStackNavigationProp<ReportsStackParamList, 'ReportsMenu'>;

type ReportItem = {
    id: 'category' | 'account' | 'merchant';
    titleKey: string;
    descriptionKey: string;
    icon: string;
    route: 'ReportsOverview' | 'AccountsOverview' | 'ReportsMenu';
};

export default function ReportsMenuScreen() {
    const {colors, spacing, fontSizes, fontWeights, radius} = useTheme();
    const navigation = useNavigation<ReportsNavProp>();
    const tabBarHeight = useBottomTabBarHeight();

    const items: ReportItem[] = [
        {
            id: 'category',
            titleKey: 'CategoryReport',
            descriptionKey: 'CategoryReportDescription',
            icon: 'donut-large',
            route: 'ReportsOverview',
        },
        {
            id: 'account',
            titleKey: 'AccountReport',
            descriptionKey: 'AccountReportDescription',
            icon: 'account-balance-wallet',
            route: 'AccountsOverview',
        },
        {
            id: 'merchant',
            titleKey: 'MerchantReport',
            descriptionKey: 'MerchantReportDescription',
            icon: 'store',
            route: 'ReportsMenu',
        },
    ];

    const s = StyleSheet.create({
        container: {
            flex: 1,
            backgroundColor: colors.backgroundSecondary,
        },
        scrollContent: {
            padding: spacing.lg,
            paddingBottom: tabBarHeight + spacing.xl,
        },
        card: {
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
        },
        cardContent: {
            padding: spacing.lg,
            minHeight: 140,
        },
        cardHeader: {
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
        title: {
            color: colors.textPrimary,
            fontSize: fontSizes.xl,
            fontWeight: fontWeights.bold,
            marginBottom: spacing.xs,
        },
        description: {
            color: colors.textSecondary,
            fontSize: fontSizes.sm,
            lineHeight: 20,
        },
        arrowContainer: {
            width: 32,
            height: 32,
            borderRadius: 16,
            backgroundColor: colors.buttonPrimary + '15',
            alignItems: 'center',
            justifyContent: 'center',
        },
        comingSoonBadge: {
            position: 'absolute',
            top: spacing.md,
            right: spacing.md,
            backgroundColor: colors.buttonSecondary,
            paddingHorizontal: spacing.sm,
            paddingVertical: spacing.xs,
            borderRadius: radius.sm,
            borderWidth: 1,
            borderColor: colors.borderSubtle,
        },
        comingSoonText: {
            color: colors.textSecondary,
            fontSize: fontSizes.xs,
            fontWeight: fontWeights.semiBold,
            textTransform: 'uppercase',
        },
    });

    const renderCard = (item: ReportItem) => {
        const isComingSoon = item.id === 'merchant';

        return (
            <TouchableOpacity
                key={item.id}
                style={s.card}
                onPress={() => !isComingSoon && navigation.navigate(item.route)}
                activeOpacity={isComingSoon ? 1 : 0.7}
                disabled={isComingSoon}>
                <View style={s.cardContent}>
                    {isComingSoon && (
                        <View style={s.comingSoonBadge}>
                            <Text style={s.comingSoonText}>{translate('FeatureComingSoon')}</Text>
                        </View>
                    )}
                    <View style={s.cardHeader}>
                        <View style={s.iconContainer}>
                            <Icon name={item.icon} size={28} color={colors.buttonPrimaryText} />
                        </View>
                        <View style={s.textContent}>
                            <Text style={s.title}>{translate(item.titleKey)}</Text>
                        </View>
                        {!isComingSoon && (
                            <View style={s.arrowContainer}>
                                <Icon name="arrow-forward" size={20} color={colors.buttonPrimary} />
                            </View>
                        )}
                    </View>
                    <Text style={s.description}>{translate(item.descriptionKey)}</Text>
                </View>
            </TouchableOpacity>
        );
    };

    return (
        <View style={s.container}>
            <Header title={translate('ReportsTitle')} showBack={false}/>
            <ScrollView 
                style={{flex: 1}}
                contentContainerStyle={s.scrollContent}
                showsVerticalScrollIndicator={false}>
                {items.map(item => renderCard(item))}
            </ScrollView>
        </View>
    );
}
