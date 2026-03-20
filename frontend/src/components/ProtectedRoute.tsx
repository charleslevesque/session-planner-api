import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface ProtectedRouteProps {
  children: ReactNode;
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const location = useLocation();
  const { isAuthenticated, isBusy, isInitializing } = useAuth();

  if (isInitializing || isBusy) {
    return (
      <div className="flex min-h-screen items-center justify-center px-6 text-center">
        <div className="surface-card max-w-md p-8">
          <p className="text-sm uppercase tracking-[0.3em] text-stone-500">Authentification</p>
          <h1 className="mt-3 text-2xl font-semibold text-stone-900">Connexion en cours</h1>
          <p className="mt-3 text-sm text-stone-600">
            Nous préparons votre espace et validons votre session.
          </p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return children;
}
