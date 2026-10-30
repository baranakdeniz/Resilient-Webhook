using ResilientWebhook.Api.Models;

namespace ResilientWebhook.Api.Services
{
    public interface IWebhookQueue
    {
        ValueTask EnqueueAsync(WebhookPayload webhookPayload, CancellationToken cancellationToken);
        IAsyncEnumerable<WebhookPayload> ReaderAllAsync(CancellationToken cancellationToken);
    }
}
