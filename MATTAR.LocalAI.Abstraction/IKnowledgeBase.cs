namespace MATTAR.LocalAi.Abstractions;

public interface IKnowledgeBase
{
    Task CreateKnowledgeBase(
        string name,
        IEnumerable<IDocument> documents);

    Task<List<IKnowledgeSearchResult>> Search(
        string query,
        string knowledgeBase,
        CancellationToken cancellationToken = default);
}