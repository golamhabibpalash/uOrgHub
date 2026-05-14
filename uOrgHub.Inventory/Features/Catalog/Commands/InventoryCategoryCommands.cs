using MediatR;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Inventory.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Inventory.Features.Catalog.Commands;

public record CreateInventoryCategoryCommand(CreateInventoryCategoryDto Dto) : ICommand<InventoryCategoryResponseDto>;
public record UpdateInventoryCategoryCommand(Guid Id, UpdateInventoryCategoryDto Dto) : ICommand<InventoryCategoryResponseDto>;
public record DeleteInventoryCategoryCommand(Guid Id) : ICommand<Unit>;

public class CreateInventoryCategoryCommandHandler : IRequestHandler<CreateInventoryCategoryCommand, InventoryCategoryResponseDto>
{
    private readonly IInventoryCategoryRepository _repo;
    private readonly AppDbContext _context;

    public CreateInventoryCategoryCommandHandler(IInventoryCategoryRepository repo, AppDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<InventoryCategoryResponseDto> Handle(CreateInventoryCategoryCommand request, CancellationToken ct)
    {
        if (await _repo.CodeExistsAsync(request.Dto.Code))
            throw new AppException($"Category code '{request.Dto.Code}' already exists.");

        var entity = new Models.Entities.InventoryCategory
        {
            Name = request.Dto.Name,
            Code = request.Dto.Code,
            TypeId = request.Dto.TypeId,
            ParentCategoryId = request.Dto.ParentCategoryId,
            Description = request.Dto.Description,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _repo.CreateAsync(entity);
        return await ToDto(created, ct);
    }

    private async Task<InventoryCategoryResponseDto> ToDto(Models.Entities.InventoryCategory e, CancellationToken ct)
    {
        var type = await _context.Set<Models.Entities.InventoryType>().FindAsync(new object[] { e.TypeId }, ct);
        Models.Entities.InventoryCategory? parent = null;
        if (e.ParentCategoryId.HasValue)
            parent = await _context.Set<Models.Entities.InventoryCategory>().FindAsync(new object[] { e.ParentCategoryId.Value }, ct);

        return new InventoryCategoryResponseDto
        {
            Id = e.Id, Name = e.Name, Code = e.Code,
            TypeId = e.TypeId, TypeName = type?.Name ?? string.Empty,
            ParentCategoryId = e.ParentCategoryId, ParentCategoryName = parent?.Name,
            Description = e.Description, IsActive = e.IsActive, CreatedAt = e.CreatedAt
        };
    }
}

public class UpdateInventoryCategoryCommandHandler : IRequestHandler<UpdateInventoryCategoryCommand, InventoryCategoryResponseDto>
{
    private readonly IInventoryCategoryRepository _repo;
    private readonly AppDbContext _context;

    public UpdateInventoryCategoryCommandHandler(IInventoryCategoryRepository repo, AppDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<InventoryCategoryResponseDto> Handle(UpdateInventoryCategoryCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.InventoryCategory), request.Id);

        if (await _repo.CodeExistsAsync(request.Dto.Code, request.Id))
            throw new AppException($"Category code '{request.Dto.Code}' already exists.");

        entity.Name = request.Dto.Name;
        entity.Code = request.Dto.Code;
        entity.TypeId = request.Dto.TypeId;
        entity.ParentCategoryId = request.Dto.ParentCategoryId;
        entity.Description = request.Dto.Description;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateAsync(entity);

        var type = await _context.Set<Models.Entities.InventoryType>().FindAsync(new object[] { updated.TypeId }, ct);
        Models.Entities.InventoryCategory? parent = null;
        if (updated.ParentCategoryId.HasValue)
            parent = await _context.Set<Models.Entities.InventoryCategory>().FindAsync(new object[] { updated.ParentCategoryId.Value }, ct);

        return new InventoryCategoryResponseDto
        {
            Id = updated.Id, Name = updated.Name, Code = updated.Code,
            TypeId = updated.TypeId, TypeName = type?.Name ?? string.Empty,
            ParentCategoryId = updated.ParentCategoryId, ParentCategoryName = parent?.Name,
            Description = updated.Description, IsActive = updated.IsActive, CreatedAt = updated.CreatedAt
        };
    }
}

public class DeleteInventoryCategoryCommandHandler : IRequestHandler<DeleteInventoryCategoryCommand, Unit>
{
    private readonly IInventoryCategoryRepository _repo;
    public DeleteInventoryCategoryCommandHandler(IInventoryCategoryRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteInventoryCategoryCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.InventoryCategory), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
