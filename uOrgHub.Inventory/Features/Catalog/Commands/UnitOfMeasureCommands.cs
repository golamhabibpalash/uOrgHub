using MediatR;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Inventory.Mappings;
using uOrgHub.Inventory.Repositories;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Inventory.Features.Catalog.Commands;

public record CreateUnitOfMeasureCommand(CreateUnitOfMeasureDto Dto) : ICommand<UnitOfMeasureResponseDto>;
public record UpdateUnitOfMeasureCommand(Guid Id, UpdateUnitOfMeasureDto Dto) : ICommand<UnitOfMeasureResponseDto>;
public record DeleteUnitOfMeasureCommand(Guid Id) : ICommand<Unit>;

public class CreateUnitOfMeasureCommandHandler : IRequestHandler<CreateUnitOfMeasureCommand, UnitOfMeasureResponseDto>
{
    private readonly IUnitOfMeasureRepository _repo;
    private readonly UnitOfMeasureMapper _mapper = new();
    public CreateUnitOfMeasureCommandHandler(IUnitOfMeasureRepository repo) => _repo = repo;

    public async Task<UnitOfMeasureResponseDto> Handle(CreateUnitOfMeasureCommand request, CancellationToken ct)
    {
        if (await _repo.AbbreviationExistsAsync(request.Dto.Abbreviation))
            throw new AppException($"Unit abbreviation '{request.Dto.Abbreviation}' already exists.");

        var entity = _mapper.ToEntity(request.Dto);
        entity.CreatedAt = DateTime.UtcNow;
        return _mapper.ToDto(await _repo.CreateAsync(entity));
    }
}

public class UpdateUnitOfMeasureCommandHandler : IRequestHandler<UpdateUnitOfMeasureCommand, UnitOfMeasureResponseDto>
{
    private readonly IUnitOfMeasureRepository _repo;
    private readonly UnitOfMeasureMapper _mapper = new();
    public UpdateUnitOfMeasureCommandHandler(IUnitOfMeasureRepository repo) => _repo = repo;

    public async Task<UnitOfMeasureResponseDto> Handle(UpdateUnitOfMeasureCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.UnitOfMeasure), request.Id);

        if (await _repo.AbbreviationExistsAsync(request.Dto.Abbreviation, request.Id))
            throw new AppException($"Unit abbreviation '{request.Dto.Abbreviation}' already exists.");

        _mapper.UpdateEntity(request.Dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        return _mapper.ToDto(await _repo.UpdateAsync(entity));
    }
}

public class DeleteUnitOfMeasureCommandHandler : IRequestHandler<DeleteUnitOfMeasureCommand, Unit>
{
    private readonly IUnitOfMeasureRepository _repo;
    public DeleteUnitOfMeasureCommandHandler(IUnitOfMeasureRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteUnitOfMeasureCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.UnitOfMeasure), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
