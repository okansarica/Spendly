// CHANGED_BY_AI: 2026-03-02 - Add report endpoints

export const ApiEndpoints = {
  Auth: {
    Login: '/api/v1/auth/login',
    SocialLogin: '/api/v1/auth/social-login',
    ForgotPassword: '/api/v1/auth/forgot-password',
    Register: '/api/v1/auth/register',
    VerifyEmail: '/api/v1/auth/verify-email',
    ResendCode: '/api/v1/auth/resend-verification',
    RefreshToken: '/api/v1/auth/refresh-access-token',
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
};
