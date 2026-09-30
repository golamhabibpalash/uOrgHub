import { useCallback, useEffect, useRef, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ArrowLeft,
  Pencil,
  Printer,
  Send,
  CheckCircle2,
  XCircle,
  Ban,
  BookOpen,
  Undo2,
  Info,
} from "lucide-react";
import Modal from "../../components/shared/Modal";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import VoucherPrintView from "../../components/accounts/VoucherPrintView";
import {
  approveVoucher,
  cancelVoucher,
  getVoucherById,
  getVoucherJournalEntry,
  postVoucher,
  rejectVoucher,
  reverseVoucher,
  submitVoucher,
  Voucher,
  VoucherStatus,
} from "../../api/accounts";
import { voucherThemes } from "../../components/accounts/voucherTheme";
import AttachmentManager from "../../components/shared/AttachmentManager";
import { getMyCompany } from "../../api/company";
import { useAuthStore } from "../../store/authStore";
import { amountInWords, formatTaka } from "../../utils/format";
import { extractApiError } from "../../utils/apiError";
import DateInput from "../../components/shared/DateInput";

const statusColors: Record<VoucherStatus, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Submitted: "bg-amber-50 text-amber-700",
  Approved: "bg-blue-50 text-blue-700",
  Posted: "bg-green-50 text-green-700",
  Rejected: "bg-red-50 text-red-700",
  Cancelled: "bg-gray-100 text-gray-400",
  Reversed: "bg-orange-50 text-orange-700",
};

const today = () => new Date().toISOString().split("T")[0];

const workflow: VoucherStatus[] = ["Draft", "Submitted", "Approved", "Posted"];

function Field({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <p className="text-xs text-gray-400">{label}</p>
      <p className="text-sm text-gray-800 mt-0.5">{value ?? "—"}</p>
    </div>
  );
}

export default function VoucherDetail() {
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const printRef = useRef<HTMLDivElement>(null);
  const autoPrinted = useRef(false);

  const [error, setError] = useState("");
  const [rejectOpen, setRejectOpen] = useState(false);
  const [rejectReason, setRejectReason] = useState("");
  const [cancelOpen, setCancelOpen] = useState(false);
  const [journalOpen, setJournalOpen] = useState(false);
  const [correctOpen, setCorrectOpen] = useState(false);
  const [correctReason, setCorrectReason] = useState("");
  const [correctDate, setCorrectDate] = useState(today);

  const { data, isLoading } = useQuery({
    queryKey: ["voucher", id],
    queryFn: () => getVoucherById(id!),
    enabled: Boolean(id),
  });

  const { hasClaim, hasRole } = useAuthStore();
  const canEditAttachments = hasRole("Admin") || hasClaim("Accounts.Vouchers.Edit");
  // Its own permission, deliberately not part of the default Accountant role.
  const canCorrect = hasRole("Admin") || hasClaim("Accounts.Vouchers.Reverse");

  const { data: company } = useQuery({ queryKey: ["my-company"], queryFn: getMyCompany, staleTime: 300000 });

  const voucher: Voucher | undefined = data?.data?.data ?? undefined;

  const { data: journal, isLoading: loadingJournal } = useQuery({
    queryKey: ["voucher-journal-entry", id],
    queryFn: () => getVoucherJournalEntry(id!),
    enabled: journalOpen && Boolean(voucher?.journalEntryId),
  });

  const actionMutation = useMutation({
    mutationFn: (fn: (voucherId: string) => Promise<unknown>) => fn(id!),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["voucher", id] });
      qc.invalidateQueries({ queryKey: ["vouchers"] });
      qc.invalidateQueries({ queryKey: ["voucher-journal-entry", id] });
      qc.invalidateQueries({ queryKey: ["journal-entries"] });
      setRejectOpen(false);
      setCancelOpen(false);
      setRejectReason("");
    },
    onError: (err: unknown) => setError(extractApiError(err)),
  });

  const correctMutation = useMutation({
    mutationFn: () => reverseVoucher(id!, correctReason.trim(), correctDate || undefined),
    onSuccess: (res) => {
      qc.invalidateQueries({ queryKey: ["voucher", id] });
      qc.invalidateQueries({ queryKey: ["vouchers"] });
      qc.invalidateQueries({ queryKey: ["journal-entries"] });
      setCorrectOpen(false);
      const draft = res.data.data;
      // Straight into the corrected draft — fixing it is the whole point of the action.
      if (draft?.id) navigate(`/accounts/vouchers/${draft.id}/edit`);
    },
    onError: (err: unknown) => setError(extractApiError(err)),
  });

  const handlePrint = useCallback(() => {
    const content = printRef.current?.innerHTML;
    if (!content) return;
    const printWindow = window.open("", "_blank");
    if (!printWindow) return;
    printWindow.document.write(`
      <html>
        <head>
          <title>${voucher?.voucherNumber ?? "Voucher"}</title>
          <script src="https://cdn.tailwindcss.com"></script>
          <style>
            @page { margin: 15mm; }
            body { font-family: 'Inter', sans-serif; color: #000; }
            /* Black-ink printing: the voucher document prints in pure black on
               white regardless of the on-screen type theme or status colours. */
            body, body *, body *::before, body *::after {
              color: #000 !important;
              background: transparent !important;
              border-color: #000 !important;
              box-shadow: none !important;
              text-shadow: none !important;
            }
          </style>
        </head>
        <body>${content}<script>window.print();</script></body>
      </html>
    `);
    printWindow.document.close();
  }, [voucher?.voucherNumber]);

  // Support the print shortcut from the voucher list (?print=1).
  useEffect(() => {
    if (voucher && searchParams.get("print") === "1" && !autoPrinted.current) {
      autoPrinted.current = true;
      handlePrint();
    }
  }, [voucher, searchParams, handlePrint]);

  if (isLoading) return <div className="p-6 text-sm text-gray-400">Loading voucher…</div>;
  if (!voucher) return <div className="p-6 text-sm text-gray-400">Voucher not found.</div>;

  const theme = voucherThemes[voucher.voucherType];
  // A Contra voucher moves money between own accounts, so there is no paying or receiving party.
  const nameLabel = { Debit: "Paid To", Credit: "Received From", Contra: "Cheque / Slip No." }[
    voucher.voucherType
  ];
  const debitLabel = { Debit: "Party Account", Credit: "Receive Into", Contra: "Transfer To" }[
    voucher.voucherType
  ];
  const creditLabel = { Debit: "Pay From", Credit: "Party Account", Contra: "Transfer From" }[
    voucher.voucherType
  ];
  const currentStep = workflow.indexOf(voucher.status);
  const isClosed = voucher.status === "Rejected" || voucher.status === "Cancelled" || voucher.status === "Reversed";

  const busy = actionMutation.isPending;

  return (
    <div className="p-6 max-w-5xl">
      {/* Header — colour-coded by voucher type */}
      <div className="flex items-start gap-3 mb-4">
        <button
          onClick={() => navigate(-1)}
          className="w-8 h-8 flex items-center justify-center rounded-lg border border-gray-200 hover:bg-gray-50 text-gray-500"
        >
          <ArrowLeft size={15} />
        </button>
        <div className={`flex-1 relative overflow-hidden border rounded-xl px-5 py-3.5 ${theme.banner}`}>
          <span className={`absolute left-0 top-0 bottom-0 w-1.5 ${theme.bar}`} />
          <div className="flex items-center justify-between gap-3 pl-2">
            <div>
              <div className="flex items-center gap-2">
                <h1 className="text-xl font-bold text-gray-900">{voucher.voucherNumber}</h1>
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium bg-white ${theme.printText}`}>
                  {theme.label}
                </span>
                <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[voucher.status]}`}>
                  {voucher.status}
                </span>
              </div>
              <p className="text-xs text-gray-500 mt-0.5">
                {theme.meaning} · {voucher.voucherDate?.split("T")[0]}
              </p>
            </div>
            <span className={`text-lg font-bold tracking-wider px-3 py-1 rounded-lg border bg-white ${theme.chip}`}>
              {theme.code}
            </span>
          </div>
        </div>
      </div>

      {error && (
        <div className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
          {error}
        </div>
      )}

      {voucher.status === "Reversed" && (
        <div className="flex gap-2 text-sm text-orange-800 bg-orange-50 border border-orange-200 rounded-xl px-4 py-3 mb-4">
          <Undo2 size={16} className="shrink-0 mt-0.5" />
          <div className="space-y-0.5">
            <p>
              <b>Reversed</b> by {voucher.reversedBy} on {voucher.reversedAt?.split("T")[0]}
              {voucher.reversalJournalEntryNumber && <> — reversal entry {voucher.reversalJournalEntryNumber}</>}.
            </p>
            {voucher.reversalReason && <p>Reason: {voucher.reversalReason}</p>}
            {voucher.correctedByVoucherId && (
              <p>
                Corrected by{" "}
                <Link to={`/accounts/vouchers/${voucher.correctedByVoucherId}`} className="font-medium underline">
                  {voucher.correctedByVoucherNumber}
                </Link>
              </p>
            )}
          </div>
        </div>
      )}

      {voucher.correctsVoucherId && (
        <div className="flex gap-2 text-sm text-blue-800 bg-blue-50 border border-blue-200 rounded-xl px-4 py-3 mb-4">
          <Info size={16} className="shrink-0 mt-0.5" />
          <p>
            Correction for{" "}
            <Link to={`/accounts/vouchers/${voucher.correctsVoucherId}`} className="font-medium underline">
              {voucher.correctsVoucherNumber}
            </Link>
            , which was reversed.
          </p>
        </div>
      )}

      {voucher.status === "Posted" && voucher.isPaymentVoucher && canCorrect && (
        <div className="flex gap-2 text-xs text-gray-600 bg-gray-50 border border-gray-200 rounded-xl px-4 py-2.5 mb-4">
          <Info size={14} className="shrink-0 mt-0.5" />
          <p>This voucher was generated by a payment. To correct it, void the payment and record it again.</p>
        </div>
      )}

      {/* Workflow progress */}
      {!isClosed && (
        <div className="bg-white border border-gray-200 rounded-xl px-5 py-4 mb-4">
          <div className="flex items-center">
            {workflow.map((step, i) => (
              <div key={step} className="flex items-center flex-1 last:flex-none">
                <div className="flex flex-col items-center">
                  <div
                    className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-medium ${
                      i <= currentStep ? "bg-primary-500 text-white" : "bg-gray-100 text-gray-400"
                    }`}
                  >
                    {i + 1}
                  </div>
                  <span
                    className={`text-xs mt-1.5 ${i <= currentStep ? "text-gray-700 font-medium" : "text-gray-400"}`}
                  >
                    {step}
                  </span>
                </div>
                {i < workflow.length - 1 && (
                  <div className={`flex-1 h-0.5 mx-2 mb-5 ${i < currentStep ? "bg-primary-500" : "bg-gray-100"}`} />
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Actions */}
      <div className="flex flex-wrap items-center gap-2 mb-4">
        {voucher.status === "Draft" && (
          <>
            <button
              onClick={() => navigate(`/accounts/vouchers/${voucher.id}/edit`)}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600"
            >
              <Pencil size={14} /> Edit
            </button>
            <button
              onClick={() => { setError(""); actionMutation.mutate(submitVoucher); }}
              disabled={busy}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              <Send size={14} /> Submit
            </button>
          </>
        )}

        {voucher.status === "Submitted" && (
          <>
            <button
              onClick={() => { setError(""); actionMutation.mutate(approveVoucher); }}
              disabled={busy}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              <CheckCircle2 size={14} /> Approve
            </button>
            <button
              onClick={() => { setError(""); setRejectOpen(true); }}
              disabled={busy}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm border border-red-200 text-red-600 rounded-lg hover:bg-red-50 disabled:opacity-50"
            >
              <XCircle size={14} /> Reject
            </button>
          </>
        )}

        {voucher.status === "Approved" && (
          <button
            onClick={() => { setError(""); actionMutation.mutate(postVoucher); }}
            disabled={busy}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
          >
            <CheckCircle2 size={14} /> Post to Ledger
          </button>
        )}

        {voucher.status !== "Posted" && voucher.status !== "Cancelled" && voucher.status !== "Reversed" && (
          <button
            onClick={() => { setError(""); setCancelOpen(true); }}
            disabled={busy}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600 disabled:opacity-50"
          >
            <Ban size={14} /> Cancel
          </button>
        )}

        {voucher.status === "Posted" && canCorrect && !voucher.isPaymentVoucher && (
          <button
            onClick={() => {
              setError("");
              setCorrectReason("");
              setCorrectDate(today());
              setCorrectOpen(true);
            }}
            disabled={busy}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm border border-orange-200 text-orange-700 rounded-lg hover:bg-orange-50 disabled:opacity-50"
          >
            <Undo2 size={14} /> Correct this voucher
          </button>
        )}

        <div className="flex-1" />

        {voucher.journalEntryId && (
          <button
            onClick={() => setJournalOpen(true)}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600"
          >
            <BookOpen size={14} /> Journal Entry {voucher.journalEntryNumber}
          </button>
        )}
        <button
          onClick={handlePrint}
          className="flex items-center gap-1.5 px-3 py-1.5 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600"
        >
          <Printer size={14} /> Print
        </button>
      </div>

      {/* Summary */}
      <div className="bg-white border border-gray-200 rounded-xl p-5 mb-4 grid grid-cols-2 md:grid-cols-4 gap-5">
        <Field label={nameLabel} value={voucher.name} />
        <Field label="Voucher Ref. No." value={voucher.referenceNumber} />
        <Field label="Section" value={voucher.section} />
        <Field
          label={voucher.projectId ? "Project" : "Charged To"}
          value={voucher.costCenterName}
        />
        <Field label="Fiscal Year" value={voucher.fiscalYearName} />
        <Field label="Prepared By" value={voucher.preparedBy} />

        <div className="col-span-2">
          <p className="text-xs text-gray-400">{debitLabel} — Debit</p>
          <p className="text-sm text-gray-800 mt-0.5">{voucher.debitAccountName}</p>
        </div>
        <div className="col-span-2">
          <p className="text-xs text-gray-400">{creditLabel} — Credit</p>
          <p className="text-sm text-gray-800 mt-0.5">{voucher.creditAccountName}</p>
        </div>

        <div className="col-span-2 md:col-span-4">
          <p className="text-xs text-gray-400">Description</p>
          <p className="text-sm text-gray-800 mt-0.5">{voucher.description}</p>
        </div>

        <div className="col-span-2 md:col-span-4 pt-3 border-t border-gray-100">
          <p className="text-xs text-gray-400">Amount</p>
          <p className="text-xl font-semibold text-gray-900 tabular-nums mt-0.5">{formatTaka(voucher.amount)}</p>
          <p className="text-xs text-gray-500 mt-1">{amountInWords(voucher.amount)}</p>
        </div>
      </div>

      {/* Audit trail */}
      <div className="bg-white border border-gray-200 rounded-xl p-5 mb-4 grid grid-cols-2 md:grid-cols-4 gap-5">
        <Field label="Created By" value={`${voucher.createdBy} · ${voucher.createdAt?.split("T")[0]}`} />
        <Field
          label="Submitted"
          value={voucher.submittedBy ? `${voucher.submittedBy} · ${voucher.submittedAt?.split("T")[0]}` : undefined}
        />
        <Field
          label="Approved"
          value={voucher.approvedBy ? `${voucher.approvedBy} · ${voucher.approvedAt?.split("T")[0]}` : undefined}
        />
        <Field
          label="Posted"
          value={voucher.postedBy ? `${voucher.postedBy} · ${voucher.postedAt?.split("T")[0]}` : undefined}
        />
        {voucher.rejectedBy && (
          <div className="col-span-2 md:col-span-4">
            <p className="text-xs text-gray-400">Rejected</p>
            <p className="text-sm text-red-600 mt-0.5">
              {voucher.rejectedBy} · {voucher.rejectedAt?.split("T")[0]} — {voucher.rejectReason}
            </p>
          </div>
        )}
      </div>

      {/* Attachments */}
      {voucher.id && (
        <div className="mb-4">
          <AttachmentManager
            entityType="Voucher"
            entityId={voucher.id}
            canEdit={canEditAttachments}
          />
        </div>
      )}

      {/* Print preview */}
      <div className="bg-white border border-gray-200 rounded-xl p-8">
        <p className="text-xs text-gray-400 mb-4">Print preview</p>
        <VoucherPrintView
          ref={printRef}
          voucher={voucher}
          companyName={company?.name}
          companyAddress={company?.address}
        />
      </div>

      {/* Correct dialog */}
      <Modal title={`Correct ${voucher.voucherNumber}`} open={correctOpen} onClose={() => !correctMutation.isPending && setCorrectOpen(false)} size="md">
        <div className="space-y-4">
          {error && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{error}</div>
          )}
          <div className="text-sm text-gray-600 space-y-1.5">
            <p>Posted vouchers can't be edited. This will:</p>
            <ol className="list-decimal pl-5 space-y-1 text-xs text-gray-500">
              <li>Post a reversal entry that cancels this voucher's {formatTaka(voucher.amount)} out of the ledger.</li>
              <li>Mark {voucher.voucherNumber} as <b>Reversed</b> — it stays on record with your reason.</li>
              <li>Open a new draft copy for you to fix and send for approval.</li>
            </ol>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">
              What was wrong? <span className="text-red-500">*</span>
            </label>
            <textarea
              rows={3}
              maxLength={500}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={correctReason}
              onChange={(e) => setCorrectReason(e.target.value)}
              placeholder="e.g. Amount should be 560, not 500"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Reversal date</label>
            <DateInput
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={correctDate}
              onChange={(e) => setCorrectDate(e.target.value)}
            />
            <p className="text-xs text-gray-400 mt-1">
              Today by default, so periods already reported stay unchanged. Can't be before {voucher.voucherDate?.split("T")[0]}.
            </p>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={() => setCorrectOpen(false)}
              disabled={correctMutation.isPending}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              onClick={() => correctMutation.mutate()}
              disabled={!correctReason.trim() || correctMutation.isPending}
              className="px-4 py-2 text-sm bg-orange-600 text-white rounded-lg hover:bg-orange-700 disabled:opacity-50"
            >
              {correctMutation.isPending ? "Reversing…" : "Reverse & create correction"}
            </button>
          </div>
        </div>
      </Modal>

      {/* Reject dialog */}
      <Modal title="Reject Voucher" open={rejectOpen} onClose={() => setRejectOpen(false)} size="md">
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">
              Reason <span className="text-red-500">*</span>
            </label>
            <textarea
              rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={rejectReason}
              onChange={(e) => setRejectReason(e.target.value)}
              placeholder="Why is this voucher being rejected?"
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={() => setRejectOpen(false)}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() =>
                actionMutation.mutate((voucherId: string) => rejectVoucher(voucherId, rejectReason.trim()))
              }
              disabled={!rejectReason.trim() || busy}
              className="px-4 py-2 text-sm bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
            >
              {busy ? "Rejecting…" : "Reject Voucher"}
            </button>
          </div>
        </div>
      </Modal>

      {/* Cancel confirmation */}
      <ConfirmDialog
        open={cancelOpen}
        title="Cancel Voucher"
        message={
          voucher.journalEntryId
            ? "This voucher already has a draft journal entry. Cancelling will delete that entry and mark the voucher cancelled."
            : "This will mark the voucher as cancelled. It cannot be edited afterwards."
        }
        confirmLabel="Cancel Voucher"
        cancelLabel="Keep Voucher"
        tone="danger"
        loading={busy}
        onConfirm={() => actionMutation.mutate(cancelVoucher)}
        onCancel={() => setCancelOpen(false)}
      />

      {/* Journal entry */}
      <Modal
        title={`Journal Entry ${voucher.journalEntryNumber ?? ""}`}
        open={journalOpen}
        onClose={() => setJournalOpen(false)}
        size="3xl"
      >
        {loadingJournal ? (
          <p className="text-sm text-gray-400 py-6 text-center">Loading journal entry…</p>
        ) : journal?.data?.data ? (
          <div className="space-y-4">
            <div className="grid grid-cols-3 gap-4">
              <Field label="Entry Number" value={journal.data.data.entryNumber} />
              <Field label="Entry Date" value={journal.data.data.entryDate?.split("T")[0]} />
              <Field label="Status" value={journal.data.data.status} />
            </div>
            <table className="w-full text-sm border border-gray-200 rounded-lg overflow-hidden">
              <thead className="bg-gray-50">
                <tr>
                  <th className="text-left px-3 py-2 text-xs font-medium text-gray-500">Account</th>
                  <th className="text-left px-3 py-2 text-xs font-medium text-gray-500">Narration</th>
                  <th className="text-right px-3 py-2 text-xs font-medium text-gray-500">Debit</th>
                  <th className="text-right px-3 py-2 text-xs font-medium text-gray-500">Credit</th>
                </tr>
              </thead>
              <tbody>
                {journal.data.data.lines.map((line) => (
                  <tr key={line.id} className="border-t border-gray-100">
                    <td className="px-3 py-2 text-gray-700">{line.accountName}</td>
                    <td className="px-3 py-2 text-gray-500">{line.description}</td>
                    <td className="px-3 py-2 text-right tabular-nums">
                      {line.debitAmount > 0 ? formatTaka(line.debitAmount) : ""}
                    </td>
                    <td className="px-3 py-2 text-right tabular-nums">
                      {line.creditAmount > 0 ? formatTaka(line.creditAmount) : ""}
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="bg-gray-50 border-t-2 border-gray-200">
                <tr>
                  <td colSpan={2} className="px-3 py-2 text-xs font-semibold text-gray-600">
                    Totals
                  </td>
                  <td className="px-3 py-2 text-right text-sm font-semibold tabular-nums">
                    {formatTaka(journal.data.data.totalDebit)}
                  </td>
                  <td className="px-3 py-2 text-right text-sm font-semibold tabular-nums">
                    {formatTaka(journal.data.data.totalCredit)}
                  </td>
                </tr>
              </tfoot>
            </table>
            <div className="flex justify-end">
              <button
                onClick={() => navigate("/accounts/journal-entries")}
                className="text-sm text-primary-600 hover:text-primary-700 font-medium"
              >
                Open in Journal Entries →
              </button>
            </div>
          </div>
        ) : (
          <p className="text-sm text-gray-400 py-6 text-center">No journal entry linked yet.</p>
        )}
      </Modal>
    </div>
  );
}
