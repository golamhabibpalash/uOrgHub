using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public class AttributeDefinitionRepository : BaseRepository<AttributeDefinition>, IAttributeDefinitionRepository
{
    public AttributeDefinitionRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<AttributeDefinition> ApplySearch(IQueryable<AttributeDefinition> query, string search)
        => query.Where(x => x.Name.Contains(search));

    protected override IQueryable<AttributeDefinition> ApplyOrdering(IQueryable<AttributeDefinition> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

    public async Task<List<AttributeDefinition>> GetByCategoryAsync(Guid categoryId)
        => await BaseQuery().Where(x => x.CategoryId == categoryId || x.CategoryId == null).ToListAsync();
}
