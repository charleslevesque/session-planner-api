import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type PropsWithChildren,
} from 'react';
import { apiGet, apiPost, apiRequest, ApiError } from '../lib/api';
import type {
  AuthResponse,
  LoginRequest,
  MeResponse,
  RefreshTokenRequest,
  RegisterRequest,
} from '../types/auth';

interface AuthContextValue {
  user: MeResponse | null;
  token: string | null;
  refreshToken: string | null;
  expiresAt: string | null;
  isAuthenticated: boolean;
  isBusy: boolean;
  login: (payload: LoginRequest) => Promise<void>;
  register: (payload: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refresh: () => Promise<boolean>;
  refreshCurrentUser: () => Promise<boolean>;
  apiFetch: <T>(path: string, init?: RequestInit) => Promise<T>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthResponse | null>(null);
  const [user, setUser] = useState<MeResponse | null>(null);
  const [isBusy, setIsBusy] = useState(false);
  const sessionRef = useRef<AuthResponse | null>(null);

  useEffect(() => {
    sessionRef.current = session;
  }, [session]);

  const clearSession = useCallback(() => {
    setSession(null);
    setUser(null);
    sessionRef.current = null;
    setIsBusy(false);
  }, []);

  const loadCurrentUser = useCallback(async (accessToken: string) => {
    const profile = await apiGet<MeResponse>('/auth/me', accessToken);
    setUser(profile);
    return profile;
  }, []);

  const login = useCallback(
    async (payload: LoginRequest) => {
      setIsBusy(true);

      try {
        const nextSession = await apiPost<LoginRequest, AuthResponse>('/auth/login', payload);
        setSession(nextSession);
        sessionRef.current = nextSession;
        await loadCurrentUser(nextSession.token);
      } catch (error) {
        clearSession();
        throw error;
      } finally {
        setIsBusy(false);
      }
    },
    [clearSession, loadCurrentUser],
  );

  const register = useCallback(async (payload: RegisterRequest) => {
    await apiPost<RegisterRequest, AuthResponse>('/auth/register', payload);
    clearSession();
  }, [clearSession]);

  const refresh = useCallback(async () => {
    const currentSession = sessionRef.current;

    if (!currentSession?.refreshToken) {
      clearSession();
      return false;
    }

    setIsBusy(true);

    try {
      const nextSession = await apiPost<RefreshTokenRequest, AuthResponse>('/auth/refresh', {
        refreshToken: currentSession.refreshToken,
      });

      setSession(nextSession);
      sessionRef.current = nextSession;
      await loadCurrentUser(nextSession.token);
      return true;
    } catch {
      clearSession();
      return false;
    } finally {
      setIsBusy(false);
    }
  }, [clearSession, loadCurrentUser]);

  const refreshCurrentUser = useCallback(async () => {
    const currentSession = sessionRef.current;

    if (!currentSession?.token) {
      clearSession();
      return false;
    }

    try {
      await loadCurrentUser(currentSession.token);
      return true;
    } catch {
      clearSession();
      return false;
    }
  }, [clearSession, loadCurrentUser]);

  const logout = useCallback(async () => {
    const currentSession = sessionRef.current;

    try {
      if (currentSession?.token && currentSession.refreshToken) {
        await apiPost<RefreshTokenRequest, void>(
          '/auth/logout',
          { refreshToken: currentSession.refreshToken },
          currentSession.token,
        );
      }
    } finally {
      clearSession();
    }
  }, [clearSession]);

  const apiFetch = useCallback(
    async <T,>(path: string, init: RequestInit = {}) => {
      const currentSession = sessionRef.current;

      if (!currentSession?.token) {
        throw new ApiError('Session expirée. Veuillez vous reconnecter.', 401);
      }

      try {
        return await apiRequest<T>(path, init, currentSession.token);
      } catch (error) {
        if (!(error instanceof ApiError) || error.status !== 401) {
          throw error;
        }

        const didRefresh = await refresh();

        if (!didRefresh || !sessionRef.current?.token) {
          throw error;
        }

        return apiRequest<T>(path, init, sessionRef.current.token);
      }
    },
    [refresh],
  );

  useEffect(() => {
    if (!session?.expiresAt) {
      return undefined;
    }

    const expiresAtTime = Date.parse(session.expiresAt);

    if (Number.isNaN(expiresAtTime)) {
      return undefined;
    }

    const refreshDelay = Math.max(expiresAtTime - Date.now() - 60_000, 0);
    const timeout = window.setTimeout(() => {
      void refresh();
    }, refreshDelay);

    return () => window.clearTimeout(timeout);
  }, [refresh, session?.expiresAt]);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token: session?.token ?? null,
      refreshToken: session?.refreshToken ?? null,
      expiresAt: session?.expiresAt ?? null,
      isAuthenticated: Boolean(session?.token && user),
      isBusy,
      login,
      register,
      logout,
      refresh,
      refreshCurrentUser,
      apiFetch,
    }),
    [
      apiFetch,
      isBusy,
      login,
      logout,
      refresh,
      refreshCurrentUser,
      register,
      session?.expiresAt,
      session?.refreshToken,
      session?.token,
      user,
    ],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }

  return context;
}
