import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useChartOfAccountsLookup } from "../../hooks/useEntityLookup";
import {
  getCustomers,
  createCustomer,
  updateCustomer,
  deleteCustomer,
  Customer,
} from "../../api/accounts";

export default function Customers() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "name" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Customer | null>(null);
  const [form, setForm] = useState({
    customerCode: "",
    name: "",
    contactPerson: "",
    email: "",
    phone: "",
    address: "",
    tin: "",
    bin: "",
    creditLimit: 0,
    paymentTermsDays: 30,
    receivableAccountId: "",
    isActive: true,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["customers", dg.page, dg.search, dg.sortBy, dg.sortDescending],
    queryFn: () => getCustomers(dg.queryParams),
  });

  const { options: coaOptions } = useChartOfAccountsLookup();

  const customers = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;
  const [saveError, setSaveError] = useState("");

  const saveMutation = useMutation({
    mutationFn: () => editing ? updateCustomer(editing.id, form) : createCustomer(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["customers"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save customer.";
      setSaveError(msg);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteCustomer(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["customers"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ customerCode: "", name: "", contactPerson: "", email: "", phone: "", address: "", tin: "", bin: "", creditLimit: 0, paymentTermsDays: 30, receivableAccountId: coaOptions[0]?.value ?? "", isActive: true });
    setSaveError("");
    setModal(true);
  }

  function openEdit(c: Customer) {
    setEditing(c);
    setForm({ customerCode: c.customerCode, name: c.name, contactPerson: c.contactPerson ?? "", email: c.email ?? "", phone: c.phone ?? "", address: c.address ?? "", tin: c.tin ?? "", bin: c.bin ?? "", creditLimit: c.creditLimit, paymentTermsDays: c.paymentTermsDays, receivableAccountId: c.receivableAccountId, isActive: c.isActive });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setSaveError(""); }

  const columns = [
    { key: "customerCode", label: "Code" },
    { key: "name", label: "Customer Name" },
    { key: "contactPerson", label: "Contact Person" },
    { key: "phone", label: "Phone" },
    { key: "email", label: "Email" },
    {
      key: "paymentTermsDays",
      label: "Payment Terms",
      render: (row: Customer) => <span>{row.paymentTermsDays} days</span>,
    },
    {
      key: "creditLimit",
      label: "Credit Limit",
      render: (row: Customer) => row.creditLimit.toLocaleString("en-BD", { minimumFractionDigits: 2 }),
    },
    {
      key: "isActive",
      label: "Status",
      sortable: false,
      render: (row: Customer) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${row.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>
          {row.isActive ? "Active" : "Inactive"}
        </span>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Customers</h2>
          <p className="text-xs text-gray-400">Manage accounts receivable customers</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Customer
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={customers}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search customers..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => deleteMutation.mutate(row.id)}
        emptyMessage="No customers found"
        actions={<ExportMenu baseUrl="customers" filters={{ search: dg.search || undefined }} />}
      />

      <Modal title={editing ? "Edit Customer" : "Add Customer"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Customer Code *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.customerCode} onChange={(e) => setForm((f) => ({ ...f, customerCode: e.target.value }))} disabled={!!editing} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Name *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Contact Person</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.contactPerson} onChange={(e) => setForm((f) => ({ ...f, contactPerson: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Phone</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.phone} onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))} />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Email</label>
            <input type="email" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.email} onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Address</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.address} onChange={(e) => setForm((f) => ({ ...f, address: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">TIN</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.tin} onChange={(e) => setForm((f) => ({ ...f, tin: e.target.value }))} placeholder="Tax ID Number" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">BIN</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.bin} onChange={(e) => setForm((f) => ({ ...f, bin: e.target.value }))} placeholder="Business ID Number" />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Credit Limit</label>
              <input type="number" min={0} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.creditLimit} onChange={(e) => setForm((f) => ({ ...f, creditLimit: parseFloat(e.target.value) || 0 }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payment Terms (Days)</label>
              <input type="number" min={0} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.paymentTermsDays} onChange={(e) => setForm((f) => ({ ...f, paymentTermsDays: parseInt(e.target.value) || 0 }))} />
            </div>
          </div>
          <div>
            <SearchableDropdown
              label="Receivable Account *"
              options={coaOptions}
              value={form.receivableAccountId}
              onChange={(v) => setForm((f) => ({ ...f, receivableAccountId: v ?? "" }))}
              placeholder="Select account"
              searchPlaceholder="Search accounts..."
              required
            />
          </div>
          {editing && (
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isActive" checked={form.isActive} onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))} />
              <label htmlFor="isActive" className="text-xs text-gray-600">Active</label>
            </div>
          )}
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
