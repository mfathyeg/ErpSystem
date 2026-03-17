using System.Text.Json;
using ErpSystem.Application.Abstractions.Outbox;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ErpSystem.Infrastructure.Messaging;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10);

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await outboxService.GetUnprocessedAsync(100, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType is null)
                {
                    _logger.LogWarning(
                        "Could not resolve event type {EventType} for message {MessageId}",
                        message.EventType,
                        message.Id);
                    continue;
                }

                var @event = JsonSerializer.Deserialize(message.Payload, eventType);
                if (@event is null)
                {
                    _logger.LogWarning(
                        "Could not deserialize message {MessageId}",
                        message.Id);
                    continue;
                }

                await publishEndpoint.Publish(@event, eventType, cancellationToken);
                await outboxService.MarkAsProcessedAsync(message.Id, cancellationToken);

                _logger.LogInformation(
                    "Successfully processed outbox message {MessageId}",
                    message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId}",
                    message.Id);

                await outboxService.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }
}
