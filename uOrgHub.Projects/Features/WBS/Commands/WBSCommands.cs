using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.WBS.Commands;

public record CreateWBSCommand(CreateWBSDto Dto) : ICommand<WBSResponseDto>;
public record UpdateWBSCommand(Guid Id, UpdateWBSDto Dto) : ICommand<WBSResponseDto>;
public record DeleteWBSCommand(Guid Id) : ICommand<Unit>;

public class CreateWBSCommandHandler : IRequestHandler<CreateWBSCommand, WBSResponseDto>
{
    private readonly AppDbContext _context;
    public CreateWBSCommandHandler(AppDbContext context) => _context = context;

    public async Task<WBSResponseDto> Handle(CreateWBSCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new AppException($"Project '{dto.ProjectId}' not found.", 404);

        if (await _context.Set<WorkBreakdownStructure>()
            .AnyAsync(x => !x.IsDeleted && x.ProjectId == dto.ProjectId && x.WBSCode == dto.WBSCode, ct))
            throw new AppException($"WBS code '{dto.WBSCode}' already exists for this project.");

        int level = 1;
        if (dto.ParentWBSId.HasValue)
        {
            var parent = await _context.Set<WorkBreakdownStructure>()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ParentWBSId.Value, ct)
                ?? throw new AppException("Parent WBS item not found.", 404);
            level = parent.Level + 1;
        }

        var entity = new WorkBreakdownStructure
        {
            ProjectId = dto.ProjectId,
            ParentWBSId = dto.ParentWBSId,
            WBSCode = dto.WBSCode,
            Title = dto.Title,
            Description = dto.Description,
            Level = level,
            PlannedStartDate = dto.PlannedStartDate,
            PlannedEndDate = dto.PlannedEndDate,
            PlannedDuration = dto.PlannedDuration,
            Status = dto.Status,
            Sequence = dto.Sequence,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<WorkBreakdownStructure>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return WBSMapper.ToDto(entity);
    }
}

public class UpdateWBSCommandHandler : IRequestHandler<UpdateWBSCommand, WBSResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateWBSCommandHandler(AppDbContext context) => _context = context;

    public async Task<WBSResponseDto> Handle(UpdateWBSCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<WorkBreakdownStructure>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(WorkBreakdownStructure), request.Id);

        var dto = request.Dto;
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.PlannedStartDate = dto.PlannedStartDate;
        entity.PlannedEndDate = dto.PlannedEndDate;
        entity.ActualStartDate = dto.ActualStartDate;
        entity.ActualEndDate = dto.ActualEndDate;
        entity.PlannedDuration = dto.PlannedDuration;
        entity.Status = dto.Status;
        entity.CompletionPercent = dto.CompletionPercent;
        entity.Sequence = dto.Sequence;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return WBSMapper.ToDto(entity);
    }
}

public class DeleteWBSCommandHandler : IRequestHandler<DeleteWBSCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteWBSCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteWBSCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<WorkBreakdownStructure>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(WorkBreakdownStructure), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class WBSMapper
{
    public static WBSResponseDto ToDto(WorkBreakdownStructure e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        ParentWBSId = e.ParentWBSId,
        WBSCode = e.WBSCode,
        Title = e.Title,
        Description = e.Description,
        Level = e.Level,
        PlannedStartDate = e.PlannedStartDate,
        PlannedEndDate = e.PlannedEndDate,
        ActualStartDate = e.ActualStartDate,
        ActualEndDate = e.ActualEndDate,
        PlannedDuration = e.PlannedDuration,
        Status = e.Status,
        CompletionPercent = e.CompletionPercent,
        Sequence = e.Sequence
    };
}
