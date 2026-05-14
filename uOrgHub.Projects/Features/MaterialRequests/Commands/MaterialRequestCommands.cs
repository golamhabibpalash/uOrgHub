using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.MaterialRequests.Commands;

public record CreateMaterialRequestCommand(CreateMaterialRequestDto Dto) : ICommand<MaterialRequestResponseDto>;
public record UpdateMaterialRequestCommand(Guid Id, UpdateMaterialRequestDto Dto) : ICommand<MaterialRequestResponseDto>;
public record DeleteMaterialRequestCommand(Guid Id) : ICommand<Unit>;
public record SubmitMaterialRequestCommand(Guid Id) : ICommand<MaterialRequestResponseDto>;
public record ApproveMaterialRequestCommand(Guid Id, ApproveMaterialRequestDto Dto) : ICommand<MaterialRequestResponseDto>;

public class CreateMaterialRequestCommandHandler : IRequestHandler<CreateMaterialRequestCommand, MaterialRequestResponseDto>
{
    private readonly AppDbContext _context;
    public CreateMaterialRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<MaterialRequestResponseDto> Handle(CreateMaterialRequestCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new AppException($"Project '{dto.ProjectId}' not found.", 404);

        var count = await _context.Set<ProjectMaterialRequest>().CountAsync(ct);
        var requestNumber = $"MRQ-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new ProjectMaterialRequest
        {
            RequestNumber = requestNumber,
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            RequestedById = dto.RequestedById,
            RequestDate = dto.RequestDate,
            RequiredDate = dto.RequiredDate,
            Status = MaterialRequestStatus.Draft,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            entity.Items.Add(new ProjectMaterialRequestItem
            {
                ItemVariantId = itemDto.ItemVariantId,
                BOQItemId = itemDto.BOQItemId,
                RequestedQuantity = itemDto.RequestedQuantity,
                UnitOfMeasure = itemDto.UnitOfMeasure,
                Notes = itemDto.Notes,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Set<ProjectMaterialRequest>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return MaterialRequestMapper.ToDto(entity);
    }
}

public class UpdateMaterialRequestCommandHandler : IRequestHandler<UpdateMaterialRequestCommand, MaterialRequestResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateMaterialRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<MaterialRequestResponseDto> Handle(UpdateMaterialRequestCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectMaterialRequest>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectMaterialRequest), request.Id);

        if (entity.Status != MaterialRequestStatus.Draft)
            throw new AppException("Only Draft material requests can be updated.");

        entity.WBSId = request.Dto.WBSId;
        entity.RequiredDate = request.Dto.RequiredDate;
        entity.Notes = request.Dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return MaterialRequestMapper.ToDto(entity);
    }
}

public class DeleteMaterialRequestCommandHandler : IRequestHandler<DeleteMaterialRequestCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteMaterialRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteMaterialRequestCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectMaterialRequest>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectMaterialRequest), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class SubmitMaterialRequestCommandHandler : IRequestHandler<SubmitMaterialRequestCommand, MaterialRequestResponseDto>
{
    private readonly AppDbContext _context;
    public SubmitMaterialRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<MaterialRequestResponseDto> Handle(SubmitMaterialRequestCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectMaterialRequest>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectMaterialRequest), request.Id);

        if (entity.Status != MaterialRequestStatus.Draft)
            throw new AppException("Only Draft material requests can be submitted.");

        entity.Status = MaterialRequestStatus.Submitted;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return MaterialRequestMapper.ToDto(entity);
    }
}

public class ApproveMaterialRequestCommandHandler : IRequestHandler<ApproveMaterialRequestCommand, MaterialRequestResponseDto>
{
    private readonly AppDbContext _context;
    public ApproveMaterialRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<MaterialRequestResponseDto> Handle(ApproveMaterialRequestCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectMaterialRequest>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectMaterialRequest), request.Id);

        if (entity.Status != MaterialRequestStatus.Submitted)
            throw new AppException("Only Submitted material requests can be approved.");

        entity.Status = MaterialRequestStatus.Approved;
        entity.ApprovedById = request.Dto.ApprovedById;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        foreach (var approval in request.Dto.ApprovedItems)
        {
            var item = entity.Items.FirstOrDefault(i => !i.IsDeleted && i.Id == approval.ItemId);
            if (item != null)
                item.ApprovedQuantity = approval.ApprovedQuantity;
        }

        await _context.SaveChangesAsync(ct);
        return MaterialRequestMapper.ToDto(entity);
    }
}

public static class MaterialRequestMapper
{
    public static MaterialRequestResponseDto ToDto(ProjectMaterialRequest e) => new()
    {
        Id = e.Id,
        RequestNumber = e.RequestNumber,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        RequestedById = e.RequestedById,
        RequestDate = e.RequestDate,
        RequiredDate = e.RequiredDate,
        Status = e.Status,
        Notes = e.Notes,
        ApprovedById = e.ApprovedById,
        ApprovedAt = e.ApprovedAt,
        CreatedAt = e.CreatedAt,
        Items = e.Items
            .Where(i => !i.IsDeleted)
            .Select(i => new MaterialRequestItemResponseDto
            {
                Id = i.Id,
                RequestId = i.RequestId,
                ItemVariantId = i.ItemVariantId,
                BOQItemId = i.BOQItemId,
                RequestedQuantity = i.RequestedQuantity,
                ApprovedQuantity = i.ApprovedQuantity,
                IssuedQuantity = i.IssuedQuantity,
                UnitOfMeasure = i.UnitOfMeasure,
                Notes = i.Notes
            })
            .ToList()
    };
}
