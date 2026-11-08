
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using ResilientWebhook.Api.Context;

namespace ResilientWebhook.Api.Workers
{
    public class IdempotencyCleanupWorker : BackgroundService
    {
        private readonly ILogger<IdempotencyCleanupWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        public IdempotencyCleanupWorker(ILogger<IdempotencyCleanupWorker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _scopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.UtcNow;

                    // Expired records finding
                    var expiredRecords = await dbContext.IdempotencyRecords
                        .Where(r => r.ExpiresAt < now)
                        .ToListAsync(stoppingToken);

                    // If there are records to delete, delete them
                    if (expiredRecords.Any())
                    {
                        dbContext.IdempotencyRecords.RemoveRange(expiredRecords);
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("{Count} expired Idempotency keys have been cleaned up.", expiredRecords.Count);
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "A database lock/connection error occurred during cleanup.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred during cleanup.");
                }

             
                //await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); 
            }
        }
        }

    }


