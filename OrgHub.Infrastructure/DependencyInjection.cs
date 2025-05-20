using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrgHub.Application.Auth.Services;
using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Common.Services;
using OrgHub.Application.Features.FixedAssets.Equipments.Interfaces;
using OrgHub.Application.Features.FixedAssets.Equipments.Services;
using OrgHub.Application.Features.HRM.Employees.Interfaces;
using OrgHub.Application.Features.HRM.Employees.Services;
using OrgHub.Application.Features.Identity.Interfaces;
using OrgHub.Application.Features.Identity.Services;
using OrgHub.Core.Interfaces;
using OrgHub.Core.Interfaces.Identity;
using OrgHub.Domain.Entities.Identity;
using OrgHub.Infrastructure.Persistence;
using OrgHub.Infrastructure.Repositories;
using OrgHub.Infrastructure.Repositories.Identity;

namespace OrgHub.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register Identity
            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

            // Register Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();

            // Register Services
            services.AddScoped(typeof(IService<,>), typeof(Service<,>));
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEquipmentService, EquipmentService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJWTServices, JwtService>();
            services.AddScoped<IUserPermissionService, UserPermissionService>();
            services.AddScoped<IRolePermissionService, RolePermissionService>();

            return services;
        }
    }
}
