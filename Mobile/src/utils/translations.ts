// CHANGED_BY_AI: 2026-03-02 - Add homepage translations
const translations: Record<string, Record<string, string>> = {
  en: {
    PleaseCheckYourEMailAndPassword: 'Please check your email and password',
    InvalidCredentials: 'Invalid credentials',
    EmailNotVerified: 'Email not verified',
    AccountLocked: 'Account locked',
    InvalidToken: 'Invalid token',
    EmailRequired: 'Email is required',
    EmailInvalid: 'Email is invalid',
    PasswordRequired: 'Password is required',
    EmailAlreadyExists: 'Email already exists',
    NameRequired: 'Name is required',
    SurnameRequired: 'Surname is required',
    'an error occurred': 'An error occurred',
    'An unexpected error occurred': 'An unexpected error occurred',
    DashboardTitle: 'Dashboard',
    DashboardSubtitle: 'Your spending overview',
    CurrentMonthTotal: 'Current Month Total',
    PreviousMonth: 'Previous Month',
    ThisWeek: 'This Week',
    DailyAverage: 'Daily Average',
    Insights: 'Insights',
    TopCategory: 'Top Category',
    HighestExpense: 'Highest Expense',
    MostUsedAccount: 'Most Used Account',
    SpendingByAccount: 'Spending By Account',
    SpendingByCategory: 'Spending By Category',
    SixMonthTrend: '6-Month Spending Trend',
    LatestExpenses: 'Latest 10 Expenses',
    NoDataAvailable: 'No data available',
    NoDataForPeriod: 'No data for this period',
    NoRecentExpenses: 'No recent expenses',
    NoTrendData: 'No trend data',
    NoCategoryData: 'No category data',
    NoAccountData: 'No account data',
    EmptyStateMessage: 'No expenses yet',
    VsLastWeek: 'vs last week',
    VsLastMonth: 'vs last month',
    OfTotal: 'of total',
    OfTransactions: 'of transactions',
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
