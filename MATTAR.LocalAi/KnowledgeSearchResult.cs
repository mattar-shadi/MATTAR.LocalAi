namespace MATTAR.LocalAi;

public class KnowledgeSearchResult
{
    public Document? Document { get; set; }
    public string KnowledgeBaseName { get; set; } = string.Empty;
    public double? Score { get; set; }
}