using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public class UnitOfMeasureRepository : BaseRepository<UnitOfMeasure>, IUnitOfMeasureRepository
{
    public UnitOfMeasureRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<UnitOfMeasure> ApplySearch(IQueryable<UnitOfMeasure> query, string search)
        => query.Where(x => x.Name.Contains(search) || x.Abbreviation.Contains(search));

    protected override IQueryable<UnitOfMeasure> ApplyOrdering(IQueryable<UnitOfMeasure> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

    public async Task<bool> AbbreviationExistsAsync(string abbreviation, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.Abbreviation == abbreviation && (excludeId == null || x.Id != excludeId));
}
