using System.Runtime.Versioning;
using MATTAR.LocalAi.OnnxProvider.Abstractions;
using Microsoft.ML.OnnxRuntime;

namespace MATTAR.LocalAi.OnnxProvider.DirectML;

[SupportedOSPlatform("windows")]
public sealed class DirectMlProviderConfigurator : IOrtProviderConfigurator
{
    public string ProviderName => "DirectML GPU activé";

    public bool IsAvailable()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            using var opts = new SessionOptions();
            opts.AppendExecutionProvider_DML(0);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [SupportedOSPlatform("windows")]
    public void Configure(SessionOptions options) =>
        options.AppendExecutionProvider_DML(0);
}
