namespace ResilientWebhook.Api.Models
{
    public record WebhookPayload(string EventId,
    string EventType,
    string PayloadJson,
    DateTime Timestamp);
   
}
