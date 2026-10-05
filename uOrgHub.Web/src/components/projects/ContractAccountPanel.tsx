import { useRef, useState } from "react";
import { Link } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import { AlertTriangle, Printer, Undo2 } from "lucide-react";
import Modal from "../shared/Modal";
import SearchableDropdown from "../shared/SearchableDropdown";
import DateInput from "../shared/DateInput";
import { getContractAccount, releaseRetention, type ContractAccount } from "../../api/projects";
import { getMyCompany } from "../../api/company";
import { useChartOfAccountsLookup } from "../../hooks/useEntityLookup";
import { printDocument } from "../../utils/printDocument";
import { extractApiError } from "../../utils/apiError";
import { formatDate } from "../../utils/format";
import { money } from "./contractAccount";

function Tile({ label, value, hint, tone }: { label: string; value: number; hint?: string; tone?: "good" | "warn" | "muted" }) {
  const color = tone === "good" ? "text-green-700" : tone === "warn" ? "text-red-600" : tone === "muted" ? "text-gray-500" : "text-gray-900";
  return (
    <div className="bg-white border border-gray-200 rounded-lg px-3 py-2.5">
      <p className="text-[11px] text-gray-500">{label}</p>
      <p className={`text-sm font-semibold tabular-nums ${color}`}>{money(value)}</p>
      {hint && <p className="text-[10px] text-gray-400 mt-0.5">{hint}</p>}
    </div>
  );
}

/** Printable statement for the client: the same figures, laid out as a letterhead document. */
function ContractStatementPrint({ ca, company }: { ca: ContractAccount; company?: { name?: string; address?: string; phone?: string } }) {
  const rows: [string, number][] = [
    ["Contract value", ca.contractValue],
    ["Work certified to date (gross)", ca.certifiedGross],
    ["Less: deductions", ca.deductions],
    ["Less: retention held", ca.retentionHeld],
    ["Net certified", ca.netCertified],
    ["Retention released (invoiced)", ca.retentionReleased],
    ["Total invoiced", ca.invoiced],
    ["Total received", ca.received],
    ["Outstanding (invoiced − received)", ca.outstanding],
    ["Retention still held", ca.retentionOutstanding],
    ["Contract value not yet billed", ca.remainingToBill],
  ];
  return (
    <div className="text-[12px] text-gray-900 bg-white">
      <div className="text-center border-b-2 border-gray-800 pb-2">
        <h1 className="text-lg font-bold uppercase">{company?.name ?? "Company"}</h1>
        {company?.address && <p className="text-[10px]">{company.address}</p>}
        {company?.phone && <p className="text-[10px]">{company.phone}</p>}
      </div>
      <h2 className="text-center font-bold tracking-widest uppercase my-3">Statement of Account</h2>
      <div className="flex justify-between mb-3">
        <p><b>Client:</b> {ca.clientName}</p>
        <p><b>Project:</b> {ca.projectCode} – {ca.projectName}</p>
        <p><b>Date:</b> {formatDate(new Date().toISOString())}</p>
      </div>
      <table className="w-full border-collapse mb-4">
        <tbody>
          {rows.map(([label, v]) => (
            <tr key={label}>
              <td className="border border-gray-700 px-2 py-1">{label}</td>
              <td className="border border-gray-700 px-2 py-1 text-right tabular-nums">{money(v)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <table className="w-full border-collapse text-[10px]">
        <thead>
          <tr className="bg-gray-100">
            {["Bill", "Period", "Gross", "Retention", "Net", "Invoice", "Received", "Balance"].map((h, i) => (
              <th key={h} className={`border border-gray-700 px-1.5 py-1 ${i > 1 ? "text-right" : "text-left"}`}>{h}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {ca.bills.filter((b) => b.status === "Certified" || b.status === "Paid").map((b) => (
            <tr key={b.id}>
              <td className="border border-gray-700 px-1.5 py-1">#{b.billSequence} {b.billNumber}</td>
              <td className="border border-gray-700 px-1.5 py-1">{formatDate(b.periodFrom)} – {formatDate(b.periodTo)}</td>
              <td className="border border-gray-700 px-1.5 py-1 text-right">{money(b.grossAmount)}</td>
              <td className="border border-gray-700 px-1.5 py-1 text-right">{money(b.retentionAmount)}</td>
              <td className="border border-gray-700 px-1.5 py-1 text-right">{money(b.netAmount)}</td>
              <td className="border border-gray-700 px-1.5 py-1 text-right">{b.invoiceNumber ?? "—"}</td>
              <td className="border border-gray-700 px-1.5 py-1 text-right">{money(b.invoicePaid)}</td>
              <td className="border border-gray-700 px-1.5 py-1 text-right">{b.invoiceNumber ? money(b.invoiceBalance) : "—"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

/**
 * The client-facing money picture of one contract: agreed, certified, invoiced, received, owed,
 * and retention held. Read live from the RA bills and their AR invoices.
 */
export default function ContractAccountPanel({ projectId }: { projectId: string }) {
  const qc = useQueryClient();
  const printRef = useRef<HTMLDivElement>(null);
  const [releaseOpen, setReleaseOpen] = useState(false);
  const [release, setRelease] = useState({ amount: 0, revenueAccountId: "", releaseDate: new Date().toISOString().split("T")[0], notes: "" });
  const { options: incomeOptions } = useChartOfAccountsLookup("Income");

  const { data } = useQuery({
    queryKey: ["contract-account", projectId],
    queryFn: () => getContractAccount(projectId),
    enabled: Boolean(projectId),
  });
  const { data: company } = useQuery({ queryKey: ["my-company"], queryFn: getMyCompany, staleTime: 300000 });
  const ca = data?.data?.data;

  const releaseMutation = useMutation({
    mutationFn: () => releaseRetention({ projectId, ...release, notes: release.notes || undefined }),
    onSuccess: (res) => {
      qc.invalidateQueries({ queryKey: ["contract-account", projectId] });
      qc.invalidateQueries({ queryKey: ["invoices"] });
      toast.success(`Retention invoice ${res.data.data?.invoiceNumber ?? ""} raised.`);
      setReleaseOpen(false);
    },
    onError: (err) => toast.error(extractApiError(err)),
  });

  if (!ca) return null;

  return (
    <div className="bg-gray-50 border border-gray-200 rounded-xl p-4 mb-4">
      <div className="flex flex-wrap items-center justify-between gap-2 mb-3">
        <div>
          <h3 className="text-sm font-medium text-gray-900">Contract Account</h3>
          <p className="text-xs text-gray-500">
            {ca.clientName}
            {ca.customerName ? ` · billed as ${ca.customerName}` : ""}
          </p>
        </div>
        <div className="flex items-center gap-2">
          {ca.retentionOutstanding > 0 && (
            <button
              onClick={() => {
                setRelease((r) => ({ ...r, amount: ca.retentionOutstanding, revenueAccountId: ca.defaultRevenueAccountId ?? "" }));
                setReleaseOpen(true);
              }}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm border border-gray-200 bg-white rounded-lg hover:bg-gray-50 text-gray-700"
            >
              <Undo2 size={14} /> Release retention
            </button>
          )}
          <button
            onClick={() => printRef.current && printDocument(`Statement – ${ca.projectCode}`, printRef.current)}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm border border-gray-200 bg-white rounded-lg hover:bg-gray-50 text-gray-700"
          >
            <Printer size={14} /> Print statement
          </button>
        </div>
      </div>

      {!ca.customerId && (
        <div className="flex gap-2 text-sm text-amber-800 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2 mb-3">
          <AlertTriangle size={16} className="shrink-0 mt-0.5" />
          <p>
            {ca.clientName} isn't linked to an Accounts customer yet, so bills can't be invoiced.{" "}
            <Link to="/projects/clients" className="font-medium underline">Link it in Clients</Link>.
          </p>
        </div>
      )}

      <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-2">
        <Tile label="Contract value" value={ca.contractValue} />
        <Tile label="Certified (gross)" value={ca.certifiedGross} />
        <Tile label="Remaining to bill" value={ca.remainingToBill} tone="muted" />
        <Tile label="Invoiced" value={ca.invoiced} hint={ca.notYetInvoiced > 0 ? `${money(ca.notYetInvoiced)} certified, not invoiced` : undefined} />
        <Tile label="Received" value={ca.received} tone="good" />
        <Tile label="Outstanding" value={ca.outstanding} tone={ca.outstanding > 0 ? "warn" : undefined} />
        <Tile label="Retention held" value={ca.retentionOutstanding} hint={ca.retentionReleased > 0 ? `${money(ca.retentionReleased)} released` : undefined} />
      </div>

      {/* Off-screen printable statement */}
      <div className="hidden">
        <div ref={printRef}>
          <ContractStatementPrint ca={ca} company={company} />
        </div>
      </div>

      <Modal title="Release retention" open={releaseOpen} onClose={() => setReleaseOpen(false)} size="md">
        <div className="space-y-3">
          <p className="text-sm text-gray-600">
            Invoices retention held back on the RA bills (typically at handover). Still held: <b>{money(ca.retentionOutstanding)}</b>.
          </p>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Amount *</label>
              <input
                type="number"
                min={0}
                max={ca.retentionOutstanding}
                step="0.01"
                value={release.amount || ""}
                onChange={(e) => setRelease((r) => ({ ...r, amount: parseFloat(e.target.value) || 0 }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Release date</label>
              <DateInput
                value={release.releaseDate}
                onChange={(e) => setRelease((r) => ({ ...r, releaseDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
          </div>
          <SearchableDropdown
            label="Revenue account *"
            options={incomeOptions}
            value={release.revenueAccountId}
            onChange={(v) => setRelease((r) => ({ ...r, revenueAccountId: v ?? "" }))}
            placeholder="Select revenue account"
            searchPlaceholder="Search accounts..."
          />
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <input
              value={release.notes}
              onChange={(e) => setRelease((r) => ({ ...r, notes: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setReleaseOpen(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button
              onClick={() => releaseMutation.mutate()}
              disabled={!(release.amount > 0) || !release.revenueAccountId || releaseMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {releaseMutation.isPending ? "Raising…" : "Raise retention invoice"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
