using Microsoft.ML.OnnxRuntime;

namespace MATTAR.LocalAi.OnnxProvider.Abstractions;

public interface IOrtProviderConfigurator
{
    string ProviderName { get; }
    bool IsAvailable();
    void Configure(SessionOptions options);
}
