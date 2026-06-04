import apiClient from './client';
import type { UserProfileDto } from '../store/authStore';

export interface LoginRequest { username: string; password: string; rememberMe?: boolean; }
export interface LoginResponse {
  requiresTwoFactor: boolean;
  twoFactorMethods?: string[];
  maskedEmail?: string;
  maskedPhone?: string;
  tempToken?: string;
  accessToken?: string;
  refreshToken?: string;
  user?: UserProfileDto;
}
export interface VerifyOTPRequest { tempToken: string; otpCode: string; channel: string; }
export interface TwoFactorResponse { accessToken: string; refreshToken: string; user: UserProfileDto; }
export interface TokenResponse { accessToken: string; refreshToken: string; expiresIn: number; user?: UserProfileDto; }
export interface PagedResult<T> { items: T[]; totalCount: number; page: number; pageSize: number; totalPages: number; }

export interface UserDto {
  id: string; username: string; email: string; phoneNumber?: string;
  firstName: string; lastName: string; fullName: string; employeeId?: string;
  isActive: boolean; isTwoFactorEnabled: boolean; twoFactorMethod: string;
  roles: string[]; claims: string[]; lastLoginAt?: string; profilePicture?: string;
  createdAt: string;
}
export interface RoleDto {
  id: string; name: string; description?: string; isSystem: boolean;
  isActive: boolean; userCount: number; claims: ClaimDto[];
}
export interface ClaimDto { id: string; name: string; description?: string; module?: string; category?: string; isActive: boolean; }
export interface AccessLogDto {
  id: string; userId?: string; username?: string; action: string; module?: string;
  entityType?: string; entityId?: string; httpMethod?: string; endpoint?: string;
  responseStatusCode: number; ipAddress?: string; userAgent?: string;
  durationMs: number; isSuccess: boolean; errorMessage?: string; createdAt: string;
}
export interface SessionDto {
  id: string; userId: string; deviceInfo?: string; ipAddress?: string;
  browser?: string; os?: string; loginAt: string; lastActivityAt: string;
  logoutAt?: string; isActive: boolean; logoutReason?: string;
}

function unwrap<T>(response: { data: { data: T } }) { return response.data.data; }

// Auth
export const login = (dto: LoginRequest) =>
  apiClient.post<{ data: LoginResponse }>('/auth/login', dto).then(unwrap);

export const verifyOTP = (dto: VerifyOTPRequest) =>
  apiClient.post<{ data: TwoFactorResponse }>('/auth/verify-otp', dto).then(unwrap);

export const refreshToken = (token: string) =>
  apiClient.post<{ data: TokenResponse }>('/auth/refresh-token', { refreshToken: token }).then(unwrap);

export const logout = () => apiClient.post('/auth/logout');

export const getProfile = () =>
  apiClient.get<{ data: UserProfileDto }>('/auth/me').then(unwrap);

export const updateProfile = (dto: Partial<{ email: string; phoneNumber: string; firstName: string; lastName: string }>) =>
  apiClient.put<{ data: UserProfileDto }>('/auth/me', dto).then(unwrap);

export const changePassword = (dto: { currentPassword: string; newPassword: string; confirmPassword: string }) =>
  apiClient.post('/auth/change-password', dto);

export const forgotPassword = (email: string) =>
  apiClient.post('/auth/forgot-password', { email });

export const resetPassword = (dto: { email: string; otpCode: string; newPassword: string; confirmPassword: string }) =>
  apiClient.post('/auth/reset-password', dto);

export interface ValidateSetPasswordTokenResponse {
  isValid: boolean;
  username?: string;
  fullName?: string;
  expiresAt?: string;
}

export const validateSetPasswordToken = (token: string) =>
  apiClient.get<{ data: ValidateSetPasswordTokenResponse }>('/auth/set-password/validate', { params: { token } }).then(unwrap);

export const setPassword = (token: string, newPassword: string) =>
  apiClient.post('/auth/set-password', { token, newPassword });

export const resendInvite = (userId: string) =>
  apiClient.post(`/users/${userId}/resend-invite`);

export const sendOTP = (otpType: string, channel: string) =>
  apiClient.post<{ data: string }>('/auth/send-otp', { otpType, channel }).then(unwrap);

export const toggle2FA = (enabled: boolean, method?: string) =>
  apiClient.put('/auth/me/2fa', { enabled, twoFactorMethod: method });

// Users
export const getUsers = (params: { page?: number; pageSize?: number; search?: string }) =>
  apiClient.get<{ data: PagedResult<UserDto> }>('/users', { params }).then(unwrap);

export const getUserById = (id: string) =>
  apiClient.get<{ data: UserDto }>(`/users/${id}`).then(unwrap);

export const createUser = (dto: { username: string; email: string; password: string; firstName: string; lastName: string; roleIds?: string[]; employeeId?: string }) =>
  apiClient.post<{ data: UserDto }>('/users', dto).then(unwrap);

export const updateUser = (id: string, dto: Partial<UserDto>) =>
  apiClient.put<{ data: UserDto }>(`/users/${id}`, dto).then(unwrap);

export const changeUsername = (id: string, newUsername: string) =>
  apiClient.put<{ data: UserDto }>(`/users/${id}/username`, { newUsername }).then(unwrap);

export const deleteUser = (id: string) => apiClient.delete(`/users/${id}`);

export const activateUser = (id: string) => apiClient.post(`/users/${id}/activate`);
export const deactivateUser = (id: string) => apiClient.post(`/users/${id}/deactivate`);
export const unlockUser = (id: string) => apiClient.post(`/users/${id}/unlock`);
export const forceLogout = (id: string) => apiClient.post(`/users/${id}/force-logout`);

export const setUserRoles = (id: string, roleIds: string[]) =>
  apiClient.put(`/users/${id}/roles`, { roleIds });

export const addUserClaim = (id: string, claimId: string, isGranted: boolean) =>
  apiClient.post(`/users/${id}/claims`, { claimId, isGranted });

export const removeUserClaim = (id: string, claimId: string) =>
  apiClient.delete(`/users/${id}/claims/${claimId}`);

export const getUserSessions = (id: string) =>
  apiClient.get<{ data: SessionDto[] }>(`/users/${id}/sessions`).then(unwrap);

export const getUserAccessLogs = (id: string, params?: AccessLogFilterParams) =>
  apiClient.get<{ data: PagedResult<AccessLogDto> }>(`/users/${id}/access-logs`, { params }).then(unwrap);

// Roles
export const getRoles = () =>
  apiClient.get<{ data: RoleDto[] }>('/roles').then(unwrap);

export const getRoleById = (id: string) =>
  apiClient.get<{ data: RoleDto }>(`/roles/${id}`).then(unwrap);

export const createRole = (dto: { name: string; description?: string }) =>
  apiClient.post<{ data: RoleDto }>('/roles', dto).then(unwrap);

export const updateRole = (id: string, dto: { name?: string; description?: string; isActive?: boolean }) =>
  apiClient.put<{ data: RoleDto }>(`/roles/${id}`, dto).then(unwrap);

export const deleteRole = (id: string) => apiClient.delete(`/roles/${id}`);

export const assignRoleClaims = (roleId: string, claimIds: string[]) =>
  apiClient.post(`/roles/${roleId}/claims`, { claimIds });

export const removeRoleClaim = (roleId: string, claimId: string) =>
  apiClient.delete(`/roles/${roleId}/claims/${claimId}`);

// Claims
export const getClaims = () =>
  apiClient.get<{ data: ClaimDto[] }>('/claims').then(unwrap);

// Menu
export interface MenuItemDto {
  key: string;
  label: string;
  icon?: string;
  path?: string;
  requiredClaim?: string;
  requiredRole?: string;
  section?: string;
  children?: MenuItemDto[];
}

export const getMenuItems = () =>
  apiClient.get<{ data: MenuItemDto[] }>('/auth/menu').then(unwrap);

export interface AccessLogFilterParams {
  page?: number;
  pageSize?: number;
  search?: string;
  userId?: string;
  username?: string;
  module?: string;
  action?: string;
  httpMethod?: string;
  isSuccess?: boolean;
  entityType?: string;
  ipAddress?: string;
  statusCodeFrom?: number;
  statusCodeTo?: number;
  durationMin?: number;
  durationMax?: number;
  dateFrom?: string;
  dateTo?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

// Access Logs
export const getAccessLogs = (params?: AccessLogFilterParams) =>
  apiClient.get<{ data: PagedResult<AccessLogDto> }>('/access-logs', { params }).then(unwrap);

export const getMyAccessLogs = (params?: AccessLogFilterParams) =>
  apiClient.get<{ data: PagedResult<AccessLogDto> }>('/access-logs/my', { params }).then(unwrap);
