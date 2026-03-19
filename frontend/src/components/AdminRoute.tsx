import type { ReactNode } from 'react';
import { PAGE_ACCESS } from '../lib/access';
import { RoleRoute } from './RoleRoute';

interface AdminRouteProps {
  children: ReactNode;
}

export function AdminRoute({ children }: AdminRouteProps) {
  return <RoleRoute allowedRoles={PAGE_ACCESS.users}>{children}</RoleRoute>;
}
