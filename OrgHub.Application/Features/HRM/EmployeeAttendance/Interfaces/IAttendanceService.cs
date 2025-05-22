using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Features.HRM.EmployeeAttendance.DTOs;
using OrgHub.Domain.Entities.HRM;

namespace OrgHub.Application.Features.HRM.EmployeeAttendance.Interfaces;

public interface IAttendanceService : IService<HRM_Attendance, AttendanceDto>
{

}
