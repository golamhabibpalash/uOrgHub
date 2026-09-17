import { Link, useParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Send, CheckCircle, XCircle } from "lucide-react";
import Field from "../../components/shared/Field";
import { getPurchaseOrderById, sendPO, confirmPO, cancelPO } from "../../api/procurement";

const statusColors: Record<string, string> = {
  Draft: "bg-gray-50 text-gray-600", Sent: "bg-blue-50 text-blue-700",
  Confirmed: "bg-green-50 text-green-700", PartiallyReceived: "bg-yellow-50 text-yellow-700",
  FullyReceived: "bg-green-100 text-green-800", Cancelled: "bg-red-50 text-red-700",
};

const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD", { year: "numeric", month: "short", day: "numeric" });
const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

export default function PurchaseOrderDetail() {
  const { id } = useParams<{ id: string }>();
  const qc = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ["purchase-order", id],
    queryFn: () => getPurchaseOrderById(id!),
    enabled: Boolean(id),
  });
  const po = data?.data?.data;

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["purchase-order", id] });
    qc.invalidateQueries({ queryKey: ["purchase-orders"] });
  };
  const sendMutation = useMutation({ mutationFn: () => sendPO(id!), onSuccess: invalidate });
  const confirmMutation = useMutation({ mutationFn: () => confirmPO(id!), onSuccess: invalidate });
  const cancelMutation = useMutation({ mutationFn: () => cancelPO(id!), onSuccess: invalidate });

  if (isLoading) return <div className="p-6 text-sm text-gray-400">Loading purchase order…</div>;
  if (!po) return <div className="p-6 text-sm text-gray-400">Purchase order not found.</div>;

  return (
    <div className="max-w-4xl mx-auto">
      <div className="flex items-center justify-between mb-4">
        <Link to="/procurement/purchase-orders" className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={16} /> Back to Purchase Orders
        </Link>
        <div className="flex items-center gap-2">
          {po.status === "Draft" && (
            <button onClick={() => sendMutation.mutate()} disabled={sendMutation.isPending} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              <Send size={14} /> Send
            </button>
          )}
          {po.status === "Sent" && (
            <>
              <button onClick={() => confirmMutation.mutate()} disabled={confirmMutation.isPending} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50">
                <CheckCircle size={14} /> Confirm
              </button>
              <button onClick={() => cancelMutation.mutate()} disabled={cancelMutation.isPending} className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-red-200 text-red-600 rounded-lg hover:bg-red-50 disabled:opacity-50">
                <XCircle size={14} /> Cancel
              </button>
            </>
          )}
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-5 mb-4">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-gray-900 font-mono">{po.poNumber}</h2>
          <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[po.status]}`}>{po.status}</span>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <Field label="Vendor" value={po.vendorName} />
          <Field label="PO Date" value={dateFmt(po.poDate)} />
          <Field label="Expected Delivery" value={dateFmt(po.expectedDeliveryDate)} />
          <Field label="Quotation Ref" value={po.quotationNumber} />
          <Field label="PR Ref" value={po.prNumber} />
          <Field label="Payment Terms" value={po.paymentTerms} />
          <Field label="Delivery Address" value={po.deliveryAddress} />
          <Field label="Notes" value={po.notes} />
          {po.approvedByName && (
            <>
              <Field label="Approved By" value={po.approvedByName} />
              <Field label="Approved At" value={po.approvedAt ? dateFmt(po.approvedAt) : undefined} />
            </>
          )}
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden mb-4">
        <h3 className="text-sm font-medium text-gray-900 px-5 pt-4 pb-2">Items</h3>
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 text-xs text-gray-500">
              <th className="text-left px-4 py-2">Item</th>
              <th className="text-right px-4 py-2">Ordered</th>
              <th className="text-right px-4 py-2">Received</th>
              <th className="text-right px-4 py-2">Unit Price</th>
              <th className="text-right px-4 py-2">Tax %</th>
              <th className="text-right px-4 py-2">Discount %</th>
              <th className="text-right px-4 py-2">Total</th>
            </tr>
          </thead>
          <tbody>
            {po.items.map((item) => (
              <tr key={item.id} className="border-t border-gray-100">
                <td className="px-4 py-2">
                  <div className="font-medium text-gray-900">{item.variantName}</div>
                  <div className="text-xs text-gray-400 font-mono">{item.variantSKU}</div>
                </td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.orderedQuantity)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.receivedQuantity)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.unitPrice)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{item.taxPercent}%</td>
                <td className="px-4 py-2 text-right tabular-nums">{item.discountPercent}%</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.totalPrice)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-5">
        <div className="flex justify-end">
          <div className="w-64 space-y-1.5 text-sm">
            <div className="flex justify-between text-gray-500"><span>Subtotal</span><span className="tabular-nums">{fmt(po.subTotal)}</span></div>
            <div className="flex justify-between text-gray-500"><span>Tax</span><span className="tabular-nums">{fmt(po.taxAmount)}</span></div>
            <div className="flex justify-between text-gray-500"><span>Discount</span><span className="tabular-nums">-{fmt(po.discountAmount)}</span></div>
            <div className="flex justify-between font-semibold text-gray-900 pt-1.5 border-t border-gray-200"><span>Total</span><span className="tabular-nums">{fmt(po.totalAmount)}</span></div>
          </div>
        </div>
      </div>
    </div>
  );
}
