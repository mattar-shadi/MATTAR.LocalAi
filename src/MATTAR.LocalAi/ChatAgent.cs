using MATTAR.LocalAi.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.Diagnostics;

namespace MATTAR.LocalAi
{
    public class ChatAgent : IChat
    {

        ChatClientAgent _agent;
        IChatSettings _settings;
        IKnowledgeBase _knowledgeBase;

        public ChatAgent(IChatSettings settings, IKnowledgeBase knowledgeBase = null)
        {
            string modelPath = $@"{AppContext.BaseDirectory}\models\cpu-int4-rtn-block-32-acc-level-4\";
            if (!Path.Exists(modelPath))
                throw new DirectoryNotFoundException(modelPath);
            Debug.Print($"Model path: {modelPath}");

            _settings = settings;
            _knowledgeBase = knowledgeBase;

            OnnxRuntimeGenAIChatClient chatClient = new(modelPath);
            _agent = chatClient.AsAIAgent();
        }

        public async Task Run(string userQ,
            string? knowledgeBaseName = null,
            Action<string>? action = null,
            CancellationToken cancellationToken = default)
        {
            if (action is null)
                return;

            await foreach (var message in _agent.RunStreamingAsync(userQ, cancellationToken: cancellationToken))
            {
                action(message.Text);
            }
        }
    }
}
