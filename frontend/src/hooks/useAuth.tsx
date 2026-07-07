import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react';

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

function parseIdToken(idToken: string): AuthUser | null {
  try {
    const payload = idToken.split('.')[1];
    const decoded = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
    return {
      sub: decoded.sub ?? '',
      email: decoded.email ?? '',
    };
  } catch {
    return null;
  }
}

function getUserFromStorage(): AuthUser | null {
  const idToken = localStorage.getItem('idToken');
  return idToken ? parseIdToken(idToken) : null;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoggedIn, setIsLoggedIn] = useState(
    () => !!localStorage.getItem('accessToken'),
  );
  const [user, setUser] = useState<AuthUser | null>(() => getUserFromStorage());

  const login = useCallback((accessToken: string, idToken: string) => {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('idToken', idToken);
    setIsLoggedIn(true);
    setUser(parseIdToken(idToken));
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('idToken');
    setIsLoggedIn(false);
    setUser(null);
  }, []);

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
