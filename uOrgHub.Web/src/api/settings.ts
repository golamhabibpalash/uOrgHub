import apiClient from "./client";
import { ApiResponse, PagedResult, PaginationRequest } from "../types/api";

export interface SystemSetting {
  id: string;
  category: string;
  key: string;
  value: string;
  dataType: string;
  description: string | null;
  isActive: boolean;
  isSystem: boolean;
  createdAt: string;
  createdBy: string;
  updatedAt: string | null;
  updatedBy: string | null;
}

export interface ValidationRule {
  id: string;
  entityType: string;
  fieldName: string;
  ruleType: string;
  ruleValue: string | null;
  errorMessage: string | null;
  severity: string;
  isEnabled: boolean;
  sortOrder: number;
  createdAt: string;
  createdBy: string;
  updatedAt: string | null;
  updatedBy: string | null;
}

// ── System Settings ──

export const getSystemSettings = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<SystemSetting>>>("settings", { params });

export const getAllSystemSettings = () =>
  apiClient.get<ApiResponse<SystemSetting[]>>("settings/all");

export const getSystemSettingsByCategory = (category: string) =>
  apiClient.get<ApiResponse<SystemSetting[]>>(`settings/by-category/${category}`);

export const getSystemSettingByKey = (key: string) =>
  apiClient.get<ApiResponse<SystemSetting>>(`settings/by-key/${key}`);

export const createSystemSetting = (data: Partial<SystemSetting>) =>
  apiClient.post<ApiResponse<SystemSetting>>("settings", data);

export const updateSystemSetting = (id: string, data: Partial<SystemSetting>) =>
  apiClient.put<ApiResponse<SystemSetting>>(`settings/${id}`, data);

export const deleteSystemSetting = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`settings/${id}`);

// ── Validation Rules ──

export const getValidationRules = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<ValidationRule>>>("validation-rules", { params });

export const getValidationRulesByEntity = (entityType: string) =>
  apiClient.get<ApiResponse<ValidationRule[]>>(`validation-rules/by-entity/${entityType}`);

export const createValidationRule = (data: Partial<ValidationRule>) =>
  apiClient.post<ApiResponse<ValidationRule>>("validation-rules", data);

export const updateValidationRule = (id: string, data: Partial<ValidationRule>) =>
  apiClient.put<ApiResponse<ValidationRule>>(`validation-rules/${id}`, data);

export const deleteValidationRule = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`validation-rules/${id}`);
