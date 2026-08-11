using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.DTOs.Voucher;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Services;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;
using ValidationException = uOrgHub.Shared.Exceptions.ValidationException;

namespace uOrgHub.Accounts.Features.Voucher;

public record GetVouchersQuery(
    PaginationRequest Request,
    VoucherType? Type = null,
    VoucherStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? AccountId = null,
    Guid? ProjectId = null,
    Guid? CostCenterId = null) : IQuery<PagedResult<VoucherResponseDto>>;

public record GetVoucherByIdQuery(Guid Id) : IQuery<VoucherResponseDto>;
public record GetVoucherJournalEntryQuery(Guid Id) : IQuery<JournalEntryResponseDto>;
public record CreateVoucherCommand(CreateVoucherDto Dto, string CreatedBy) : ICommand<VoucherResponseDto>;
public record UpdateVoucherCommand(Guid Id, UpdateVoucherDto Dto, string UpdatedBy) : ICommand<VoucherResponseDto>;
public record SubmitVoucherCommand(Guid Id, string SubmittedBy) : ICommand<VoucherResponseDto>;
public record ApproveVoucherCommand(Guid Id, string ApprovedBy) : ICommand<VoucherResponseDto>;
public record PostVoucherCommand(Guid Id, string PostedBy) : ICommand<VoucherResponseDto>;
public record RejectVoucherCommand(Guid Id, string Reason, string RejectedBy) : ICommand<VoucherResponseDto>;
public record CancelVoucherCommand(Guid Id) : ICommand<VoucherResponseDto>;
public record GetAllVouchersForExportQuery : IQuery<List<VoucherResponseDto>>;
public record GetVoucherAccountOptionsQuery(VoucherType VoucherType) : IQuery<VoucherAccountOptionsDto>;

public class GetVouchersQueryHandler : IRequestHandler<GetVouchersQuery, PagedResult<VoucherResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly VoucherMapper _mapper = new();

    public GetVouchersQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<VoucherResponseDto>> Handle(GetVouchersQuery request, CancellationToken ct)
    {
        var query = VoucherQueryHelper.BaseQuery(_context);

        if (request.Type.HasValue)
            query = query.Where(x => x.VoucherType == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.FromDate.HasValue)
            query = query.Where(x => x.VoucherDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(x => x.VoucherDate <= request.ToDate.Value);

        if (request.AccountId.HasValue)
            query = query.Where(x => x.DebitAccountId == request.AccountId.Value || x.CreditAccountId == request.AccountId.Value);

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.CostCenterId.HasValue)
            query = query.Where(x => x.CostCenterId == request.CostCenterId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.VoucherNumber, x => x.Description, x => x.Name);

        query = query.ApplySorting(request.Request.SortBy ?? "VoucherDate", request.Request.SortDescending);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<VoucherResponseDto>
        {
            Items = items.Select(_mapper.ToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetVoucherByIdQueryHandler : IRequestHandler<GetVoucherByIdQuery, VoucherResponseDto>
{
    private readonly AppDbContext _context;
    private readonly VoucherMapper _mapper = new();

    public GetVoucherByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<VoucherResponseDto> Handle(GetVoucherByIdQuery request, CancellationToken ct)
    {
        var entity = await VoucherQueryHelper.BaseQuery(_context)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Voucher), request.Id);

        return _mapper.ToDto(entity);
    }
}

public class GetVoucherJournalEntryQueryHandler : IRequestHandler<GetVoucherJournalEntryQuery, JournalEntryResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IJournalEntryService _jeService;

    public GetVoucherJournalEntryQueryHandler(AppDbContext context, IJournalEntryService jeService)
    {
        _context = context;
        _jeService = jeService;
    }

    public async Task<JournalEntryResponseDto> Handle(GetVoucherJournalEntryQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Voucher>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Voucher), request.Id);

        if (!entity.JournalEntryId.HasValue)
            throw new AppException("This voucher has no linked journal entry yet.");

        return await _jeService.GetByIdAsync(entity.JournalEntryId.Value);
    }
}

public class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, VoucherResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IDocumentNumberingService _numbering;
    private readonly IValidator<CreateVoucherDto> _validator;
    private readonly VoucherMapper _mapper = new();

    public CreateVoucherCommandHandler(AppDbContext context, IDocumentNumberingService numbering, IValidator<CreateVoucherDto> validator)
    {
        _context = context;
        _numbering = numbering;
        _validator = validator;
    }

    public async Task<VoucherResponseDto> Handle(CreateVoucherCommand request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request.Dto, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        await VoucherGuard.ValidateAccountsAsync(_context, request.Dto.VoucherType, request.Dto.DebitAccountId, request.Dto.CreditAccountId, ct);
        var chargeTarget = await VoucherGuard.ResolveChargeTargetAsync(_context, request.Dto.ProjectId, request.Dto.CostCenterId, ct);
        if (request.Dto.FiscalYearId.HasValue)
            await VoucherGuard.ValidateFiscalYearAsync(_context, request.Dto.FiscalYearId.Value, request.Dto.VoucherDate, ct);

        var prefix = request.Dto.VoucherType == VoucherType.Debit ? "DR" : "CR";
        var voucherNumber = await _numbering.GenerateNextAsync("Voucher", prefix);

        if (await _context.Set<Models.Entities.Voucher>().AnyAsync(x => x.VoucherNumber == voucherNumber && !x.IsDeleted, ct))
            throw new AppException($"Voucher number '{voucherNumber}' already exists.");

        var entity = _mapper.ToEntity(request.Dto);
        entity.VoucherNumber = voucherNumber;
        entity.Status = VoucherStatus.Draft;
        entity.CostCenterId = chargeTarget.CostCenterId;
        entity.ProjectId = chargeTarget.ProjectId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = request.CreatedBy;

        _context.Set<Models.Entities.Voucher>().Add(entity);
        await _context.SaveChangesAsync(ct);

        await VoucherGuard.ReloadReferencedAsync(_context, entity, ct);
        return _mapper.ToDto(entity);
    }
}

public class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, VoucherResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IValidator<UpdateVoucherDto> _validator;
    private readonly VoucherMapper _mapper = new();

    public UpdateVoucherCommandHandler(AppDbContext context, IValidator<UpdateVoucherDto> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<VoucherResponseDto> Handle(UpdateVoucherCommand request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request.Dto, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var entity = await VoucherQueryHelper.BaseQuery(_context)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Voucher), request.Id);

        if (entity.Status != VoucherStatus.Draft)
            throw new AppException("Only draft vouchers can be edited.");

        await VoucherGuard.ValidateAccountsAsync(_context, entity.VoucherType, request.Dto.DebitAccountId, request.Dto.CreditAccountId, ct);
        var chargeTarget = await VoucherGuard.ResolveChargeTargetAsync(_context, request.Dto.ProjectId, request.Dto.CostCenterId, ct);
        if (request.Dto.FiscalYearId.HasValue)
            await VoucherGuard.ValidateFiscalYearAsync(_context, request.Dto.FiscalYearId.Value, request.Dto.VoucherDate, ct);

        _mapper.UpdateEntity(request.Dto, entity);
        entity.CostCenterId = chargeTarget.CostCenterId;
        entity.ProjectId = chargeTarget.ProjectId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = request.UpdatedBy;

        await _context.SaveChangesAsync(ct);

        await VoucherGuard.ReloadReferencedAsync(_context, entity, ct);
        return _mapper.ToDto(entity);
    }
}

public class SubmitVoucherCommandHandler : IRequestHandler<SubmitVoucherCommand, VoucherResponseDto>
{
    private readonly AppDbContext _context;
    private readonly VoucherMapper _mapper = new();

    public SubmitVoucherCommandHandler(AppDbContext context) => _context = context;

    public async Task<VoucherResponseDto> Handle(SubmitVoucherCommand request, CancellationToken ct)
    {
        var entity = await VoucherQueryHelper.BaseQuery(_context)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Voucher), request.Id);

        if (entity.Status != VoucherStatus.Draft)
            throw new AppException("Only draft vouchers can be submitted.");

        entity.Status = VoucherStatus.Submitted;
        entity.SubmittedBy = request.SubmittedBy;
        entity.SubmittedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await VoucherGuard.ReloadReferencedAsync(_context, entity, ct);
        return _mapper.ToDto(entity);
    }
}

public class ApproveVoucherCommandHandler : IRequestHandler<ApproveVoucherCommand, VoucherResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IJournalEntryService _jeService;
    private readonly VoucherMapper _mapper = new();

    public ApproveVoucherCommandHandler(AppDbContext context, IJournalEntryService jeService)
    {
        _context = context;
        _jeService = jeService;
    }

    public async Task<VoucherResponseDto> Handle(ApproveVoucherCommand request, CancellationToken ct)
    {
        var entity = await VoucherQueryHelper.BaseQuery(_context)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Voucher), request.Id);

        if (entity.Status != VoucherStatus.Submitted)
            throw new AppException("Only submitted vouchers can be approved.");

        if (!entity.JournalEntryId.HasValue)
            entity.JournalEntryId = await CreateJournalEntryAsync(entity, ct);

        entity.Status = VoucherStatus.Approved;
        entity.ApprovedBy = request.ApprovedBy;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await VoucherGuard.ReloadReferencedAsync(_context, entity, ct);
        return _mapper.ToDto(entity);
    }

    private async Task<Guid> CreateJournalEntryAsync(Models.Entities.Voucher voucher, CancellationToken ct)
    {
        var dto = new CreateJournalEntryDto
        {
            EntryDate = voucher.VoucherDate,
            ReferenceNumber = voucher.VoucherNumber,
            Description = $"Voucher {voucher.VoucherNumber} - {voucher.Description}",
            Lines =
            [
                // Both lines carry the voucher's cost center. That is the only thing tying the
                // amount back to a project: ProjectFinancialService reads posted journal entry
                // lines by cost center, so a line without one is invisible to project reporting.
                new CreateJournalEntryLineDto
                {
                    AccountId = voucher.DebitAccountId,
                    Description = voucher.Description,
                    DebitAmount = voucher.Amount,
                    CostCenterId = voucher.CostCenterId,
                    LineOrder = 1
                },
                new CreateJournalEntryLineDto
                {
                    AccountId = voucher.CreditAccountId,
                    Description = voucher.Description,
                    CreditAmount = voucher.Amount,
                    CostCenterId = voucher.CostCenterId,
                    LineOrder = 2
                }
            ]
        };

        var je = await _jeService.CreateAsync(dto);

        // IBaseService.CreateAsync has no author parameter, so stamp the voucher's
        // preparer onto the generated entry rather than leaving it as "System".
        var entry = await _context.Set<Models.Entities.JournalEntry>()
            .FirstOrDefaultAsync(x => x.Id == je.Id, ct);
        if (entry is not null)
            entry.CreatedBy = voucher.CreatedBy;

        return je.Id;
    }
}

public class PostVoucherCommandHandler : IRequestHandler<PostVoucherCommand, VoucherResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IJournalEntryService _jeService;
    private readonly VoucherMapper _mapper = new();

    public PostVoucherCommandHandler(AppDbContext context, IJournalEntryService jeService)
    {
        _context = context;
        _jeService = jeService;
    }

    public async Task<VoucherResponseDto> Handle(PostVoucherCommand request, CancellationToken ct)
    {
        var entity = await VoucherQueryHelper.BaseQuery(_context)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Voucher), request.Id);

        if (entity.Status != VoucherStatus.Approved)
            throw new AppException("Only approved vouchers can be posted.");

        if (!entity.JournalEntryId.HasValue)
            throw new AppException("Voucher has no journal entry to post. Approve it first.");

        await _jeService.PostAsync(entity.JournalEntryId.Value, request.PostedBy);

        entity.Status = VoucherStatus.Posted;
        entity.PostedBy = request.PostedBy;
        entity.PostedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await VoucherGuard.ReloadReferencedAsync(_context, entity, ct);
        return _mapper.ToDto(entity);
    }
}

public class RejectVoucherCommandHandler : IRequestHandler<RejectVoucherCommand, VoucherResponseDto>
{
    private readonly AppDbContext _context;
    private readonly VoucherMapper _mapper = new();

    public RejectVoucherCommandHandler(AppDbContext context) => _context = context;

    public async Task<VoucherResponseDto> Handle(RejectVoucherCommand request, CancellationToken ct)
    {
        var entity = await VoucherQueryHelper.BaseQuery(_context)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Voucher), request.Id);

        if (entity.Status != VoucherStatus.Submitted)
            throw new AppException("Only submitted vouchers can be rejected.");

        entity.Status = VoucherStatus.Rejected;
        entity.RejectedBy = request.RejectedBy;
        entity.RejectedAt = DateTime.UtcNow;
        entity.RejectReason = request.Reason;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await VoucherGuard.ReloadReferencedAsync(_context, entity, ct);
        return _mapper.ToDto(entity);
    }
}

public class CancelVoucherCommandHandler : IRequestHandler<CancelVoucherCommand, VoucherResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IJournalEntryService _jeService;
    private readonly VoucherMapper _mapper = new();

    public CancelVoucherCommandHandler(AppDbContext context, IJournalEntryService jeService)
    {
        _context = context;
        _jeService = jeService;
    }

    public async Task<VoucherResponseDto> Handle(CancelVoucherCommand request, CancellationToken ct)
    {
        var entity = await VoucherQueryHelper.BaseQuery(_context)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Voucher), request.Id);

        if (entity.Status == VoucherStatus.Posted)
            throw new AppException("Posted vouchers cannot be cancelled. Reverse the journal entry instead.");

        if (entity.Status == VoucherStatus.Cancelled)
            throw new AppException("Voucher is already cancelled.");

        if (entity.JournalEntryId.HasValue)
            await _jeService.DeleteAsync(entity.JournalEntryId.Value);

        entity.Status = VoucherStatus.Cancelled;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await VoucherGuard.ReloadReferencedAsync(_context, entity, ct);
        return _mapper.ToDto(entity);
    }
}

public class GetAllVouchersForExportQueryHandler : IRequestHandler<GetAllVouchersForExportQuery, List<VoucherResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly VoucherMapper _mapper = new();

    public GetAllVouchersForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<VoucherResponseDto>> Handle(GetAllVouchersForExportQuery request, CancellationToken ct)
    {
        var items = await VoucherQueryHelper.BaseQuery(_context)
            .OrderByDescending(x => x.VoucherDate)
            .ToListAsync(ct);

        return items.Select(_mapper.ToDto).ToList();
    }
}

/// <summary>
/// Serves both dropdowns for one voucher type from <see cref="VoucherAccountRules"/> — the same
/// rules the save-time guard applies, so the form can never offer an account the server rejects.
/// </summary>
public class GetVoucherAccountOptionsQueryHandler : IRequestHandler<GetVoucherAccountOptionsQuery, VoucherAccountOptionsDto>
{
    private readonly AppDbContext _context;
    private readonly VoucherMapper _mapper = new();

    public GetVoucherAccountOptionsQueryHandler(AppDbContext context) => _context = context;

    public async Task<VoucherAccountOptionsDto> Handle(GetVoucherAccountOptionsQuery request, CancellationToken ct)
    {
        var moneyTypes = VoucherAccountRules.MoneyAccountTypes.ToList();
        var partyTypes = VoucherAccountRules.PartyAccountTypes(request.VoucherType).ToList();
        var wanted = moneyTypes.Union(partyTypes).ToList();

        var accounts = await _context.Set<Models.Entities.ChartOfAccount>()
            .Include(x => x.AccountGroup)
            .Where(x => !x.IsDeleted && x.IsActive && x.AllowDirectEntry && wanted.Contains(x.AccountType))
            .OrderBy(x => x.AccountCode)
            .ToListAsync(ct);

        // Grouped rather than keyed directly: nothing stops two bank accounts pointing at the same
        // GL account, and a duplicate key here would take the whole dropdown down.
        var bankAccounts = await _context.Set<Models.Entities.BankAccount>()
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.AccountNumber)
            .ToListAsync(ct);

        var banksByAccount = bankAccounts
            .GroupBy(x => x.ChartOfAccountId)
            .ToDictionary(g => g.Key, g => g.First());

        VoucherAccountOptionDto ToOption(Models.Entities.ChartOfAccount account)
        {
            var dto = _mapper.ToAccountOptionDto(account);
            if (banksByAccount.TryGetValue(account.Id, out var bank))
            {
                dto.IsBankLinked = true;
                dto.BankName = bank.BankName;
                dto.AccountNumber = bank.AccountNumber;
            }
            return dto;
        }

        // Bank-backed accounts first: on the money side they are what the user reaches for most.
        var money = accounts
            .Where(a => moneyTypes.Contains(a.AccountType))
            .Select(ToOption)
            .OrderByDescending(o => o.IsBankLinked)
            .ThenBy(o => o.AccountCode)
            .ToList();

        var party = accounts
            .Where(a => partyTypes.Contains(a.AccountType))
            .Select(ToOption)
            .ToList();

        return new VoucherAccountOptionsDto
        {
            VoucherType = request.VoucherType,
            MoneyAccounts = money,
            PartyAccounts = party,
            MoneyIsOnDebitSide = VoucherAccountRules.MoneyIsOnDebitSide(request.VoucherType),
            MoneyFieldLabel = VoucherAccountRules.FieldLabel(request.VoucherType, VoucherAccountRole.Money)
        };
    }
}

file static class VoucherQueryHelper
{
    public static IQueryable<Models.Entities.Voucher> BaseQuery(AppDbContext context)
        => context.Set<Models.Entities.Voucher>()
            .Include(x => x.DebitAccount)
            .Include(x => x.CreditAccount)
            .Include(x => x.FiscalYear)
            .Include(x => x.CostCenter)
            .Include(x => x.JournalEntry)
            .Where(x => !x.IsDeleted);
}

file static class VoucherGuard
{
    /// <summary>
    /// Checks both GL accounts against <see cref="VoucherAccountRules"/> — the same rules that
    /// built the dropdowns — so the entry the voucher will generate is a valid double entry:
    ///
    /// Credit Voucher (money in):  debit the receiving cash/bank account, credit the source.
    /// Debit Voucher  (money out): debit the expense/payable, credit the paying cash/bank account.
    /// </summary>
    public static async Task ValidateAccountsAsync(
        AppDbContext context,
        VoucherType voucherType,
        Guid debitAccountId,
        Guid creditAccountId,
        CancellationToken ct)
    {
        if (debitAccountId == creditAccountId)
            throw new ValidationException(new List<string> { "Debit and credit accounts must be different." });

        var debit = await LoadPostableAccountAsync(context, debitAccountId, "Debit", ct);
        var credit = await LoadPostableAccountAsync(context, creditAccountId, "Credit", ct);

        Check(debit, VoucherAccountRules.RoleOfDebitSide(voucherType));
        Check(credit, VoucherAccountRules.RoleOfCreditSide(voucherType));

        void Check(Models.Entities.ChartOfAccount account, VoucherAccountRole role)
        {
            if (VoucherAccountRules.IsEligible(account, voucherType, role))
                return;

            var label = VoucherAccountRules.FieldLabel(voucherType, role);
            var allowed = string.Join(", ", VoucherAccountRules.TypesFor(voucherType, role));

            var reason = role == VoucherAccountRole.Money
                ? "money can only be received into or paid from an asset account such as cash or a bank account"
                : $"a {voucherType} Voucher cannot be booked against an account of type {account.AccountType}";

            throw new ValidationException(new List<string>
            {
                $"'{account.AccountName}' is not valid for {label} on a {voucherType} Voucher — {reason}. " +
                $"Allowed account types: {allowed}."
            });
        }
    }

    /// <summary>
    /// Turns the voucher's charge target into the cost center its journal entry lines will carry.
    /// A project resolves to its own cost center — the one auto-created alongside the project.
    /// An overhead voucher names a cost center directly; if that cost center happens to belong to
    /// a project, the project is recorded too so the two never disagree.
    /// </summary>
    public static async Task<(Guid CostCenterId, Guid? ProjectId)> ResolveChargeTargetAsync(
        AppDbContext context,
        Guid? projectId,
        Guid? costCenterId,
        CancellationToken ct)
    {
        if (projectId.HasValue && projectId.Value != Guid.Empty)
        {
            var forProject = await context.Set<Models.Entities.CostCenter>()
                .Where(c => !c.IsDeleted && c.IsActive && c.ProjectId == projectId.Value)
                .OrderBy(c => c.CreatedAt)
                .FirstOrDefaultAsync(ct)
                ?? throw new ValidationException(new List<string>
                {
                    "The selected project has no active cost center, so the voucher cannot be charged to it. " +
                    "Add a cost center for the project under Accounts › Cost Centers."
                });

            return (forProject.Id, projectId.Value);
        }

        if (costCenterId.HasValue && costCenterId.Value != Guid.Empty)
        {
            var costCenter = await context.Set<Models.Entities.CostCenter>()
                .FirstOrDefaultAsync(c => c.Id == costCenterId.Value && !c.IsDeleted, ct)
                ?? throw new ValidationException(new List<string> { "Cost center is not valid." });

            if (!costCenter.IsActive)
                throw new ValidationException(new List<string> { $"Cost center '{costCenter.Name}' is inactive." });

            return (costCenter.Id, costCenter.ProjectId);
        }

        throw new ValidationException(new List<string>
        {
            "Select either a project or, for head-office / overhead vouchers, a cost center."
        });
    }

    private static async Task<Models.Entities.ChartOfAccount> LoadPostableAccountAsync(
        AppDbContext context,
        Guid accountId,
        string label,
        CancellationToken ct)
    {
        var account = await context.Set<Models.Entities.ChartOfAccount>()
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, ct)
            ?? throw new ValidationException(new List<string> { $"{label} account is not valid." });

        if (!account.IsActive)
            throw new ValidationException(new List<string> { $"{label} account '{account.AccountName}' is inactive." });

        if (!account.AllowDirectEntry)
            throw new ValidationException(new List<string> { $"{label} account '{account.AccountName}' does not allow direct entry." });

        return account;
    }

    public static async Task ValidateFiscalYearAsync(AppDbContext context, Guid fiscalYearId, DateTime date, CancellationToken ct)
    {
        var fy = await context.Set<Models.Entities.FiscalYear>().FirstOrDefaultAsync(x => x.Id == fiscalYearId && !x.IsDeleted, ct)
            ?? throw new ValidationException(new List<string> { "Fiscal year is not valid." });

        if (fy.Status == FiscalYearStatus.Closed)
            throw new ValidationException(new List<string> { $"Fiscal year '{fy.Name}' is closed." });

        if (date < fy.StartDate || date > fy.EndDate)
            throw new ValidationException(new List<string> { $"Voucher date is outside fiscal year '{fy.Name}'." });
    }

    public static async Task ReloadReferencedAsync(AppDbContext context, Models.Entities.Voucher entity, CancellationToken ct)
    {
        await context.Entry(entity).Reference(x => x.DebitAccount).LoadAsync(ct);
        await context.Entry(entity).Reference(x => x.CreditAccount).LoadAsync(ct);
        await context.Entry(entity).Reference(x => x.FiscalYear).LoadAsync(ct);
        await context.Entry(entity).Reference(x => x.CostCenter).LoadAsync(ct);
        await context.Entry(entity).Reference(x => x.JournalEntry).LoadAsync(ct);
    }
}