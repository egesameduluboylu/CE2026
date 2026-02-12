import type { I18nBundle } from './types';

const missingKeys = new Set<string>();

export function interpolate(template: string, params: Record<string, string | number> = {}): string {
  return template.replace(/\{(\w+)\}/g, (match, key) => {
    const value = params[key];
    return value !== undefined ? String(value) : match;
  });
}

export function translate(
  dictionary: I18nBundle,
  key: string,
  params: Record<string, string | number> = {}
): string {
  const value = dictionary[key];
  
  if (value === undefined) {
    // Log missing key only once
    if (!missingKeys.has(key)) {
      missingKeys.add(key);
      console.warn(`[I18n] Missing translation key: ${key}`);
    }
    return `[missing] ${key}`;
  }

  return interpolate(value, params);
}

export function detectLanguage(): string {
  // Priority order:
  // 1. User profile (would need API call, skip for now)
  // 2. localStorage
  // 3. navigator.language
  // 4. default "en"

  // Check localStorage first
  const stored = localStorage.getItem('i18n.lang');
  if (stored) {
    return stored;
  }

  // Check navigator.language
  if (typeof navigator !== 'undefined') {
    const navLang = navigator.language;
    
    // Map common variants
    if (navLang.startsWith('tr-')) {
      return 'tr-TR';
    }
    if (navLang.startsWith('en-')) {
      return 'en-US';
    }
    
    // Return the language part if it's a known variant
    const lang = navLang.split('-')[0];
    if (['tr', 'en'].includes(lang)) {
      return lang === 'tr' ? 'tr-TR' : 'en-US';
    }
  }

  // Default fallback
  return 'en-US';
}
