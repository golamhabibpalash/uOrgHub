import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import type { SelectOption } from "../components/shared/SearchableDropdown";
import {
  getAllDepartments,
  getAllDesignations,
  getAllEmployees,
  getActiveLeaveTypes,
  getAllSalaryGrades,
} from "../api/hr";
import { getChartOfAccounts, getAccountGroups, getCostCenters, getCustomers, getVendors, getFiscalYears, getBankAccounts } from "../api/accounts";
import { getInventoryTypes, getInventoryCategories, getUnitsOfMeasure, getWarehouses } from "../api/inventory";
import { getProjectCategories } from "../api/projects";

type OptionMapper<T> = (item: T) => SelectOption;

function toOptions<T>(data: T[] | null | undefined, mapper: OptionMapper<T>): SelectOption[] {
  if (!data) return [];
  return data.map(mapper);
}

// --- HR Lookups ---

export function useDepartmentLookup() {
  const query = useQuery({
    queryKey: ["departments-all"],
    queryFn: getAllDepartments,
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data, (d) => ({ value: d.id, label: d.name, searchText: `${d.name} ${d.code ?? ""}` })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useDesignationLookup() {
  const query = useQuery({
    queryKey: ["designations-all"],
    queryFn: getAllDesignations,
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data, (d) => ({ value: d.id, label: d.name, searchText: `${d.name} ${d.code ?? ""}` })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useEmployeeLookup() {
  const query = useQuery({
    queryKey: ["employees-all"],
    queryFn: () => getAllEmployees(),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data, (e) => ({
      value: e.id,
      label: `${e.firstName} ${e.lastName}`,
      searchText: `${e.firstName} ${e.lastName} ${e.employeeCode ?? ""} ${e.email ?? ""}`,
    })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useLeaveTypeLookup() {
  const query = useQuery({
    queryKey: ["leave-types-active"],
    queryFn: getActiveLeaveTypes,
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data, (l) => ({ value: l.id, label: l.name })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useSalaryGradeLookup() {
  const query = useQuery({
    queryKey: ["salary-grades-all"],
    queryFn: getAllSalaryGrades,
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data, (s) => ({
      value: s.id,
      label: `${s.gradeCode} — ${s.name}`,
      searchText: `${s.name} ${s.gradeCode}`,
    })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

// --- Accounts Lookups ---

export function useChartOfAccountsLookup() {
  const query = useQuery({
    queryKey: ["chart-of-accounts"],
    queryFn: () => getChartOfAccounts({ page: 1, pageSize: 500 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (a) => ({
      value: a.id,
      label: `${a.accountCode} — ${a.accountName}`,
      searchText: `${a.accountName} ${a.accountCode}`,
    })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useAccountGroupLookup() {
  const query = useQuery({
    queryKey: ["account-groups"],
    queryFn: () => getAccountGroups({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (g) => ({ value: g.id, label: g.name })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useCostCenterLookup() {
  const query = useQuery({
    queryKey: ["cost-centers"],
    queryFn: () => getCostCenters({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (c) => ({ value: c.id, label: c.name })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useCustomerLookup() {
  const query = useQuery({
    queryKey: ["customers"],
    queryFn: () => getCustomers({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (c) => ({
      value: c.id,
      label: c.name,
      searchText: `${c.name} ${c.customerCode ?? ""} ${c.email ?? ""} ${c.contactPerson ?? ""}`,
    })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useVendorLookup() {
  const query = useQuery({
    queryKey: ["vendors"],
    queryFn: () => getVendors({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (v) => ({
      value: v.id,
      label: v.name,
      searchText: `${v.name} ${v.vendorCode ?? ""} ${v.email ?? ""} ${v.contactPerson ?? ""}`,
    })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useFiscalYearLookup() {
  const query = useQuery({
    queryKey: ["fiscal-years"],
    queryFn: () => getFiscalYears({ page: 1, pageSize: 50 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (f) => ({ value: f.id, label: f.name })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useBankAccountLookup() {
  const query = useQuery({
    queryKey: ["bank-accounts"],
    queryFn: () => getBankAccounts({ page: 1, pageSize: 100 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (b) => ({
      value: b.id,
      label: `${b.accountName} (${b.bankName ?? ""})`,
      searchText: `${b.accountName} ${b.bankName ?? ""} ${b.accountNumber ?? ""}`,
    })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

// --- Inventory Lookups ---

export function useInventoryTypeLookup() {
  const query = useQuery({
    queryKey: ["inventory-types-all"],
    queryFn: () => getInventoryTypes({ page: 1, pageSize: 100 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (t) => ({ value: t.id, label: t.name })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useInventoryCategoryLookup() {
  const query = useQuery({
    queryKey: ["inventory-categories-all"],
    queryFn: () => getInventoryCategories({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (c) => ({ value: c.id, label: c.name })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useUnitOfMeasureLookup() {
  const query = useQuery({
    queryKey: ["units-of-measure-all"],
    queryFn: () => getUnitsOfMeasure({ page: 1, pageSize: 100 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (u) => ({
      value: u.id,
      label: `${u.name} (${u.abbreviation ?? ""})`,
      searchText: `${u.name} ${u.abbreviation ?? ""}`,
    })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

// --- Projects Lookups ---

export function useProjectCategoryLookup() {
  const query = useQuery({
    queryKey: ["project-categories"],
    queryFn: () => getProjectCategories({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (c) => ({
      value: c.id,
      label: `${c.code} — ${c.name}`,
      searchText: `${c.name} ${c.code}`,
    })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useWarehouseLookup() {
  const query = useQuery({
    queryKey: ["warehouses"],
    queryFn: () => getWarehouses({ page: 1, pageSize: 100 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (w) => ({ value: w.id, label: w.name })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}
