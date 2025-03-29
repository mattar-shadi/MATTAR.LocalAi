using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001
public class Memory
{
    private Kernel _kernel;
    private readonly IVectorStore _vectorStore;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;

    public IVectorStoreRecordCollection<Guid, Document> Collection { get; }

    public Memory(IVectorStore vectorStore, ITextEmbeddingGenerationService textEmbeddingGenerationService)
    {
        _vectorStore = vectorStore;
        _textEmbeddingGenerationService = textEmbeddingGenerationService;

        var definition = new VectorStoreRecordDefinition
        {
            Properties =
            [
                new VectorStoreRecordKeyProperty(nameof(Document.Id), typeof(Guid)),
                new VectorStoreRecordDataProperty(nameof(Document.Name), typeof(string))
                    {
                        IsFilterable = true
                    },
                new VectorStoreRecordDataProperty(nameof(Document.Content), typeof(string))
                    {
                        IsFullTextSearchable = true
                    },
                new VectorStoreRecordVectorProperty(nameof(Document.ContentEmbedding), typeof(ReadOnlyMemory<float>)) 
                    {
                        Dimensions = 4,
                        DistanceFunction = DistanceFunction.CosineDistance,
                        IndexKind = IndexKind.Hnsw
                    },
            ]
        };

        Collection = _vectorStore.GetCollection<Guid, Document>("documents", definition);
        Collection.CreateCollectionIfNotExistsAsync().Wait();

        List<Document> documents = GetDocuments().Result;
        UpsertAsync(Collection, documents).Wait();
    }

    public async Task UpsertAsync(IVectorStoreRecordCollection<Guid, Document> collection, List<Document> documents)
    {
        // Create a record and upsert with the already generated embedding.
        foreach (var document in documents)
        {
            await collection.UpsertAsync(document);
        }
    }

    private async Task<List<Document>> GetDocuments()
    {
        List<Document> documents =
        [
            new Document
            {
                Id = Guid.NewGuid(),
                Name = "Hotel Happy",
                Content = "A luxury hotel with a pool.",
                ContentEmbedding = null,
                Tags = new[] { "luxury", "pool" }
            },
            new Document
            {
                Id = Guid.NewGuid(),
                Name = "Hotel Sad",
                Content = "A budget hotel with no pool.",
                ContentEmbedding = null,
                Tags = new[] { "budget", "no pool" }
            },
        ];

        foreach (var d in documents)
        {
            d.ContentEmbedding = await _textEmbeddingGenerationService
                .GenerateEmbeddingAsync(d.Content);
        }

        return documents;
    }
}