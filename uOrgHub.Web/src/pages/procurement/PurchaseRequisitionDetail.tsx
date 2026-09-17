import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Send, CheckCircle, XCircle, FileText } from "lucide-react";
import Field from "../../components/shared/Field";
import Modal from "../../components/shared/Modal";
import { getPurchaseRequisitionById, submitPR, approvePR, rejectPR } from "../../api/procurement";

const statusColors: Record<string, string> = {
  Draft: "bg-gray-50 text-gray-600",
  Submitted: "bg-blue-50 text-blue-700",
  Approved: "bg-green-50 text-green-700",
  Rejected: "bg-red-50 text-red-700",
  Converted: "bg-purple-50 text-purple-700",
};

const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD", { year: "numeric", month: "short", day: "numeric" });
const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

export default function PurchaseRequisitionDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();

  const [rejectOpen, setRejectOpen] = useState(false);
  const [rejectReason, setRejectReason] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["purchase-requisition", id],
    queryFn: () => getPurchaseRequisitionById(id!),
    enabled: Boolean(id),
  });
  const pr = data?.data?.data;

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["purchase-requisition", id] });
    qc.invalidateQueries({ queryKey: ["purchase-requisitions"] });
  };

  const submitMutation = useMutation({ mutationFn: () => submitPR(id!), onSuccess: invalidate });
  const approveMutation = useMutation({ mutationFn: () => approvePR(id!), onSuccess: invalidate });
  const rejectMutation = useMutation({
    mutationFn: () => rejectPR(id!, rejectReason),
    onSuccess: () => { invalidate(); setRejectOpen(false); setRejectReason(""); },
  });

  if (isLoading) return <div className="p-6 text-sm text-gray-400">Loading purchase requisition…</div>;
  if (!pr) return <div className="p-6 text-sm text-gray-400">Purchase requisition not found.</div>;

  const totalEstimated = pr.items.reduce((sum, i) => sum + i.estimatedTotalCost, 0);

  return (
    <div className="max-w-4xl mx-auto">
      <div className="flex items-center justify-between mb-4">
        <Link to="/procurement/purchase-requisitions" className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={16} /> Back to Purchase Requisitions
        </Link>
        <div className="flex items-center gap-2">
          <button
            onClick={() => navigate(`/procurement/purchase-requisitions/${id}/document`)}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600"
          >
            <FileText size={14} /> Application Document
          </button>
          {pr.status === "Draft" && (
            <button onClick={() => submitMutation.mutate()} disabled={submitMutation.isPending} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              <Send size={14} /> Submit
            </button>
          )}
          {pr.status === "Submitted" && (
            <>
              <button onClick={() => approveMutation.mutate()} disabled={approveMutation.isPending} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50">
                <CheckCircle size={14} /> Approve
              </button>
              <button onClick={() => setRejectOpen(true)} className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-red-200 text-red-600 rounded-lg hover:bg-red-50">
                <XCircle size={14} /> Reject
              </button>
            </>
          )}
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-5 mb-4">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-gray-900 font-mono">{pr.prNumber}</h2>
          <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[pr.status]}`}>{pr.status}</span>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <Field label="Requesting Department" value={pr.departmentName} />
          <Field label="Requested By" value={pr.requestedByName} />
          <Field label="PR Date" value={dateFmt(pr.prDate)} />
          <Field label="Required Date" value={dateFmt(pr.requiredDate)} />
          <Field label="Purpose" value={pr.purpose} />
          <Field label="Notes" value={pr.notes} />
          {pr.status === "Approved" && (
            <>
              <Field label="Approved By" value={pr.approvedByName} />
              <Field label="Approved At" value={pr.approvedAt ? dateFmt(pr.approvedAt) : undefined} />
            </>
          )}
          {pr.status === "Rejected" && <Field label="Rejection Reason" value={pr.rejectionReason} />}
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <h3 className="text-sm font-medium text-gray-900 px-5 pt-4 pb-2">Items</h3>
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 text-xs text-gray-500">
              <th className="text-left px-4 py-2">Item</th>
              <th className="text-left px-4 py-2">Warehouse</th>
              <th className="text-right px-4 py-2">Qty</th>
              <th className="text-right px-4 py-2">Est. Unit Cost</th>
              <th className="text-right px-4 py-2">Est. Total</th>
              <th className="text-left px-4 py-2">Notes</th>
            </tr>
          </thead>
          <tbody>
            {pr.items.map((item) => (
              <tr key={item.id} className="border-t border-gray-100">
                <td className="px-4 py-2">
                  <div className="font-medium text-gray-900">{item.variantName}</div>
                  <div className="text-xs text-gray-400 font-mono">{item.variantSKU}</div>
                </td>
                <td className="px-4 py-2 text-gray-600">{item.warehouseName}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.requestedQuantity)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.estimatedUnitCost)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.estimatedTotalCost)}</td>
                <td className="px-4 py-2 text-gray-500">{item.notes || "—"}</td>
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr className="border-t border-gray-200 bg-gray-50 font-semibold">
              <td className="px-4 py-2" colSpan={4}>Total Estimated Cost</td>
              <td className="px-4 py-2 text-right tabular-nums">{fmt(totalEstimated)}</td>
              <td className="px-4 py-2" />
            </tr>
          </tfoot>
        </table>
      </div>

      <Modal title="Reject Purchase Requisition" open={rejectOpen} onClose={() => setRejectOpen(false)}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Reason for rejection</label>
            <textarea rows={3} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={rejectReason} onChange={(e) => setRejectReason(e.target.value)} placeholder="Enter rejection reason..." />
          </div>
          <div className="flex justify-end gap-2">
            <button onClick={() => setRejectOpen(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => rejectMutation.mutate()} disabled={rejectMutation.isPending || !rejectReason}
              className="px-4 py-2 text-sm bg-red-500 text-white rounded-lg hover:bg-red-600 disabled:opacity-50">
              {rejectMutation.isPending ? "Rejecting..." : "Reject"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
