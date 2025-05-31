using DiscordChannelReader;
using DiscordChannelReader.Config;
using DiscordChannelReader.Utils;
using System.Net.Http.Headers;

namespace TensorArtPlayWrightDotNet
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests : PageTest
    {
        private const string DownloadRoot = "TestDownloads";

        [OneTimeSetUp]
        public void SetupDirectory()
        {
            if (Directory.Exists(DownloadRoot))
                Directory.Delete(DownloadRoot, true);

            Directory.CreateDirectory(DownloadRoot);
        }

        public async Task HomepageHasPlaywrightInTitleAndGetStartedLinkLinkingToTheIntroPage()
        {
            // Arrange
            var url = "https://playwright.dev";
            var expectedTitlePattern = new Regex("Playwright");
            var expectedHref = "/docs/intro";
            var expectedUrlPattern = new Regex(".*intro");

            // Act
            await Page.GotoAsync(url);
            var getStarted = Page.Locator("text=Get Started");
            await getStarted.ClickAsync();

            // Assert
            await Expect(Page).ToHaveTitleAsync(expectedTitlePattern);
            await Expect(getStarted).ToHaveAttributeAsync("href", expectedHref);
            await Expect(Page).ToHaveURLAsync(expectedUrlPattern);
        }


        [Test]
        public async Task DiscordClient_ShouldLoadChannels()
        {
            // Arrange
            var testDiscordChannelReader = new DiscordReader();

            // Act
            var collectedChannels = await testDiscordChannelReader.ListAllChannelsAsync();

            // Assert
            Assert.IsNotNull(collectedChannels, "The returned channel list should not be null.");
            Assert.IsNotEmpty(collectedChannels, "At least one channel should be loaded from the Discord client.");
        }

        [Test]
        public async Task DiscordClient_ShouldLoadVoiceRecordings()
        {
            // Arrange
            var testDiscordChannelReader = new DiscordReader();

            // Act
            var collectedAudioAttachments = await testDiscordChannelReader.ListRecentAudioAttachmentsAsync();

            // Assert
            Assert.IsNotNull(collectedAudioAttachments, "The returned channel list should not be null.");
            Assert.IsNotEmpty(collectedAudioAttachments, "At least one channel should be loaded from the Discord client.");
        }

        [Test]
        public async Task DiscordClient_ShouldDownloadVoiceRecordingsToDisk()
        {
            // Arrange
            var testDiscordChannelReader = new DiscordReader();

            // Act
            await testDiscordChannelReader.DownloadRecentAudioAttachmentsAsync(DownloadRoot);

            // Assert
            var allFiles = Directory.GetFiles(DownloadRoot, "*.*", SearchOption.AllDirectories)
                                    .Where(f => f.EndsWith(".mp3") || f.EndsWith(".ogg") || f.EndsWith(".wav") || f.EndsWith(".m4a") || f.EndsWith(".flac"))
                                    .ToList();

            Assert.IsNotEmpty(allFiles, "No audio files were downloaded to the disk.");
        }

        [Test]
        public async Task OpenAIManipulator_ShouldReturnFlagColors()
        {
            // Arrange
            var manipulator = new OpenAIManipulator();
            var prompt = "Wymieñ kolory polskiej flagi narodowej.";

            // Act
            var result = await manipulator.Ask(prompt);

            // Assert
            Assert.IsNotNull(result, "Response from OpenAI should not be null.");
            Console.WriteLine(result);
            StringAssert.Contains("bia³y", result.ToLower(), "Expected result to contain 'bia³y'");
            StringAssert.Contains("czerwony", result.ToLower(), "Expected result to contain 'czerwony'");
        }

        [Test]
        public async Task OpenAIManipulator_ShouldListAvailableModels()
        {
            // Arrange
            var config = DiscordConfigMayh.Load("discordconfig.json");
            var apiKey = TokenObfuscator.Decrypt(config.EncryptedOpenAiKey);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // Act
            var response = await client.GetAsync("https://api.openai.com/v1/models");
            var responseText = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status code: {response.StatusCode}");
            Console.WriteLine("Raw response:");
            Console.WriteLine(responseText);

            // Assert (informacyjne)
            Assert.IsTrue(response.IsSuccessStatusCode, "Token might be invalid or restricted to a different project.");
        }


    }
}
