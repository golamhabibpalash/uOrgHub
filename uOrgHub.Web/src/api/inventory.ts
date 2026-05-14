import apiClient from "./client";
import { ApiResponse, PagedResult, PaginationRequest } from "../types/api";

// ── Enums ──────────────────────────────────────────────────────────────────

export type AttributeDataType = "Text" | "Number" | "Boolean" | "List";
export type StockTransactionType = "GoodsReceived" | "GoodsIssued" | "Transfer" | "Adjustment" | "Return";
export type StockTransactionStatus = "Draft" | "Confirmed" | "Cancelled";

// ── Inventory Types ────────────────────────────────────────────────────────

export interface InventoryType {
  id: string;
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
}

export const getInventoryTypes = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<InventoryType>>>("/inventorytypes", { params });

export const getInventoryTypeById = (id: string) =>
  apiClient.get<ApiResponse<InventoryType>>(`/inventorytypes/${id}`);

export const createInventoryType = (data: Partial<InventoryType>) =>
  apiClient.post<ApiResponse<InventoryType>>("/inventorytypes", data);

export const updateInventoryType = (id: string, data: Partial<InventoryType>) =>
  apiClient.put<ApiResponse<InventoryType>>(`/inventorytypes/${id}`, data);

export const deleteInventoryType = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/inventorytypes/${id}`);

// ── Inventory Categories ───────────────────────────────────────────────────

export interface InventoryCategory {
  id: string;
  name: string;
  code: string;
  typeId: string;
  typeName: string;
  parentCategoryId?: string;
  parentCategoryName?: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
}

export const getInventoryCategories = (params: PaginationRequest, typeId?: string) =>
  apiClient.get<ApiResponse<PagedResult<InventoryCategory>>>("/inventorycategories", { params: { ...params, typeId } });

export const getInventoryCategoryById = (id: string) =>
  apiClient.get<ApiResponse<InventoryCategory>>(`/inventorycategories/${id}`);

export const createInventoryCategory = (data: Partial<InventoryCategory>) =>
  apiClient.post<ApiResponse<InventoryCategory>>("/inventorycategories", data);

export const updateInventoryCategory = (id: string, data: Partial<InventoryCategory>) =>
  apiClient.put<ApiResponse<InventoryCategory>>(`/inventorycategories/${id}`, data);

export const deleteInventoryCategory = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/inventorycategories/${id}`);

// ── Units of Measure ───────────────────────────────────────────────────────

export interface UnitOfMeasure {
  id: string;
  name: string;
  abbreviation: string;
  isActive: boolean;
  createdAt: string;
}

export const getUnitsOfMeasure = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<UnitOfMeasure>>>("/unitsofmeasure", { params });

export const getUnitOfMeasureById = (id: string) =>
  apiClient.get<ApiResponse<UnitOfMeasure>>(`/unitsofmeasure/${id}`);

export const createUnitOfMeasure = (data: Partial<UnitOfMeasure>) =>
  apiClient.post<ApiResponse<UnitOfMeasure>>("/unitsofmeasure", data);

export const updateUnitOfMeasure = (id: string, data: Partial<UnitOfMeasure>) =>
  apiClient.put<ApiResponse<UnitOfMeasure>>(`/unitsofmeasure/${id}`, data);

export const deleteUnitOfMeasure = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/unitsofmeasure/${id}`);

// ── Attribute Definitions ──────────────────────────────────────────────────

export interface AttributeDefinition {
  id: string;
  name: string;
  dataType: AttributeDataType;
  categoryId?: string;
  categoryName?: string;
  isRequired: boolean;
  predefinedValues?: string;
  isActive: boolean;
  createdAt: string;
}

export const getAttributeDefinitions = (params: PaginationRequest, categoryId?: string) =>
  apiClient.get<ApiResponse<PagedResult<AttributeDefinition>>>("/attributedefinitions", { params: { ...params, categoryId } });

export const getAttributeDefinitionById = (id: string) =>
  apiClient.get<ApiResponse<AttributeDefinition>>(`/attributedefinitions/${id}`);

export const createAttributeDefinition = (data: Partial<AttributeDefinition>) =>
  apiClient.post<ApiResponse<AttributeDefinition>>("/attributedefinitions", data);

export const updateAttributeDefinition = (id: string, data: Partial<AttributeDefinition>) =>
  apiClient.put<ApiResponse<AttributeDefinition>>(`/attributedefinitions/${id}`, data);

export const deleteAttributeDefinition = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/attributedefinitions/${id}`);

// ── Items ──────────────────────────────────────────────────────────────────

export interface Item {
  id: string;
  baseName: string;
  itemCode?: string;
  typeId: string;
  typeName: string;
  categoryId: string;
  categoryName: string;
  unitOfMeasureId: string;
  unitOfMeasureName: string;
  unitAbbreviation: string;
  brand?: string;
  manufacturer?: string;
  description?: string;
  reorderLevel: number;
  standardCost: number;
  isActive: boolean;
  variantCount: number;
  createdAt: string;
}

export const getItems = (params: PaginationRequest, categoryId?: string, typeId?: string) =>
  apiClient.get<ApiResponse<PagedResult<Item>>>("/items", { params: { ...params, categoryId, typeId } });

export const getItemById = (id: string) =>
  apiClient.get<ApiResponse<Item>>(`/items/${id}`);

export const createItem = (data: Partial<Item>) =>
  apiClient.post<ApiResponse<Item>>("/items", data);

export const updateItem = (id: string, data: Partial<Item>) =>
  apiClient.put<ApiResponse<Item>>(`/items/${id}`, data);

export const deleteItem = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/items/${id}`);

export const getItemVariantsByItem = (itemId: string, params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<ItemVariant>>>(`/items/${itemId}/variants`, { params });

// ── Item Variants ──────────────────────────────────────────────────────────

export interface VariantAttributeValue {
  id: string;
  attributeDefinitionId: string;
  attributeName: string;
  value: string;
}

export interface ItemVariant {
  id: string;
  itemId: string;
  itemBaseName: string;
  sku: string;
  variantName: string;
  barcode?: string;
  costPrice: number;
  sellingPrice: number;
  isDefault: boolean;
  isActive: boolean;
  attributeHash?: string;
  attributes: VariantAttributeValue[];
  createdAt: string;
}

export const getItemVariants = (params: PaginationRequest, itemId?: string) =>
  apiClient.get<ApiResponse<PagedResult<ItemVariant>>>("/itemvariants", { params: { ...params, itemId } });

export const getItemVariantById = (id: string) =>
  apiClient.get<ApiResponse<ItemVariant>>(`/itemvariants/${id}`);

export const createItemVariant = (data: {
  itemId: string;
  barcode?: string;
  costPrice: number;
  sellingPrice: number;
  isDefault: boolean;
  attributes: { attributeDefinitionId: string; value: string }[];
}) => apiClient.post<ApiResponse<ItemVariant>>("/itemvariants", data);

export const updateItemVariant = (id: string, data: {
  barcode?: string;
  costPrice: number;
  sellingPrice: number;
  isDefault: boolean;
  isActive: boolean;
  attributes: { attributeDefinitionId: string; value: string }[];
}) => apiClient.put<ApiResponse<ItemVariant>>(`/itemvariants/${id}`, data);

export const deleteItemVariant = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/itemvariants/${id}`);

// ── Warehouses ─────────────────────────────────────────────────────────────

export interface Warehouse {
  id: string;
  name: string;
  code: string;
  location?: string;
  contactPerson?: string;
  contactPhone?: string;
  isActive: boolean;
  createdAt: string;
}

export const getWarehouses = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<Warehouse>>>("/warehouses", { params });

export const getWarehouseById = (id: string) =>
  apiClient.get<ApiResponse<Warehouse>>(`/warehouses/${id}`);

export const createWarehouse = (data: Partial<Warehouse>) =>
  apiClient.post<ApiResponse<Warehouse>>("/warehouses", data);

export const updateWarehouse = (id: string, data: Partial<Warehouse>) =>
  apiClient.put<ApiResponse<Warehouse>>(`/warehouses/${id}`, data);

export const deleteWarehouse = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/warehouses/${id}`);

// ── Stock Balances ─────────────────────────────────────────────────────────

export interface StockBalance {
  id: string;
  itemVariantId: string;
  variantSKU: string;
  variantName: string;
  itemBaseName: string;
  warehouseId: string;
  warehouseName: string;
  warehouseCode: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
  lastUpdated: string;
}

export const getStockBalances = (params: PaginationRequest, warehouseId?: string, itemVariantId?: string) =>
  apiClient.get<ApiResponse<PagedResult<StockBalance>>>("/stockbalances", { params: { ...params, warehouseId, itemVariantId } });

export const getStockBalanceById = (id: string) =>
  apiClient.get<ApiResponse<StockBalance>>(`/stockbalances/${id}`);

// ── Stock Transactions ─────────────────────────────────────────────────────

export interface StockTransaction {
  id: string;
  transactionNumber: string;
  transactionDate: string;
  transactionType: StockTransactionType;
  transactionTypeName: string;
  status: StockTransactionStatus;
  statusName: string;
  itemVariantId: string;
  variantSKU: string;
  variantName: string;
  warehouseId: string;
  warehouseName: string;
  fromWarehouseId?: string;
  fromWarehouseName?: string;
  quantity: number;
  unitCost: number;
  totalCost: number;
  referenceNumber?: string;
  notes?: string;
  createdAt: string;
}

export const getStockTransactions = (
  params: PaginationRequest,
  warehouseId?: string,
  itemVariantId?: string,
  status?: StockTransactionStatus,
) =>
  apiClient.get<ApiResponse<PagedResult<StockTransaction>>>("/stocktransactions", {
    params: { ...params, warehouseId, itemVariantId, status },
  });

export const getStockTransactionById = (id: string) =>
  apiClient.get<ApiResponse<StockTransaction>>(`/stocktransactions/${id}`);

export const createStockTransaction = (data: {
  transactionDate: string;
  transactionType: StockTransactionType;
  itemVariantId: string;
  warehouseId: string;
  fromWarehouseId?: string;
  quantity: number;
  unitCost: number;
  referenceNumber?: string;
  notes?: string;
}) => apiClient.post<ApiResponse<StockTransaction>>("/stocktransactions", data);

export const updateStockTransaction = (id: string, data: {
  transactionDate: string;
  itemVariantId: string;
  warehouseId: string;
  fromWarehouseId?: string;
  quantity: number;
  unitCost: number;
  referenceNumber?: string;
  notes?: string;
}) => apiClient.put<ApiResponse<StockTransaction>>(`/stocktransactions/${id}`, data);

export const deleteStockTransaction = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/stocktransactions/${id}`);

export const confirmStockTransaction = (id: string) =>
  apiClient.post<ApiResponse<StockTransaction>>(`/stocktransactions/${id}/confirm`, {});

export const cancelStockTransaction = (id: string) =>
  apiClient.post<ApiResponse<StockTransaction>>(`/stocktransactions/${id}/cancel`, {});
