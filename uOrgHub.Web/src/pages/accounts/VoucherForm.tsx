import { useMemo, useRef, useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import { AlertTriangle, ArrowLeft, Info, Pencil, Trash2 } from "lucide-react";
import SearchableDropdown, { SelectOption } from "../../components/shared/SearchableDropdown";
import {
  useEmployeeNameLookup,
  useFiscalYearLookup,
  useOverheadCostCenterLookup,
  useProjectLookup,
  useVoucherAccountOptions,
} from "../../hooks/useEntityLookup";
import {
  createVoucher,
  getVoucherById,
  updateVoucher,
  CreateVoucherPayload,
  UpdateVoucherPayload,
  VoucherAccountOption,
  VoucherType,
} from "../../api/accounts";
import { voucherThemes } from "../../components/accounts/voucherTheme";
import { amountInWords, formatDate, formatTaka } from "../../utils/format";
import { extractApiError } from "../../utils/apiError";
import DateInput from "../../components/shared/DateInput";
import ConfirmDialog from "../../components/shared/ConfirmDialog";

/** What the voucher is charged to. Every voucher is one or the other, never both. */
type ChargeMode = "project" | "overhead";

/**
 * Keeps a free-typed name selectable in a creatable dropdown. Without this the closed control
 * falls back to its placeholder whenever the stored value is not one of the employees — which is
 * the normal case for an external payee, or for a voucher saved before that person was hired.
 */
function withTypedValue(options: SelectOption[], value: string): SelectOption[] {
  if (!value || options.some((o) => o.value === value)) return options;
  return [...options, { value, label: value }];
}

/** Falls back to Debit for a missing or unrecognised ?type=, rather than trusting the URL. */
function parseVoucherType(raw: string | null): VoucherType {
  return raw === "Credit" || raw === "Contra" ? raw : "Debit";
}

interface FormState {
  voucherDate: string;
  referenceNumber: string;
  fiscalYearId: string;
  chargeMode: ChargeMode;
  projectId: string;
  costCenterId: string;
  name: string;
  section: string;
  description: string;
  debitAccountId: string;
  creditAccountId: string;
  amount: string;
  preparedBy: string;
  receivedBy: string;
}

const emptyForm: FormState = {
  voucherDate: new Date().toISOString().split("T")[0],
  referenceNumber: "",
  fiscalYearId: "",
  chargeMode: "project",
  projectId: "",
  costCenterId: "",
  name: "",
  section: "",
  description: "",
  debitAccountId: "",
  creditAccountId: "",
  amount: "",
  preparedBy: "",
  receivedBy: "",
};

/**
 * A voucher held in memory only, built by "Save and Continue". Nothing here is written to the
 * database until the user clicks the Save Voucher button on the temporary list; account names
 * are captured at add time so the table stays readable even if the account list later changes.
 */
interface TempVoucher {
  /** Stable local key — never sent to the server. */
  key: string;
  tempRef: string;
  voucherDate: string;
  referenceNumber: string;
  fiscalYearId: string;
  chargeMode: ChargeMode;
  projectId: string;
  costCenterId: string;
  name: string;
  section: string;
  description: string;
  debitAccountId: string;
  debitAccountName: string;
  creditAccountId: string;
  creditAccountName: string;
  amount: number;
  preparedBy: string;
  receivedBy: string;
}

/** Re-numbers the pending vouchers so the displayed Temporary-00N always runs 1, 2, 3… */
function renumberTemp(list: TempVoucher[]): TempVoucher[] {
  return list.map((t, i) => ({ ...t, tempRef: `Temporary-${String(i + 1).padStart(3, "0")}` }));
}

/**
 * Resolves what the form should start from before mounting it, so the editable
 * state can be seeded once via useState rather than patched in afterwards.
 */
export default function VoucherForm() {
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const isEdit = Boolean(id);

  const { data: existing, isLoading } = useQuery({
    queryKey: ["voucher", id],
    queryFn: () => getVoucherById(id!),
    enabled: isEdit,
  });

  if (isEdit && isLoading) {
    return <div className="p-6 text-sm text-gray-400">Loading voucher…</div>;
  }

  const voucher = existing?.data?.data;

  if (isEdit && !voucher) {
    return <div className="p-6 text-sm text-gray-400">Voucher not found.</div>;
  }

  // Resolved once so the form's key (and thus its temporary-voucher session) follows the type.
  // Changing ?type= remounts the form rather than carrying one type's pending vouchers into another.
  const voucherType = voucher?.voucherType ?? parseVoucherType(searchParams.get("type"));

  return (
    <VoucherFormFields
      key={id ? id : `new-${voucherType}`}
      voucherId={id}
      voucherNumber={voucher?.voucherNumber}
      voucherType={voucherType}
      initialForm={
        voucher
          ? {
              voucherDate: voucher.voucherDate?.split("T")[0] ?? "",
              referenceNumber: voucher.referenceNumber ?? "",
              fiscalYearId: voucher.fiscalYearId ?? "",
              chargeMode: voucher.projectId ? "project" : "overhead",
              projectId: voucher.projectId ?? "",
              costCenterId: voucher.projectId ? "" : voucher.costCenterId ?? "",
              name: voucher.name ?? "",
              section: voucher.section ?? "",
              description: voucher.description,
              debitAccountId: voucher.debitAccountId,
              creditAccountId: voucher.creditAccountId,
              amount: String(voucher.amount),
              preparedBy: voucher.preparedBy ?? "",
              receivedBy: voucher.receivedBy ?? "",
            }
          : emptyForm
      }
    />
  );
}

interface VoucherFormFieldsProps {
  voucherId?: string;
  voucherNumber?: string;
  voucherType: VoucherType;
  initialForm: FormState;
}

function VoucherFormFields({ voucherId, voucherNumber, voucherType, initialForm }: VoucherFormFieldsProps) {
  const navigate = useNavigate();
  const qc = useQueryClient();
  const isEdit = Boolean(voucherId);
  // The temporary-voucher workflow only applies to creating new vouchers, not to editing an
  // existing one, so every piece of it is gated behind isNew.
  const isNew = !isEdit;

  const [form, setForm] = useState<FormState>(initialForm);
  const [error, setError] = useState("");

  const [tempVouchers, setTempVouchers] = useState<TempVoucher[]>([]);
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [batchError, setBatchError] = useState("");
  const [confirmClearOpen, setConfirmClearOpen] = useState(false);
  // When on, "Save and Continue" keeps the entered details in the form so the next voucher can
  // reuse them (a typical entry is the previous one with a small change).
  const [keepFormData, setKeepFormData] = useState(false);
  const tempKeyRef = useRef(1);

  const {
    debitOptions,
    creditOptions,
    debitAccounts,
    creditAccounts,
    debitFieldLabel,
    creditFieldLabel,
    isOwnAccountTransfer,
    isLoading: loadingAccounts,
  } = useVoucherAccountOptions(voucherType);
  const { options: projects, isLoading: loadingProjects } = useProjectLookup();
  const { options: overheadCostCenters, isLoading: loadingCostCenters } = useOverheadCostCenterLookup();
  const { options: fiscalYears, findByDate } = useFiscalYearLookup();
  const { options: employees, isLoading: loadingEmployees } = useEmployeeNameLookup();

  // The fiscal year follows the voucher date: an explicit pick wins, otherwise the year covering
  // the date is used. Derived at render rather than in an effect, so it also fills itself in once
  // the fiscal year list finishes loading.
  const fiscalYearId = form.fiscalYearId || findByDate(form.voucherDate) || "";
  const fiscalYearIsAutomatic = form.fiscalYearId === "" && fiscalYearId !== "";
  const noFiscalYearForDate = form.voucherDate !== "" && fiscalYearId === "";

  const theme = voucherThemes[voucherType];

  // "Paid To" / "Received From" only make sense when money crosses the organisation's boundary.
  // A Contra voucher moves it internally, so the field names the reason instead of a party.
  const labels = {
    Debit: { nameLabel: "Paid To", namePlaceholder: "Name of person or party receiving the money" },
    Credit: { nameLabel: "Received From", namePlaceholder: "Name of person or party paying the money" },
    // Named for the instrument rather than "Reference", which would now collide with the
    // voucher's own reference number field sitting beside it.
    Contra: { nameLabel: "Cheque / Slip No.", namePlaceholder: "Cheque number, slip number or similar" },
  }[voucherType];

  const amountValue = Number(form.amount) || 0;
  const words = useMemo(() => (amountValue > 0 ? amountInWords(amountValue) : ""), [amountValue]);

  const chargeTargetChosen =
    form.chargeMode === "project" ? form.projectId !== "" : form.costCenterId !== "";

  const canSave =
    form.voucherDate !== "" &&
    form.description.trim() !== "" &&
    chargeTargetChosen &&
    form.debitAccountId !== "" &&
    form.creditAccountId !== "" &&
    form.debitAccountId !== form.creditAccountId &&
    amountValue > 0;

  const saveMutation = useMutation({
    mutationFn: () => {
      // Voucher type is fixed at creation — the update endpoint does not accept it.
      const common: UpdateVoucherPayload = {
        voucherDate: form.voucherDate,
        referenceNumber: form.referenceNumber.trim() || undefined,
        fiscalYearId: fiscalYearId || undefined,
        // Exactly one of these goes to the server; sending both is a validation error.
        projectId: form.chargeMode === "project" ? form.projectId : undefined,
        costCenterId: form.chargeMode === "overhead" ? form.costCenterId : undefined,
        name: form.name || undefined,
        section: form.section || undefined,
        description: form.description,
        debitAccountId: form.debitAccountId,
        creditAccountId: form.creditAccountId,
        amount: amountValue,
        preparedBy: form.preparedBy || undefined,
        receivedBy: form.receivedBy || undefined,
      };
      return isEdit
        ? updateVoucher(voucherId!, common)
        : createVoucher({ voucherType, ...common });
    },
    onSuccess: (res) => {
      qc.invalidateQueries({ queryKey: ["vouchers"] });
      qc.invalidateQueries({ queryKey: ["voucher", voucherId] });
      const saved = res.data?.data;
      navigate(saved ? `/accounts/vouchers/${saved.id}` : "/accounts/vouchers");
    },
    onError: (err: unknown) => setError(extractApiError(err)),
  });

  /** Plain account name for a selected id, falling back to the dropdown label if unknown. */
  const accountNameOf = (accounts: VoucherAccountOption[], id: string) =>
    accounts.find((a) => a.id === id)?.accountName ?? "";

  const optionLabelOf = (options: SelectOption[], id: string) =>
    options.find((o) => o.value === id)?.label ?? id;

  function buildPayload(temp: TempVoucher): CreateVoucherPayload {
    return {
      voucherType,
      voucherDate: temp.voucherDate,
      referenceNumber: temp.referenceNumber.trim() || undefined,
      fiscalYearId: temp.fiscalYearId || undefined,
      // Exactly one of these goes to the server; sending both is a validation error.
      projectId: temp.chargeMode === "project" ? temp.projectId : undefined,
      costCenterId: temp.chargeMode === "overhead" ? temp.costCenterId : undefined,
      name: temp.name || undefined,
      section: temp.section || undefined,
      description: temp.description,
      debitAccountId: temp.debitAccountId,
      creditAccountId: temp.creditAccountId,
      amount: temp.amount,
      preparedBy: temp.preparedBy || undefined,
      receivedBy: temp.receivedBy || undefined,
    };
  }

  /**
   * Saves every pending temporary voucher through the normal create endpoint. There is no batch
   * endpoint — each voucher is created one at a time with the same server-side validation the
   * single Save Voucher button uses. Saving stops at the first failure so the user can fix or
   * remove that entry; the vouchers saved before it are already in the database and are dropped
   * from the list, while the failed one and everything after it stay pending.
   */
  const batchSaveMutation = useMutation({
    mutationFn: async (): Promise<{ savedKeys: string[]; failedTempRef?: string; error?: string }> => {
      const savedKeys: string[] = [];
      for (const temp of tempVouchers) {
        try {
          await createVoucher(buildPayload(temp));
          savedKeys.push(temp.key);
        } catch (err) {
          return { savedKeys, failedTempRef: temp.tempRef, error: extractApiError(err) };
        }
      }
      return { savedKeys };
    },
    onSuccess: (result) => {
      qc.invalidateQueries({ queryKey: ["vouchers"] });
      setTempVouchers((prev) => renumberTemp(prev.filter((t) => !result.savedKeys.includes(t.key))));
      setEditingKey(null);
      setError("");
      if (result.error) {
        setBatchError(`${result.failedTempRef}: ${result.error}`);
      } else {
        toast.dismiss();
        toast.success(
          `${result.savedKeys.length} ${result.savedKeys.length === 1 ? "voucher" : "vouchers"} saved to the database.`,
        );
      }
    },
    onError: () => setBatchError("Unexpected error while saving the vouchers. Please try again."),
  });

  function handleSaveAndContinue() {
    setError("");
    setBatchError("");
    if (!canSave) return;

    const entry: TempVoucher = {
      key: editingKey ?? `temp-${tempKeyRef.current++}`,
      tempRef: "",
      voucherDate: form.voucherDate,
      referenceNumber: form.referenceNumber,
      fiscalYearId,
      chargeMode: form.chargeMode,
      projectId: form.projectId,
      costCenterId: form.costCenterId,
      name: form.name,
      section: form.section,
      description: form.description,
      debitAccountId: form.debitAccountId,
      debitAccountName:
        accountNameOf(debitAccounts, form.debitAccountId) || optionLabelOf(debitOptions, form.debitAccountId),
      creditAccountId: form.creditAccountId,
      creditAccountName:
        accountNameOf(creditAccounts, form.creditAccountId) || optionLabelOf(creditOptions, form.creditAccountId),
      amount: amountValue,
      preparedBy: form.preparedBy,
      receivedBy: form.receivedBy,
    };

    setTempVouchers((prev) =>
      editingKey
        ? prev.map((t) => (t.key === editingKey ? { ...entry, key: editingKey, tempRef: t.tempRef } : t))
        : renumberTemp([...prev, entry]),
    );

    setEditingKey(null);
    // Leave the entered details in place when the user opted to reuse them for the next voucher.
    if (!keepFormData) {
      setForm({ ...emptyForm, voucherDate: new Date().toISOString().split("T")[0] });
    }
  }

  function handleEditTemp(temp: TempVoucher) {
    setForm({
      voucherDate: temp.voucherDate,
      referenceNumber: temp.referenceNumber,
      fiscalYearId: temp.fiscalYearId,
      chargeMode: temp.chargeMode,
      projectId: temp.projectId,
      costCenterId: temp.costCenterId,
      name: temp.name,
      section: temp.section,
      description: temp.description,
      debitAccountId: temp.debitAccountId,
      creditAccountId: temp.creditAccountId,
      amount: String(temp.amount),
      preparedBy: temp.preparedBy,
      receivedBy: temp.receivedBy,
    });
    setEditingKey(temp.key);
    setError("");
    setBatchError("");
  }

  function handleCancelEdit() {
    setEditingKey(null);
    setForm({ ...emptyForm, voucherDate: new Date().toISOString().split("T")[0] });
  }

  function handleRemoveTemp(key: string) {
    setTempVouchers((prev) => renumberTemp(prev.filter((t) => t.key !== key)));
    setBatchError("");
    if (editingKey === key) {
      setEditingKey(null);
      setForm({ ...emptyForm, voucherDate: new Date().toISOString().split("T")[0] });
    }
  }

  function handleClearAll() {
    setTempVouchers([]);
    setBatchError("");
    if (editingKey) setForm({ ...emptyForm, voucherDate: new Date().toISOString().split("T")[0] });
    setEditingKey(null);
    setConfirmClearOpen(false);
  }

  const hasTempVouchers = tempVouchers.length > 0;
  const editingTemp = editingKey ? tempVouchers.find((t) => t.key === editingKey) : undefined;
  const totalTempAmount = tempVouchers.reduce((sum, t) => sum + t.amount, 0);

  function update<K extends keyof FormState>(key: K, value: FormState[K]) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  /** Switching charge mode clears the other side, so only one ever reaches the server. */
  function setChargeMode(mode: ChargeMode) {
    setForm((f) => ({ ...f, chargeMode: mode, projectId: "", costCenterId: "" }));
  }

  /**
   * Changing the date drops any manual fiscal-year pick, letting the date decide again. The
   * fiscal year is a function of the date, so a stale override would put the voucher in a year
   * that no longer contains it — which the server rejects anyway.
   */
  function setVoucherDate(date: string) {
    setForm((f) => ({ ...f, voucherDate: date, fiscalYearId: "" }));
  }

  const inputClass =
    "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";

  const chargeTabClass = (active: boolean) =>
    `px-3 py-1.5 text-xs rounded-lg border transition-colors ${
      active
        ? "bg-primary-50 border-primary-300 text-primary-700 font-medium"
        : "bg-white border-gray-200 text-gray-500 hover:bg-gray-50"
    }`;

  const noMoneyAccounts = debitOptions.length === 0 || creditOptions.length === 0;

  const debitDropdown = (
    <SearchableDropdown
      label={`${debitFieldLabel} (Debit)`}
      required
      options={debitOptions}
      loading={loadingAccounts}
      value={form.debitAccountId}
      onChange={(v) => update("debitAccountId", v ?? "")}
      placeholder="Select account"
      searchPlaceholder="Search by code or name…"
      noResultsMessage={debitOptions.length === 0 ? "No eligible accounts set up yet" : "No results found"}
    />
  );

  const creditDropdown = (
    <SearchableDropdown
      label={`${creditFieldLabel} (Credit)`}
      required
      options={creditOptions}
      loading={loadingAccounts}
      value={form.creditAccountId}
      onChange={(v) => update("creditAccountId", v ?? "")}
      placeholder="Select account"
      searchPlaceholder="Search by code or name…"
      noResultsMessage={creditOptions.length === 0 ? "No eligible accounts set up yet" : "No results found"}
    />
  );

  return (
    <div className="p-6 max-w-4xl">
      <div className="flex items-center gap-3 mb-4">
        <button
          onClick={() => navigate(-1)}
          className="w-8 h-8 flex items-center justify-center rounded-lg border border-gray-200 hover:bg-gray-50 text-gray-500"
        >
          <ArrowLeft size={15} />
        </button>
        <h1 className="text-xl font-bold text-gray-900">
          {isEdit ? `Edit ${theme.label}` : theme.label}
        </h1>
      </div>

      {/* Colour-coded banner: red for money out, green for money in */}
      <div className={`relative overflow-hidden border rounded-xl px-5 py-4 mb-4 ${theme.banner}`}>
        <span className={`absolute left-0 top-0 bottom-0 w-1.5 ${theme.bar}`} />
        <div className="flex items-center justify-between pl-2">
          <div>
            <p className="text-sm font-semibold text-gray-900">{theme.meaning}</p>
            <p className="text-xs text-gray-500 mt-0.5">
              {voucherNumber ?? "Voucher number is generated when you save"}
            </p>
          </div>
          <span className={`text-lg font-bold tracking-wider px-3 py-1 rounded-lg border bg-white ${theme.chip}`}>
            {theme.code}
          </span>
        </div>
      </div>

      {error && (
        <div className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
          {error}
        </div>
      )}

      <div className="bg-white border border-gray-200 rounded-xl p-5 space-y-5">
        {/* Header details */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">
              Date <span className="text-red-500">*</span>
            </label>
            <DateInput
              className={inputClass}
              value={form.voucherDate}
              onChange={(e) => setVoucherDate(e.target.value)}
            />
            {noFiscalYearForDate && (
              <p className="text-[11px] text-amber-600 mt-1">
                No open fiscal year covers this date.
              </p>
            )}
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Voucher Ref. No.</label>
            <input
              className={inputClass}
              value={form.referenceNumber}
              onChange={(e) => update("referenceNumber", e.target.value)}
              maxLength={50}
              placeholder="Number on the paper voucher"
            />
            <p className="text-[11px] text-gray-400 mt-1">
              Leave blank if there is no physical voucher.
            </p>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">{labels.nameLabel}</label>
            <input
              className={inputClass}
              value={form.name}
              onChange={(e) => update("name", e.target.value)}
              placeholder={labels.namePlaceholder}
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Section</label>
            <input
              className={inputClass}
              value={form.section}
              onChange={(e) => update("section", e.target.value)}
              placeholder="Department or section"
            />
          </div>
        </div>

        {/* Charge target — a project, or a cost center for head-office / overhead spend */}
        <div className="border border-gray-100 bg-gray-50/60 rounded-lg p-4">
          <div className="flex flex-wrap items-center gap-2 mb-3">
            <label className="text-xs text-gray-500 mr-1">
              Charge To <span className="text-red-500">*</span>
            </label>
            <button
              type="button"
              className={chargeTabClass(form.chargeMode === "project")}
              onClick={() => setChargeMode("project")}
            >
              Project
            </button>
            <button
              type="button"
              className={chargeTabClass(form.chargeMode === "overhead")}
              onClick={() => setChargeMode("overhead")}
            >
              Head Office / Overhead
            </button>
          </div>

          {form.chargeMode === "project" ? (
            <SearchableDropdown
              label="Project"
              required
              options={projects}
              loading={loadingProjects}
              value={form.projectId}
              onChange={(v) => update("projectId", v ?? "")}
              placeholder="Select project"
              searchPlaceholder="Search by name or code…"
              noResultsMessage="No projects found"
            />
          ) : (
            <SearchableDropdown
              label="Cost Center"
              required
              options={overheadCostCenters}
              loading={loadingCostCenters}
              value={form.costCenterId}
              onChange={(v) => update("costCenterId", v ?? "")}
              placeholder="Select cost center"
              searchPlaceholder="Search by name or code…"
              noResultsMessage="No non-project cost centers set up yet"
            />
          )}

          <p className="text-[11px] text-gray-400 mt-2">
            {form.chargeMode === "project"
              ? "The amount is attributed to this project's cost center and appears in its financial summary once posted."
              : "Use this for costs that belong to no single project, such as head-office running costs or bank charges."}
          </p>
        </div>

        {/* Accounts — always debit first, matching the order they appear in the ledger */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-1">
          {debitDropdown}
          {creditDropdown}
        </div>

        {isOwnAccountTransfer && (
          <div className="flex items-start gap-2 text-xs text-sky-800 bg-sky-50 border border-sky-200 rounded-lg px-3 py-2">
            <Info size={14} className="mt-0.5 shrink-0" />
            <span>
              Both sides must be your own cash or bank accounts. Nothing enters or leaves the
              organisation, so this is counted as neither a receipt nor a payment.
            </span>
          </div>
        )}

        {noMoneyAccounts && !loadingAccounts && (
          <div className="flex items-start gap-2 text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2">
            <Info size={14} className="mt-0.5 shrink-0" />
            <span>
              No eligible accounts are available for one side of this voucher. Add a cash or bank
              account under{" "}
              <button onClick={() => navigate("/accounts/chart-of-accounts")} className="underline font-medium">
                Chart of Accounts
              </button>{" "}
              first — money can only move through an asset account.
            </span>
          </div>
        )}

        {form.debitAccountId !== "" && form.debitAccountId === form.creditAccountId && (
          <p className="text-xs text-red-500">Debit and credit accounts must be different.</p>
        )}

        {/* Amount + description */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">
              Amount <span className="text-red-500">*</span>
            </label>
            <input
              type="number"
              min="0"
              step="0.01"
              className={`${inputClass} text-right tabular-nums`}
              value={form.amount}
              onChange={(e) => update("amount", e.target.value)}
              placeholder="0.00"
            />
          </div>
          <div className="md:col-span-2">
            <label className="text-xs text-gray-500 mb-1 block">Amount in Words</label>
            <div className="w-full border border-gray-100 bg-gray-50 rounded-lg px-3 py-2 text-sm text-gray-600 min-h-[38px]">
              {words || <span className="text-gray-300">Enter an amount</span>}
            </div>
          </div>
        </div>

        <div>
          <label className="text-xs text-gray-500 mb-1 block">
            Description / Purpose <span className="text-red-500">*</span>
          </label>
          <textarea
            rows={3}
            className={inputClass}
            value={form.description}
            onChange={(e) => update("description", e.target.value)}
            placeholder="What is this payment or receipt for?"
          />
        </div>

        {/* Preparation details */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 pt-1 border-t border-gray-100">
          <div className="pt-4">
            {/* Creatable: "Received By" is often someone outside the company, such as a
                supplier's representative, so a typed name has to remain possible. */}
            <SearchableDropdown
              label="Prepared By"
              options={withTypedValue(employees, form.preparedBy)}
              loading={loadingEmployees}
              value={form.preparedBy}
              onChange={(v) => update("preparedBy", v ?? "")}
              creatable
              onCreate={(name) => update("preparedBy", name)}
              placeholder="Select employee"
              searchPlaceholder="Search employee or type a name…"
              noResultsMessage="No employees found"
            />
          </div>
          <div className="pt-4">
            <SearchableDropdown
              label="Received By"
              options={withTypedValue(employees, form.receivedBy)}
              loading={loadingEmployees}
              value={form.receivedBy}
              onChange={(v) => update("receivedBy", v ?? "")}
              creatable
              onCreate={(name) => update("receivedBy", name)}
              placeholder="Select employee"
              searchPlaceholder="Search employee or type a name…"
              noResultsMessage="No employees found"
            />
          </div>
          <div className="pt-4">
            <SearchableDropdown
              label="Fiscal Year"
              options={fiscalYears}
              value={fiscalYearId}
              onChange={(v) => update("fiscalYearId", v ?? "")}
              placeholder="Set from the voucher date"
              searchPlaceholder="Search fiscal year…"
            />
            {fiscalYearIsAutomatic && (
              <p className="text-[11px] text-gray-400 mt-1">Selected from the voucher date.</p>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="flex flex-col gap-2 pt-4 border-t border-gray-100">
          <div className="flex items-center justify-between gap-4">
            {isNew && !editingKey && (
              <label className="flex items-center gap-2 text-xs text-gray-600 cursor-pointer select-none">
                <input
                  type="checkbox"
                  checked={keepFormData}
                  onChange={(e) => setKeepFormData(e.target.checked)}
                  className="h-3.5 w-3.5 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                Keep form details for the next voucher
              </label>
            )}
            <div className="flex justify-end gap-2 ml-auto">
              <button
                onClick={() => navigate(-1)}
                className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
              >
                Cancel
              </button>
              {isNew && (
                <button
                  onClick={handleSaveAndContinue}
                  disabled={!canSave || saveMutation.isPending || batchSaveMutation.isPending}
                  className="px-4 py-2 text-sm border border-primary-300 text-primary-700 rounded-lg hover:bg-primary-50 disabled:opacity-50"
                >
                  {editingKey ? "Update Entry" : "Save and Continue"}
                </button>
              )}
              <button
                onClick={() => { setError(""); saveMutation.mutate(); }}
                disabled={!canSave || saveMutation.isPending || (isNew && hasTempVouchers)}
                className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
              >
                {saveMutation.isPending ? "Saving…" : isEdit ? "Update Voucher" : "Save Voucher"}
              </button>
            </div>
          </div>
          {isNew && editingTemp && (
            <p className="flex items-center gap-2 text-[11px] text-sky-800 bg-sky-50 border border-sky-200 rounded-lg px-3 py-1.5">
              Editing {editingTemp.tempRef} — click "Update Entry" to save your changes, or
              <button onClick={handleCancelEdit} className="underline font-medium">
                cancel edit
              </button>
              .
            </p>
          )}
          {isNew && hasTempVouchers && !editingKey && (
            <p className="text-[11px] text-amber-600">
              Save Voucher is disabled while temporary vouchers are pending. Use the Save Voucher
              button under the temporary vouchers table to persist them to the database.
            </p>
          )}
        </div>
      </div>

      {/* Pending temporary vouchers collected by "Save and Continue" */}
      {isNew && hasTempVouchers && (
        <div className="mt-6 bg-white border border-gray-200 rounded-xl">
          <div className="px-5 py-4 border-b border-gray-100 flex items-center justify-between">
            <div>
              <h2 className="text-sm font-semibold text-gray-900">Temporary Vouchers</h2>
              <p className="text-xs text-gray-400 mt-0.5">
                {tempVouchers.length} pending · total {formatTaka(totalTempAmount)}
              </p>
            </div>
            <button
              onClick={() => setConfirmClearOpen(true)}
              className="text-xs text-red-600 hover:text-red-700 font-medium"
            >
              Clear All
            </button>
          </div>

          <div className="flex items-start gap-2 text-xs text-amber-800 bg-amber-50 border-b border-amber-200 px-5 py-2.5">
            <AlertTriangle size={14} className="mt-0.5 shrink-0" />
            <span>
              This data not saved in database yet, to save on database click on 'Save voucher' button
            </span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-xs text-gray-500 border-b border-gray-100 bg-gray-50/60">
                  <th className="px-4 py-2.5 font-medium">SL</th>
                  <th className="px-4 py-2.5 font-medium">Voucher No.</th>
                  <th className="px-4 py-2.5 font-medium">Date</th>
                  <th className="px-4 py-2.5 font-medium">{debitFieldLabel}</th>
                  <th className="px-4 py-2.5 font-medium">{creditFieldLabel}</th>
                  <th className="px-4 py-2.5 font-medium">Description</th>
                  <th className="px-4 py-2.5 font-medium text-right">Debit</th>
                  <th className="px-4 py-2.5 font-medium text-right">Credit</th>
                  <th className="px-4 py-2.5 font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {tempVouchers.map((temp, i) => {
                  // A Debit voucher posts the amount to the debit side, a Credit voucher to the
                  // credit side; a Contra moves it between own accounts, so both sides show it.
                  const debitSide = voucherType !== "Credit" ? temp.amount : null;
                  const creditSide = voucherType === "Credit" || voucherType === "Contra" ? temp.amount : null;
                  return (
                    <tr key={temp.key} className="border-b border-gray-50 hover:bg-gray-50/50">
                      <td className="px-4 py-2.5 text-gray-400 tabular-nums">{i + 1}</td>
                      <td className="px-4 py-2.5 font-medium text-gray-700">{temp.tempRef}</td>
                      <td className="px-4 py-2.5 text-gray-600">{formatDate(temp.voucherDate)}</td>
                      <td className="px-4 py-2.5 text-gray-700">{temp.debitAccountName}</td>
                      <td className="px-4 py-2.5 text-gray-700">{temp.creditAccountName}</td>
                      <td className="px-4 py-2.5">
                        <span className="block max-w-[220px] truncate text-gray-600" title={temp.description}>
                          {temp.description}
                        </span>
                      </td>
                      <td className="px-4 py-2.5 text-right tabular-nums text-gray-700">
                        {debitSide != null ? formatTaka(debitSide) : "—"}
                      </td>
                      <td className="px-4 py-2.5 text-right tabular-nums text-gray-700">
                        {creditSide != null ? formatTaka(creditSide) : "—"}
                      </td>
                      <td className="px-4 py-2.5">
                        <div className="flex items-center gap-3">
                          <button
                            onClick={() => handleEditTemp(temp)}
                            className="text-gray-400 hover:text-primary-600"
                            title="Edit"
                          >
                            <Pencil size={14} />
                          </button>
                          <button
                            onClick={() => handleRemoveTemp(temp.key)}
                            className="text-gray-400 hover:text-red-600"
                            title="Remove"
                          >
                            <Trash2 size={14} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          <div className="flex items-center justify-between gap-4 px-5 py-4 border-t border-gray-100 bg-gray-50/60 rounded-b-xl">
            <div className="min-w-0">
              {batchError ? (
                <p className="text-xs text-red-600">{batchError}</p>
              ) : (
                <p className="text-xs text-gray-400">
                  {tempVouchers.length} temporary {tempVouchers.length === 1 ? "voucher" : "vouchers"} not yet saved to
                  the database.
                </p>
              )}
            </div>
            <button
              onClick={() => { setBatchError(""); batchSaveMutation.mutate(); }}
              disabled={batchSaveMutation.isPending || editingKey !== null}
              className="shrink-0 px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {batchSaveMutation.isPending ? "Saving…" : "Save Voucher"}
            </button>
          </div>
        </div>
      )}

      <ConfirmDialog
        open={confirmClearOpen}
        title="Clear temporary vouchers"
        message={`Remove all ${tempVouchers.length} pending temporary voucher${
          tempVouchers.length === 1 ? "" : "s"
        }? Nothing has been saved to the database yet.`}
        confirmLabel="Clear All"
        cancelLabel="Keep Vouchers"
        tone="danger"
        onConfirm={handleClearAll}
        onCancel={() => setConfirmClearOpen(false)}
      />
    </div>
  );
}
