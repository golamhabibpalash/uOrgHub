import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Search } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import { getVendors, createVendor, updateVendor, deleteVendor, Vendor, VendorType, VendorStatus } from "../../api/procurement";

export default function Vendors() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [filterType, setFilterType] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Vendor | null>(null);
  const [form, setForm] = useState({
    companyName: "", contactPerson: "", email: "", phone: "",
    address: "", tradeLicense: "", tin: "", bin: "",
    vendorType: "Supplier" as VendorType, status: "Active" as VendorStatus,
    creditLimit: 0, paymentTermDays: 30, notes: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["vendors", page, search, filterStatus, filterType],
    queryFn: () => getVendors({ page, pageSize: 10, search },
      filterStatus ? filterStatus as VendorStatus : undefined,
      filterType ? filterType as VendorType : undefined),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;

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
    { key: "vendorType", label: "Type", render: (row: Vendor) => <span className="text-xs bg-blue-50 text-blue-700 px-2 py-0.5 rounded">{typeLabels[row.vendorType]}</span> },
    { key: "status", label: "Status", render: (row: Vendor) => <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span> },
    { key: "creditLimit", label: "Credit Limit", render: (row: Vendor) => <span>${row.creditLimit.toLocaleString()}</span> },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Vendors</h2>
          <p className="text-xs text-gray-400">Manage vendor database</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Vendor
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex gap-3 flex-wrap">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input type="text" placeholder="Search vendors..." value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1); }}
              className="pl-8 text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-48 focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
            <option value="">All Status</option>
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
            <option value="Blacklisted">Blacklisted</option>
          </select>
          <select value={filterType} onChange={(e) => { setFilterType(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
            <option value="">All Types</option>
            <option value="Supplier">Supplier</option>
            <option value="Contractor">Contractor</option>
            <option value="Consultant">Consultant</option>
            <option value="ServiceProvider">Service Provider</option>
          </select>
        </div>
        <DataTable columns={columns} data={items} loading={isLoading} onEdit={openEdit} onDelete={(row) => deleteMutation.mutate(row.id)} />
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editing ? "Edit Vendor" : "Add Vendor"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Company Name *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.companyName} onChange={(e) => setForm((f) => ({ ...f, companyName: e.target.value }))} />
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
              <label className="text-xs text-gray-500 mb-1 block">Phone</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.phone} onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))} />
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
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}