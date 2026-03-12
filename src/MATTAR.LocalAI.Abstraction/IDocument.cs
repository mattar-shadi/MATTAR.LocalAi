using Microsoft.Extensions.AI;

public interface IDocument
{
    ulong Id { get; set; }
    string Name { get; set; }
    string FullPath { get; set; }
    string Content { get; set; }
    Embedding<float> Embedding { get; set; }
    string[] Tags { get; set; }
    string ToString()
    {
        return $" - {Id} : {Name} ({FullPath}) \n {Content}";
    }
}