import { VoucherType } from "../../api/accounts";

/**
 * Colour identity for the two voucher types, kept in one place so the entry screen,
 * form, list, detail page and printed voucher all read the same way.
 *
 * Debit Voucher  = money paid out    → red
 * Credit Voucher = money received    → green
 *
 * Class names are written out in full: Tailwind only picks up literal strings.
 */
export interface VoucherTheme {
  label: string;
  code: "DR" | "CR";
  meaning: string;
  effect: string;
  /** Solid fill for icon tiles. */
  solid: string;
  /** Small pill used in lists and headers. */
  badge: string;
  /** Bordered chip used for the DR/CR marker. */
  chip: string;
  /** Tinted banner behind form and detail headers. */
  banner: string;
  /** Accent bar down the side of a panel. */
  bar: string;
  /** Hover treatment for the selection cards. */
  hover: string;
  /** Printed voucher: title box and heavy rules. */
  printBorder: string;
  /** Printed voucher: table header tint. */
  printHeader: string;
  /** Printed voucher: title text. */
  printText: string;
}

export const voucherThemes: Record<VoucherType, VoucherTheme> = {
  Debit: {
    label: "Debit Voucher",
    code: "DR",
    meaning: "Money paid out",
    effect: "Cash or bank goes down",
    solid: "bg-rose-500",
    badge: "bg-rose-50 text-rose-700",
    chip: "text-rose-700 bg-rose-50 border-rose-300",
    banner: "bg-rose-50 border-rose-200",
    bar: "bg-rose-500",
    hover: "hover:border-rose-400 hover:ring-rose-100",
    printBorder: "border-rose-700",
    printHeader: "bg-rose-50",
    printText: "text-rose-700",
  },
  Credit: {
    label: "Credit Voucher",
    code: "CR",
    meaning: "Money received",
    effect: "Cash or bank goes up",
    solid: "bg-emerald-600",
    badge: "bg-emerald-50 text-emerald-700",
    chip: "text-emerald-700 bg-emerald-50 border-emerald-300",
    banner: "bg-emerald-50 border-emerald-200",
    bar: "bg-emerald-600",
    hover: "hover:border-emerald-400 hover:ring-emerald-100",
    printBorder: "border-emerald-700",
    printHeader: "bg-emerald-50",
    printText: "text-emerald-700",
  },
};

export const voucherTheme = (type: VoucherType): VoucherTheme => voucherThemes[type];
