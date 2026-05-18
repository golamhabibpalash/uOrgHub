import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ProjectNav from "../../components/projects/ProjectNav";
import {
  getMaterialRequests,
  getMaterialRequestItems,
  createMaterialRequest,
  submitMaterialRequest,
  approveMaterialRequest,
  createMaterialRequestItem,
  MaterialRequest,
} from "../../api/projects";

export default function MaterialRequestPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [page] = useState(1);
  const [modal, setModal] = useState(false);
  const [itemModal, setItemModal] = useState(false);
  const [viewModal, setViewModal] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState<MaterialRequest | null>(null);
  const [requestForm, setRequestForm] = useState({
    requestDate: "",
    requiredDate: "",
    notes: "",
  });
  const [itemForm, setItemForm] = useState({
    itemVariantId: "",
    itemVariantName: "",
    quantity: 0,
    uom: "",
    notes: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["materialRequests", projectId, page],
    queryFn: () => getMaterialRequests(projectId!, { page, pageSize: 10 }),
    enabled: !!projectId,
  });

  const { data: itemsData } = useQuery({
    queryKey: ["materialRequestItems", projectId, selectedRequest?.id],
    queryFn: () => getMaterialRequestItems(projectId!, selectedRequest!.id),
    enabled: !!selectedRequest,
  });

  const requests = data?.data?.data?.items ?? [];
  const items = itemsData?.data?.data ?? [];

  const createMutation = useMutation({
    mutationFn: () => createMaterialRequest(projectId!, requestForm),
    onSuccess: (res) => {
      qc.invalidateQueries({ queryKey: ["materialRequests", projectId] });
      qc.invalidateQueries({ queryKey: ["projectDashboard"] });
      setSelectedRequest(res.data?.data);
      closeModal();
      openItemModal();
    },
  });

  const submitMutation = useMutation({
    mutationFn: (requestId: string) => submitMaterialRequest(projectId!, requestId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["materialRequests", projectId] }),
  });

  const approveMutation = useMutation({
    mutationFn: (requestId: string) => approveMaterialRequest(projectId!, requestId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["materialRequests", projectId] }),
  });

  const addItemMutation = useMutation({
    mutationFn: () =>
      createMaterialRequestItem(projectId!, selectedRequest!.id, itemForm),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["materialRequestItems", projectId, selectedRequest?.id] });
      closeItemModal();
    },
  });

  function openModal() {
    setRequestForm({
      requestDate: new Date().toISOString().split("T")[0],
      requiredDate: "",
      notes: "",
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setRequestForm({ requestDate: "", requiredDate: "", notes: "" });
  }

  function openItemModal() {
    setItemForm({
      itemVariantId: "",
      itemVariantName: "",
      quantity: 0,
      uom: "",
      notes: "",
    });
    setItemModal(true);
  }

  function closeItemModal() {
    setItemModal(false);
  }

  function openView(request: MaterialRequest) {
    setSelectedRequest(request);
    setViewModal(true);
  }

  function closeView() {
    setViewModal(false);
    setSelectedRequest(null);
  }

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      Draft: "bg-gray-100 text-gray-600",
      Submitted: "bg-blue-50 text-blue-700",
      Approved: "bg-green-50 text-green-700",
      Rejected: "bg-red-50 text-red-700",
    };
    return styles[status] || "bg-gray-100 text-gray-600";
  };

  return (
    <div>
      <div className="mb-4">
        <Link
          to={`/projects/${projectId}`}
          className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700"
        >
          <ArrowLeft size={16} /> Back to Project
        </Link>
      </div>

      <ProjectNav />

      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Material Requests</h2>
          <p className="text-xs text-gray-400">Manage material procurement requests</p>
        </div>
        <button
          onClick={openModal}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Create Request
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : requests.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No requests found</div>
        ) : (
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="bg-gray-50">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Request No
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Date
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Required Date
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Items
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Status
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {requests.map((req) => (
                <tr key={req.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-2.5 text-gray-700 font-medium">
                    {req.requestNumber}
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">
                    {new Date(req.requestDate).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">
                    {new Date(req.requiredDate).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">{req.itemsCount} items</td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(req.status)}`}>
                      {req.status}
                    </span>
                  </td>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => openView(req)}
                        className="text-xs text-primary-600 hover:underline"
                      >
                        View
                      </button>
                      {req.status === "Draft" && (
                        <button
                          onClick={() => submitMutation.mutate(req.id)}
                          className="text-xs text-blue-600"
                        >
                          Submit
                        </button>
                      )}
                      {req.status === "Submitted" && (
                        <button
                          onClick={() => approveMutation.mutate(req.id)}
                          className="text-xs text-green-600"
                        >
                          Approve
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal title="Create Material Request" open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Request Date *</label>
              <input
                type="date"
                value={requestForm.requestDate}
                onChange={(e) =>
                  setRequestForm((f) => ({ ...f, requestDate: e.target.value }))
                }
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Required Date *</label>
              <input
                type="date"
                value={requestForm.requiredDate}
                onChange={(e) =>
                  setRequestForm((f) => ({ ...f, requiredDate: e.target.value }))
                }
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea
              value={requestForm.notes}
              onChange={(e) => setRequestForm((f) => ({ ...f, notes: e.target.value }))}
              rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={closeModal}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => createMutation.mutate()}
              disabled={createMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {createMutation.isPending ? "Creating..." : "Create"}
            </button>
          </div>
        </div>
      </Modal>

      <Modal title="Add Item" open={itemModal} onClose={closeItemModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Item Variant</label>
            <input
              value={itemForm.itemVariantName}
              onChange={(e) =>
                setItemForm((f) => ({
                  ...f,
                  itemVariantName: e.target.value,
                  itemVariantId: e.target.value,
                }))
              }
              placeholder="Search items..."
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Quantity *</label>
              <input
                type="number"
                value={itemForm.quantity}
                onChange={(e) =>
                  setItemForm((f) => ({ ...f, quantity: parseFloat(e.target.value) || 0 }))
                }
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">UOM *</label>
              <input
                value={itemForm.uom}
                onChange={(e) => setItemForm((f) => ({ ...f, uom: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <input
              value={itemForm.notes}
              onChange={(e) => setItemForm((f) => ({ ...f, notes: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={() => {
                closeItemModal();
                closeView();
              }}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Done
            </button>
            <button
              onClick={() => addItemMutation.mutate()}
              disabled={addItemMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {addItemMutation.isPending ? "Adding..." : "Add Item"}
            </button>
          </div>
        </div>
      </Modal>

      <Modal title="Material Request Details" open={viewModal} onClose={closeView}>
        {selectedRequest && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-gray-500">Request Number</p>
                <p className="text-sm font-medium text-gray-900">
                  {selectedRequest.requestNumber}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Status</p>
                <span className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(selectedRequest.status)}`}>
                  {selectedRequest.status}
                </span>
              </div>
              <div>
                <p className="text-xs text-gray-500">Request Date</p>
                <p className="text-sm text-gray-900">
                  {new Date(selectedRequest.requestDate).toLocaleDateString()}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Required Date</p>
                <p className="text-sm text-gray-900">
                  {new Date(selectedRequest.requiredDate).toLocaleDateString()}
                </p>
              </div>
            </div>
            {selectedRequest.notes && (
              <div>
                <p className="text-xs text-gray-500 mb-1">Notes</p>
                <p className="text-sm text-gray-700">{selectedRequest.notes}</p>
              </div>
            )}
            <div>
              <p className="text-xs text-gray-500 mb-2">Items</p>
              {items.length === 0 ? (
                <p className="text-sm text-gray-400">No items added</p>
              ) : (
                <table className="w-full text-sm border-collapse">
                  <thead>
                    <tr className="bg-gray-50">
                      <th className="text-left px-3 py-2 text-xs font-medium text-gray-500">
                        Item
                      </th>
                      <th className="text-right px-3 py-2 text-xs font-medium text-gray-500">
                        Qty
                      </th>
                      <th className="text-left px-3 py-2 text-xs font-medium text-gray-500">
                        UOM
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {items.map((item) => (
                      <tr key={item.id} className="border-t border-gray-100">
                        <td className="px-3 py-2 text-gray-700">
                          {item.itemVariantName || "-"}
                        </td>
                        <td className="px-3 py-2 text-right text-gray-700">
                          {item.quantity}
                        </td>
                        <td className="px-3 py-2 text-gray-700">{item.uom}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
            {selectedRequest.status === "Draft" && (
              <button
                onClick={() => submitMutation.mutate(selectedRequest.id)}
                disabled={submitMutation.isPending}
                className="w-full py-2 text-sm bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50"
              >
                {submitMutation.isPending ? "Submitting..." : "Submit Request"}
              </button>
            )}
            {selectedRequest.status === "Submitted" && (
              <button
                onClick={() => approveMutation.mutate(selectedRequest.id)}
                disabled={approveMutation.isPending}
                className="w-full py-2 text-sm bg-green-500 text-white rounded-lg hover:bg-green-600 disabled:opacity-50"
              >
                {approveMutation.isPending ? "Approving..." : "Approve Request"}
              </button>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}