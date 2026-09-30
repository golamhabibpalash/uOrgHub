import { Fragment, useState, useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import { Plus, Pencil, CheckCircle, XCircle, ChevronDown, ChevronUp, AlertCircle } from "lucide-react";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import AttachmentManager from "../../components/shared/AttachmentManager";
import PendingAttachments from "../../components/shared/PendingAttachments";
import {
  useVendorLookup,
  useFiscalYearLookup,
  useCostCenterLookup,
  useChartOfAccountsLookup,
} from "../../hooks/useEntityLookup";
import {
  getBills,
  getBillById,
  createBill,
  updateBill,
  approveBill,
  voidBill,
  getTaxRates,
  BillStatus,
} from "../../api/accounts";
import { uploadAttachment } from "../../api/attachments";
import { useAuthStore } from "../../store/authStore";
import { extractApiError } from "../../utils/apiError";
import DateInput from "../../components/shared/DateInput";
import BillLineItemsEditor from "./BillLineItemsEditor";
import { billLineErrors, calcBillLine, emptyBillLine, money, newLineUid, type BillLineForm } from "./billLine";

const ATTACHMENT_ENTITY = "Bill";

const statusColors: Record<BillStatus, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Received: "bg-blue-50 text-blue-700",
  PartiallyPaid: "bg-yellow-50 text-yellow-700",
  Paid: "bg-green-50 text-green-700",
  Overdue: "bg-red-50 text-red-700",
  Cancelled: "bg-gray-100 text-gray-400",
  Void: "bg-red-100 text-red-500",
};

interface BillForm {
  vendorBillNumber: string;
  vendorId: string;
  fiscalYearId: string;
  billDate: string;
  dueDate: string;
  notes: string;
  costCenterId: string;
  lines: BillLineForm[];
}

const today = () => new Date().toISOString().split("T")[0];

const emptyForm = (): BillForm => ({
  vendorBillNumber: "",
  vendorId: "",
  fiscalYearId: "",
  billDate: today(),
  dueDate: "",
  notes: "",
  costCenterId: "",
  lines: [emptyBillLine()],
});

const inputCls = "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";

/** Header-level checks mirroring CreateBillValidator, so users see problems before a round-trip. */
function formErrors(form: BillForm, isEdit: boolean): string[] {
  const errors: string[] = [];
  if (!isEdit && !form.vendorId) errors.push("Select a vendor.");
  if (!isEdit && !form.fiscalYearId) errors.push("Select a fiscal year.");
  if (!form.billDate) errors.push("Bill date is required.");
  if (!form.dueDate) errors.push("Due date is required.");
  else if (form.billDate && form.dueDate < form.billDate) errors.push("Due date must be on or after the bill date.");
  const badLines = form.lines
    .map((l, i) => (Object.keys(billLineErrors(l)).length > 0 ? i + 1 : null))
    .filter((n): n is number => n !== null);
  if (badLines.length > 0) errors.push(`Complete the highlighted fields on line ${badLines.join(", ")}.`);
  return errors;
}

export default function Bills() {
  const qc = useQueryClient();
  const { hasClaim, hasRole } = useAuthStore();
  // Mirrors the "Bill" attachment target: Edit, or Create so the person entering a bill can attach it.
  const canEditAttachments = hasRole("Admin") || hasClaim("Accounts.Bills.Edit") || hasClaim("Accounts.Bills.Create");

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [modal, setModal] = useState(false);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<BillForm>(emptyForm);
  const [pendingFiles, setPendingFiles] = useState<File[]>([]);
  const [showErrors, setShowErrors] = useState(false);
  const [saveError, setSaveError] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["bills", page, search, statusFilter],
    queryFn: () => getBills({ page, pageSize: 10, search }, undefined, statusFilter || undefined),
  });

  const { data: taxRatesData } = useQuery({ queryKey: ["tax-rates", 1, ""], queryFn: () => getTaxRates({ page: 1, pageSize: 100 }) });
  const { options: vendorOptions } = useVendorLookup();
  const { options: fiscalYearOptions } = useFiscalYearLookup();
  const { options: costCenterOptions } = useCostCenterLookup();
  const { options: coaOptions } = useChartOfAccountsLookup();

  const bills = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const taxRates = useMemo(() => taxRatesData?.data?.data?.items ?? [], [taxRatesData]);
  const taxRateOptions = useMemo(
    () => taxRates.map((t) => ({ value: t.id, label: `${t.code} (${t.rate}%)`, searchText: t.name })),
    [taxRates],
  );
  const taxRateById = useMemo(() => new Map(taxRates.map((t) => [t.id, t.rate])), [taxRates]);

  const totals = useMemo(
    () =>
      form.lines.reduce(
        (acc, l) => {
          const a = calcBillLine(l, taxRateById.get(l.taxRateId) ?? 0);
          return { gross: acc.gross + a.gross, discount: acc.discount + a.discount, tax: acc.tax + a.tax, total: acc.total + a.total };
        },
        { gross: 0, discount: 0, tax: 0, total: 0 },
      ),
    [form.lines, taxRateById],
  );

  const linesPayload = () =>
    form.lines.map((l, i) => ({
      description: l.description.trim(),
      quantity: l.quantity,
      unitPrice: l.unitPrice,
      discountPercent: l.discountPercent,
      lineOrder: i + 1,
      taxRateId: l.taxRateId || undefined,
      expenseAccountId: l.expenseAccountId,
      costCenterId: l.costCenterId || undefined,
    }));

  const onSaveError = (fallback: string) => (err: unknown) => setSaveError(extractApiError(err) || fallback);

  const createMutation = useMutation({
    mutationFn: async () => {
      const payload = {
        vendorBillNumber: form.vendorBillNumber || undefined,
        vendorId: form.vendorId,
        fiscalYearId: form.fiscalYearId,
        billDate: form.billDate,
        dueDate: form.dueDate,
        notes: form.notes || undefined,
        costCenterId: form.costCenterId || undefined,
        lines: linesPayload(),
      };
      const res = await createBill(payload as unknown as Parameters<typeof createBill>[0]);
      const bill = res.data.data;

      // The bill exists now; upload queued files against it. A failed file doesn't undo the bill —
      // tell the user which one so they can re-attach it from the bill's row.
      let failed = 0;
      if (bill?.id) {
        for (const file of pendingFiles) {
          try {
            await uploadAttachment(file, ATTACHMENT_ENTITY, bill.id);
          } catch (err) {
            failed++;
            toast.error(`${file.name} was not attached: ${extractApiError(err)}`);
          }
        }
      }
      return { bill, uploaded: pendingFiles.length - failed };
    },
    onSuccess: ({ bill, uploaded }) => {
      qc.invalidateQueries({ queryKey: ["bills"] });
      if (bill?.id && uploaded > 0) qc.invalidateQueries({ queryKey: ["attachments", ATTACHMENT_ENTITY, bill.id] });
      closeModal(true);
    },
    onError: onSaveError("Failed to save bill."),
  });

  const updateMutation = useMutation({
    mutationFn: () => {
      if (!editingId) throw new Error("No bill selected for edit");
      const payload = {
        vendorBillNumber: form.vendorBillNumber || undefined,
        billDate: form.billDate,
        dueDate: form.dueDate,
        notes: form.notes || undefined,
        costCenterId: form.costCenterId || undefined,
        lines: linesPayload(),
      };
      return updateBill(editingId, payload as unknown as Parameters<typeof updateBill>[1]);
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["bills"] }); closeModal(true); },
    onError: onSaveError("Failed to update bill."),
  });

  const approveMutation = useMutation({
    mutationFn: (id: string) => approveBill(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["bills"] }),
  });

  const voidMutation = useMutation({
    mutationFn: (id: string) => voidBill(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["bills"] }),
  });

  const saving = createMutation.isPending || updateMutation.isPending;
  const validationErrors = showErrors ? formErrors(form, Boolean(editingId)) : [];

  function openAdd() {
    setEditingId(null);
    setForm(emptyForm());
    setPendingFiles([]);
    setShowErrors(false);
    setSaveError("");
    setModal(true);
  }

  async function openEdit(id: string) {
    setEditingId(id);
    setSaveError("");
    setShowErrors(false);
    try {
      const res = await getBillById(id);
      const bill = res.data.data;
      if (!bill) { setSaveError("Bill not found."); return; }
      setForm({
        vendorBillNumber: bill.vendorBillNumber ?? "",
        vendorId: bill.vendorId,
        fiscalYearId: bill.fiscalYearId,
        billDate: bill.billDate.split("T")[0],
        dueDate: bill.dueDate.split("T")[0],
        notes: bill.notes ?? "",
        costCenterId: bill.costCenterId ?? "",
        lines: bill.lines.length > 0
          ? [...bill.lines]
              .sort((a, b) => a.lineOrder - b.lineOrder)
              .map((l) => ({
                uid: newLineUid(),
                description: l.description,
                quantity: l.quantity,
                unitPrice: l.unitPrice,
                discountPercent: l.discountPercent,
                lineOrder: l.lineOrder,
                taxRateId: l.taxRateId ?? "",
                expenseAccountId: l.expenseAccountId,
                costCenterId: l.costCenterId ?? "",
              }))
          : [emptyBillLine()],
      });
      setModal(true);
    } catch {
      toast.error("Failed to load bill data.");
    }
  }

  function closeModal(force = false) {
    if (saving && !force) return;
    setModal(false);
    setSaveError("");
    setEditingId(null);
    setPendingFiles([]);
    setShowErrors(false);
  }

  function submit() {
    setShowErrors(true);
    setSaveError("");
    if (formErrors(form, Boolean(editingId)).length > 0) return;
    if (editingId) updateMutation.mutate();
    else createMutation.mutate();
  }

  const set = <K extends keyof BillForm>(key: K, value: BillForm[K]) => setForm((f) => ({ ...f, [key]: value }));

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Bills</h2>
          <p className="text-xs text-gray-400">Manage vendor bills (AP)</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> New Bill
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center gap-3">
          <input
            type="text"
            placeholder="Search bills..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-52 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
          <select
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
          >
            <option value="">All Statuses</option>
            {(["Draft", "Received", "PartiallyPaid", "Paid", "Overdue", "Cancelled", "Void"] as BillStatus[]).map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center h-40 text-sm text-gray-400">Loading...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-gray-50">
                  {["Bill #", "Vendor", "Vendor Bill #", "Date", "Due Date", "Status", "Total", "Paid", "Balance Due", "Actions"].map((h) => (
                    <th key={h} className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {bills.length === 0 ? (
                  <tr><td colSpan={10} className="text-center py-10 text-gray-400">No bills found</td></tr>
                ) : bills.map((bill) => (
                  <Fragment key={bill.id}>
                    <tr className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="px-4 py-2.5 font-medium text-primary-600">{bill.billNumber}</td>
                      <td className="px-4 py-2.5">{bill.vendorName}</td>
                      <td className="px-4 py-2.5 text-gray-500">{bill.vendorBillNumber ?? "—"}</td>
                      <td className="px-4 py-2.5">{bill.billDate?.split("T")[0]}</td>
                      <td className="px-4 py-2.5">{bill.dueDate?.split("T")[0]}</td>
                      <td className="px-4 py-2.5">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[bill.status]}`}>{bill.status}</span>
                      </td>
                      <td className="px-4 py-2.5">{money(bill.totalAmount)}</td>
                      <td className="px-4 py-2.5 text-green-700">{money(bill.paidAmount)}</td>
                      <td className="px-4 py-2.5 font-medium text-red-600">{money(bill.totalAmount - bill.paidAmount)}</td>
                      <td className="px-4 py-2.5">
                        <div className="flex items-center gap-2">
                          <button onClick={() => setExpandedId(expandedId === bill.id ? null : bill.id)} className="text-gray-400 hover:text-primary-600" title="View details & attachments">
                            {expandedId === bill.id ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
                          </button>
                          {bill.status === "Draft" && (
                            <>
                              <button onClick={() => openEdit(bill.id)} className="text-gray-500 hover:text-primary-600" title="Edit Bill"><Pencil size={13} /></button>
                              <button onClick={() => approveMutation.mutate(bill.id)} className="text-green-500 hover:text-green-700" title="Approve"><CheckCircle size={13} /></button>
                            </>
                          )}
                          {(bill.status === "Draft" || bill.status === "Received") && (
                            <button onClick={() => voidMutation.mutate(bill.id)} className="text-red-400 hover:text-red-600" title="Void"><XCircle size={13} /></button>
                          )}
                        </div>
                      </td>
                    </tr>
                    {expandedId === bill.id && (
                      <tr className="bg-gray-50">
                        <td colSpan={10} className="px-6 py-4">
                          <div className="grid grid-cols-1 lg:grid-cols-5 gap-4">
                            <div className="lg:col-span-3 bg-white border border-gray-200 rounded-xl p-4">
                              <h3 className="text-sm font-medium text-gray-900 mb-2">Line items</h3>
                              <table className="w-full text-xs">
                                <thead>
                                  <tr className="text-gray-500 border-b border-gray-100">
                                    <th className="text-left pb-1.5 font-medium">Description</th>
                                    <th className="text-right pb-1.5 font-medium">Qty</th>
                                    <th className="text-right pb-1.5 font-medium">Unit Price</th>
                                    <th className="text-right pb-1.5 font-medium">Disc%</th>
                                    <th className="text-right pb-1.5 font-medium">Tax</th>
                                    <th className="text-right pb-1.5 font-medium">Line Total</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {bill.lines.map((line) => (
                                    <tr key={line.id} className="border-b border-gray-50 last:border-0">
                                      <td className="py-1.5">{line.description}</td>
                                      <td className="py-1.5 text-right tabular-nums">{line.quantity}</td>
                                      <td className="py-1.5 text-right tabular-nums">{money(line.unitPrice)}</td>
                                      <td className="py-1.5 text-right tabular-nums">{line.discountPercent}%</td>
                                      <td className="py-1.5 text-right tabular-nums">{money(line.taxAmount)}</td>
                                      <td className="py-1.5 text-right tabular-nums font-medium">{money(line.lineTotal)}</td>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
                            </div>
                            <div className="lg:col-span-2">
                              <AttachmentManager
                                entityType={ATTACHMENT_ENTITY}
                                entityId={bill.id}
                                canEdit={canEditAttachments && bill.status !== "Void" && bill.status !== "Cancelled"}
                              />
                            </div>
                          </div>
                        </td>
                      </tr>
                    )}
                  </Fragment>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editingId ? "Edit Bill" : "New Bill"} open={modal} onClose={() => closeModal()} size="5xl">
        <div className="space-y-5">
          {(saveError || validationErrors.length > 0) && (
            <div className="flex gap-2 text-sm text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              <AlertCircle size={16} className="shrink-0 mt-0.5" />
              <ul className="space-y-0.5">
                {saveError && <li>{saveError}</li>}
                {validationErrors.map((e) => <li key={e}>{e}</li>)}
              </ul>
            </div>
          )}

          {/* Bill details */}
          <section>
            <div className="flex items-baseline justify-between mb-3">
              <h3 className="text-sm font-medium text-gray-900">Bill details</h3>
              {!editingId && <span className="text-xs text-gray-400">Bill number is generated on save</span>}
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
              <SearchableDropdown
                label="Vendor *"
                options={vendorOptions}
                value={form.vendorId}
                onChange={(v) => set("vendorId", v ?? "")}
                placeholder="Select vendor"
                searchPlaceholder="Search vendors..."
                disabled={Boolean(editingId)}
                error={showErrors && !editingId && !form.vendorId ? "Vendor is required" : undefined}
              />
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Vendor Bill Number</label>
                <input
                  className={inputCls}
                  value={form.vendorBillNumber}
                  maxLength={50}
                  placeholder="Number on the vendor's invoice"
                  onChange={(e) => set("vendorBillNumber", e.target.value)}
                />
              </div>
              <SearchableDropdown
                label="Fiscal Year *"
                options={fiscalYearOptions}
                value={form.fiscalYearId}
                onChange={(v) => set("fiscalYearId", v ?? "")}
                placeholder="Select year"
                searchPlaceholder="Search fiscal years..."
                disabled={Boolean(editingId)}
                error={showErrors && !editingId && !form.fiscalYearId ? "Fiscal year is required" : undefined}
              />
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Bill Date *</label>
                <DateInput className={inputCls} value={form.billDate} onChange={(e) => set("billDate", e.target.value)} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Due Date *</label>
                <DateInput className={inputCls} value={form.dueDate} onChange={(e) => set("dueDate", e.target.value)} />
              </div>
              <SearchableDropdown
                label="Cost Center"
                options={costCenterOptions}
                value={form.costCenterId}
                onChange={(v) => set("costCenterId", v ?? "")}
                placeholder="None"
                searchPlaceholder="Search cost centers..."
              />
            </div>
          </section>

          {/* Line items */}
          <section>
            <div className="flex items-baseline justify-between mb-3">
              <h3 className="text-sm font-medium text-gray-900">
                Line items <span className="text-xs font-normal text-gray-400">({form.lines.length})</span>
              </h3>
            </div>
            <BillLineItemsEditor
              lines={form.lines}
              onChange={(lines) => set("lines", lines)}
              taxRateOptions={taxRateOptions}
              taxRateById={taxRateById}
              accountOptions={coaOptions}
              costCenterOptions={costCenterOptions}
              showErrors={showErrors}
            />
          </section>

          {/* Notes + attachments | totals */}
          <section className="grid grid-cols-1 lg:grid-cols-5 gap-5">
            <div className="lg:col-span-3 space-y-4">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Notes</label>
                <textarea rows={3} className={inputCls} value={form.notes} onChange={(e) => set("notes", e.target.value)} />
              </div>
              {editingId ? (
                <AttachmentManager entityType={ATTACHMENT_ENTITY} entityId={editingId} canEdit={canEditAttachments} bare />
              ) : (
                <PendingAttachments files={pendingFiles} onChange={setPendingFiles} disabled={saving} />
              )}
            </div>
            <div className="lg:col-span-2">
              <div className="bg-gray-50 border border-gray-200 rounded-xl p-4 space-y-2 text-sm lg:sticky lg:top-0">
                <div className="flex justify-between text-gray-600">
                  <span>Gross amount</span>
                  <span className="tabular-nums">{money(totals.gross)}</span>
                </div>
                <div className="flex justify-between text-gray-600">
                  <span>Discount</span>
                  <span className="tabular-nums">{totals.discount > 0 ? `− ${money(totals.discount)}` : money(0)}</span>
                </div>
                <div className="flex justify-between text-gray-600">
                  <span>Subtotal</span>
                  <span className="tabular-nums">{money(totals.gross - totals.discount)}</span>
                </div>
                <div className="flex justify-between text-gray-600">
                  <span>Tax</span>
                  <span className="tabular-nums">{money(totals.tax)}</span>
                </div>
                <div className="flex justify-between border-t border-gray-200 pt-2 mt-1 text-base font-semibold text-gray-900">
                  <span>Total</span>
                  <span className="tabular-nums">{money(totals.total)}</span>
                </div>
              </div>
            </div>
          </section>

          <div className="flex justify-end gap-2 pt-3 border-t border-gray-100">
            <button onClick={() => closeModal()} disabled={saving} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-50">Cancel</button>
            <button onClick={submit} disabled={saving} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {saving
                ? (pendingFiles.length > 0 && !editingId ? "Saving & uploading..." : "Saving...")
                : editingId ? "Update Bill" : "Create Bill"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
