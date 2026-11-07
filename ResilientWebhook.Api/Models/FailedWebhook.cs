namespace ResilientWebhook.Api.Models
{
    public class FailedWebhook
    {
        public int Id { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string PayloadJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime FailedAt { get; set; } = DateTime.UtcNow;
    }
}
