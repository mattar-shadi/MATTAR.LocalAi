using System.Diagnostics;
using MATTAR.LocalAi.Functions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

namespace MATTAR.LocalAi;

#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001
public class Chat : IChat
{
    private readonly Kernel? _kernel;
    private readonly ChatHistory _history;
    private readonly IChatCompletionService? _chat;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;
    private readonly IVectorStore _vectorStore;
    private readonly IVectorStoreRecordCollection<Guid, Document> _collection;

    public Chat(ChatSettings settings)
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

        // Create Semantic Kernel
        var kernelBuilder = Kernel.CreateBuilder();
        // set onnx runtime model
        kernelBuilder.AddOnnxRuntimeGenAIChatCompletion(modelId: "phi4", modelPath: modelPath);
        // Add In Memory Vector Store 
        kernelBuilder.AddInMemoryVectorStore();
        // Add Vector Store Text Search
        kernelBuilder.AddVectorStoreTextSearch<Document>();                    
        kernelBuilder.AddBertOnnxTextEmbeddingGeneration(
            onnxModelPath: vectorStoreModelPath,
            vocabPath: vectorStoreVocabModelPath
        );
        kernelBuilder.Plugins.AddFromType<TimeInformationPlugin>();

        _kernel = kernelBuilder.Build();

        // Create chat
        _chat = _kernel.GetRequiredService<IChatCompletionService>();

        // Get Vector Store
        _vectorStore = _kernel.GetRequiredService<IVectorStore>();

        // Get Text Embedding Generation Service
        _textEmbeddingGenerationService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // Get Vector Store Record Collection
        _collection = new Memory(_vectorStore, _textEmbeddingGenerationService).Collection;

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
        Action<string>? action = null,
        CancellationToken cancellationToken = default)
    {
        action ??= (s) => Console.Write(s);

        _history.AddUserMessage(userQ);

        // Generate a vector for your search text, using your chosen embedding generation implementation.
        ReadOnlyMemory<float> searchVector = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(userQ);

        // Do the search, passing an options object with a Top value to limit resulst to the single top match.
        VectorSearchOptions<Document> searchOptions = new() { Top = 2 };
        var searchResult = await _collection.VectorizedSearchAsync(searchVector, searchOptions, cancellationToken);

        List<string> search = [@"## Résultat de ma recherche :"];
        await foreach (VectorSearchResult<Document> record in searchResult.Results)
        {
            if (record.Score < 0.23)
                continue;

            search.Add($"\n - {record.Record.Content}");
        }

        if(search.Count > 1)
        {
            _history.AddUserMessage(string.Join("", search));
        }
        
        var result = _chat?.GetStreamingChatMessageContentsAsync(
            chatHistory: _history,
            executionSettings: new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                ModelId = "phi4",
                ExtensionData = new Dictionary<string, object>
                {
                    {"Current Date", DateTime.Now.ToString("R") }
                },
            },
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