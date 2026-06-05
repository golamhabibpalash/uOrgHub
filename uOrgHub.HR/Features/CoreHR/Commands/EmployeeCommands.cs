using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Features.CoreHR.Queries;
using uOrgHub.HR.Mappings;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Data;
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
        if (string.IsNullOrWhiteSpace(request.Dto.EmployeeCode))
            request.Dto.EmployeeCode = await _repo.GetNextEmployeeCodeAsync();
        else if (await _repo.CodeExistsAsync(request.Dto.EmployeeCode))
            throw new AppException($"Employee code '{request.Dto.EmployeeCode}' already exists.");

        if (await _repo.EmailExistsAsync(request.Dto.Email))
            throw new AppException($"Email '{request.Dto.Email}' already in use.");

        var entity = _mapper.ToEntity(request.Dto);
        entity.CreatedAt = DateTime.UtcNow;
        var created = await _repo.CreateAsync(entity);
        var dto = _mapper.ToDto(created);
        dto.DepartmentName = created.Department?.Name ?? "";
        dto.DesignationName = created.Designation?.Name ?? "";
        dto.ManagerName = created.Manager != null ? $"{created.Manager.FirstName} {created.Manager.LastName}" : null;
        dto.ProfilePictureUrl = EmployeePictureUrl.ToPublicUrl(created.ProfilePicturePath);
        dto.NidPhotoUrl = EmployeePictureUrl.ToPublicUrl(created.NidPhotoPath);
        return dto;
    }
}

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeResponseDto>
{
    private readonly IEmployeeRepository _repo;
    private readonly AppDbContext _context;
    private readonly EmployeeMapper _mapper = new();

    public UpdateEmployeeCommandHandler(IEmployeeRepository repo, AppDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<EmployeeResponseDto> Handle(UpdateEmployeeCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Models.Entities.Employee), request.Id);

        if (await _repo.EmailExistsAsync(request.Dto.Email, request.Id))
            throw new AppException($"Email '{request.Dto.Email}' already in use.");

        if (request.Dto.ManagerId.HasValue)
            await EnsureNoCircularReference(request.Id, request.Dto.ManagerId.Value, ct);

        _mapper.UpdateEntity(request.Dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(entity);
        var dto = _mapper.ToDto(updated);
        dto.DepartmentName = updated.Department?.Name ?? "";
        dto.DesignationName = updated.Designation?.Name ?? "";
        dto.ManagerName = updated.Manager != null ? $"{updated.Manager.FirstName} {updated.Manager.LastName}" : null;
        dto.ProfilePictureUrl = EmployeePictureUrl.ToPublicUrl(updated.ProfilePicturePath);
        dto.NidPhotoUrl = EmployeePictureUrl.ToPublicUrl(updated.NidPhotoPath);
        return dto;
    }

    private async Task EnsureNoCircularReference(Guid employeeId, Guid proposedManagerId, CancellationToken ct)
    {
        if (proposedManagerId == employeeId)
            throw new AppException("An employee cannot be their own manager.");

        var visited = new HashSet<Guid> { employeeId };
        var currentId = proposedManagerId;

        while (currentId != Guid.Empty)
        {
            if (!visited.Add(currentId))
                throw new AppException("Circular reporting relationship detected.");

            var managerId = await _context.Set<Models.Entities.Employee>()
                .Where(e => e.Id == currentId && !e.IsDeleted)
                .Select(e => e.ManagerId)
                .FirstOrDefaultAsync(ct);

            currentId = managerId ?? Guid.Empty;
        }
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

        var deps = await _repo.GetDependenciesAsync(request.Id, ct);
        if (!deps.CanDelete)
            throw new AppException(deps.BlockingReason ?? "Employee has linked records and cannot be deleted.", 409);

        await _repo.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
