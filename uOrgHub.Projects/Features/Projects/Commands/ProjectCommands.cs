using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.Projects.Commands;

public record CreateProjectCommand(CreateProjectDto Dto) : ICommand<ProjectResponseDto>;
public record UpdateProjectCommand(Guid Id, UpdateProjectDto Dto) : ICommand<ProjectResponseDto>;
public record DeleteProjectCommand(Guid Id) : ICommand<Unit>;
public record AddProjectTeamMemberCommand(Guid ProjectId, AddProjectTeamMemberDto Dto) : ICommand<ProjectTeamResponseDto>;
public record UpdateProjectTeamMemberCommand(Guid ProjectId, Guid MemberId, UpdateProjectTeamMemberDto Dto) : ICommand<ProjectTeamResponseDto>;
public record RemoveProjectTeamMemberCommand(Guid ProjectId, Guid EmployeeId) : ICommand<Unit>;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectResponseDto>
{
    private readonly AppDbContext _context;
    public CreateProjectCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectResponseDto> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        if (dto.ContractValue < 0)
            throw new AppException("Contract value cannot be negative.");
        if (dto.PlannedEndDate <= dto.StartDate)
            throw new AppException("Planned end date must be after start date.");

        var count = await _context.Set<Project>().CountAsync(ct);
        var code = $"PRJ-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new Project
        {
            ProjectCode = code,
            ProjectName = dto.ProjectName,
            ClientId = dto.ClientId,
            CategoryId = dto.CategoryId,
            ProjectManagerId = dto.ProjectManagerId,
            Location = dto.Location,
            SiteAddress = dto.SiteAddress,
            StartDate = dto.StartDate,
            PlannedEndDate = dto.PlannedEndDate,
            ContractValue = dto.ContractValue,
            Status = dto.Status,
            Priority = dto.Priority,
            Description = dto.Description,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<Project>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var costCenter = new CostCenter
        {
            Code = code,
            Name = dto.ProjectName,
            Description = $"Auto-created for project {code}",
            ProjectId = entity.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<CostCenter>().Add(costCenter);
        await _context.SaveChangesAsync(ct);

        return await ProjectMapper.ToDtoWithIncludes(_context, entity.Id, ct);
    }
}

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateProjectCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectResponseDto> Handle(UpdateProjectCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        if (dto.ContractValue < 0)
            throw new AppException("Contract value cannot be negative.");
        if (dto.PlannedEndDate <= dto.StartDate)
            throw new AppException("Planned end date must be after start date.");

        var entity = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        if (dto.Status == ProjectStatus.Active && entity.StartDate == default)
            throw new AppException("Start date must be set before activating the project.");

        if (dto.Status == ProjectStatus.Completed && !dto.ActualEndDate.HasValue)
            entity.ActualEndDate = DateTime.UtcNow;
        else
            entity.ActualEndDate = dto.ActualEndDate;

        entity.ProjectName = dto.ProjectName;
        entity.ClientId = dto.ClientId;
        entity.CategoryId = dto.CategoryId;
        entity.ProjectManagerId = dto.ProjectManagerId;
        entity.Location = dto.Location;
        entity.SiteAddress = dto.SiteAddress;
        entity.StartDate = dto.StartDate;
        entity.PlannedEndDate = dto.PlannedEndDate;
        entity.ContractValue = dto.ContractValue;
        entity.Status = dto.Status;
        entity.Priority = dto.Priority;
        entity.Description = dto.Description;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return await ProjectMapper.ToDtoWithIncludes(_context, entity.Id, ct);
    }
}

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteProjectCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class AddProjectTeamMemberCommandHandler : IRequestHandler<AddProjectTeamMemberCommand, ProjectTeamResponseDto>
{
    private readonly AppDbContext _context;
    public AddProjectTeamMemberCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectTeamResponseDto> Handle(AddProjectTeamMemberCommand request, CancellationToken ct)
    {
        var project = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var dto = request.Dto;

        if (dto.Role == TeamRole.ProjectManager)
        {
            var existingPM = await _context.Set<ProjectTeam>()
                .AnyAsync(x => !x.IsDeleted && x.ProjectId == request.ProjectId
                    && x.Role == TeamRole.ProjectManager && x.IsActive, ct);
            if (existingPM)
                throw new AppException("This project already has an active Project Manager.");
        }

        var alreadyMember = await _context.Set<ProjectTeam>()
            .AnyAsync(x => !x.IsDeleted && x.ProjectId == request.ProjectId
                && x.EmployeeId == dto.EmployeeId && x.IsActive, ct);
        if (alreadyMember)
            throw new AppException("This employee is already an active member of the project team.");

        var member = new ProjectTeam
        {
            ProjectId = request.ProjectId,
            EmployeeId = dto.EmployeeId,
            Role = dto.Role,
            JoinedDate = dto.JoinedDate,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ProjectTeam>().Add(member);
        await _context.SaveChangesAsync(ct);
        return ProjectTeamMapper.ToDto(member);
    }
}

public class UpdateProjectTeamMemberCommandHandler : IRequestHandler<UpdateProjectTeamMemberCommand, ProjectTeamResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateProjectTeamMemberCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectTeamResponseDto> Handle(UpdateProjectTeamMemberCommand request, CancellationToken ct)
    {
        var member = await _context.Set<ProjectTeam>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.MemberId && x.ProjectId == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(ProjectTeam), request.MemberId);

        member.Role = request.Dto.Role;
        member.LeftDate = request.Dto.LeftDate;
        member.IsActive = request.Dto.IsActive;
        member.Notes = request.Dto.Notes;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return ProjectTeamMapper.ToDto(member);
    }
}

public class RemoveProjectTeamMemberCommandHandler : IRequestHandler<RemoveProjectTeamMemberCommand, Unit>
{
    private readonly AppDbContext _context;
    public RemoveProjectTeamMemberCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(RemoveProjectTeamMemberCommand request, CancellationToken ct)
    {
        var member = await _context.Set<ProjectTeam>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.ProjectId == request.ProjectId
                && x.EmployeeId == request.EmployeeId && x.IsActive, ct)
            ?? throw new AppException("Team member not found for this project.", 404);

        member.IsDeleted = true;
        member.DeletedAt = DateTime.UtcNow;
        member.IsActive = false;
        member.LeftDate = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class ProjectMapper
{
    public static ProjectResponseDto ToDto(Project e, string clientName = "", string categoryName = "") => new()
    {
        Id = e.Id,
        ProjectCode = e.ProjectCode,
        ProjectName = e.ProjectName,
        ClientId = e.ClientId,
        ClientName = clientName,
        CategoryId = e.CategoryId,
        CategoryName = categoryName,
        ProjectManagerId = e.ProjectManagerId,
        Location = e.Location,
        SiteAddress = e.SiteAddress,
        StartDate = e.StartDate,
        PlannedEndDate = e.PlannedEndDate,
        ActualEndDate = e.ActualEndDate,
        ContractValue = e.ContractValue,
        Status = e.Status,
        Priority = e.Priority,
        Description = e.Description,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };

    public static async Task<ProjectResponseDto> ToDtoWithIncludes(AppDbContext context, Guid id, CancellationToken ct)
    {
        var e = await context.Set<Project>()
            .Include(x => x.Client)
            .Include(x => x.Category)
            .FirstAsync(x => x.Id == id, ct);

        return ToDto(e, e.Client?.CompanyName ?? string.Empty, e.Category?.Name ?? string.Empty);
    }
}

public static class ProjectTeamMapper
{
    public static ProjectTeamResponseDto ToDto(ProjectTeam e, string employeeName = "") => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        EmployeeId = e.EmployeeId,
        EmployeeName = employeeName,
        Role = e.Role,
        JoinedDate = e.JoinedDate,
        LeftDate = e.LeftDate,
        IsActive = e.IsActive,
        Notes = e.Notes
    };
}
