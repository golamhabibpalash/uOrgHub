using FluentValidation;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Accounts.Models.Enums;
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

    public async Task<List<AccountGroupResponseDto>> GetAllFlatAsync()
    {
        var items = await _repository.GetAllFlatAsync();
        return items.Select(_mapper.ToDto).ToList();
    }

    public async Task<AccountGroupResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.AccountGroup), id);
        return _mapper.ToDto(entity);
    }

    public async Task<string> GenerateCodeAsync(AccountGroupType type, Guid? parentId)
    {
        return await _repository.GetNextCodeAsync(type, parentId);
    }

    public async Task<AccountGroupResponseDto> CreateAsync(CreateAccountGroupDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        if (dto.ParentAccountGroupId.HasValue)
        {
            var parentExists = await _repository.ExistsAsync(dto.ParentAccountGroupId.Value);
            if (!parentExists)
                throw new ValidationException(new List<string> { "Parent account group not found." });
        }

        var code = await _repository.GetNextCodeAsync(dto.Type, dto.ParentAccountGroupId);

        if (!string.IsNullOrWhiteSpace(dto.CustomCode))
        {
            if (await _repository.CustomCodeExistsAsync(dto.CustomCode))
                throw new ValidationException(new List<string> { "Custom code already exists." });
            if (dto.CustomCode == code)
                throw new ValidationException(new List<string> { "Custom code cannot match the system-generated code." });
        }

        var entity = _mapper.ToEntity(dto);
        entity.Code = code;
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

        if (dto.Code != entity.Code && await _repository.CodeExistsAsync(dto.Code, id))
            throw new ValidationException(new List<string> { "Code already exists" });

        if (dto.ParentAccountGroupId.HasValue)
        {
            if (dto.ParentAccountGroupId.Value == id)
                throw new ValidationException(new List<string> { "A group cannot be its own parent." });

            var parentExists = await _repository.ExistsAsync(dto.ParentAccountGroupId.Value);
            if (!parentExists)
                throw new ValidationException(new List<string> { "Parent account group not found." });

            var descendantIds = await _repository.GetDescendantIdsAsync(id);
            if (descendantIds.Contains(dto.ParentAccountGroupId.Value))
                throw new ValidationException(new List<string> { "Cannot set a descendant group as parent (circular reference)." });
        }

        if (!string.IsNullOrWhiteSpace(dto.CustomCode))
        {
            if (dto.CustomCode != entity.CustomCode && await _repository.CustomCodeExistsAsync(dto.CustomCode, id))
                throw new ValidationException(new List<string> { "Custom code already exists." });
            if (dto.CustomCode == entity.Code)
                throw new ValidationException(new List<string> { "Custom code cannot match the system-generated code." });
        }

        dto.Id = id;
        _mapper.UpdateEntity(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var exists = await _repository.ExistsAsync(id);
        if (!exists) throw new NotFoundException(nameof(Models.Entities.AccountGroup), id);

        if (await _repository.HasChildrenAsync(id))
            throw new ValidationException(new List<string> { "Cannot delete a group with child groups. Remove or reassign children first." });

        await _repository.DeleteAsync(id);
    }
}