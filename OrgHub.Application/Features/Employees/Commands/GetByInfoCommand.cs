using MediatR;
using OrgHub.Application.Features.Employees.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgHub.Application.Features.Employees.Commands;

public class GetByInfoCommand : IRequest<List<EmployeeDto>>
{
    public string Info { get; set; }
    public GetByInfoCommand(string info)
    {
        Info = info;
    }
}

