import apiClient from "./client";
import type { ApiResponse, PagedResult, PaginationRequest } from "../types/api";
import type { SmsEncoding } from "../utils/smsSegments";

export type SmsMessageStatus = "Queued" | "Sending" | "Submitted" | "Delivered" | "Failed" | "Undetermined";
export type SmsGranularity = "Day" | "Week" | "Month";

export interface SmsProviderField {
  key: string;
  label: string;
  isSecret: boolean;
  required: boolean;
  placeholder?: string;
  help?: string;
  defaultValue?: string;
  kind: "text" | "select" | "textarea";
  options?: string[];
}

export interface SmsProviderCapabilities {
  balance: boolean;
  rate: boolean;
  deliveryReports: boolean;
  requiresSenderId: boolean;
}

export interface SmsProviderDescriptor {
  key: string;
  displayName: string;
  description: string;
  defaultBaseUrl?: string;
  fields: SmsProviderField[];
  capabilities: SmsProviderCapabilities;
}

export interface SmsGateway {
  id: string;
  name: string;
  providerKey: string;
  providerName: string;
  senderId?: string;
  baseUrl?: string;
  effectiveBaseUrl?: string;
  settings: Record<string, string>;
  /** Which secrets are stored — the values never leave the server. */
  credentials: { key: string; isSet: boolean }[];
  isEnabled: boolean;
  isDefault: boolean;
  ratePerSegment?: number;
  lastProviderRate?: number;
  effectiveRate?: number;
  currency: string;
  timeoutSeconds: number;
  maxSendsPerSecond: number;
  lastBalance?: number;
  balanceUnit?: string;
  lastBalanceSyncedAt?: string;
  lastBalanceError?: string;
  lastTestedAt?: string;
  lastTestSucceeded?: boolean;
  lastTestMessage?: string;
  capabilities?: SmsProviderCapabilities;
  createdAt: string;
}

export interface SmsGatewayInput {
  name: string;
  providerKey?: string;
  senderId?: string;
  baseUrl?: string;
  settings: Record<string, string>;
  credentials: Record<string, string>;
  clearCredentials?: string[];
  isEnabled: boolean;
  ratePerSegment?: number | null;
  currency: string;
  timeoutSeconds: number;
  maxSendsPerSecond: number;
}

export interface SmsGatewayTestResult {
  success: boolean;
  message: string;
  balance?: number;
  balanceUnit?: string;
  testMessageId?: string;
}

export interface SmsBalance {
  gatewayId: string;
  gatewayName: string;
  supported: boolean;
  success: boolean;
  balance?: number;
  unit?: string;
  rate?: number;
  currency: string;
  syncedAt?: string;
  error?: string;
}

export interface SmsTotals {
  sent: number;
  successful: number;
  delivered: number;
  failed: number;
  undetermined: number;
  pending: number;
  segments: number;
  cost: number;
}

export interface SmsDashboard {
  activeGateway?: SmsGateway;
  allTime: SmsTotals;
  today: SmsTotals;
  last30Days: SmsTotals;
  gatewayCount: number;
}

export interface SmsAnalyticsPoint {
  period: string;
  sent: number;
  successful: number;
  failed: number;
  pending: number;
  segments: number;
  cost: number;
}

export interface SmsAnalytics {
  from: string;
  to: string;
  granularity: SmsGranularity;
  totals: SmsTotals;
  points: SmsAnalyticsPoint[];
  currency: string;
}

export interface SmsComposeInfo {
  hasActiveGateway: boolean;
  gatewayId?: string;
  gatewayName?: string;
  providerName?: string;
  ratePerSegment?: number;
  currency: string;
  canSendBulk: boolean;
  maxRecipients: number;
}

export interface SendSmsResult {
  requestId: string;
  duplicate: boolean;
  gatewayName: string;
  recipientCount: number;
  segmentsPerRecipient: number;
  totalSegments: number;
  encoding: SmsEncoding;
  estimatedCost?: number;
  currency: string;
}

export interface SmsSendRequestStatus {
  requestId: string;
  recipientCount: number;
  queued: number;
  submitted: number;
  delivered: number;
  failed: number;
  undetermined: number;
  completed: boolean;
  cost?: number;
  failures: { recipient: string; errorMessage?: string }[];
}

export interface SmsMessage {
  id: string;
  recipient: string;
  messagePreview: string;
  gatewayName: string;
  providerKey: string;
  segments: number;
  encoding: SmsEncoding;
  status: SmsMessageStatus;
  cost?: number;
  costIsActual: boolean;
  providerMessageId?: string;
  errorMessage?: string;
  createdAt: string;
  submittedAt?: string;
  createdBy: string;
}

export interface SmsMessageDetail extends SmsMessage {
  messageText: string;
  gatewayId: string;
  sendRequestId?: string;
  providerStatus?: string;
  errorCode?: string;
  providerResponse?: string;
  attempts: number;
  deliveredAt?: string;
  failedAt?: string;
  lastStatusCheckAt?: string;
  source?: string;
  sourceReference?: string;
}

export interface SmsHistoryFilters {
  from?: string; // ISO, UTC, inclusive
  to?: string;   // ISO, UTC, exclusive
  gatewayId?: string;
  status?: SmsMessageStatus;
}

/** The viewer's UTC offset in minutes (east positive), so the server buckets by their local day. */
export const tzOffsetMinutes = () => -new Date().getTimezoneOffset();

// Gateways
export const getSmsGateways = () => apiClient.get<ApiResponse<SmsGateway[]>>("/smsgateways");
export const getSmsProviders = () => apiClient.get<ApiResponse<SmsProviderDescriptor[]>>("/smsgateways/providers");
export const createSmsGateway = (data: SmsGatewayInput) => apiClient.post<ApiResponse<SmsGateway>>("/smsgateways", data);
export const updateSmsGateway = (id: string, data: SmsGatewayInput) => apiClient.put<ApiResponse<SmsGateway>>(`/smsgateways/${id}`, data);
export const deleteSmsGateway = (id: string) => apiClient.delete<ApiResponse<string>>(`/smsgateways/${id}`);
export const setSmsGatewayEnabled = (id: string, enabled: boolean) =>
  apiClient.post<ApiResponse<SmsGateway>>(`/smsgateways/${id}/${enabled ? "enable" : "disable"}`);
export const activateSmsGateway = (id: string) => apiClient.post<ApiResponse<SmsGateway>>(`/smsgateways/${id}/activate`);
export const testSmsGateway = (id: string, testRecipient?: string) =>
  apiClient.post<ApiResponse<SmsGatewayTestResult>>(`/smsgateways/${id}/test`, { testRecipient: testRecipient || undefined });
export const syncSmsBalance = (force = false, gatewayId?: string) =>
  apiClient.post<ApiResponse<SmsBalance | null>>("/smsgateways/sync-balance", null, { params: { force, gatewayId } });

// Sending
export const getSmsComposeInfo = () => apiClient.get<ApiResponse<SmsComposeInfo>>("/smsmessages/compose-info");
export const sendSms = (data: { recipients: string[]; message: string; idempotencyKey: string }) =>
  apiClient.post<ApiResponse<SendSmsResult>>("/smsmessages/send", data);
export const getSmsSendStatus = (requestId: string) =>
  apiClient.get<ApiResponse<SmsSendRequestStatus>>(`/smsmessages/requests/${requestId}`);

// History
export const getSmsMessages = (params: PaginationRequest, filters: SmsHistoryFilters) =>
  apiClient.get<ApiResponse<PagedResult<SmsMessage>>>("/smsmessages", { params: { ...params, ...filters } });
export const getSmsMessage = (id: string) => apiClient.get<ApiResponse<SmsMessageDetail>>(`/smsmessages/${id}`);

// Dashboard
export const getSmsDashboard = () =>
  apiClient.get<ApiResponse<SmsDashboard>>("/smsdashboard", { params: { tzOffsetMinutes: tzOffsetMinutes() } });
export const getSmsAnalytics = (from: string, to: string, granularity: SmsGranularity, gatewayId?: string) =>
  apiClient.get<ApiResponse<SmsAnalytics>>("/smsdashboard/analytics", {
    params: { from, to, granularity, tzOffsetMinutes: tzOffsetMinutes(), gatewayId },
  });

// Templates
export interface SmsPlaceholder {
  key: string;
  description: string;
  sample: string;
}

export interface SmsTemplate {
  eventKey: string;
  label: string;
  description: string;
  body: string;
  defaultBody: string;
  isCustom: boolean;
  placeholders: SmsPlaceholder[];
  updatedAt?: string;
  updatedBy?: string;
}

export const getSmsTemplates = () => apiClient.get<ApiResponse<SmsTemplate[]>>("/smstemplates");
export const updateSmsTemplate = (eventKey: string, body: string) =>
  apiClient.put<ApiResponse<SmsTemplate>>(`/smstemplates/${eventKey}`, { body });
export const resetSmsTemplate = (eventKey: string) =>
  apiClient.delete<ApiResponse<SmsTemplate>>(`/smstemplates/${eventKey}`);
