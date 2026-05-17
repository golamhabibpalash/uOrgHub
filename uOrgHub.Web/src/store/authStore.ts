import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface UserProfileDto {
  id: string;
  username: string;
  email: string;
  phoneNumber?: string;
  firstName: string;
  lastName: string;
  fullName: string;
  employeeId?: string;
  isActive: boolean;
  isTwoFactorEnabled: boolean;
  twoFactorMethod: string;
  roles: string[];
  claims: string[];
  lastLoginAt?: string;
  profilePicture?: string;
}

interface AuthState {
  user: UserProfileDto | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  setAuth: (token: string, refreshToken: string, user: UserProfileDto) => void;
  setUser: (user: UserProfileDto) => void;
  setTokens: (token: string, refreshToken: string) => void;
  logout: () => void;
  hasRole: (role: string) => boolean;
  hasClaim: (claim: string) => boolean;
  hasAnyRole: (...roles: string[]) => boolean;
  hasAnyClaim: (...claims: string[]) => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      token: null,
      refreshToken: null,
      isAuthenticated: false,

      setAuth: (token, refreshToken, user) =>
        set({ token, refreshToken, user, isAuthenticated: true }),

      setUser: (user) => set({ user }),

      setTokens: (token, refreshToken) => set({ token, refreshToken }),

      logout: () => {
        set({ user: null, token: null, refreshToken: null, isAuthenticated: false });
        window.location.href = '/login';
      },

      hasRole: (role) => get().user?.roles.includes(role) ?? false,

      hasClaim: (claim) => get().user?.claims.includes(claim) ?? false,

      hasAnyRole: (...roles) => roles.some(r => get().user?.roles.includes(r) ?? false),

      hasAnyClaim: (...claims) => claims.some(c => get().user?.claims.includes(c) ?? false),
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
