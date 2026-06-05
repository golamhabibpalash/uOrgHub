using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Services;

public class MenuService : IMenuService
{
    private static readonly List<MenuItemDto> _allItems =
    [
        new("dashboard", "Dashboard", "LayoutDashboard", "/dashboard", null, null, "main", null),

        new("hr", "HR & Payroll", "Users", null, null, null, "main",
        [
            new("hr-dashboard", "Dashboard", "Users", "/hr", Claims.HR.Employees.View, null, null, null),
            new("hr-departments", "Departments", "Building2", "/hr/departments", Claims.HR.Departments.View, null, null, null),
            new("hr-employees", "Employees", "UserCircle", "/hr/employees", Claims.HR.Employees.View, null, null, null),
            new("hr-designations", "Designations", "Briefcase", "/hr/designations", Claims.HR.Designations.View, null, null, null),
            new("hr-organogram", "Organogram", "GitBranch", "/hr/organogram", Claims.HR.Employees.View, null, null, null),
            new("hr-leave", "Leave", "CalendarClock", "/hr/leave", Claims.HR.LeaveRequests.View, null, null, null),
            new("hr-attendance", "Attendance", "Clock", "/hr/attendance", Claims.HR.AttendanceLogs.View, null, null, null),
            new("hr-payroll", "Payroll", "Wallet", "/hr/payroll", Claims.HR.SalaryGrades.View, null, null, null),
            new("hr-recruitment", "Recruitment", "UserCheck", "/hr/recruitment", Claims.HR.Candidates.View, null, null, null),
            new("hr-performance", "Performance", "Target", "/hr/performance", Claims.HR.ReviewCycles.View, null, null, null),
        ]),

        new("accounts", "Accounts", "Receipt", null, null, null, "main",
        [
            new("acc-dashboard", "Dashboard", "Receipt", "/accounts", Claims.Accounts.AccountGroups.View, null, null, null),
            new("acc-account-groups", "Account Groups", "Layers", "/accounts/account-groups", Claims.Accounts.AccountGroups.View, null, null, null),
            new("acc-fiscal-years", "Fiscal Years", "Calendar", "/accounts/fiscal-years", Claims.Accounts.FiscalYears.View, null, null, null),
            new("acc-chart-of-accounts", "Chart of Accounts", "BookOpen", "/accounts/chart-of-accounts", Claims.Accounts.ChartOfAccounts.View, null, null, null),
            new("acc-journal-entries", "Journal Entries", "FileSpreadsheet", "/accounts/journal-entries", Claims.Accounts.JournalEntries.View, null, null, null),
            new("acc-cost-centers", "Cost Centers", "MapPin", "/accounts/cost-centers", Claims.Accounts.CostCenters.View, null, null, null),
            new("acc-tax-rates", "Tax Rates", "Percent", "/accounts/tax-rates", Claims.Accounts.TaxRates.View, null, null, null),
            new("acc-bank-accounts", "Bank Accounts", "Landmark", "/accounts/bank-accounts", Claims.Accounts.BankAccounts.View, null, null, null),
            new("acc-customers", "Customers", "Users", "/accounts/customers", Claims.Accounts.Customers.View, null, null, null),
            new("acc-invoices", "Invoices", "FileText", "/accounts/invoices", Claims.Accounts.Invoices.View, null, null, null),
            new("acc-vendors", "Vendors", "ShoppingBag", "/accounts/vendors", Claims.Accounts.Vendors.View, null, null, null),
            new("acc-bills", "Bills", "FileText", "/accounts/bills", Claims.Accounts.Bills.View, null, null, null),
            new("acc-payments", "Payments", "CreditCard", "/accounts/payments", Claims.Accounts.Payments.View, null, null, null),
            new("acc-budgets", "Budgets", "PiggyBank", "/accounts/budgets", Claims.Accounts.Budgets.View, null, null, null),
        ]),

        new("inventory", "Inventory", "Box", null, null, null, "main",
        [
            new("inv-dashboard", "Dashboard", "Box", "/inventory", Claims.Inventory.Items.View, null, null, null),
            new("inv-types", "Inventory Types", "Tag", "/inventory/types", Claims.Inventory.Types.View, null, null, null),
            new("inv-categories", "Categories", "Package", "/inventory/categories", Claims.Inventory.Categories.View, null, null, null),
            new("inv-uom", "Units of Measure", "Ruler", "/inventory/units-of-measure", Claims.Inventory.UnitsOfMeasure.View, null, null, null),
            new("inv-attributes", "Attributes", "Tag", "/inventory/attributes", Claims.Inventory.Attributes.View, null, null, null),
            new("inv-items", "Items", "Package", "/inventory/items", Claims.Inventory.Items.View, null, null, null),
            new("inv-item-variants", "Item Variants", "Package", "/inventory/item-variants", Claims.Inventory.ItemVariants.View, null, null, null),
            new("inv-warehouses", "Warehouses", "Warehouse", "/inventory/warehouses", Claims.Inventory.Warehouses.View, null, null, null),
            new("inv-stock-balances", "Stock Balances", "ArrowDownToLine", "/inventory/stock-balances", Claims.Inventory.StockBalances.View, null, null, null),
            new("inv-stock-transactions", "Stock Transactions", "ArrowUpFromLine", "/inventory/stock-transactions", Claims.Inventory.StockTransactions.View, null, null, null),
        ]),

        new("procurement", "Procurement", "ShoppingCart", null, null, null, "main",
        [
            new("proc-dashboard", "Dashboard", "ShoppingCart", "/procurement", Claims.Procurement.Vendors.View, null, null, null),
            new("proc-vendors", "Vendors", "Users", "/procurement/vendors", Claims.Procurement.Vendors.View, null, null, null),
            new("proc-pr", "Purchase Requisitions", "FileText", "/procurement/purchase-requisitions", Claims.Procurement.PurchaseRequisitions.View, null, null, null),
            new("proc-rfqs", "Request for Quotation", "FileSpreadsheet", "/procurement/rfqs", Claims.Procurement.RFQs.View, null, null, null),
            new("proc-quotations", "Vendor Quotations", "Tag", "/procurement/quotations", Claims.Procurement.Quotations.View, null, null, null),
            new("proc-po", "Purchase Orders", "Package", "/procurement/purchase-orders", Claims.Procurement.PurchaseOrders.View, null, null, null),
            new("proc-grns", "Goods Received Notes", "ArrowDownToLine", "/procurement/grns", Claims.Procurement.GRNs.View, null, null, null),
        ]),

        new("projects", "Projects", "HardHat", null, null, null, "main",
        [
            new("proj-all", "All Projects", "HardHat", "/projects", Claims.Projects.Projects_.View, null, null, null),
            new("proj-clients", "Clients", "Users", "/projects/clients", Claims.Projects.Clients.View, null, null, null),
        ]),

        new("admin-users", "Users", "Users", "/admin/users", Claims.Admin.Users.View, null, "admin", null),
        new("admin-roles", "Roles", "ShieldCheck", "/admin/roles", Claims.Admin.Roles.View, null, "admin", null),
        new("admin-access-logs", "Access Logs", "ScrollText", "/admin/access-logs", Claims.Admin.AccessLogs.View, null, "admin", null),
        new("admin-theme", "Theme", "Palette", "/admin/theme", Claims.Admin.Theme.View, null, "admin", null),
        new("admin-company", "Company", "Building2", "/admin/company", Claims.Admin.Company.View, null, "admin", null),

        new("profile", "My Profile", "UserCircle", "/profile", Claims.Self.ViewProfile, null, "profile", null),

        new("settings", "Settings", "Settings", "/settings", null, null, "system", null),
    ];

    public List<MenuItemDto> GetAuthorizedMenu(List<string> userClaims, List<string> userRoles)
    {
        var isAdmin = userRoles.Contains("Admin");

        bool HasAccess(MenuItemDto item)
        {
            if (item.RequiredClaim == null && item.RequiredRole == null) return true;
            if (isAdmin) return true;
            if (item.RequiredClaim != null && userClaims.Contains(item.RequiredClaim)) return true;
            if (item.RequiredRole != null && userRoles.Contains(item.RequiredRole)) return true;
            return false;
        }

        List<MenuItemDto> Filter(List<MenuItemDto> items)
        {
            var result = new List<MenuItemDto>();
            foreach (var item in items)
            {
                if (item.Children is { Count: > 0 })
                {
                    var filteredChildren = Filter(item.Children);
                    if (filteredChildren.Count > 0)
                        result.Add(item with { Children = filteredChildren });
                }
                else if (HasAccess(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        return Filter(_allItems);
    }
}
