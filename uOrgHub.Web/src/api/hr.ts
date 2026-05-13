import apiClient from "./client";
import { ApiResponse, PagedResult, PaginationRequest } from "../types/api";

export interface Employee {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  departmentId: string;
  departmentName: string;
  designationId: string;
  designationName: string;
  employmentType: string;
  status: string;
  joiningDate: string;
  basicSalary: number;
}

export interface Department {
  id: string;
  name: string;
  code: string;
  description: string;
  isActive: boolean;
}

export interface Designation {
  id: string;
  name: string;
  code: string;
  departmentId: string;
  departmentName: string;
  isActive: boolean;
}

// Departments
export const getDepartments = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<Department>>>("/departments", {
    params,
  });

export const getDepartmentById = (id: string) =>
  apiClient.get<ApiResponse<Department>>(`/departments/${id}`);

export const createDepartment = (data: Partial<Department>) =>
  apiClient.post<ApiResponse<Department>>("/departments", data);

export const updateDepartment = (id: string, data: Partial<Department>) =>
  apiClient.put<ApiResponse<Department>>(`/departments/${id}`, data);

export const deleteDepartment = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/departments/${id}`);

// Employees
export const getEmployees = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<Employee>>>("/employees", { params });

export const getEmployeeById = (id: string) =>
  apiClient.get<ApiResponse<Employee>>(`/employees/${id}`);

export const createEmployee = (data: Partial<Employee>) =>
  apiClient.post<ApiResponse<Employee>>("/employees", data);

export const updateEmployee = (id: string, data: Partial<Employee>) =>
  apiClient.put<ApiResponse<Employee>>(`/employees/${id}`, data);

export const deleteEmployee = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/employees/${id}`);
