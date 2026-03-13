// CHANGED_BY_AI: 2026-03-13 - Enable merchant report navigation
// CHANGED_BY_AI: 2026-03-02 - Add reports menu screen
// CHANGED_BY_AI: 2026-03-13 - Redesign with modern card layout and add merchant report
// CHANGED_BY_AI: 2026-03-13 - Refactor to use MenuCard component
import React from 'react';
import { View, Text, StyleSheet, ScrollView } from 'react-native';
import { useTheme } from '../../theme/ThemeContext';
import { translate } from '../../utils/translations';
import { useNavigation } from '@react-navigation/native';
import { useBottomTabBarHeight } from '@react-navigation/bottom-tabs';
import Header from '../../components/Header';
import MenuCard from '../../components/MenuCard';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import type { ReportsStackParamList } from '../../navigation/ReportsNavigator';

type ReportsNavProp = NativeStackNavigationProp<ReportsStackParamList, 'ReportsMenu'>;

type ReportItem = {
    id: 'category' | 'account' | 'merchant';
    titleKey: string;
    descriptionKey: string;
    icon: string;
    route: 'ReportsOverview' | 'AccountsOverview' | 'MerchantsOverview';
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
            route: 'MerchantsOverview',
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
        cardWrapper: {
            position: 'relative',
        },
    });

    const renderCard = (item: ReportItem) => {
        return (
            <View key={item.id} style={s.cardWrapper}>
                <MenuCard
                    title={translate(item.titleKey)}
                    description={translate(item.descriptionKey)}
                    icon={item.icon}
                    onPress={() => navigation.navigate(item.route)}
                    showArrow={true}
                />
            </View>
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
