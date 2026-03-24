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
  isInitializing: boolean;
  isBusy: boolean;
  login: (payload: LoginRequest) => Promise<void>;
  register: (payload: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refresh: () => Promise<boolean>;
  refreshCurrentUser: () => Promise<boolean>;
  apiFetch: <T>(path: string, init?: RequestInit) => Promise<T>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const AUTH_TOKEN_KEY = 'auth_token';
const AUTH_USER_KEY = 'auth_user';
const AUTH_REFRESH_TOKEN_KEY = 'auth_refresh_token';
const AUTH_EXPIRES_AT_KEY = 'auth_expires_at';

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

function isStoredUser(value: unknown): value is MeResponse {
  if (!isRecord(value)) {
    return false;
  }

  return (
    typeof value.id === 'number' &&
    Number.isFinite(value.id) &&
    typeof value.email === 'string' &&
    typeof value.name === 'string' &&
    typeof value.role === 'string'
  );
}

function safeReadStorage(key: string): string | null {
  try {
    return window.sessionStorage.getItem(key);
  } catch {
    return null;
  }
}

function safeWriteStorage(key: string, value: string) {
  try {
    window.sessionStorage.setItem(key, value);
  } catch {
    // Ignore storage errors (quota/private mode) to keep auth functional in-memory.
  }
}

function safeRemoveStorage(key: string) {
  try {
    window.sessionStorage.removeItem(key);
  } catch {
    // Ignore storage errors.
  }
}

function readStoredAuth(): { session: AuthResponse; user: MeResponse } | null {
  const token = safeReadStorage(AUTH_TOKEN_KEY);
  const refreshToken = safeReadStorage(AUTH_REFRESH_TOKEN_KEY);
  const expiresAt = safeReadStorage(AUTH_EXPIRES_AT_KEY);
  const rawUser = safeReadStorage(AUTH_USER_KEY);

  if (!token || !refreshToken || !expiresAt || !rawUser) {
    return null;
  }

  try {
    const parsed = JSON.parse(rawUser) as unknown;

    if (!isStoredUser(parsed)) {
      return null;
    }

    return {
      session: {
        token,
        refreshToken,
        expiresAt,
      },
      user: parsed,
    };
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthResponse | null>(null);
  const [user, setUser] = useState<MeResponse | null>(null);
  const [isInitializing, setIsInitializing] = useState(true);
  const [isBusy, setIsBusy] = useState(false);
  const sessionRef = useRef<AuthResponse | null>(null);

  useEffect(() => {
    sessionRef.current = session;
  }, [session]);

  const persistAuth = useCallback((nextSession: AuthResponse, nextUser: MeResponse) => {
    safeWriteStorage(AUTH_TOKEN_KEY, nextSession.token);
    safeWriteStorage(AUTH_REFRESH_TOKEN_KEY, nextSession.refreshToken);
    safeWriteStorage(AUTH_EXPIRES_AT_KEY, nextSession.expiresAt);
    safeWriteStorage(AUTH_USER_KEY, JSON.stringify(nextUser));
  }, []);

  const clearStoredAuth = useCallback(() => {
    safeRemoveStorage(AUTH_TOKEN_KEY);
    safeRemoveStorage(AUTH_REFRESH_TOKEN_KEY);
    safeRemoveStorage(AUTH_EXPIRES_AT_KEY);
    safeRemoveStorage(AUTH_USER_KEY);
  }, []);

  const applyAuthState = useCallback(
    (nextSession: AuthResponse, nextUser: MeResponse) => {
      setSession(nextSession);
      setUser(nextUser);
      sessionRef.current = nextSession;
      persistAuth(nextSession, nextUser);
    },
    [persistAuth],
  );

  const clearSession = useCallback(() => {
    setSession(null);
    setUser(null);
    sessionRef.current = null;
    clearStoredAuth();
    setIsBusy(false);
  }, [clearStoredAuth]);

  const loadCurrentUser = useCallback(async (accessToken: string) => {
    return apiGet<MeResponse>('/auth/me', accessToken);
  }, []);

  const login = useCallback(
    async (payload: LoginRequest) => {
      setIsBusy(true);

      try {
        const nextSession = await apiPost<LoginRequest, AuthResponse>('/auth/login', payload);
        const profile = await loadCurrentUser(nextSession.token);
        applyAuthState(nextSession, profile);
      } catch (error) {
        clearSession();
        throw error;
      } finally {
        setIsBusy(false);
      }
    },
    [applyAuthState, clearSession, loadCurrentUser],
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

      const profile = await loadCurrentUser(nextSession.token);
      applyAuthState(nextSession, profile);
      return true;
    } catch {
      clearSession();
      return false;
    } finally {
      setIsBusy(false);
    }
  }, [applyAuthState, clearSession, loadCurrentUser]);

  const refreshCurrentUser = useCallback(async () => {
    const currentSession = sessionRef.current;

    if (!currentSession?.token) {
      clearSession();
      return false;
    }

    try {
      const profile = await loadCurrentUser(currentSession.token);
      setUser(profile);
      safeWriteStorage(AUTH_USER_KEY, JSON.stringify(profile));
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
    let isActive = true;

    const restoreAuth = async () => {
      const restored = readStoredAuth();

      if (!restored) {
        clearSession();

        if (isActive) {
          setIsInitializing(false);
        }

        return;
      }

      sessionRef.current = restored.session;
      setSession(restored.session);
      setUser(restored.user);

      try {
        const profile = await loadCurrentUser(restored.session.token);

        if (!isActive) {
          return;
        }

        applyAuthState(restored.session, profile);
      } catch {
        if (!isActive) {
          return;
        }

        clearSession();
      } finally {
        if (isActive) {
          setIsInitializing(false);
        }
      }
    };

    void restoreAuth();

    return () => {
      isActive = false;
    };
  }, [applyAuthState, clearSession, loadCurrentUser]);

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
      isInitializing,
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
      isInitializing,
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
