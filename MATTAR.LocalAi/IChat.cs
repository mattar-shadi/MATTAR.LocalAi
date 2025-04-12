namespace MATTAR.LocalAi
{
    public interface IChat
    {
        public Task Run(
            string userQ,
            Action<string>? action = null,
            CancellationToken cancellationToken = default);
    }
}