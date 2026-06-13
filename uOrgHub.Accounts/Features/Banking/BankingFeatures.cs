using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.Banking;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.Banking;

public record GetBankAccountsQuery(PaginationRequest Request) : IQuery<PagedResult<BankAccountResponseDto>>;
public record GetBankAccountByIdQuery(Guid Id) : IQuery<BankAccountResponseDto>;
public record CreateBankAccountCommand(CreateBankAccountDto Dto) : ICommand<BankAccountResponseDto>;
public record UpdateBankAccountCommand(Guid Id, UpdateBankAccountDto Dto) : ICommand<BankAccountResponseDto>;
public record GetBankTransactionsQuery(Guid BankAccountId, PaginationRequest Request) : IQuery<PagedResult<BankTransactionResponseDto>>;
public record CreateBankTransactionCommand(CreateBankTransactionDto Dto) : ICommand<BankTransactionResponseDto>;
public record GetAllBankAccountsForExportQuery : IQuery<List<BankAccountResponseDto>>;

public class GetBankAccountsQueryHandler : IRequestHandler<GetBankAccountsQuery, PagedResult<BankAccountResponseDto>>
{
    private readonly AppDbContext _context;
    public GetBankAccountsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<BankAccountResponseDto>> Handle(GetBankAccountsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.BankAccount>()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.AccountName, x => x.AccountNumber, x => x.BankName);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.AccountName)
            : query.OrderBy(x => x.AccountName);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<BankAccountResponseDto>
        {
            Items = items.Select(BankingMappingHelper.ToBankAccountDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetBankAccountByIdQueryHandler : IRequestHandler<GetBankAccountByIdQuery, BankAccountResponseDto>
{
    private readonly AppDbContext _context;
    public GetBankAccountByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<BankAccountResponseDto> Handle(GetBankAccountByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.BankAccount>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.BankAccount), request.Id);

        return BankingMappingHelper.ToBankAccountDto(e);
    }
}

public class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, BankAccountResponseDto>
{
    private readonly AppDbContext _context;
    public CreateBankAccountCommandHandler(AppDbContext context) => _context = context;

    public async Task<BankAccountResponseDto> Handle(CreateBankAccountCommand request, CancellationToken ct)
    {
        if (await _context.Set<Models.Entities.BankAccount>().AnyAsync(x => x.AccountNumber == request.Dto.AccountNumber && !x.IsDeleted, ct))
            throw new AppException($"Bank account number '{request.Dto.AccountNumber}' already exists.");

        var entity = new Models.Entities.BankAccount
        {
            AccountNumber = request.Dto.AccountNumber,
            AccountName = request.Dto.AccountName,
            BankName = request.Dto.BankName,
            BranchName = request.Dto.BranchName,
            RoutingNumber = request.Dto.RoutingNumber,
            Currency = request.Dto.Currency,
            OpeningBalance = request.Dto.OpeningBalance,
            CurrentBalance = request.Dto.OpeningBalance,
            ChartOfAccountId = request.Dto.ChartOfAccountId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Models.Entities.BankAccount>().Add(entity);

        if (request.Dto.OpeningBalance > 0 && request.Dto.OpeningBalanceEquityAccountId.HasValue)
        {
            var coa = await _context.Set<Models.Entities.ChartOfAccount>()
                .FirstAsync(a => a.Id == request.Dto.ChartOfAccountId, ct);
            var equityAccount = await _context.Set<Models.Entities.ChartOfAccount>()
                .FirstAsync(a => a.Id == request.Dto.OpeningBalanceEquityAccountId.Value, ct);

            var year = DateTime.UtcNow.Year;
            var prefix = $"JV-{year}-";
            var lastEntry = await _context.Set<Models.Entities.JournalEntry>()
                .Where(x => x.EntryNumber.StartsWith(prefix))
                .OrderByDescending(x => x.EntryNumber)
                .FirstOrDefaultAsync(ct);
            var sequence = 1;
            if (lastEntry != null && int.TryParse(lastEntry.EntryNumber.Split('-').Last(), out var lastNum))
                sequence = lastNum + 1;

            var journalEntry = new Models.Entities.JournalEntry
            {
                EntryNumber = $"{prefix}{sequence:D4}",
                EntryDate = DateTime.UtcNow,
                Description = $"Opening balance for bank account {entity.AccountNumber} - {entity.AccountName}",
                Status = JournalEntryStatus.Posted,
                TotalDebit = request.Dto.OpeningBalance,
                TotalCredit = request.Dto.OpeningBalance,
                PostedBy = entity.CreatedBy,
                PostedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = entity.CreatedBy,
            };

            journalEntry.Lines.Add(new Models.Entities.JournalEntryLine
            {
                JournalEntryId = journalEntry.Id,
                AccountId = request.Dto.ChartOfAccountId,
                Description = $"Opening balance - {entity.AccountName}",
                DebitAmount = request.Dto.OpeningBalance,
                CreditAmount = 0,
                LineOrder = 1,
                CreatedAt = DateTime.UtcNow
            });

            journalEntry.Lines.Add(new Models.Entities.JournalEntryLine
            {
                JournalEntryId = journalEntry.Id,
                AccountId = request.Dto.OpeningBalanceEquityAccountId.Value,
                Description = $"Opening balance offset - {entity.AccountName}",
                DebitAmount = 0,
                CreditAmount = request.Dto.OpeningBalance,
                LineOrder = 2,
                CreatedAt = DateTime.UtcNow
            });

            _context.Set<Models.Entities.JournalEntry>().Add(journalEntry);

            coa.CurrentBalance += request.Dto.OpeningBalance;
            equityAccount.CurrentBalance += request.Dto.OpeningBalance;
        }

        await _context.SaveChangesAsync(ct);
        return BankingMappingHelper.ToBankAccountDto(entity);
    }
}

public class UpdateBankAccountCommandHandler : IRequestHandler<UpdateBankAccountCommand, BankAccountResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateBankAccountCommandHandler(AppDbContext context) => _context = context;

    public async Task<BankAccountResponseDto> Handle(UpdateBankAccountCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.BankAccount>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.BankAccount), request.Id);

        entity.AccountName = request.Dto.AccountName;
        entity.BankName = request.Dto.BankName;
        entity.BranchName = request.Dto.BranchName;
        entity.RoutingNumber = request.Dto.RoutingNumber;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return BankingMappingHelper.ToBankAccountDto(entity);
    }
}

public class GetBankTransactionsQueryHandler : IRequestHandler<GetBankTransactionsQuery, PagedResult<BankTransactionResponseDto>>
{
    private readonly AppDbContext _context;
    public GetBankTransactionsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<BankTransactionResponseDto>> Handle(GetBankTransactionsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.BankTransaction>()
            .Include(x => x.BankAccount)
            .Where(x => !x.IsDeleted && x.BankAccountId == request.BankAccountId)
            .OrderByDescending(x => x.TransactionDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<BankTransactionResponseDto>
        {
            Items = items.Select(BankingMappingHelper.ToBankTransactionDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class CreateBankTransactionCommandHandler : IRequestHandler<CreateBankTransactionCommand, BankTransactionResponseDto>
{
    private readonly AppDbContext _context;
    public CreateBankTransactionCommandHandler(AppDbContext context) => _context = context;

    public async Task<BankTransactionResponseDto> Handle(CreateBankTransactionCommand request, CancellationToken ct)
    {
        var bankAccount = await _context.Set<Models.Entities.BankAccount>()
            .Where(x => !x.IsDeleted && x.Id == request.Dto.BankAccountId)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.BankAccount), request.Dto.BankAccountId);

        var entity = new Models.Entities.BankTransaction
        {
            BankAccountId = request.Dto.BankAccountId,
            TransactionType = request.Dto.TransactionType,
            TransactionDate = request.Dto.TransactionDate,
            Amount = request.Dto.Amount,
            Description = request.Dto.Description,
            ReferenceNumber = request.Dto.ReferenceNumber,
            ChequeNumber = request.Dto.ChequeNumber,
            Payee = request.Dto.Payee,
            JournalEntryId = request.Dto.JournalEntryId,
            CreatedAt = DateTime.UtcNow
        };

        bankAccount.CurrentBalance += request.Dto.Amount;
        bankAccount.UpdatedAt = DateTime.UtcNow;

        _context.Set<Models.Entities.BankTransaction>().Add(entity);
        await _context.SaveChangesAsync(ct);

        entity.BankAccount = bankAccount;
        return BankingMappingHelper.ToBankTransactionDto(entity);
    }
}

public class GetAllBankAccountsForExportQueryHandler : IRequestHandler<GetAllBankAccountsForExportQuery, List<BankAccountResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllBankAccountsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<BankAccountResponseDto>> Handle(GetAllBankAccountsForExportQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Models.Entities.BankAccount>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.AccountName)
            .ToListAsync(ct);
        return items.Select(BankingMappingHelper.ToBankAccountDto).ToList();
    }
}

file static class BankingMappingHelper
{
    public static BankAccountResponseDto ToBankAccountDto(Models.Entities.BankAccount e) => new()
    {
        Id = e.Id,
        AccountNumber = e.AccountNumber,
        AccountName = e.AccountName,
        BankName = e.BankName,
        BranchName = e.BranchName,
        RoutingNumber = e.RoutingNumber,
        Currency = e.Currency,
        OpeningBalance = e.OpeningBalance,
        CurrentBalance = e.CurrentBalance,
        IsActive = e.IsActive,
        ChartOfAccountId = e.ChartOfAccountId
    };

    public static BankTransactionResponseDto ToBankTransactionDto(Models.Entities.BankTransaction e) => new()
    {
        Id = e.Id,
        BankAccountId = e.BankAccountId,
        BankAccountName = e.BankAccount?.AccountName ?? string.Empty,
        TransactionType = e.TransactionType,
        TransactionDate = e.TransactionDate,
        Amount = e.Amount,
        Description = e.Description,
        ReferenceNumber = e.ReferenceNumber,
        ChequeNumber = e.ChequeNumber,
        Payee = e.Payee,
        IsReconciled = e.IsReconciled,
        JournalEntryId = e.JournalEntryId
    };
}
