import { useRef, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Pencil, Printer, FileDown, RotateCcw } from "lucide-react";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import ProcurementDocumentView from "../../components/procurement/ProcurementDocumentView";
import { getPRDocument, savePRDocument, regeneratePRDocument } from "../../api/procurement";
import { useAuthStore } from "../../store/authStore";
import { useReportPdf } from "../../hooks/useReportPdf";
import { printDocument } from "../../utils/printDocument";
import { extractApiError } from "../../utils/apiError";

const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD", { year: "numeric", month: "short", day: "numeric" });
const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

export default function PurchaseRequisitionDocument() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const printRef = useRef<HTMLDivElement>(null);

  const [editing, setEditing] = useState(false);
  const [draftText, setDraftText] = useState("");
  const [regenerateOpen, setRegenerateOpen] = useState(false);
  const [error, setError] = useState("");

  const { hasClaim } = useAuthStore();
  const canEdit = hasClaim("Procurement.PurchaseRequisitions.Edit");
  const { downloadPdf, isDownloading } = useReportPdf();

  const { data, isLoading } = useQuery({
    queryKey: ["pr-document", id],
    queryFn: () => getPRDocument(id!),
    enabled: Boolean(id),
  });
  const doc = data?.data?.data;

  const saveMutation = useMutation({
    mutationFn: () => savePRDocument(id!, draftText),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["pr-document", id] });
      setEditing(false);
    },
    onError: (err: unknown) => setError(extractApiError(err)),
  });

  const regenerateMutation = useMutation({
    mutationFn: () => regeneratePRDocument(id!),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["pr-document", id] });
      setRegenerateOpen(false);
    },
    onError: (err: unknown) => setError(extractApiError(err)),
  });

  function startEdit() {
    setDraftText(doc?.documentText ?? "");
    setEditing(true);
  }

  if (isLoading) return <div className="p-6 text-sm text-gray-400">Loading application…</div>;
  if (!doc) return <div className="p-6 text-sm text-gray-400">Purchase requisition not found.</div>;

  return (
    <div className="max-w-4xl mx-auto">
      <div className="flex items-center justify-between mb-4 no-print">
        <button onClick={() => navigate("/procurement/purchase-requisitions")} className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={15} /> Back to Purchase Requisitions
        </button>
        <div className="flex items-center gap-2">
          {doc.isDocumentEdited && (
            <span className="text-xs text-gray-400">
              Edited {doc.documentEditedAt ? dateFmt(doc.documentEditedAt) : ""}
            </span>
          )}
          {canEdit && !editing && (
            <button onClick={startEdit} className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600">
              <Pencil size={14} /> Edit Application
            </button>
          )}
          {canEdit && (
            <button onClick={() => setRegenerateOpen(true)} className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600">
              <RotateCcw size={14} /> Regenerate
            </button>
          )}
          <button
            onClick={() => printRef.current && printDocument(`Purchase Requisition ${doc.prNumber}`, printRef.current)}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600"
          >
            <Printer size={14} /> Print
          </button>
          <button
            onClick={() => downloadPdf({ url: `/purchaserequisitions/${id}/pdf`, filename: `PurchaseRequisition_${doc.prNumber}.pdf` })}
            disabled={isDownloading}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600 disabled:opacity-50"
          >
            <FileDown size={14} /> {isDownloading ? "Preparing..." : "Export PDF"}
          </button>
        </div>
      </div>

      {error && <div className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2 no-print">{error}</div>}

      {editing ? (
        <div className="border border-gray-200 rounded-xl p-4 bg-white">
          <label className="text-xs text-gray-500 mb-1 block">Application Text</label>
          <textarea
            rows={16}
            className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500 font-mono"
            value={draftText}
            onChange={(e) => setDraftText(e.target.value)}
          />
          <p className="text-xs text-gray-400 mt-1">Separate paragraphs with a blank line. The structured requisition data (items, dates, department) is unaffected by this text.</p>
          <div className="flex justify-end gap-2 mt-3">
            <button onClick={() => setEditing(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button
              onClick={() => saveMutation.mutate()}
              disabled={saveMutation.isPending || !draftText.trim()}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      ) : (
        <div className="border border-gray-200 rounded-xl overflow-hidden bg-gray-50">
          <div ref={printRef} className="p-8">
            <ProcurementDocumentView
              company={doc.company}
              title="PURCHASE REQUISITION"
              referenceLine={`Ref. No. ${doc.prNumber}    |    Date: ${dateFmt(doc.prDate)}`}
              metaFields={[
                { label: "Requesting Department", value: doc.departmentName },
                { label: "Date of Requisition", value: dateFmt(doc.prDate) },
                { label: "Requested By", value: doc.requestedByName },
                { label: "Required Date", value: dateFmt(doc.requiredDate) },
              ]}
              documentText={doc.documentText ?? ""}
              items={doc.items}
              showPricing
              totalLabel="TOTAL"
              totalValue={fmt(doc.totalEstimatedCost)}
              remarks={doc.notes}
              signatures={[
                { label: "Prepared By", name: doc.requestedByName },
                { label: "Approved By", name: doc.approvedByName },
              ]}
            />
          </div>
        </div>
      )}

      <ConfirmDialog
        open={regenerateOpen}
        title="Regenerate application text?"
        message="This rebuilds the application text from the requisition's current data and discards any manual edits made to it. This cannot be undone."
        confirmLabel="Regenerate"
        tone="danger"
        loading={regenerateMutation.isPending}
        onConfirm={() => regenerateMutation.mutate()}
        onCancel={() => setRegenerateOpen(false)}
      />
    </div>
  );
}
