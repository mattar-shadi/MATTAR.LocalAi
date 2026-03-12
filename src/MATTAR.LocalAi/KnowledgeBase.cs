using MATTAR.LocalAi.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace MATTAR.LocalAi;

public class KnowledgeBase(
    VectorStore vectorStore,
    IEmbeddingGenerator<string,
        Embedding<float>> embeddingGenerator) : IKnowledgeBase
{
    private readonly VectorStore _vectorStore = vectorStore;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;

    public async Task CreateKnowledgeBase(
        string name,
        IEnumerable<IDocument> documents)
    {
        VectorStoreCollection<ulong, IDocument> existingCollection = _vectorStore.GetCollection<ulong, IDocument>(name);
        
        await existingCollection.EnsureCollectionExistsAsync();
        await UpdateOrInsertAsync(existingCollection, documents);
    }

    public static async Task UpdateOrInsertAsync(
        VectorStoreCollection<ulong, IDocument> collection,
        IEnumerable<IDocument> documents)
    {
        // Create a record and update or insert with the already generated embedding.
        foreach (var document in documents)
        {
            await collection.UpsertAsync(document);
        }
    }

    public async Task<List<IKnowledgeSearchResult>> Search(
        string query,
        string knowledgeBaseName,
        CancellationToken cancellationToken = default)
    {
        // Generate a vector for your search text, using your chosen embedding generation implementation.
        Embedding<float> searchVector = await _embeddingGenerator.GenerateAsync(value: query, cancellationToken: cancellationToken);

        VectorStoreCollection<ulong, Document> collection = _vectorStore.GetCollection<ulong, Document>(knowledgeBaseName);

        // Do the search, passing an options object with a Top value to limit result to the single top match.
        VectorSearchOptions<Document> searchOptions = new() { Skip = 0 };
        var searchResult = collection.SearchAsync(
            searchVector,
            top: 2,
            options: searchOptions,
            cancellationToken: cancellationToken);

        List<IKnowledgeSearchResult> search = [];
        await foreach (VectorSearchResult<Document> record in searchResult)
        {
            KnowledgeSearchResult result = new()
            {
                Document = (IDocument)record.Record, // Cast explicite vers IDocument
                KnowledgeBaseName = knowledgeBaseName,
                Score = record.Score
            };
            search.Add(result);
        }

        return search;
    }

    private async Task<List<Document>> GetDocuments()
    {
        List<Document> documents =
        [
            new Document
            {
                Id = 1,
                Name = "Hotel Happy",
                Content = "A luxury hotel with a pool.",
                Tags = ["luxury", "pool"]
            },
            new Document
            {
                Id = 2,
                Name = "Hotel Sad",
                Content = "A budget hotel with no pool.",
                Tags = ["budget", "no pool"]
            },
            new Document
            {
                Id = 4,
                Name = "Les noms de chat",
                Content = @"J'ai deux chats, l'un est noir et s'appelle COCA.
L'autre est roux et blanc, il s'appelle Nino.
Coca vomit souvent. Nino quant à lui est grincheux.",
                Tags = ["budget", "no pool"]
            },
            new Document
            {
                Id = 5,
                Name = "La situation physique de mes chats",
                Content = "Mes chats sont de différents poids. Coca pèse 3,7 kg alors que Nino pèse 6 kg.",
                Tags = ["chat"]
            },
        ];

        foreach (var d in documents)
        {
            d.Embedding = await _embeddingGenerator.GenerateAsync($"{d.Name} - {d.Content}");
        }

        return documents;
    }
}