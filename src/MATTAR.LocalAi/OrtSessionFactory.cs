using MATTAR.LocalAi.OnnxProvider.Abstractions;
using MATTAR.LocalAi.OnnxProvider.Cpu;
using MATTAR.LocalAi.OnnxProvider.Cuda;
using MATTAR.LocalAi.OnnxProvider.DirectML;
using Microsoft.ML.OnnxRuntime;

namespace MATTAR.LocalAi;

public static class OrtSessionFactory
{
    public static IOrtProviderConfigurator SelectProvider()
    {
        if (OperatingSystem.IsWindows())
        {
            var directMl = new DirectMlProviderConfigurator();
            if (directMl.IsAvailable())
                return directMl;
        }

        var cuda = new CudaProviderConfigurator();
        if (cuda.IsAvailable())
            return cuda;

        return new CpuProviderConfigurator();
    }

    public static InferenceSession CreateSession(string modelPath)
    {
        var provider = SelectProvider();
        Console.WriteLine(provider.ProviderName);

        var options = new SessionOptions();
        provider.Configure(options);
        return new InferenceSession(modelPath, options);
    }
}
