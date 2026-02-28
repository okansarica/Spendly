import React from 'react';
import {View, Text, StyleSheet} from 'react-native';

export default function RegisterScreen() {
  return (
    <View style={styles.container}>
      <Text style={styles.text}>Register Screen</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {flex: 1, padding: 24, justifyContent: 'center', backgroundColor: '#fff'},
  text: {fontSize: 16, textAlign: 'center', color: '#333'},
});

