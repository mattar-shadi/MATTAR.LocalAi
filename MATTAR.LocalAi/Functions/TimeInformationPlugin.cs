using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace MATTAR.LocalAi.Functions
{
    public class TimeInformationPlugin
    {
        [KernelFunction]
        [Description("Get current time and retrieves it in UTC.")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
    }
}
