// CHANGED_BY_AI: 2026-03-11 - Add mobile app version check service
import apiClient, {ApiResponse, apiCall} from './apiClient';

export type VersionCheckResponse = {
  isThereNewVersion: boolean;
  forceUpdate: boolean;
  appleStoreUrl?: string;
  playStoreUrl?: string;
  requiredVersion?: string;
  localizedMessages?: {
    languageCode?: string;
    message?: string;
  }[];
};

async function checkVersion(version: string): Promise<ApiResponse<VersionCheckResponse>> {
  return apiCall(() => apiClient.post('/api/v1/version/check', {version}));
}

export const versionService = {
  checkVersion,
};
