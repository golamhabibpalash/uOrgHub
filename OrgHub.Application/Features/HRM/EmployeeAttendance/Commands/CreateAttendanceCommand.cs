using MediatR;

namespace OrgHub.Application.Features.HRM.EmployeeAttendance.Commands;

public class CreateAttendanceCommand : IRequest<CreateAttendanceDto>
{
    public required CreateAttendanceDto CreateAttendanceDto { get; set; }
}
