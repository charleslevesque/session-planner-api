export interface UserResponse {
  id: number;
  username: string;
  roles: string;
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
  admin: 'Admin',
  professor: 'Professor',
  lab_instructor: 'Lab Instructor',
  course_instructor: 'Course Instructor',
};
