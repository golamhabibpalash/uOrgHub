using FluentValidation;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Mappings;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Services;

public class DesignationService : IDesignationService
{
    private readonly IDesignationRepository _repository;
    private readonly IValidator<CreateDesignationDto> _createValidator;
    private readonly IValidator<UpdateDesignationDto> _updateValidator;
    private readonly DesignationMapper _mapper = new();

    public DesignationService(
        IDesignationRepository repository,
        IValidator<CreateDesignationDto> createValidator,
        IValidator<UpdateDesignationDto> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResult<DesignationResponseDto>> GetAllAsync(PaginationRequest request)
    {
        var result = await _repository.GetAllAsync(request);
        return new PagedResult<DesignationResponseDto>
        {
            Items = result.Items.Select(_mapper.ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<DesignationResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.Designation), id);
        return _mapper.ToDto(entity);
    }

    public async Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new Shared.Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = _mapper.ToEntity(dto);
        entity.CreatedAt = DateTime.UtcNow;

        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<DesignationResponseDto> UpdateAsync(Guid id, UpdateDesignationDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new Shared.Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.Designation), id);

        _mapper.UpdateEntity(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var exists = await _repository.ExistsAsync(id);
        if (!exists) throw new NotFoundException(nameof(Models.Entities.Designation), id);
        await _repository.DeleteAsync(id);
    }
}
