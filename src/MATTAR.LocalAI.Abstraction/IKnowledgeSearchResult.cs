using System.Reflection.Metadata;

namespace MATTAR.LocalAi.Abstractions;

public interface IKnowledgeSearchResult
{
    IDocument? Document { get; set; }
    string KnowledgeBaseName { get; set; }
    double? Score { get; set; }
}
