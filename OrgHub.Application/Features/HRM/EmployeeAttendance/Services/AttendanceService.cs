using AutoMapper;
using OrgHub.Application.Common.Services;
using OrgHub.Application.Features.HRM.EmployeeAttendance.DTOs;
using OrgHub.Application.Features.HRM.EmployeeAttendance.Interfaces;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities.HRM;

namespace OrgHub.Application.Features.HRM.EmployeeAttendance.Services;

public class AttendanceService : Service<HRM_Attendance, AttendanceDto>, IAttendanceService
{
    private readonly IRepository<HRM_Attendance> _repository;
    private readonly IMapper _mapper;
    public AttendanceService(IRepository<HRM_Attendance> repository, IMapper mapper) : base(repository, mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
}
