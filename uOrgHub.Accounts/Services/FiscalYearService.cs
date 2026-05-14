using FluentValidation;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;
using ValidationException = uOrgHub.Shared.Exceptions.ValidationException;

namespace uOrgHub.Accounts.Services;

public class FiscalYearService : IFiscalYearService
{
    private readonly IFiscalYearRepository _repository;
    private readonly IValidator<CreateFiscalYearDto> _createValidator;
    private readonly IValidator<UpdateFiscalYearDto> _updateValidator;
    private readonly FiscalYearMapper _mapper = new();

    public FiscalYearService(
        IFiscalYearRepository repository,
        IValidator<CreateFiscalYearDto> createValidator,
        IValidator<UpdateFiscalYearDto> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResult<FiscalYearResponseDto>> GetAllAsync(PaginationRequest request)
    {
        var result = await _repository.GetAllAsync(request);
        return new PagedResult<FiscalYearResponseDto>
        {
            Items = result.Items.Select(_mapper.ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<FiscalYearResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.FiscalYear), id);
        return _mapper.ToDto(entity);
    }

    public async Task<FiscalYearResponseDto> CreateAsync(CreateFiscalYearDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        if (dto.IsCurrent)
        {
            var current = await _repository.GetCurrentAsync();
            if (current != null)
            {
                current.IsCurrent = false;
                await _repository.UpdateAsync(current);
            }
        }

        var entity = _mapper.ToEntity(dto);
        entity.CreatedAt = DateTime.UtcNow;

        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<FiscalYearResponseDto> UpdateAsync(Guid id, UpdateFiscalYearDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.FiscalYear), id);

        if (dto.IsCurrent && !entity.IsCurrent)
        {
            var current = await _repository.GetCurrentAsync();
            if (current != null && current.Id != id)
            {
                current.IsCurrent = false;
                await _repository.UpdateAsync(current);
            }
        }

        _mapper.UpdateEntity(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.FiscalYear), id);

        if (entity.IsCurrent)
            throw new ValidationException(new List<string> { "Cannot delete the current fiscal year" });

        await _repository.DeleteAsync(id);
    }

    public async Task<FiscalYearResponseDto> GetCurrentAsync()
    {
        var entity = await _repository.GetCurrentAsync()
            ?? throw new NotFoundException("FiscalYear", Guid.Empty);
        return _mapper.ToDto(entity);
    }
}