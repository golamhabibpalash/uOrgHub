using System.ComponentModel.DataAnnotations;

namespace OrgHub.Domain.Entities.Others;

public class Logs
{
    [Key]
    public long Id { get; set; }

    [StringLength(50)]
    public required string Level { get; set; }
    public required string Message { get; set; }
    public string? Exception { get; set; }
    public string? Properties { get; set; }

    [StringLength(100)]
    public string? Action { get; set; }

}
