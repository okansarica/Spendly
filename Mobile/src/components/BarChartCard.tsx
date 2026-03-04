import React from 'react';
import {View, Text, StyleSheet} from 'react-native';
import {BarChart} from 'react-native-chart-kit';
import {useTheme} from '../theme/ThemeContext';
import {formatCurrency} from '../utils/formatCurrency';

interface Dataset {
  data: number[];
  color?: () => string;
}

interface BarChartCardProps {
  data?: number[];
  datasets?: Dataset[];
  labels: string[];
  chartWidth: number;
  chartHeight: number;
  showBarTops?: boolean;
  showValuesOnTopOfBars?: boolean;
}

export default function BarChartCard({
  data,
  datasets,
  labels,
  chartWidth,
  chartHeight,
  showBarTops = false,
  showValuesOnTopOfBars = false,
}: BarChartCardProps) {
  const {colors, radius, fontSizes, spacing} = useTheme();

  const truncateLabel = (label: string): string => {
    if (label.length <= 5) return label;
    return label.substring(0, 5);
  };

  const formatYLabel = (value: string): string => {
    const numValue = parseFloat(value);
    if (numValue >= 1000) {
      return `${(numValue / 1000).toFixed(0)}k`;
    }
    return numValue.toFixed(0);
  };

  const chartConfig = {
    backgroundColor: colors.cardBackground,
    backgroundGradientFrom: colors.cardBackground,
    backgroundGradientTo: colors.cardBackground,
    decimalPlaces: 0,
    color: (_opacity = 1) => colors.buttonPrimary,
    labelColor: (_opacity = 1) => colors.textPrimary,
    style: {
      borderRadius: radius.lg,
    },
    propsForLabels: {
      fontSize: fontSizes.xs,
      fontWeight: 'bold',
    },
    barPercentage: 0.7,
    fillShadowGradient: colors.buttonPrimary,
    fillShadowGradientOpacity: 1,
    formatYLabel: formatYLabel,
  };

  const truncatedLabels = labels.map(truncateLabel);

  const chartData = {
    labels: truncatedLabels,
    datasets: datasets || [{data: data || []}],
  };

  const primaryData = data || (datasets && datasets[0]?.data) || [];

  const s = StyleSheet.create({
    container: {
      width: '100%',
    },
    summaryContainer: {
      marginTop: spacing.md,
    },
    summaryItem: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'center',
      paddingVertical: spacing.xs,
    },
    summaryLabel: {
      fontSize: fontSizes.sm,
      color: colors.textPrimary,
    },
    summaryValue: {
      fontSize: fontSizes.sm,
      color: colors.textSecondary,
      fontWeight: '500',
    },
  });

  return (
    <View style={s.container}>
      <BarChart
        data={chartData}
        width={chartWidth}
        height={chartHeight}
        yAxisLabel=""
        yAxisSuffix=""
        chartConfig={chartConfig}
        fromZero
        showBarTops={showBarTops}
        showValuesOnTopOfBars={showValuesOnTopOfBars}
        withHorizontalLabels={true}
        withVerticalLabels={true}
        withInnerLines={true}
        segments={4}
        yLabelsOffset={-5}
        style={{
          borderRadius: radius.lg,
          paddingRight: spacing.md,
        }}
      />
      <View style={s.summaryContainer}>
        {labels.map((label, index) => (
          <View key={`${label}-${index}`} style={s.summaryItem}>
            <Text style={s.summaryLabel}>{label}</Text>
            <Text style={s.summaryValue}>{formatCurrency(primaryData[index] || 0)}</Text>
          </View>
        ))}
      </View>
    </View>
  );
}



