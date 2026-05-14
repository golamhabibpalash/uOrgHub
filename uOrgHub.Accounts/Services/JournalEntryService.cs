using FluentValidation;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;
using ValidationException = uOrgHub.Shared.Exceptions.ValidationException;

namespace uOrgHub.Accounts.Services;

public class JournalEntryService : IJournalEntryService
{
    private readonly AppDbContext _context;
    private readonly IJournalEntryRepository _repository;
    private readonly IValidator<CreateJournalEntryDto> _createValidator;
    private readonly IValidator<UpdateJournalEntryDto> _updateValidator;
    private readonly JournalEntryMapper _mapper = new();

    public JournalEntryService(
        AppDbContext context,
        IJournalEntryRepository repository,
        IValidator<CreateJournalEntryDto> createValidator,
        IValidator<UpdateJournalEntryDto> updateValidator)
    {
        _context = context;
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResult<JournalEntryResponseDto>> GetAllAsync(PaginationRequest request)
    {
        var result = await _repository.GetAllAsync(request);
        return new PagedResult<JournalEntryResponseDto>
        {
            Items = result.Items.Select(_mapper.ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<JournalEntryResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.JournalEntry), id);
        return _mapper.ToDto(entity);
    }

    public async Task<JournalEntryResponseDto> CreateAsync(CreateJournalEntryDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var totalDebit = dto.Lines.Sum(x => x.DebitAmount);
        var totalCredit = dto.Lines.Sum(x => x.CreditAmount);
        
        if (totalDebit != totalCredit)
            throw new ValidationException(new List<string> { "Total Debit must equal Total Credit" });

        var entryNumber = await _repository.GenerateEntryNumberAsync();

        var entity = _mapper.ToEntity(dto);
        entity.EntryNumber = entryNumber;
        entity.TotalDebit = totalDebit;
        entity.TotalCredit = totalCredit;
        entity.Status = JournalEntryStatus.Draft;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = "System";

        var created = await _repository.CreateAsync(entity);
        return _mapper.ToDto(created);
    }

    public async Task<JournalEntryResponseDto> UpdateAsync(Guid id, UpdateJournalEntryDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.JournalEntry), id);

        if (entity.Status != JournalEntryStatus.Draft)
            throw new ValidationException(new List<string> { "Only draft entries can be edited" });

        var totalDebit = dto.Lines.Sum(x => x.DebitAmount);
        var totalCredit = dto.Lines.Sum(x => x.CreditAmount);

        if (totalDebit != totalCredit)
            throw new ValidationException(new List<string> { "Total Debit must equal Total Credit" });

        _context.Set<JournalEntryLine>().RemoveRange(entity.Lines);
        
        _mapper.UpdateEntity(dto, entity);
        entity.TotalDebit = totalDebit;
        entity.TotalCredit = totalCredit;
        entity.UpdatedAt = DateTime.UtcNow;

        foreach (var line in dto.Lines)
        {
            entity.Lines.Add(new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                JournalEntryId = entity.Id,
                AccountId = line.AccountId,
                Description = line.Description,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                LineOrder = line.LineOrder,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return _mapper.ToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.JournalEntry), id);

        if (entity.Status != JournalEntryStatus.Draft)
            throw new ValidationException(new List<string> { "Only draft entries can be deleted" });

        await _repository.DeleteAsync(id);
    }

    public async Task<JournalEntryResponseDto> PostAsync(Guid id, string postedBy)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.JournalEntry), id);

        if (entity.Status != JournalEntryStatus.Draft)
            throw new ValidationException(new List<string> { "Only draft entries can be posted" });

        if (entity.TotalDebit != entity.TotalCredit)
            throw new ValidationException(new List<string> { "Total Debit must equal Total Credit before posting" });

        foreach (var line in entity.Lines)
        {
            var account = await _context.Set<ChartOfAccount>().FindAsync(line.AccountId);
            if (account == null) continue;

            if (line.DebitAmount > 0)
            {
                if (account.AccountType == AccountGroupType.Asset || account.AccountType == AccountGroupType.Expense)
                    account.CurrentBalance += line.DebitAmount;
                else
                    account.CurrentBalance -= line.DebitAmount;
            }

            if (line.CreditAmount > 0)
            {
                if (account.AccountType == AccountGroupType.Asset || account.AccountType == AccountGroupType.Expense)
                    account.CurrentBalance -= line.CreditAmount;
                else
                    account.CurrentBalance += line.CreditAmount;
            }
        }

        entity.Status = JournalEntryStatus.Posted;
        entity.PostedBy = postedBy;
        entity.PostedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.ToDto(entity);
    }

    public async Task<JournalEntryResponseDto> CancelAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Models.Entities.JournalEntry), id);

        if (entity.Status != JournalEntryStatus.Posted)
            throw new ValidationException(new List<string> { "Only posted entries can be cancelled" });

        foreach (var line in entity.Lines)
        {
            var account = await _context.Set<ChartOfAccount>().FindAsync(line.AccountId);
            if (account == null) continue;

            if (line.DebitAmount > 0)
            {
                if (account.AccountType == AccountGroupType.Asset || account.AccountType == AccountGroupType.Expense)
                    account.CurrentBalance -= line.DebitAmount;
                else
                    account.CurrentBalance += line.DebitAmount;
            }

            if (line.CreditAmount > 0)
            {
                if (account.AccountType == AccountGroupType.Asset || account.AccountType == AccountGroupType.Expense)
                    account.CurrentBalance += line.CreditAmount;
                else
                    account.CurrentBalance -= line.CreditAmount;
            }
        }

        entity.Status = JournalEntryStatus.Cancelled;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.ToDto(entity);
    }
}