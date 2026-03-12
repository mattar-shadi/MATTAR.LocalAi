using MATTAR.LocalAi.MauiBlazorHybrid.Services;
using MATTAR.LocalAi.MauiBlazorHybrid.Shared.Services;
using MATTAR.LocalAi.Extensions;
using Microsoft.Extensions.Logging;
using MATTAR.LocalAi.Abstractions;

namespace MATTAR.LocalAi.MauiBlazorHybrid;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the MATTAR.LocalAi.MauiBlazorHybrid.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        builder.Services.AddScoped<IChatSettings, ChatSettings>(ChatSettings =>
        {
            return new ChatSettings
            {
                SystemPrompt = @"
You are a helpful assistant.
Answer the user's questions as best as you can.
You speak the same langage of the user.
You are a large language model trained by Microsoft.
You answer the user's questions briefly and concisely.
You answer only to the last question or to the last message."
            };
        });
        builder.Services.AddTransient<IChat, Chat>();
        builder.Services.AddChatSqliteMemory();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
