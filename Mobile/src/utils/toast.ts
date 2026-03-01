import Toast from 'react-native-toast-message';

export const showErrorToast = (message: string, title = 'Error') => {
  Toast.show({
    type: 'error',
    text1: title,
    text2: message,
  });
};

export const showSuccessToast = (message: string, title = 'Success') => {
  Toast.show({
    type: 'success',
    text1: title,
    text2: message,
  });
};

export const showInfoToast = (message: string, title = 'Info') => {
  Toast.show({
    type: 'info',
    text1: title,
    text2: message,
  });
};

