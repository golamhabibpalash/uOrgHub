using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrgHub.Domain.Entities
{
    public class Employee : CommonProps
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string EmployeeCode { get; set; }
        public required string Name { get; set; }
        public required string Designation { get; set; }
        public string Phone { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime JoiningDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
