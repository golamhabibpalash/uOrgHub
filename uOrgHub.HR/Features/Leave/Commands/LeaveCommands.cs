using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Leave;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.HR.Features.Leave.Commands;

public record CreateLeaveTypeCommand(CreateLeaveTypeDto Dto) : ICommand<LeaveTypeResponseDto>;
public record UpdateLeaveTypeCommand(Guid Id, UpdateLeaveTypeDto Dto) : ICommand<LeaveTypeResponseDto>;
public record CreateLeaveRequestCommand(CreateLeaveRequestDto Dto) : ICommand<LeaveRequestResponseDto>;
public record UpdateLeaveRequestCommand(Guid Id, UpdateLeaveRequestDto Dto) : ICommand<LeaveRequestResponseDto>;
public record DeleteLeaveRequestCommand(Guid Id) : ICommand<Unit>;
public record ApproveLeaveRequestCommand(ApproveLeaveRequestDto Dto) : ICommand<LeaveApprovalResponseDto>;
public record CancelLeaveRequestCommand(Guid Id) : ICommand<Unit>;

public class CreateLeaveTypeCommandHandler : IRequestHandler<CreateLeaveTypeCommand, LeaveTypeResponseDto>
{
    private readonly AppDbContext _context;
    public CreateLeaveTypeCommandHandler(AppDbContext context) => _context = context;

    public async Task<LeaveTypeResponseDto> Handle(CreateLeaveTypeCommand request, CancellationToken ct)
    {
        var exists = await _context.Set<LeaveType>().AnyAsync(x => !x.IsDeleted && x.Code == request.Dto.Code, ct);
        if (exists) throw new AppException($"Leave type code '{request.Dto.Code}' already exists.");

        var entity = new LeaveType
        {
            Name = request.Dto.Name, Code = request.Dto.Code, Description = request.Dto.Description,
            TotalDaysPerYear = request.Dto.TotalDaysPerYear,
            MaxConsecutiveDays = request.Dto.MaxConsecutiveDays,
            MinDaysNotice = request.Dto.MinDaysNotice,
            ApprovalLevels = request.Dto.ApprovalLevels, IsPaidLeave = request.Dto.IsPaidLeave,
            CarryForward = request.Dto.CarryForward, MaxCarryForwardDays = request.Dto.MaxCarryForwardDays,
            RequiresDocument = request.Dto.RequiresDocument,
            GenderRestriction = request.Dto.GenderRestriction, IsActive = request.Dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<LeaveType>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return LeaveMappingHelper.MapToDto(entity);
    }
}

public class UpdateLeaveTypeCommandHandler : IRequestHandler<UpdateLeaveTypeCommand, LeaveTypeResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateLeaveTypeCommandHandler(AppDbContext context) => _context = context;

    public async Task<LeaveTypeResponseDto> Handle(UpdateLeaveTypeCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<LeaveType>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(LeaveType), request.Id);

        entity.Name = request.Dto.Name; entity.Description = request.Dto.Description;
        entity.TotalDaysPerYear = request.Dto.TotalDaysPerYear;
        entity.ApprovalLevels = request.Dto.ApprovalLevels; entity.IsPaidLeave = request.Dto.IsPaidLeave;
        entity.CarryForward = request.Dto.CarryForward; entity.MaxCarryForwardDays = request.Dto.MaxCarryForwardDays;
        entity.IsActive = request.Dto.IsActive; entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<LeaveType>().Update(entity);
        await _context.SaveChangesAsync(ct);
        return LeaveMappingHelper.MapToDto(entity);
    }
}

public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, LeaveRequestResponseDto>
{
    private readonly AppDbContext _context;
    public CreateLeaveRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<LeaveRequestResponseDto> Handle(CreateLeaveRequestCommand request, CancellationToken ct)
    {
        if (request.Dto.EndDate.Date < request.Dto.StartDate.Date)
            throw new AppException("Start date cannot be greater than end date.");

        var leaveType = await _context.Set<LeaveType>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Dto.LeaveTypeId, ct)
            ?? throw new NotFoundException(nameof(LeaveType), request.Dto.LeaveTypeId);

        var totalDays = (decimal)(request.Dto.EndDate.Date - request.Dto.StartDate.Date).TotalDays + 1;

        var entity = new LeaveRequest
        {
            EmployeeId = request.Dto.EmployeeId, LeaveTypeId = request.Dto.LeaveTypeId,
            StartDate = request.Dto.StartDate.Date, EndDate = request.Dto.EndDate.Date,
            TotalDays = totalDays, Reason = request.Dto.Reason,
            DocumentPath = request.Dto.DocumentPath,
            Status = LeaveStatus.Pending, CurrentApprovalLevel = 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<LeaveRequest>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        return new LeaveRequestResponseDto
        {
            Id = entity.Id, EmployeeId = entity.EmployeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
            LeaveTypeId = entity.LeaveTypeId, LeaveTypeName = leaveType.Name,
            StartDate = entity.StartDate, EndDate = entity.EndDate, TotalDays = entity.TotalDays,
            Reason = entity.Reason, Status = entity.Status,
            CurrentApprovalLevel = entity.CurrentApprovalLevel, CreatedAt = entity.CreatedAt
        };
    }
}

public class ApproveLeaveRequestCommandHandler : IRequestHandler<ApproveLeaveRequestCommand, LeaveApprovalResponseDto>
{
    private readonly AppDbContext _context;
    public ApproveLeaveRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<LeaveApprovalResponseDto> Handle(ApproveLeaveRequestCommand request, CancellationToken ct)
    {
        var leaveRequest = await _context.Set<LeaveRequest>()
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Dto.LeaveRequestId, ct)
            ?? throw new NotFoundException(nameof(LeaveRequest), request.Dto.LeaveRequestId);

        var approval = new LeaveApproval
        {
            LeaveRequestId = request.Dto.LeaveRequestId,
            ApproverId = request.Dto.ApproverId,
            ApprovalLevel = request.Dto.ApprovalLevel,
            Status = request.Dto.IsApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected,
            Comments = request.Dto.Comments,
            ActionedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<LeaveApproval>().Add(approval);

        if (!request.Dto.IsApproved)
        {
            leaveRequest.Status = LeaveStatus.Rejected;
        }
        else if (request.Dto.ApprovalLevel >= leaveRequest.LeaveType!.ApprovalLevels)
        {
            leaveRequest.Status = LeaveStatus.Approved;
        }
        else
        {
            leaveRequest.CurrentApprovalLevel = request.Dto.ApprovalLevel + 1;
        }
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var approver = await _context.Set<Employee>().FindAsync(approval.ApproverId);
        return new LeaveApprovalResponseDto
        {
            Id = approval.Id, LeaveRequestId = approval.LeaveRequestId,
            ApproverId = approval.ApproverId,
            ApproverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : string.Empty,
            ApprovalLevel = approval.ApprovalLevel, Status = approval.Status,
            Comments = approval.Comments, ActionedAt = approval.ActionedAt
        };
    }
}

public class CancelLeaveRequestCommandHandler : IRequestHandler<CancelLeaveRequestCommand, Unit>
{
    private readonly AppDbContext _context;
    public CancelLeaveRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(CancelLeaveRequestCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<LeaveRequest>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(LeaveRequest), request.Id);

        if (entity.Status != LeaveStatus.Pending)
            throw new AppException("Only pending leave requests can be cancelled.");

        entity.Status = LeaveStatus.Cancelled; entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class UpdateLeaveRequestCommandHandler : IRequestHandler<UpdateLeaveRequestCommand, LeaveRequestResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateLeaveRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<LeaveRequestResponseDto> Handle(UpdateLeaveRequestCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<LeaveRequest>()
            .Include(x => x.Employee).Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(LeaveRequest), request.Id);

        if (entity.Status != LeaveStatus.Pending)
            throw new AppException("Only pending leave requests can be edited.");

        if (request.Dto.EndDate.Date < request.Dto.StartDate.Date)
            throw new AppException("Start date cannot be greater than end date.");

        var leaveType = await _context.Set<LeaveType>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Dto.LeaveTypeId, ct)
            ?? throw new NotFoundException(nameof(LeaveType), request.Dto.LeaveTypeId);

        entity.LeaveTypeId = request.Dto.LeaveTypeId;
        entity.StartDate = request.Dto.StartDate.Date;
        entity.EndDate = request.Dto.EndDate.Date;
        entity.TotalDays = (decimal)(request.Dto.EndDate.Date - request.Dto.StartDate.Date).TotalDays + 1;
        entity.Reason = request.Dto.Reason;
        entity.DocumentPath = request.Dto.DocumentPath;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return new LeaveRequestResponseDto
        {
            Id = entity.Id, EmployeeId = entity.EmployeeId,
            EmployeeName = entity.Employee != null ? $"{entity.Employee.FirstName} {entity.Employee.LastName}" : string.Empty,
            LeaveTypeId = entity.LeaveTypeId, LeaveTypeName = entity.LeaveType?.Name ?? string.Empty,
            StartDate = entity.StartDate, EndDate = entity.EndDate, TotalDays = entity.TotalDays,
            Reason = entity.Reason, Status = entity.Status,
            CurrentApprovalLevel = entity.CurrentApprovalLevel, CreatedAt = entity.CreatedAt
        };
    }
}

public class DeleteLeaveRequestCommandHandler : IRequestHandler<DeleteLeaveRequestCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteLeaveRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteLeaveRequestCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<LeaveRequest>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(LeaveRequest), request.Id);

        if (entity.Status != LeaveStatus.Pending)
            throw new AppException("Only pending leave requests can be deleted.");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

file static class LeaveMappingHelper
{
    internal static LeaveTypeResponseDto MapToDto(LeaveType e) => new()
    {
        Id = e.Id, Name = e.Name, Code = e.Code, Description = e.Description,
        TotalDaysPerYear = e.TotalDaysPerYear, MaxConsecutiveDays = e.MaxConsecutiveDays,
        MinDaysNotice = e.MinDaysNotice, ApprovalLevels = e.ApprovalLevels,
        IsPaidLeave = e.IsPaidLeave, CarryForward = e.CarryForward,
        MaxCarryForwardDays = e.MaxCarryForwardDays, RequiresDocument = e.RequiresDocument,
        GenderRestriction = e.GenderRestriction, IsActive = e.IsActive, CreatedAt = e.CreatedAt
    };
}
