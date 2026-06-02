using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using uOrgHub.Auth.Repositories;

namespace uOrgHub.Auth.Services;

public class AccessLogRetentionWorker : BackgroundService
{
    private const int DefaultRetentionDays = 90;
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<AccessLogRetentionWorker> _logger;

    public AccessLogRetentionWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<AccessLogRetentionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(RunInterval);
        do
        {
            await PruneOnceAsync(stoppingToken);
        }
        while (await SafeWaitAsync(timer, stoppingToken));
    }

    private async Task PruneOnceAsync(CancellationToken ct)
    {
        try
        {
            var retentionDays = _config.GetValue<int?>("AccessLog:RetentionDays") ?? DefaultRetentionDays;
            if (retentionDays <= 0) return;

            var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IAccessLogRepository>();
            var deleted = await repo.DeleteOlderThanAsync(cutoff, ct);

            if (deleted > 0)
                _logger.LogInformation("Pruned {Count} access log entries older than {Cutoff:o}", deleted, cutoff);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Access log retention pruning failed");
        }
    }

    private static async Task<bool> SafeWaitAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try { return await timer.WaitForNextTickAsync(ct); }
        catch (OperationCanceledException) { return false; }
    }
}
