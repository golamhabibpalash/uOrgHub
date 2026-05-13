using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.DTOs.Validators;
using uOrgHub.HR.Repositories;
using uOrgHub.HR.Services;

namespace uOrgHub.HR;

public static class HRServiceExtension
{
    public static IServiceCollection AddHRModule(this IServiceCollection services)
    {
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDesignationRepository, DesignationRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDesignationService, DesignationService>();
        services.AddScoped<IEmployeeService, EmployeeService>();

        services.AddScoped<IValidator<CreateDepartmentDto>, CreateDepartmentDtoValidator>();
        services.AddScoped<IValidator<UpdateDepartmentDto>, UpdateDepartmentDtoValidator>();
        services.AddScoped<IValidator<CreateDesignationDto>, CreateDesignationDtoValidator>();
        services.AddScoped<IValidator<UpdateDesignationDto>, UpdateDesignationDtoValidator>();
        services.AddScoped<IValidator<CreateEmployeeDto>, CreateEmployeeDtoValidator>();
        services.AddScoped<IValidator<UpdateEmployeeDto>, UpdateEmployeeDtoValidator>();

        return services;
    }
}
