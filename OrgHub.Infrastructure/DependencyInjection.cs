using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrgHub.Application.Auth.Services;
using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Common.Services;
using OrgHub.Application.Features.FixedAssets.Equipments.Interfaces;
using OrgHub.Application.Features.FixedAssets.Equipments.Services;
using OrgHub.Application.Features.HRM.EmployeeAttendance.Interfaces;
using OrgHub.Application.Features.HRM.EmployeeAttendance.Services;
using OrgHub.Application.Features.HRM.Employees.Interfaces;
using OrgHub.Application.Features.HRM.Employees.Services;
using OrgHub.Application.Features.Identity.Interfaces;
using OrgHub.Application.Features.Identity.Services;
using OrgHub.Application.Features.Others.Interfaces;
using OrgHub.Application.Features.Others.Services;
using OrgHub.Core.Interfaces;
using OrgHub.Core.Interfaces.Identity;
using OrgHub.Domain.Entities.Identity;
using OrgHub.Infrastructure.Persistence;
using OrgHub.Infrastructure.Persistence.Others;
using OrgHub.Infrastructure.Repositories;
using OrgHub.Infrastructure.Repositories.FixedAssets;
using OrgHub.Infrastructure.Repositories.HRM;
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
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<AuditLoggingInterceptor>();

            // Register Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            
            #region OTHERS Repositories
            #endregion OTHERS Repositories

            #region IDENTITY Repositories
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            #endregion IDENTITY Repositories

            #region HRM Repositories
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            #endregion HRM Repositories

            #region FIXED ASSETS Repositories

            #endregion FIXED ASSETS Repositories


            // Register Services
            services.AddScoped(typeof(IService<,>), typeof(Service<,>));

            #region OTHERS Services

            #endregion OTHERS Services

            #region IDENTITY Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJWTServices, JwtService>();
            services.AddScoped<IUserPermissionService, UserPermissionService>();
            services.AddScoped<IRolePermissionService, RolePermissionService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            #endregion IDENTITY Services

            #region HRM Services
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEquipmentService, EquipmentService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            #endregion HRM Services

            #region FIXED ASSETS Services

            #endregion FIXED ASSETS Services


            return services;
        }
    }
}
