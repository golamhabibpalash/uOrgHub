import { Link, useParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, CheckCircle } from "lucide-react";
import Field from "../../components/shared/Field";
import { getGRNById, confirmGRN } from "../../api/procurement";

const statusColors: Record<string, string> = {
  Draft: "bg-gray-50 text-gray-600", Confirmed: "bg-green-50 text-green-700", Cancelled: "bg-red-50 text-red-700",
};

const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD", { year: "numeric", month: "short", day: "numeric" });
const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

export default function GRNDetail() {
  const { id } = useParams<{ id: string }>();
  const qc = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ["grn", id],
    queryFn: () => getGRNById(id!),
    enabled: Boolean(id),
  });
  const grn = data?.data?.data;

  const confirmMutation = useMutation({
    mutationFn: () => confirmGRN(id!),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["grn", id] });
      qc.invalidateQueries({ queryKey: ["grns"] });
    },
  });

  if (isLoading) return <div className="p-6 text-sm text-gray-400">Loading GRN…</div>;
  if (!grn) return <div className="p-6 text-sm text-gray-400">GRN not found.</div>;

  const totalValue = grn.items.reduce((sum, i) => sum + i.receivedQuantity * i.unitCost, 0);

  return (
    <div className="max-w-4xl mx-auto">
      <div className="flex items-center justify-between mb-4">
        <Link to="/procurement/grns" className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={16} /> Back to Goods Received Notes
        </Link>
        {grn.status === "Draft" && (
          <button onClick={() => confirmMutation.mutate()} disabled={confirmMutation.isPending} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50">
            <CheckCircle size={14} /> Confirm
          </button>
        )}
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-5 mb-4">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-gray-900 font-mono">{grn.grnNumber}</h2>
          <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[grn.status]}`}>{grn.status}</span>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <Field label="Purchase Order" value={grn.poNumber} />
          <Field label="Warehouse" value={grn.warehouseName} />
          <Field label="GRN Date" value={dateFmt(grn.grnDate)} />
          <Field label="Received By" value={grn.receivedByName} />
          <Field label="Invoice Number" value={grn.invoiceNumber} />
          <Field label="Invoice Date" value={grn.invoiceDate ? dateFmt(grn.invoiceDate) : undefined} />
          <Field label="Notes" value={grn.notes} />
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <h3 className="text-sm font-medium text-gray-900 px-5 pt-4 pb-2">Items</h3>
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 text-xs text-gray-500">
              <th className="text-left px-4 py-2">Item</th>
              <th className="text-right px-4 py-2">Ordered</th>
              <th className="text-right px-4 py-2">Received</th>
              <th className="text-right px-4 py-2">Rejected</th>
              <th className="text-right px-4 py-2">Accepted</th>
              <th className="text-right px-4 py-2">Unit Cost</th>
              <th className="text-right px-4 py-2">Line Total</th>
            </tr>
          </thead>
          <tbody>
            {grn.items.map((item) => (
              <tr key={item.id} className="border-t border-gray-100">
                <td className="px-4 py-2">
                  <div className="font-medium text-gray-900">{item.variantName}</div>
                  <div className="text-xs text-gray-400 font-mono">{item.variantSKU}</div>
                </td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.orderedQuantity)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.receivedQuantity)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.rejectedQuantity)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.acceptedQuantity)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.unitCost)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.receivedQuantity * item.unitCost)}</td>
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr className="border-t border-gray-200 bg-gray-50 font-semibold">
              <td className="px-4 py-2" colSpan={6}>Total Received Value</td>
              <td className="px-4 py-2 text-right tabular-nums">{fmt(totalValue)}</td>
            </tr>
          </tfoot>
        </table>
      </div>
    </div>
  );
}
