// CHANGED_BY_AI: 2026-03-03 - Add user profile API service
// CHANGED_BY_AI: 2026-03-05 - Add Firebase token endpoint
import apiClient from './apiClient';
import {ApiEndpoints} from '../constants/apiEndpoints';

export type UserProfile = {
  id: string;
  name: string;
  surname: string;
  email: string;
  isNewsletterSubscribed: boolean;
  languageCode: string;
};

export type UpdateUserProfileRequest = {
  name: string;
  surname: string;
  isNewsletterSubscribed: boolean;
};

export type ChangePasswordRequest = {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
};

export type SetLanguagePreferenceRequest = {
  languageCode: string;
};

export type LanguagePreferenceResponse = {
  languageCode: string;
};

export const userService = {
  getProfile: () => apiClient.get<UserProfile>(ApiEndpoints.Users.Profile),

  updateProfile: (payload: UpdateUserProfileRequest) =>
    apiClient.put<UserProfile>(ApiEndpoints.Users.Profile, payload),

  changePassword: (payload: ChangePasswordRequest) =>
    apiClient.put(ApiEndpoints.Users.ChangePassword, payload),

  setLanguagePreference: (payload: SetLanguagePreferenceRequest) =>
    apiClient.put<LanguagePreferenceResponse>(ApiEndpoints.Users.Language, payload),

  deleteAccount: () => apiClient.delete(ApiEndpoints.Users.DeleteAccount),

  sendFirebaseToken: (token: string) =>
    apiClient.post(ApiEndpoints.Users.FirebaseToken, {token}),
};
