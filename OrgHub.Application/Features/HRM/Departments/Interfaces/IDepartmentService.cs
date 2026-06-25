using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Features.HRM.Departments.DTOs;
using OrgHub.Domain.Entities.HRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgHub.Application.Features.HRM.Departments.Interfaces
{
    public interface IDepartmentService : IService<HRM_Department, HRM_DepartmentDtos>
    {
        
    }
}
