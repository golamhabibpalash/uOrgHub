using FluentValidation;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Mappings;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    private readonly IValidator<CreateDepartmentDto> _createValidator;
    private readonly IValidator<UpdateDepartmentDto> _updateValidator;
    private readonly DepartmentMapper _mapper = new();

    public DepartmentService(
        IDepartmentRepository repository,
        IValidator<CreateDepartmentDto> createValidator,
        IValidator<UpdateDepartmentDto> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResult<DepartmentResponseDto>> GetAllAsync(PaginationRequest request)
    {
        var result = await _repository.GetAllAsync(request);
        return new PagedResult<DepartmentResponseDto>
        {
            Items = result.Items.Select(_mapper.ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<DepartmentResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.Department), id);
        return _mapper.ToDto(entity);
    }

    public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new Shared.Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = _mapper.ToEntity(dto);
        entity.CreatedAt = DateTime.UtcNow;

        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<DepartmentResponseDto> UpdateAsync(Guid id, UpdateDepartmentDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new Shared.Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.Department), id);

        _mapper.UpdateEntity(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var exists = await _repository.ExistsAsync(id);
        if (!exists) throw new NotFoundException(nameof(Models.Entities.Department), id);
        await _repository.DeleteAsync(id);
    }
}
