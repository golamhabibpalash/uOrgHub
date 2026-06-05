import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import ExportMenu from "../../components/shared/ExportMenu";
import Modal from "../../components/shared/Modal";
import { useDataGrid } from "../../hooks/useDataGrid";
import {
  getInventoryCategories,
  createInventoryCategory,
  updateInventoryCategory,
  deleteInventoryCategory,
  getInventoryTypes,
  InventoryCategory,
} from "../../api/inventory";

export default function InventoryCategories() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "name" });
  const [filterTypeId, setFilterTypeId] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<InventoryCategory | null>(null);
  const [form, setForm] = useState({ name: "", code: "", typeId: "", parentCategoryId: "", description: "", isActive: true });

  const { data, isLoading } = useQuery({
    queryKey: ["inventory-categories", dg.page, dg.search, dg.sortBy, dg.sortDescending, filterTypeId],
    queryFn: () => getInventoryCategories(dg.queryParams, filterTypeId || undefined),
  });

  const { data: typesData } = useQuery({
    queryKey: ["inventory-types", 1, ""],
    queryFn: () => getInventoryTypes({ page: 1, pageSize: 100 }),
  });

  const { data: allCatsData } = useQuery({
    queryKey: ["inventory-categories-all"],
    queryFn: () => getInventoryCategories({ page: 1, pageSize: 200 }),
  });

  const categories = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;
  const allTypes = typesData?.data?.data?.items ?? [];
  const allCategories = allCatsData?.data?.data?.items ?? [];

  const saveMutation = useMutation({
    mutationFn: () => editing
      ? updateInventoryCategory(editing.id, { ...form, parentCategoryId: form.parentCategoryId || undefined })
      : createInventoryCategory({ ...form, parentCategoryId: form.parentCategoryId || undefined }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["inventory-categories"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteInventoryCategory(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["inventory-categories"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ name: "", code: "", typeId: allTypes[0]?.id ?? "", parentCategoryId: "", description: "", isActive: true });
    setModal(true);
  }

  function openEdit(c: InventoryCategory) {
    setEditing(c);
    setForm({ name: c.name, code: c.code, typeId: c.typeId, parentCategoryId: c.parentCategoryId ?? "", description: c.description ?? "", isActive: c.isActive });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  const columns = [
    { key: "code", label: "Code" },
    { key: "name", label: "Category Name" },
    { key: "typeName", label: "Type" },
    {
      key: "parentCategoryName", label: "Parent",
      render: (row: InventoryCategory) => <span className="text-gray-500">{row.parentCategoryName ?? "—"}</span>,
    },
    {
      key: "isActive", label: "Status", sortable: false,
      render: (row: InventoryCategory) => (
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
          <h2 className="text-base font-medium text-gray-900">Inventory Categories</h2>
          <p className="text-xs text-gray-400">Organize items into categories</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Category
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={categories}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search categories..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => deleteMutation.mutate(row.id)}
        emptyMessage="No categories found"
        toolbarPrefix={
          <select
            value={filterTypeId}
            onChange={(e) => { setFilterTypeId(e.target.value); dg.setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
          >
            <option value="">All Types</option>
            {allTypes.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
          </select>
        }
        actions={<ExportMenu baseUrl="/inventorycategories" filters={{ search: dg.search || undefined, typeId: filterTypeId || undefined }} />}
      />

      <Modal title={editing ? "Edit Category" : "Add Category"} open={modal} onClose={closeModal}>
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
            <label className="text-xs text-gray-500 mb-1 block">Inventory Type *</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.typeId} onChange={(e) => setForm((f) => ({ ...f, typeId: e.target.value }))}>
              <option value="">Select type...</option>
              {allTypes.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Parent Category</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.parentCategoryId} onChange={(e) => setForm((f) => ({ ...f, parentCategoryId: e.target.value }))}>
              <option value="">None (Top Level)</option>
              {allCategories.filter((c) => c.id !== editing?.id).map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
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
