namespace ResilientWebhook.Api.Services
{
    public interface IIdempotencyStore
    {
        public Task<bool> TryAddAsync(string key, TimeSpan ttl);
        public Task RemoveAsync(string key);
    }
}
