using Microsoft.Extensions.DependencyInjection;

namespace uOrgHub.API.Services.Storage;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddLocalFileStorage(
        this IServiceCollection services,
        Action<LocalFileStorageOptions>? configure = null)
    {
        var options = new LocalFileStorageOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        return services;
    }
}
