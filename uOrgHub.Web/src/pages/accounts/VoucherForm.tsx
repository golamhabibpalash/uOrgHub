import { useMemo, useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Info } from "lucide-react";
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
  UpdateVoucherPayload,
  VoucherType,
} from "../../api/accounts";
import { voucherThemes } from "../../components/accounts/voucherTheme";
import { amountInWords } from "../../utils/format";
import { extractApiError } from "../../utils/apiError";

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

interface FormState {
  voucherDate: string;
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

  return (
    <VoucherFormFields
      key={id ?? "new"}
      voucherId={id}
      voucherNumber={voucher?.voucherNumber}
      voucherType={voucher?.voucherType ?? (searchParams.get("type") === "Credit" ? "Credit" : "Debit")}
      initialForm={
        voucher
          ? {
              voucherDate: voucher.voucherDate?.split("T")[0] ?? "",
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

  const [form, setForm] = useState<FormState>(initialForm);
  const [error, setError] = useState("");

  const { moneyOptions, partyOptions, moneyIsOnDebitSide, moneyFieldLabel, isLoading: loadingAccounts } =
    useVoucherAccountOptions(voucherType);
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

  const isDebitVoucher = voucherType === "Debit";
  const theme = voucherThemes[voucherType];

  // The server says which side holds the money account for this voucher type; the form only
  // decides which of the two dropdowns writes to debitAccountId and which to creditAccountId.
  type AccountField = "debitAccountId" | "creditAccountId";
  const moneyField: AccountField = moneyIsOnDebitSide ? "debitAccountId" : "creditAccountId";
  const partyField: AccountField = moneyIsOnDebitSide ? "creditAccountId" : "debitAccountId";

  const partyFieldLabel = isDebitVoucher
    ? "Party Account — Expense / Payable (Debit)"
    : "Party Account — Income / Investor / Receivable (Credit)";

  const labels = isDebitVoucher
    ? { nameLabel: "Paid To", namePlaceholder: "Name of person or party receiving the money" }
    : { nameLabel: "Received From", namePlaceholder: "Name of person or party paying the money" };

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

  const moneyDropdown = (
    <SearchableDropdown
      label={`${moneyFieldLabel} — Cash / Bank / Asset (${moneyIsOnDebitSide ? "Debit" : "Credit"})`}
      required
      options={moneyOptions}
      loading={loadingAccounts}
      value={form[moneyField]}
      onChange={(v) => update(moneyField, v ?? "")}
      placeholder="Select account"
      searchPlaceholder="Search by code or name…"
      noResultsMessage={moneyOptions.length === 0 ? "No asset accounts set up yet" : "No results found"}
    />
  );

  const partyDropdown = (
    <SearchableDropdown
      label={partyFieldLabel}
      required
      options={partyOptions}
      loading={loadingAccounts}
      value={form[partyField]}
      onChange={(v) => update(partyField, v ?? "")}
      placeholder="Select account"
      searchPlaceholder="Search by code or name…"
      noResultsMessage="No results found"
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
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">
              Date <span className="text-red-500">*</span>
            </label>
            <input
              type="date"
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

        {/* Accounts — money side and party side, ordered debit-first to match the ledger */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-1">
          {moneyIsOnDebitSide ? moneyDropdown : partyDropdown}
          {moneyIsOnDebitSide ? partyDropdown : moneyDropdown}
        </div>

        {moneyOptions.length === 0 && !loadingAccounts && (
          <div className="flex items-start gap-2 text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2">
            <Info size={14} className="mt-0.5 shrink-0" />
            <span>
              No asset accounts are available for {moneyFieldLabel.toLowerCase()}. Add a cash or bank
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
        <div className="flex justify-end gap-2 pt-4 border-t border-gray-100">
          <button
            onClick={() => navigate(-1)}
            className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            onClick={() => { setError(""); saveMutation.mutate(); }}
            disabled={!canSave || saveMutation.isPending}
            className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
          >
            {saveMutation.isPending ? "Saving…" : isEdit ? "Update Voucher" : "Save Voucher"}
          </button>
        </div>
      </div>
    </div>
  );
}
