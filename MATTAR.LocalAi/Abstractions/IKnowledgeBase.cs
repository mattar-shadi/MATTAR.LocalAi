using Microsoft.Extensions.VectorData;

namespace MATTAR.LocalAi.Abstractions
{
    public interface IKnowledgeBase
    {
        Task CreateKnowledgeBase(
            string name,
            IEnumerable<Document> documents);

        Task<List<KnowledgeSearchResult>> Search(
            string query,
            string knowledgeBase,
            CancellationToken cancellationToken = default);
    }
}
