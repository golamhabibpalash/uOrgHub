namespace uOrgHub.API.DTOs;

public record DashboardStatsDto(
    int ActiveProjects,
    decimal TotalProjectValue,
    int TotalEmployees,
    int NewEmployeesThisMonth,
    int InventoryItems,
    int LowStockCount,
    int OpenPOs,
    decimal OpenPOValue,
    decimal MonthlyPayroll,
    int PayrollDueInDays,
    int PendingApprovalsCount,
    List<PendingApprovalDto> PendingApprovalDetails,
    List<LowStockAlertDto> LowStockAlerts,
    List<RecentActivityDto> RecentActivities,
    List<ProjectProgressDto> ProjectProgress,
    List<MonthlyExpenseDataDto> MonthlyExpenseVsBudget,
    List<BudgetUtilizationDto> BudgetUtilization,
    List<UpcomingDeadlineDto> UpcomingDeadlines
);

public record PendingApprovalDto(
    string Id,
    string Type,
    string Title,
    string Module,
    decimal? Amount,
    string RequestedBy,
    DateTime RequestedAt,
    string Urgency,
    string Link
);

public record LowStockAlertDto(
    string Id,
    string ItemName,
    string VariantName,
    decimal CurrentStock,
    decimal ReorderLevel,
    string Unit,
    decimal StockPercent,
    string WarehouseName
);

public record RecentActivityDto(
    string Id,
    string Title,
    string Module,
    DateTime Timestamp,
    string Type
);

public record ProjectProgressDto(
    string Id,
    string ProjectCode,
    string ProjectName,
    decimal CompletionPercent,
    string Status,
    int DaysRemaining,
    decimal ContractValue
);

public record MonthlyExpenseDataDto(
    string Month,
    decimal Budget,
    decimal Actual
);

public record BudgetUtilizationDto(
    string Type,
    decimal Allocated,
    decimal Spent,
    decimal Percent
);

public record UpcomingDeadlineDto(
    string Id,
    string DeadlineType,
    string Reference,
    string Title,
    DateTime DueDate,
    int DaysLeft,
    string Status,
    string Link
);
