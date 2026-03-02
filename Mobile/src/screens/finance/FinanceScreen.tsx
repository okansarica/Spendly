// CHANGED_BY_AI: 2026-03-02 - Add shared header usage
import React from 'react';
import {View, Text, StyleSheet} from 'react-native';
import {useTheme} from '../../theme/ThemeContext';
import Header from '../../components/Header';
import {translate} from '../../utils/translations';

export default function FinanceScreen() {
  const {colors, fontSizes, fontWeights} = useTheme();
  return (
    <View style={[styles.container, {backgroundColor: colors.backgroundSecondary}]}>
      <Header title={translate('FinanceTitle')} showBack={false} />
      <Text style={[styles.text, {color: colors.textPrimary, fontSize: fontSizes.xl, fontWeight: fontWeights.semiBold}]}>
        Finance
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {flex: 1, alignItems: 'center', justifyContent: 'center'},
  text: {},
});
