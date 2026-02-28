import React from 'react';
import {View, Text, StyleSheet} from 'react-native';

export default function VerificationScreen() {
  return (
    <View style={styles.container}>
      <Text style={styles.text}>Please verify your email to continue.</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {flex: 1, padding: 24, justifyContent: 'center', backgroundColor: '#fff'},
  text: {fontSize: 16, textAlign: 'center', color: '#333'},
});

