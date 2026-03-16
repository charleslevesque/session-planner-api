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

export const ALL_ROLES = ['admin', 'planner', 'technician', 'teacher', 'management'] as const;
export type RoleName = (typeof ALL_ROLES)[number];
