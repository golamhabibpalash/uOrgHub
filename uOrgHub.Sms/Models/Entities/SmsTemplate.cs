using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Sms.Models.Entities;

/// <summary>
/// A company's own wording for one message event (see <c>SmsTemplateCatalog</c>). Absent means
/// the catalog's built-in default is used; removing the row is "reset to default".
/// </summary>
[Table("sms_templates")]
public class SmsTemplate : BaseEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }

    [Required, MaxLength(50)]
    public string EventKey { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;
}
