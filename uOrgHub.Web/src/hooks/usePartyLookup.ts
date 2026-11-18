import { useCallback, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { getCustomers, getVendors, type PartyType } from "../api/accounts";
import { getClients } from "../api/projects";
import type { SelectOption } from "../components/shared/SearchableDropdown";
import { formatPhone, normalizePhone } from "../utils/smsSegments";

export interface PartyContactInfo {
  id: string;
  name: string;
  phone?: string;
  /** The phone in stored form when it is a usable mobile number. */
  mobile: string | null;
}

/**
 * Customers, vendors or clients as picker options, with each one's phone — so a form can say
 * up front whether an SMS is possible. The server still resolves the number itself when sending.
 */
export function usePartyLookup(type: PartyType | "") {
  const customers = useQuery({ queryKey: ["customers"], queryFn: () => getCustomers({ page: 1, pageSize: 200 }), staleTime: 60000, enabled: type === "Customer" });
  const vendors = useQuery({ queryKey: ["vendors"], queryFn: () => getVendors({ page: 1, pageSize: 200 }), staleTime: 60000, enabled: type === "Vendor" });
  const clients = useQuery({ queryKey: ["clients-all"], queryFn: () => getClients({ page: 1, pageSize: 200 }), staleTime: 60000, enabled: type === "Client" });

  const contacts = useMemo<PartyContactInfo[]>(() => {
    const toInfo = (id: string, name: string, phone?: string | null) => ({ id, name, phone: phone ?? undefined, mobile: phone ? normalizePhone(phone) : null });
    if (type === "Customer") return (customers.data?.data?.data?.items ?? []).map((c) => toInfo(c.id, c.name, c.phone));
    if (type === "Vendor") return (vendors.data?.data?.data?.items ?? []).map((v) => toInfo(v.id, v.name, v.phone));
    if (type === "Client") return (clients.data?.data?.data?.items ?? []).map((c) => toInfo(c.id, c.companyName, c.phone));
    return [];
  }, [type, customers.data, vendors.data, clients.data]);

  const options = useMemo<SelectOption[]>(
    () => contacts.map((c) => ({
      value: c.id,
      label: c.mobile ? `${c.name} · ${formatPhone(c.mobile)}` : `${c.name} · no mobile`,
      searchText: `${c.name} ${c.phone ?? ""}`,
    })),
    [contacts],
  );

  const find = useCallback((id: string) => contacts.find((c) => c.id === id), [contacts]);
  const isLoading = customers.isLoading || vendors.isLoading || clients.isLoading;
  return { options, find, isLoading };
}
