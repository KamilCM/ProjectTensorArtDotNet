using DiscordChannelReader;
using DiscordChannelReader.Utils;

namespace TensorArtPlayWrightDotNet
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests : PageTest
    {
        [Test]
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
    }
}
