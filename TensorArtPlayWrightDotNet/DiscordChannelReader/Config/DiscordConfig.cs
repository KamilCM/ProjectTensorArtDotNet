using System.Text.Json;

namespace DiscordChannelReader.Config;

public class DiscordConfigMayh
{
    public string EncryptedToken { get; set; } = "";

    public static DiscordConfigMayh Load(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<DiscordConfigMayh>(json)!;
    }
}
