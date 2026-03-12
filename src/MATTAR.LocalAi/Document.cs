using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace MATTAR.LocalAi;

public class Document : IDocument
{
    [VectorStoreKey]
    public ulong Id { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public string Name { get; set; } = string.Empty;

    [VectorStoreData]
    public string FullPath { get; set; } = string.Empty;

    [VectorStoreData(IsIndexed = true)]
    public string Content { get; set; } = string.Empty;

    [VectorStoreVector(Dimensions: 384, DistanceFunction = DistanceFunction.CosineDistance, IndexKind = IndexKind.Hnsw)]
    public Embedding<float> Embedding { get; set; }

    public string[] Tags { get; set; } = [];

    public override string ToString()
    {
        return $"- {Id} : {Name} ({FullPath}) \n {Content}";
    }
}