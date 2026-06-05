using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.ProjectExpenses.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Projects.Features.ProjectExpenses.Queries;

public record GetProjectExpensesListQuery(PaginationRequest Request, Guid? ProjectId = null, ExpenseStatus? Status = null) : IQuery<PagedResult<ProjectExpenseResponseDto>>;
public record GetProjectExpenseByIdQuery(Guid Id) : IQuery<ProjectExpenseResponseDto>;
public record GetAllProjectExpensesForExportQuery : IQuery<List<ProjectExpenseResponseDto>>;

public class GetProjectExpensesListQueryHandler : IRequestHandler<GetProjectExpensesListQuery, PagedResult<ProjectExpenseResponseDto>>
{
    private readonly AppDbContext _context;
    public GetProjectExpensesListQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ProjectExpenseResponseDto>> Handle(GetProjectExpensesListQuery request, CancellationToken ct)
    {
        var query = _context.Set<ProjectExpense>()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.ExpenseNumber, x => x.Description);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.ExpenseDate)
            : query.OrderBy(x => x.ExpenseDate);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ProjectExpenseResponseDto>
        {
            Items = items.Select(ExpenseMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetProjectExpenseByIdQueryHandler : IRequestHandler<GetProjectExpenseByIdQuery, ProjectExpenseResponseDto>
{
    private readonly AppDbContext _context;
    public GetProjectExpenseByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<ProjectExpenseResponseDto> Handle(GetProjectExpenseByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<ProjectExpense>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectExpense), request.Id);

        return ExpenseMapper.ToDto(entity);
    }
}

public class GetAllProjectExpensesForExportQueryHandler : IRequestHandler<GetAllProjectExpensesForExportQuery, List<ProjectExpenseResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllProjectExpensesForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ProjectExpenseResponseDto>> Handle(GetAllProjectExpensesForExportQuery request, CancellationToken ct)
    {
        return await _context.Set<ProjectExpense>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.ExpenseDate)
            .Select(x => ExpenseMapper.ToDto(x))
            .ToListAsync(ct);
    }
}
