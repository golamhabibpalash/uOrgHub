import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle2 } from "lucide-react";
import { getVoucherSmsPreview, postVoucher, sendVoucherSms, type Voucher } from "../../api/accounts";
import Modal from "../shared/Modal";
import PartySmsComposer from "../sms/PartySmsComposer";
import { extractApiError } from "../../utils/apiError";
import { formatTaka } from "../../utils/format";

interface Props {
  voucher: Voucher;
  onClose: () => void;
}

const newKey = () => (typeof crypto !== "undefined" && "randomUUID" in crypto ? crypto.randomUUID() : `${Date.now()}-${Math.random().toString(36).slice(2)}`);

/**
 * Post confirmation for a voucher marked "Send SMS when posted": the message is built from the
 * voucher as it stands now, so the poster confirms the final figures, can edit the text, or untick
 * the SMS. The voucher is posted first; an SMS that then fails doesn't undo the posting.
 */
export default function VoucherPostDialog({ voucher, onClose }: Props) {
  const qc = useQueryClient();
  const preview = useQuery({
    queryKey: ["voucher-sms-preview", voucher.id],
    queryFn: () => getVoucherSmsPreview(voucher.id),
    staleTime: 0,
  });
  const p = preview.data?.data?.data ?? undefined;
  const [edited, setEdited] = useState<string | null>(null);
  const message = edited ?? p?.message ?? "";
  const [sendSms, setSendSms] = useState(true);
  const [key] = useState(newKey);
  const [error, setError] = useState<string | null>(null);
  const willSend = !!p?.canSend && sendSms && message.trim() !== "";

  const mutation = useMutation({
    mutationFn: async () => {
      await postVoucher(voucher.id);
      if (willSend) {
        // Posted already — an SMS failure is reported by the API client's toast, not rethrown.
        try { await sendVoucherSms(voucher.id, message, key); } catch { /* toast shown by client */ }
      }
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["voucher", voucher.id] });
      qc.invalidateQueries({ queryKey: ["vouchers"] });
      qc.invalidateQueries({ queryKey: ["voucher-journal-entry", voucher.id] });
      qc.invalidateQueries({ queryKey: ["journal-entries"] });
      onClose();
    },
    onError: (e) => setError(extractApiError(e)),
  });

  return (
    <Modal title={`Post ${voucher.voucherNumber}`} open onClose={() => !mutation.isPending && onClose()} size="lg">
      <div className="space-y-4">
        <p className="text-sm text-gray-600">
          Posts <strong>{formatTaka(voucher.amount)}</strong> to the ledger{voucher.name ? <> — {voucher.voucherType === "Debit" ? "paid to" : "received from"} <strong>{voucher.name}</strong></> : null}.
        </p>

        <div className="rounded-lg border border-gray-200 p-3 space-y-3">
          <label className="inline-flex items-center gap-2 text-sm font-medium text-gray-900">
            <input type="checkbox" className="h-4 w-4 accent-primary-500" checked={!!p?.canSend && sendSms}
              disabled={!p?.canSend || mutation.isPending} onChange={(e) => setSendSms(e.target.checked)} />
            Send SMS to the party
          </label>
          {(sendSms || !p?.canSend) && (
            <PartySmsComposer preview={p} loading={preview.isLoading} message={message} onChange={setEdited} disabled={mutation.isPending} />
          )}
        </div>

        {error && <p role="alert" className="text-sm text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{error}</p>}

        <div className="flex justify-end gap-2">
          <button type="button" onClick={onClose} disabled={mutation.isPending}
            className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-50">Cancel</button>
          <button type="button" onClick={() => { setError(null); mutation.mutate(); }} disabled={mutation.isPending || preview.isLoading}
            className="inline-flex items-center gap-1.5 px-4 py-2 text-sm rounded-lg bg-green-600 text-white hover:bg-green-700 disabled:opacity-50">
            <CheckCircle2 size={14} /> {mutation.isPending ? "Posting…" : willSend ? "Post & send SMS" : "Post to Ledger"}
          </button>
        </div>
      </div>
    </Modal>
  );
}
