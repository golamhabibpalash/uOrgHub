import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import ExportMenu from "../../components/shared/ExportMenu";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import {
  getAttributeDefinitions,
  createAttributeDefinition,
  updateAttributeDefinition,
  deleteAttributeDefinition,
  getInventoryCategories,
  AttributeDefinition,
  AttributeDataType,
} from "../../api/inventory";

const DATA_TYPES: AttributeDataType[] = ["Text", "Number", "Boolean", "List"];

const dataTypeColors: Record<AttributeDataType, string> = {
  Text: "bg-blue-50 text-blue-700",
  Number: "bg-purple-50 text-purple-700",
  Boolean: "bg-orange-50 text-orange-700",
  List: "bg-teal-50 text-teal-700",
};

export default function AttributeDefinitions() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<AttributeDefinition | null>(null);
  const [form, setForm] = useState({ name: "", dataType: "Text" as AttributeDataType, categoryId: "", isRequired: false, predefinedValues: "", isActive: true });

  const { data, isLoading } = useQuery({
    queryKey: ["attribute-definitions", page, search],
    queryFn: () => getAttributeDefinitions({ page, pageSize: 10, search }),
  });

  const { data: catsData } = useQuery({
    queryKey: ["inventory-categories-all"],
    queryFn: () => getInventoryCategories({ page: 1, pageSize: 200 }),
  });

  const attributes = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const allCategories = catsData?.data?.data?.items ?? [];

  const saveMutation = useMutation({
    mutationFn: () => editing
      ? updateAttributeDefinition(editing.id, { ...form, categoryId: form.categoryId || undefined })
      : createAttributeDefinition({ ...form, categoryId: form.categoryId || undefined }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["attribute-definitions"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteAttributeDefinition(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["attribute-definitions"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ name: "", dataType: "Text", categoryId: "", isRequired: false, predefinedValues: "", isActive: true });
    setModal(true);
  }

  function openEdit(a: AttributeDefinition) {
    setEditing(a);
    setForm({ name: a.name, dataType: a.dataType, categoryId: a.categoryId ?? "", isRequired: a.isRequired, predefinedValues: a.predefinedValues ?? "", isActive: a.isActive });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  const columns = [
    { key: "name", label: "Attribute Name" },
    {
      key: "dataType", label: "Data Type", sortable: false,
      render: (row: AttributeDefinition) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${dataTypeColors[row.dataType]}`}>{row.dataType}</span>
      ),
    },
    { key: "categoryName", label: "Category", render: (row: AttributeDefinition) => <span>{row.categoryName ?? "Global"}</span> },
    {
      key: "isRequired", label: "Required", sortable: false,
      render: (row: AttributeDefinition) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${row.isRequired ? "bg-amber-50 text-amber-700" : "bg-gray-50 text-gray-500"}`}>
          {row.isRequired ? "Required" : "Optional"}
        </span>
      ),
    },
    {
      key: "predefinedValues", label: "Values",
      render: (row: AttributeDefinition) => row.predefinedValues
        ? <span className="text-xs text-gray-500 truncate max-w-32 block">{row.predefinedValues}</span>
        : <span className="text-gray-300">—</span>,
    },
    {
      key: "isActive", label: "Status", sortable: false,
      render: (row: AttributeDefinition) => (
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
          <h2 className="text-base font-medium text-gray-900">Attribute Definitions</h2>
          <p className="text-xs text-gray-400">Define item variant attributes</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Attribute
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
          <input
            type="text" placeholder="Search attributes..." value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
          <div className="ml-auto"><ExportMenu baseUrl="/attributedefinitions" filters={{ search: search || undefined }} /></div>
        </div>
        <DataTable columns={columns} data={attributes} loading={isLoading} onEdit={openEdit} onDelete={(row) => deleteMutation.mutate(row.id)} />
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editing ? "Edit Attribute" : "Add Attribute Definition"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Attribute Name *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Data Type *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.dataType} onChange={(e) => setForm((f) => ({ ...f, dataType: e.target.value as AttributeDataType }))}>
                {DATA_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Category (leave blank for global)</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.categoryId} onChange={(e) => setForm((f) => ({ ...f, categoryId: e.target.value }))}>
              <option value="">Global (all categories)</option>
              {allCategories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          {form.dataType === "List" && (
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Predefined Values (comma-separated)</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" placeholder="e.g. Red,Blue,Green" value={form.predefinedValues} onChange={(e) => setForm((f) => ({ ...f, predefinedValues: e.target.value }))} />
            </div>
          )}
          <div className="flex items-center gap-2">
            <input type="checkbox" id="isRequired" checked={form.isRequired} onChange={(e) => setForm((f) => ({ ...f, isRequired: e.target.checked }))} />
            <label htmlFor="isRequired" className="text-xs text-gray-600">Required field</label>
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
