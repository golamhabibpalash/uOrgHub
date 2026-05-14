using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.ProjectMilestones.Commands;

public record CreateProjectMilestoneCommand(CreateProjectMilestoneDto Dto) : ICommand<ProjectMilestoneResponseDto>;
public record UpdateProjectMilestoneCommand(Guid Id, UpdateProjectMilestoneDto Dto) : ICommand<ProjectMilestoneResponseDto>;
public record DeleteProjectMilestoneCommand(Guid Id) : ICommand<Unit>;

public class CreateProjectMilestoneCommandHandler : IRequestHandler<CreateProjectMilestoneCommand, ProjectMilestoneResponseDto>
{
    private readonly AppDbContext _context;
    public CreateProjectMilestoneCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectMilestoneResponseDto> Handle(CreateProjectMilestoneCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new AppException($"Project '{dto.ProjectId}' not found.", 404);

        var entity = new ProjectMilestone
        {
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            Title = dto.Title,
            Description = dto.Description,
            PlannedDate = dto.PlannedDate,
            Status = dto.Status,
            IsCritical = dto.IsCritical,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ProjectMilestone>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return MilestoneMapper.ToDto(entity);
    }
}

public class UpdateProjectMilestoneCommandHandler : IRequestHandler<UpdateProjectMilestoneCommand, ProjectMilestoneResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateProjectMilestoneCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectMilestoneResponseDto> Handle(UpdateProjectMilestoneCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectMilestone>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectMilestone), request.Id);

        var dto = request.Dto;
        entity.WBSId = dto.WBSId;
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.PlannedDate = dto.PlannedDate;
        entity.ActualDate = dto.ActualDate;
        entity.Status = dto.Status;
        entity.IsCritical = dto.IsCritical;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return MilestoneMapper.ToDto(entity);
    }
}

public class DeleteProjectMilestoneCommandHandler : IRequestHandler<DeleteProjectMilestoneCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteProjectMilestoneCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteProjectMilestoneCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectMilestone>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectMilestone), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class MilestoneMapper
{
    public static ProjectMilestoneResponseDto ToDto(ProjectMilestone e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        Title = e.Title,
        Description = e.Description,
        PlannedDate = e.PlannedDate,
        ActualDate = e.ActualDate,
        Status = e.Status,
        IsCritical = e.IsCritical,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };
}
