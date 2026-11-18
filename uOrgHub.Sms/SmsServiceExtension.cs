using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Shared.Behaviors;
using uOrgHub.Shared.Services;
using uOrgHub.Sms.Providers;
using uOrgHub.Sms.Services;

namespace uOrgHub.Sms;

public static class SmsServiceExtension
{
    public static IServiceCollection AddSmsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(SmsServiceExtension).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(SmsServiceExtension).Assembly);

        // Providers: one line per vendor. Nothing else changes to add one.
        services.AddSingleton<ISmsProvider, LogOnlySmsProvider>();
        services.AddSingleton<ISmsProvider, BulkSmsBdProvider>();
        services.AddSingleton<ISmsProvider, AlphaSmsProvider>();
        services.AddSingleton<ISmsProvider, GenericHttpSmsProvider>();
        services.AddSingleton<ISmsProviderRegistry, SmsProviderRegistry>();

        // HttpClientFactory's default logging writes every request URL at Information level, and
        // some vendors take the API key in the query string — so this client logs nothing.
        services.AddHttpClient(SmsGatewayRuntime.HttpClientName, c => c.Timeout = Timeout.InfiniteTimeSpan)
            .RemoveAllLoggers();

        services.AddSingleton<ISmsCredentialProtector, SmsCredentialProtector>();
        services.AddScoped<ISmsGatewayRuntime, SmsGatewayRuntime>();
        services.AddScoped<SmsSendService>();
        services.AddScoped<ISmsSender>(sp => sp.GetRequiredService<SmsSendService>());
        services.AddScoped<ISmsDispatcher, SmsDispatcher>();
        services.AddSingleton<SmsDispatchSignal>();
        services.AddHostedService<SmsDispatchWorker>();
        return services;
    }
}
