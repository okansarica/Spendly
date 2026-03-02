// CHANGED_BY_AI: 2026-03-02 - Add shared header usage
import React from 'react';
import {View, Text, StyleSheet, TouchableOpacity} from 'react-native';
import {useTheme} from '../../theme/ThemeContext';
import Header from '../../components/Header';
import {translate} from '../../utils/translations';

export default function ReportsScreen() {
  const {colors, spacing, fontSizes, fontWeights, radius} = useTheme();

  const reportOptions = [
    {id: 'monthly', title: 'Monthly Summary Report', icon: '📊'},
    {id: 'category', title: 'Category Distribution Report', icon: '📈'},
  ];

  const handleReportPress = (reportId: string) => {
    console.log('Report selected:', reportId);
  };

  return (
    <View style={[styles.container, {backgroundColor: colors.backgroundSecondary}]}> 
      <Header title={translate('ReportsTitle')} showBack={false} />
      {reportOptions.map((report, index) => (
        <React.Fragment key={report.id}>
          <TouchableOpacity
            style={[
              styles.reportItem,
              {
                backgroundColor: colors.cardBackground,
                paddingHorizontal: spacing.lg,
                paddingVertical: spacing.md,
                borderRadius: radius.md,
                borderWidth: 1,
                borderColor: colors.borderSubtle,
                marginHorizontal: spacing.lg,
                marginTop: spacing.lg,
              },
            ]}
            onPress={() => handleReportPress(report.id)}>
            <View style={styles.reportContent}>
              <Text style={{fontSize: fontSizes.xl, marginRight: spacing.md}}>{report.icon}</Text>
              <Text style={[styles.reportTitle, {color: colors.textPrimary, fontSize: fontSizes.md, fontWeight: fontWeights.medium}]}>
                {report.title}
              </Text>
            </View>
            <Text style={[styles.arrow, {color: colors.textSecondary, fontSize: fontSizes.lg}]}>›</Text>
          </TouchableOpacity>
          {index < reportOptions.length - 1 && <View style={[styles.divider, {height: spacing.lg}]} />}
        </React.Fragment>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  reportItem: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  reportContent: {
    flexDirection: 'row',
    alignItems: 'center',
    flex: 1,
  },
  reportTitle: {
    flex: 1,
  },
  arrow: {},
  divider: {
    height: 1,
  },
});
