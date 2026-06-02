using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Auth.Repositories;
using uOrgHub.Auth.Services;

namespace uOrgHub.Auth;

public static class AuthServiceExtension
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IAccessLogRepository, AccessLogRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IAccessLogService, AccessLogService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();

        // Access log async pipeline
        services.AddSingleton<IAccessLogQueue, AccessLogQueue>();
        services.AddHostedService<AccessLogBackgroundWorker>();
        services.AddHostedService<AccessLogRetentionWorker>();

        services.AddValidatorsFromAssembly(typeof(AuthServiceExtension).Assembly);

        return services;
    }
}
