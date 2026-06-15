using MATTAR.LocalAi.Abstractions;

namespace MATTAR.LocalAi.Tests
{
    public class FoundryLocalChatAgentTests
    {
        private IChatSettings _chatSettings = null!;
        private FoundryLocalChatAgent _chat = null!;

        [SetUp]
        public void Setup()
        {
            _chatSettings = new ChatSettings
            {
                SystemPrompt = "You are an AI assistant."
            };
            _chat = new FoundryLocalChatAgent(_chatSettings);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _chat.DisposeAsync();
        }

        [Test]
        public void Constructor_WithValidSettings_DoesNotThrow()
        {
            // Construction must not require Foundry Local to be running
            Assert.That(_chat, Is.Not.Null);
        }

        [Test]
        public async Task Run_WithNullAction_ReturnsImmediatelyWithoutInitializingFoundry()
        {
            // action == null triggers the early-return path; no Foundry Local connection needed
            await _chat.Run(
                "Hello",
                action: null,
                cancellationToken: CancellationToken.None);

            Assert.Pass();
        }
    }
}
