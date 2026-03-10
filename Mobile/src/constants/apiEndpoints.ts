// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname endpoint
// CHANGED_BY_AI: 2026-03-02 - Add report endpoints
// CHANGED_BY_AI: 2026-03-02 - Remove unused finance endpoints
// CHANGED_BY_AI: 2026-03-03 - Add user profile and logout endpoints

export const ApiEndpoints = {
  Auth: {
    Login: '/api/v1/auth/login',
    SocialLogin: '/api/v1/auth/social-login',
    ForgotPassword: '/api/v1/auth/forgot-password',
    Register: '/api/v1/auth/register',
    VerifyEmail: '/api/v1/auth/verify-email',
    ResendCode: '/api/v1/auth/resend-verification',
    RefreshToken: '/api/v1/auth/refresh-access-token',
    Logout: '/api/v1/auth/logout',
  },
  Homepage: {
    Get: '/api/v1/homepage',
  },
  Reports: {
    Overview: '/api/v1/reports/overview',
    CategoryDetail: (categoryId: string) => `/api/v1/reports/category/${categoryId}`,
    AccountsOverview: '/api/v1/reports/accounts/overview',
    AccountDetail: (accountId: string) => `/api/v1/reports/accounts/${accountId}`,
  },
  Categories: {
    Base: '/api/v1/user-categories',
    ById: (id: string) => `/api/v1/user-categories/${id}`,
    Merchants: (id: string) => `/api/v1/user-categories/${id}/merchants`,
    MerchantLink: (id: string, merchantId: string) => `/api/v1/user-categories/${id}/merchants/${merchantId}`,
  },
  Banks: {
    Base: '/api/v1/banks',
    BankDefinitions: '/api/v1/banks/bank-definitions',
    ById: (id: string) => `/api/v1/banks/${id}`,
    Accounts: (bankId: string) => `/api/v1/banks/${bankId}/accounts`,
    AccountById: (bankId: string, id: string) => `/api/v1/banks/${bankId}/accounts/${id}`,
  },
  UserMerchants: {
    Base: '/api/v1/user-merchants',
    ById: (id: string) => `/api/v1/user-merchants/${id}`,
    Category: (id: string) => `/api/v1/user-merchants/${id}/category`,
  },
  Users: {
    Profile: '/api/v1/users/profile',
    ChangePassword: '/api/v1/users/change-password',
    Language: '/api/v1/users/language',
    DeleteAccount: '/api/v1/users/account',
    SubscriptionEnd: '/api/v1/users/subscription-end',
    SubscriptionPlans: '/api/v1/users/subscription-plans',
    CreatePaymentUrl: '/api/v1/users/create-payment-url',
    FirebaseToken: '/api/v1/users/firebase-token',
  },
};
