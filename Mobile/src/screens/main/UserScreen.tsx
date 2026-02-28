import React from 'react';
import {View, Text, StyleSheet} from 'react-native';
import {useTheme} from '../../theme/ThemeContext';

export default function UserScreen() {
  const {colors, fontSizes, fontWeights} = useTheme();
  return (
    <View style={[styles.container, {backgroundColor: colors.backgroundSecondary}]}>
      <Text style={[styles.text, {color: colors.textPrimary, fontSize: fontSizes.xl, fontWeight: fontWeights.semiBold}]}>
        User
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {flex: 1, alignItems: 'center', justifyContent: 'center'},
  text: {},
});

