using AutoMapper;
using MediatR;
using OrgHub.Application.Features.HRM.EmployeeAttendance.Commands;
using OrgHub.Application.Features.HRM.EmployeeAttendance.DTOs;
using OrgHub.Application.Features.HRM.EmployeeAttendance.Interfaces;

namespace OrgHub.Application.Features.HRM.EmployeeAttendance.Handlers;

public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, CreateAttendanceDto>
{
    private readonly IMapper _mapper;
    private readonly IAttendanceService _attendanceService;
    public CreateAttendanceCommandHandler(IMapper mapper, IAttendanceService attendanceService)
    {
        _mapper = mapper;
        _attendanceService = attendanceService;
    }
    public async Task<CreateAttendanceDto> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
    {
        var attendanceDto = _mapper.Map<AttendanceDto>(request.CreateAttendanceDto);
        var result = await _attendanceService.AddAsync(attendanceDto);
        return _mapper.Map<CreateAttendanceDto>(result);
    }
}
