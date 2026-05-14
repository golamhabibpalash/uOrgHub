using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.Budget;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.Budget;

public record GetBudgetsQuery(PaginationRequest Request, Guid? FiscalYearId = null) : IQuery<PagedResult<BudgetResponseDto>>;
public record GetBudgetByIdQuery(Guid Id) : IQuery<BudgetResponseDto>;
public record CreateBudgetCommand(CreateBudgetDto Dto) : ICommand<BudgetResponseDto>;
public record UpdateBudgetCommand(Guid Id, UpdateBudgetDto Dto) : ICommand<BudgetResponseDto>;
public record ApproveBudgetCommand(Guid Id) : ICommand<BudgetResponseDto>;
public record DeleteBudgetCommand(Guid Id) : ICommand<Unit>;

public class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, PagedResult<BudgetResponseDto>>
{
    private readonly AppDbContext _context;
    public GetBudgetsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<BudgetResponseDto>> Handle(GetBudgetsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Budget>()
            .Include(x => x.Lines)
                .ThenInclude(l => l.Account)
            .Where(x => !x.IsDeleted);

        if (request.FiscalYearId.HasValue)
            query = query.Where(x => x.FiscalYearId == request.FiscalYearId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<BudgetResponseDto>
        {
            Items = items.Select(BudgetMappingHelper.ToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetBudgetByIdQueryHandler : IRequestHandler<GetBudgetByIdQuery, BudgetResponseDto>
{
    private readonly AppDbContext _context;
    public GetBudgetByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<BudgetResponseDto> Handle(GetBudgetByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Budget>()
            .Include(x => x.Lines)
                .ThenInclude(l => l.Account)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Budget), request.Id);

        return BudgetMappingHelper.ToDto(e);
    }
}

public class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, BudgetResponseDto>
{
    private readonly AppDbContext _context;
    public CreateBudgetCommandHandler(AppDbContext context) => _context = context;

    public async Task<BudgetResponseDto> Handle(CreateBudgetCommand request, CancellationToken ct)
    {
        var entity = new Models.Entities.Budget
        {
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            FiscalYearId = request.Dto.FiscalYearId,
            CostCenterId = request.Dto.CostCenterId,
            Status = BudgetStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var lineDto in request.Dto.Lines)
        {
            entity.Lines.Add(new Models.Entities.BudgetLine
            {
                AccountId = lineDto.AccountId,
                CostCenterId = lineDto.CostCenterId,
                Period = lineDto.Period,
                PlannedAmount = lineDto.PlannedAmount,
                ActualAmount = 0,
                CreatedAt = DateTime.UtcNow
            });
        }

        entity.TotalAmount = entity.Lines.Sum(l => l.PlannedAmount);

        _context.Set<Models.Entities.Budget>().Add(entity);
        await _context.SaveChangesAsync(ct);

        await _context.Entry(entity).Collection(x => x.Lines).Query()
            .Include(l => l.Account).LoadAsync(ct);

        return BudgetMappingHelper.ToDto(entity);
    }
}

public class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, BudgetResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateBudgetCommandHandler(AppDbContext context) => _context = context;

    public async Task<BudgetResponseDto> Handle(UpdateBudgetCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Budget>()
            .Include(x => x.Lines)
                .ThenInclude(l => l.Account)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Budget), request.Id);

        if (entity.Status != BudgetStatus.Draft)
            throw new AppException("Only draft budgets can be updated.");

        entity.Name = request.Dto.Name;
        entity.Description = request.Dto.Description;
        entity.CostCenterId = request.Dto.CostCenterId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return BudgetMappingHelper.ToDto(entity);
    }
}

public class ApproveBudgetCommandHandler : IRequestHandler<ApproveBudgetCommand, BudgetResponseDto>
{
    private readonly AppDbContext _context;
    public ApproveBudgetCommandHandler(AppDbContext context) => _context = context;

    public async Task<BudgetResponseDto> Handle(ApproveBudgetCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Budget>()
            .Include(x => x.Lines)
                .ThenInclude(l => l.Account)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Budget), request.Id);

        if (entity.Status != BudgetStatus.Draft)
            throw new AppException("Only draft budgets can be approved.");

        entity.Status = BudgetStatus.Approved;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return BudgetMappingHelper.ToDto(entity);
    }
}

public class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteBudgetCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteBudgetCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Budget>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Budget), request.Id);

        if (entity.Status != BudgetStatus.Draft && entity.Status != BudgetStatus.Cancelled)
            throw new AppException("Only draft or cancelled budgets can be deleted.");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

file static class BudgetMappingHelper
{
    public static BudgetResponseDto ToDto(Models.Entities.Budget e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        Status = e.Status,
        TotalAmount = e.TotalAmount,
        FiscalYearId = e.FiscalYearId,
        CostCenterId = e.CostCenterId,
        Lines = e.Lines.Select(l => new BudgetLineResponseDto
        {
            Id = l.Id,
            AccountId = l.AccountId,
            AccountName = l.Account?.AccountName ?? string.Empty,
            CostCenterId = l.CostCenterId,
            Period = l.Period,
            PlannedAmount = l.PlannedAmount,
            ActualAmount = l.ActualAmount
        }).ToList()
    };
}
