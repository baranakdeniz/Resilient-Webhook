using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResilientWebhook.Api.Context;
using ResilientWebhook.Api.Models;
using ResilientWebhook.Api.Services;
using ResilientWebhook.Api.Validations;
using ResilientWebhook.Api.Workers;
using Serilog;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

//Helps to check the health of the application and its dependencies, such as the database connection.
builder.Services.AddHealthChecks()
.AddDbContextCheck<AppDbContext>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddSingleton<IWebhookQueue, WebhookQueue>();


builder.Services.AddHostedService<WebhookBackgroundWorker>();

builder.Services.AddSingleton<InMemoryIdempotencyStore>();
builder.Services.AddScoped<IIdempotencyStore, DbIdempotencyStore>();
builder.Services.AddSingleton<WebhookSignatureValidator>();
builder.Services.AddScoped<WebhookProcessor>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=webhooks.db"));

var app = builder.Build();
_ = app.Services.GetRequiredService<WebhookSignatureValidator>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//Serilog using
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        // /health endpoint'ine gelen istekleri loglama
        if (httpContext.Request.Path.StartsWithSegments("/health"))
            return Serilog.Events.LogEventLevel.Verbose; // Low level , won't be written to the life

        return Serilog.Events.LogEventLevel.Information;
    };
});

app.UseIpRateLimiting();
app.MapPost("/api/webhook", async (
    HttpContext context,
    WebhookProcessor processor,
    CancellationToken cancellationToken) => {
        return await processor.ProcessAsync(context, cancellationToken);
    });

app.MapHealthChecks("/health");




app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

