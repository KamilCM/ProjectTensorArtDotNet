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

    private bool IsAudioAttachment(Attachment attachment)
    {
        if(attachment == null)
            return false;
        var audioExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".flac" };
        var isAudio = audioExtensions.Any(ext => attachment.Filename.EndsWith(ext, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(attachment.ContentType))
            isAudio |= attachment.ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

        return isAudio;
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

    public async Task<List<(string ChannelName, string FileName, string Url, string ContentType)>> ListRecentAudioAttachmentsAsync(string? channelNameFilter = null)
    {
        var attachments = new List<(string ChannelName, string FileName, string Url, string ContentType)>();
        var cutoff = DateTimeOffset.UtcNow.AddDays(-14);

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
        await Task.Delay(3000); // Wait for ready

        foreach (var guild in _client.Guilds)
        {
            foreach (var channel in guild.TextChannels)
            {
                if (!string.IsNullOrWhiteSpace(channelNameFilter) &&
                    !channel.Name.Equals(channelNameFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var messages = await channel.GetMessagesAsync(100).FlattenAsync();

                    foreach (var msg in messages)
                    {
                        if (msg.Timestamp < cutoff) continue;

                        foreach (var attachment in msg.Attachments)
                        {
                            if (IsAudioAttachment(attachment as Attachment))
                            {
                                attachments.Add((channel.Name, attachment.Filename, attachment.Url, attachment.ContentType ?? "unknown"));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing channel {channel.Name}: {ex.Message}");
                }
            }
        }

        await _client.StopAsync();
        return attachments;
    }

    public async Task DownloadRecentAudioAttachmentsAsync(string targetRootDir, string? channelNameFilter = null)
    {
        var saver = new AudioFileSaver(targetRootDir);
        // extract to config in future
        var cutoff = DateTimeOffset.UtcNow.AddDays(-14);

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
        await Task.Delay(3000); // wait for ready

        foreach (var guild in _client.Guilds)
        {
            foreach (var channel in guild.TextChannels)
            {
                if (!string.IsNullOrWhiteSpace(channelNameFilter) &&
                    !channel.Name.Equals(channelNameFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var messages = await channel.GetMessagesAsync(100).FlattenAsync();
                    foreach (var msg in messages)
                    {
                        if (msg.Timestamp < cutoff) continue;

                        foreach (var attachment in msg.Attachments)
                        {
                            if (IsAudioAttachment(attachment as Attachment))
                            {
                                await saver.SaveAudioAsync(
                                    guild.Name,
                                    channel.Name,
                                    $"{msg.Id}_{attachment.Filename}", // Add message ID prefix
                                    attachment.Url
                                );

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in {channel.Name}: {ex.Message}");
                }
            }
        }

        await _client.StopAsync();
    }



}
