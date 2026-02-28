import React, {useState} from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
} from 'react-native';
import {useNavigation} from '@react-navigation/native';
import {useAuthStore} from '../../store/authStore';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {AuthStackParamList} from '../../navigation/AuthNavigator';

type ForgotPasswordNavProp = NativeStackNavigationProp<AuthStackParamList, 'ForgotPassword'>;

export default function ForgotPasswordScreen() {
  const navigation = useNavigation<ForgotPasswordNavProp>();
  const {forgotPassword, isLoading, error, clearError} = useAuthStore();
  const [email, setEmail] = useState('');
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = async () => {
    await forgotPassword(email);
    setSubmitted(true);
  };

  return (
    <View style={styles.container}>
      {error ? (
        <Text style={styles.error} onPress={clearError}>
          {error}
        </Text>
      ) : null}

      {submitted ? (
        <Text style={styles.success}>If the email exists, a reset link has been sent.</Text>
      ) : null}

      <TextInput
        style={styles.input}
        placeholder="Email"
        autoCapitalize="none"
        keyboardType="email-address"
        value={email}
        onChangeText={setEmail}
      />

      <TouchableOpacity style={styles.button} onPress={handleSubmit} disabled={isLoading}>
        {isLoading ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={styles.buttonText}>Send Reset Link</Text>
        )}
      </TouchableOpacity>

      <TouchableOpacity onPress={() => navigation.navigate('Login')}>
        <Text style={styles.link}>Login</Text>
      </TouchableOpacity>

      <TouchableOpacity onPress={() => navigation.navigate('Register')}>
        <Text style={styles.link}>Register</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {flex: 1, padding: 24, justifyContent: 'center', backgroundColor: '#fff'},
  input: {borderWidth: 1, borderColor: '#ccc', borderRadius: 8, padding: 12, marginBottom: 12, fontSize: 16},
  button: {backgroundColor: '#4A90E2', borderRadius: 8, padding: 14, alignItems: 'center', marginBottom: 12},
  buttonText: {color: '#fff', fontSize: 16, fontWeight: '600'},
  link: {color: '#4A90E2', textAlign: 'center', marginTop: 8, fontSize: 14},
  error: {color: '#D32F2F', backgroundColor: '#FFEBEE', borderRadius: 6, padding: 10, marginBottom: 12, textAlign: 'center'},
  success: {color: '#388E3C', backgroundColor: '#E8F5E9', borderRadius: 6, padding: 10, marginBottom: 12, textAlign: 'center'},
});

