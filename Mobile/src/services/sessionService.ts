import 'react-native-get-random-values';
import AsyncStorage from '@react-native-async-storage/async-storage';
import {v4 as uuidv4} from 'uuid';

class SessionService {
  private static SESSION_ID_KEY = 'X-Session-Id';

  async getSessionId(): Promise<string> {
    let sessionId = await AsyncStorage.getItem(SessionService.SESSION_ID_KEY);
    
    if (!sessionId) {
      sessionId = uuidv4();
      await AsyncStorage.setItem(SessionService.SESSION_ID_KEY, sessionId);
    }
    
    return sessionId;
  }

  async clearSessionId(): Promise<void> {
    await AsyncStorage.removeItem(SessionService.SESSION_ID_KEY);
  }
}

export default new SessionService();

