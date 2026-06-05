import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getItems, createItem, updateItem, deleteItem,
  getInventoryTypes, getInventoryCategories, getUnitsOfMeasure,
  Item,
} from "../../api/inventory";

export default function Items() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [filterCatId, setFilterCatId] = useState("");
  const [filterTypeId, setFilterTypeId] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Item | null>(null);
  const [form, setForm] = useState({
    baseName: "", typeId: "", categoryId: "", unitOfMeasureId: "",
    brand: "", manufacturer: "", description: "", reorderLevel: 0, standardCost: 0, isActive: true,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["items", page, search, filterCatId, filterTypeId],
    queryFn: () => getItems({ page, pageSize: 10, search }, filterCatId || undefined, filterTypeId || undefined),
  });

  const { data: typesData } = useQuery({ queryKey: ["inventory-types", 1, ""], queryFn: () => getInventoryTypes({ page: 1, pageSize: 100 }) });
  const { data: catsData } = useQuery({ queryKey: ["inventory-categories-all"], queryFn: () => getInventoryCategories({ page: 1, pageSize: 200 }) });
  const { data: uomsData } = useQuery({ queryKey: ["units-of-measure", 1, ""], queryFn: () => getUnitsOfMeasure({ page: 1, pageSize: 100 }) });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const allTypes = typesData?.data?.data?.items ?? [];
  const allCategories = catsData?.data?.data?.items ?? [];
  const allUoMs = uomsData?.data?.data?.items ?? [];

  const saveMutation = useMutation({
    mutationFn: () => editing ? updateItem(editing.id, form) : createItem(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["items"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteItem(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["items"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ baseName: "", typeId: allTypes[0]?.id ?? "", categoryId: allCategories[0]?.id ?? "", unitOfMeasureId: allUoMs[0]?.id ?? "", brand: "", manufacturer: "", description: "", reorderLevel: 0, standardCost: 0, isActive: true });
    setModal(true);
  }

  function openEdit(item: Item) {
    setEditing(item);
    setForm({ baseName: item.baseName, typeId: item.typeId, categoryId: item.categoryId, unitOfMeasureId: item.unitOfMeasureId, brand: item.brand ?? "", manufacturer: item.manufacturer ?? "", description: item.description ?? "", reorderLevel: item.reorderLevel, standardCost: item.standardCost, isActive: item.isActive });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  const columns = [
    { key: "itemCode", label: "Item Code", render: (row: Item) => <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.itemCode ?? "—"}</span> },
    { key: "baseName", label: "Item Name" },
    { key: "typeName", label: "Type" },
    { key: "categoryName", label: "Category" },
    { key: "unitAbbreviation", label: "UoM", render: (row: Item) => <span className="font-mono text-xs">{row.unitAbbreviation}</span> },
    { key: "standardCost", label: "Std. Cost", render: (row: Item) => <span>${row.standardCost.toFixed(2)}</span> },
    { key: "variantCount", label: "Variants", render: (row: Item) => <span className="text-xs bg-blue-50 text-blue-700 px-2 py-0.5 rounded-full">{row.variantCount}</span> },
    {
      key: "isActive", label: "Status",
      render: (row: Item) => (
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
          <h2 className="text-base font-medium text-gray-900">Items</h2>
          <p className="text-xs text-gray-400">Manage inventory items</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Item
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex gap-3 flex-wrap">
          <input
            type="text" placeholder="Search items..." value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-48 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
          <select value={filterTypeId} onChange={(e) => { setFilterTypeId(e.target.value); setPage(1); }} className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
            <option value="">All Types</option>
            {allTypes.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
          </select>
          <select value={filterCatId} onChange={(e) => { setFilterCatId(e.target.value); setPage(1); }} className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
            <option value="">All Categories</option>
            {allCategories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          <div className="ml-auto">
            <ExportMenu baseUrl="items" filters={{ search: search || undefined, categoryId: filterCatId || undefined, typeId: filterTypeId || undefined }} />
          </div>
        </div>
        <DataTable columns={columns} data={items} loading={isLoading} onEdit={openEdit} onDelete={(row) => deleteMutation.mutate(row.id)} />
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editing ? "Edit Item" : "Add Item"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Item Name *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.baseName} onChange={(e) => setForm((f) => ({ ...f, baseName: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Type *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.typeId} onChange={(e) => setForm((f) => ({ ...f, typeId: e.target.value }))}>
                <option value="">Select type...</option>
                {allTypes.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Category *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.categoryId} onChange={(e) => setForm((f) => ({ ...f, categoryId: e.target.value }))}>
                <option value="">Select category...</option>
                {allCategories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Unit of Measure *</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.unitOfMeasureId} onChange={(e) => setForm((f) => ({ ...f, unitOfMeasureId: e.target.value }))}>
              <option value="">Select unit...</option>
              {allUoMs.map((u) => <option key={u.id} value={u.id}>{u.name} ({u.abbreviation})</option>)}
            </select>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Brand</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.brand} onChange={(e) => setForm((f) => ({ ...f, brand: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Manufacturer</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.manufacturer} onChange={(e) => setForm((f) => ({ ...f, manufacturer: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Reorder Level</label>
              <input type="number" min="0" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.reorderLevel} onChange={(e) => setForm((f) => ({ ...f, reorderLevel: parseFloat(e.target.value) || 0 }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Standard Cost</label>
              <input type="number" min="0" step="0.01" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.standardCost} onChange={(e) => setForm((f) => ({ ...f, standardCost: parseFloat(e.target.value) || 0 }))} />
            </div>
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
