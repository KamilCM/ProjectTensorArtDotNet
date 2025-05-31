using DiscordChannelReader.Config;
using DiscordChannelReader.Utils;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class OpenAIManipulator
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public OpenAIManipulator()
    {
        var config = DiscordConfigMayh.Load("discordconfig.json");
        _apiKey = TokenObfuscator.Decrypt(config.EncryptedOpenAiKey);

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> Ask(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-4",
            temperature = 0, // ważne dla stabilności odpowiedzi
            messages = new object[]
            {
                new { role = "system", content = "Jesteś pomocnym asystentem AI." },
                new { role = "user", content = prompt }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);

        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString()!;
    }
}
