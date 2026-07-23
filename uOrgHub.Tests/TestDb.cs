using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;

namespace uOrgHub.Tests;

/// <summary>
/// Builds in-memory AppDbContexts for tests.
///
/// AppDbContext.OnModelCreating applies entity configurations from whatever uOrgHub.* assemblies
/// happen to be loaded. The API loads every module up front, but a test process loads them lazily
/// on first use — and EF caches the model per context type, so the first test to build a context
/// decides which entities exist for the whole run. That made results depend on test ordering.
/// Touching a type from every module here forces them all in before the model is built.
/// </summary>
public static class TestDb
{
    private static readonly Type[] ModuleAnchors =
    [
        typeof(uOrgHub.HR.Models.Entities.Employee),
        typeof(uOrgHub.Accounts.Models.Entities.JournalEntryLine),
        typeof(uOrgHub.Projects.Models.Entities.Project),
        typeof(uOrgHub.Auth.Models.Entities.ApplicationUser),
        typeof(uOrgHub.Procurement.Models.Entities.PurchaseOrder),
        typeof(uOrgHub.Inventory.Models.Entities.InventoryCategory),
        // uOrgHub.Settings is not referenced by the test project; add it here alongside a
        // ProjectReference if Settings entities are ever needed in a test.
    ];

    public static AppDbContext NewContext(string? name = null)
    {
        foreach (var anchor in ModuleAnchors)
            _ = anchor.FullName;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name ?? "TestDb_" + Guid.NewGuid())
            .Options;

        return new AppDbContext(options);
    }
}
