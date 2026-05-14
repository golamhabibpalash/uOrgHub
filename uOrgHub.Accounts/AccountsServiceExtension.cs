using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Accounts.Services;

namespace uOrgHub.Accounts;

public static class AccountsServiceExtension
{
    public static IServiceCollection AddAccountsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AccountsServiceExtension).Assembly));
        services.AddValidatorsFromAssembly(typeof(AccountsServiceExtension).Assembly);

        services.AddScoped<IAccountGroupRepository, AccountGroupRepository>();
        services.AddScoped<IAccountGroupService, AccountGroupService>();

        services.AddScoped<IChartOfAccountRepository, ChartOfAccountRepository>();
        services.AddScoped<IChartOfAccountService, ChartOfAccountService>();

        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IJournalEntryService, JournalEntryService>();

        services.AddScoped<IFiscalYearRepository, FiscalYearRepository>();
        services.AddScoped<IFiscalYearService, FiscalYearService>();

        return services;
    }
}
