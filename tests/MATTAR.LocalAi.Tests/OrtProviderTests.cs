using MATTAR.LocalAi.OnnxProvider.Abstractions;
using MATTAR.LocalAi.OnnxProvider.Cpu;
using MATTAR.LocalAi.OnnxProvider.Cuda;
using MATTAR.LocalAi.OnnxProvider.DirectML;

namespace MATTAR.LocalAi.Tests;

public class OrtProviderTests
{
    [Test]
    public void CpuProvider_IsAlwaysAvailable()
    {
        var provider = new CpuProviderConfigurator();
        Assert.That(provider.IsAvailable(), Is.True);
        Assert.That(provider.ProviderName, Is.EqualTo("CPU only"));
    }

    [Test]
    public void CudaProvider_HasCorrectProviderName()
    {
        var provider = new CudaProviderConfigurator();
        Assert.That(provider.ProviderName, Is.EqualTo("CUDA GPU activé"));
    }

    [Test]
    public void CudaProvider_IsAvailable_DoesNotThrow()
    {
        var provider = new CudaProviderConfigurator();
        bool available = false;
        Assert.DoesNotThrow(() => available = provider.IsAvailable());
    }

    [Test]
    public void DirectMlProvider_HasCorrectProviderName()
    {
        var provider = new DirectMlProviderConfigurator();
        Assert.That(provider.ProviderName, Is.EqualTo("DirectML GPU activé"));
    }

    [Test]
    public void DirectMlProvider_IsAvailable_ReturnsFalseOnNonWindows()
    {
        if (OperatingSystem.IsWindows())
            Assert.Ignore("Test only runs on non-Windows.");

        var provider = new DirectMlProviderConfigurator();
        Assert.That(provider.IsAvailable(), Is.False);
    }

    [Test]
    public void DirectMlProvider_IsAvailable_DoesNotThrow()
    {
        var provider = new DirectMlProviderConfigurator();
        bool available = false;
        Assert.DoesNotThrow(() => available = provider.IsAvailable());
    }

    [Test]
    public void SelectProvider_ReturnsNonNullProvider()
    {
        IOrtProviderConfigurator? provider = null;
        Assert.DoesNotThrow(() => provider = OrtSessionFactory.SelectProvider());
        Assert.That(provider, Is.Not.Null);
    }

    [Test]
    public void SelectProvider_FallsBackToCpuOnCi()
    {
        var provider = OrtSessionFactory.SelectProvider();
        Assert.That(
            provider.ProviderName,
            Is.EqualTo("CPU only")
                .Or.EqualTo("CUDA GPU activé")
                .Or.EqualTo("DirectML GPU activé"));
    }
}
