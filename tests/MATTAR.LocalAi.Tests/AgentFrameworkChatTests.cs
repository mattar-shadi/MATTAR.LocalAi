using MATTAR.LocalAi.Abstractions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace MATTAR.LocalAi.Tests
{
    public class AgentFrameworkChatTests
    {
        private IChatSettings _chatSettings;
        private ChatAgent _chat;
        private VectorStore _vectorStore;

        [SetUp]
        public void Setup()
        {
            _chatSettings = new ChatSettings
            {
                SystemPrompt = @"You are a AI assistant."
            };
            _vectorStore = new InMemoryVectorStore();
            _chat = new ChatAgent(_chatSettings);
        }

        [TearDown]
        public void TearDown()
        {
            _vectorStore?.Dispose();
        }

        [Test]
        public void SimpleTest()
        {
            _chat.Run(
                "Hello, how are you?",
                action: (response) =>
                {
                    TestContext.Out.Write(response);
                },
                cancellationToken: CancellationToken.None).Wait();
            Assert.Pass();
        }
    }
}
