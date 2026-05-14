using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.TaxRate;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.TaxRate;

public record GetTaxRatesQuery(PaginationRequest Request) : IQuery<PagedResult<TaxRateResponseDto>>;
public record GetTaxRateByIdQuery(Guid Id) : IQuery<TaxRateResponseDto>;
public record CreateTaxRateCommand(CreateTaxRateDto Dto) : ICommand<TaxRateResponseDto>;
public record UpdateTaxRateCommand(Guid Id, UpdateTaxRateDto Dto) : ICommand<TaxRateResponseDto>;
public record DeleteTaxRateCommand(Guid Id) : ICommand<Unit>;

public class GetTaxRatesQueryHandler : IRequestHandler<GetTaxRatesQuery, PagedResult<TaxRateResponseDto>>
{
    private readonly AppDbContext _context;
    public GetTaxRatesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<TaxRateResponseDto>> Handle(GetTaxRatesQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.TaxRate>()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search) || x.Code.Contains(request.Request.Search));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<TaxRateResponseDto>
        {
            Items = items.Select(TaxRateMappingHelper.ToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetTaxRateByIdQueryHandler : IRequestHandler<GetTaxRateByIdQuery, TaxRateResponseDto>
{
    private readonly AppDbContext _context;
    public GetTaxRateByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<TaxRateResponseDto> Handle(GetTaxRateByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.TaxRate>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.TaxRate), request.Id);

        return TaxRateMappingHelper.ToDto(e);
    }
}

public class CreateTaxRateCommandHandler : IRequestHandler<CreateTaxRateCommand, TaxRateResponseDto>
{
    private readonly AppDbContext _context;
    public CreateTaxRateCommandHandler(AppDbContext context) => _context = context;

    public async Task<TaxRateResponseDto> Handle(CreateTaxRateCommand request, CancellationToken ct)
    {
        if (await _context.Set<Models.Entities.TaxRate>().AnyAsync(x => x.Code == request.Dto.Code && !x.IsDeleted, ct))
            throw new AppException($"Tax rate code '{request.Dto.Code}' already exists.");

        var entity = new Models.Entities.TaxRate
        {
            Code = request.Dto.Code,
            Name = request.Dto.Name,
            TaxType = request.Dto.TaxType,
            Rate = request.Dto.Rate,
            Description = request.Dto.Description,
            TaxAccountId = request.Dto.TaxAccountId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Models.Entities.TaxRate>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return TaxRateMappingHelper.ToDto(entity);
    }
}

public class UpdateTaxRateCommandHandler : IRequestHandler<UpdateTaxRateCommand, TaxRateResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateTaxRateCommandHandler(AppDbContext context) => _context = context;

    public async Task<TaxRateResponseDto> Handle(UpdateTaxRateCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.TaxRate>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.TaxRate), request.Id);

        entity.Name = request.Dto.Name;
        entity.TaxType = request.Dto.TaxType;
        entity.Rate = request.Dto.Rate;
        entity.Description = request.Dto.Description;
        entity.TaxAccountId = request.Dto.TaxAccountId;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return TaxRateMappingHelper.ToDto(entity);
    }
}

public class DeleteTaxRateCommandHandler : IRequestHandler<DeleteTaxRateCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteTaxRateCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteTaxRateCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.TaxRate>()
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.TaxRate), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

file static class TaxRateMappingHelper
{
    public static TaxRateResponseDto ToDto(Models.Entities.TaxRate e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        TaxType = e.TaxType,
        Rate = e.Rate,
        Description = e.Description,
        IsActive = e.IsActive,
        TaxAccountId = e.TaxAccountId
    };
}
