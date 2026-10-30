using System.Collections.Concurrent;

namespace ResilientWebhook.Api.Context
{
    public sealed class InMemoryIdempotencyStore
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> _expiryByKey = new();
        
        public bool TryAdd(string key, TimeSpan ttl)
        {
            var now = DateTimeOffset.UtcNow;

           
            foreach (var kv in _expiryByKey)
                if (kv.Value < now) _expiryByKey.TryRemove(kv.Key, out _);

            // Atomik: sadece anahtar yoksa ekler
            return _expiryByKey.TryAdd(key, now + ttl);

        }
        public void Remove(string key) => _expiryByKey.TryRemove(key, out _);
    }
}
