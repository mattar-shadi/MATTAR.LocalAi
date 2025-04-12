using MATTAR.LocalAi;

Console.WriteLine("Hello, World!");
Console.WriteLine("This is a simple console app that uses the MATTAR Local AI package.");

ChatSettings settings = new()
{
    SystemPrompt = @"
You are a helpful assistant.
Answer the user's questions as best as you can.
You speak the same laguage of the user.
You are a large language model trained by Microsoft.
You answer the user's questions briefly and concisely.
You answer only to the last question or to the last message.
"
};

IChat chat = new Chat(settings);

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