using Microsoft.EntityFrameworkCore;
using ResilientWebhook.Api.Models;

namespace ResilientWebhook.Api.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<FailedWebhook> FailedWebhooks { get; set; }
        public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }
        public DbSet<ProcessedWebhook> ProcessedWebhooks { get; set; }

    }
}
