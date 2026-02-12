export interface I18nBundle {
  [key: string]: string;
}

export interface I18nState {
  lang: string;
  dictionary: I18nBundle;
  loading: boolean;
  error: string | null;
}

export interface I18nContextType extends I18nState {
  setLanguage: (lang: string) => void;
  t: (key: string, params?: Record<string, string | number>) => string;
  refreshBundle: () => Promise<void>;
}

export interface CachedBundle {
  etag: string;
  savedAt: number;
  data: I18nBundle;
}

export interface I18nFetcherOptions {
  tenantId?: string;
  defaultLang?: string;
}
