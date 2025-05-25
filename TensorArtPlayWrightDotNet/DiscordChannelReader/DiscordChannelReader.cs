using Discord;
using Discord.WebSocket;
using DiscordChannelReader.Config;
using DiscordChannelReader.Utils;

namespace DiscordChannelReader;

public class DiscordReader : IChannelReader
{
    private readonly DiscordSocketClient _client;
    private readonly string _token;

    public DiscordReader()
    {
        _client = new DiscordSocketClient();
        var config = DiscordConfigMayh.Load("discordconfig.json");
        _token = TokenObfuscator.Decrypt(config.EncryptedToken);
    }

    public async Task ReadLatestMessagesAsync(ulong channelId, int limit = 10)
    {
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();

        await Task.Delay(3000); // Wait for connection to be ready

        var channel = _client.GetChannel(channelId) as IMessageChannel;
        if (channel != null)
        {
            var messages = await channel.GetMessagesAsync(limit).FlattenAsync();
            foreach (var msg in messages)
            {
                Console.WriteLine($"[{msg.Timestamp}] {msg.Author}: {msg.Content}");
            }
        }
        else
        {
            Console.WriteLine("Channel not found or inaccessible.");
        }

        await _client.StopAsync();
    }

    public async Task<List<(string Type, string Name, ulong Id)>> ListAllChannelsAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
        await Task.Delay(3000); // wait for ready

        var result = new List<(string Type, string Name, ulong Id)>();

        foreach (var guild in _client.Guilds)
        {
            foreach (var channel in guild.Channels)
            {
                result.Add((channel.GetType().Name, channel.Name, channel.Id));
            }
        }

        await _client.StopAsync();
        return result;
    }

}
