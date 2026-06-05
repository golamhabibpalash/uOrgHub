using MediatR;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Mappings;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.HR.Features.CoreHR.Commands;

public record CreateDesignationCommand(CreateDesignationDto Dto) : ICommand<DesignationResponseDto>;
public record UpdateDesignationCommand(Guid Id, UpdateDesignationDto Dto) : ICommand<DesignationResponseDto>;
public record DeleteDesignationCommand(Guid Id) : ICommand<Unit>;

public class CreateDesignationCommandHandler : IRequestHandler<CreateDesignationCommand, DesignationResponseDto>
{
    private readonly IDesignationRepository _repo;
    private readonly DesignationMapper _mapper = new();

    public CreateDesignationCommandHandler(IDesignationRepository repo) => _repo = repo;

    public async Task<DesignationResponseDto> Handle(CreateDesignationCommand request, CancellationToken ct)
    {
        if (await _repo.CodeExistsAsync(request.Dto.Code))
            throw new AppException($"Designation code '{request.Dto.Code}' already exists.");

        if (request.Dto.ParentDesignationId.HasValue && !await _repo.ParentExistsAsync(request.Dto.ParentDesignationId))
            throw new AppException("Selected parent designation does not exist.");

        var entity = _mapper.ToEntity(request.Dto);
        entity.CreatedAt = DateTime.UtcNow;
        var created = await _repo.CreateAsync(entity);
        return _mapper.ToDto(created);
    }
}

public class UpdateDesignationCommandHandler : IRequestHandler<UpdateDesignationCommand, DesignationResponseDto>
{
    private readonly IDesignationRepository _repo;
    private readonly DesignationMapper _mapper = new();

    public UpdateDesignationCommandHandler(IDesignationRepository repo) => _repo = repo;

    public async Task<DesignationResponseDto> Handle(UpdateDesignationCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.Designation), request.Id);

        if (await _repo.CodeExistsAsync(request.Dto.Code, request.Id))
            throw new AppException($"Designation code '{request.Dto.Code}' already exists.");

        if (request.Dto.ParentDesignationId.HasValue && !await _repo.ParentExistsAsync(request.Dto.ParentDesignationId))
            throw new AppException("Selected parent designation does not exist.");

        if (await _repo.HasCircularReferenceAsync(request.Id, request.Dto.ParentDesignationId))
            throw new AppException("Circular reference detected: a designation cannot be its own ancestor.");

        _mapper.UpdateEntity(request.Dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }
}

public class DeleteDesignationCommandHandler : IRequestHandler<DeleteDesignationCommand, Unit>
{
    private readonly IDesignationRepository _repo;

    public DeleteDesignationCommandHandler(IDesignationRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteDesignationCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.Designation), request.Id);

        var deps = await _repo.GetDependenciesAsync(request.Id, ct);
        if (!deps.CanDelete)
            throw new AppException(deps.BlockingReason ?? "Cannot delete designation with existing dependencies.");

        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
