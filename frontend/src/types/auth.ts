export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export const APP_ROLES = ['admin', 'technician', 'planner', 'management', 'teacher'] as const;
export type RoleName = (typeof APP_ROLES)[number];

export interface MeResponse {
  id: number;
  email: string;
  name: string;
  role: RoleName;
}
