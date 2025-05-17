using MediatR;
using OrgHub.Application.Features.Employees.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgHub.Application.Features.Employees.Commands;

public class GetByIdCommand:IRequest<EmployeeDto>
{
    public int Id { get; set; }
	public GetByIdCommand(int id)
	{
		Id = id;
    }
}
