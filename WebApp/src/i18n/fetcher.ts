import type { I18nBundle, I18nFetcherOptions } from './types';
import { I18nStorage } from './storage';

export class I18nFetcher {
  private baseUrl: string;
  private tenantId?: string;
  private abortController?: AbortController;

  constructor(options: I18nFetcherOptions = {}) {
    this.baseUrl = 'http://localhost:5001/api/i18n';
    this.tenantId = options.tenantId;
  }

  async fetchBundle(lang: string, signal?: AbortSignal): Promise<{ bundle: I18nBundle; etag: string }> {
    const url = new URL(`${this.baseUrl}/bundle`, window.location.origin);
    url.searchParams.set('lang', lang);
    if (this.tenantId) {
      url.searchParams.set('tenantId', this.tenantId);
    }

    // Try to get cached bundle first
    const cached = I18nStorage.getCachedBundle(this.tenantId, lang);
    const headers: Record<string, string> = {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
    };

    if (cached) {
      headers['If-None-Match'] = cached.etag;
    }

    try {
      const response = await fetch(url.toString(), {
        method: 'GET',
        headers,
        signal,
      });

      if (response.status === 304) {
        // Not modified - use cached bundle
        if (!cached) {
          throw new Error('Received 304 but no cached bundle available');
        }
        return { bundle: cached.data, etag: cached.etag };
      }

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const etag = response.headers.get('ETag') || '';
      const data = await response.json();

      if (!data.resources || typeof data.resources !== 'object') {
        throw new Error('Invalid bundle format received');
      }

      const bundle = data.resources as I18nBundle;
      
      // Cache the new bundle
      I18nStorage.setCachedBundle(this.tenantId, lang, etag, bundle);

      return { bundle, etag };
    } catch (error) {
      if (error instanceof Error && error.name === 'AbortError') {
        throw error;
      }

      // On network error, try to return cached bundle if available
      if (cached) {
        console.warn('Network error, using cached bundle:', error);
        return { bundle: cached.data, etag: cached.etag };
      }

      throw error;
    }
  }

  async refreshBundle(lang: string): Promise<{ bundle: I18nBundle; etag: string }> {
    // Clear cache and fetch fresh
    I18nStorage.removeCachedBundle(this.tenantId, lang);
    return this.fetchBundle(lang);
  }

  cancelPendingRequest(): void {
    if (this.abortController) {
      this.abortController.abort();
      this.abortController = undefined;
    }
  }

  createAbortController(): AbortSignal {
    this.cancelPendingRequest();
    this.abortController = new AbortController();
    return this.abortController.signal;
  }

  setTenantId(tenantId?: string): void {
    this.tenantId = tenantId;
  }
}
