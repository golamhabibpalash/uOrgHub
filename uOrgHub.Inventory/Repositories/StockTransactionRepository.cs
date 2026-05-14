using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public class StockTransactionRepository : BaseRepository<StockTransaction>, IStockTransactionRepository
{
    public StockTransactionRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<StockTransaction> ApplySearch(IQueryable<StockTransaction> query, string search)
        => query.Where(x => x.TransactionNumber.Contains(search) || (x.ReferenceNumber != null && x.ReferenceNumber.Contains(search)));

    protected override IQueryable<StockTransaction> ApplyOrdering(IQueryable<StockTransaction> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.TransactionDate) : query.OrderBy(x => x.TransactionDate);

    public async Task<string> GenerateTransactionNumberAsync()
    {
        var count = await _context.Set<StockTransaction>().CountAsync() + 1;
        var datePart = DateTime.UtcNow.ToString("yyyyMM");
        return $"TXN-{datePart}-{count:D5}";
    }
}
