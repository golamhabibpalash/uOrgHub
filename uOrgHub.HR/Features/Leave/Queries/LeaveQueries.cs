using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Leave;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.Leave.Queries;

public record GetLeaveTypesQuery(PaginationRequest Request) : IQuery<PagedResult<LeaveTypeResponseDto>>;
public record GetLeaveRequestsQuery(PaginationRequest Request, Guid? EmployeeId = null, LeaveStatus? Status = null) : IQuery<PagedResult<LeaveRequestResponseDto>>;
public record GetLeaveBalancesQuery(Guid EmployeeId, int? Year = null) : IQuery<List<LeaveBalanceResponseDto>>;

public class GetLeaveTypesQueryHandler : IRequestHandler<GetLeaveTypesQuery, PagedResult<LeaveTypeResponseDto>>
{
    private readonly AppDbContext _context;
    public GetLeaveTypesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<LeaveTypeResponseDto>> Handle(GetLeaveTypesQuery request, CancellationToken ct)
    {
        var query = _context.Set<LeaveType>().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search));

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Name)
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

public class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, PagedResult<LeaveRequestResponseDto>>
{
    private readonly AppDbContext _context;
    public GetLeaveRequestsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<LeaveRequestResponseDto>> Handle(GetLeaveRequestsQuery request, CancellationToken ct)
    {
        var query = _context.Set<LeaveRequest>()
            .Include(x => x.Employee).Include(x => x.LeaveType)
            .Where(x => !x.IsDeleted);

        if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.StartDate)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<LeaveRequestResponseDto>
        {
            Items = items.Select(e => new LeaveRequestResponseDto
            {
                Id = e.Id, EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                LeaveTypeId = e.LeaveTypeId, LeaveTypeName = e.LeaveType?.Name ?? string.Empty,
                StartDate = e.StartDate, EndDate = e.EndDate, TotalDays = e.TotalDays,
                Reason = e.Reason, Status = e.Status,
                CurrentApprovalLevel = e.CurrentApprovalLevel, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
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
