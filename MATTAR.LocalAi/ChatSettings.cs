using MATTAR.LocalAi.Abstractions;

namespace MATTAR.LocalAi;

public class ChatSettings : IChatSettings
{
    public string SystemPrompt { get; set; } = string.Empty;
}
