using MATTAR.LocalAi;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, World!");
Console.WriteLine("This is a simple console app that uses the MATTAR Local AI package.");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var modelPath = configuration["ModelPath"] 
    ?? throw new InvalidOperationException("ModelPath is not set in appsettings.json");
var vectorStoreModelPath = configuration["VectorStoreModelPath"]
    ?? throw new InvalidOperationException("ModelPath is not set in appsettings.json");
var vectorStoreVocabModelPath = configuration["VectorStoreVocabModelPath"] 
    ?? throw new InvalidOperationException("ModelPath is not set in appsettings.json");

var chatSettings = new ChatSettings
{
    ModelPath = modelPath,
    VectorStoreModelPath = vectorStoreModelPath,
    VectorStoreVocabModelPath = vectorStoreVocabModelPath
};

var chat = new Chat(chatSettings);

while (true)
{
    Console.Write("User: ");
    var userQ = Console.ReadLine();
    if (string.IsNullOrEmpty(userQ))
    {
        break;
    }
    Console.Write($"Assistant: ");
    await chat.Run(userQ);
    Console.WriteLine("");
}