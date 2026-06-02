using System.Threading.Channels;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Services;

public interface IAccessLogQueue
{
    bool TryEnqueue(UserAccessLog log);
    ChannelReader<UserAccessLog> Reader { get; }
}
