import type { RoleName } from '../types/auth';

export const PAGE_ACCESS = {
  dashboard: ['admin', 'professor', 'lab_instructor', 'course_instructor'],
  sessionsManage: ['admin', 'lab_instructor'],
  besoins: ['admin', 'lab_instructor', 'professor', 'course_instructor'],
  sessionNeeds: ['admin', 'lab_instructor', 'professor', 'course_instructor'],
  matrice: ['admin', 'lab_instructor'],
  users: ['admin'],
  security: ['admin', 'professor', 'lab_instructor', 'course_instructor'],
} as const satisfies Record<string, readonly RoleName[]>;

export function hasRoleAccess(role: string | null | undefined, allowedRoles: readonly RoleName[]): boolean {
  if (!role) {
    return false;
  }

  return allowedRoles.includes(role as RoleName);
}
