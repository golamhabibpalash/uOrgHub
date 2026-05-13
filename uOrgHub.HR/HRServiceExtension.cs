using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.HR.Repositories;

namespace uOrgHub.HR;

public static class HRServiceExtension
{
    public static IServiceCollection AddHRModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(HRServiceExtension).Assembly));

        services.AddValidatorsFromAssembly(typeof(HRServiceExtension).Assembly);

        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDesignationRepository, DesignationRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        return services;
    }
}
