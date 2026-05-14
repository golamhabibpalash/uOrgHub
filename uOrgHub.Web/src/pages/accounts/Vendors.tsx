import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import {
  getVendors,
  createVendor,
  updateVendor,
  deleteVendor,
  getChartOfAccounts,
  Vendor,
} from "../../api/accounts";

export default function Vendors() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Vendor | null>(null);
  const [form, setForm] = useState({
    vendorCode: "",
    name: "",
    contactPerson: "",
    email: "",
    phone: "",
    address: "",
    tin: "",
    bin: "",
    paymentTermsDays: 30,
    payableAccountId: "",
    isActive: true,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["vendors", page, search],
    queryFn: () => getVendors({ page, pageSize: 10, search }),
  });

  const { data: accountsData } = useQuery({
    queryKey: ["chart-of-accounts", 1, ""],
    queryFn: () => getChartOfAccounts({ page: 1, pageSize: 200 }),
  });

  const vendors = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const coaAccounts = accountsData?.data?.data?.items ?? [];

  const saveMutation = useMutation({
    mutationFn: () => editing ? updateVendor(editing.id, form) : createVendor(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["vendors"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteVendor(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["vendors"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ vendorCode: "", name: "", contactPerson: "", email: "", phone: "", address: "", tin: "", bin: "", paymentTermsDays: 30, payableAccountId: coaAccounts[0]?.id ?? "", isActive: true });
    setModal(true);
  }

  function openEdit(v: Vendor) {
    setEditing(v);
    setForm({ vendorCode: v.vendorCode, name: v.name, contactPerson: v.contactPerson ?? "", email: v.email ?? "", phone: v.phone ?? "", address: v.address ?? "", tin: v.tin ?? "", bin: v.bin ?? "", paymentTermsDays: v.paymentTermsDays, payableAccountId: v.payableAccountId, isActive: v.isActive });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  const columns = [
    { key: "vendorCode", label: "Code" },
    { key: "name", label: "Vendor Name" },
    { key: "contactPerson", label: "Contact Person" },
    { key: "phone", label: "Phone" },
    { key: "email", label: "Email" },
    {
      key: "paymentTermsDays",
      label: "Payment Terms",
      render: (row: Vendor) => <span>{row.paymentTermsDays} days</span>,
    },
    {
      key: "isActive",
      label: "Status",
      render: (row: Vendor) => (
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
          <h2 className="text-base font-medium text-gray-900">Vendors</h2>
          <p className="text-xs text-gray-400">Manage accounts payable vendors/suppliers</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Vendor
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100">
          <input
            type="text"
            placeholder="Search vendors..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
        </div>
        <DataTable columns={columns} data={vendors} loading={isLoading} onEdit={openEdit} onDelete={(row) => deleteMutation.mutate(row.id)} />
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editing ? "Edit Vendor" : "Add Vendor"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Vendor Code *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.vendorCode} onChange={(e) => setForm((f) => ({ ...f, vendorCode: e.target.value }))} disabled={!!editing} />
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
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Payment Terms (Days)</label>
            <input type="number" min={0} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.paymentTermsDays} onChange={(e) => setForm((f) => ({ ...f, paymentTermsDays: parseInt(e.target.value) || 0 }))} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Payable Account *</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.payableAccountId} onChange={(e) => setForm((f) => ({ ...f, payableAccountId: e.target.value }))}>
              <option value="">Select account</option>
              {coaAccounts.map((a) => <option key={a.id} value={a.id}>{a.accountCode} — {a.accountName}</option>)}
            </select>
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
