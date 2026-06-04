import apiClient from './client';
import type { ApiResponse } from '../types/api';

export interface ThemeSettings {
  id: string;
  themeName: string;
  primaryColor: string;
  sidebarBg: string;
  sidebarText: string;
  sidebarActiveBg: string;
  sidebarActiveText: string;
  isDarkMode: boolean;
}

export interface PredefinedPalette {
  name: string;
  label: string;
  primaryColor: string;
  sidebarBg: string;
  sidebarText: string;
  sidebarActiveBg: string;
  sidebarActiveText: string;
}

export type PaletteKey =
  | 'corporate-blue'
  | 'professional-green'
  | 'modern-indigo'
  | 'dark-enterprise'
  | 'warm-amber'
  | 'slate-gray';

export const PALETTES: Record<PaletteKey, PredefinedPalette> = {
  'corporate-blue': {
    name: 'corporate-blue', label: 'Corporate Blue',
    primaryColor: '#0ea5e9', sidebarBg: '#1e293b',
    sidebarText: '#cbd5e1', sidebarActiveBg: '#0ea5e9', sidebarActiveText: '#ffffff',
  },
  'professional-green': {
    name: 'professional-green', label: 'Professional Green',
    primaryColor: '#10b981', sidebarBg: '#064e3b',
    sidebarText: '#a7f3d0', sidebarActiveBg: '#10b981', sidebarActiveText: '#ffffff',
  },
  'modern-indigo': {
    name: 'modern-indigo', label: 'Modern Indigo',
    primaryColor: '#6366f1', sidebarBg: '#1e1b4b',
    sidebarText: '#c7d2fe', sidebarActiveBg: '#6366f1', sidebarActiveText: '#ffffff',
  },
  'dark-enterprise': {
    name: 'dark-enterprise', label: 'Dark Enterprise',
    primaryColor: '#6b7280', sidebarBg: '#111111',
    sidebarText: '#d1d5db', sidebarActiveBg: '#6b7280', sidebarActiveText: '#ffffff',
  },
  'warm-amber': {
    name: 'warm-amber', label: 'Warm Amber',
    primaryColor: '#d97706', sidebarBg: '#292524',
    sidebarText: '#fde68a', sidebarActiveBg: '#d97706', sidebarActiveText: '#ffffff',
  },
  'slate-gray': {
    name: 'slate-gray', label: 'Slate Gray',
    primaryColor: '#64748b', sidebarBg: '#0f172a',
    sidebarText: '#cbd5e1', sidebarActiveBg: '#64748b', sidebarActiveText: '#ffffff',
  },
};

export const getTheme = () =>
  apiClient.get<ApiResponse<ThemeSettings>>('/theme').then(r => r.data.data!);

export const updateTheme = (dto: Partial<ThemeSettings>) =>
  apiClient.put<ApiResponse<ThemeSettings>>('/theme', dto).then(r => r.data.data!);

export const resetTheme = () =>
  apiClient.post<ApiResponse<ThemeSettings>>('/theme/reset').then(r => r.data.data!);
