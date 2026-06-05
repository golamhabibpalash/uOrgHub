import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import ExportMenu from "../../components/shared/ExportMenu";
import Modal from "../../components/shared/Modal";
import { useDataGrid } from "../../hooks/useDataGrid";
import {
  getUnitsOfMeasure,
  createUnitOfMeasure,
  updateUnitOfMeasure,
  deleteUnitOfMeasure,
  UnitOfMeasure,
} from "../../api/inventory";

export default function UnitsOfMeasure() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "name" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<UnitOfMeasure | null>(null);
  const [form, setForm] = useState({ name: "", abbreviation: "", isActive: true });

  const { data, isLoading } = useQuery({
    queryKey: ["units-of-measure", dg.page, dg.search, dg.sortBy, dg.sortDescending],
    queryFn: () => getUnitsOfMeasure(dg.queryParams),
  });

  const units = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => editing ? updateUnitOfMeasure(editing.id, form) : createUnitOfMeasure(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["units-of-measure"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteUnitOfMeasure(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["units-of-measure"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ name: "", abbreviation: "", isActive: true });
    setModal(true);
  }

  function openEdit(u: UnitOfMeasure) {
    setEditing(u);
    setForm({ name: u.name, abbreviation: u.abbreviation, isActive: u.isActive });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  const columns = [
    { key: "name", label: "Unit Name" },
    {
      key: "abbreviation", label: "Abbreviation",
      render: (row: UnitOfMeasure) => (
        <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.abbreviation}</span>
      ),
    },
    {
      key: "isActive", label: "Status", sortable: false,
      render: (row: UnitOfMeasure) => (
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
          <h2 className="text-base font-medium text-gray-900">Units of Measure</h2>
          <p className="text-xs text-gray-400">Define measurement units for items</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Unit
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={units}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search units..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => deleteMutation.mutate(row.id)}
        emptyMessage="No units of measure found"
        actions={<ExportMenu baseUrl="/unitsofmeasure" filters={{ search: dg.search || undefined }} />}
      />

      <Modal title={editing ? "Edit Unit of Measure" : "Add Unit of Measure"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Unit Name *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Abbreviation *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm font-mono focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.abbreviation} onChange={(e) => setForm((f) => ({ ...f, abbreviation: e.target.value.toUpperCase() }))} />
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
