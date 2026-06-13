using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_numbering_sequences")]
public class NumberingSequence
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    [Required][MaxLength(50)] public string DocumentType { get; set; } = string.Empty;

    [Required][MaxLength(20)] public string Prefix { get; set; } = string.Empty;

    public int Year { get; set; }

    public int? Month { get; set; }

    public int LastSequence { get; set; }

    [Required][MaxLength(100)] public string Pattern { get; set; } = string.Empty;
}
