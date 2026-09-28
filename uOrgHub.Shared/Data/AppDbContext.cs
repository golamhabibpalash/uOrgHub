using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Services;

namespace uOrgHub.Shared.Data;

public class AppDbContext : DbContext
{
    // Sister-concern isolation (SISTER_CONCERN_PLAN.md). Null outside a real request (seeders,
    // migrations, tests) or for a company-less user — ApplyCompanyScopeFilters treats null as
    // "don't filter" rather than "show nothing". Optional/nullable so every existing
    // `new AppDbContext(options)` call site (tests included) keeps compiling unchanged; EF's
    // AddDbContext resolves it from DI automatically for the real API.
    private readonly Guid? _companyId;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentCompanyAccessor? currentCompany = null) : base(options)
    {
        _companyId = currentCompany?.CompanyId;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var moduleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("uOrgHub.") == true)
            .ToList();

        foreach (var assembly in moduleAssemblies)
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        ApplyCompanyScopeFilters(modelBuilder);
    }

    // uOrgHub.Shared must never reference a business module (see CLAUDE.md), so this cannot list
    // entity types directly — it discovers every ICompanyScoped entity across whichever module
    // assemblies are loaded and applies the filter generically via reflection. A filter that
    // closes over `_companyId` (instance state on this context) can only be declared here, in
    // OnModelCreating itself — not inside a module's own IEntityTypeConfiguration<T>, which
    // ApplyConfigurationsFromAssembly instantiates with no constructor args and so can never see
    // this context's field. Each entity's own configuration still owns its FK/index as normal;
    // this only adds the read-time scoping on top.
    private void ApplyCompanyScopeFilters(ModelBuilder modelBuilder)
    {
        var scopedClrTypes = modelBuilder.Model.GetEntityTypes()
            .Select(e => e.ClrType)
            .Where(t => typeof(ICompanyScoped).IsAssignableFrom(t));

        var setFilterMethod = typeof(AppDbContext).GetMethod(nameof(SetCompanyScopeFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

        foreach (var clrType in scopedClrTypes)
            setFilterMethod.MakeGenericMethod(clrType).Invoke(this, [modelBuilder]);
    }

    private void SetCompanyScopeFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ICompanyScoped
    {
        Expression<Func<TEntity, bool>> filter = e => _companyId == null || e.CompanyId == _companyId;
        modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
    }
}
