using MediatR;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Inventory.Mappings;
using uOrgHub.Inventory.Repositories;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Inventory.Features.Warehouse.Commands;

public record CreateWarehouseCommand(CreateWarehouseDto Dto) : ICommand<WarehouseResponseDto>;
public record UpdateWarehouseCommand(Guid Id, UpdateWarehouseDto Dto) : ICommand<WarehouseResponseDto>;
public record DeleteWarehouseCommand(Guid Id) : ICommand<Unit>;

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseResponseDto>
{
    private readonly IWarehouseRepository _repo;
    private readonly WarehouseMapper _mapper = new();
    public CreateWarehouseCommandHandler(IWarehouseRepository repo) => _repo = repo;

    public async Task<WarehouseResponseDto> Handle(CreateWarehouseCommand request, CancellationToken ct)
    {
        if (await _repo.CodeExistsAsync(request.Dto.Code))
            throw new AppException($"Warehouse code '{request.Dto.Code}' already exists.");

        var entity = _mapper.ToEntity(request.Dto);
        entity.CreatedAt = DateTime.UtcNow;
        return _mapper.ToDto(await _repo.CreateAsync(entity));
    }
}

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseResponseDto>
{
    private readonly IWarehouseRepository _repo;
    private readonly WarehouseMapper _mapper = new();
    public UpdateWarehouseCommandHandler(IWarehouseRepository repo) => _repo = repo;

    public async Task<WarehouseResponseDto> Handle(UpdateWarehouseCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.Warehouse), request.Id);

        if (await _repo.CodeExistsAsync(request.Dto.Code, request.Id))
            throw new AppException($"Warehouse code '{request.Dto.Code}' already exists.");

        _mapper.UpdateEntity(request.Dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        return _mapper.ToDto(await _repo.UpdateAsync(entity));
    }
}

public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, Unit>
{
    private readonly IWarehouseRepository _repo;
    public DeleteWarehouseCommandHandler(IWarehouseRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteWarehouseCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.Warehouse), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
