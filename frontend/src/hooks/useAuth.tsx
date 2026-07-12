import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { useQueryClient } from '@tanstack/react-query';

export interface AuthUser {
  sub: string;
  email: string;
}

type AuthContextValue = {
  isLoggedIn: boolean;
  user: AuthUser | null;
  login: (accessToken: string, idToken: string) => void;
  logout: () => void;
};

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const payload = token.split('.')[1];
    if (!payload) return null;
    return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
  } catch {
    return null;
  }
}

export function isTokenExpired(token: string): boolean {
  const payload = decodeJwtPayload(token);
  const exp = payload?.exp;
  if (typeof exp !== 'number') return true;
  return Date.now() >= exp * 1000;
}

function getTokenExpiresAt(token: string): number | null {
  const payload = decodeJwtPayload(token);
  const exp = payload?.exp;
  return typeof exp === 'number' ? exp * 1000 : null;
}

function parseIdToken(idToken: string): AuthUser | null {
  const decoded = decodeJwtPayload(idToken);
  if (!decoded) return null;
  return {
    sub: String(decoded.sub ?? ''),
    email: String(decoded.email ?? ''),
  };
}

function clearStoredAuth() {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('idToken');
}

function getInitialAuthState(): { isLoggedIn: boolean; user: AuthUser | null } {
  const accessToken = localStorage.getItem('accessToken');
  const idToken = localStorage.getItem('idToken');

  if (!accessToken || isTokenExpired(accessToken)) {
    if (accessToken || idToken) clearStoredAuth();
    return { isLoggedIn: false, user: null };
  }

  return {
    isLoggedIn: true,
    user: idToken && !isTokenExpired(idToken) ? parseIdToken(idToken) : null,
  };
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const [authState, setAuthState] = useState(getInitialAuthState);
  const { isLoggedIn, user } = authState;

  const login = useCallback((accessToken: string, idToken: string) => {
    queryClient.removeQueries();
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('idToken', idToken);
    setAuthState({
      isLoggedIn: true,
      user: parseIdToken(idToken),
    });
  }, [queryClient]);

  const logout = useCallback(() => {
    clearStoredAuth();
    queryClient.removeQueries();
    setAuthState({ isLoggedIn: false, user: null });
  }, [queryClient]);

  // Log out when the access token expires, and re-check when the tab regains focus.
  useEffect(() => {
    if (!isLoggedIn) return;

    const accessToken = localStorage.getItem('accessToken');
    if (!accessToken || isTokenExpired(accessToken)) {
      logout();
      return;
    }

    const expiresAt = getTokenExpiresAt(accessToken);
    if (!expiresAt) {
      logout();
      return;
    }

    const timeout = window.setTimeout(logout, Math.max(0, expiresAt - Date.now()));

    const handleVisibilityChange = () => {
      if (document.visibilityState !== 'visible') return;
      const token = localStorage.getItem('accessToken');
      if (!token || isTokenExpired(token)) logout();
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      window.clearTimeout(timeout);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [isLoggedIn, logout]);

  // If the API rejects the token (401), clear the session.
  useEffect(() => {
    const handleUnauthorized = () => logout();
    window.addEventListener('auth:unauthorized', handleUnauthorized);
    return () => window.removeEventListener('auth:unauthorized', handleUnauthorized);
  }, [logout]);

  const value = useMemo(
    () => ({ isLoggedIn, user, login, logout }),
    [isLoggedIn, user, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
