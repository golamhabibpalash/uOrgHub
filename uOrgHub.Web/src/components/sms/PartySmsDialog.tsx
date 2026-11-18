import { useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Send } from "lucide-react";
import type { PartySmsPreview, PartySmsResult } from "../../api/accounts";
import type { ApiResponse } from "../../types/api";
import type { AxiosResponse } from "axios";
import Modal from "../shared/Modal";
import PartySmsComposer from "./PartySmsComposer";
import { extractApiError } from "../../utils/apiError";

interface Props {
  title: string;
  /** Distinguishes cached previews, e.g. ["payment-sms", id]. */
  queryKey: unknown[];
  loadPreview: () => Promise<AxiosResponse<ApiResponse<PartySmsPreview>>>;
  send: (message: string, idempotencyKey: string) => Promise<AxiosResponse<ApiResponse<PartySmsResult>>>;
  /** Called once the dialog closes, sent or skipped. */
  onClose: (sent: boolean) => void;
  skipLabel?: string;
}

const newKey = () => (typeof crypto !== "undefined" && "randomUUID" in crypto ? crypto.randomUUID() : `${Date.now()}-${Math.random().toString(36).slice(2)}`);

/** Send an SMS to a payment's or voucher's party: preview, tweak, send — or skip. */
export default function PartySmsDialog({ title, queryKey, loadPreview, send, onClose, skipLabel = "Skip" }: Props) {
  const preview = useQuery({ queryKey, queryFn: loadPreview, staleTime: 0 });
  const p = preview.data?.data?.data ?? undefined;
  const [edited, setEdited] = useState<string | null>(null);
  const message = edited ?? p?.message ?? "";
  // One key per dialog: a double-click sends once; opening the dialog again later is a new send.
  const [key] = useState(newKey);
  const [error, setError] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: () => send(message, key),
    onSuccess: () => onClose(true),
    onError: (e) => setError(extractApiError(e)),
  });

  return (
    <Modal title={title} open onClose={() => !mutation.isPending && onClose(false)} size="lg">
      <div className="space-y-3">
        <PartySmsComposer preview={p} loading={preview.isLoading} message={message} onChange={setEdited} disabled={mutation.isPending} />
        {preview.isError && <p role="alert" className="text-sm text-red-700">{extractApiError(preview.error)}</p>}
        {error && <p role="alert" className="text-sm text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{error}</p>}
        <div className="flex justify-end gap-2 pt-1">
          <button type="button" onClick={() => onClose(false)} disabled={mutation.isPending}
            className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-50">
            {p?.canSend ? skipLabel : "Close"}
          </button>
          {p?.canSend && (
            <button type="button" onClick={() => { setError(null); mutation.mutate(); }}
              disabled={mutation.isPending || message.trim() === ""}
              className="inline-flex items-center gap-1.5 px-4 py-2 text-sm rounded-lg bg-primary-600 text-white hover:bg-primary-700 disabled:opacity-50">
              <Send size={14} /> {mutation.isPending ? "Sending…" : "Send SMS"}
            </button>
          )}
        </div>
      </div>
    </Modal>
  );
}
