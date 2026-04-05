using MATTAR.LocalAi.OnnxProvider.Abstractions;
using Microsoft.ML.OnnxRuntime;

namespace MATTAR.LocalAi.OnnxProvider.Cuda;

public sealed class CudaProviderConfigurator : IOrtProviderConfigurator
{
    public string ProviderName => "CUDA GPU activé";

    public bool IsAvailable()
    {
        try
        {
            using var opts = new SessionOptions();
            opts.AppendExecutionProvider_CUDA(0);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Configure(SessionOptions options) =>
        options.AppendExecutionProvider_CUDA(0);
}
