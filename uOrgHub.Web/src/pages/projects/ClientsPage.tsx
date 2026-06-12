import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import ExportMenu from "../../components/shared/ExportMenu";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import { getClients, createClient, updateClient, deleteClient, Client } from "../../api/projects";

export default function ClientsPage() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "companyName" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Client | null>(null);
  const [form, setForm] = useState({
    companyName: "",
    contactPerson: "",
    email: "",
    phone: "",
    address: "",
    clientType: "Private",
    status: "Active",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["clients", ...dg.queryKey],
    queryFn: () => getClients(dg.queryParams),
  });

  const clients = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () =>
      editing ? updateClient(editing.id, form) : createClient(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["clients"] });
      closeModal();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteClient(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["clients"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({
      companyName: "",
      contactPerson: "",
      email: "",
      phone: "",
      address: "",
      clientType: "Private",
      status: "Active",
    });
    setModal(true);
  }

  function openEdit(client: Client) {
    setEditing(client);
    setForm({
      companyName: client.companyName,
      contactPerson: client.contactPerson,
      email: client.email,
      phone: client.phone,
      address: client.address || "",
      clientType: client.clientType,
      status: client.status,
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  const getTypeBadge = (clientType: string) => {
    const styles: Record<string, string> = {
      Government: "bg-purple-50 text-purple-700",
      Private: "bg-blue-50 text-blue-700",
      NGO: "bg-green-50 text-green-700",
      International: "bg-amber-50 text-amber-700",
    };
    return styles[clientType] || "bg-gray-100 text-gray-600";
  };

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      Active: "bg-green-50 text-green-700",
      Inactive: "bg-red-50 text-red-700",
    };
    return styles[status] || "bg-gray-100 text-gray-600";
  };

  const columns = [
    { key: "clientCode", label: "Code" },
    { key: "companyName", label: "Company Name" },
    { key: "contactPerson", label: "Contact Person" },
    {
      key: "clientType",
      label: "Type",
      sortable: false,
      render: (row: Client) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${getTypeBadge(row.clientType)}`}>
          {row.clientType}
        </span>
      ),
    },
    {
      key: "status",
      label: "Status",
      sortable: false,
      render: (row: Client) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(row.status)}`}>
          {row.status}
        </span>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-medium text-gray-900">Clients</h2>
          <p className="text-sm text-gray-500">Manage project clients</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add Client
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={clients}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search clients..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => {
          if (confirm("Delete this client?")) {
            deleteMutation.mutate(row.id);
          }
        }}
        emptyMessage="No clients found"
        actions={<ExportMenu baseUrl="/clients" filters={{ search: dg.search || undefined }} />}
      />

      <Modal
        title={editing ? "Edit Client" : "Add Client"}
        open={modal}
        onClose={closeModal}
      >
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Client Code</label>
              <div className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm bg-gray-50 text-gray-500">
                {editing ? editing.clientCode : "Auto-generated on save"}
              </div>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Company Name *</label>
              <input
                value={form.companyName}
                onChange={(e) => setForm((f) => ({ ...f, companyName: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Contact Person *</label>
            <input
              value={form.contactPerson}
              onChange={(e) => setForm((f) => ({ ...f, contactPerson: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Email</label>
              <input
                type="email"
                value={form.email}
                onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Phone</label>
              <input
                value={form.phone}
                onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Address</label>
            <textarea
              value={form.address}
              onChange={(e) => setForm((f) => ({ ...f, address: e.target.value }))}
              rows={2}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Type</label>
              <select
                value={form.clientType}
                onChange={(e) => setForm((f) => ({ ...f, clientType: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              >
                <option value="Government">Government</option>
                <option value="Private">Private</option>
                <option value="NGO">NGO</option>
                <option value="International">International</option>
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Status</label>
              <select
                value={form.status}
                onChange={(e) => setForm((f) => ({ ...f, status: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={closeModal}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => saveMutation.mutate()}
              disabled={saveMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}