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
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (!hasRoleAccess(user?.role, allowedRoles)) {
    return <Navigate to={redirectTo} replace />;
  }

  return children;
}
