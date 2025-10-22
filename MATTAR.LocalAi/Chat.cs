using MATTAR.LocalAi.Abstractions;
using MATTAR.LocalAi.Functions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace MATTAR.LocalAi;

#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001
public class Chat : IChat
{
    private readonly Kernel? _kernel;
    private readonly ChatHistory _history;
    private readonly IChatCompletionService? _chat;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _textEmbeddingGenerationService;
    private readonly IKnowledgeBase _knowledgeBase;

    public Chat(IChatSettings settings, IKnowledgeBase knowledgeBase = null)
    {
        string modelPath = $@"{AppContext.BaseDirectory}\models\cpu-int4-rtn-block-32-acc-level-4\";
        if (!Path.Exists(modelPath))
            throw new DirectoryNotFoundException(modelPath);
        Debug.Print($"Model path: {modelPath}");

        string vectorStoreModelPath = $@"{AppContext.BaseDirectory}\models\bge-micro-v2\onnx\model.onnx";
        if (!Path.Exists(vectorStoreModelPath))
            throw new FileNotFoundException(vectorStoreModelPath);
        Debug.Print($"Vector Store Model Path: {vectorStoreModelPath}");

        string vectorStoreVocabModelPath = $@"{AppContext.BaseDirectory}\models\bge-micro-v2\vocab.txt";
        if (!Path.Exists(vectorStoreVocabModelPath))
            throw new FileNotFoundException(vectorStoreVocabModelPath);
        Debug.Print($"Vector Store Vocab Model Path: {vectorStoreVocabModelPath}");

        // Créer Semantic Kernel
        var kernelBuilder = Kernel.CreateBuilder();
        // set onnx runtime model
        kernelBuilder.AddOnnxRuntimeGenAIChatCompletion(modelId: "phi4", modelPath: modelPath);

        // Add Vector Store Text Search
        kernelBuilder.AddVectorStoreTextSearch<Document>();
        kernelBuilder.AddBertOnnxEmbeddingGenerator(
            onnxModelPath: vectorStoreModelPath,
            vocabPath: vectorStoreVocabModelPath
        );
        //kernelBuilder.AddBertOnnxTextEmbeddingGeneration(
        //    onnxModelPath: vectorStoreModelPath,
        //    vocabPath: vectorStoreVocabModelPath
        //);

        kernelBuilder.Plugins.AddFromType<TimeInformationPlugin>("GetCurrentUtcTime");

        _kernel = kernelBuilder.Build();

        // Create chat
        _chat = _kernel.GetRequiredService<IChatCompletionService>();

        // Get Text Embedding Generation Service
        _textEmbeddingGenerationService = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

        // Get Vector Store Record Collection
        _knowledgeBase = knowledgeBase;

        // Create a History
        _history = new ChatHistory();
        _history.AddSystemMessage(settings.SystemPrompt);
        _history.AddSystemMessage(@"
You are a helpful assistant with some tools.
<|tool|>
[
    {
        ""name"": ""GetCurrentUtcTime"",
        ""description"": ""Get current time and retrieves it in UTC."",
        ""parameters"": [],
        ""returns"": {
            ""name"": ""currentUTCDate"",
            ""type"": ""str""
        }
    }
]
<|/tool|>
");
    }

    /// <summary>
    /// Run the Chat
    /// </summary>
    /// <param name="userQ">User Question</param>
    /// <param name="action">How to proceed the response</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    public async Task Run(
        string userQ,
        string? knowledgeBaseName = null,
        Action<string>? action = null,
        CancellationToken cancellationToken = default)
    {
        action ??= (s) => Console.Write(s);

        _history.AddUserMessage(userQ);

        if(knowledgeBaseName is not null && _knowledgeBase is not null)
        {
            List<KnowledgeSearchResult> results = await _knowledgeBase.Search(userQ, knowledgeBaseName, cancellationToken);
            if (results.Count > 1)
            {
                string formattedAllResults = "I found informations in Knowledge Base: \n";
                formattedAllResults += string.Join("", results.Select(x => x.Document?.ToString()));
                _history.AddAssistantMessage(formattedAllResults);
                action(formattedAllResults);
            }
        }

        // Prompt Execution Settings
        // The modelId is the name of the model you want to use.
        // The extension data is a dictionary of key-value pairs that you can use to pass additional data to the model.
        // The function choice behavior is a setting that determines how the model should choose which function to call.
        PromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ModelId = "phi4",
        };

        var result = _chat?.GetStreamingChatMessageContentsAsync(
            chatHistory: _history,
            executionSettings: settings,
            kernel: _kernel,
            cancellationToken: cancellationToken);

        if (result is null)
            return;

        string response = string.Empty;

        await foreach (StreamingChatMessageContent message in result.WithCancellation(cancellationToken))
        {
            action(message.ToString());
            response += message.Content;
        }
        _history.AddAssistantMessage(response);
    }
}