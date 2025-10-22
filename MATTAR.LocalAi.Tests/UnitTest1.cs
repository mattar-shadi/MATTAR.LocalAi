using MATTAR.LocalAi.Abstractions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace MATTAR.LocalAi.Tests
{
    public class Tests
    {
        private IChatSettings _chatSettings;
        private IChat _chat;
        private VectorStore _vectorStore;

        [SetUp]
        public void Setup()
        {
            _chatSettings = new ChatSettings
            {
                SystemPrompt = @"You are a AI assistant."
            };
            _vectorStore = new InMemoryVectorStore();
            _chat = new Chat(_chatSettings);
        }

        [TearDown]
        public void TearDown()
        {
            _vectorStore?.Dispose();
        }

        [Test]
        public void Test1()
        {
            _chat.Run(
                "Hello, how are you?",
                action: (response) =>
                {
                    Console.Write(response);
                },
                cancellationToken: CancellationToken.None).Wait();
            Assert.Pass();
        }
    }
}
