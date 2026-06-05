import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Tag } from "lucide-react";
import ExportMenu from "../../components/shared/ExportMenu";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import {
  getItemVariants, createItemVariant, updateItemVariant, deleteItemVariant,
  getItems, getAttributeDefinitions,
  ItemVariant, AttributeDefinition,
} from "../../api/inventory";

interface AttrRow { attributeDefinitionId: string; value: string }

export default function ItemVariants() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [filterItemId, setFilterItemId] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<ItemVariant | null>(null);
  const [form, setForm] = useState({
    itemId: "", barcode: "", costPrice: 0, sellingPrice: 0, isDefault: false, isActive: true,
  });
  const [attrRows, setAttrRows] = useState<AttrRow[]>([]);

  const { data, isLoading } = useQuery({
    queryKey: ["item-variants", page, search, filterItemId],
    queryFn: () => getItemVariants({ page, pageSize: 10, search }, filterItemId || undefined),
  });

  const { data: itemsData } = useQuery({
    queryKey: ["items-all"],
    queryFn: () => getItems({ page: 1, pageSize: 200 }),
  });

  const { data: attrsData } = useQuery({
    queryKey: ["attribute-definitions-all"],
    queryFn: () => getAttributeDefinitions({ page: 1, pageSize: 200 }),
  });

  const variants = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const allItems = itemsData?.data?.data?.items ?? [];
  const allAttrs: AttributeDefinition[] = attrsData?.data?.data?.items ?? [];

  const saveMutation = useMutation({
    mutationFn: () => editing
      ? updateItemVariant(editing.id, { ...form, attributes: attrRows })
      : createItemVariant({ ...form, attributes: attrRows }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["item-variants"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteItemVariant(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["item-variants"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ itemId: allItems[0]?.id ?? "", barcode: "", costPrice: 0, sellingPrice: 0, isDefault: false, isActive: true });
    setAttrRows([]);
    setModal(true);
  }

  function openEdit(v: ItemVariant) {
    setEditing(v);
    setForm({ itemId: v.itemId, barcode: v.barcode ?? "", costPrice: v.costPrice, sellingPrice: v.sellingPrice, isDefault: v.isDefault, isActive: v.isActive });
    setAttrRows(v.attributes.map((a) => ({ attributeDefinitionId: a.attributeDefinitionId, value: a.value })));
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setAttrRows([]); }

  function addAttrRow() { setAttrRows((r) => [...r, { attributeDefinitionId: allAttrs[0]?.id ?? "", value: "" }]); }
  function removeAttrRow(i: number) { setAttrRows((r) => r.filter((_, idx) => idx !== i)); }
  function updateAttrRow(i: number, field: keyof AttrRow, value: string) {
    setAttrRows((r) => r.map((row, idx) => idx === i ? { ...row, [field]: value } : row));
  }

  function getPredefinedValues(attrId: string): string[] {
    const def = allAttrs.find((a) => a.id === attrId);
    if (def?.dataType === "List" && def.predefinedValues) return def.predefinedValues.split(",").map((v) => v.trim());
    return [];
  }

  const columns = [
    { key: "sku", label: "SKU", render: (row: ItemVariant) => <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.sku}</span> },
    { key: "itemBaseName", label: "Item" },
    { key: "variantName", label: "Variant Name" },
    {
      key: "attributes", label: "Attributes",
      render: (row: ItemVariant) => (
        <div className="flex flex-wrap gap-1">
          {row.attributes.slice(0, 3).map((a) => (
            <span key={a.id} className="text-xs bg-blue-50 text-blue-700 px-1.5 py-0.5 rounded">
              {a.attributeName}: {a.value}
            </span>
          ))}
          {row.attributes.length > 3 && <span className="text-xs text-gray-400">+{row.attributes.length - 3}</span>}
        </div>
      ),
    },
    { key: "costPrice", label: "Cost", render: (row: ItemVariant) => <span>${row.costPrice.toFixed(2)}</span> },
    { key: "sellingPrice", label: "Price", render: (row: ItemVariant) => <span>${row.sellingPrice.toFixed(2)}</span> },
    {
      key: "isDefault", label: "Default",
      render: (row: ItemVariant) => row.isDefault
        ? <span className="text-xs bg-amber-50 text-amber-700 px-2 py-0.5 rounded-full">Default</span>
        : null,
    },
    {
      key: "isActive", label: "Status",
      render: (row: ItemVariant) => (
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
          <h2 className="text-base font-medium text-gray-900">Item Variants</h2>
          <p className="text-xs text-gray-400">Manage item variants with attributes (SKU auto-generated)</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Variant
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex gap-3 flex-wrap items-center justify-between">
          <input
            type="text" placeholder="Search variants..." value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-48 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
          <select value={filterItemId} onChange={(e) => { setFilterItemId(e.target.value); setPage(1); }} className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
            <option value="">All Items</option>
            {allItems.map((i) => <option key={i.id} value={i.id}>{i.baseName}</option>)}
          </select>
          <div className="ml-auto"><ExportMenu baseUrl="/itemvariants" filters={{ search: search || undefined, itemId: filterItemId || undefined }} /></div>
        </div>
        <DataTable columns={columns} data={variants} loading={isLoading} onEdit={openEdit} onDelete={(row) => deleteMutation.mutate(row.id)} />
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editing ? "Edit Variant" : "Add Item Variant"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Item *</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.itemId} onChange={(e) => setForm((f) => ({ ...f, itemId: e.target.value }))} disabled={!!editing}>
              <option value="">Select item...</option>
              {allItems.map((i) => <option key={i.id} value={i.id}>{i.baseName} ({i.itemCode})</option>)}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Barcode</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm font-mono focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.barcode} onChange={(e) => setForm((f) => ({ ...f, barcode: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Cost Price</label>
              <input type="number" min="0" step="0.01" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.costPrice} onChange={(e) => setForm((f) => ({ ...f, costPrice: parseFloat(e.target.value) || 0 }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Selling Price</label>
              <input type="number" min="0" step="0.01" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.sellingPrice} onChange={(e) => setForm((f) => ({ ...f, sellingPrice: parseFloat(e.target.value) || 0 }))} />
            </div>
          </div>
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isDefault" checked={form.isDefault} onChange={(e) => setForm((f) => ({ ...f, isDefault: e.target.checked }))} />
              <label htmlFor="isDefault" className="text-xs text-gray-600">Default variant</label>
            </div>
            {editing && (
              <div className="flex items-center gap-2">
                <input type="checkbox" id="isActive" checked={form.isActive} onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))} />
                <label htmlFor="isActive" className="text-xs text-gray-600">Active</label>
              </div>
            )}
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs text-gray-500 font-medium">Attributes</label>
              <button type="button" onClick={addAttrRow} className="flex items-center gap-1 text-xs text-primary-600 hover:text-primary-700">
                <Tag size={12} /> Add Attribute
              </button>
            </div>
            {attrRows.length === 0 && <p className="text-xs text-gray-400 italic">No attributes added</p>}
            <div className="space-y-2">
              {attrRows.map((row, i) => {
                const predefined = getPredefinedValues(row.attributeDefinitionId);
                const attrDef = allAttrs.find((a) => a.id === row.attributeDefinitionId);
                return (
                  <div key={i} className="flex gap-2 items-center">
                    <select
                      className="flex-1 border border-gray-200 rounded-lg px-2 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-primary-500"
                      value={row.attributeDefinitionId}
                      onChange={(e) => updateAttrRow(i, "attributeDefinitionId", e.target.value)}
                    >
                      {allAttrs.map((a) => <option key={a.id} value={a.id}>{a.name}</option>)}
                    </select>
                    {attrDef?.dataType === "Boolean" ? (
                      <select className="flex-1 border border-gray-200 rounded-lg px-2 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-primary-500" value={row.value} onChange={(e) => updateAttrRow(i, "value", e.target.value)}>
                        <option value="true">Yes</option>
                        <option value="false">No</option>
                      </select>
                    ) : predefined.length > 0 ? (
                      <select className="flex-1 border border-gray-200 rounded-lg px-2 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-primary-500" value={row.value} onChange={(e) => updateAttrRow(i, "value", e.target.value)}>
                        <option value="">Select...</option>
                        {predefined.map((v) => <option key={v} value={v}>{v}</option>)}
                      </select>
                    ) : (
                      <input
                        className="flex-1 border border-gray-200 rounded-lg px-2 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-primary-500"
                        placeholder="Value"
                        value={row.value}
                        onChange={(e) => updateAttrRow(i, "value", e.target.value)}
                      />
                    )}
                    <button type="button" onClick={() => removeAttrRow(i)} className="text-red-400 hover:text-red-600"><Trash2 size={13} /></button>
                  </div>
                );
              })}
            </div>
          </div>

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
