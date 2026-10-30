using ResilientWebhook.Api.Models;
using System.Security.Cryptography;
using System.Threading.Channels;
namespace ResilientWebhook.Api.Services
{
    public class WebhookQueue : IWebhookQueue
    {
        private readonly Channel<WebhookPayload> _channel = Channel.CreateUnbounded<WebhookPayload>();
        
        public IAsyncEnumerable<WebhookPayload> ReaderAllAsync(CancellationToken cancellationToken)
        {
           return _channel.Reader.ReadAllAsync(cancellationToken);  
        }

        public async ValueTask EnqueueAsync(WebhookPayload payload, CancellationToken cancellationToken)
        {
            if(payload == null) throw new ArgumentNullException(nameof(payload));

            await _channel.Writer.WriteAsync(payload,cancellationToken);
        }
    }
}
