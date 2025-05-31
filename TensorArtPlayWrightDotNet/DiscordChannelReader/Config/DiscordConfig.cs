using DiscordChannelReader.Utils;
using System.Text.Json;

namespace DiscordChannelReader.Config;

public class DiscordConfigMayh
{
    public string EncryptedToken { get; set; } = "";
    public string TargetRootPath { get; set; } = "";
    public string EncryptedOpenAiKey { get; set; } = "";
    public string OpenAiKey => TokenObfuscator.Decrypt(EncryptedOpenAiKey);

    public static DiscordConfigMayh Load(string path)
    {
        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<DiscordConfigMayh>(json);
        if (config == null)
            throw new InvalidOperationException("Failed to load Discord config.");
        return config;
    }
}
