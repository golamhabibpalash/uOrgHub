import { useState, useMemo, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ChevronRight, ChevronDown, FolderOpen, Folder, Search, Hash } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getAllAccountGroups,
  getGeneratedCode,
  createAccountGroup,
  updateAccountGroup,
  deleteAccountGroup,
  AccountGroup,
  AccountGroupType,
} from "../../api/accounts";

const GROUP_TYPES: AccountGroupType[] = ["Asset", "Liability", "Equity", "Income", "Expense"];

const typeColors: Record<AccountGroupType, string> = {
  Asset: "bg-blue-50 text-blue-700",
  Liability: "bg-red-50 text-red-700",
  Equity: "bg-purple-50 text-purple-700",
  Income: "bg-green-50 text-green-700",
  Expense: "bg-orange-50 text-orange-700",
};

interface TreeNode extends AccountGroup {
  children: TreeNode[];
}

function buildTree(groups: AccountGroup[]): TreeNode[] {
  const map = new Map<string, TreeNode>();
  const roots: TreeNode[] = [];

  for (const g of groups) {
    map.set(g.id, { ...g, children: [] });
  }
  for (const g of groups) {
    const node = map.get(g.id)!;
    if (g.parentAccountGroupId && map.has(g.parentAccountGroupId)) {
      map.get(g.parentAccountGroupId)!.children.push(node);
    } else {
      roots.push(node);
    }
  }
  return roots;
}

function filterTree(nodes: TreeNode[], term: string): TreeNode[] {
  if (!term) return nodes;
  const lower = term.toLowerCase();
  return nodes
    .map((node) => {
      const children = filterTree(node.children, term);
      const match = node.name.toLowerCase().includes(lower) || node.code.toLowerCase().includes(lower);
      if (match || children.length > 0) {
        return { ...node, children };
      }
      return null;
    })
    .filter(Boolean) as TreeNode[];
}

export default function AccountGroups() {
  const qc = useQueryClient();
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<AccountGroup | null>(null);
  const [form, setForm] = useState({ name: "", type: "Asset" as AccountGroupType, description: "", isActive: true, parentAccountGroupId: "", customCode: "" });
  const [generatedCode, setGeneratedCode] = useState("");
  const [saveError, setSaveError] = useState("");
  const [expanded, setExpanded] = useState<Set<string>>(new Set());

  const { data: allData, isLoading } = useQuery({
    queryKey: ["account-groups-all"],
    queryFn: getAllAccountGroups,
  });

  const allGroups = useMemo(() => allData?.data?.data ?? [], [allData]);

  const tree = useMemo(() => {
    const raw = buildTree(allGroups);
    const filtered = filterTree(raw, search);
    return filtered;
  }, [allGroups, search]);

  const parentOptions = useMemo(() => {
    const excludeId = editing?.id;
    return allGroups
      .filter((g) => !excludeId || g.id !== excludeId)
      .map((g) => ({ id: g.id, label: `${g.code} - ${g.name}` }));
  }, [allGroups, editing]);

  useEffect(() => {
    if (editing) return;
    const parentId = form.parentAccountGroupId || undefined;
    const timeout = setTimeout(() => {
      getGeneratedCode(form.type, parentId).then((res) => {
        setGeneratedCode(res.data?.data?.code ?? "");
      }).catch(() => {});
    }, 100);
    return () => clearTimeout(timeout);
  }, [form.type, form.parentAccountGroupId, editing]);

  const saveMutation = useMutation({
    mutationFn: () => {
      const payload: Record<string, unknown> = {
        name: form.name,
        type: form.type,
        description: form.description || undefined,
        isActive: form.isActive,
        customCode: form.customCode || undefined,
        parentAccountGroupId: form.parentAccountGroupId || undefined,
      };
      if (editing) {
        payload.id = editing.id;
        payload.code = editing.code;
        return updateAccountGroup(editing.id, payload);
      }
      return createAccountGroup(payload);
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["account-groups-all"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save account group.";
      setSaveError(msg);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteAccountGroup(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["account-groups-all"] }),
  });

  function openAdd(parentId?: string) {
    setEditing(null);
    setGeneratedCode("");
    setForm({ name: "", type: "Asset", description: "", isActive: true, parentAccountGroupId: parentId ?? "", customCode: "" });
    setSaveError("");
    setModal(true);
  }

  function openEdit(g: AccountGroup) {
    setEditing(g);
    setGeneratedCode(g.code);
    setForm({
      name: g.name,
      type: g.type,
      description: g.description ?? "",
      isActive: g.isActive,
      parentAccountGroupId: g.parentAccountGroupId ?? "",
      customCode: g.customCode ?? "",
    });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setSaveError(""); }

  const toggleExpand = (id: string) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  };

  function renderTreeNodes(nodes: TreeNode[], depth: number) {
    return nodes.map((node) => {
      const isExpanded = expanded.has(node.id);
      const hasChildren = node.children.length > 0;

      return (
        <div key={node.id}>
          <div
            className="flex items-center gap-2 px-4 py-2.5 hover:bg-gray-50 border-b border-gray-100 transition-colors group"
            style={{ paddingLeft: `${16 + depth * 24}px` }}
          >
            <div className="flex items-center gap-1 min-w-0 flex-1">
              {hasChildren ? (
                <button onClick={() => toggleExpand(node.id)} className="p-0.5 hover:bg-gray-100 rounded shrink-0">
                  {isExpanded ? <ChevronDown size={14} className="text-gray-500" /> : <ChevronRight size={14} className="text-gray-500" />}
                </button>
              ) : (
                <span className="w-5 shrink-0" />
              )}
              {hasChildren ? <FolderOpen size={15} className="text-amber-500 shrink-0" /> : <Folder size={15} className="text-gray-400 shrink-0" />}
              <span className="text-sm font-medium text-gray-900 truncate ml-1">{node.name}</span>
              <span className="text-xs text-gray-400 ml-2 shrink-0">({node.code})</span>
              {node.customCode && <span className="text-xs text-gray-400 ml-1 shrink-0">[{node.customCode}]</span>}
            </div>
            <span className={`text-xs px-2 py-0.5 rounded-full shrink-0 ${typeColors[node.type]}`}>{node.type}</span>
            <span className={`text-xs px-2 py-0.5 rounded-full shrink-0 ${node.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>
              {node.isActive ? "Active" : "Inactive"}
            </span>
            <div className="flex items-center gap-1 shrink-0 opacity-0 group-hover:opacity-100 transition-opacity">
              <button
                onClick={() => openAdd(node.id)}
                className="p-1 hover:bg-gray-100 rounded text-gray-400 hover:text-primary-600"
                title="Add sub-group"
              >
                <Plus size={13} />
              </button>
              <button
                onClick={() => openEdit(node)}
                className="p-1 hover:bg-gray-100 rounded text-gray-400 hover:text-gray-600"
                title="Edit"
              >
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z"/><path d="m15 5 4 4"/></svg>
              </button>
              <button
                onClick={() => { if (confirm("Delete this group?")) deleteMutation.mutate(node.id); }}
                className="p-1 hover:bg-gray-100 rounded text-gray-400 hover:text-red-600"
                title="Delete"
              >
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/></svg>
              </button>
            </div>
          </div>
          {hasChildren && isExpanded && renderTreeNodes(node.children, depth + 1)}
        </div>
      );
    });
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Account Groups</h2>
          <p className="text-xs text-gray-400">Hierarchical chart of accounts structure</p>
        </div>
        <button onClick={() => openAdd()} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Group
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Search groups..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="text-sm border border-gray-200 rounded-lg pl-8 pr-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <ExportMenu baseUrl="/accounts/account-groups" />
        </div>

        {isLoading ? (
          <div className="p-8 text-center text-sm text-gray-400">Loading...</div>
        ) : tree.length === 0 ? (
          <div className="p-8 text-center text-sm text-gray-400">No account groups found. Click "Add Group" to create one.</div>
        ) : (
          <div>{renderTreeNodes(tree, 0)}</div>
        )}
      </div>

      <Modal title={editing ? "Edit Account Group" : "Add Account Group"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Group Name *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Type *</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.type} onChange={(e) => setForm((f) => ({ ...f, type: e.target.value as AccountGroupType }))}>
              {GROUP_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Parent Group</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.parentAccountGroupId} onChange={(e) => setForm((f) => ({ ...f, parentAccountGroupId: e.target.value }))}>
              <option value="">-- None (Root Level) --</option>
              {parentOptions.map((opt) => (
                <option key={opt.id} value={opt.id}>{opt.label}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">System Generated Code</label>
            <div className="flex items-center gap-2 px-3 py-2 bg-gray-50 border border-gray-200 rounded-lg text-sm text-gray-600">
              <Hash size={14} className="text-gray-400" />
              <span className="font-mono font-medium">{generatedCode || "—"}</span>
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Custom Code <span className="text-gray-400">(optional)</span></label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" placeholder="e.g. AST-CURR-001" value={form.customCode} onChange={(e) => setForm((f) => ({ ...f, customCode: e.target.value }))} />
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