using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.BOQ.Commands;
using uOrgHub.Projects.Features.DPR.Commands;
using uOrgHub.Projects.Features.ProjectBudgets.Commands;
using uOrgHub.Projects.Features.ProjectExpenses.Commands;
using uOrgHub.Projects.Features.ProjectMilestones.Commands;
using uOrgHub.Projects.Features.Projects.Commands;
using uOrgHub.Projects.Features.WBS.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.Projects.Queries;

public record GetProjectsQuery(PaginationRequest Request, ProjectStatus? Status = null, Guid? ClientId = null) : IQuery<PagedResult<ProjectResponseDto>>;
public record GetProjectByIdQuery(Guid Id) : IQuery<ProjectResponseDto>;
public record GetProjectDashboardQuery(Guid ProjectId) : IQuery<ProjectDashboardDto>;
public record GetProjectBudgetSummaryQuery(Guid ProjectId) : IQuery<ProjectBudgetSummaryDto>;
public record GetProjectProgressQuery(Guid ProjectId) : IQuery<ProjectProgressDto>;
public record GetProjectTeamQuery(Guid ProjectId) : IQuery<List<ProjectTeamResponseDto>>;
public record GetProjectMilestonesQuery(Guid ProjectId) : IQuery<List<ProjectMilestoneResponseDto>>;
public record GetProjectDPRsQuery(Guid ProjectId, PaginationRequest Request) : IQuery<PagedResult<DPRResponseDto>>;
public record GetProjectExpensesQuery(Guid ProjectId, PaginationRequest Request) : IQuery<PagedResult<ProjectExpenseResponseDto>>;
public record GetProjectWBSTreeQuery(Guid ProjectId) : IQuery<List<WBSResponseDto>>;
public record GetAllProjectsForExportQuery : IQuery<List<ProjectResponseDto>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PagedResult<ProjectResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ProjectResponseDto>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Project>()
            .Include(x => x.Client)
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.ClientId.HasValue)
            query = query.Where(x => x.ClientId == request.ClientId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.ProjectName, x => x.ProjectCode);

        query = query.ApplySorting(request.Request.SortBy ?? "ProjectName", request.Request.SortDescending);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ProjectResponseDto>
        {
            Items = items.Select(e => ProjectMapper.ToDto(e, e.Client?.CompanyName ?? string.Empty, e.Category?.Name ?? string.Empty)).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectResponseDto>
{
    private readonly AppDbContext _context;
    public GetProjectByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<ProjectResponseDto> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<Project>()
            .Include(x => x.Client)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        return ProjectMapper.ToDto(entity, entity.Client?.CompanyName ?? string.Empty, entity.Category?.Name ?? string.Empty);
    }
}

public class GetProjectDashboardQueryHandler : IRequestHandler<GetProjectDashboardQuery, ProjectDashboardDto>
{
    private readonly AppDbContext _context;
    public GetProjectDashboardQueryHandler(AppDbContext context) => _context = context;

    public async Task<ProjectDashboardDto> Handle(GetProjectDashboardQuery request, CancellationToken ct)
    {
        var project = await _context.Set<Project>()
            .Include(x => x.Client)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var budgets = await _context.Set<ProjectBudget>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .ToListAsync(ct);

        var wbsItems = await _context.Set<WorkBreakdownStructure>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .ToListAsync(ct);

        var milestones = await _context.Set<ProjectMilestone>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .OrderBy(x => x.PlannedDate)
            .ToListAsync(ct);

        var team = await _context.Set<ProjectTeam>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId && x.IsActive)
            .ToListAsync(ct);

        var dprCount = await _context.Set<DailyProgressReport>()
            .CountAsync(x => !x.IsDeleted && x.ProjectId == request.ProjectId, ct);

        var totalBudget = budgets.Sum(b => b.RevisedAmount ?? b.AllocatedAmount);
        var totalSpent = budgets.Sum(b => b.SpentAmount);
        var completedWBS = wbsItems.Count(w => w.Status == WBSStatus.Completed);
        var overallCompletion = wbsItems.Count > 0
            ? wbsItems.Average(w => (double)w.CompletionPercent)
            : 0;

        var upcomingMilestones = milestones
            .Where(m => m.Status == MilestoneStatus.Pending)
            .Take(5)
            .Select(MilestoneMapper.ToDto)
            .ToList();

        return new ProjectDashboardDto
        {
            Project = ProjectMapper.ToDto(project, project.Client?.CompanyName ?? string.Empty, project.Category?.Name ?? string.Empty),
            TotalBudget = totalBudget,
            TotalSpent = totalSpent,
            BudgetUtilizationPercent = totalBudget > 0 ? Math.Round(totalSpent / totalBudget * 100, 2) : 0,
            OverallCompletionPercent = (decimal)Math.Round(overallCompletion, 2),
            TotalWBSItems = wbsItems.Count,
            CompletedWBSItems = completedWBS,
            TotalMilestones = milestones.Count,
            AchievedMilestones = milestones.Count(m => m.Status == MilestoneStatus.Achieved),
            TeamCount = team.Count,
            DPRCount = dprCount,
            Team = team.Select(t => ProjectTeamMapper.ToDto(t)).ToList(),
            UpcomingMilestones = upcomingMilestones
        };
    }
}

public class GetProjectBudgetSummaryQueryHandler : IRequestHandler<GetProjectBudgetSummaryQuery, ProjectBudgetSummaryDto>
{
    private readonly AppDbContext _context;
    public GetProjectBudgetSummaryQueryHandler(AppDbContext context) => _context = context;

    public async Task<ProjectBudgetSummaryDto> Handle(GetProjectBudgetSummaryQuery request, CancellationToken ct)
    {
        var project = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var budgets = await _context.Set<ProjectBudget>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .ToListAsync(ct);

        var totalAllocated = budgets.Sum(b => b.AllocatedAmount);
        var totalRevised = budgets.Sum(b => b.RevisedAmount ?? 0);
        var totalSpent = budgets.Sum(b => b.SpentAmount);
        var effective = budgets.Sum(b => b.RevisedAmount ?? b.AllocatedAmount);

        return new ProjectBudgetSummaryDto
        {
            ProjectId = project.Id,
            ProjectCode = project.ProjectCode,
            TotalAllocated = totalAllocated,
            TotalRevised = totalRevised,
            TotalSpent = totalSpent,
            RemainingBudget = effective - totalSpent,
            Budgets = budgets.Select(BudgetMapper.ToDto).ToList()
        };
    }
}

public class GetProjectProgressQueryHandler : IRequestHandler<GetProjectProgressQuery, ProjectProgressDto>
{
    private readonly AppDbContext _context;
    public GetProjectProgressQueryHandler(AppDbContext context) => _context = context;

    public async Task<ProjectProgressDto> Handle(GetProjectProgressQuery request, CancellationToken ct)
    {
        var project = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var wbsItems = await _context.Set<WorkBreakdownStructure>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .ToListAsync(ct);

        var overallCompletion = wbsItems.Count > 0
            ? (decimal)Math.Round(wbsItems.Average(w => (double)w.CompletionPercent), 2)
            : 0;

        var tree = BuildWBSTree(wbsItems, null);

        return new ProjectProgressDto
        {
            ProjectId = project.Id,
            ProjectCode = project.ProjectCode,
            OverallCompletionPercent = overallCompletion,
            TotalWBSItems = wbsItems.Count,
            CompletedWBSItems = wbsItems.Count(w => w.Status == WBSStatus.Completed),
            InProgressWBSItems = wbsItems.Count(w => w.Status == WBSStatus.InProgress),
            WBSTree = tree
        };
    }

    private static List<WBSResponseDto> BuildWBSTree(List<WorkBreakdownStructure> all, Guid? parentId)
    {
        return all
            .Where(x => x.ParentWBSId == parentId)
            .OrderBy(x => x.Sequence)
            .Select(x =>
            {
                var dto = WBSMapper.ToDto(x);
                dto.Children = BuildWBSTree(all, x.Id);
                return dto;
            })
            .ToList();
    }
}

public class GetProjectTeamQueryHandler : IRequestHandler<GetProjectTeamQuery, List<ProjectTeamResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectTeamQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ProjectTeamResponseDto>> Handle(GetProjectTeamQuery request, CancellationToken ct)
    {
        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var team = await _context.Set<ProjectTeam>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .OrderBy(x => x.Role)
            .ToListAsync(ct);

        return team.Select(t => ProjectTeamMapper.ToDto(t)).ToList();
    }
}

public class GetProjectMilestonesQueryHandler : IRequestHandler<GetProjectMilestonesQuery, List<ProjectMilestoneResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectMilestonesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ProjectMilestoneResponseDto>> Handle(GetProjectMilestonesQuery request, CancellationToken ct)
    {
        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var milestones = await _context.Set<ProjectMilestone>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .OrderBy(x => x.PlannedDate)
            .ToListAsync(ct);

        return milestones.Select(MilestoneMapper.ToDto).ToList();
    }
}

public class GetProjectDPRsQueryHandler : IRequestHandler<GetProjectDPRsQuery, PagedResult<DPRResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectDPRsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<DPRResponseDto>> Handle(GetProjectDPRsQuery request, CancellationToken ct)
    {
        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var query = _context.Set<DailyProgressReport>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .AsQueryable();

        query = query.ApplySorting(request.Request.SortBy ?? "ReportDate", request.Request.SortDescending);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<DPRResponseDto>
        {
            Items = items.Select(DPRMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetProjectExpensesQueryHandler : IRequestHandler<GetProjectExpensesQuery, PagedResult<ProjectExpenseResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectExpensesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ProjectExpenseResponseDto>> Handle(GetProjectExpensesQuery request, CancellationToken ct)
    {
        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var query = _context.Set<ProjectExpense>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .AsQueryable();

        query = query.ApplySorting(request.Request.SortBy ?? "ExpenseDate", request.Request.SortDescending);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ProjectExpenseResponseDto>
        {
            Items = items.Select(ExpenseMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetProjectWBSTreeQueryHandler : IRequestHandler<GetProjectWBSTreeQuery, List<WBSResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectWBSTreeQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<WBSResponseDto>> Handle(GetProjectWBSTreeQuery request, CancellationToken ct)
    {
        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var allItems = await _context.Set<WorkBreakdownStructure>()
            .Where(x => !x.IsDeleted && x.ProjectId == request.ProjectId)
            .OrderBy(x => x.Sequence)
            .ToListAsync(ct);

        return BuildWBSTree(allItems, null);
    }

    private static List<WBSResponseDto> BuildWBSTree(List<WorkBreakdownStructure> all, Guid? parentId)
    {
        return all
            .Where(x => x.ParentWBSId == parentId)
            .OrderBy(x => x.Sequence)
            .Select(x =>
            {
                var dto = WBSMapper.ToDto(x);
                dto.Children = BuildWBSTree(all, x.Id);
                return dto;
            })
            .ToList();
    }
}

public class GetAllProjectsForExportQueryHandler : IRequestHandler<GetAllProjectsForExportQuery, List<ProjectResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllProjectsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ProjectResponseDto>> Handle(GetAllProjectsForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<Project>()
            .Include(x => x.Client)
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.ProjectName)
            .Select(x => ProjectMapper.ToDto(x, x.Client.CompanyName, x.Category.Name))
            .ToListAsync(ct);
    }
}
