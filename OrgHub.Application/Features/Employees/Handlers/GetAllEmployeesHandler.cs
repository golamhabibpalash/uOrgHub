using MediatR;
using OrgHub.Application.Features.Employees.Commands;
using OrgHub.Application.Features.Employees.DTOs;
using OrgHub.Core.Interfaces;
public class GetAllEmployeesHandler : IRequestHandler<GetAllEmployeesCommand, List<EmployeeDto>>
{
    private readonly IEmployeeRepository _repo;

    public GetAllEmployeesHandler(IEmployeeRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<EmployeeDto>> Handle(GetAllEmployeesCommand request, CancellationToken cancellationToken)
    {
        var list = await _repo.GetAllAsync();
        return list.Select(e => new EmployeeDto
        {
            Id = e.Id,
            Name = e.Name,
            EmployeeCode = e.EmployeeCode,
            Designation = e.Designation,
            Email = e.Email,
            Phone = e.Phone
        }).ToList();
    }
}
