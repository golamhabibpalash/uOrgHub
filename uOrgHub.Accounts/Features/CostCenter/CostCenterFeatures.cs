using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.CostCenter;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.CostCenter;

public record GetCostCentersQuery(PaginationRequest Request, Guid? ProjectId = null) : IQuery<PagedResult<CostCenterResponseDto>>;
public record GetCostCenterByIdQuery(Guid Id) : IQuery<CostCenterResponseDto>;
public record CreateCostCenterCommand(CreateCostCenterDto Dto) : ICommand<CostCenterResponseDto>;
public record UpdateCostCenterCommand(Guid Id, UpdateCostCenterDto Dto) : ICommand<CostCenterResponseDto>;
public record DeleteCostCenterCommand(Guid Id) : ICommand<Unit>;
public record GetAllCostCentersForExportQuery : IQuery<List<CostCenterResponseDto>>;

public class GetCostCentersQueryHandler : IRequestHandler<GetCostCentersQuery, PagedResult<CostCenterResponseDto>>
{
    private readonly AppDbContext _context;
    public GetCostCentersQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<CostCenterResponseDto>> Handle(GetCostCentersQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.CostCenter>()
            .Include(x => x.ParentCostCenter)
            .Where(x => !x.IsDeleted);

        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Name, x => x.Code);

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<CostCenterResponseDto>
        {
            Items = items.Select(CostCenterMappingHelper.ToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, CostCenterResponseDto>
{
    private readonly AppDbContext _context;
    public GetCostCenterByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<CostCenterResponseDto> Handle(GetCostCenterByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.CostCenter>()
            .Include(x => x.ParentCostCenter)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.CostCenter), request.Id);

        return CostCenterMappingHelper.ToDto(e);
    }
}

public class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, CostCenterResponseDto>
{
    private readonly AppDbContext _context;
    public CreateCostCenterCommandHandler(AppDbContext context) => _context = context;

    public async Task<CostCenterResponseDto> Handle(CreateCostCenterCommand request, CancellationToken ct)
    {
        var code = request.Dto.Code;
        if (string.IsNullOrWhiteSpace(code))
        {
            var count = await _context.Set<Models.Entities.CostCenter>().IgnoreQueryFilters().CountAsync(ct);
            code = $"CC-{DateTime.UtcNow.Year}-{(count + 1):D4}";
        }
        if (await _context.Set<Models.Entities.CostCenter>().AnyAsync(x => x.Code == code && !x.IsDeleted, ct))
            throw new AppException($"Cost center code '{code}' already exists.");

        var entity = new Models.Entities.CostCenter
        {
            Code = code,
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            ParentCostCenterId = request.Dto.ParentCostCenterId,
            DepartmentId = request.Dto.DepartmentId,
            ProjectId = request.Dto.ProjectId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Models.Entities.CostCenter>().Add(entity);
        await _context.SaveChangesAsync(ct);

        await _context.Entry(entity).Reference(x => x.ParentCostCenter).LoadAsync(ct);
        return CostCenterMappingHelper.ToDto(entity);
    }
}

public class UpdateCostCenterCommandHandler : IRequestHandler<UpdateCostCenterCommand, CostCenterResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateCostCenterCommandHandler(AppDbContext context) => _context = context;

    public async Task<CostCenterResponseDto> Handle(UpdateCostCenterCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.CostCenter>()
            .Include(x => x.ParentCostCenter)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.CostCenter), request.Id);

        entity.Name = request.Dto.Name;
        entity.Description = request.Dto.Description;
        entity.ParentCostCenterId = request.Dto.ParentCostCenterId;
        entity.DepartmentId = request.Dto.DepartmentId;
        entity.ProjectId = request.Dto.ProjectId;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return CostCenterMappingHelper.ToDto(entity);
    }
}

public class DeleteCostCenterCommandHandler : IRequestHandler<DeleteCostCenterCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteCostCenterCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteCostCenterCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.CostCenter>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.CostCenter), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class GetAllCostCentersForExportQueryHandler : IRequestHandler<GetAllCostCentersForExportQuery, List<CostCenterResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllCostCentersForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<CostCenterResponseDto>> Handle(GetAllCostCentersForExportQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Models.Entities.CostCenter>()
            .Include(x => x.ParentCostCenter)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
        return items.Select(CostCenterMappingHelper.ToDto).ToList();
    }
}

file static class CostCenterMappingHelper
{
    public static CostCenterResponseDto ToDto(Models.Entities.CostCenter e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        IsActive = e.IsActive,
        ParentCostCenterId = e.ParentCostCenterId,
        ParentCostCenterName = e.ParentCostCenter?.Name,
        DepartmentId = e.DepartmentId,
        ProjectId = e.ProjectId
    };
}
