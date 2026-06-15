using MATTAR.LocalAi.Abstractions;

namespace MATTAR.LocalAi.Tests
{
    public class FoundryLocalChatAgentTests
    {
        private IChatSettings _settings = null!;
        private FoundryLocalChatAgent _sut = null!;

        [SetUp]
        public void Setup()
        {
            _settings = new ChatSettings
            {
                SystemPrompt = "You are an AI assistant."
            };
            _sut = new FoundryLocalChatAgent(_settings);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _sut.DisposeAsync();
        }

        [Test]
        public void Constructor_WithValidSettings_DoesNotThrow()
        {
            // Construction must not require Foundry Local to be running
            Assert.That(_sut, Is.Not.Null);
        }

        [Test]
        public async Task Run_WithNullAction_ReturnsImmediatelyWithoutInitializingFoundry()
        {
            // action == null triggers the early-return path; no Foundry Local connection needed
            await _sut.Run(
                "Hello",
                action: null,
                cancellationToken: CancellationToken.None);

            Assert.Pass();
        }
    }
}
