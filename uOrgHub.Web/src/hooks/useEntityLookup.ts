import { useCallback, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import type { SelectOption } from "../components/shared/SearchableDropdown";
import {
  getAllDepartments,
  getAllDesignations,
  getAllEmployees,
  getActiveLeaveTypes,
  getAllSalaryGrades,
} from "../api/hr";
import { getChartOfAccounts, getAllAccountGroups, getCostCenters, getCustomers, getVendors, getFiscalYears, getBankAccounts, getVoucherAccountOptions, AccountGroupType, VoucherAccountOption, VoucherType } from "../api/accounts";
import { getInventoryTypes, getInventoryCategories, getUnitsOfMeasure, getWarehouses } from "../api/inventory";
import { getProjectCategories, getClients, getProjects } from "../api/projects";

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
      label: `${e.firstName} ${e.lastName} (${e.employeeCode ?? ""})`,
      searchText: `${e.firstName} ${e.lastName} ${e.employeeCode ?? ""} ${e.email ?? ""}`,
    })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

/**
 * Employees keyed by name rather than id, for fields that store a person's name as free text —
 * a voucher's "Prepared By" and "Received By" are printed as signature lines, so the name is the
 * value. Names are de-duplicated: two employees sharing one name collapse to a single option,
 * which is harmless when the stored artifact is only ever the name itself.
 */
export function useEmployeeNameLookup() {
  const query = useQuery({
    queryKey: ["employees-all"],
    queryFn: () => getAllEmployees(),
    staleTime: 60000,
  });
  const options = useMemo(() => {
    const seen = new Set<string>();
    return (query.data?.data?.data ?? []).reduce<SelectOption[]>((acc, e) => {
      const name = `${e.firstName} ${e.lastName}`.trim();
      if (!name || seen.has(name)) return acc;
      seen.add(name);
      acc.push({
        value: name,
        label: e.employeeCode ? `${name} (${e.employeeCode})` : name,
        searchText: `${name} ${e.employeeCode ?? ""} ${e.designationName ?? ""}`,
      });
      return acc;
    }, []);
  }, [query.data]);
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

export function useChartOfAccountsLookup(accountType?: AccountGroupType) {
  const query = useQuery({
    queryKey: ["chart-of-accounts"],
    queryFn: () => getChartOfAccounts({ page: 1, pageSize: 500 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(
      (query.data?.data?.data?.items ?? []).filter((a) => !accountType || a.accountType === accountType),
      (a) => ({
        value: a.id,
        label: `${a.accountCode} — ${a.accountName}`,
        searchText: `${a.accountName} ${a.accountCode}`,
      }),
    ),
    [query.data, accountType],
  );
  return { options, isLoading: query.isLoading };
}

export function useAccountGroupLookup() {
  const query = useQuery({
    queryKey: ["account-groups-all"],
    queryFn: getAllAccountGroups,
    staleTime: 60000,
  });

  const options = useMemo((): SelectOption[] => {
    const groups = query.data?.data?.data ?? [];
    const groupMap = new Map(groups.map((g) => [g.id, g]));

    function getCodePath(id: string): string[] {
      const g = groupMap.get(id);
      if (!g) return [];
      if (!g.parentAccountGroupId) return [g.code];
      return [...getCodePath(g.parentAccountGroupId), g.code];
    }

    return groups
      .map((g) => {
        const codePath = getCodePath(g.id);
        const pathStr = codePath.join(" > ");
        return { g, codePath, pathStr };
      })
      .sort((a, b) => {
        for (let i = 0; i < Math.min(a.codePath.length, b.codePath.length); i++) {
          const cmp = a.codePath[i].localeCompare(b.codePath[i]);
          if (cmp !== 0) return cmp;
        }
        return a.codePath.length - b.codePath.length;
      })
      .map(({ g, pathStr }) => ({
        value: g.id,
        label: `${pathStr} - ${g.name}`,
        searchText: `${g.name} ${g.code} ${pathStr}`,
      }));
  }, [query.data]);

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

/**
 * Cost centers not tied to a project — head office, admin, and the like. These are what an
 * overhead voucher is charged to, since picking a project is not an option for that spend.
 */
export function useOverheadCostCenterLookup() {
  const query = useQuery({
    queryKey: ["cost-centers"],
    queryFn: () => getCostCenters({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(
      (query.data?.data?.data?.items ?? []).filter((c) => !c.projectId && c.isActive),
      (c) => ({ value: c.id, label: `${c.code} — ${c.name}`, searchText: `${c.name} ${c.code}` }),
    ),
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

  const fiscalYears = useMemo(() => query.data?.data?.data?.items ?? [], [query.data]);

  const options = useMemo(
    () => toOptions(fiscalYears, (f) => ({ value: f.id, label: f.name })),
    [fiscalYears],
  );

  /**
   * The open fiscal year containing the given date. Closed years are skipped — a voucher dated
   * into one would be rejected server-side anyway, so offering it would only mislead.
   */
  const findByDate = useCallback(
    (date: string): string | undefined => {
      if (!date) return undefined;
      const match = fiscalYears.find(
        (f) =>
          f.status !== "Closed" &&
          date >= f.startDate.split("T")[0] &&
          date <= f.endDate.split("T")[0],
      );
      return match?.id;
    },
    [fiscalYears],
  );

  return { options, fiscalYears, findByDate, isLoading: query.isLoading };
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

function toAccountOption(a: VoucherAccountOption): SelectOption {
  return {
    value: a.id,
    // Bank-linked accounts show their bank so two similarly named accounts stay distinguishable.
    label: a.isBankLinked
      ? `${a.accountCode} — ${a.accountName} (${a.bankName})`
      : `${a.accountCode} — ${a.accountName}`,
    searchText: `${a.accountName} ${a.accountCode} ${a.accountGroupName} ${a.bankName ?? ""} ${a.accountNumber ?? ""}`,
    group: a.groupLabel || undefined,
  };
}

/**
 * The accounts valid for each side of a voucher, decided server-side by the same rules the save
 * endpoint enforces — so the form never offers something the server will reject.
 */
export function useVoucherAccountOptions(voucherType: VoucherType) {
  const query = useQuery({
    queryKey: ["voucher-account-options", voucherType],
    queryFn: () => getVoucherAccountOptions(voucherType),
    staleTime: 60000,
  });

  const data = query.data?.data?.data;

  const moneyOptions = useMemo(() => toOptions(data?.moneyAccounts, toAccountOption), [data]);
  const partyOptions = useMemo(() => toOptions(data?.partyAccounts, toAccountOption), [data]);

  return {
    moneyOptions,
    partyOptions,
    // A Credit Voucher takes money in, so cash/bank sits on the debit side; a Debit Voucher pays
    // money out, so it sits on the credit side. The server states which, rather than the form
    // re-deriving it.
    moneyIsOnDebitSide: data?.moneyIsOnDebitSide ?? voucherType === "Credit",
    moneyFieldLabel: data?.moneyFieldLabel ?? (voucherType === "Credit" ? "Receive Into" : "Pay From"),
    isLoading: query.isLoading,
  };
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

export function useClientLookup() {
  const query = useQuery({
    queryKey: ["clients"],
    queryFn: () => getClients({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (c) => ({
      value: c.id,
      label: `${c.companyName} (${c.clientCode})`,
      searchText: `${c.companyName} ${c.clientCode} ${c.contactPerson ?? ""}`,
    })),
    [query.data],
  );
  return { options, isLoading: query.isLoading };
}

export function useProjectLookup() {
  const query = useQuery({
    queryKey: ["projects-all"],
    queryFn: () => getProjects({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const options = useMemo(
    () => toOptions(query.data?.data?.data?.items, (p) => ({
      value: p.id,
      label: `${p.projectName} (${p.projectCode ?? ""})`,
      searchText: `${p.projectName} ${p.projectCode ?? ""}`,
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
