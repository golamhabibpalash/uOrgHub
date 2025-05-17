using AutoMapper;
using MediatR;
using OrgHub.Application.Features.Employees.Commands;
using OrgHub.Application.Features.Employees.DTOs;
using OrgHub.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgHub.Application.Features.Employees.Handlers;

public class GetByInfoHandler : IRequestHandler<GetByInfoCommand, List<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;
    public GetByInfoHandler(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }
    public async Task<List<EmployeeDto>> Handle(GetByInfoCommand request, CancellationToken cancellationToken)
    {
        var list = await _employeeRepository.GetAllByInfoAsync(request.Info);
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
