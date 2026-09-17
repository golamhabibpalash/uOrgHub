import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Search } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import ReportLayout from "../../components/shared/ReportLayout";
import { useDataGrid } from "../../hooks/useDataGrid";
import { getVendors, createVendor, updateVendor, deleteVendor, Vendor, VendorType, VendorStatus } from "../../api/procurement";

// Mirrors CreateVendorValidator/UpdateVendorValidator in uOrgHub.Procurement — keep in sync.
const PHONE_PATTERN = /^\+?[\d\s\-()]{7,20}$/;

export default function Vendors() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "companyName" });
  const [filterStatus, setFilterStatus] = useState("");
  const [filterType, setFilterType] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Vendor | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [form, setForm] = useState({
    companyName: "", contactPerson: "", email: "", phone: "",
    address: "", tradeLicense: "", tin: "", bin: "",
    vendorType: "Supplier" as VendorType, status: "Active" as VendorStatus,
    creditLimit: 0, paymentTermDays: 30, notes: "",
  });

  const companyNameError = form.companyName.trim() ? undefined : "Company name is required.";
  const phoneError = !form.phone.trim()
    ? "Phone number is required."
    : !PHONE_PATTERN.test(form.phone.trim())
      ? "Enter a valid phone number (digits, spaces, +, -, ( ) only, 7–20 characters)."
      : undefined;
  const isFormValid = !companyNameError && !phoneError;

  const { data, isLoading } = useQuery({
    queryKey: ["vendors", ...dg.queryKey, filterStatus, filterType],
    queryFn: () => getVendors(dg.queryParams,
      filterStatus ? filterStatus as VendorStatus : undefined,
      filterType ? filterType as VendorType : undefined),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => editing
      ? updateVendor(editing.id, form)
      : createVendor(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["vendors"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteVendor(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["vendors"] }),
  });

  function openAdd() {
    setEditing(null);
    setSubmitted(false);
    setForm({
      companyName: "", contactPerson: "", email: "", phone: "",
      address: "", tradeLicense: "", tin: "", bin: "",
      vendorType: "Supplier", status: "Active",
      creditLimit: 0, paymentTermDays: 30, notes: "",
    });
    setModal(true);
  }

  function openEdit(vendor: Vendor) {
    setEditing(vendor);
    setSubmitted(false);
    setForm({
      companyName: vendor.companyName,
      contactPerson: vendor.contactPerson ?? "",
      email: vendor.email ?? "",
      phone: vendor.phone ?? "",
      address: vendor.address ?? "",
      tradeLicense: vendor.tradeLicense ?? "",
      tin: vendor.tin ?? "",
      bin: vendor.bin ?? "",
      vendorType: vendor.vendorType,
      status: vendor.status,
      creditLimit: vendor.creditLimit,
      paymentTermDays: vendor.paymentTermDays,
      notes: vendor.notes ?? "",
    });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  const statusColors: Record<string, string> = {
    Active: "bg-green-50 text-green-700",
    Inactive: "bg-gray-50 text-gray-600",
    Blacklisted: "bg-red-50 text-red-700",
  };

  const typeLabels: Record<string, string> = {
    Supplier: "Supplier",
    Contractor: "Contractor",
    Consultant: "Consultant",
    ServiceProvider: "Service Provider",
  };

  const columns = [
    { key: "vendorCode", label: "Code", render: (row: Vendor) => <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.vendorCode}</span> },
    { key: "companyName", label: "Company Name" },
    { key: "contactPerson", label: "Contact" },
    { key: "email", label: "Email" },
    { key: "vendorType", label: "Type", sortable: false, render: (row: Vendor) => <span className="text-xs bg-blue-50 text-blue-700 px-2 py-0.5 rounded">{typeLabels[row.vendorType]}</span> },
    { key: "status", label: "Status", sortable: false, render: (row: Vendor) => <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span> },
    { key: "creditLimit", label: "Credit Limit", render: (row: Vendor) => <span>${row.creditLimit.toLocaleString()}</span> },
  ];

  // Search, both status/type filters and export live in one row (ReportLayout's `filters` slot)
  // instead of being split across two rows, and Print/Columns come from ReportLayout for free —
  // the same printing experience the Accounts reports already use.
  const filters = (
    <div className="flex flex-wrap items-center gap-3">
      <div className="relative flex-1 min-w-[200px] max-w-sm">
        <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
        <input
          type="text"
          placeholder="Search vendors..."
          value={dg.search}
          onChange={(e) => dg.setSearch(e.target.value)}
          className="w-full text-sm border border-gray-200 rounded-lg pl-9 pr-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
        />
      </div>
      <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); dg.setPage(1); }}
        className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
        <option value="">All Status</option>
        <option value="Active">Active</option>
        <option value="Inactive">Inactive</option>
        <option value="Blacklisted">Blacklisted</option>
      </select>
      <select value={filterType} onChange={(e) => { setFilterType(e.target.value); dg.setPage(1); }}
        className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
        <option value="">All Types</option>
        <option value="Supplier">Supplier</option>
        <option value="Contractor">Contractor</option>
        <option value="Consultant">Consultant</option>
        <option value="ServiceProvider">Service Provider</option>
      </select>
      <div className="ml-auto">
        <ExportMenu baseUrl="vendors" filters={{ search: dg.search || undefined, status: filterStatus || undefined, type: filterType || undefined }} />
      </div>
    </div>
  );

  return (
    <div>
      <ReportLayout
        title="Vendors"
        subtitle="Manage vendor database"
        filters={filters}
        headerActions={
          <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
            <Plus size={15} /> Add Vendor
          </button>
        }
      >
        <DataGrid
          columns={columns}
          data={items}
          loading={isLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          page={dg.page}
          totalPages={totalPages}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={totalCount}
          onEdit={openEdit}
          onDelete={(row) => deleteMutation.mutate(row.id)}
          emptyMessage="No vendors found"
        />
      </ReportLayout>

      <Modal title={editing ? "Edit Vendor" : "Add Vendor"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Company Name *</label>
            <input className={`w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 ${
                submitted && companyNameError ? "border-red-400 focus:ring-red-400" : "border-gray-200 focus:ring-primary-500"
              }`}
              value={form.companyName} onChange={(e) => setForm((f) => ({ ...f, companyName: e.target.value }))} />
            {submitted && companyNameError && <p className="text-xs text-red-500 mt-1">{companyNameError}</p>}
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Contact Person</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.contactPerson} onChange={(e) => setForm((f) => ({ ...f, contactPerson: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Vendor Type</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.vendorType} onChange={(e) => setForm((f) => ({ ...f, vendorType: e.target.value as VendorType }))}>
                <option value="Supplier">Supplier</option>
                <option value="Contractor">Contractor</option>
                <option value="Consultant">Consultant</option>
                <option value="ServiceProvider">Service Provider</option>
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Email</label>
              <input type="email" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.email} onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Phone *</label>
              <input className={`w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 ${
                  submitted && phoneError ? "border-red-400 focus:ring-red-400" : "border-gray-200 focus:ring-primary-500"
                }`}
                value={form.phone} onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))} />
              {submitted && phoneError && <p className="text-xs text-red-500 mt-1">{phoneError}</p>}
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Address</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.address} onChange={(e) => setForm((f) => ({ ...f, address: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Trade License</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.tradeLicense} onChange={(e) => setForm((f) => ({ ...f, tradeLicense: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">TIN</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.tin} onChange={(e) => setForm((f) => ({ ...f, tin: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">BIN</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.bin} onChange={(e) => setForm((f) => ({ ...f, bin: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Credit Limit</label>
              <input type="number" min="0" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.creditLimit} onChange={(e) => setForm((f) => ({ ...f, creditLimit: parseFloat(e.target.value) || 0 }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payment Terms (Days)</label>
              <input type="number" min="0" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.paymentTermDays} onChange={(e) => setForm((f) => ({ ...f, paymentTermDays: parseInt(e.target.value) || 0 }))} />
            </div>
            {editing && (
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Status</label>
                <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                  value={form.status} onChange={(e) => setForm((f) => ({ ...f, status: e.target.value as VendorStatus }))}>
                  <option value="Active">Active</option>
                  <option value="Inactive">Inactive</option>
                  <option value="Blacklisted">Blacklisted</option>
                </select>
              </div>
            )}
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button
              onClick={() => { setSubmitted(true); if (isFormValid) saveMutation.mutate(); }}
              disabled={saveMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}