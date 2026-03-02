// CHANGED_BY_AI: 2026-03-02 - Add shared header usage
import React, {useState, useEffect} from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
} from 'react-native';
import {useNavigation} from '@react-navigation/native';
import Toast from 'react-native-toast-message';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {forgotPassword, clearError} from '../../store/authStore';
import Header from '../../components/Header';
import {translate} from '../../utils/translations';
import type {NativeStackNavigationProp} from '@react-navigation/native-stack';
import type {AuthStackParamList} from '../../navigation/AuthNavigator';

type ForgotPasswordNavProp = NativeStackNavigationProp<AuthStackParamList, 'ForgotPassword'>;

export default function ForgotPasswordScreen() {
  const navigation = useNavigation<ForgotPasswordNavProp>();
  const dispatch = useAppDispatch();
  const isLoading = useAppSelector(s => s.auth.isLoading);
  const error = useAppSelector(s => s.auth.error);
  const [email, setEmail] = useState('');
  const [submitted, setSubmitted] = useState(false);

  useEffect(() => {
    if (error) {
      Toast.show({
        type: 'error',
        text1: 'Error',
        text2: error,
      });
      dispatch(clearError());
    }
  }, [error, dispatch]);

  const handleSubmit = async () => {
    const result = await dispatch(forgotPassword(email));
    if (forgotPassword.fulfilled.match(result)) {
      setSubmitted(true);
      Toast.show({
        type: 'success',
        text1: 'Success',
        text2: 'If the email exists, a reset link has been sent.',
      });
    }
  };

  return (
    <View style={styles.container}>
      <Header title={translate('ForgotPasswordTitle')} />

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
});
