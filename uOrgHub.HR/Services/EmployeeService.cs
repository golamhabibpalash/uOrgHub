using FluentValidation;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Mappings;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IValidator<CreateEmployeeDto> _createValidator;
    private readonly IValidator<UpdateEmployeeDto> _updateValidator;
    private readonly EmployeeMapper _mapper = new();

    public EmployeeService(
        IEmployeeRepository repository,
        IValidator<CreateEmployeeDto> createValidator,
        IValidator<UpdateEmployeeDto> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResult<EmployeeResponseDto>> GetAllAsync(PaginationRequest request)
    {
        var result = await _repository.GetAllAsync(request);
        return new PagedResult<EmployeeResponseDto>
        {
            Items = result.Items.Select(_mapper.ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<EmployeeResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.Employee), id);
        return _mapper.ToDto(entity);
    }

    public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new Shared.Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = _mapper.ToEntity(dto);
        entity.CreatedAt = DateTime.UtcNow;

        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<EmployeeResponseDto> UpdateAsync(Guid id, UpdateEmployeeDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new Shared.Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.Employee), id);

        _mapper.UpdateEntity(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var exists = await _repository.ExistsAsync(id);
        if (!exists) throw new NotFoundException(nameof(Models.Entities.Employee), id);
        await _repository.DeleteAsync(id);
    }
}
