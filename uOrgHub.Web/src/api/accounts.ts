import apiClient from "./client";
import { ApiResponse, PagedResult, PaginationRequest } from "../types/api";

// ── Enums ──────────────────────────────────────────────────────────────────

export type AccountGroupType = "Asset" | "Liability" | "Equity" | "Income" | "Expense";
export type FiscalYearStatus = "Active" | "Closed" | "Pending";
export type JournalEntryStatus = "Draft" | "Posted" | "Cancelled";
export type TaxType = "VAT" | "WithholdingTax" | "CustomsDuty" | "ExciseDuty" | "SalesTax";
export type BankTransactionType = "Deposit" | "Withdrawal" | "Transfer" | "Fee" | "Interest" | "ChequeDeposit" | "ChequeIssue";
export type InvoiceStatus = "Draft" | "Sent" | "PartiallyPaid" | "Paid" | "Overdue" | "Cancelled" | "Void";
export type BillStatus = "Draft" | "Received" | "PartiallyPaid" | "Paid" | "Overdue" | "Cancelled" | "Void";
export type PaymentType = "CustomerPayment" | "VendorPayment" | "AdvanceToVendor" | "AdvanceFromCustomer" | "Refund";
export type PaymentMethod = "Cash" | "BankTransfer" | "Cheque" | "CreditCard" | "DebitCard" | "MobileBanking" | "OnlineTransfer";
export type BudgetStatus = "Draft" | "Approved" | "Active" | "Closed" | "Cancelled";

// ── Account Groups ─────────────────────────────────────────────────────────

export interface AccountGroup {
  id: string;
  name: string;
  code: string;
  type: AccountGroupType;
  description?: string;
  isActive: boolean;
  createdAt: string;
  createdBy: string;
}

export const getAccountGroups = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<AccountGroup>>>("/accounts/account-groups", { params });

export const createAccountGroup = (data: Partial<AccountGroup>) =>
  apiClient.post<ApiResponse<AccountGroup>>("/accounts/account-groups", data);

export const updateAccountGroup = (id: string, data: Partial<AccountGroup>) =>
  apiClient.put<ApiResponse<AccountGroup>>(`/accounts/account-groups/${id}`, data);

export const deleteAccountGroup = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/accounts/account-groups/${id}`);

// ── Fiscal Years ───────────────────────────────────────────────────────────

export interface FiscalYear {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  status: FiscalYearStatus;
  isCurrent: boolean;
  createdAt: string;
  createdBy: string;
}

export const getFiscalYears = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<FiscalYear>>>("/accounts/fiscal-years", { params });

export const createFiscalYear = (data: Partial<FiscalYear>) =>
  apiClient.post<ApiResponse<FiscalYear>>("/accounts/fiscal-years", data);

export const updateFiscalYear = (id: string, data: Partial<FiscalYear>) =>
  apiClient.put<ApiResponse<FiscalYear>>(`/accounts/fiscal-years/${id}`, data);

export const deleteFiscalYear = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/accounts/fiscal-years/${id}`);

// ── Chart of Accounts ──────────────────────────────────────────────────────

export interface ChartOfAccount {
  id: string;
  accountCode: string;
  accountName: string;
  accountGroupId: string;
  accountGroupName?: string;
  parentAccountId?: string;
  parentAccountName?: string;
  accountType: AccountGroupType;
  isActive: boolean;
  openingBalance: number;
  currentBalance: number;
  description?: string;
  allowDirectEntry: boolean;
  createdAt: string;
  createdBy: string;
}

export interface JournalEntryLineResponse {
  id: string;
  accountId: string;
  accountName?: string;
  description?: string;
  debitAmount: number;
  creditAmount: number;
  lineOrder: number;
}

export const getChartOfAccounts = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<ChartOfAccount>>>("/accounts/chart-of-accounts", { params });

export const createChartOfAccount = (data: Partial<ChartOfAccount>) =>
  apiClient.post<ApiResponse<ChartOfAccount>>("/accounts/chart-of-accounts", data);

export const updateChartOfAccount = (id: string, data: Partial<ChartOfAccount>) =>
  apiClient.put<ApiResponse<ChartOfAccount>>(`/accounts/chart-of-accounts/${id}`, data);

export const deleteChartOfAccount = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/accounts/chart-of-accounts/${id}`);

export const getAccountLedger = (id: string) =>
  apiClient.get<ApiResponse<JournalEntryLineResponse[]>>(`/accounts/chart-of-accounts/${id}/ledger`);

// ── Journal Entries ────────────────────────────────────────────────────────

export interface JournalEntry {
  id: string;
  entryNumber: string;
  entryDate: string;
  referenceNumber?: string;
  description: string;
  status: JournalEntryStatus;
  totalDebit: number;
  totalCredit: number;
  createdBy: string;
  postedBy?: string;
  postedAt?: string;
  createdAt: string;
  lines: JournalEntryLineResponse[];
}

export interface CreateJournalEntryLineDto {
  accountId: string;
  description?: string;
  debitAmount: number;
  creditAmount: number;
  lineOrder: number;
}

export const getJournalEntries = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<JournalEntry>>>("/accounts/journal-entries", { params });

export const getJournalEntryById = (id: string) =>
  apiClient.get<ApiResponse<JournalEntry>>(`/accounts/journal-entries/${id}`);

export const createJournalEntry = (data: { entryDate: string; referenceNumber?: string; description: string; lines: CreateJournalEntryLineDto[] }) =>
  apiClient.post<ApiResponse<JournalEntry>>("/accounts/journal-entries", data);

export const updateJournalEntry = (id: string, data: Partial<JournalEntry>) =>
  apiClient.put<ApiResponse<JournalEntry>>(`/accounts/journal-entries/${id}`, data);

export const postJournalEntry = (id: string) =>
  apiClient.post<ApiResponse<JournalEntry>>(`/accounts/journal-entries/${id}/post`, {});

export const cancelJournalEntry = (id: string) =>
  apiClient.post<ApiResponse<JournalEntry>>(`/accounts/journal-entries/${id}/cancel`, {});

export const deleteJournalEntry = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/accounts/journal-entries/${id}`);

// ── Cost Centers ───────────────────────────────────────────────────────────

export interface CostCenter {
  id: string;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
  parentCostCenterId?: string;
  parentCostCenterName?: string;
  departmentId?: string;
}

export const getCostCenters = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<CostCenter>>>("/accounts/cost-centers", { params });

export const createCostCenter = (data: Partial<CostCenter>) =>
  apiClient.post<ApiResponse<CostCenter>>("/accounts/cost-centers", data);

export const updateCostCenter = (id: string, data: Partial<CostCenter>) =>
  apiClient.put<ApiResponse<CostCenter>>(`/accounts/cost-centers/${id}`, data);

export const deleteCostCenter = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/accounts/cost-centers/${id}`);

// ── Tax Rates ──────────────────────────────────────────────────────────────

export interface TaxRate {
  id: string;
  code: string;
  name: string;
  taxType: TaxType;
  rate: number;
  description?: string;
  isActive: boolean;
  taxAccountId?: string;
}

export const getTaxRates = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<TaxRate>>>("/accounts/tax-rates", { params });

export const createTaxRate = (data: Partial<TaxRate>) =>
  apiClient.post<ApiResponse<TaxRate>>("/accounts/tax-rates", data);

export const updateTaxRate = (id: string, data: Partial<TaxRate>) =>
  apiClient.put<ApiResponse<TaxRate>>(`/accounts/tax-rates/${id}`, data);

export const deleteTaxRate = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/accounts/tax-rates/${id}`);

// ── Bank Accounts ──────────────────────────────────────────────────────────

export interface BankAccount {
  id: string;
  accountNumber: string;
  accountName: string;
  bankName: string;
  branchName?: string;
  routingNumber?: string;
  currency: string;
  openingBalance: number;
  currentBalance: number;
  isActive: boolean;
  chartOfAccountId: string;
}

export interface BankTransaction {
  id: string;
  bankAccountId: string;
  bankAccountName: string;
  transactionType: BankTransactionType;
  transactionDate: string;
  amount: number;
  description: string;
  referenceNumber?: string;
  chequeNumber?: string;
  payee?: string;
  isReconciled: boolean;
  journalEntryId?: string;
}

export const getBankAccounts = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<BankAccount>>>("/accounts/bank-accounts", { params });

export const createBankAccount = (data: Partial<BankAccount>) =>
  apiClient.post<ApiResponse<BankAccount>>("/accounts/bank-accounts", data);

export const updateBankAccount = (id: string, data: Partial<BankAccount>) =>
  apiClient.put<ApiResponse<BankAccount>>(`/accounts/bank-accounts/${id}`, data);

export const deleteBankAccount = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/accounts/bank-accounts/${id}`);

export const getBankTransactions = (bankAccountId: string, params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<BankTransaction>>>(`/accounts/bank-accounts/${bankAccountId}/transactions`, { params });

export const createBankTransaction = (bankAccountId: string, data: Partial<BankTransaction>) =>
  apiClient.post<ApiResponse<BankTransaction>>(`/accounts/bank-accounts/${bankAccountId}/transactions`, data);

// ── Customers ──────────────────────────────────────────────────────────────

export interface Customer {
  id: string;
  customerCode: string;
  name: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  tin?: string;
  bin?: string;
  creditLimit: number;
  paymentTermsDays: number;
  isActive: boolean;
  receivableAccountId: string;
}

export const getCustomers = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<Customer>>>("/accounts/customers", { params });

export const createCustomer = (data: Partial<Customer>) =>
  apiClient.post<ApiResponse<Customer>>("/accounts/customers", data);

export const updateCustomer = (id: string, data: Partial<Customer>) =>
  apiClient.put<ApiResponse<Customer>>(`/accounts/customers/${id}`, data);

export const deleteCustomer = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/accounts/customers/${id}`);

// ── Invoices ───────────────────────────────────────────────────────────────

export interface InvoiceLine {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
  taxAmount: number;
  lineTotal: number;
  lineOrder: number;
  taxRateId?: string;
  revenueAccountId: string;
  costCenterId?: string;
}

export interface Invoice {
  id: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  fiscalYearId: string;
  invoiceDate: string;
  dueDate: string;
  status: InvoiceStatus;
  subTotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  paidAmount: number;
  balanceDue: number;
  notes?: string;
  costCenterId?: string;
  journalEntryId?: string;
  lines: InvoiceLine[];
}

export const getInvoices = (params: PaginationRequest, customerId?: string, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<Invoice>>>("/accounts/invoices", { params: { ...params, customerId, status } });

export const getInvoiceById = (id: string) =>
  apiClient.get<ApiResponse<Invoice>>(`/accounts/invoices/${id}`);

export const createInvoice = (data: Partial<Invoice>) =>
  apiClient.post<ApiResponse<Invoice>>("/accounts/invoices", data);

export const updateInvoice = (id: string, data: Partial<Invoice>) =>
  apiClient.put<ApiResponse<Invoice>>(`/accounts/invoices/${id}`, data);

export const postInvoice = (id: string) =>
  apiClient.post<ApiResponse<Invoice>>(`/accounts/invoices/${id}/post`, {});

export const voidInvoice = (id: string) =>
  apiClient.post<ApiResponse<Invoice>>(`/accounts/invoices/${id}/void`, {});

// ── Vendors ────────────────────────────────────────────────────────────────

export interface Vendor {
  id: string;
  vendorCode: string;
  name: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  tin?: string;
  bin?: string;
  paymentTermsDays: number;
  isActive: boolean;
  payableAccountId: string;
}

export const getVendors = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<Vendor>>>("/accounts/vendors", { params });

export const createVendor = (data: Partial<Vendor>) =>
  apiClient.post<ApiResponse<Vendor>>("/accounts/vendors", data);

export const updateVendor = (id: string, data: Partial<Vendor>) =>
  apiClient.put<ApiResponse<Vendor>>(`/accounts/vendors/${id}`, data);

export const deleteVendor = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/accounts/vendors/${id}`);

// ── Bills ──────────────────────────────────────────────────────────────────

export interface BillLine {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
  taxAmount: number;
  lineTotal: number;
  lineOrder: number;
  taxRateId?: string;
  expenseAccountId: string;
  costCenterId?: string;
}

export interface Bill {
  id: string;
  billNumber: string;
  vendorBillNumber?: string;
  vendorId: string;
  vendorName: string;
  fiscalYearId: string;
  billDate: string;
  dueDate: string;
  status: BillStatus;
  subTotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  paidAmount: number;
  balanceDue: number;
  notes?: string;
  costCenterId?: string;
  journalEntryId?: string;
  lines: BillLine[];
}

export const getBills = (params: PaginationRequest, vendorId?: string, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<Bill>>>("/accounts/bills", { params: { ...params, vendorId, status } });

export const getBillById = (id: string) =>
  apiClient.get<ApiResponse<Bill>>(`/accounts/bills/${id}`);

export const createBill = (data: Partial<Bill>) =>
  apiClient.post<ApiResponse<Bill>>("/accounts/bills", data);

export const updateBill = (id: string, data: Partial<Bill>) =>
  apiClient.put<ApiResponse<Bill>>(`/accounts/bills/${id}`, data);

export const approveBill = (id: string) =>
  apiClient.post<ApiResponse<Bill>>(`/accounts/bills/${id}/approve`, {});

export const voidBill = (id: string) =>
  apiClient.post<ApiResponse<Bill>>(`/accounts/bills/${id}/void`, {});

// ── Payments ───────────────────────────────────────────────────────────────

export interface PaymentAllocation {
  id: string;
  invoiceId?: string;
  billId?: string;
  allocatedAmount: number;
}

export interface Payment {
  id: string;
  paymentNumber: string;
  paymentType: PaymentType;
  paymentMethod: PaymentMethod;
  paymentDate: string;
  amount: number;
  referenceNumber?: string;
  chequeNumber?: string;
  notes?: string;
  customerId?: string;
  customerName?: string;
  vendorId?: string;
  vendorName?: string;
  bankAccountId?: string;
  fiscalYearId: string;
  journalEntryId?: string;
  allocations: PaymentAllocation[];
}

export const getPayments = (params: PaginationRequest, customerId?: string, vendorId?: string) =>
  apiClient.get<ApiResponse<PagedResult<Payment>>>("/accounts/payments", { params: { ...params, customerId, vendorId } });

export const getPaymentById = (id: string) =>
  apiClient.get<ApiResponse<Payment>>(`/accounts/payments/${id}`);

export const createPayment = (data: Partial<Payment>) =>
  apiClient.post<ApiResponse<Payment>>("/accounts/payments", data);

// ── Budgets ────────────────────────────────────────────────────────────────

export interface BudgetLine {
  id: string;
  accountId: string;
  accountName: string;
  costCenterId?: string;
  period: number;
  plannedAmount: number;
  actualAmount: number;
  variance: number;
}

export interface Budget {
  id: string;
  name: string;
  description?: string;
  status: BudgetStatus;
  totalAmount: number;
  fiscalYearId: string;
  costCenterId?: string;
  lines: BudgetLine[];
}

export const getBudgets = (params: PaginationRequest, fiscalYearId?: string) =>
  apiClient.get<ApiResponse<PagedResult<Budget>>>("/accounts/budgets", { params: { ...params, fiscalYearId } });

export const getBudgetById = (id: string) =>
  apiClient.get<ApiResponse<Budget>>(`/accounts/budgets/${id}`);

export const createBudget = (data: Partial<Budget>) =>
  apiClient.post<ApiResponse<Budget>>("/accounts/budgets", data);

export const updateBudget = (id: string, data: Partial<Budget>) =>
  apiClient.put<ApiResponse<Budget>>(`/accounts/budgets/${id}`, data);

export const approveBudget = (id: string) =>
  apiClient.post<ApiResponse<Budget>>(`/accounts/budgets/${id}/approve`, {});

export const deleteBudget = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/accounts/budgets/${id}`);
