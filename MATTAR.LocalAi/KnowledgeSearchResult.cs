using MATTAR.LocalAi.Abstractions;

namespace MATTAR.LocalAi;

public class KnowledgeSearchResult : IKnowledgeSearchResult
{
    public IDocument? Document { get; set; }
    public string KnowledgeBaseName { get; set; } = string.Empty;
    public double? Score { get; set; }
}