using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.RFIs.Commands;

public record CreateRFICommand(CreateRFIDto Dto) : ICommand<RFIResponseDto>;
public record UpdateRFICommand(Guid Id, UpdateRFIDto Dto) : ICommand<RFIResponseDto>;
public record RespondRFICommand(Guid Id, RespondRFIDto Dto) : ICommand<RFIResponseDto>;
public record CloseRFICommand(Guid Id) : ICommand<RFIResponseDto>;
public record DeleteRFICommand(Guid Id) : ICommand<Unit>;

public class CreateRFICommandHandler : IRequestHandler<CreateRFICommand, RFIResponseDto>
{
    private readonly AppDbContext _context;
    public CreateRFICommandHandler(AppDbContext context) => _context = context;

    public async Task<RFIResponseDto> Handle(CreateRFICommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), dto.ProjectId);

        var count = await _context.Set<ProjectRFI>().CountAsync(ct);
        var rfiNumber = $"RFI-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new ProjectRFI
        {
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            RFINumber = rfiNumber,
            Subject = dto.Subject,
            Description = dto.Description,
            RaisedById = dto.RaisedById,
            RaisedDate = dto.RaisedDate,
            AssignedToId = dto.AssignedToId,
            ResponseDueDate = dto.ResponseDueDate,
            IsUrgent = dto.IsUrgent,
            Status = RFIStatus.Open,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ProjectRFI>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return RFIMapper.ToDto(entity);
    }
}

public class UpdateRFICommandHandler : IRequestHandler<UpdateRFICommand, RFIResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateRFICommandHandler(AppDbContext context) => _context = context;

    public async Task<RFIResponseDto> Handle(UpdateRFICommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectRFI>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectRFI), request.Id);

        if (entity.Status == RFIStatus.Closed || entity.Status == RFIStatus.Cancelled)
            throw new AppException("Closed or cancelled RFIs cannot be modified.");

        var dto = request.Dto;
        entity.WBSId = dto.WBSId;
        entity.Subject = dto.Subject;
        entity.Description = dto.Description;
        entity.AssignedToId = dto.AssignedToId;
        entity.ResponseDueDate = dto.ResponseDueDate;
        entity.IsUrgent = dto.IsUrgent;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return RFIMapper.ToDto(entity);
    }
}

public class RespondRFICommandHandler : IRequestHandler<RespondRFICommand, RFIResponseDto>
{
    private readonly AppDbContext _context;
    public RespondRFICommandHandler(AppDbContext context) => _context = context;

    public async Task<RFIResponseDto> Handle(RespondRFICommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectRFI>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectRFI), request.Id);

        if (entity.Status != RFIStatus.Open)
            throw new AppException("Only open RFIs can be responded to.");

        entity.Response = request.Dto.Response;
        entity.ResponseDate = request.Dto.ResponseDate;
        entity.Status = RFIStatus.Responded;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return RFIMapper.ToDto(entity);
    }
}

public class CloseRFICommandHandler : IRequestHandler<CloseRFICommand, RFIResponseDto>
{
    private readonly AppDbContext _context;
    public CloseRFICommandHandler(AppDbContext context) => _context = context;

    public async Task<RFIResponseDto> Handle(CloseRFICommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectRFI>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectRFI), request.Id);

        if (entity.Status == RFIStatus.Closed)
            throw new AppException("RFI is already closed.");

        entity.Status = RFIStatus.Closed;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return RFIMapper.ToDto(entity);
    }
}

public class DeleteRFICommandHandler : IRequestHandler<DeleteRFICommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteRFICommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteRFICommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectRFI>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectRFI), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class RFIMapper
{
    public static RFIResponseDto ToDto(ProjectRFI e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        RFINumber = e.RFINumber,
        Subject = e.Subject,
        Description = e.Description,
        RaisedById = e.RaisedById,
        RaisedDate = e.RaisedDate,
        AssignedToId = e.AssignedToId,
        ResponseDueDate = e.ResponseDueDate,
        ResponseDate = e.ResponseDate,
        Response = e.Response,
        Status = e.Status,
        IsUrgent = e.IsUrgent,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };
}
