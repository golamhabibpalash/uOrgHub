using MediatR;
using OrgHub.Application.Features.HRM.Departments.Commands;
using OrgHub.Application.Features.HRM.Departments.DTOs;
using OrgHub.Application.Features.HRM.Departments.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace OrgHub.Application.Features.HRM.Departments.Handlers;

public class CreateDepartmentHandler (IDepartmentService departmentService) : IRequestHandler<CreateDepartmentCommand, CreateDepartmentDtos>
{
    private readonly IDepartmentService _departmentService = departmentService;
    public async Task<CreateDepartmentDtos> Handle( CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        HRM_DepartmentDtos hRM_DepartmentDtos = new()
        {
            Name = request.CreateDepartmentDtos.Name,
            Code = request.CreateDepartmentDtos.Code,
            ParentDepartmentId = request.CreateDepartmentDtos.ParentDepartmentId,
            HeadEmployeeId = request.CreateDepartmentDtos.HeadEmployeeId,
            IsActive = true,
            CreatedAt = DateTime.Now,
            CreatedBy = Guid.NewGuid()
        };
        var result = await _departmentService.AddAsync(hRM_DepartmentDtos);
        return new CreateDepartmentDtos
        {
            Name = result.Name,
            Code = result.Code,
            ParentDepartmentId = result.ParentDepartmentId,
            HeadEmployeeId = result.HeadEmployeeId
        };
    }
}
