using OrgHub.Domain.Entities.HRM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgHub.Application.Features.HRM.Departments.DTOs;

public class HRM_DepartmentDtos
{
    public int Id { get; set; }
    public required string Name { get; set; }
    [StringLength(50)]
    public required string Code { get; set; }
    public int? ParentDepartmentId { get; set; }
    public int? HeadEmployeeId { get; set; }
    public bool IsActive { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? LastUpdatedBy { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
}
