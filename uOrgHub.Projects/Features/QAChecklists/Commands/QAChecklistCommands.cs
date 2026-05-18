using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.QAChecklists.Commands;

public record CreateQAChecklistCommand(CreateQAChecklistDto Dto) : ICommand<QAChecklistResponseDto>;
public record UpdateQAChecklistCommand(Guid Id, UpdateQAChecklistDto Dto) : ICommand<QAChecklistResponseDto>;
public record SubmitQAChecklistCommand(Guid Id, SubmitQAChecklistDto Dto) : ICommand<QAChecklistResponseDto>;
public record DeleteQAChecklistCommand(Guid Id) : ICommand<Unit>;

public class CreateQAChecklistCommandHandler : IRequestHandler<CreateQAChecklistCommand, QAChecklistResponseDto>
{
    private readonly AppDbContext _context;
    public CreateQAChecklistCommandHandler(AppDbContext context) => _context = context;

    public async Task<QAChecklistResponseDto> Handle(CreateQAChecklistCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), dto.ProjectId);

        var count = await _context.Set<QAChecklist>().CountAsync(ct);
        var checklistNumber = $"QA-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new QAChecklist
        {
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            ChecklistNumber = checklistNumber,
            Title = dto.Title,
            InspectionType = dto.InspectionType,
            ScheduledDate = dto.ScheduledDate,
            InspectedById = dto.InspectedById,
            Status = QAChecklistStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            entity.Items.Add(new QAChecklistItem
            {
                Description = itemDto.Description,
                IsRequired = itemDto.IsRequired,
                Sequence = itemDto.Sequence,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Set<QAChecklist>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var created = await _context.Set<QAChecklist>()
            .Include(x => x.Items)
            .FirstAsync(x => x.Id == entity.Id, ct);
        return QAChecklistMapper.ToDto(created);
    }
}

public class UpdateQAChecklistCommandHandler : IRequestHandler<UpdateQAChecklistCommand, QAChecklistResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateQAChecklistCommandHandler(AppDbContext context) => _context = context;

    public async Task<QAChecklistResponseDto> Handle(UpdateQAChecklistCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<QAChecklist>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(QAChecklist), request.Id);

        if (entity.Status == QAChecklistStatus.Passed || entity.Status == QAChecklistStatus.Failed)
            throw new AppException("Completed checklists cannot be modified.");

        var dto = request.Dto;
        entity.WBSId = dto.WBSId;
        entity.Title = dto.Title;
        entity.InspectionType = dto.InspectionType;
        entity.ScheduledDate = dto.ScheduledDate;
        entity.InspectedById = dto.InspectedById;
        entity.Remarks = dto.Remarks;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return QAChecklistMapper.ToDto(entity);
    }
}

public class SubmitQAChecklistCommandHandler : IRequestHandler<SubmitQAChecklistCommand, QAChecklistResponseDto>
{
    private readonly AppDbContext _context;
    public SubmitQAChecklistCommandHandler(AppDbContext context) => _context = context;

    public async Task<QAChecklistResponseDto> Handle(SubmitQAChecklistCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<QAChecklist>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(QAChecklist), request.Id);

        if (entity.Status == QAChecklistStatus.Passed || entity.Status == QAChecklistStatus.Failed)
            throw new AppException("Checklist has already been completed.");

        var dto = request.Dto;

        foreach (var itemUpdate in dto.Items)
        {
            var item = entity.Items.FirstOrDefault(x => !x.IsDeleted && x.Id == itemUpdate.Id);
            if (item != null)
            {
                item.Result = itemUpdate.Result;
                item.Remarks = itemUpdate.Remarks;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        entity.InspectedById = dto.InspectedById;
        entity.InspectedDate = dto.InspectedDate;
        entity.OverallResult = dto.OverallResult;
        entity.Remarks = dto.Remarks;
        entity.Status = dto.OverallResult == InspectionResult.Pass || dto.OverallResult == InspectionResult.ConditionalPass
            ? QAChecklistStatus.Passed
            : QAChecklistStatus.Failed;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return QAChecklistMapper.ToDto(entity);
    }
}

public class DeleteQAChecklistCommandHandler : IRequestHandler<DeleteQAChecklistCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteQAChecklistCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteQAChecklistCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<QAChecklist>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(QAChecklist), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class QAChecklistMapper
{
    public static QAChecklistResponseDto ToDto(QAChecklist e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        ChecklistNumber = e.ChecklistNumber,
        Title = e.Title,
        InspectionType = e.InspectionType,
        Status = e.Status,
        InspectedById = e.InspectedById,
        ScheduledDate = e.ScheduledDate,
        InspectedDate = e.InspectedDate,
        OverallResult = e.OverallResult,
        Remarks = e.Remarks,
        CreatedAt = e.CreatedAt,
        Items = e.Items
            .Where(i => !i.IsDeleted)
            .OrderBy(i => i.Sequence)
            .Select(i => new QAChecklistItemResponseDto
            {
                Id = i.Id,
                Description = i.Description,
                IsRequired = i.IsRequired,
                Sequence = i.Sequence,
                Result = i.Result,
                Remarks = i.Remarks
            }).ToList()
    };
}
