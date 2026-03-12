namespace MATTAR.LocalAi.MauiBlazorHybrid.Shared.Models
{
    internal class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Autors { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}
