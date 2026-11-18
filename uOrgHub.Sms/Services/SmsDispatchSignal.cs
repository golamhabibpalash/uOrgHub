using System.Threading.Channels;

namespace uOrgHub.Sms.Services;

/// <summary>Wakes the dispatch worker as soon as something is queued, instead of waiting for its next poll.</summary>
public sealed class SmsDispatchSignal
{
    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

    public void Notify() => _channel.Writer.TryWrite(true);

    /// <summary>Completes when notified or after <paramref name="timeout"/>, whichever is first.</summary>
    public async Task WaitAsync(TimeSpan timeout, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);
        try
        {
            await _channel.Reader.ReadAsync(cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // poll interval elapsed
        }
    }
}
