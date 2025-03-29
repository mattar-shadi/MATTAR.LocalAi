public class Document
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public ReadOnlyMemory<float>? ContentEmbedding { get; set; }
    public string[] Tags { get; set; } = [];
}