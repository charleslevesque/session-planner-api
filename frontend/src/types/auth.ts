export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role?: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface UpdateCurrentUserEmailRequest {
  newEmail: string;
  currentPassword: string;
}

export const APP_ROLES = ['admin', 'professor', 'lab_instructor', 'course_instructor'] as const;
export type RoleName = (typeof APP_ROLES)[number];

export interface MeResponse {
  id: number;
  email: string;
  name: string;
  role: RoleName;
}
