import { useAuth } from '@/contexts/AuthContext';
import { getTokenStorage, setTokenStorage, type TokenStorage } from '@/lib/auth';
import { useAppTranslation } from '@/hooks/useTranslation';

export function Settings() {
  const { t } = useAppTranslation();
  const { setToken } = useAuth();
  const current = getTokenStorage();

  const handleStorageChange = (mode: TokenStorage) => {
    setTokenStorage(mode);
    if (mode === 'sessionStorage') {
      const t = (window as unknown as { __accessToken: string | null }).__accessToken;
      if (t) sessionStorage.setItem('accessToken', t);
    } else {
      sessionStorage.removeItem('accessToken');
    }
    setToken((window as unknown as { __accessToken: string | null }).__accessToken);
  };

  return (
    <div>
      <h1 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-100 mb-4">{t("pages.settings")}</h1>
      <div className="rounded-lg border border-zinc-200 dark:border-zinc-800 p-4 max-w-md">
        <h2 className="font-medium text-zinc-900 dark:text-zinc-100 mb-2">{t("pages.settings.token_storage")}</h2>
        <p className="text-sm text-zinc-600 dark:text-zinc-400 mb-4">
          {t("pages.settings.token_storage_description")}
        </p>
        <div className="flex gap-4">
          <label className="flex items-center gap-2">
            <input
              type="radio"
              name="storage"
              checked={current === 'memory'}
              onChange={() => handleStorageChange('memory')}
              className="rounded"
            />
            <span className="text-sm">{t("pages.settings.memory")}</span>
          </label>
          <label className="flex items-center gap-2">
            <input
              type="radio"
              name="storage"
              checked={current === 'sessionStorage'}
              onChange={() => handleStorageChange('sessionStorage')}
              className="rounded"
            />
            <span className="text-sm">{t("pages.settings.session_storage")}</span>
          </label>
        </div>
      </div>
    </div>
  );
}
