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
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
