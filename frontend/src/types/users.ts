export interface UserResponse {
  id: number;
  username: string;
  roles: string;
  isActive: boolean;
}

export interface CreateUserRequest {
  username: string;
  password: string;
  roleName: string;
}

export interface UpdateUserRoleRequest {
  roleName: string;
}

export interface UpdateUserPasswordRequest {
  newPassword: string;
}

export const ALL_ROLES = ['admin', 'professor', 'lab_instructor', 'course_instructor'] as const;
export type RoleName = (typeof ALL_ROLES)[number];

export const ROLE_LABELS: Record<RoleName, string> = {
  admin: 'Administrateur',
  professor: 'Professeur(e)',
  lab_instructor: 'Chargé(e) de laboratoire',
  course_instructor: 'Chargé(e) de cours',
};
