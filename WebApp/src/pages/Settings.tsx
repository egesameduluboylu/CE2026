import { useAuth } from '@/contexts/AuthContext';
import { getTokenStorage, setTokenStorage, type TokenStorage } from '@/lib/auth';

export function Settings() {
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
      <h1 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-100 mb-4">Settings</h1>
      <div className="rounded-lg border border-zinc-200 dark:border-zinc-800 p-4 max-w-md">
        <h2 className="font-medium text-zinc-900 dark:text-zinc-100 mb-2">Token storage</h2>
        <p className="text-sm text-zinc-600 dark:text-zinc-400 mb-4">
          Where to keep the access token. Memory is default; sessionStorage survives refresh but not new tab.
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
            <span className="text-sm">Memory</span>
          </label>
          <label className="flex items-center gap-2">
            <input
              type="radio"
              name="storage"
              checked={current === 'sessionStorage'}
              onChange={() => handleStorageChange('sessionStorage')}
              className="rounded"
            />
            <span className="text-sm">sessionStorage</span>
          </label>
        </div>
      </div>
    </div>
  );
}
