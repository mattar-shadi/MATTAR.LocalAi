using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using MATTAR.LocalAi.Abstractions;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MATTAR.LocalAi;

/// <summary>
/// <see cref="IChat"/> implementation backed by Microsoft Foundry Local.
/// </summary>
/// <remarks>
/// Foundry Local must be installed and the target model must have been downloaded
/// before calling <see cref="Run"/>. To download and start a model, run:
/// <code>foundry model run phi-4</code>
/// The default model ID is <c>phi-4</c>, a compact multilingual instruction model.
/// Pass a different <paramref name="modelId"/> to use another model available in the
/// local Foundry catalog.
/// </remarks>
public class FoundryLocalChatAgent : IChat, IAsyncDisposable
{
    private readonly IChatSettings _settings;
    private readonly IKnowledgeBase? _knowledgeBase;
    private readonly string _modelId;
    private readonly ILogger _logger;

    private FoundryLocalManager? _manager;
    // OpenAIChatClient is Microsoft.AI.Foundry.Local.OpenAIChatClient (not the OpenAI SDK client)
    private OpenAIChatClient? _chatClient;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    /// <param name="settings">Chat settings, including the system prompt.</param>
    /// <param name="knowledgeBase">Optional knowledge base (reserved for future RAG support).</param>
    /// <param name="modelId">
    /// Foundry Local model alias to use.
    /// Default: <c>phi-4</c>.
    /// </param>
    /// <param name="logger">Optional logger; defaults to a no-op logger.</param>
    public FoundryLocalChatAgent(
        IChatSettings settings,
        IKnowledgeBase? knowledgeBase = null,
        string modelId = "phi-4",
        ILogger? logger = null)
    {
        _settings = settings;
        _knowledgeBase = knowledgeBase;
        _modelId = modelId;
        _logger = logger ?? NullLogger.Instance;
    }

    private async Task<OpenAIChatClient> GetOrCreateChatClientAsync(CancellationToken cancellationToken)
    {
        if (_chatClient is not null)
            return _chatClient;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_chatClient is not null)
                return _chatClient;

            var config = new Configuration { AppName = "MATTAR.LocalAi" };
            // CreateAsync initialises the FoundryLocalManager singleton; the instance is
            // then available via the static FoundryLocalManager.Instance property.
            await FoundryLocalManager.CreateAsync(config, _logger, cancellationToken);
            _manager = FoundryLocalManager.Instance;

            var catalog = await _manager.GetCatalogAsync(cancellationToken);
            var model = await catalog.GetModelAsync(_modelId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Foundry Local model '{_modelId}' was not found in the catalog. " +
                    $"Ensure Foundry Local is installed and the model is available by running: foundry model run {_modelId}");

            _chatClient = await model.GetChatClientAsync(cancellationToken);
            return _chatClient;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task Run(
        string userQ,
        string? knowledgeBaseName = null,
        Action<string>? action = null,
        CancellationToken cancellationToken = default)
    {
        if (action is null)
            return;

        var chatClient = await GetOrCreateChatClientAsync(cancellationToken);

        var messages = new List<ChatMessage>
        {
            ChatMessage.FromSystem(_settings.SystemPrompt),
            ChatMessage.FromUser(userQ)
        };

        await foreach (var response in chatClient.CompleteChatStreamingAsync(messages, cancellationToken))
        {
            var text = response.Choices?.FirstOrDefault()?.Delta?.Content;
            if (!string.IsNullOrEmpty(text))
                action(text);
        }
    }

    public ValueTask DisposeAsync()
    {
        _initLock.Dispose();
        _manager?.Dispose();
        return ValueTask.CompletedTask;
    }
}
