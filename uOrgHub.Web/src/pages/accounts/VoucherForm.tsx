import { useMemo, useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Info } from "lucide-react";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import {
  useChartOfAccountsLookup,
  useFiscalYearLookup,
  useVoucherCashAccountLookup,
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

interface FormState {
  voucherDate: string;
  fiscalYearId: string;
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

  const { options: allAccounts, isLoading: loadingAccounts } = useChartOfAccountsLookup();
  const { options: cashAccounts, isLoading: loadingCash } = useVoucherCashAccountLookup();
  const { options: fiscalYears } = useFiscalYearLookup();

  const isDebitVoucher = voucherType === "Debit";
  const theme = voucherThemes[voucherType];

  // A Debit Voucher pays money out, so cash/bank sits on the credit side.
  // A Credit Voucher takes money in, so cash/bank sits on the debit side.
  const labels = isDebitVoucher
    ? {
        nameLabel: "Paid To",
        namePlaceholder: "Name of person or party receiving the money",
        debit: { label: "Expense / Party Account (Debit)", options: allAccounts, loading: loadingAccounts },
        credit: { label: "Paid From — Cash / Bank (Credit)", options: cashAccounts, loading: loadingCash },
        receivedByLabel: "Received By",
      }
    : {
        nameLabel: "Received From",
        namePlaceholder: "Name of person or party paying the money",
        debit: { label: "Received Into — Cash / Bank (Debit)", options: cashAccounts, loading: loadingCash },
        credit: { label: "Income / Party Account (Credit)", options: allAccounts, loading: loadingAccounts },
        receivedByLabel: "Received By",
      };

  const amountValue = Number(form.amount) || 0;
  const words = useMemo(() => (amountValue > 0 ? amountInWords(amountValue) : ""), [amountValue]);

  const canSave =
    form.voucherDate !== "" &&
    form.description.trim() !== "" &&
    form.debitAccountId !== "" &&
    form.creditAccountId !== "" &&
    form.debitAccountId !== form.creditAccountId &&
    amountValue > 0;

  const saveMutation = useMutation({
    mutationFn: () => {
      // Voucher type is fixed at creation — the update endpoint does not accept it.
      const common: UpdateVoucherPayload = {
        voucherDate: form.voucherDate,
        fiscalYearId: form.fiscalYearId || undefined,
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

  const inputClass =
    "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";

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
              onChange={(e) => update("voucherDate", e.target.value)}
            />
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

        {/* Accounts */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-1">
          <SearchableDropdown
            label={labels.debit.label}
            required
            options={labels.debit.options}
            loading={labels.debit.loading}
            value={form.debitAccountId}
            onChange={(v) => update("debitAccountId", v ?? "")}
            placeholder="Select account"
            searchPlaceholder="Search by code or name…"
            noResultsMessage={
              !isDebitVoucher && cashAccounts.length === 0
                ? "No cash/bank accounts set up yet"
                : "No results found"
            }
          />
          <SearchableDropdown
            label={labels.credit.label}
            required
            options={labels.credit.options}
            loading={labels.credit.loading}
            value={form.creditAccountId}
            onChange={(v) => update("creditAccountId", v ?? "")}
            placeholder="Select account"
            searchPlaceholder="Search by code or name…"
            noResultsMessage={
              isDebitVoucher && cashAccounts.length === 0
                ? "No cash/bank accounts set up yet"
                : "No results found"
            }
          />
        </div>

        {cashAccounts.length === 0 && !loadingCash && (
          <div className="flex items-start gap-2 text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2">
            <Info size={14} className="mt-0.5 shrink-0" />
            <span>
              No cash or bank accounts are available yet. Set one up under{" "}
              <button
                onClick={() => navigate("/accounts/bank-accounts")}
                className="underline font-medium"
              >
                Bank Accounts
              </button>{" "}
              first — every voucher needs one side to be a cash or bank account.
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
            <label className="text-xs text-gray-500 mb-1 block">Prepared By</label>
            <input
              className={inputClass}
              value={form.preparedBy}
              onChange={(e) => update("preparedBy", e.target.value)}
              placeholder="Name"
            />
          </div>
          <div className="pt-4">
            <label className="text-xs text-gray-500 mb-1 block">{labels.receivedByLabel}</label>
            <input
              className={inputClass}
              value={form.receivedBy}
              onChange={(e) => update("receivedBy", e.target.value)}
              placeholder="Name"
            />
          </div>
          <div className="pt-4">
            <SearchableDropdown
              label="Fiscal Year"
              options={fiscalYears}
              value={form.fiscalYearId}
              onChange={(v) => update("fiscalYearId", v ?? "")}
              placeholder="Optional"
              searchPlaceholder="Search fiscal year…"
            />
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
