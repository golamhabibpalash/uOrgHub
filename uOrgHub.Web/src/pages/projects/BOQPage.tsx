import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft } from "lucide-react";
import Modal from "../../components/shared/Modal";
import {
  getBOQList,
  getBOQItems,
  createBOQ,
  approveBOQ,
  createBOQItem,
  deleteBOQItem,
  BOQ,
} from "../../api/projects";

export default function BOQPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [modal, setModal] = useState(false);
  const [itemModal, setItemModal] = useState(false);
  const [selectedBOQ, setSelectedBOQ] = useState<BOQ | null>(null);
  const [boqForm, setBoqForm] = useState({ title: "", boqNumber: "" });
  const [itemForm, setItemForm] = useState({
    sequence: 0,
    description: "",
    specification: "",
    uom: "",
    estimatedQty: 0,
    unitRate: 0,
  });

  const { data: boqData } = useQuery({
    queryKey: ["boqs", projectId],
    queryFn: () => getBOQList(projectId!),
    enabled: !!projectId,
  });

  const { data: itemsData } = useQuery({
    queryKey: ["boqItems", projectId, selectedBOQ?.id],
    queryFn: () => getBOQItems(projectId!, selectedBOQ!.id),
    enabled: !!selectedBOQ,
  });

  const boqList = boqData?.data?.data ?? [];
  const items = itemsData?.data?.data ?? [];

  const createBOQMutation = useMutation({
    mutationFn: () => createBOQ(projectId!, boqForm),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["boqs", projectId] });
      closeModal();
    },
  });

  const approveBOQMutation = useMutation({
    mutationFn: (boqId: string) => approveBOQ(projectId!, boqId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["boqs", projectId] }),
  });

  const createItemMutation = useMutation({
    mutationFn: () => createBOQItem(projectId!, selectedBOQ!.id, itemForm),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["boqItems", projectId, selectedBOQ?.id] });
      closeItemModal();
    },
  });

  const deleteItemMutation = useMutation({
    mutationFn: (itemId: string) => deleteBOQItem(projectId!, selectedBOQ!.id, itemId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["boqItems", projectId, selectedBOQ?.id] }),
  });

  function openModal() {
    setBoqForm({ title: "", boqNumber: "" });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setBoqForm({ title: "", boqNumber: "" });
  }

  function openItemModal() {
    setItemForm({
      sequence: items.length + 1,
      description: "",
      specification: "",
      uom: "",
      estimatedQty: 0,
      unitRate: 0,
    });
    setItemModal(true);
  }

  function closeItemModal() {
    setItemModal(false);
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

  const totalEstimated = items.reduce((sum, item) => sum + (item.estimatedAmount || 0), 0);
  const totalActual = items.reduce((sum, item) => sum + (item.actualAmount || 0), 0);

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

      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Bill of Quantities (BOQ)</h2>
          <p className="text-xs text-gray-400">Manage project BOQ items</p>
        </div>
        <button
          onClick={openModal}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Create BOQ
        </button>
      </div>

      <div className="grid grid-cols-3 gap-4 mb-6">
        {boqList.map((boq) => (
          <div
            key={boq.id}
            onClick={() => setSelectedBOQ(boq)}
            className={`bg-white border rounded-xl p-4 cursor-pointer transition-colors ${
              selectedBOQ?.id === boq.id
                ? "border-primary-500 ring-1 ring-primary-500"
                : "border-gray-200 hover:border-gray-300"
            }`}
          >
            <div className="flex items-center justify-between mb-2">
              <span className="font-medium text-gray-900">{boq.boqNumber}</span>
              <span className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(boq.status)}`}>
                {boq.status}
              </span>
            </div>
            <p className="text-sm text-gray-600 mb-2">{boq.title}</p>
            <p className="text-xs text-gray-500">
              Est: BDT {boq.totalEstimatedCost.toLocaleString()} | Act: BDT{" "}
              {boq.totalActualCost.toLocaleString()}
            </p>
          </div>
        ))}
      </div>

      {selectedBOQ && (
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
            <div>
              <h3 className="font-medium text-gray-900">
                {selectedBOQ.boqNumber} - {selectedBOQ.title}
              </h3>
              <p className="text-xs text-gray-500">
                Total Estimated: BDT {totalEstimated.toLocaleString()} | Total Actual: BDT{" "}
                {totalActual.toLocaleString()}
              </p>
            </div>
            <div className="flex gap-2">
              {selectedBOQ.status === "Draft" && (
                <button
                  onClick={() => approveBOQMutation.mutate(selectedBOQ.id)}
                  disabled={approveBOQMutation.isPending}
                  className="px-3 py-1.5 text-sm bg-green-500 text-white rounded-lg hover:bg-green-600 disabled:opacity-50"
                >
                  {approveBOQMutation.isPending ? "Approving..." : "Approve"}
                </button>
              )}
              <button
                onClick={openItemModal}
                className="flex items-center gap-1 px-3 py-1.5 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
              >
                <Plus size={14} /> Add Item
              </button>
            </div>
          </div>

          {items.length === 0 ? (
            <div className="text-center py-10 text-gray-400">No items found</div>
          ) : (
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-gray-50">
                  <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                    Seq
                  </th>
                  <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                    Description
                  </th>
                  <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                    Spec
                  </th>
                  <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                    UOM
                  </th>
                  <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">
                    Est. Qty
                  </th>
                  <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">
                    Unit Rate
                  </th>
                  <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">
                    Est. Amount
                  </th>
                  <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">
                    Actual Qty
                  </th>
                  <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">
                    Actual Amount
                  </th>
                  <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.id} className="border-t border-gray-100 hover:bg-gray-50">
                    <td className="px-4 py-2.5 text-gray-700">{item.sequence}</td>
                    <td className="px-4 py-2.5 text-gray-700">{item.description}</td>
                    <td className="px-4 py-2.5 text-gray-600 text-xs">
                      {item.specification || "-"}
                    </td>
                    <td className="px-4 py-2.5 text-gray-700">{item.uom}</td>
                    <td className="px-4 py-2.5 text-right text-gray-700">
                      {item.estimatedQty.toLocaleString()}
                    </td>
                    <td className="px-4 py-2.5 text-right text-gray-700">
                      {item.unitRate.toLocaleString()}
                    </td>
                    <td className="px-4 py-2.5 text-right font-medium text-gray-900">
                      {item.estimatedAmount.toLocaleString()}
                    </td>
                    <td className="px-4 py-2.5 text-right text-gray-700">
                      {item.actualQty?.toLocaleString() || "-"}
                    </td>
                    <td className="px-4 py-2.5 text-right font-medium text-gray-900">
                      {item.actualAmount?.toLocaleString() || "-"}
                    </td>
                    <td className="px-4 py-2.5">
                      <button
                        onClick={() => {
                          if (confirm("Delete this item?")) {
                            deleteItemMutation.mutate(item.id);
                          }
                        }}
                        className="text-xs text-red-500 hover:text-red-700"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
                <tr className="border-t border-gray-200 bg-gray-50 font-medium">
                  <td colSpan={6} className="px-4 py-2.5 text-right">
                    Total
                  </td>
                  <td className="px-4 py-2.5 text-right">
                    {totalEstimated.toLocaleString()}
                  </td>
                  <td colSpan={2} />
                  <td />
                </tr>
              </tbody>
            </table>
          )}
        </div>
      )}

      <Modal title="Create BOQ" open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">BOQ Number *</label>
            <input
              value={boqForm.boqNumber}
              onChange={(e) => setBoqForm((f) => ({ ...f, boqNumber: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Title *</label>
            <input
              value={boqForm.title}
              onChange={(e) => setBoqForm((f) => ({ ...f, title: e.target.value }))}
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
              onClick={() => createBOQMutation.mutate()}
              disabled={createBOQMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {createBOQMutation.isPending ? "Creating..." : "Create"}
            </button>
          </div>
        </div>
      </Modal>

      <Modal title="Add BOQ Item" open={itemModal} onClose={closeItemModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Sequence *</label>
              <input
                type="number"
                value={itemForm.sequence}
                onChange={(e) =>
                  setItemForm((f) => ({ ...f, sequence: parseInt(e.target.value) || 0 }))
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
            <label className="text-xs text-gray-500 mb-1 block">Description *</label>
            <input
              value={itemForm.description}
              onChange={(e) => setItemForm((f) => ({ ...f, description: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Specification</label>
            <input
              value={itemForm.specification}
              onChange={(e) => setItemForm((f) => ({ ...f, specification: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Estimated Qty *</label>
              <input
                type="number"
                value={itemForm.estimatedQty}
                onChange={(e) =>
                  setItemForm((f) => ({
                    ...f,
                    estimatedQty: parseFloat(e.target.value) || 0,
                  }))
                }
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Unit Rate *</label>
              <input
                type="number"
                value={itemForm.unitRate}
                onChange={(e) =>
                  setItemForm((f) => ({
                    ...f,
                    unitRate: parseFloat(e.target.value) || 0,
                  }))
                }
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={closeItemModal}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => createItemMutation.mutate()}
              disabled={createItemMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {createItemMutation.isPending ? "Adding..." : "Add Item"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}