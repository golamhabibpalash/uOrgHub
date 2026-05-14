using FluentValidation;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;
using ValidationException = uOrgHub.Shared.Exceptions.ValidationException;

namespace uOrgHub.Accounts.Services;

public class AccountGroupService : IAccountGroupService
{
    private readonly IAccountGroupRepository _repository;
    private readonly IValidator<CreateAccountGroupDto> _createValidator;
    private readonly IValidator<UpdateAccountGroupDto> _updateValidator;
    private readonly AccountGroupMapper _mapper = new();

    public AccountGroupService(
        IAccountGroupRepository repository,
        IValidator<CreateAccountGroupDto> createValidator,
        IValidator<UpdateAccountGroupDto> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResult<AccountGroupResponseDto>> GetAllAsync(PaginationRequest request)
    {
        var result = await _repository.GetAllAsync(request);
        return new PagedResult<AccountGroupResponseDto>
        {
            Items = result.Items.Select(_mapper.ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<AccountGroupResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.AccountGroup), id);
        return _mapper.ToDto(entity);
    }

    public async Task<AccountGroupResponseDto> CreateAsync(CreateAccountGroupDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        if (await _repository.CodeExistsAsync(dto.Code))
            throw new ValidationException(new List<string> { "Code already exists" });

        var entity = _mapper.ToEntity(dto);
        entity.CreatedAt = DateTime.UtcNow;

        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<AccountGroupResponseDto> UpdateAsync(Guid id, UpdateAccountGroupDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.AccountGroup), id);

        if (await _repository.CodeExistsAsync(dto.Code, id))
            throw new ValidationException(new List<string> { "Code already exists" });

        _mapper.UpdateEntity(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var exists = await _repository.ExistsAsync(id);
        if (!exists) throw new NotFoundException(nameof(Models.Entities.AccountGroup), id);
        await _repository.DeleteAsync(id);
    }
}