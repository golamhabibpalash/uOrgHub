using FluentValidation;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;
using ValidationException = uOrgHub.Shared.Exceptions.ValidationException;

namespace uOrgHub.Accounts.Services;

public class ChartOfAccountService : IChartOfAccountService
{
    private readonly IChartOfAccountRepository _repository;
    private readonly IValidator<CreateChartOfAccountDto> _createValidator;
    private readonly IValidator<UpdateChartOfAccountDto> _updateValidator;
    private readonly ChartOfAccountMapper _mapper = new();

    public ChartOfAccountService(
        IChartOfAccountRepository repository,
        IValidator<CreateChartOfAccountDto> createValidator,
        IValidator<UpdateChartOfAccountDto> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResult<ChartOfAccountResponseDto>> GetAllAsync(PaginationRequest request)
    {
        var result = await _repository.GetAllAsync(request);
        return new PagedResult<ChartOfAccountResponseDto>
        {
            Items = result.Items.Select(_mapper.ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<ChartOfAccountResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.ChartOfAccount), id);
        return _mapper.ToDto(entity);
    }

    public async Task<ChartOfAccountResponseDto> CreateAsync(CreateChartOfAccountDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        if (await _repository.CodeExistsAsync(dto.AccountCode))
            throw new ValidationException(new List<string> { "Account Code already exists" });

        var entity = _mapper.ToEntity(dto);
        entity.CurrentBalance = dto.OpeningBalance;
        entity.CreatedAt = DateTime.UtcNow;

        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<ChartOfAccountResponseDto> UpdateAsync(Guid id, UpdateChartOfAccountDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.ChartOfAccount), id);

        if (await _repository.CodeExistsAsync(dto.AccountCode, id))
            throw new ValidationException(new List<string> { "Account Code already exists" });

        _mapper.UpdateEntity(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entity);
        return _mapper.ToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var exists = await _repository.ExistsAsync(id);
        if (!exists) throw new NotFoundException(nameof(Models.Entities.ChartOfAccount), id);
        await _repository.DeleteAsync(id);
    }

    public async Task<List<JournalEntryLineResponseDto>> GetLedgerAsync(Guid accountId)
    {
        var lines = await _repository.GetLedgerAsync(accountId);
        var mapper = new ChartOfAccountMapper();
        return lines.Select(mapper.ToLedgerLineDto).ToList();
    }
}