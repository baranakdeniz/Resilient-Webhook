
using Microsoft.Extensions.DependencyInjection;
using ResilientWebhook.Api.Context;
using ResilientWebhook.Api.Infrastructure;
using ResilientWebhook.Api.Models;
using ResilientWebhook.Api.Services;
using System.Text.Json;

namespace ResilientWebhook.Api.Workers
{
    public class WebhookBackgroundWorker : BackgroundService
    {
        private readonly IWebhookQueue _webhookQueue;
      
        private ILogger<WebhookBackgroundWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;


        public WebhookBackgroundWorker(IWebhookQueue webHookQueue, ILogger<WebhookBackgroundWorker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _webhookQueue = webHookQueue;
            _logger = logger;
            _serviceScopeFactory= serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5, 
                CancellationToken = stoppingToken 
            };

            // Kaynağı belirle
            IAsyncEnumerable<WebhookPayload> source = _webhookQueue.ReaderAllAsync(stoppingToken);

            // Paralel işlemi başlat
            await Parallel.ForEachAsync(
                source: source,                        
                parallelOptions: parallelOptions,       
                body: async (payload, ct) =>            
                {
                   
                  

                    try
                    {
                        await RetryPolicy.ExecuteAsync(async () =>
                        {
                            await ProcessWebhookAsync(payload, ct);
                        }, stoppingToken);
                        using var successScope = _serviceScopeFactory.CreateScope();
                        var db = successScope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var successRecord = new ProcessedWebhook
                        {
                            EventId = payload.EventId,
                            EventType = payload.EventType,
                            ProcessedAt = DateTime.UtcNow
                        };
                        db.Add(successRecord);
                        await db.SaveChangesAsync(ct);
                    }
                    catch (Exception ex)
                    {
                       
                        _logger.LogError(ex, "Throwing to DLQ: {EventId}", payload.EventId);
                        using var scope = _serviceScopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var failedRecord = new FailedWebhook
                        {
                            EventId = payload.EventId,
                            PayloadJson = JsonSerializer.Serialize(payload),
                            ErrorMessage = ex.Message,
                        };
                        dbContext.Add(failedRecord);
                        await dbContext.SaveChangesAsync(ct);
                    }
                });
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
