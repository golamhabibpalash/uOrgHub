using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Shared.Data;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;

    public AuditInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context == null) return;

        var httpContextAccessor = _serviceProvider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userName = httpContextAccessor?.HttpContext?.User?.FindFirst("username")?.Value ?? "system";

        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (string.IsNullOrEmpty(entry.Entity.CreatedBy))
                    entry.Entity.CreatedBy = userName;
                if (entry.Entity.CreatedAt == default)
                    entry.Entity.CreatedAt = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userName;
            }
        }

        // Sister-concern isolation (SISTER_CONCERN_PLAN.md). Separate loop from the one above:
        // NumberingSequence is company-scoped but does not inherit BaseEntity, so it wouldn't be
        // reached by Entries<BaseEntity>(). Handlers never set CompanyId themselves — it's
        // stamped here from the caller's JWT, same as CreatedBy just above.
        var companyIdClaim = httpContextAccessor?.HttpContext?.User?.FindFirst("company_id")?.Value;
        if (Guid.TryParse(companyIdClaim, out var companyId))
        {
            foreach (var entry in context.ChangeTracker.Entries<ICompanyScoped>())
            {
                if (entry.State == EntityState.Added && entry.Entity.CompanyId == Guid.Empty)
                    entry.Entity.CompanyId = companyId;
            }
        }
    }
}
