import { setAccessToken } from './api';

const STORAGE_KEY = 'platform_token_storage';

export type TokenStorage = 'memory' | 'sessionStorage';

export function getTokenStorage(): TokenStorage {
  try {
    return (localStorage.getItem(STORAGE_KEY) as TokenStorage) || 'memory';
  } catch {
    return 'memory';
  }
}

export function setTokenStorage(mode: TokenStorage): void {
  try {
    localStorage.setItem(STORAGE_KEY, mode);
  } catch {
    // ignore
  }
}

export function persistToken(token: string | null): void {
  setAccessToken(token);
  if (getTokenStorage() === 'sessionStorage' && typeof sessionStorage !== 'undefined') {
    if (token) sessionStorage.setItem('accessToken', token);
    else sessionStorage.removeItem('accessToken');
  }
}

export function loadStoredToken(): string | null {
  if (getTokenStorage() === 'sessionStorage' && typeof sessionStorage !== 'undefined') {
    return sessionStorage.getItem('accessToken');
  }
  return null;
}
