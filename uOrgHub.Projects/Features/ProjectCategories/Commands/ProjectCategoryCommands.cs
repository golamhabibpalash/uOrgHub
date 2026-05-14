using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.ProjectCategories.Commands;

public record CreateProjectCategoryCommand(CreateProjectCategoryDto Dto) : ICommand<ProjectCategoryResponseDto>;
public record UpdateProjectCategoryCommand(Guid Id, UpdateProjectCategoryDto Dto) : ICommand<ProjectCategoryResponseDto>;
public record DeleteProjectCategoryCommand(Guid Id) : ICommand<Unit>;

public class CreateProjectCategoryCommandHandler : IRequestHandler<CreateProjectCategoryCommand, ProjectCategoryResponseDto>
{
    private readonly AppDbContext _context;
    public CreateProjectCategoryCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectCategoryResponseDto> Handle(CreateProjectCategoryCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        if (await _context.Set<ProjectCategory>().AnyAsync(x => !x.IsDeleted && x.Code == dto.Code, ct))
            throw new AppException($"Category code '{dto.Code}' already exists.");

        var entity = new ProjectCategory
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ProjectCategory>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return ProjectCategoryMapper.ToDto(entity);
    }
}

public class UpdateProjectCategoryCommandHandler : IRequestHandler<UpdateProjectCategoryCommand, ProjectCategoryResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateProjectCategoryCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectCategoryResponseDto> Handle(UpdateProjectCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectCategory>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectCategory), request.Id);

        entity.Name = request.Dto.Name;
        entity.Description = request.Dto.Description;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return ProjectCategoryMapper.ToDto(entity);
    }
}

public class DeleteProjectCategoryCommandHandler : IRequestHandler<DeleteProjectCategoryCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteProjectCategoryCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteProjectCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectCategory>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectCategory), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class ProjectCategoryMapper
{
    public static ProjectCategoryResponseDto ToDto(ProjectCategory e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Code = e.Code,
        Description = e.Description,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };
}
