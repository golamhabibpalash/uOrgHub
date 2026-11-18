import type { SmsMessageStatus } from "../../api/sms";
import { SMS_STATUS_STYLE } from "./smsFormat";

export default function SmsStatusBadge({ status }: { status: SmsMessageStatus }) {
  const { label, className, Icon } = SMS_STATUS_STYLE[status];
  return (
    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${className}`}>
      <Icon size={12} aria-hidden /> {label}
    </span>
  );
}

