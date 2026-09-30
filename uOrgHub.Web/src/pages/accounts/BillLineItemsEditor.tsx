import { Copy, Plus, Trash2 } from "lucide-react";
import SearchableDropdown, { type SelectOption } from "../../components/shared/SearchableDropdown";
import { billLineErrors, calcBillLine, emptyBillLine, money, newLineUid, type BillLineForm } from "./billLine";

const inputBase =
  "w-full border rounded-lg px-2.5 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";
const inputCls = (hasError: boolean) =>
  `${inputBase} ${hasError ? "border-red-300 bg-red-50/40" : "border-gray-200"}`;
const numCls = (hasError: boolean) => `${inputCls(hasError)} text-right tabular-nums`;

function FieldLabel({ children }: { children: React.ReactNode }) {
  return <label className="block text-[11px] font-medium uppercase tracking-wide text-gray-400 mb-1">{children}</label>;
}

interface BillLineItemsEditorProps {
  lines: BillLineForm[];
  onChange: (lines: BillLineForm[]) => void;
  taxRateOptions: SelectOption[];
  taxRateById: Map<string, number>;
  accountOptions: SelectOption[];
  costCenterOptions: SelectOption[];
  /** Only show validation highlights after the user has tried to save. */
  showErrors: boolean;
}

/**
 * Card-per-line editor for bill line items. Each line reads as two rows — what was bought and where
 * it is booked, then the quantities and money — so every field gets a readable width instead of
 * being squeezed into one wide table row.
 */
export default function BillLineItemsEditor({
  lines,
  onChange,
  taxRateOptions,
  taxRateById,
  accountOptions,
  costCenterOptions,
  showErrors,
}: BillLineItemsEditorProps) {
  const update = (idx: number, patch: Partial<BillLineForm>) =>
    onChange(lines.map((l, i) => (i === idx ? { ...l, ...patch } : l)));

  const remove = (idx: number) => onChange(lines.filter((_, i) => i !== idx));

  const duplicate = (idx: number) =>
    onChange([...lines.slice(0, idx + 1), { ...lines[idx], uid: newLineUid() }, ...lines.slice(idx + 1)]);

  const add = () => onChange([...lines, emptyBillLine(lines.length + 1)]);

  const num = (v: string) => {
    const n = parseFloat(v);
    return Number.isFinite(n) ? n : 0;
  };

  return (
    <div className="space-y-3">
      {lines.map((line, idx) => {
        const errors = showErrors ? billLineErrors(line) : {};
        const amounts = calcBillLine(line, taxRateById.get(line.taxRateId) ?? 0);
        return (
          <div key={line.uid} className="group relative border border-gray-200 rounded-xl bg-white hover:border-gray-300 transition-colors">
            <div className="flex items-center justify-between px-4 py-2 border-b border-gray-100 bg-gray-50/60 rounded-t-xl">
              <span className="text-xs font-medium text-gray-500">Line {idx + 1}</span>
              <div className="flex items-center gap-1">
                <button
                  type="button"
                  onClick={() => duplicate(idx)}
                  title="Duplicate line"
                  className="p-1.5 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-md"
                >
                  <Copy size={13} />
                </button>
                <button
                  type="button"
                  onClick={() => remove(idx)}
                  disabled={lines.length === 1}
                  title={lines.length === 1 ? "A bill needs at least one line" : "Remove line"}
                  className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-md disabled:opacity-30 disabled:hover:bg-transparent disabled:hover:text-gray-400"
                >
                  <Trash2 size={13} />
                </button>
              </div>
            </div>

            <div className="p-4 space-y-3">
              <div className="grid grid-cols-1 md:grid-cols-12 gap-3">
                <div className="md:col-span-7">
                  <FieldLabel>Description *</FieldLabel>
                  <input
                    className={inputCls(Boolean(errors.description))}
                    value={line.description}
                    placeholder="What is this charge for?"
                    maxLength={500}
                    onChange={(e) => update(idx, { description: e.target.value })}
                  />
                  {errors.description && <p className="text-[11px] text-red-500 mt-0.5">{errors.description}</p>}
                </div>
                <div className="md:col-span-5">
                  <FieldLabel>Expense account *</FieldLabel>
                  <SearchableDropdown
                    options={accountOptions}
                    value={line.expenseAccountId}
                    onChange={(v) => update(idx, { expenseAccountId: v ?? "" })}
                    placeholder="Select account"
                    searchPlaceholder="Search accounts..."
                    error={errors.expenseAccountId}
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-12 gap-3 items-start">
                <div className="md:col-span-1">
                  <FieldLabel>Qty</FieldLabel>
                  <input
                    type="number"
                    min={0}
                    step="any"
                    className={numCls(Boolean(errors.quantity))}
                    value={line.quantity}
                    onChange={(e) => update(idx, { quantity: num(e.target.value) })}
                  />
                </div>
                <div className="md:col-span-2">
                  <FieldLabel>Unit price</FieldLabel>
                  <input
                    type="number"
                    min={0}
                    step="0.01"
                    placeholder="0.00"
                    className={numCls(Boolean(errors.unitPrice))}
                    value={line.unitPrice || ""}
                    onChange={(e) => update(idx, { unitPrice: num(e.target.value) })}
                  />
                </div>
                <div className="md:col-span-1">
                  <FieldLabel>Disc %</FieldLabel>
                  <input
                    type="number"
                    min={0}
                    max={100}
                    step="any"
                    placeholder="0"
                    className={numCls(Boolean(errors.discountPercent))}
                    value={line.discountPercent || ""}
                    onChange={(e) => update(idx, { discountPercent: num(e.target.value) })}
                  />
                </div>
                <div className="md:col-span-2">
                  <FieldLabel>Tax</FieldLabel>
                  <SearchableDropdown
                    options={taxRateOptions}
                    value={line.taxRateId}
                    onChange={(v) => update(idx, { taxRateId: v ?? "" })}
                    placeholder="No tax"
                    searchPlaceholder="Search tax rates..."
                  />
                </div>
                <div className="col-span-2 md:col-span-3">
                  <FieldLabel>Cost center</FieldLabel>
                  <SearchableDropdown
                    options={costCenterOptions}
                    value={line.costCenterId}
                    onChange={(v) => update(idx, { costCenterId: v ?? "" })}
                    placeholder="Same as bill"
                    searchPlaceholder="Search cost centers..."
                  />
                </div>
                <div className="col-span-2 md:col-span-3 md:text-right">
                  <FieldLabel>Amount</FieldLabel>
                  <p className="text-base font-semibold text-gray-900 tabular-nums py-1">{money(amounts.total)}</p>
                  {(amounts.discount > 0 || amounts.tax > 0) && (
                    <p className="text-[11px] text-gray-400 tabular-nums">
                      {money(amounts.gross)}
                      {amounts.discount > 0 && <> − {money(amounts.discount)} disc</>}
                      {amounts.tax > 0 && <> + {money(amounts.tax)} tax</>}
                    </p>
                  )}
                </div>
              </div>
            </div>
          </div>
        );
      })}

      <button
        type="button"
        onClick={add}
        className="w-full flex items-center justify-center gap-1.5 border-2 border-dashed border-gray-200 rounded-xl py-2.5 text-sm text-gray-500 hover:border-primary-400 hover:text-primary-600 hover:bg-primary-50/40 transition-colors"
      >
        <Plus size={15} /> Add line
      </button>
    </div>
  );
}
