using MATTAR.LocalAi.OnnxProvider.Abstractions;
using Microsoft.ML.OnnxRuntime;

namespace MATTAR.LocalAi.OnnxProvider.Cpu;

public sealed class CpuProviderConfigurator : IOrtProviderConfigurator
{
    public string ProviderName => "CPU only";

    public bool IsAvailable() => true;

    public void Configure(SessionOptions options) =>
        options.AppendExecutionProvider_CPU();
}
