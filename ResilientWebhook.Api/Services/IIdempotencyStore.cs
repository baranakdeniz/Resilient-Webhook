namespace ResilientWebhook.Api.Services
{
    public interface IIdempotencyStore
    {
        public Task<bool> TryAddAsync(string key, TimeSpan ttl);//neden datetimeoffset olmasın?
        public Task RemoveAsync(string key);
    }
}
