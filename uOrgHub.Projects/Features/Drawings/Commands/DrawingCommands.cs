using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.Drawings.Commands;

public record CreateDrawingCommand(CreateDrawingDto Dto) : ICommand<DrawingResponseDto>;
public record UpdateDrawingCommand(Guid Id, UpdateDrawingDto Dto) : ICommand<DrawingResponseDto>;
public record DeleteDrawingCommand(Guid Id) : ICommand<Unit>;

public class CreateDrawingCommandHandler : IRequestHandler<CreateDrawingCommand, DrawingResponseDto>
{
    private readonly AppDbContext _context;
    public CreateDrawingCommandHandler(AppDbContext context) => _context = context;

    public async Task<DrawingResponseDto> Handle(CreateDrawingCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var project = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), dto.ProjectId);

        var exists = await _context.Set<ProjectDrawing>()
            .AnyAsync(x => !x.IsDeleted && x.ProjectId == dto.ProjectId
                && x.DrawingNumber == dto.DrawingNumber && x.Revision == dto.Revision, ct);
        if (exists)
            throw new AppException($"Drawing '{dto.DrawingNumber}' revision '{dto.Revision}' already exists for this project.");

        var count = await _context.Set<ProjectDrawing>().CountAsync(ct);
        var drawingNumber = string.IsNullOrWhiteSpace(dto.DrawingNumber)
            ? $"DWG-{DateTime.UtcNow.Year}-{(count + 1):D4}"
            : dto.DrawingNumber;

        var entity = new ProjectDrawing
        {
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            DrawingNumber = drawingNumber,
            Title = dto.Title,
            Revision = dto.Revision,
            Discipline = dto.Discipline,
            Status = DrawingStatus.Draft,
            DrawnById = dto.DrawnById,
            CheckedById = dto.CheckedById,
            FilePath = dto.FilePath,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ProjectDrawing>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return DrawingMapper.ToDto(entity);
    }
}

public class UpdateDrawingCommandHandler : IRequestHandler<UpdateDrawingCommand, DrawingResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateDrawingCommandHandler(AppDbContext context) => _context = context;

    public async Task<DrawingResponseDto> Handle(UpdateDrawingCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectDrawing>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectDrawing), request.Id);

        if (entity.Status == DrawingStatus.Obsolete)
            throw new AppException("Obsolete drawings cannot be modified.");

        var dto = request.Dto;
        entity.WBSId = dto.WBSId;
        entity.Title = dto.Title;
        entity.Revision = dto.Revision;
        entity.Discipline = dto.Discipline;
        entity.Status = dto.Status;
        entity.DrawnById = dto.DrawnById;
        entity.CheckedById = dto.CheckedById;
        entity.ApprovedById = dto.ApprovedById;
        entity.IssuedDate = dto.IssuedDate;
        entity.FilePath = dto.FilePath;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return DrawingMapper.ToDto(entity);
    }
}

public class DeleteDrawingCommandHandler : IRequestHandler<DeleteDrawingCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteDrawingCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteDrawingCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectDrawing>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectDrawing), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class DrawingMapper
{
    public static DrawingResponseDto ToDto(ProjectDrawing e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        DrawingNumber = e.DrawingNumber,
        Title = e.Title,
        Revision = e.Revision,
        Discipline = e.Discipline,
        Status = e.Status,
        DrawnById = e.DrawnById,
        CheckedById = e.CheckedById,
        ApprovedById = e.ApprovedById,
        IssuedDate = e.IssuedDate,
        FilePath = e.FilePath,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };
}
