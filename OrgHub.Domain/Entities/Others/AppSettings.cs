using System.Runtime.Versioning;

namespace OrgHub.Domain.Entities.Others;

public class AppSettings
{
    public int Id { get; set; }
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string? Remarks { get; set; } = string.Empty;
}
