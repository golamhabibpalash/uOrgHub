using MediatR;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using System;

namespace OrgHub.Application.Features.HRM.Employees.Commands;

/// <summary>
/// Command to update an employee.
/// </summary>
public class DeleteEmployeeCommand : IRequest<EmployeeDto>
{
    public DeleteEmployeeCommand(int id)
    {
        Id = id;
    }

    public int Id { get; set; }
}