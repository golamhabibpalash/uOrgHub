import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useEffect, useState } from "react";
import LoginPage from "./pages/auth/LoginPage";
import TwoFactorPage from "./pages/auth/TwoFactorPage";
import ForgotPasswordPage from "./pages/auth/ForgotPasswordPage";
import CompanySetup from "./pages/company/CompanySetup";
import ProtectedRoute from "./components/auth/ProtectedRoute";
import AccessDeniedPage from "./components/auth/AccessDeniedPage";
import UsersPage from "./pages/admin/UsersPage";
import UserDetailPage from "./pages/admin/UserDetailPage";
import UserCreatePage from "./pages/admin/UserCreatePage";
import RolesPage from "./pages/admin/RolesPage";
import AccessLogsPage from "./pages/admin/AccessLogsPage";
import CompanySettingsPage from "./pages/admin/CompanySettingsPage";
import ThemeSettingsPage from "./pages/admin/ThemeSettingsPage";
import { ThemeProvider } from "./context/ThemeProvider";
import MyProfilePage from "./pages/profile/MyProfilePage";
import AppLayout from "./components/layout/AppLayout";
import HomePage from "./pages/dashboard/HomePage";
import HRDashboard from "./pages/hr/HRDashboard";
import Departments from "./pages/hr/Departments";
import Employees from "./pages/hr/Employees";
import Designations from "./pages/hr/Designations";
import LeaveManagement from "./pages/hr/LeaveManagement";
import AttendanceManagement from "./pages/hr/AttendanceManagement";
import PayrollManagement from "./pages/hr/PayrollManagement";
import Recruitment from "./pages/hr/Recruitment";
import PerformanceManagement from "./pages/hr/PerformanceManagement";
import AccountsDashboard from "./pages/accounts/AccountsDashboard";
import AccountGroups from "./pages/accounts/AccountGroups";
import FiscalYears from "./pages/accounts/FiscalYears";
import ChartOfAccounts from "./pages/accounts/ChartOfAccounts";
import JournalEntries from "./pages/accounts/JournalEntries";
import CostCenters from "./pages/accounts/CostCenters";
import TaxRates from "./pages/accounts/TaxRates";
import BankAccounts from "./pages/accounts/BankAccounts";
import AccountsCustomers from "./pages/accounts/Customers";
import Invoices from "./pages/accounts/Invoices";
import AccountsVendors from "./pages/accounts/Vendors";
import Bills from "./pages/accounts/Bills";
import Payments from "./pages/accounts/Payments";
import Budgets from "./pages/accounts/Budgets";
import InventoryDashboard from "./pages/inventory/InventoryDashboard";
import InventoryTypes from "./pages/inventory/InventoryTypes";
import InventoryCategories from "./pages/inventory/InventoryCategories";
import UnitsOfMeasure from "./pages/inventory/UnitsOfMeasure";
import AttributeDefinitions from "./pages/inventory/AttributeDefinitions";
import Items from "./pages/inventory/Items";
import ItemVariants from "./pages/inventory/ItemVariants";
import Warehouses from "./pages/inventory/Warehouses";
import StockBalances from "./pages/inventory/StockBalances";
import StockTransactions from "./pages/inventory/StockTransactions";
import ProcurementDashboard from "./pages/procurement/ProcurementDashboard";
import ProcurementVendors from "./pages/procurement/Vendors";
import PurchaseRequisitions from "./pages/procurement/PurchaseRequisitions";
import RequestForQuotations from "./pages/procurement/RequestForQuotations";
import VendorQuotations from "./pages/procurement/VendorQuotations";
import PurchaseOrders from "./pages/procurement/PurchaseOrders";
import GoodsReceivedNotes from "./pages/procurement/GoodsReceivedNotes";
import ProjectsDashboard from "./pages/projects/ProjectsDashboard";
import ProjectDetail from "./pages/projects/ProjectDetail";
import WBSPage from "./pages/projects/WBSPage";
import BOQPage from "./pages/projects/BOQPage";
import DPRPage from "./pages/projects/DPRPage";
import MaterialRequestPage from "./pages/projects/MaterialRequestPage";
import ExpensePage from "./pages/projects/ExpensePage";
import ClientsPage from "./pages/projects/ClientsPage";
import MilestonePage from "./pages/projects/MilestonePage";
import DrawingsPage from "./pages/projects/DrawingsPage";
import RFIsPage from "./pages/projects/RFIsPage";
import SubmittalsPage from "./pages/projects/SubmittalsPage";
import ResourceAllocationsPage from "./pages/projects/ResourceAllocationsPage";
import QAChecklistsPage from "./pages/projects/QAChecklistsPage";
import NCRsPage from "./pages/projects/NCRsPage";
import SafetyIncidentsPage from "./pages/projects/SafetyIncidentsPage";
import RABillsPage from "./pages/projects/RABillsPage";
import { getCompanyStatus } from "./api/company";

function Placeholder({ name }: { name: string }) {
  return <div className="text-gray-500 text-sm p-4">{name} — coming soon</div>;
}

function LoadingScreen() {
  return (
    <div className="min-h-screen bg-slate-900 flex items-center justify-center">
      <div className="text-center">
        <div className="w-10 h-10 border-4 border-primary-500 border-t-transparent rounded-full animate-spin mx-auto mb-4" />
        <p className="text-slate-400 text-sm">Loading...</p>
      </div>
    </div>
  );
}

export default function App() {
  const [bootState, setBootState] = useState<'loading' | 'setup' | 'ready'>('loading');

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const status = await getCompanyStatus();
        if (cancelled) return;
        setBootState(status.hasCompany ? 'ready' : 'setup');
      } catch {
        if (cancelled) return;
        setBootState('ready');
      }
    })();
    return () => { cancelled = true; };
  }, []);

  if (bootState === 'loading') return <LoadingScreen />;

  if (bootState === 'setup') {
    return (
      <BrowserRouter>
        <Routes>
          <Route path="*" element={<CompanySetup />} />
        </Routes>
      </BrowserRouter>
    );
  }

  return (
    <BrowserRouter>
      <ThemeProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/auth/2fa" element={<TwoFactorPage />} />
          <Route path="/auth/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/access-denied" element={<AccessDeniedPage />} />

          <Route path="/" element={<ProtectedRoute><AppLayout /></ProtectedRoute>}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard" element={<HomePage />} />
            <Route path="hr" element={<HRDashboard />} />
            <Route path="hr/departments" element={<Departments />} />
            <Route path="hr/employees" element={<Employees />} />
            <Route path="hr/designations" element={<Designations />} />
            <Route path="hr/leave" element={<LeaveManagement />} />
            <Route path="hr/attendance" element={<AttendanceManagement />} />
            <Route path="hr/payroll" element={<PayrollManagement />} />
            <Route path="hr/recruitment" element={<Recruitment />} />
            <Route path="hr/performance" element={<PerformanceManagement />} />
            <Route path="accounts" element={<AccountsDashboard />} />
            <Route path="accounts/account-groups" element={<AccountGroups />} />
            <Route path="accounts/fiscal-years" element={<FiscalYears />} />
            <Route path="accounts/chart-of-accounts" element={<ChartOfAccounts />} />
            <Route path="accounts/journal-entries" element={<JournalEntries />} />
            <Route path="accounts/cost-centers" element={<CostCenters />} />
            <Route path="accounts/tax-rates" element={<TaxRates />} />
            <Route path="accounts/bank-accounts" element={<BankAccounts />} />
            <Route path="accounts/customers" element={<AccountsCustomers />} />
            <Route path="accounts/invoices" element={<Invoices />} />
            <Route path="accounts/vendors" element={<AccountsVendors />} />
            <Route path="accounts/bills" element={<Bills />} />
            <Route path="accounts/payments" element={<Payments />} />
            <Route path="accounts/budgets" element={<Budgets />} />
            <Route path="inventory" element={<InventoryDashboard />} />
            <Route path="inventory/types" element={<InventoryTypes />} />
            <Route path="inventory/categories" element={<InventoryCategories />} />
            <Route path="inventory/units-of-measure" element={<UnitsOfMeasure />} />
            <Route path="inventory/attributes" element={<AttributeDefinitions />} />
            <Route path="inventory/items" element={<Items />} />
            <Route path="inventory/item-variants" element={<ItemVariants />} />
            <Route path="inventory/warehouses" element={<Warehouses />} />
            <Route path="inventory/stock-balances" element={<StockBalances />} />
            <Route path="inventory/stock-transactions" element={<StockTransactions />} />
            <Route path="procurement" element={<ProcurementDashboard />} />
            <Route path="procurement/vendors" element={<ProcurementVendors />} />
            <Route path="procurement/purchase-requisitions" element={<PurchaseRequisitions />} />
            <Route path="procurement/rfqs" element={<RequestForQuotations />} />
            <Route path="procurement/quotations" element={<VendorQuotations />} />
            <Route path="procurement/purchase-orders" element={<PurchaseOrders />} />
            <Route path="procurement/grns" element={<GoodsReceivedNotes />} />
            <Route path="projects" element={<ProjectsDashboard />} />
            <Route path="projects/clients" element={<ClientsPage />} />
            <Route path="projects/:id" element={<ProjectDetail />} />
            <Route path="projects/:id/wbs" element={<WBSPage />} />
            <Route path="projects/:id/boq" element={<BOQPage />} />
            <Route path="projects/:id/dpr" element={<DPRPage />} />
            <Route path="projects/:id/materials" element={<MaterialRequestPage />} />
            <Route path="projects/:id/expenses" element={<ExpensePage />} />
            <Route path="projects/:id/milestones" element={<MilestonePage />} />
            <Route path="projects/:id/drawings" element={<DrawingsPage />} />
            <Route path="projects/:id/rfis" element={<RFIsPage />} />
            <Route path="projects/:id/submittals" element={<SubmittalsPage />} />
            <Route path="projects/:id/resource-allocations" element={<ResourceAllocationsPage />} />
            <Route path="projects/:id/qa-checklists" element={<QAChecklistsPage />} />
            <Route path="projects/:id/ncrs" element={<NCRsPage />} />
            <Route path="projects/:id/safety-incidents" element={<SafetyIncidentsPage />} />
            <Route path="projects/:id/ra-bills" element={<RABillsPage />} />
            <Route path="settings/*" element={<Placeholder name="Settings" />} />
            <Route path="profile" element={<MyProfilePage />} />
            <Route path="admin/theme" element={<ThemeSettingsPage />} />
            <Route path="admin/users" element={<ProtectedRoute requiredClaim="Users.View"><UsersPage /></ProtectedRoute>} />
            <Route path="admin/users/new" element={<ProtectedRoute requiredClaim="Users.View"><UserCreatePage /></ProtectedRoute>} />
            <Route path="admin/users/:id" element={<ProtectedRoute requiredClaim="Users.View"><UserDetailPage /></ProtectedRoute>} />
            <Route path="admin/company" element={<ProtectedRoute><CompanySettingsPage /></ProtectedRoute>} />
            <Route path="admin/roles" element={<ProtectedRoute requiredClaim="Users.View"><RolesPage /></ProtectedRoute>} />
            <Route path="admin/access-logs" element={<ProtectedRoute requiredClaim="Users.View"><AccessLogsPage /></ProtectedRoute>} />
          </Route>
        </Routes>
      </ThemeProvider>
    </BrowserRouter>
  );
}
