using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Auth.Repositories;

namespace uOrgHub.Auth.Services;

public class AccessLogBackgroundWorker : BackgroundService
{
    private const int MaxBatchSize = 256;

    private readonly IAccessLogQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccessLogBackgroundWorker> _logger;

    public AccessLogBackgroundWorker(
        IAccessLogQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<AccessLogBackgroundWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = _queue.Reader;
        var batch = new List<UserAccessLog>(MaxBatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await reader.WaitToReadAsync(stoppingToken))
                    return;

                batch.Clear();
                while (batch.Count < MaxBatchSize && reader.TryRead(out var item))
                    batch.Add(item);

                if (batch.Count == 0) continue;

                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IAccessLogRepository>();
                await repo.AddRangeAsync(batch, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist access log batch of {Count} entries", batch.Count);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}
