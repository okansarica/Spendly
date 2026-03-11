import {Platform} from 'react-native';

export const APP_CONFIG = {
  version: '1.0.0',
  apiBaseUrl: Platform.OS === 'ios' ? 'http://localhost:5001' : 'http://10.0.2.2:5001',
};

export default APP_CONFIG;

