using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.API.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;

namespace uOrgHub.API.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var now = DateTime.UtcNow;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        await LoadProjects();
        await LoadEmployees(firstOfMonth);
        await LoadInventory();
        await LoadProcurement();
        await LoadPayroll(now);
        await LoadPendingApprovals();
        await LoadRecentActivities(now);
        await LoadLowStockAlerts();
        await LoadProjectProgress(now);
        await LoadExpenseVsBudget(now);
        await LoadBudgetUtilization();
        await LoadUpcomingDeadlines(now);

        return new DashboardStatsDto(
            ActiveProjects: _activeProjects,
            TotalProjectValue: _totalProjectValue,
            TotalEmployees: _totalEmployees,
            NewEmployeesThisMonth: _newEmployeesThisMonth,
            InventoryItems: _inventoryItems,
            LowStockCount: _lowStockAlerts.Count,
            OpenPOs: _openPOs,
            OpenPOValue: _openPOValue,
            MonthlyPayroll: _monthlyPayroll,
            PayrollDueInDays: _payrollDueInDays,
            PendingApprovalsCount: _pendingApprovals.Count,
            PendingApprovalDetails: _pendingApprovals,
            LowStockAlerts: _lowStockAlerts,
            RecentActivities: _recentActivities,
            ProjectProgress: _projectProgress,
            MonthlyExpenseVsBudget: _expenseVsBudget,
            BudgetUtilization: _budgetUtilization,
            UpcomingDeadlines: _upcomingDeadlines
        );
    }

    private int _activeProjects;
    private decimal _totalProjectValue;

    private async Task LoadProjects()
    {
        var activeStatuses = new[] { ProjectStatus.Active };
        var valueStatuses = new[] { ProjectStatus.Active, ProjectStatus.Completed, ProjectStatus.OnHold };

        _activeProjects = await _db.Set<Project>()
            .CountAsync(p => !p.IsDeleted && activeStatuses.Contains(p.Status));

        _totalProjectValue = (await _db.Set<Project>()
            .Where(p => !p.IsDeleted && valueStatuses.Contains(p.Status))
            .ToListAsync())
            .Sum(p => p.ContractValue);
    }

    private int _totalEmployees;
    private int _newEmployeesThisMonth;

    private async Task LoadEmployees(DateTime firstOfMonth)
    {
        _totalEmployees = await _db.Set<Employee>()
            .CountAsync(e => !e.IsDeleted && e.Status == EmployeeStatus.Active);

        _newEmployeesThisMonth = await _db.Set<Employee>()
            .CountAsync(e => !e.IsDeleted && e.Status == EmployeeStatus.Active && e.JoiningDate >= firstOfMonth);
    }

    private int _inventoryItems;

    private async Task LoadInventory()
    {
        _inventoryItems = await _db.Set<Item>()
            .CountAsync(i => !i.IsDeleted && i.IsActive);
    }

    private int _openPOs;
    private decimal _openPOValue;

    private async Task LoadProcurement()
    {
        var openStatuses = new[] { POStatus.Sent, POStatus.Confirmed, POStatus.PartiallyReceived };

        _openPOs = await _db.Set<PurchaseOrder>()
            .CountAsync(po => !po.IsDeleted && openStatuses.Contains(po.Status));

        _openPOValue = (await _db.Set<PurchaseOrder>()
            .Where(po => !po.IsDeleted && openStatuses.Contains(po.Status))
            .ToListAsync())
            .Sum(po => po.TotalAmount);
    }

    private decimal _monthlyPayroll;
    private int _payrollDueInDays;

    private async Task LoadPayroll(DateTime now)
    {
        var latestCycle = await _db.Set<PayrollCycle>()
            .Where(pc => !pc.IsDeleted && pc.Status == PayrollStatus.Processed)
            .OrderByDescending(pc => pc.Year)
            .ThenByDescending(pc => pc.Month)
            .FirstOrDefaultAsync();

        if (latestCycle != null)
        {
            _monthlyPayroll = (await _db.Set<PayrollEntry>()
                .Where(pe => !pe.IsDeleted && pe.PayrollCycleId == latestCycle.Id && pe.Status == PayrollStatus.Processed)
                .ToListAsync())
                .Sum(pe => pe.NetSalary);
        }

        var nextDue = await _db.Set<PayrollCycle>()
            .Where(pc => !pc.IsDeleted && pc.Status == PayrollStatus.Draft && pc.EndDate >= now)
            .OrderBy(pc => pc.EndDate)
            .FirstOrDefaultAsync();

        _payrollDueInDays = nextDue != null ? (int)(nextDue.EndDate - now).TotalDays : 30;
    }

    private List<PendingApprovalDto> _pendingApprovals = new();

    private async Task LoadPendingApprovals()
    {
        var results = new List<PendingApprovalDto>();

        var prs = await _db.Set<PurchaseRequisition>()
            .Where(x => !x.IsDeleted && x.Status == PRStatus.Submitted)
            .ToListAsync();
        foreach (var pr in prs)
            results.Add(new PendingApprovalDto(
                pr.Id.ToString(), "PR", $"Purchase Requisition – {pr.PRNumber}",
                "Procurement", null, "", pr.CreatedAt, "medium",
                "/procurement/purchase-requisitions"));

        var leaves = await _db.Set<LeaveRequest>()
            .Where(x => !x.IsDeleted && x.Status == LeaveStatus.Pending)
            .Include(x => x.Employee)
            .ToListAsync();
        foreach (var l in leaves)
            results.Add(new PendingApprovalDto(
                l.Id.ToString(), "Leave",
                $"Leave – {(l.Employee?.FirstName ?? "")} ({(l.EndDate - l.StartDate).Days + 1} days)",
                "HR", null, l.Employee?.FirstName ?? "", l.CreatedAt, "medium",
                "/hr/leave"));

        var invoices = await _db.Set<Invoice>()
            .Where(x => !x.IsDeleted && x.Status == InvoiceStatus.Draft)
            .ToListAsync();
        foreach (var inv in invoices)
            results.Add(new PendingApprovalDto(
                inv.Id.ToString(), "Expense",
                $"Invoice {inv.InvoiceNumber} – {inv.TotalAmount:N0}",
                "Accounts", inv.TotalAmount, "", inv.CreatedAt, "medium",
                "/accounts/invoices"));

        _pendingApprovals = results.OrderByDescending(x => x.RequestedAt).Take(10).ToList();
    }

    private List<LowStockAlertDto> _lowStockAlerts = new();

    private async Task LoadLowStockAlerts()
    {
        var alerts = await (from sb in _db.Set<StockBalance>().Where(sb => !sb.IsDeleted)
                            join v in _db.Set<ItemVariant>().Where(v => !v.IsDeleted) on sb.ItemVariantId equals v.Id
                            join i in _db.Set<Item>().Where(i => !i.IsDeleted) on v.ItemId equals i.Id
                            join w in _db.Set<Warehouse>().Where(w => !w.IsDeleted) on sb.WarehouseId equals w.Id
                            where sb.QuantityOnHand < i.ReorderLevel
                            select new LowStockAlertDto(
                                sb.Id.ToString(),
                                i.BaseName ?? "",
                                v.VariantName ?? "",
                                sb.QuantityOnHand,
                                i.ReorderLevel,
                                "",
                                i.ReorderLevel > 0
                                    ? Math.Round(sb.QuantityOnHand / i.ReorderLevel * 100, 0)
                                    : 0,
                                w.Name ?? ""
                            ))
                            .Take(10)
                            .ToListAsync();

        _lowStockAlerts = alerts;
    }

    private List<RecentActivityDto> _recentActivities = new();

    private async Task LoadRecentActivities(DateTime now)
    {
        var logs = await _db.Set<UserAccessLog>()
            .Where(l => l.CreatedAt >= now.AddDays(-7))
            .OrderByDescending(l => l.CreatedAt)
            .Take(10)
            .ToListAsync();

        _recentActivities = logs.Select(l => new RecentActivityDto(
            l.Id.ToString(),
            $"{l.Action} – {l.Module}" + (l.EntityType != null ? $" ({l.EntityType})" : ""),
            l.Module ?? "",
            l.CreatedAt,
            l.IsSuccess ? "success" : "warning"
        )).ToList();
    }

    private List<ProjectProgressDto> _projectProgress = new();

    private async Task LoadProjectProgress(DateTime now)
    {
        var trackingStatuses = new[] { ProjectStatus.Active, ProjectStatus.Completed, ProjectStatus.OnHold };

        var projects = await _db.Set<Project>()
            .Where(p => !p.IsDeleted && trackingStatuses.Contains(p.Status))
            .ToListAsync();

        _projectProgress = projects.Select(p =>
        {
            var totalDays = (p.PlannedEndDate - p.StartDate).Days;
            var elapsedDays = (now - p.StartDate).Days;
            var completion = totalDays > 0 ? Math.Round((decimal)elapsedDays / totalDays * 100, 0) : 0;
            var daysRemaining = (p.PlannedEndDate - now).Days;

            string status;
            if (p.Status == ProjectStatus.Completed) status = "Completed";
            else if (daysRemaining < 0) status = "Delayed";
            else if (completion >= 90) status = "Ahead";
            else status = "On Track";

            return new ProjectProgressDto(
                p.Id.ToString(),
                p.ProjectCode ?? "",
                p.ProjectName ?? "",
                Math.Min(completion, 100),
                status,
                Math.Max(daysRemaining, 0),
                p.ContractValue
            );
        }).OrderByDescending(p => p.ContractValue).Take(10).ToList();
    }

    private List<MonthlyExpenseDataDto> _expenseVsBudget = new();

    private async Task LoadExpenseVsBudget(DateTime now)
    {
        var months = new List<MonthlyExpenseDataDto>();

        for (int i = 5; i >= 0; i--)
        {
            var month = now.AddMonths(-i);
            var monthInt = month.Year * 100 + month.Month;

            var budget = (await _db.Set<BudgetLine>()
                .Where(bl => !bl.IsDeleted && bl.Period == monthInt)
                .ToListAsync())
                .Sum(bl => bl.PlannedAmount);

            var actual = (await _db.Set<JournalEntryLine>()
                .Where(jl => !jl.IsDeleted && jl.JournalEntry != null
                    && jl.JournalEntry.EntryDate.Year == month.Year
                    && jl.JournalEntry.EntryDate.Month == month.Month
                    && jl.JournalEntry.Status == JournalEntryStatus.Posted)
                .ToListAsync())
                .Sum(jl => jl.DebitAmount);

            months.Add(new MonthlyExpenseDataDto(month.ToString("MMM"), budget, actual));
        }

        _expenseVsBudget = months;
    }

    private List<BudgetUtilizationDto> _budgetUtilization = new();

    private async Task LoadBudgetUtilization()
    {
        var budgets = await _db.Set<ProjectBudget>()
            .Where(pb => !pb.IsDeleted)
            .ToListAsync();

        _budgetUtilization = budgets
            .GroupBy(pb => pb.BudgetType)
            .Select(g => new BudgetUtilizationDto(
                g.Key.ToString(),
                g.Sum(x => x.AllocatedAmount),
                g.Sum(x => x.SpentAmount),
                0
            ))
            .ToList()
            .Select(b => b with
            {
                Percent = b.Allocated > 0 ? Math.Round(b.Spent / b.Allocated * 100, 0) : 0
            })
            .ToList();
    }

    private List<UpcomingDeadlineDto> _upcomingDeadlines = new();

    private async Task LoadUpcomingDeadlines(DateTime now)
    {
        var results = new List<UpcomingDeadlineDto>();

        var milestones = await _db.Set<ProjectMilestone>()
            .Where(m => !m.IsDeleted && m.Status != MilestoneStatus.Achieved && m.PlannedDate >= now)
            .OrderBy(m => m.PlannedDate)
            .Take(5)
            .ToListAsync();
        foreach (var m in milestones)
        {
            var daysLeft = (int)(m.PlannedDate - now).TotalDays;
            results.Add(new UpcomingDeadlineDto(
                m.Id.ToString(), "Project Milestone", "",
                m.Title ?? "", m.PlannedDate, daysLeft, m.Status.ToString(), "/projects"));
        }

        var pendingPOStatuses = new[] { POStatus.Sent, POStatus.Confirmed, POStatus.PartiallyReceived };
        var pendingPOs = await _db.Set<PurchaseOrder>()
            .Where(po => !po.IsDeleted
                && po.ExpectedDeliveryDate >= now
                && pendingPOStatuses.Contains(po.Status))
            .OrderBy(po => po.ExpectedDeliveryDate)
            .Take(5)
            .ToListAsync();
        foreach (var po in pendingPOs)
        {
            var daysLeft = (int)(po.ExpectedDeliveryDate - now).TotalDays;
            results.Add(new UpcomingDeadlineDto(
                po.Id.ToString(), "Purchase Order", po.PONumber,
                $"Delivery expected – {po.PONumber}", po.ExpectedDeliveryDate,
                daysLeft, po.Status.ToString(), "/procurement/purchase-orders"));
        }

        _upcomingDeadlines = results
            .OrderBy(d => d.DueDate)
            .Take(10)
            .ToList();
    }
}
