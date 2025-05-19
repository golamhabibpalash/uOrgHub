using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Common.Services;
using OrgHub.Application.Features.FixedAssets.Equipments.Interfaces;
using OrgHub.Application.Features.FixedAssets.Equipments.Services;
using OrgHub.Application.Features.HRM.Employees.Interfaces;
using OrgHub.Application.Features.HRM.Employees.Services;
using OrgHub.Core.Interfaces; // For IEmployeeRepository
using OrgHub.Infrastructure.Persistence;
using OrgHub.Infrastructure.Repositories;

namespace OrgHub.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

            // Register Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();

            // Register Services
            services.AddScoped(typeof(IService<,>), typeof(Service<,>));
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEquipmentService, EquipmentService>();

            return services;
        }
    }
}
