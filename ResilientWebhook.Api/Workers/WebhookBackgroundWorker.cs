
using ResilientWebhook.Api.Models;
using ResilientWebhook.Api.Services;

namespace ResilientWebhook.Api.Workers
{
    public class WebhookBackgroundWorker : BackgroundService
    {
        private readonly IWebhookQueue _webhookQueue;
        private ILogger<WebhookBackgroundWorker> _logger;


        public WebhookBackgroundWorker(IWebhookQueue webHookQueue, ILogger<WebhookBackgroundWorker> logger)
        {
            _webhookQueue = webHookQueue;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    await foreach (var payload in _webhookQueue.ReaderAllAsync(stoppingToken))
                    {
                        await ProcessWebhookAsync(payload, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing webhook payloads.");
                }
            }
        }
        private async Task ProcessWebhookAsync(WebhookPayload payload, CancellationToken cancellationToken)
    {
        
        // Simulate processing the webhook payload
        _logger.LogInformation($"Processing webhook: {payload.EventId}, Type: {payload.EventType}, Timestamp: {payload.Timestamp}");
        await Task.Delay(1000, cancellationToken); 
            _logger.LogInformation($"Finished processing webhook: {payload.EventId}");
        }
    }
    
}
