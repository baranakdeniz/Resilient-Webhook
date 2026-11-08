using System.ComponentModel.DataAnnotations;

namespace ResilientWebhook.Api.Models
{
    public class IdempotencyRecord
    {
        [Key]
        public string Key { get; set; } = string.Empty;//set? gerekli mi ? idempotencyler geldiği gibi kalır değiştirilemez içeriği?
        public DateTime ExpiresAt { get; set; }

    }
}
