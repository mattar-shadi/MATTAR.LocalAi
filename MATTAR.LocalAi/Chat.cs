using System.Diagnostics;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

namespace MATTAR.LocalAi;

#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001
public class Chat
{
    private readonly Kernel? _kernel;
    private readonly ChatHistory _history;
    private readonly IChatCompletionService? _chat;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;
    private readonly IVectorStore _vectorStore;
    private readonly IVectorStoreRecordCollection<Guid, Document> _collection;

    public Chat(ChatSettings settings)
    {
        string modelPath = Path.GetFullPath(settings.ModelPath);
        Debug.Print($"Model path: {modelPath}");

        string vectorStoreModelPath = Path.GetFullPath(settings.VectorStoreModelPath);
        Debug.Print($"Vector Store Model Path: {vectorStoreModelPath}");

        string vectorStoreVocabModelPath = Path.GetFullPath(settings.VectorStoreVocabModelPath);
        Debug.Print($"Vector Store Vocab Model Path: {vectorStoreVocabModelPath}");

        // Create Semantic Kernel
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOnnxRuntimeGenAIChatCompletion(modelPath: modelPath);  // set onnx runtime model
        kernelBuilder.AddInMemoryVectorStore();                                 // Add In Memory Vector Store 
        kernelBuilder.AddVectorStoreTextSearch<Document>();                     // Add Vector Store Text Search
        kernelBuilder.AddBertOnnxTextEmbeddingGeneration(
            onnxModelPath: vectorStoreModelPath,
            vocabPath: vectorStoreVocabModelPath
        );

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
        _history.AddSystemMessage(@"Tu est un assistant, tu répond toujours en français.");
        _history.AddUserMessage(userQ);
        var response = "";

        // Generate a vector for your search text, using your chosen embedding generation implementation.
        ReadOnlyMemory<float> searchVector = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(userQ);

        // Do the search, passing an options object with a Top value to limit resulst to the single top match.
        var searchResult = await _collection.VectorizedSearchAsync(searchVector, new() { Top = 1 });

        string search = "Result of the research :";
        // Inspect the returned hotel.
        await foreach (var record in searchResult.Results)
        {
            string research = $@"
File name: {record.Record.Name}
File content: {record.Record.Content}
File score: {record.Score}
Tags : {record.Record.Tags.Aggregate((x,y) => string.Concat(x, y))}
";
            search += research;
            Debug.Print(search);
        }
        _history.AddSystemMessage(search);

        var result = _chat?.GetStreamingChatMessageContentsAsync(
            chatHistory: _history,
            cancellationToken: cancellationToken);

        if (result is null)
            return;

        await foreach (var message in result)
        {
            action(message.ToString());
            response += message.Content;
        }
        _history.AddAssistantMessage(response);
    }
}