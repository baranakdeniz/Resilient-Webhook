
using Microsoft.EntityFrameworkCore;
using ResilientWebhook.Api.Context;
using ResilientWebhook.Api.Models;

namespace ResilientWebhook.Api.Services
{
    public class DbIdempotencyStore : IIdempotencyStore
    {
        private readonly AppDbContext _dbContext;

        public DbIdempotencyStore(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> TryAddAsync(string key, TimeSpan ttl)

        {  var record = new IdempotencyRecord
                {
                    Key = key,
                    ExpiresAt = DateTimeOffset.UtcNow.Add(ttl)
                };
            _dbContext.IdempotencyRecords.Add(record);
            try
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                _dbContext.Entry(record).State = EntityState.Detached; 
                return false; 
            }
    
        }
          
              
                    
        public async Task RemoveAsync(string key)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
               "DELETE FROM IdempotencyRecords WHERE Key = {0}", key);

        }

        
    }
}
