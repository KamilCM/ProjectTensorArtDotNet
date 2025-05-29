using System.Net;

namespace DiscordChannelReader.Utils;

public class AudioFileSaver
{
    private readonly string _rootPath;

    public AudioFileSaver(string rootPath)
    {
        _rootPath = rootPath;
    }

    public async Task SaveAudioAsync(string guildName, string channelName, string fileName, string url)
    {
        string safeGuild = MakeSafe(guildName);
        string safeChannel = MakeSafe(channelName);
        string safeFileName = MakeSafe(fileName);

        string guildDir = Path.Combine(_rootPath, safeGuild);
        string channelDir = Path.Combine(guildDir, safeChannel);
        Directory.CreateDirectory(channelDir);

        string targetPath = Path.Combine(channelDir, safeFileName);
        if (File.Exists(targetPath))
        {
            Console.WriteLine($"Already exists: {targetPath}");
            return;
        }

        try
        {
            using var httpClient = new HttpClient();
            var data = await httpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(targetPath, data);
            Console.WriteLine($"Saved: {targetPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save {safeFileName}: {ex.Message}");
        }
    }

    private string MakeSafe(string input)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            input = input.Replace(c, '_');
        return input;
    }
}
