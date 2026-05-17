import apiClient from './client';

const API_ORIGIN = (import.meta.env.VITE_API_URL ?? 'http://localhost:5177/api/v1').replace(/\/api\/v1\/?$/, '');

export function getLogoUrl(path: string | null | undefined): string | null {
  if (!path) return null;
  if (path.startsWith('http')) return path;
  return API_ORIGIN + path;
}

export interface CompanyStatusDto {
  hasCompany: boolean;
  hasUsers: boolean;
}

export interface CompanySetupDto {
  companyName: string;
  tagLine?: string;
  address?: string;
  phone?: string;
  email?: string;
  taxId?: string;
  logoUrl?: string;
  currency?: string;
  timeZone?: string;
  adminUsername: string;
  adminEmail: string;
  adminFirstName: string;
  adminLastName: string;
  adminPassword: string;
}

export interface CompanySetupResultDto {
  companyId: string;
  companyName: string;
}

export interface CompanyDto {
  id: string;
  name: string;
  tagLine?: string;
  address?: string;
  phone?: string;
  email?: string;
  taxId?: string;
  logoUrl?: string;
  currency: string;
  timeZone: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

function unwrap<T>(response: { data: { data: T } }) { return response.data.data; }

export const getCompanyStatus = () =>
  apiClient.get<{ data: CompanyStatusDto }>('/company/status').then(unwrap);

export const setupCompany = (dto: CompanySetupDto) =>
  apiClient.post<{ data: CompanySetupResultDto }>('/company/setup', dto).then(unwrap);

export const getCompanies = (params?: { page?: number; pageSize?: number }) =>
  apiClient.get<{ data: { items: CompanyDto[]; totalCount: number } }>('/company', { params }).then(unwrap);

export const getCompanyById = (id: string) =>
  apiClient.get<{ data: CompanyDto }>(`/company/${id}`).then(unwrap);

export const getMyCompany = () =>
  apiClient.get<{ data: CompanyDto }>('/company/mine').then(unwrap);

export const updateCompany = (id: string, dto: Partial<CompanyDto>) =>
  apiClient.put<{ data: CompanyDto }>(`/company/${id}`, dto).then(unwrap);

export const deleteCompany = (id: string) =>
  apiClient.delete(`/company/${id}`);

export const uploadLogo = (id: string, file: File) => {
  const formData = new FormData();
  formData.append('file', file);
  return apiClient.post<{ data: string }>(`/company/${id}/logo`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }).then(unwrap);
};
