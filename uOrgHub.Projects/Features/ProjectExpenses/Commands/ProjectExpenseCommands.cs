using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.ProjectExpenses.Commands;

public record CreateProjectExpenseCommand(CreateProjectExpenseDto Dto) : ICommand<ProjectExpenseResponseDto>;
public record UpdateProjectExpenseCommand(Guid Id, UpdateProjectExpenseDto Dto) : ICommand<ProjectExpenseResponseDto>;
public record DeleteProjectExpenseCommand(Guid Id) : ICommand<Unit>;
public record ApproveProjectExpenseCommand(Guid Id, ApproveExpenseDto Dto) : ICommand<ProjectExpenseResponseDto>;

public class CreateProjectExpenseCommandHandler : IRequestHandler<CreateProjectExpenseCommand, ProjectExpenseResponseDto>
{
    private readonly AppDbContext _context;
    public CreateProjectExpenseCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectExpenseResponseDto> Handle(CreateProjectExpenseCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var _ = await _context.Set<Project>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.ProjectId, ct)
            ?? throw new AppException($"Project '{dto.ProjectId}' not found.", 404);

        var count = await _context.Set<ProjectExpense>().CountAsync(ct);
        var expenseNumber = $"EXP-{DateTime.UtcNow.Year}-{(count + 1):D4}";

        var entity = new ProjectExpense
        {
            ExpenseNumber = expenseNumber,
            ProjectId = dto.ProjectId,
            WBSId = dto.WBSId,
            ExpenseDate = dto.ExpenseDate,
            ExpenseType = dto.ExpenseType,
            Description = dto.Description,
            Amount = dto.Amount,
            VendorId = dto.VendorId,
            POId = dto.POId,
            InvoiceNumber = dto.InvoiceNumber,
            RecordedById = dto.RecordedById,
            CostCenterId = dto.CostCenterId,
            Status = ExpenseStatus.Draft,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ProjectExpense>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return ExpenseMapper.ToDto(entity);
    }
}

public class UpdateProjectExpenseCommandHandler : IRequestHandler<UpdateProjectExpenseCommand, ProjectExpenseResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateProjectExpenseCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectExpenseResponseDto> Handle(UpdateProjectExpenseCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectExpense>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectExpense), request.Id);

        if (entity.Status != ExpenseStatus.Draft)
            throw new AppException("Only Draft expenses can be updated.");

        var dto = request.Dto;
        entity.WBSId = dto.WBSId;
        entity.ExpenseDate = dto.ExpenseDate;
        entity.ExpenseType = dto.ExpenseType;
        entity.Description = dto.Description;
        entity.Amount = dto.Amount;
        entity.VendorId = dto.VendorId;
        entity.POId = dto.POId;
        entity.InvoiceNumber = dto.InvoiceNumber;
        entity.CostCenterId = dto.CostCenterId;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return ExpenseMapper.ToDto(entity);
    }
}

public class DeleteProjectExpenseCommandHandler : IRequestHandler<DeleteProjectExpenseCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteProjectExpenseCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteProjectExpenseCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectExpense>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectExpense), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class ApproveProjectExpenseCommandHandler : IRequestHandler<ApproveProjectExpenseCommand, ProjectExpenseResponseDto>
{
    private readonly AppDbContext _context;
    public ApproveProjectExpenseCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProjectExpenseResponseDto> Handle(ApproveProjectExpenseCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectExpense>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectExpense), request.Id);

        if (entity.Status != ExpenseStatus.Draft)
            throw new AppException("Only Draft expenses can be approved.");

        var costCenterId = entity.CostCenterId;

        entity.Status = ExpenseStatus.Approved;
        entity.ApprovedById = request.Dto.ApprovedById;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        if (request.Dto.DebitAccountId.HasValue && request.Dto.CreditAccountId.HasValue)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"PEX-{year}-";
            var lastEntry = await _context.Set<JournalEntry>()
                .Where(x => x.EntryNumber.StartsWith(prefix))
                .OrderByDescending(x => x.EntryNumber)
                .FirstOrDefaultAsync(ct);
            var seq = 1;
            if (lastEntry != null && int.TryParse(lastEntry.EntryNumber.Split('-').Last(), out var lastSeq))
                seq = lastSeq + 1;
            var entryNumber = $"{prefix}{seq:D4}";

            var je = new JournalEntry
            {
                EntryNumber = entryNumber,
                EntryDate = entity.ExpenseDate,
                ReferenceNumber = entity.InvoiceNumber,
                Description = $"Project expense: {entity.Description}",
                Status = JournalEntryStatus.Posted,
                TotalDebit = entity.Amount,
                TotalCredit = entity.Amount,
                PostedBy = request.Dto.ApprovedById.ToString(),
                PostedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.Dto.ApprovedById.ToString()
            };
            je.Lines = new List<JournalEntryLine>
            {
                new()
                {
                    JournalEntryId = je.Id,
                    AccountId = request.Dto.DebitAccountId.Value,
                    Description = entity.Description,
                    DebitAmount = entity.Amount,
                    CreditAmount = 0,
                    LineOrder = 1,
                    CostCenterId = costCenterId,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    JournalEntryId = je.Id,
                    AccountId = request.Dto.CreditAccountId.Value,
                    Description = entity.Description,
                    DebitAmount = 0,
                    CreditAmount = entity.Amount,
                    LineOrder = 2,
                    CostCenterId = costCenterId,
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var line in je.Lines)
            {
                var account = await _context.Set<ChartOfAccount>().FindAsync(new object[] { line.AccountId }, ct);
                if (account == null) continue;

                if (line.DebitAmount > 0)
                {
                    account.CurrentBalance += account.AccountType is AccountGroupType.Asset or AccountGroupType.Expense
                        ? line.DebitAmount : -line.DebitAmount;
                }
                if (line.CreditAmount > 0)
                {
                    account.CurrentBalance -= account.AccountType is AccountGroupType.Asset or AccountGroupType.Expense
                        ? line.CreditAmount : -line.CreditAmount;
                }
            }

            _context.Set<JournalEntry>().Add(je);
            await _context.SaveChangesAsync(ct);
        }

        return ExpenseMapper.ToDto(entity);
    }
}

public static class ExpenseMapper
{
    public static ProjectExpenseResponseDto ToDto(ProjectExpense e) => new()
    {
        Id = e.Id,
        ExpenseNumber = e.ExpenseNumber,
        ProjectId = e.ProjectId,
        WBSId = e.WBSId,
        ExpenseDate = e.ExpenseDate,
        ExpenseType = e.ExpenseType,
        Description = e.Description,
        Amount = e.Amount,
        VendorId = e.VendorId,
        POId = e.POId,
        InvoiceNumber = e.InvoiceNumber,
        RecordedById = e.RecordedById,
        CostCenterId = e.CostCenterId,
        Status = e.Status,
        ApprovedById = e.ApprovedById,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt
    };
}
