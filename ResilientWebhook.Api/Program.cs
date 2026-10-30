using Microsoft.AspNetCore.Mvc;
using ResilientWebhook.Api.Context;
using ResilientWebhook.Api.Models;
using ResilientWebhook.Api.Services;
using ResilientWebhook.Api.Validations;
using ResilientWebhook.Api.Workers;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
 builder.Services.AddSingleton<IWebhookQueue, WebhookQueue>();

builder.Services.AddHostedService<WebhookBackgroundWorker>();

builder.Services.AddSingleton<InMemoryIdempotencyStore>();

builder.Services.AddSingleton<WebhookSignatureValidator>();
builder.Services.AddSingleton<WebhookProcessor>();

var app = builder.Build();
_ = app.Services.GetRequiredService<WebhookSignatureValidator>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapPost("/api/webhook", async (
    HttpContext context,
    WebhookProcessor processor,
    CancellationToken cancellationToken) => {
        return await processor.ProcessAsync(context, cancellationToken);
    });




app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

