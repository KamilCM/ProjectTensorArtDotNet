using System.Threading.Tasks;

namespace DiscordChannelReader;

public interface IChannelReader
{
    Task ReadLatestMessagesAsync(ulong channelId, int limit = 10);
}
