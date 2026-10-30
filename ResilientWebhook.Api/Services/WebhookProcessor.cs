using ResilientWebhook.Api.Context;
using ResilientWebhook.Api.Models;
using ResilientWebhook.Api.Validations;
using System.Text.Json;

namespace ResilientWebhook.Api.Services
{
    public class WebhookProcessor
    {
        private readonly WebhookSignatureValidator _validator;
        private readonly InMemoryIdempotencyStore _idempotencyStore;
        private readonly IWebhookQueue _queue;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public WebhookProcessor(
            WebhookSignatureValidator validator,
            InMemoryIdempotencyStore idempotencyStore,
            IWebhookQueue queue)
        {
            _validator = validator;
            _idempotencyStore = idempotencyStore;
            _queue = queue;
        }

        public async Task<IResult> ProcessAsync(HttpContext context, CancellationToken ct)
        {
            // 1. Ham body
            using var ms = new MemoryStream();
            await context.Request.Body.CopyToAsync(ms, ct);
            byte[] rawBytes = ms.ToArray();

            // 2. Header kontrolü
            if (!context.Request.Headers.TryGetValue("webhook-timestamp", out var ts) ||
                !context.Request.Headers.TryGetValue("webhook-signature", out var sig))
                return Results.Unauthorized();

            // 3. Timestamp penceresi
            if (!long.TryParse(ts, out var unix)) return Results.Unauthorized();
            try
            {
                var requestTime = DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
                if (Math.Abs((DateTime.UtcNow - requestTime).TotalMinutes) > 5)
                    return Results.Unauthorized();
            }
            catch { return Results.Unauthorized(); }

            // 4. İmza doğrulama
            if (!_validator.TryValidate(ts!, rawBytes, sig!))
                return Results.Unauthorized();

            // 5. Idempotency (deserialize'den ÖNCE)
            var webhookId = context.Request.Headers["webhook-id"].ToString();
            if (string.IsNullOrEmpty(webhookId)) return Results.BadRequest();

            if (!_idempotencyStore.TryAdd(webhookId, TimeSpan.FromMinutes(5)))
                return Results.Ok(); // zaten işlendi

            // 6. Deserialize — başarısızsa anahtarı geri al
            WebhookPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<WebhookPayload>(rawBytes.AsSpan(), JsonOptions);
            }
            catch
            {
                _idempotencyStore.Remove(webhookId);
                return Results.BadRequest();
            }
            if (payload is null)
            {
                _idempotencyStore.Remove(webhookId);
                return Results.BadRequest();
            }

            // 7. Kuyruğa al — fırlatırsa anahtarı geri al
            try
            {
                await _queue.EnqueueAsync(payload, ct);
            }
            catch
            {
                _idempotencyStore.Remove(webhookId);
                throw;
            }

            return Results.Ok();
        }
    }
}
