import { Link, useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { ArrowLeft, FileText } from "lucide-react";
import Field from "../../components/shared/Field";
import { getRFQById, getRFQQuotations } from "../../api/procurement";

const statusColors: Record<string, string> = {
  Draft: "bg-gray-50 text-gray-600",
  Sent: "bg-blue-50 text-blue-700",
  Closed: "bg-green-50 text-green-700",
  Cancelled: "bg-red-50 text-red-700",
};

const quotationStatusColors: Record<string, string> = {
  Received: "bg-blue-50 text-blue-700", Evaluated: "bg-yellow-50 text-yellow-700",
  Accepted: "bg-green-50 text-green-700", Rejected: "bg-red-50 text-red-700",
};

const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD", { year: "numeric", month: "short", day: "numeric" });
const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

export default function RfqDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data, isLoading } = useQuery({
    queryKey: ["rfq", id],
    queryFn: () => getRFQById(id!),
    enabled: Boolean(id),
  });
  const rfq = data?.data?.data;

  const { data: quotationsData } = useQuery({
    queryKey: ["rfq-quotations", id],
    queryFn: () => getRFQQuotations(id!, { page: 1, pageSize: 50 }),
    enabled: Boolean(id),
  });
  const quotations = quotationsData?.data?.data?.items ?? [];

  if (isLoading) return <div className="p-6 text-sm text-gray-400">Loading RFQ…</div>;
  if (!rfq) return <div className="p-6 text-sm text-gray-400">RFQ not found.</div>;

  return (
    <div className="max-w-4xl mx-auto">
      <div className="flex items-center justify-between mb-4">
        <Link to="/procurement/rfqs" className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={16} /> Back to RFQs
        </Link>
        <button
          onClick={() => navigate(`/procurement/rfqs/${id}/document`)}
          className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600"
        >
          <FileText size={14} /> Application Document
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-5 mb-4">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-gray-900 font-mono">{rfq.rfqNumber}</h2>
          <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[rfq.status]}`}>{rfq.status}</span>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <Field label="Title" value={rfq.title} />
          <Field label="PR Ref" value={rfq.prNumber} />
          <Field label="RFQ Date" value={dateFmt(rfq.rfqDate)} />
          <Field label="Closing Date" value={dateFmt(rfq.closingDate)} />
          <Field label="Description" value={rfq.description} />
          <Field label="Notes" value={rfq.notes} />
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden mb-4">
        <h3 className="text-sm font-medium text-gray-900 px-5 pt-4 pb-2">Items</h3>
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 text-xs text-gray-500">
              <th className="text-left px-4 py-2">Item</th>
              <th className="text-right px-4 py-2">Quantity</th>
              <th className="text-left px-4 py-2">Notes</th>
            </tr>
          </thead>
          <tbody>
            {rfq.items.map((item) => (
              <tr key={item.id} className="border-t border-gray-100">
                <td className="px-4 py-2">
                  <div className="font-medium text-gray-900">{item.variantName}</div>
                  <div className="text-xs text-gray-400 font-mono">{item.variantSKU}</div>
                </td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(item.requestedQuantity)}</td>
                <td className="px-4 py-2 text-gray-500">{item.notes || "—"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <h3 className="text-sm font-medium text-gray-900 px-5 pt-4 pb-2">Quotations Received</h3>
        {quotations.length === 0 ? (
          <p className="text-sm text-gray-400 px-5 pb-4">No vendor quotations received yet.</p>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-xs text-gray-500">
                <th className="text-left px-4 py-2">Quote #</th>
                <th className="text-left px-4 py-2">Vendor</th>
                <th className="text-left px-4 py-2">Quote Date</th>
                <th className="text-left px-4 py-2">Valid Until</th>
                <th className="text-right px-4 py-2">Total</th>
                <th className="text-left px-4 py-2">Status</th>
              </tr>
            </thead>
            <tbody>
              {quotations.map((q) => (
                <tr key={q.id} className="border-t border-gray-100">
                  <td className="px-4 py-2 font-mono text-xs">{q.quotationNumber}</td>
                  <td className="px-4 py-2">{q.vendorName}</td>
                  <td className="px-4 py-2">{dateFmt(q.quotationDate)}</td>
                  <td className="px-4 py-2">{dateFmt(q.validUntil)}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{fmt(q.totalAmount)}</td>
                  <td className="px-4 py-2">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${quotationStatusColors[q.status]}`}>{q.status}</span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
