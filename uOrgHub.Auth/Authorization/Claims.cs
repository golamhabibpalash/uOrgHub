namespace uOrgHub.Auth.Authorization;

public static class Claims
{
    public static class Admin
    {
        public static class Users
        {
            public const string View = "Admin.Users.View";
            public const string Create = "Admin.Users.Create";
            public const string Edit = "Admin.Users.Edit";
            public const string Delete = "Admin.Users.Delete";
            public const string AssignRoles = "Admin.Users.AssignRoles";
        }
        public static class Roles
        {
            public const string View = "Admin.Roles.View";
            public const string Create = "Admin.Roles.Create";
            public const string Edit = "Admin.Roles.Edit";
            public const string Delete = "Admin.Roles.Delete";
        }
        public static class ClaimsAdmin
        {
            public const string View = "Admin.Claims.View";
        }
        public static class AccessLogs
        {
            public const string View = "Admin.AccessLogs.View";
        }
        public static class Company
        {
            public const string View = "Admin.Company.View";
            public const string Edit = "Admin.Company.Edit";
        }
        public static class Theme
        {
            public const string View = "Admin.Theme.View";
            public const string Edit = "Admin.Theme.Edit";
        }
    }

    public static class HR
    {
        public static class Employees { public const string View = "HR.Employees.View"; public const string Create = "HR.Employees.Create"; public const string Edit = "HR.Employees.Edit"; public const string Delete = "HR.Employees.Delete"; }
        public static class Departments { public const string View = "HR.Departments.View"; public const string Create = "HR.Departments.Create"; public const string Edit = "HR.Departments.Edit"; public const string Delete = "HR.Departments.Delete"; }
        public static class Designations { public const string View = "HR.Designations.View"; public const string Create = "HR.Designations.Create"; public const string Edit = "HR.Designations.Edit"; public const string Delete = "HR.Designations.Delete"; }
        public static class WorkSchedules { public const string View = "HR.WorkSchedules.View"; public const string Create = "HR.WorkSchedules.Create"; public const string Edit = "HR.WorkSchedules.Edit"; public const string Delete = "HR.WorkSchedules.Delete"; }
        public static class Shifts { public const string View = "HR.Shifts.View"; public const string Create = "HR.Shifts.Create"; public const string Edit = "HR.Shifts.Edit"; public const string Delete = "HR.Shifts.Delete"; }
        public static class AttendanceLogs { public const string View = "HR.AttendanceLogs.View"; public const string Create = "HR.AttendanceLogs.Create"; public const string Edit = "HR.AttendanceLogs.Edit"; public const string Delete = "HR.AttendanceLogs.Delete"; }
        public static class LeaveTypes { public const string View = "HR.LeaveTypes.View"; public const string Create = "HR.LeaveTypes.Create"; public const string Edit = "HR.LeaveTypes.Edit"; public const string Delete = "HR.LeaveTypes.Delete"; }
        public static class LeaveRequests { public const string View = "HR.LeaveRequests.View"; public const string Create = "HR.LeaveRequests.Create"; public const string Edit = "HR.LeaveRequests.Edit"; public const string Delete = "HR.LeaveRequests.Delete"; public const string Approve = "HR.LeaveRequests.Approve"; }
        public static class SalaryGrades { public const string View = "HR.SalaryGrades.View"; public const string Create = "HR.SalaryGrades.Create"; public const string Edit = "HR.SalaryGrades.Edit"; public const string Delete = "HR.SalaryGrades.Delete"; }
        public static class SalaryComponents { public const string View = "HR.SalaryComponents.View"; public const string Create = "HR.SalaryComponents.Create"; public const string Edit = "HR.SalaryComponents.Edit"; public const string Delete = "HR.SalaryComponents.Delete"; }
        public static class PayrollCycles { public const string View = "HR.PayrollCycles.View"; public const string Create = "HR.PayrollCycles.Create"; public const string Edit = "HR.PayrollCycles.Edit"; public const string Delete = "HR.PayrollCycles.Delete"; public const string Process = "HR.PayrollCycles.Process"; }
        public static class ExpenseRequests { public const string View = "HR.ExpenseRequests.View"; public const string Create = "HR.ExpenseRequests.Create"; public const string Edit = "HR.ExpenseRequests.Edit"; public const string Delete = "HR.ExpenseRequests.Delete"; public const string Approve = "HR.ExpenseRequests.Approve"; }
        public static class JobPostings { public const string View = "HR.JobPostings.View"; public const string Create = "HR.JobPostings.Create"; public const string Edit = "HR.JobPostings.Edit"; public const string Delete = "HR.JobPostings.Delete"; }
        public static class Candidates { public const string View = "HR.Candidates.View"; public const string Create = "HR.Candidates.Create"; public const string Edit = "HR.Candidates.Edit"; public const string Delete = "HR.Candidates.Delete"; }
        public static class Applications { public const string View = "HR.Applications.View"; public const string Create = "HR.Applications.Create"; public const string Edit = "HR.Applications.Edit"; public const string Delete = "HR.Applications.Delete"; }
        public static class Interviews { public const string View = "HR.Interviews.View"; public const string Create = "HR.Interviews.Create"; public const string Edit = "HR.Interviews.Edit"; public const string Delete = "HR.Interviews.Delete"; }
        public static class ReviewCycles { public const string View = "HR.ReviewCycles.View"; public const string Create = "HR.ReviewCycles.Create"; public const string Edit = "HR.ReviewCycles.Edit"; public const string Delete = "HR.ReviewCycles.Delete"; }
        public static class Goals { public const string View = "HR.Goals.View"; public const string Create = "HR.Goals.Create"; public const string Edit = "HR.Goals.Edit"; public const string Delete = "HR.Goals.Delete"; }
        public static class PerformanceReviews { public const string View = "HR.PerformanceReviews.View"; public const string Create = "HR.PerformanceReviews.Create"; public const string Edit = "HR.PerformanceReviews.Edit"; public const string Delete = "HR.PerformanceReviews.Delete"; }
    }

    public static class Accounts
    {
        public static class AccountGroups { public const string View = "Accounts.AccountGroups.View"; public const string Create = "Accounts.AccountGroups.Create"; public const string Edit = "Accounts.AccountGroups.Edit"; public const string Delete = "Accounts.AccountGroups.Delete"; }
        public static class ChartOfAccounts { public const string View = "Accounts.ChartOfAccounts.View"; public const string Create = "Accounts.ChartOfAccounts.Create"; public const string Edit = "Accounts.ChartOfAccounts.Edit"; public const string Delete = "Accounts.ChartOfAccounts.Delete"; }
        public static class FiscalYears { public const string View = "Accounts.FiscalYears.View"; public const string Create = "Accounts.FiscalYears.Create"; public const string Edit = "Accounts.FiscalYears.Edit"; public const string Delete = "Accounts.FiscalYears.Delete"; }
        public static class JournalEntries { public const string View = "Accounts.JournalEntries.View"; public const string Create = "Accounts.JournalEntries.Create"; public const string Edit = "Accounts.JournalEntries.Edit"; public const string Delete = "Accounts.JournalEntries.Delete"; public const string Post = "Accounts.JournalEntries.Post"; }
        public static class CostCenters { public const string View = "Accounts.CostCenters.View"; public const string Create = "Accounts.CostCenters.Create"; public const string Edit = "Accounts.CostCenters.Edit"; public const string Delete = "Accounts.CostCenters.Delete"; }
        public static class TaxRates { public const string View = "Accounts.TaxRates.View"; public const string Create = "Accounts.TaxRates.Create"; public const string Edit = "Accounts.TaxRates.Edit"; public const string Delete = "Accounts.TaxRates.Delete"; }
        public static class BankAccounts { public const string View = "Accounts.BankAccounts.View"; public const string Create = "Accounts.BankAccounts.Create"; public const string Edit = "Accounts.BankAccounts.Edit"; public const string Delete = "Accounts.BankAccounts.Delete"; }
        public static class Customers { public const string View = "Accounts.Customers.View"; public const string Create = "Accounts.Customers.Create"; public const string Edit = "Accounts.Customers.Edit"; public const string Delete = "Accounts.Customers.Delete"; }
        public static class Invoices { public const string View = "Accounts.Invoices.View"; public const string Create = "Accounts.Invoices.Create"; public const string Edit = "Accounts.Invoices.Edit"; public const string Delete = "Accounts.Invoices.Delete"; public const string Approve = "Accounts.Invoices.Approve"; }
        public static class Vendors { public const string View = "Accounts.Vendors.View"; public const string Create = "Accounts.Vendors.Create"; public const string Edit = "Accounts.Vendors.Edit"; public const string Delete = "Accounts.Vendors.Delete"; }
        public static class Bills { public const string View = "Accounts.Bills.View"; public const string Create = "Accounts.Bills.Create"; public const string Edit = "Accounts.Bills.Edit"; public const string Delete = "Accounts.Bills.Delete"; public const string Approve = "Accounts.Bills.Approve"; }
        public static class Payments { public const string View = "Accounts.Payments.View"; public const string Create = "Accounts.Payments.Create"; public const string Edit = "Accounts.Payments.Edit"; public const string Delete = "Accounts.Payments.Delete"; }
        public static class Budgets { public const string View = "Accounts.Budgets.View"; public const string Create = "Accounts.Budgets.Create"; public const string Edit = "Accounts.Budgets.Edit"; public const string Delete = "Accounts.Budgets.Delete"; }
    }

    public static class Inventory
    {
        public static class Types { public const string View = "Inventory.Types.View"; public const string Create = "Inventory.Types.Create"; public const string Edit = "Inventory.Types.Edit"; public const string Delete = "Inventory.Types.Delete"; }
        public static class Categories { public const string View = "Inventory.Categories.View"; public const string Create = "Inventory.Categories.Create"; public const string Edit = "Inventory.Categories.Edit"; public const string Delete = "Inventory.Categories.Delete"; }
        public static class UnitsOfMeasure { public const string View = "Inventory.UnitsOfMeasure.View"; public const string Create = "Inventory.UnitsOfMeasure.Create"; public const string Edit = "Inventory.UnitsOfMeasure.Edit"; public const string Delete = "Inventory.UnitsOfMeasure.Delete"; }
        public static class Attributes { public const string View = "Inventory.Attributes.View"; public const string Create = "Inventory.Attributes.Create"; public const string Edit = "Inventory.Attributes.Edit"; public const string Delete = "Inventory.Attributes.Delete"; }
        public static class Items { public const string View = "Inventory.Items.View"; public const string Create = "Inventory.Items.Create"; public const string Edit = "Inventory.Items.Edit"; public const string Delete = "Inventory.Items.Delete"; }
        public static class ItemVariants { public const string View = "Inventory.ItemVariants.View"; public const string Create = "Inventory.ItemVariants.Create"; public const string Edit = "Inventory.ItemVariants.Edit"; public const string Delete = "Inventory.ItemVariants.Delete"; }
        public static class Warehouses { public const string View = "Inventory.Warehouses.View"; public const string Create = "Inventory.Warehouses.Create"; public const string Edit = "Inventory.Warehouses.Edit"; public const string Delete = "Inventory.Warehouses.Delete"; }
        public static class StockBalances { public const string View = "Inventory.StockBalances.View"; }
        public static class StockTransactions { public const string View = "Inventory.StockTransactions.View"; public const string Create = "Inventory.StockTransactions.Create"; public const string Edit = "Inventory.StockTransactions.Edit"; public const string Delete = "Inventory.StockTransactions.Delete"; }
    }

    public static class Procurement
    {
        public static class Vendors { public const string View = "Procurement.Vendors.View"; public const string Create = "Procurement.Vendors.Create"; public const string Edit = "Procurement.Vendors.Edit"; public const string Delete = "Procurement.Vendors.Delete"; }
        public static class PurchaseRequisitions { public const string View = "Procurement.PurchaseRequisitions.View"; public const string Create = "Procurement.PurchaseRequisitions.Create"; public const string Edit = "Procurement.PurchaseRequisitions.Edit"; public const string Delete = "Procurement.PurchaseRequisitions.Delete"; public const string Approve = "Procurement.PurchaseRequisitions.Approve"; }
        public static class RFQs { public const string View = "Procurement.RFQs.View"; public const string Create = "Procurement.RFQs.Create"; public const string Edit = "Procurement.RFQs.Edit"; public const string Delete = "Procurement.RFQs.Delete"; }
        public static class Quotations { public const string View = "Procurement.Quotations.View"; public const string Create = "Procurement.Quotations.Create"; public const string Edit = "Procurement.Quotations.Edit"; public const string Delete = "Procurement.Quotations.Delete"; }
        public static class PurchaseOrders { public const string View = "Procurement.PurchaseOrders.View"; public const string Create = "Procurement.PurchaseOrders.Create"; public const string Edit = "Procurement.PurchaseOrders.Edit"; public const string Delete = "Procurement.PurchaseOrders.Delete"; public const string Approve = "Procurement.PurchaseOrders.Approve"; }
        public static class GRNs { public const string View = "Procurement.GRNs.View"; public const string Create = "Procurement.GRNs.Create"; public const string Edit = "Procurement.GRNs.Edit"; public const string Delete = "Procurement.GRNs.Delete"; }
    }

    public static class Projects
    {
        public static class Projects_ { public const string View = "Projects.Projects.View"; public const string Create = "Projects.Projects.Create"; public const string Edit = "Projects.Projects.Edit"; public const string Delete = "Projects.Projects.Delete"; }
        public static class Clients { public const string View = "Projects.Clients.View"; public const string Create = "Projects.Clients.Create"; public const string Edit = "Projects.Clients.Edit"; public const string Delete = "Projects.Clients.Delete"; }
        public static class BOQs { public const string View = "Projects.BOQs.View"; public const string Create = "Projects.BOQs.Create"; public const string Edit = "Projects.BOQs.Edit"; public const string Delete = "Projects.BOQs.Delete"; }
        public static class WBS { public const string View = "Projects.WBS.View"; public const string Create = "Projects.WBS.Create"; public const string Edit = "Projects.WBS.Edit"; public const string Delete = "Projects.WBS.Delete"; }
        public static class Milestones { public const string View = "Projects.Milestones.View"; public const string Create = "Projects.Milestones.Create"; public const string Edit = "Projects.Milestones.Edit"; public const string Delete = "Projects.Milestones.Delete"; }
        public static class DPRs { public const string View = "Projects.DPRs.View"; public const string Create = "Projects.DPRs.Create"; public const string Edit = "Projects.DPRs.Edit"; public const string Delete = "Projects.DPRs.Delete"; }
        public static class MaterialRequests { public const string View = "Projects.MaterialRequests.View"; public const string Create = "Projects.MaterialRequests.Create"; public const string Edit = "Projects.MaterialRequests.Edit"; public const string Delete = "Projects.MaterialRequests.Delete"; public const string Approve = "Projects.MaterialRequests.Approve"; }
        public static class Expenses { public const string View = "Projects.Expenses.View"; public const string Create = "Projects.Expenses.Create"; public const string Edit = "Projects.Expenses.Edit"; public const string Delete = "Projects.Expenses.Delete"; public const string Approve = "Projects.Expenses.Approve"; }
        public static class Budgets { public const string View = "Projects.Budgets.View"; public const string Create = "Projects.Budgets.Create"; public const string Edit = "Projects.Budgets.Edit"; public const string Delete = "Projects.Budgets.Delete"; }
        public static class Drawings { public const string View = "Projects.Drawings.View"; public const string Create = "Projects.Drawings.Create"; public const string Edit = "Projects.Drawings.Edit"; public const string Delete = "Projects.Drawings.Delete"; }
        public static class RFIs { public const string View = "Projects.RFIs.View"; public const string Create = "Projects.RFIs.Create"; public const string Edit = "Projects.RFIs.Edit"; public const string Delete = "Projects.RFIs.Delete"; }
        public static class Submittals { public const string View = "Projects.Submittals.View"; public const string Create = "Projects.Submittals.Create"; public const string Edit = "Projects.Submittals.Edit"; public const string Delete = "Projects.Submittals.Delete"; }
        public static class ResourceAllocations { public const string View = "Projects.ResourceAllocations.View"; public const string Create = "Projects.ResourceAllocations.Create"; public const string Edit = "Projects.ResourceAllocations.Edit"; public const string Delete = "Projects.ResourceAllocations.Delete"; }
        public static class QAChecklists { public const string View = "Projects.QAChecklists.View"; public const string Create = "Projects.QAChecklists.Create"; public const string Edit = "Projects.QAChecklists.Edit"; public const string Delete = "Projects.QAChecklists.Delete"; }
        public static class NCRs { public const string View = "Projects.NCRs.View"; public const string Create = "Projects.NCRs.Create"; public const string Edit = "Projects.NCRs.Edit"; public const string Delete = "Projects.NCRs.Delete"; }
        public static class SafetyIncidents { public const string View = "Projects.SafetyIncidents.View"; public const string Create = "Projects.SafetyIncidents.Create"; public const string Edit = "Projects.SafetyIncidents.Edit"; public const string Delete = "Projects.SafetyIncidents.Delete"; }
        public static class RABills { public const string View = "Projects.RABills.View"; public const string Create = "Projects.RABills.Create"; public const string Edit = "Projects.RABills.Edit"; public const string Delete = "Projects.RABills.Delete"; public const string Approve = "Projects.RABills.Approve"; }
    }

    public static class Self
    {
        public const string ViewProfile = "Self.ViewProfile";
        public const string EditProfile = "Self.EditProfile";
        public const string ViewPayslip = "Self.ViewPayslip";
        public const string ViewAttendance = "Self.ViewAttendance";
        public const string SubmitAttendance = "Self.SubmitAttendance";
        public const string ViewLeave = "Self.ViewLeave";
        public const string SubmitLeave = "Self.SubmitLeave";
        public const string ViewExpense = "Self.ViewExpense";
        public const string SubmitExpense = "Self.SubmitExpense";
        public const string ViewAccessLogs = "Self.ViewAccessLogs";
    }
}
