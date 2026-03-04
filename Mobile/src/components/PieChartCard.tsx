import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { PieChart } from 'react-native-chart-kit';
import { useTheme } from '../theme/ThemeContext';
import { formatCurrency } from '../utils/formatCurrency';

interface PieChartData {
    label: string;
    amount: number;
    percentage: number;
}

interface PieChartCardProps {
    data: PieChartData[];
    chartWidth: number;
    chartHeight: number;
    title?: string;
}

const formatPercentage = (percentage: number): string => {
    return percentage.toFixed(1);
};

export default function PieChartCard({
                                         data,
                                         chartWidth,
                                         chartHeight,
                                         title,
                                     }: PieChartCardProps) {
    const {colors, spacing, radius, fontSizes} = useTheme();

    const preparePieData = (dataArray: PieChartData[]) => {
        return dataArray.map((item, index) => ({
            //name: item.label,
            name: '',
            population: item.amount,
            color: colors.chartPalette[index % colors.chartPalette.length],
            //legendFontColor: colors.textPrimary,
            //legendFontSize: fontSizes.sm,
        }));
    };

    const renderLegend = (items: PieChartData[]) => (
        <View>
            {items.map((item, index) => (
                <View key={`${item.label}-${index}`} style={s.legendItem}>
                    <View style={s.legendLeft}>
                        <View
                            style={[
                                s.legendDot,
                                {
                                    backgroundColor:
                                        colors.chartPalette[index % colors.chartPalette.length],
                                },
                            ]}
                        />
                        <Text style={s.legendLabel}>{item.label}</Text>
                    </View>
                    <Text style={s.legendValue}>
                        {formatPercentage(item.percentage)}% • {formatCurrency(item.amount)}
                    </Text>
                </View>
            ))}
        </View>
    );

    const chartConfig = {
        backgroundColor: colors.cardBackground,
        backgroundGradientFrom: colors.cardBackground,
        backgroundGradientTo: colors.cardBackground,
        decimalPlaces: 0,
        color: (_opacity = 1) => `rgba(37, 99, 235, ${_opacity})`,
        labelColor: (_opacity = 1) => colors.textPrimary,
        style: {
            borderRadius: radius.lg,
        },
        propsForLabels: {
            fontSize: fontSizes.xs,
        },
    };

    const s = StyleSheet.create({
        titleContainer:{},
        title: {
            fontSize: fontSizes.sm,
            color: colors.textSecondary,            
        },
        chartContainer: {
            alignItems: 'center',
            //marginVertical: spacing.md,
            justifyContent: 'center',
        },
        legendItem: {
            flexDirection: 'row',
            justifyContent: 'space-between',
            alignItems: 'center',
            paddingVertical: spacing.xs,
        },
        legendLeft: {
            flexDirection: 'row',
            alignItems: 'center',
            gap: spacing.xs,
        },
        legendDot: {
            width: 8,
            height: 8,
            borderRadius: 4,
        },
        legendLabel: {
            fontSize: fontSizes.sm,
            color: colors.textPrimary,
        },
        legendValue: {
            fontSize: fontSizes.sm,
            color: colors.textSecondary,
        },
    });

    return (
        <View>
            {
                title && <View style={s.titleContainer}>
                    <Text style={s.title}>{title}</Text>
                </View>
            }
            <View style={s.chartContainer}>
                <PieChart
                    data={preparePieData(data)}
                    width={200} // should be hard coded
                    height={chartHeight}
                    chartConfig={chartConfig}
                    accessor="population"
                    backgroundColor="transparent"
                    paddingLeft="50"
                    absolute
                    hasLegend={false}
                />
            </View>
            {renderLegend(data)}
        </View>
    );
}

