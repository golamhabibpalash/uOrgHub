/** Form model and arithmetic for bill line items, shared by Bills and BillLineItemsEditor. */

export interface BillLineForm {
  /** Client-only React key — never sent to the API. */
  uid: number;
  description: string;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
  lineOrder: number;
  taxRateId: string;
  expenseAccountId: string;
  costCenterId: string;
}

let nextUid = 1;
export const newLineUid = () => nextUid++;

export const emptyBillLine = (lineOrder = 1): BillLineForm => ({
  uid: newLineUid(),
  description: "",
  quantity: 1,
  unitPrice: 0,
  discountPercent: 0,
  lineOrder,
  taxRateId: "",
  expenseAccountId: "",
  costCenterId: "",
});

export interface BillLineAmounts {
  gross: number;
  discount: number;
  net: number;
  tax: number;
  total: number;
}

/** Same arithmetic as CreateBillHandler: tax is charged on the discounted amount. */
export function calcBillLine(line: BillLineForm, taxRatePercent: number): BillLineAmounts {
  const gross = line.quantity * line.unitPrice;
  const discount = gross * (line.discountPercent / 100);
  const net = gross - discount;
  const tax = net * (taxRatePercent / 100);
  return { gross, discount, net, tax, total: net + tax };
}

/** Per-line problems matching CreateBillLineValidator, keyed by field. */
export function billLineErrors(line: BillLineForm): Partial<Record<keyof BillLineForm, string>> {
  const errors: Partial<Record<keyof BillLineForm, string>> = {};
  if (!line.description.trim()) errors.description = "Description is required";
  if (!line.expenseAccountId) errors.expenseAccountId = "Expense account is required";
  if (!(line.quantity > 0)) errors.quantity = "Must be greater than 0";
  if (line.unitPrice < 0) errors.unitPrice = "Cannot be negative";
  if (line.discountPercent < 0 || line.discountPercent > 100) errors.discountPercent = "0–100";
  return errors;
}

export const money = (n: number) => n.toLocaleString("en-BD", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
