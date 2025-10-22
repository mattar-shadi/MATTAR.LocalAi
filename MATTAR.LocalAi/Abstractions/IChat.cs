namespace MATTAR.LocalAi.Abstractions
{
    public interface IChat
    {
        public Task Run(
            string userQ,
            string? knowledgeBaseName = null,
            Action<string>? action = null,
            CancellationToken cancellationToken = default);
    }
}