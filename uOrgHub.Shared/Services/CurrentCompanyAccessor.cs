using Microsoft.AspNetCore.Http;

namespace uOrgHub.Shared.Services;

public class CurrentCompanyAccessor : ICurrentCompanyAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentCompanyAccessor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Guid? CompanyId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("company_id")?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
