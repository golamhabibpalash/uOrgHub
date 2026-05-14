using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.ProjectBudgets.Commands;

public record CreateProjectBudgetCommand(CreateProjectBudgetDto Dto) : ICommand<ProjectBudgetResponseDto>;
public record UpdateProjectBudgetCommand(Guid Id, UpdateProjectBudgetDto Dto) : ICommand<ProjectBudgetResponseDto>;
public record DeleteProjectBudgetCommand(Guid Id) : ICommand<Unit>;

public class CreateProjectBudgetCommandHandler : IRequestHandler<CreateProjectBudgetCommand, ProjectBudgetResponseDto>
{
    private readonly AppDbContext _context;
    public CreateProjectBudgetCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectBudgetResponseDto> Handle(CreateProjectBudgetCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new AppException($"Project '{dto.ProjectId}' not found.", 404);

        var entity = new ProjectBudget
        {
            ProjectId = dto.ProjectId,
            BudgetType = dto.BudgetType,
            FiscalYearId = dto.FiscalYearId,
            AllocatedAmount = dto.AllocatedAmount,
            RevisedAmount = dto.RevisedAmount,
            SpentAmount = 0,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ProjectBudget>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return BudgetMapper.ToDto(entity);
    }
}

public class UpdateProjectBudgetCommandHandler : IRequestHandler<UpdateProjectBudgetCommand, ProjectBudgetResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateProjectBudgetCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectBudgetResponseDto> Handle(UpdateProjectBudgetCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectBudget>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectBudget), request.Id);

        entity.AllocatedAmount = request.Dto.AllocatedAmount;
        entity.RevisedAmount = request.Dto.RevisedAmount;
        entity.Notes = request.Dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return BudgetMapper.ToDto(entity);
    }
}

public class DeleteProjectBudgetCommandHandler : IRequestHandler<DeleteProjectBudgetCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteProjectBudgetCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteProjectBudgetCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectBudget>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectBudget), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class BudgetMapper
{
    public static ProjectBudgetResponseDto ToDto(ProjectBudget e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        BudgetType = e.BudgetType,
        FiscalYearId = e.FiscalYearId,
        AllocatedAmount = e.AllocatedAmount,
        SpentAmount = e.SpentAmount,
        RevisedAmount = e.RevisedAmount,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };
}
