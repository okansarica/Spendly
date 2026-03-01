const translations: Record<string, Record<string, string>> = {
  en: {
    PleaseCheckYourEMailAndPassword: 'Please check your email and password',
    InvalidCredentials: 'Invalid credentials',
    EmailNotVerified: 'Email not verified',
    AccountLocked: 'Account locked',
    InvalidToken: 'Invalid token',
    'an error occurred': 'An error occurred',
    'An unexpected error occurred': 'An unexpected error occurred',
  },
};

let currentLanguage = 'en';

export const setLanguage = (lang: string) => {
  currentLanguage = lang;
};

export const translate = (key: string): string => {
  return translations[currentLanguage]?.[key] || translations.en?.[key] || key;
};

export const getCurrentLanguage = () => currentLanguage;

