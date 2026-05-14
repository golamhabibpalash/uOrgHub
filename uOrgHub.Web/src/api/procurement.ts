import apiClient from "./client";
import { ApiResponse, PagedResult, PaginationRequest } from "../types/api";

export type VendorType = "Supplier" | "Contractor" | "Consultant" | "ServiceProvider";
export type VendorStatus = "Active" | "Inactive" | "Blacklisted";
export type PRStatus = "Draft" | "Submitted" | "Approved" | "Rejected" | "Converted";
export type RFQStatus = "Draft" | "Sent" | "Closed" | "Cancelled";
export type QuotationStatus = "Received" | "Evaluated" | "Accepted" | "Rejected";
export type POStatus = "Draft" | "Sent" | "Confirmed" | "PartiallyReceived" | "FullyReceived" | "Cancelled";
export type GRNStatus = "Draft" | "Confirmed" | "Cancelled";

export interface Vendor {
  id: string;
  vendorCode: string;
  companyName: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  tradeLicense?: string;
  tin?: string;
  bin?: string;
  vendorType: VendorType;
  status: VendorStatus;
  creditLimit: number;
  paymentTermDays: number;
  notes?: string;
  createdAt: string;
}

export const getVendors = (params: PaginationRequest, status?: VendorStatus, vendorType?: VendorType) =>
  apiClient.get<ApiResponse<PagedResult<Vendor>>>("vendors", { params: { ...params, status, vendorType } });

export const getVendorById = (id: string) =>
  apiClient.get<ApiResponse<Vendor>>(`/vendors/${id}`);

export const createVendor = (data: Partial<Vendor>) =>
  apiClient.post<ApiResponse<Vendor>>("/vendors", data);

export const updateVendor = (id: string, data: Partial<Vendor>) =>
  apiClient.put<ApiResponse<Vendor>>(`/vendors/${id}`, data);

export const deleteVendor = (id: string) =>
  apiClient.delete<ApiResponse<string>>(`/vendors/${id}`);

export const getVendorQuotationsByVendor = (vendorId: string, params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<VendorQuotation>>>(`/vendors/${vendorId}/quotations`, { params });

export const getVendorOrders = (vendorId: string, params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<PurchaseOrder>>>(`/vendors/${vendorId}/orders`, { params });

export interface PurchaseRequisition {
  id: string;
  prNumber: string;
  prDate: string;
  requiredDate: string;
  departmentId: string;
  departmentName: string;
  requestedById: string;
  requestedByName: string;
  purpose?: string;
  status: PRStatus;
  approvedById?: string;
  approvedByName?: string;
  approvedAt?: string;
  rejectionReason?: string;
  notes?: string;
  createdAt: string;
  items: PRItem[];
}

export interface PRItem {
  id: string;
  itemVariantId: string;
  variantSKU: string;
  variantName: string;
  warehouseId: string;
  warehouseName: string;
  requestedQuantity: number;
  estimatedUnitCost: number;
  estimatedTotalCost: number;
  notes?: string;
}

export const getPurchaseRequisitions = (params: PaginationRequest, status?: PRStatus) =>
  apiClient.get<ApiResponse<PagedResult<PurchaseRequisition>>>("purchaserequisitions", { params: { ...params, status } });

export const getPurchaseRequisitionById = (id: string) =>
  apiClient.get<ApiResponse<PurchaseRequisition>>(`/purchaserequisitions/${id}`);

export const createPurchaseRequisition = (data: {
  prDate: string;
  requiredDate: string;
  departmentId: string;
  requestedById: string;
  purpose?: string;
  notes?: string;
  items: { itemVariantId: string; warehouseId: string; requestedQuantity: number; estimatedUnitCost: number; notes?: string }[];
}) => apiClient.post<ApiResponse<PurchaseRequisition>>("/purchaserequisitions", data);

export const updatePurchaseRequisition = (id: string, data: {
  prDate: string;
  requiredDate: string;
  departmentId: string;
  requestedById: string;
  purpose?: string;
  notes?: string;
  items: { id?: string; itemVariantId: string; warehouseId: string; requestedQuantity: number; estimatedUnitCost: number; notes?: string }[];
}) => apiClient.put<ApiResponse<PurchaseRequisition>>(`/purchaserequisitions/${id}`, data);

export const deletePurchaseRequisition = (id: string) =>
  apiClient.delete<ApiResponse<string>>(`/purchaserequisitions/${id}`);

export const submitPR = (id: string) =>
  apiClient.post<ApiResponse<PurchaseRequisition>>(`/purchaserequisitions/${id}/submit`, {});

export const approvePR = (id: string) =>
  apiClient.post<ApiResponse<PurchaseRequisition>>(`/purchaserequisitions/${id}/approve`, {});

export const rejectPR = (id: string, reason: string) =>
  apiClient.post<ApiResponse<PurchaseRequisition>>(`/purchaserequisitions/${id}/reject`, { reason });

export interface RequestForQuotation {
  id: string;
  rfqNumber: string;
  rfqDate: string;
  closingDate: string;
  prId?: string;
  prNumber?: string;
  title: string;
  description?: string;
  status: RFQStatus;
  notes?: string;
  createdAt: string;
  items: RFQItem[];
}

export interface RFQItem {
  id: string;
  itemVariantId: string;
  variantSKU: string;
  variantName: string;
  requestedQuantity: number;
  notes?: string;
}

export const getRFQs = (params: PaginationRequest, status?: RFQStatus) =>
  apiClient.get<ApiResponse<PagedResult<RequestForQuotation>>>("rfqs", { params: { ...params, status } });

export const getRFQById = (id: string) =>
  apiClient.get<ApiResponse<RequestForQuotation>>(`/rfqs/${id}`);

export const createRFQ = (data: {
  rfqDate: string;
  closingDate: string;
  prId?: string;
  title: string;
  description?: string;
  notes?: string;
  items: { itemVariantId: string; requestedQuantity: number; notes?: string }[];
}) => apiClient.post<ApiResponse<RequestForQuotation>>("/rfqs", data);

export const updateRFQ = (id: string, data: {
  rfqDate: string;
  closingDate: string;
  prId?: string;
  title: string;
  description?: string;
  status: RFQStatus;
  notes?: string;
  items: { id?: string; itemVariantId: string; requestedQuantity: number; notes?: string }[];
}) => apiClient.put<ApiResponse<RequestForQuotation>>(`/rfqs/${id}`, data);

export const deleteRFQ = (id: string) =>
  apiClient.delete<ApiResponse<string>>(`/rfqs/${id}`);

export const getRFQQuotations = (rfqId: string, params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<VendorQuotation>>>(`/rfqs/${rfqId}/quotations`, { params });

export interface VendorQuotation {
  id: string;
  quotationNumber: string;
  rfqId: string;
  rfqNumber: string;
  vendorId: string;
  vendorName: string;
  quotationDate: string;
  validUntil: string;
  status: QuotationStatus;
  totalAmount: number;
  deliveryDays: number;
  paymentTerms?: string;
  notes?: string;
  createdAt: string;
  items: QuotationItem[];
}

export interface QuotationItem {
  id: string;
  rfqItemId: string;
  itemVariantId: string;
  variantSKU: string;
  variantName: string;
  quotedQuantity: number;
  unitPrice: number;
  totalPrice: number;
  notes?: string;
}

export const getVendorQuotations = (params: PaginationRequest, status?: QuotationStatus) =>
  apiClient.get<ApiResponse<PagedResult<VendorQuotation>>>("vendorquotations", { params: { ...params, status } });

export const getVendorQuotationById = (id: string) =>
  apiClient.get<ApiResponse<VendorQuotation>>(`/vendorquotations/${id}`);

export const createVendorQuotation = (data: {
  rfqId: string;
  vendorId: string;
  quotationDate: string;
  validUntil: string;
  deliveryDays: number;
  paymentTerms?: string;
  notes?: string;
  items: { rfqItemId: string; itemVariantId: string; quotedQuantity: number; unitPrice: number; notes?: string }[];
}) => apiClient.post<ApiResponse<VendorQuotation>>("/vendorquotations", data);

export const updateVendorQuotation = (id: string, data: {
  quotationDate: string;
  validUntil: string;
  status: QuotationStatus;
  deliveryDays: number;
  paymentTerms?: string;
  notes?: string;
  items: { id?: string; rfqItemId: string; itemVariantId: string; quotedQuantity: number; unitPrice: number; notes?: string }[];
}) => apiClient.put<ApiResponse<VendorQuotation>>(`/vendorquotations/${id}`, data);

export const deleteVendorQuotation = (id: string) =>
  apiClient.delete<ApiResponse<string>>(`/vendorquotations/${id}`);

export interface PurchaseOrder {
  id: string;
  poNumber: string;
  poDate: string;
  expectedDeliveryDate: string;
  vendorId: string;
  vendorName: string;
  quotationId?: string;
  quotationNumber?: string;
  prId?: string;
  prNumber?: string;
  status: POStatus;
  subTotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  paymentTerms?: string;
  deliveryAddress?: string;
  notes?: string;
  approvedById?: string;
  approvedByName?: string;
  approvedAt?: string;
  createdAt: string;
  items: POItem[];
}

export interface POItem {
  id: string;
  itemVariantId: string;
  variantSKU: string;
  variantName: string;
  orderedQuantity: number;
  receivedQuantity: number;
  unitPrice: number;
  taxPercent: number;
  discountPercent: number;
  totalPrice: number;
  notes?: string;
}

export const getPurchaseOrders = (params: PaginationRequest, status?: POStatus) =>
  apiClient.get<ApiResponse<PagedResult<PurchaseOrder>>>("purchaseorders", { params: { ...params, status } });

export const getPurchaseOrderById = (id: string) =>
  apiClient.get<ApiResponse<PurchaseOrder>>(`/purchaseorders/${id}`);

export const createPurchaseOrder = (data: {
  poDate: string;
  expectedDeliveryDate: string;
  vendorId: string;
  quotationId?: string;
  prId?: string;
  paymentTerms?: string;
  deliveryAddress?: string;
  notes?: string;
  items: { itemVariantId: string; orderedQuantity: number; unitPrice: number; taxPercent: number; discountPercent: number; notes?: string }[];
}) => apiClient.post<ApiResponse<PurchaseOrder>>("/purchaseorders", data);

export const updatePurchaseOrder = (id: string, data: {
  poDate: string;
  expectedDeliveryDate: string;
  vendorId: string;
  quotationId?: string;
  prId?: string;
  paymentTerms?: string;
  deliveryAddress?: string;
  notes?: string;
  items: { id?: string; itemVariantId: string; orderedQuantity: number; unitPrice: number; taxPercent: number; discountPercent: number; notes?: string }[];
}) => apiClient.put<ApiResponse<PurchaseOrder>>(`/purchaseorders/${id}`, data);

export const deletePurchaseOrder = (id: string) =>
  apiClient.delete<ApiResponse<string>>(`/purchaseorders/${id}`);

export const sendPO = (id: string) =>
  apiClient.post<ApiResponse<PurchaseOrder>>(`/purchaseorders/${id}/send`, {});

export const confirmPO = (id: string) =>
  apiClient.post<ApiResponse<PurchaseOrder>>(`/purchaseorders/${id}/confirm`, {});

export const cancelPO = (id: string) =>
  apiClient.post<ApiResponse<PurchaseOrder>>(`/purchaseorders/${id}/cancel`, {});

export const getPOGRNs = (poId: string, params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<GoodsReceivedNote>>>(`/purchaseorders/${poId}/grns`, { params });

export interface GoodsReceivedNote {
  id: string;
  grnNumber: string;
  grnDate: string;
  poId: string;
  poNumber: string;
  warehouseId: string;
  warehouseName: string;
  receivedById: string;
  receivedByName: string;
  status: GRNStatus;
  notes?: string;
  invoiceNumber?: string;
  invoiceDate?: string;
  createdAt: string;
  items: GRNItem[];
}

export interface GRNItem {
  id: string;
  poItemId: string;
  itemVariantId: string;
  variantSKU: string;
  variantName: string;
  orderedQuantity: number;
  receivedQuantity: number;
  rejectedQuantity: number;
  acceptedQuantity: number;
  unitCost: number;
  notes?: string;
}

export const getGRNs = (params: PaginationRequest, status?: GRNStatus) =>
  apiClient.get<ApiResponse<PagedResult<GoodsReceivedNote>>>("goodsreceivednotes", { params: { ...params, status } });

export const getGRNById = (id: string) =>
  apiClient.get<ApiResponse<GoodsReceivedNote>>(`/goodsreceivednotes/${id}`);

export const createGRN = (data: {
  grnDate: string;
  poId: string;
  warehouseId: string;
  receivedById: string;
  notes?: string;
  invoiceNumber?: string;
  invoiceDate?: string;
  items: { poItemId: string; itemVariantId: string; orderedQuantity: number; receivedQuantity: number; rejectedQuantity: number; unitCost: number; notes?: string }[];
}) => apiClient.post<ApiResponse<GoodsReceivedNote>>("/goodsreceivednotes", data);

export const updateGRN = (id: string, data: {
  grnDate: string;
  warehouseId: string;
  receivedById: string;
  notes?: string;
  invoiceNumber?: string;
  invoiceDate?: string;
  items: { id?: string; poItemId: string; itemVariantId: string; orderedQuantity: number; receivedQuantity: number; rejectedQuantity: number; unitCost: number; notes?: string }[];
}) => apiClient.put<ApiResponse<GoodsReceivedNote>>(`/goodsreceivednotes/${id}`, data);

export const deleteGRN = (id: string) =>
  apiClient.delete<ApiResponse<string>>(`/goodsreceivednotes/${id}`);

export const confirmGRN = (id: string) =>
  apiClient.post<ApiResponse<GoodsReceivedNote>>(`/goodsreceivednotes/${id}/confirm`, {});