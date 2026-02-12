import type { I18nBundle, CachedBundle } from './types';

const CACHE_PREFIX = 'i18n.bundle:';
const CACHE_DURATION = 3600000; // 1 hour

export class I18nStorage {
  static getCacheKey(tenantId: string | undefined, lang: string): string {
    return `${CACHE_PREFIX}${tenantId || 'null'}:${lang}`;
  }

  static getCachedBundle(tenantId: string | undefined, lang: string): CachedBundle | null {
    try {
      const key = this.getCacheKey(tenantId, lang);
      const cached = localStorage.getItem(key);
      
      if (!cached) return null;

      const bundle: CachedBundle = JSON.parse(cached);
      
      // Check if cache is expired
      if (Date.now() - bundle.savedAt > CACHE_DURATION) {
        localStorage.removeItem(key);
        return null;
      }

      return bundle;
    } catch (error) {
      console.warn('Failed to parse cached bundle:', error);
      return null;
    }
  }

  static setCachedBundle(tenantId: string | undefined, lang: string, etag: string, data: I18nBundle): void {
    try {
      const key = this.getCacheKey(tenantId, lang);
      const bundle: CachedBundle = {
        etag,
        savedAt: Date.now(),
        data
      };
      
      localStorage.setItem(key, JSON.stringify(bundle));
    } catch (error) {
      console.warn('Failed to cache bundle:', error);
    }
  }

  static removeCachedBundle(tenantId: string | undefined, lang: string): void {
    try {
      const key = this.getCacheKey(tenantId, lang);
      localStorage.removeItem(key);
    } catch (error) {
      console.warn('Failed to remove cached bundle:', error);
    }
  }

  static getStoredLanguage(): string | null {
    try {
      return localStorage.getItem('i18n.lang');
    } catch {
      return null;
    }
  }

  static setStoredLanguage(lang: string): void {
    try {
      localStorage.setItem('i18n.lang', lang);
    } catch (error) {
      console.warn('Failed to store language:', error);
    }
  }
}
