using Microsoft.ML.OnnxRuntime;
using System.Diagnostics;

namespace MATTAR.LocalAi;

public static class OnnxRuntimeSessionFactory
{
    public static bool IsDirectMlAvailable()
    {
        try
        {
            using var probe = new SessionOptions();
            probe.AppendExecutionProvider_DML(0);
            return true;
        }
        catch (OnnxRuntimeException ex)
        {
            Debug.Print($"DirectML not available: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.Print($"DirectML check failed: {ex.Message}");
            return false;
        }
    }

    public static void DetectExecutionProvider()
    {
        if (IsDirectMlAvailable())
            Console.WriteLine("DirectML GPU activé");
        else
            Console.WriteLine("CPU only");
    }

    public static SessionOptions CreateSessionOptions()
    {
        var sessionOptions = new SessionOptions();

        if (IsDirectMlAvailable())
            sessionOptions.AppendExecutionProvider_DML(0);
        else
            sessionOptions.AppendExecutionProvider_CPU();

        return sessionOptions;
    }

    public static InferenceSession CreateInferenceSession(string modelPath)
    {
        using var sessionOptions = CreateSessionOptions();
        return new InferenceSession(modelPath, sessionOptions);
    }
}
