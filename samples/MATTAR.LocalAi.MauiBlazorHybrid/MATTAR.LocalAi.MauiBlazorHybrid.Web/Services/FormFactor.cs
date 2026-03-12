using MATTAR.LocalAi.MauiBlazorHybrid.Shared.Services;

namespace MATTAR.LocalAi.MauiBlazorHybrid.Web.Services
{
    public class FormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return "Web";
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
