using ResilientWebhook.Api.Models;
using System.Security.Cryptography;
using System.Threading.Channels;
namespace ResilientWebhook.Api.Services
{
    public class WebhookQueue : IWebhookQueue
    {
        private readonly Channel<WebhookPayload> _channel = Channel.CreateBounded<WebhookPayload>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

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
