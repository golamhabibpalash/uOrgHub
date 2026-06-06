using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Shared.Services;
using uOrgHub.Settings.Repositories;
using uOrgHub.Settings.Services;

namespace uOrgHub.Settings;

public static class SettingsServiceExtension
{
    public static IServiceCollection AddSettingsModule(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(SettingsServiceExtension).Assembly);

        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IValidationRuleRepository, ValidationRuleRepository>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<IValidationRuleService, ValidationRuleService>();
        services.AddScoped<IValidationRuleEngine, ValidationRuleEngine>();

        return services;
    }
}
