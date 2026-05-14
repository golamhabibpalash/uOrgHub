using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.DPR.Commands;

public record CreateDPRCommand(CreateDPRDto Dto) : ICommand<DPRResponseDto>;
public record UpdateDPRCommand(Guid Id, UpdateDPRDto Dto) : ICommand<DPRResponseDto>;
public record DeleteDPRCommand(Guid Id) : ICommand<Unit>;
public record SubmitDPRCommand(Guid Id) : ICommand<DPRResponseDto>;
public record ApproveDPRCommand(Guid Id, ApproveDPRDto Dto) : ICommand<DPRResponseDto>;

public class CreateDPRCommandHandler : IRequestHandler<CreateDPRCommand, DPRResponseDto>
{
    private readonly AppDbContext _context;
    public CreateDPRCommandHandler(AppDbContext context) => _context = context;

    public async Task<DPRResponseDto> Handle(CreateDPRCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new AppException($"Project '{dto.ProjectId}' not found.", 404);

        var entity = new DailyProgressReport
        {
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            ReportDate = dto.ReportDate,
            ReportedById = dto.ReportedById,
            WeatherCondition = dto.WeatherCondition,
            WorkDone = dto.WorkDone,
            Issues = dto.Issues,
            NextDayPlan = dto.NextDayPlan,
            ManpowerCount = dto.ManpowerCount,
            EquipmentUsed = dto.EquipmentUsed,
            Status = DPRStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<DailyProgressReport>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return DPRMapper.ToDto(entity);
    }
}

public class UpdateDPRCommandHandler : IRequestHandler<UpdateDPRCommand, DPRResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateDPRCommandHandler(AppDbContext context) => _context = context;

    public async Task<DPRResponseDto> Handle(UpdateDPRCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<DailyProgressReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(DailyProgressReport), request.Id);

        if (entity.Status != DPRStatus.Draft)
            throw new AppException("Only Draft DPRs can be updated.");

        var dto = request.Dto;
        entity.WBSId = dto.WBSId;
        entity.WeatherCondition = dto.WeatherCondition;
        entity.WorkDone = dto.WorkDone;
        entity.Issues = dto.Issues;
        entity.NextDayPlan = dto.NextDayPlan;
        entity.ManpowerCount = dto.ManpowerCount;
        entity.EquipmentUsed = dto.EquipmentUsed;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return DPRMapper.ToDto(entity);
    }
}

public class DeleteDPRCommandHandler : IRequestHandler<DeleteDPRCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteDPRCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteDPRCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<DailyProgressReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(DailyProgressReport), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class SubmitDPRCommandHandler : IRequestHandler<SubmitDPRCommand, DPRResponseDto>
{
    private readonly AppDbContext _context;
    public SubmitDPRCommandHandler(AppDbContext context) => _context = context;

    public async Task<DPRResponseDto> Handle(SubmitDPRCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<DailyProgressReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(DailyProgressReport), request.Id);

        if (entity.Status != DPRStatus.Draft)
            throw new AppException("Only Draft DPRs can be submitted.");

        entity.Status = DPRStatus.Submitted;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return DPRMapper.ToDto(entity);
    }
}

public class ApproveDPRCommandHandler : IRequestHandler<ApproveDPRCommand, DPRResponseDto>
{
    private readonly AppDbContext _context;
    public ApproveDPRCommandHandler(AppDbContext context) => _context = context;

    public async Task<DPRResponseDto> Handle(ApproveDPRCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<DailyProgressReport>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(DailyProgressReport), request.Id);

        if (entity.Status != DPRStatus.Submitted)
            throw new AppException("Only Submitted DPRs can be approved.");

        entity.Status = DPRStatus.Approved;
        entity.ApprovedById = request.Dto.ApprovedById;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return DPRMapper.ToDto(entity);
    }
}

public static class DPRMapper
{
    public static DPRResponseDto ToDto(DailyProgressReport e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        ReportDate = e.ReportDate,
        ReportedById = e.ReportedById,
        WeatherCondition = e.WeatherCondition,
        WorkDone = e.WorkDone,
        Issues = e.Issues,
        NextDayPlan = e.NextDayPlan,
        ManpowerCount = e.ManpowerCount,
        EquipmentUsed = e.EquipmentUsed,
        Status = e.Status,
        ApprovedById = e.ApprovedById,
        ApprovedAt = e.ApprovedAt,
        CreatedAt = e.CreatedAt
    };
}
