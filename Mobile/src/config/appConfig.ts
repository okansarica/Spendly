// CHANGED_BY_AI: 2026-03-11 - Resolve API host dynamically for physical devices
import {NativeModules, Platform} from 'react-native';

const DEFAULT_API_PORT = '5001';

const getHostFromBundleUrl = () => {
  const scriptURL = NativeModules?.SourceCode?.scriptURL as string | undefined;
  if (!scriptURL) {
    return undefined;
  }

  const match = scriptURL.match(/https?:\/\/([^/:]+)(?::\d+)?/);
  return match?.[1];
};

const getApiBaseUrl = () => {
  const hostFromBundle = getHostFromBundleUrl();

  if (Platform.OS === 'android') {
    const host = hostFromBundle === 'localhost' ? '10.0.2.2' : hostFromBundle;
    return `http://${host ?? '10.0.2.2'}:${DEFAULT_API_PORT}`;
  }

  return `http://${hostFromBundle ?? 'localhost'}:${DEFAULT_API_PORT}`;
};

export const APP_CONFIG = {
  version: '1.0.0',
  apiBaseUrl: getApiBaseUrl(),
};

export default APP_CONFIG;

