using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Projects.Services;
using uOrgHub.Shared.Services;

namespace uOrgHub.Projects;

public static class ProjectsServiceExtension
{
    public static IServiceCollection AddProjectsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProjectsServiceExtension).Assembly));
        services.AddValidatorsFromAssembly(typeof(ProjectsServiceExtension).Assembly);
        services.AddScoped<IProjectFinancialService, ProjectFinancialService>();
        services.AddScoped<IProjectCostLimitChecker, ProjectCostLimitChecker>();
        return services;
    }
}
