import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import type { I18nContextType, I18nState } from './types';
import { I18nFetcher } from './fetcher';
import { I18nStorage } from './storage';
import { translate, detectLanguage } from './utils';

const I18nContext = createContext<I18nContextType | undefined>(undefined);

interface I18nProviderProps {
  children: React.ReactNode;
  tenantId?: string;
  defaultLang?: string;
}

export function I18nProvider({ children, tenantId, defaultLang }: I18nProviderProps) {
  const [state, setState] = useState<I18nState>(() => {
    const initialLang = defaultLang || detectLanguage();
    
    // Try to load cached bundle immediately for instant UI
    const cached = I18nStorage.getCachedBundle(tenantId, initialLang);
    
    return {
      lang: initialLang,
      dictionary: cached?.data || {},
      loading: !cached, // Only loading if no cached data
      error: null,
    };
  });

  const [fetcher] = useState(() => new I18nFetcher({ tenantId }));

  const setLanguage = useCallback(async (newLang: string) => {
    if (newLang === state.lang) return;

    setState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const signal = fetcher.createAbortController();
      const { bundle } = await fetcher.fetchBundle(newLang, signal);
      
      I18nStorage.setStoredLanguage(newLang);
      
      setState({
        lang: newLang,
        dictionary: bundle,
        loading: false,
        error: null,
      });
    } catch (error) {
      console.error('Failed to load language bundle:', error);
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Failed to load language',
      }));
    }
  }, [state.lang, fetcher]);

  const refreshBundle = useCallback(async () => {
    if (state.loading) return;

    setState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const { bundle } = await fetcher.refreshBundle(state.lang);
      
      setState(prev => ({
        ...prev,
        dictionary: bundle,
        loading: false,
        error: null,
      }));
    } catch (error) {
      console.error('Failed to refresh language bundle:', error);
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Failed to refresh language',
      }));
    }
  }, [state.lang, state.loading, fetcher]);

  const t = useCallback((key: string, params?: Record<string, string | number>) => {
    return translate(state.dictionary, key, params);
  }, [state.dictionary]);

  // Initial load effect
  useEffect(() => {
    const loadInitialBundle = async () => {
      // If we already have cached data, still revalidate in background
      const hasCachedData = Object.keys(state.dictionary).length > 0;
      
      if (!hasCachedData) {
        setState(prev => ({ ...prev, loading: true, error: null }));
      }

      try {
        const signal = fetcher.createAbortController();
        const { bundle } = await fetcher.fetchBundle(state.lang, signal);
        
        setState(prev => ({
          ...prev,
          dictionary: bundle,
          loading: false,
          error: null,
        }));
      } catch (error) {
        console.error('Failed to load initial language bundle:', error);
        setState(prev => ({
          ...prev,
          loading: false,
          error: error instanceof Error ? error.message : 'Failed to load language',
        }));
      }
    };

    loadInitialBundle();

    // Cleanup
    return () => {
      fetcher.cancelPendingRequest();
    };
  }, []); // Only run once on mount

  // Update fetcher tenantId if it changes
  useEffect(() => {
    fetcher.setTenantId(tenantId);
  }, [tenantId, fetcher]);

  const contextValue: I18nContextType = {
    ...state,
    setLanguage,
    t,
    refreshBundle,
  };

  return (
    <I18nContext.Provider value={contextValue}>
      {children}
    </I18nContext.Provider>
  );
}

export function useI18n(): I18nContextType {
  const context = useContext(I18nContext);
  
  if (context === undefined) {
    throw new Error('useI18n must be used within an I18nProvider');
  }
  
  return context;
}
