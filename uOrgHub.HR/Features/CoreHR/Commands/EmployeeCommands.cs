using MediatR;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Mappings;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.HR.Features.CoreHR.Commands;

public record CreateEmployeeCommand(CreateEmployeeDto Dto) : ICommand<EmployeeResponseDto>;
public record UpdateEmployeeCommand(Guid Id, UpdateEmployeeDto Dto) : ICommand<EmployeeResponseDto>;
public record DeleteEmployeeCommand(Guid Id) : ICommand<Unit>;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeResponseDto>
{
    private readonly IEmployeeRepository _repo;
    private readonly EmployeeMapper _mapper = new();

    public CreateEmployeeCommandHandler(IEmployeeRepository repo) => _repo = repo;

    public async Task<EmployeeResponseDto> Handle(CreateEmployeeCommand request, CancellationToken ct)
    {
        if (await _repo.CodeExistsAsync(request.Dto.EmployeeCode))
            throw new AppException($"Employee code '{request.Dto.EmployeeCode}' already exists.");

        if (await _repo.EmailExistsAsync(request.Dto.Email))
            throw new AppException($"Email '{request.Dto.Email}' already in use.");

        var entity = _mapper.ToEntity(request.Dto);
        entity.CreatedAt = DateTime.UtcNow;
        var created = await _repo.CreateAsync(entity);
        return _mapper.ToDto(created);
    }
}

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeResponseDto>
{
    private readonly IEmployeeRepository _repo;
    private readonly EmployeeMapper _mapper = new();

    public UpdateEmployeeCommandHandler(IEmployeeRepository repo) => _repo = repo;

    public async Task<EmployeeResponseDto> Handle(UpdateEmployeeCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.Employee), request.Id);

        if (await _repo.EmailExistsAsync(request.Dto.Email, request.Id))
            throw new AppException($"Email '{request.Dto.Email}' already in use.");

        _mapper.UpdateEntity(request.Dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }
}

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Unit>
{
    private readonly IEmployeeRepository _repo;

    public DeleteEmployeeCommandHandler(IEmployeeRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteEmployeeCommand request, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(request.Id))
            throw new NotFoundException(nameof(Models.Entities.Employee), request.Id);
        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
