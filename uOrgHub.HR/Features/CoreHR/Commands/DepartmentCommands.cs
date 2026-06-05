using MediatR;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Mappings;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.HR.Features.CoreHR.Commands;

public record CreateDepartmentCommand(CreateDepartmentDto Dto) : ICommand<DepartmentResponseDto>;
public record UpdateDepartmentCommand(Guid Id, UpdateDepartmentDto Dto) : ICommand<DepartmentResponseDto>;
public record DeleteDepartmentCommand(Guid Id) : ICommand<Unit>;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentResponseDto>
{
    private readonly IDepartmentRepository _repo;
    private readonly DepartmentMapper _mapper = new();

    public CreateDepartmentCommandHandler(IDepartmentRepository repo) => _repo = repo;

    public async Task<DepartmentResponseDto> Handle(CreateDepartmentCommand request, CancellationToken ct)
    {
        if (await _repo.CodeExistsAsync(request.Dto.Code))
            throw new AppException($"Department code '{request.Dto.Code}' already exists.");

        if (request.Dto.ParentDepartmentId.HasValue && !await _repo.ParentExistsAsync(request.Dto.ParentDepartmentId))
            throw new AppException("Selected parent department does not exist.");

        var entity = _mapper.ToEntity(request.Dto);
        entity.CreatedAt = DateTime.UtcNow;
        var created = await _repo.CreateAsync(entity);
        return _mapper.ToDto(created);
    }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentResponseDto>
{
    private readonly IDepartmentRepository _repo;
    private readonly DepartmentMapper _mapper = new();

    public UpdateDepartmentCommandHandler(IDepartmentRepository repo) => _repo = repo;

    public async Task<DepartmentResponseDto> Handle(UpdateDepartmentCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.Department), request.Id);

        if (await _repo.CodeExistsAsync(request.Dto.Code, request.Id))
            throw new AppException($"Department code '{request.Dto.Code}' already exists.");

        if (request.Dto.ParentDepartmentId.HasValue && !await _repo.ParentExistsAsync(request.Dto.ParentDepartmentId))
            throw new AppException("Selected parent department does not exist.");

        if (await _repo.HasCircularReferenceAsync(request.Id, request.Dto.ParentDepartmentId))
            throw new AppException("Circular reference detected: a department cannot be its own ancestor.");

        _mapper.UpdateEntity(request.Dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }
}

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Unit>
{
    private readonly IDepartmentRepository _repo;

    public DeleteDepartmentCommandHandler(IDepartmentRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteDepartmentCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.Department), request.Id);

        var deps = await _repo.GetDependenciesAsync(request.Id, ct);
        if (!deps.CanDelete)
            throw new AppException(deps.BlockingReason ?? "Department has linked records and cannot be deleted.", 409);

        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
