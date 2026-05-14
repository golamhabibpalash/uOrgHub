using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace uOrgHub.Projects;

public static class ProjectsServiceExtension
{
    public static IServiceCollection AddProjectsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProjectsServiceExtension).Assembly));
        services.AddValidatorsFromAssembly(typeof(ProjectsServiceExtension).Assembly);
        return services;
    }
}
