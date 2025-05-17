using MediatR;
using OrgHub.Application.Features.Employees.Models;
using System;

namespace OrgHub.Application.Features.Employees.Commands;

/// <summary>
/// Command to update an employee.
/// </summary>
public class UpdateEmployeeCommand : IRequest<EmployeeDto>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Designation { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public DateTime JoiningDate { get; set; }
    public bool IsActive { get; set; }
}