using System.Threading.Channels;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Services;

public class AccessLogQueue : IAccessLogQueue
{
    private readonly Channel<UserAccessLog> _channel;

    public AccessLogQueue()
    {
        _channel = Channel.CreateBounded<UserAccessLog>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });
    }

    public bool TryEnqueue(UserAccessLog log) => _channel.Writer.TryWrite(log);

    public ChannelReader<UserAccessLog> Reader => _channel.Reader;
}
