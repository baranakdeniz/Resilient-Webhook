namespace ResilientWebhook.Api.Models
{
    public class ProcessedWebhook
    {
        public int Id { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
