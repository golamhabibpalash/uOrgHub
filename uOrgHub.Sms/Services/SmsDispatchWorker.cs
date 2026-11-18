using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace uOrgHub.Sms.Services;

/// <summary>
/// Background loop driving <see cref="ISmsDispatcher"/>: sends queued messages as soon as the
/// send service signals (or every 15 s), and polls delivery reports once a minute.
///
/// Assumes one API process per database, which is how every deployed instance runs. A second
/// process would be safe for sending (each message is claimed before its call) but could poll
/// the same delivery report twice.
/// </summary>
public sealed class SmsDispatchWorker(
    IServiceScopeFactory scopes, SmsDispatchSignal signal, ILogger<SmsDispatchWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan DeliveryInterval = TimeSpan.FromMinutes(1);
    private const int BatchSize = 25;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let migrations finish before touching SMS tables.
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        await RunAsync(d => d.RecoverInterruptedAsync(stoppingToken), "recover interrupted SMS", stoppingToken);
        var nextDeliveryCheck = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            // Drain the queue in batches before going back to sleep.
            int sent;
            do
            {
                sent = await RunAsync(d => d.ProcessQueuedAsync(BatchSize, stoppingToken), "send queued SMS", stoppingToken);
            } while (sent == BatchSize && !stoppingToken.IsCancellationRequested);

            if (DateTime.UtcNow >= nextDeliveryCheck)
            {
                await RunAsync(d => d.CheckDeliveryAsync(BatchSize, stoppingToken), "check SMS delivery reports", stoppingToken);
                nextDeliveryCheck = DateTime.UtcNow + DeliveryInterval;
            }

            await signal.WaitAsync(PollInterval, stoppingToken);
        }
    }

    private async Task<int> RunAsync(Func<ISmsDispatcher, Task<int>> work, string what, CancellationToken ct)
    {
        try
        {
            using var scope = scopes.CreateScope();
            return await work(scope.ServiceProvider.GetRequiredService<ISmsDispatcher>());
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return 0;
        }
        catch (Exception ex)
        {
            // e.g. database unavailable or tables not migrated yet — try again next round.
            logger.LogError(ex, "Failed to {What}", what);
            return 0;
        }
    }
}
