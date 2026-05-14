using MediatR;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Inventory.Mappings;
using uOrgHub.Inventory.Repositories;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Inventory.Features.Catalog.Commands;

public record CreateInventoryTypeCommand(CreateInventoryTypeDto Dto) : ICommand<InventoryTypeResponseDto>;
public record UpdateInventoryTypeCommand(Guid Id, UpdateInventoryTypeDto Dto) : ICommand<InventoryTypeResponseDto>;
public record DeleteInventoryTypeCommand(Guid Id) : ICommand<Unit>;

public class CreateInventoryTypeCommandHandler : IRequestHandler<CreateInventoryTypeCommand, InventoryTypeResponseDto>
{
    private readonly IInventoryTypeRepository _repo;
    private readonly InventoryTypeMapper _mapper = new();

    public CreateInventoryTypeCommandHandler(IInventoryTypeRepository repo) => _repo = repo;

    public async Task<InventoryTypeResponseDto> Handle(CreateInventoryTypeCommand request, CancellationToken ct)
    {
        if (await _repo.CodeExistsAsync(request.Dto.Code))
            throw new AppException($"Inventory type code '{request.Dto.Code}' already exists.");

        var entity = _mapper.ToEntity(request.Dto);
        entity.CreatedAt = DateTime.UtcNow;
        var created = await _repo.CreateAsync(entity);
        return _mapper.ToDto(created);
    }
}

public class UpdateInventoryTypeCommandHandler : IRequestHandler<UpdateInventoryTypeCommand, InventoryTypeResponseDto>
{
    private readonly IInventoryTypeRepository _repo;
    private readonly InventoryTypeMapper _mapper = new();

    public UpdateInventoryTypeCommandHandler(IInventoryTypeRepository repo) => _repo = repo;

    public async Task<InventoryTypeResponseDto> Handle(UpdateInventoryTypeCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.InventoryType), request.Id);

        if (await _repo.CodeExistsAsync(request.Dto.Code, request.Id))
            throw new AppException($"Inventory type code '{request.Dto.Code}' already exists.");

        _mapper.UpdateEntity(request.Dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }
}

public class DeleteInventoryTypeCommandHandler : IRequestHandler<DeleteInventoryTypeCommand, Unit>
{
    private readonly IInventoryTypeRepository _repo;

    public DeleteInventoryTypeCommandHandler(IInventoryTypeRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteInventoryTypeCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.InventoryType), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
