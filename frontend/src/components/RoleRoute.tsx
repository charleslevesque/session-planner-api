import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { hasRoleAccess } from '../lib/access';
import type { RoleName } from '../types/auth';

interface RoleRouteProps {
  children: ReactNode;
  allowedRoles: readonly RoleName[];
  redirectTo?: string;
}

export function RoleRoute({ children, allowedRoles, redirectTo = '/dashboard' }: RoleRouteProps) {
  const { user, isAuthenticated, isInitializing } = useAuth();

  if (isInitializing) {
    return (
      <div className="flex min-h-screen items-center justify-center px-6 text-center">
        <div className="surface-card max-w-md p-8">
          <p className="text-sm uppercase tracking-[0.3em] text-stone-500">Authentification</p>
          <h1 className="mt-3 text-2xl font-semibold text-stone-900">Connexion en cours</h1>
          <p className="mt-3 text-sm text-stone-600">Validation de vos droits d&apos;acces.</p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (!hasRoleAccess(user?.role, allowedRoles)) {
    return <Navigate to={redirectTo} replace />;
  }

  return children;
}
