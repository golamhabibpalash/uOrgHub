using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Leave;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.Leave.Queries;

public record GetLeaveTypesQuery(PaginationRequest Request) : IQuery<PagedResult<LeaveTypeResponseDto>>;
public record GetAllLeaveTypesQuery : IQuery<List<LeaveTypeResponseDto>>;
public record GetLeaveRequestsQuery(PaginationRequest Request, Guid? EmployeeId = null, LeaveStatus? Status = null) : IQuery<PagedResult<LeaveRequestResponseDto>>;
public record GetAllLeaveRequestsQuery : IQuery<List<LeaveRequestResponseDto>>;
public record GetLeaveBalancesQuery(Guid EmployeeId, int? Year = null) : IQuery<List<LeaveBalanceResponseDto>>;

public class GetLeaveTypesQueryHandler : IRequestHandler<GetLeaveTypesQuery, PagedResult<LeaveTypeResponseDto>>
{
    private readonly AppDbContext _context;
    public GetLeaveTypesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<LeaveTypeResponseDto>> Handle(GetLeaveTypesQuery request, CancellationToken ct)
    {
        var query = _context.Set<LeaveType>().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Name);

        var totalCount = await query.CountAsync(ct);
        query = query.ApplySorting(request.Request.SortBy ?? "Name", request.Request.SortDescending);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<LeaveTypeResponseDto>
        {
            Items = items.Select(e => new LeaveTypeResponseDto
            {
                Id = e.Id, Name = e.Name, Code = e.Code, Description = e.Description,
                TotalDaysPerYear = e.TotalDaysPerYear, MaxConsecutiveDays = e.MaxConsecutiveDays,
                MinDaysNotice = e.MinDaysNotice, ApprovalLevels = e.ApprovalLevels,
                IsPaidLeave = e.IsPaidLeave, CarryForward = e.CarryForward,
                MaxCarryForwardDays = e.MaxCarryForwardDays, RequiresDocument = e.RequiresDocument,
                GenderRestriction = e.GenderRestriction, IsActive = e.IsActive, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllLeaveTypesQueryHandler : IRequestHandler<GetAllLeaveTypesQuery, List<LeaveTypeResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllLeaveTypesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<LeaveTypeResponseDto>> Handle(GetAllLeaveTypesQuery request, CancellationToken ct)
    {
        var items = await _context.Set<LeaveType>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new LeaveTypeResponseDto
            {
                Id = x.Id, Name = x.Name, Code = x.Code, Description = x.Description,
                TotalDaysPerYear = x.TotalDaysPerYear, MaxConsecutiveDays = x.MaxConsecutiveDays,
                MinDaysNotice = x.MinDaysNotice, ApprovalLevels = x.ApprovalLevels,
                IsPaidLeave = x.IsPaidLeave, CarryForward = x.CarryForward,
                MaxCarryForwardDays = x.MaxCarryForwardDays, RequiresDocument = x.RequiresDocument,
                GenderRestriction = x.GenderRestriction, IsActive = x.IsActive, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, PagedResult<LeaveRequestResponseDto>>
{
    private readonly AppDbContext _context;
    public GetLeaveRequestsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<LeaveRequestResponseDto>> Handle(GetLeaveRequestsQuery request, CancellationToken ct)
    {
        var query = _context.Set<LeaveRequest>()
            .Include(x => x.Employee).Include(x => x.LeaveType)
            .Include(x => x.Approvals).ThenInclude(x => x.Approver)
            .Where(x => !x.IsDeleted);

        if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);

        var totalCount = await query.CountAsync(ct);
        query = query.ApplySorting(request.Request.SortBy ?? "StartDate", request.Request.SortDescending);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<LeaveRequestResponseDto>
        {
            Items = items.Select(e =>
            {
                var rejectedApproval = e.Approvals.FirstOrDefault(a => a.Status == Models.Enums.ApprovalStatus.Rejected);
                return new LeaveRequestResponseDto
                {
                    Id = e.Id, EmployeeId = e.EmployeeId,
                    EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                    LeaveTypeId = e.LeaveTypeId, LeaveTypeName = e.LeaveType?.Name ?? string.Empty,
                    StartDate = e.StartDate, EndDate = e.EndDate, TotalDays = e.TotalDays,
                    Reason = e.Reason, Status = e.Status,
                    CurrentApprovalLevel = e.CurrentApprovalLevel, CreatedAt = e.CreatedAt,
                    RejectionReason = e.RejectionReason ?? rejectedApproval?.Comments,
                    RejectedBy = rejectedApproval?.Approver != null ? $"{rejectedApproval.Approver.FirstName} {rejectedApproval.Approver.LastName}" : null,
                    RejectedAt = rejectedApproval?.ActionedAt
                };
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllLeaveRequestsQueryHandler : IRequestHandler<GetAllLeaveRequestsQuery, List<LeaveRequestResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllLeaveRequestsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<LeaveRequestResponseDto>> Handle(GetAllLeaveRequestsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<LeaveRequest>()
            .Include(x => x.Employee).Include(x => x.LeaveType)
            .Include(x => x.Approvals).ThenInclude(x => x.Approver)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(ct);

        return items.Select(e =>
        {
            var rejectedApproval = e.Approvals.FirstOrDefault(a => a.Status == Models.Enums.ApprovalStatus.Rejected);
            return new LeaveRequestResponseDto
            {
                Id = e.Id, EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                LeaveTypeId = e.LeaveTypeId, LeaveTypeName = e.LeaveType?.Name ?? string.Empty,
                StartDate = e.StartDate, EndDate = e.EndDate, TotalDays = e.TotalDays,
                Reason = e.Reason, Status = e.Status,
                CurrentApprovalLevel = e.CurrentApprovalLevel, CreatedAt = e.CreatedAt,
                RejectionReason = e.RejectionReason ?? rejectedApproval?.Comments,
                RejectedBy = rejectedApproval?.Approver != null ? $"{rejectedApproval.Approver.FirstName} {rejectedApproval.Approver.LastName}" : null,
                RejectedAt = rejectedApproval?.ActionedAt
            };
        }).ToList();
    }
}

public class GetLeaveBalancesQueryHandler : IRequestHandler<GetLeaveBalancesQuery, List<LeaveBalanceResponseDto>>
{
    private readonly AppDbContext _context;
    public GetLeaveBalancesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<LeaveBalanceResponseDto>> Handle(GetLeaveBalancesQuery request, CancellationToken ct)
    {
        var year = request.Year ?? DateTime.UtcNow.Year;
        var balances = await _context.Set<LeaveBalance>()
            .Include(x => x.Employee).Include(x => x.LeaveType)
            .Where(x => !x.IsDeleted && x.EmployeeId == request.EmployeeId && x.Year == year)
            .ToListAsync(ct);

        return balances.Select(e => new LeaveBalanceResponseDto
        {
            Id = e.Id, EmployeeId = e.EmployeeId,
            EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
            LeaveTypeId = e.LeaveTypeId, LeaveTypeName = e.LeaveType?.Name ?? string.Empty,
            Year = e.Year, TotalAllocated = e.TotalAllocated, TotalUsed = e.TotalUsed,
            TotalPending = e.TotalPending, CarriedForward = e.CarriedForward, Remaining = e.Remaining
        }).ToList();
    }
}
