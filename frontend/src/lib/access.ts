import type { RoleName } from '../types/auth';

export const PAGE_ACCESS = {
  dashboard: ['admin', 'technician', 'planner', 'management', 'teacher'],
  besoins: ['admin', 'technician', 'teacher'],
  matrice: ['admin', 'planner'],
  users: ['admin'],
  security: ['admin', 'technician', 'planner', 'management', 'teacher'],
} as const satisfies Record<string, readonly RoleName[]>;

export function hasRoleAccess(role: string | null | undefined, allowedRoles: readonly RoleName[]): boolean {
  if (!role) {
    return false;
  }

  return allowedRoles.includes(role as RoleName);
}
