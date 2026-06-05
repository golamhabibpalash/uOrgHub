import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import ExportMenu from "../../components/shared/ExportMenu";
import Modal from "../../components/shared/Modal";
import { useDataGrid } from "../../hooks/useDataGrid";
import {
  getWarehouses, createWarehouse, updateWarehouse, deleteWarehouse, Warehouse,
} from "../../api/inventory";

export default function Warehouses() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "name" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Warehouse | null>(null);
  const [form, setForm] = useState({ name: "", code: "", location: "", contactPerson: "", contactPhone: "", isActive: true });

  const { data, isLoading } = useQuery({
    queryKey: ["warehouses", dg.page, dg.search, dg.sortBy, dg.sortDescending],
    queryFn: () => getWarehouses(dg.queryParams),
  });

  const warehouses = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => editing ? updateWarehouse(editing.id, form) : createWarehouse(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["warehouses"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteWarehouse(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["warehouses"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ name: "", code: "", location: "", contactPerson: "", contactPhone: "", isActive: true });
    setModal(true);
  }

  function openEdit(w: Warehouse) {
    setEditing(w);
    setForm({ name: w.name, code: w.code, location: w.location ?? "", contactPerson: w.contactPerson ?? "", contactPhone: w.contactPhone ?? "", isActive: w.isActive });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  const columns = [
    { key: "code", label: "Code" },
    { key: "name", label: "Warehouse Name" },
    { key: "location", label: "Location", render: (row: Warehouse) => <span>{row.location ?? "—"}</span> },
    { key: "contactPerson", label: "Contact Person", render: (row: Warehouse) => <span>{row.contactPerson ?? "—"}</span> },
    { key: "contactPhone", label: "Phone", render: (row: Warehouse) => <span>{row.contactPhone ?? "—"}</span> },
    {
      key: "isActive", label: "Status", sortable: false,
      render: (row: Warehouse) => (
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
          <h2 className="text-base font-medium text-gray-900">Warehouses</h2>
          <p className="text-xs text-gray-400">Manage storage locations</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Warehouse
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={warehouses}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search warehouses..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => deleteMutation.mutate(row.id)}
        emptyMessage="No warehouses found"
        actions={<ExportMenu baseUrl="/warehouses" filters={{ search: dg.search || undefined }} />}
      />

      <Modal title={editing ? "Edit Warehouse" : "Add Warehouse"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Name *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Code *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.code} onChange={(e) => setForm((f) => ({ ...f, code: e.target.value.toUpperCase() }))} />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Location</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.location} onChange={(e) => setForm((f) => ({ ...f, location: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Contact Person</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.contactPerson} onChange={(e) => setForm((f) => ({ ...f, contactPerson: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Phone</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.contactPhone} onChange={(e) => setForm((f) => ({ ...f, contactPhone: e.target.value }))} />
            </div>
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
