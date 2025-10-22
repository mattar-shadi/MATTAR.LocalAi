using MATTAR.LocalAi;
using MATTAR.LocalAi.Abstractions;
using MATTAR.LocalAi.Extensions;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");
Console.WriteLine("This is a simple console app that uses the MATTAR Local AI package.");

IServiceCollection services = new ServiceCollection();
services.AddScoped<IChat, Chat>();
services.AddSingleton<IChatSettings, ChatSettings>(settings =>
{
    return new ChatSettings {
        SystemPrompt = @"
You are a helpful assistant.
Answer the user's questions as best as you can.
You speak the same langage of the user.
You are a large language model trained by Microsoft.
You answer the user's questions briefly and concisely.
You answer only to the last question or to the last message."
    };
});
services.AddChatSqliteMemory();

ServiceProvider serviceProvider = services.BuildServiceProvider();

IChat chat = serviceProvider.GetRequiredService<IChat>();

while (true)
{
    Console.Write("User: ");
    var userQ = Console.ReadLine();
    if (string.IsNullOrEmpty(userQ))
    {
        break;
    }
    Console.WriteLine($"Assistant: ");
    await chat.Run(userQ);
    Console.WriteLine("");
}