using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.SafetyIncidents.Commands;

public record CreateSafetyIncidentCommand(CreateSafetyIncidentDto Dto) : ICommand<SafetyIncidentResponseDto>;
public record UpdateSafetyIncidentCommand(Guid Id, UpdateSafetyIncidentDto Dto) : ICommand<SafetyIncidentResponseDto>;
public record DeleteSafetyIncidentCommand(Guid Id) : ICommand<Unit>;

public class CreateSafetyIncidentCommandHandler : IRequestHandler<CreateSafetyIncidentCommand, SafetyIncidentResponseDto>
{
    private readonly AppDbContext _context;
    public CreateSafetyIncidentCommandHandler(AppDbContext context) => _context = context;

    public async Task<SafetyIncidentResponseDto> Handle(CreateSafetyIncidentCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), dto.ProjectId);

        var count = await _context.Set<SafetyIncident>().CountAsync(ct);
        var incidentNumber = $"SI-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new SafetyIncident
        {
            ProjectId = dto.ProjectId,
            IncidentNumber = incidentNumber,
            Title = dto.Title,
            Description = dto.Description,
            IncidentDate = dto.IncidentDate,
            Location = dto.Location,
            ReportedById = dto.ReportedById,
            Severity = dto.Severity,
            Status = SafetyIncidentStatus.Reported,
            InjuredPersonName = dto.InjuredPersonName,
            InjuryType = dto.InjuryType,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<SafetyIncident>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return SafetyIncidentMapper.ToDto(entity);
    }
}

public class UpdateSafetyIncidentCommandHandler : IRequestHandler<UpdateSafetyIncidentCommand, SafetyIncidentResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateSafetyIncidentCommandHandler(AppDbContext context) => _context = context;

    public async Task<SafetyIncidentResponseDto> Handle(UpdateSafetyIncidentCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<SafetyIncident>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SafetyIncident), request.Id);

        if (entity.Status == SafetyIncidentStatus.Closed)
            throw new AppException("Closed safety incidents cannot be modified.");

        var dto = request.Dto;
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.IncidentDate = dto.IncidentDate;
        entity.Location = dto.Location;
        entity.Severity = dto.Severity;
        entity.Status = dto.Status;
        entity.InjuredPersonName = dto.InjuredPersonName;
        entity.InjuryType = dto.InjuryType;
        entity.RootCause = dto.RootCause;
        entity.CorrectiveAction = dto.CorrectiveAction;
        entity.PreventiveAction = dto.PreventiveAction;
        entity.InvestigatedById = dto.InvestigatedById;
        entity.InvestigationDate = dto.InvestigationDate;
        entity.ClosedDate = dto.ClosedDate;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return SafetyIncidentMapper.ToDto(entity);
    }
}

public class DeleteSafetyIncidentCommandHandler : IRequestHandler<DeleteSafetyIncidentCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteSafetyIncidentCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteSafetyIncidentCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<SafetyIncident>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SafetyIncident), request.Id);

        if (entity.Status == SafetyIncidentStatus.Closed)
            throw new AppException("Closed safety incidents cannot be deleted.");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class SafetyIncidentMapper
{
    public static SafetyIncidentResponseDto ToDto(SafetyIncident e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        IncidentNumber = e.IncidentNumber,
        Title = e.Title,
        Description = e.Description,
        IncidentDate = e.IncidentDate,
        Location = e.Location,
        ReportedById = e.ReportedById,
        Severity = e.Severity,
        Status = e.Status,
        InjuredPersonName = e.InjuredPersonName,
        InjuryType = e.InjuryType,
        RootCause = e.RootCause,
        CorrectiveAction = e.CorrectiveAction,
        PreventiveAction = e.PreventiveAction,
        InvestigatedById = e.InvestigatedById,
        InvestigationDate = e.InvestigationDate,
        ClosedDate = e.ClosedDate,
        CreatedAt = e.CreatedAt
    };
}
