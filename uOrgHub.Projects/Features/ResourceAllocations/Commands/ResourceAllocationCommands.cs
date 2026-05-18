using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.ResourceAllocations.Commands;

public record CreateResourceAllocationCommand(CreateResourceAllocationDto Dto) : ICommand<ResourceAllocationResponseDto>;
public record UpdateResourceAllocationCommand(Guid Id, UpdateResourceAllocationDto Dto) : ICommand<ResourceAllocationResponseDto>;
public record DeleteResourceAllocationCommand(Guid Id) : ICommand<Unit>;

public class CreateResourceAllocationCommandHandler : IRequestHandler<CreateResourceAllocationCommand, ResourceAllocationResponseDto>
{
    private readonly AppDbContext _context;
    public CreateResourceAllocationCommandHandler(AppDbContext context) => _context = context;

    public async Task<ResourceAllocationResponseDto> Handle(CreateResourceAllocationCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), dto.ProjectId);

        if (dto.PlannedEndDate <= dto.PlannedStartDate)
            throw new AppException("Planned end date must be after planned start date.");

        var plannedCost = dto.PlannedQuantity * dto.UnitCost;

        var entity = new SiteResourceAllocation
        {
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            ResourceType = dto.ResourceType,
            Description = dto.Description,
            EmployeeId = dto.EmployeeId,
            EquipmentCode = dto.EquipmentCode,
            UnitOfMeasure = dto.UnitOfMeasure,
            PlannedStartDate = dto.PlannedStartDate,
            PlannedEndDate = dto.PlannedEndDate,
            PlannedQuantity = dto.PlannedQuantity,
            UnitCost = dto.UnitCost,
            PlannedCost = plannedCost,
            Status = ResourceAllocationStatus.Planned,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<SiteResourceAllocation>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return ResourceAllocationMapper.ToDto(entity);
    }
}

public class UpdateResourceAllocationCommandHandler : IRequestHandler<UpdateResourceAllocationCommand, ResourceAllocationResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateResourceAllocationCommandHandler(AppDbContext context) => _context = context;

    public async Task<ResourceAllocationResponseDto> Handle(UpdateResourceAllocationCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<SiteResourceAllocation>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SiteResourceAllocation), request.Id);

        if (entity.Status == ResourceAllocationStatus.Cancelled)
            throw new AppException("Cancelled allocations cannot be modified.");

        var dto = request.Dto;
        if (dto.PlannedEndDate <= dto.PlannedStartDate)
            throw new AppException("Planned end date must be after planned start date.");

        entity.WBSId = dto.WBSId;
        entity.Description = dto.Description;
        entity.EmployeeId = dto.EmployeeId;
        entity.EquipmentCode = dto.EquipmentCode;
        entity.UnitOfMeasure = dto.UnitOfMeasure;
        entity.PlannedStartDate = dto.PlannedStartDate;
        entity.PlannedEndDate = dto.PlannedEndDate;
        entity.ActualStartDate = dto.ActualStartDate;
        entity.ActualEndDate = dto.ActualEndDate;
        entity.PlannedQuantity = dto.PlannedQuantity;
        entity.ActualQuantity = dto.ActualQuantity;
        entity.UnitCost = dto.UnitCost;
        entity.PlannedCost = dto.PlannedQuantity * dto.UnitCost;
        entity.ActualCost = dto.ActualQuantity * dto.UnitCost;
        entity.Status = dto.Status;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return ResourceAllocationMapper.ToDto(entity);
    }
}

public class DeleteResourceAllocationCommandHandler : IRequestHandler<DeleteResourceAllocationCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteResourceAllocationCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteResourceAllocationCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<SiteResourceAllocation>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SiteResourceAllocation), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public static class ResourceAllocationMapper
{
    public static ResourceAllocationResponseDto ToDto(SiteResourceAllocation e) => new()
    {
        Id = e.Id,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        ResourceType = e.ResourceType,
        Description = e.Description,
        EmployeeId = e.EmployeeId,
        EquipmentCode = e.EquipmentCode,
        UnitOfMeasure = e.UnitOfMeasure,
        PlannedStartDate = e.PlannedStartDate,
        PlannedEndDate = e.PlannedEndDate,
        ActualStartDate = e.ActualStartDate,
        ActualEndDate = e.ActualEndDate,
        PlannedQuantity = e.PlannedQuantity,
        ActualQuantity = e.ActualQuantity,
        UnitCost = e.UnitCost,
        PlannedCost = e.PlannedCost,
        ActualCost = e.ActualCost,
        Status = e.Status,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };
}
