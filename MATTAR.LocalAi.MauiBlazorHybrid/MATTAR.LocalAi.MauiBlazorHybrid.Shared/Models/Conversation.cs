namespace MATTAR.LocalAi.MauiBlazorHybrid.Shared.Models
{
    internal class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<Message> Messages { get; set; } = [];
    }
}
